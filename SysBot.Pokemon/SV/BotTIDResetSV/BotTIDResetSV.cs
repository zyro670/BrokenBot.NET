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

public class TIDResetBotSV : PokeRoutineExecutor9SV, IEncounterBot
{
    private readonly PokeTradeHub<PK9> Hub;
    private readonly IDumper DumpSetting;
    private readonly int[] DesiredMinIVs;
    private readonly int[] DesiredMaxIVs;
    private readonly TIDResetBotSettings Settings;
    public ICountSettings Counts => Settings;
    public readonly IReadOnlyList<string> UnwantedMarks;
    private SAV9SV TrainerSav = new();

    public TIDResetBotSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
    {
        Hub = hub;
        Settings = Hub.Config.TIDReset;
        DumpSetting = Hub.Config.Folder;
        StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
        StopConditionSettings.ReadUnwantedMarks(Hub.Config.StopConditions, out UnwantedMarks);
    }

    public override async Task MainLoop(CancellationToken token)
    {
        await InitializeHardware(Hub.Config.TIDReset, token).ConfigureAwait(false);
        Log("Starting main TIDResetBotSV loop.");
        Config.IterateNextRoutine();

        try
        {
            await InnerLoop(token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log(e.Message);
        }

        Log($"Ending {nameof(TIDResetBotSV)} loop.");
        await HardStop().ConfigureAwait(false);
        return;
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
        while (!token.IsCancellationRequested)
        {
            var (TrainerSav, res) = await GetFakeTrainerSAVSV(token).ConfigureAwait(false);
            Log(res);
            string[] tidList = Hub.Config.TIDReset.DesiredTIDs.Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool desired = tidList.Contains($"{TrainerSav.TrainerTID7}");
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

    private async Task<(SAV9SV, string)> GetFakeTrainerSAVSV(CancellationToken token)
    {
        var sav = new SAV9SV();
        var info = sav.MyStatus;
        var read = await SwitchConnection.PointerPeek(info.Data.Length, Offsets.MyStatusPointer, token).ConfigureAwait(false);
        read.CopyTo(info.Data, 0);
        return (sav, $"TID: {sav.TrainerTID7}");
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
