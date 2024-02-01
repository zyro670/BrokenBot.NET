using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon;

public class TIDResetBotSWSH : PokeRoutineExecutor8SWSH, IEncounterBot
{
    private readonly PokeTradeHub<PK8> Hub;
    private readonly IDumper DumpSetting;
    private readonly int[] DesiredMinIVs;
    private readonly int[] DesiredMaxIVs;
    private readonly TIDResetBotSettings Settings;
    public ICountSettings Counts => Settings;
    public readonly IReadOnlyList<string> UnwantedMarks;
    private SAV8SWSH TrainerSav = new();

    public TIDResetBotSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg)
    {
        Hub = hub;
        Settings = Hub.Config.TIDReset;
        DumpSetting = Hub.Config.Folder;
        StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
        StopConditionSettings.ReadUnwantedMarks(Hub.Config.StopConditions, out UnwantedMarks);
    }

    public override async Task MainLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await InitializeHardware(Hub.Config.TIDReset, token).ConfigureAwait(false);
            Log("Starting main TIDResetBotSWSH loop.");
            Config.IterateNextRoutine();

            try
            {
                await InnerLoop(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            Log($"Ending {nameof(TIDResetBotSWSH)} loop.");
            await HardStop().ConfigureAwait(false);
            return;
        }
    }

    private bool IsWaiting;
    public void Acknowledge() => IsWaiting = false;

    public override async Task HardStop()
    {
        // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
        await SetStick(LEFT, 0, 0, 0, CancellationToken.None).ConfigureAwait(false); // reset
        await CleanExit(CancellationToken.None).ConfigureAwait(false);
    }

    private async Task InnerLoop(CancellationToken token)
    {
        uint prevOT = 0;
        while (!token.IsCancellationRequested)
        {
            var sav = await GetFakeTrainerSAV(token).ConfigureAwait(false);
            prevOT = sav.TrainerTID7;
            if (sav.TrainerTID7 >= 0)
            {
                await SelectLang(token).ConfigureAwait(false);
                await Task.Delay(1_000, token).ConfigureAwait(false);
                await SelectAvatar(token).ConfigureAwait(false);
                await Task.Delay(2_000, token).ConfigureAwait(false);
                await EnterTrainerOT(token).ConfigureAwait(false);
                await Task.Delay(1_000, token).ConfigureAwait(false);
                var tries = 0;
                while (sav.TrainerTID7 == prevOT)
                {
                    await Click(A, 0_500, token).ConfigureAwait(false);
                    tries++;

                    if (tries == 100)
                    {
                        sav = await GetFakeTrainerSAV(token).ConfigureAwait(false);
                        tries = 0;
                    }
                }

                Log($"{sav.TrainerTID7}");
                string[] tidList = Hub.Config.TIDReset.DesiredTIDs.Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool desired = tidList.Contains($"{sav.TrainerTID7}");
                if (desired)
                {
                    Log($"TID match has been found!");
                    EchoUtil.EchoEmbed($"{Hub.Config.StopConditions.MatchFoundEchoMention}", "A TID match has been found!", "", "", true);
                    return;
                }
                else
                    Log("Not a match, restarting the game...");
                await ResetGame(token).ConfigureAwait(false);
            }
        }
    }

    private async Task SelectLang(CancellationToken token)
    {
        switch (Settings.SWSH_LangSelect)
        {
            case TIDResetBotSettings.SWSHLanguage.English: await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Spanish: await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.French:
                for (int i = 0; i < 2; i++)
                    await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Dutch:
                for (int i = 0; i < 2; i++)
                    await Click(DUP, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Italian: await Click(DUP, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Japanese1: await Click(DRIGHT, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Japanese2: await Click(DRIGHT, 0_050, token).ConfigureAwait(false); await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Korean: await Click(DRIGHT, 0_050, token).ConfigureAwait(false); await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Chinese1: await Click(DRIGHT, 0_050, token).ConfigureAwait(false); await Click(DUP, 0_050, token).ConfigureAwait(false); await Click(DUP, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHLanguage.Chinese2: await Click(DRIGHT, 0_050, token).ConfigureAwait(false); await Click(DUP, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
        }
    }

    private async Task SelectAvatar(CancellationToken token)
    {
        switch (Settings.SWSH_AvatarSelect)
        {
            case TIDResetBotSettings.SWSHAvatar.BoyDefault: await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHAvatar.BoyBlonde: await Click(DRIGHT, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHAvatar.BoyDark: await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHAvatar.BoyTan: await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(DRIGHT, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHAvatar.GirlDefault: await Click(DLEFT, 0_050, token).ConfigureAwait(false); await Click(DLEFT, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHAvatar.GirlBlonde: await Click(DLEFT, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHAvatar.GirlDark: await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(DLEFT, 0_050, token).ConfigureAwait(false); await Click(DLEFT, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
            case TIDResetBotSettings.SWSHAvatar.GirlTan: await Click(DLEFT, 0_050, token).ConfigureAwait(false); await Click(DDOWN, 0_050, token).ConfigureAwait(false); await Click(A, 0_050, token).ConfigureAwait(false); break;
        }
    }

    private async Task EnterTrainerOT(CancellationToken token)
    {
        var strokes = Settings.SWSH_OT.ToUpper().ToArray();
        var number = $"NumPad";
        List<HidKeyboardKey> keystopress = [];
        foreach (var str in strokes)
        {
            foreach (HidKeyboardKey keypress in (HidKeyboardKey[])Enum.GetValues(typeof(HidKeyboardKey)))
            {
                if (str.ToString().Equals(keypress.ToString()) || (number + str.ToString()).Equals(keypress.ToString()))
                    keystopress.Add(keypress);
            }
        }
        await SwitchConnection.SendAsync(SwitchCommand.TypeMultipleKeys(keystopress, true), token).ConfigureAwait(false);
        await Click(PLUS, 0_500, token).ConfigureAwait(false);
        await Click(PLUS, 0_500, token).ConfigureAwait(false);
    }

    private async Task ResetGame(CancellationToken token)
    {
        await Click(B, 0_050, token).ConfigureAwait(false);
        await CloseGame(Hub.Config, token).ConfigureAwait(false);
        Log("Opening game!");
        await StartGameScreen(token).ConfigureAwait(false);
    }

    public async Task StartGameScreen(CancellationToken token)
    {
        // Open game.
        await Click(A, 1_000, token).ConfigureAwait(false);

        await Click(A, 1_000, token).ConfigureAwait(false);
        // If they have DLC on the system and can't use it, requires an UP + A to start the game.
        // Should be harmless otherwise since they'll be in loading screen.
        await Click(DUP, 0_600, token).ConfigureAwait(false);
        await Click(A, 0_600, token).ConfigureAwait(false);

        // Switch Logo and game load screen
        await Task.Delay(Settings.TimeToLoadScreen, token).ConfigureAwait(false);
    }

}
