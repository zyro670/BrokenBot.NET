using PKHeX.Core;
using SysBot.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon;

public class EggBotSV : PokeRoutineExecutor9SV, IEncounterBot
{
    private readonly PokeTradeHub<PK9> Hub;
    private readonly IDumper DumpSetting;
    private readonly int[] DesiredMinIVs;
    private readonly int[] DesiredMaxIVs;
    private readonly EggSettingsSV Settings;
    public ICountSettings Counts => Settings;

    public EggBotSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
    {
        Hub = hub;
        Settings = Hub.Config.EggSV;
        DumpSetting = Hub.Config.Folder;
        StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
    }

    private int eggcount = 0;
    private int sandwichcount = 0;
    private PK9 prevPK = new();
    private static readonly PK9 Blank = new();
    private readonly byte[] BlankVal = [0x01];
    private ulong OverworldOffset;

    public override async Task MainLoop(CancellationToken token)
    {
        await InitializeHardware(Hub.Config.EggSV, token).ConfigureAwait(false);
        Log("Identifying trainer data of the host console.");
        await IdentifyTrainer(token).ConfigureAwait(false);
        await InitializeSessionOffsets(token).ConfigureAwait(false);

        Log("Starting main EggBot loop.");
        Config.IterateNextRoutine();

        try
        {
            await InnerLoop(token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log(e.Message);
        }

        Log($"Ending {nameof(EggBotSV)} loop.");
        await HardStop().ConfigureAwait(false);
        return;
    }

    public override async Task HardStop()
    {
        // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
        await SetStick(LEFT, 0, 0, 0, CancellationToken.None).ConfigureAwait(false); // reset
        await CleanExit(CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Return true if we need to stop looping.
    /// </summary>
    private async Task InnerLoop(CancellationToken token)
    {
        await SwitchConnection.WriteBytesMainAsync(BlankVal, Offsets.LoadedIntoDesiredState, token).ConfigureAwait(false);
        if (Settings.EatFirst == true)
            await MakeSandwich(token).ConfigureAwait(false);

        await WaitForEggs(token).ConfigureAwait(false);
        return;
    }

    private bool IsWaiting;
    public void Acknowledge() => IsWaiting = false;

    private async Task ReopenPicnic(CancellationToken token)
    {
        await Task.Delay(0_500, token).ConfigureAwait(false);
        await Click(Y, 1_500, token).ConfigureAwait(false);
        var overworldWaitCycles = 0;
        var hasReset = false;
        while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false)) // Wait until we return to the overworld
        {
            await Click(A, 1_000, token).ConfigureAwait(false);
            overworldWaitCycles++;

            if (overworldWaitCycles == 10)
            {
                for (int i = 0; i < 5; i++)
                    await Click(B, 0_500, token).ConfigureAwait(false); // Click a few times to attempt to escape any menu

                await Click(Y, 1_500, token).ConfigureAwait(false); // Attempt to leave the picnic again, in case you were stuck interacting with a pokemon
                await Click(A, 1_000, token).ConfigureAwait(false); // Overworld seems to trigger true when you leave the Pokemon washing mode, so we have to try to exit picnic immediately

                for (int i = 0; i < 4; i++)
                    await Click(B, 0_500, token).ConfigureAwait(false); // Click a few times to attempt to escape any menu
            }

            else if (overworldWaitCycles >= 53) // If still not in the overworld after ~1 minute of trying, hard reset the game
            {
                overworldWaitCycles = 0;
                Log("Failed to return to the overworld after 1 minute.  Forcing a game reset.");
                await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                await InitializeSessionOffsets(token).ConfigureAwait(false);  // Re-acquire overworld offset to escape the while loop
                hasReset = true;
            }
        }
        for (int i = 0; i < 10; i++)
            await Click(A, 0_500, token).ConfigureAwait(false); // Click A alot incase pokemon are not level 100
        await Click(X, 1_700, token).ConfigureAwait(false);
        if (hasReset) // If we are starting fresh, we need to reposition over the picnic button
        {
            await Click(DRIGHT, 0_250, token).ConfigureAwait(false);
            await Click(DDOWN, 0_250, token).ConfigureAwait(false);
            await Click(DDOWN, 0_250, token).ConfigureAwait(false);
        }
        await Click(A, 7_000, token).ConfigureAwait(false); // First picnic might take longer.
    }

    private async Task WaitForEggs(CancellationToken token)
    {
        PK9 pkprev = new();
        var reset = 0;
        while (!token.IsCancellationRequested)
        {
            var wait = TimeSpan.FromMinutes(30);
            var endTime = DateTime.Now + wait;
            var ctr = 0;
            var waiting = 0;
            while (DateTime.Now < endTime)
            {
                var pk = await ReadPokemonSV(Offsets.EggData, 344, token).ConfigureAwait(false);
                while (pk == prevPK || pk == null || pkprev!.EncryptionConstant == pk.EncryptionConstant || (Species)pk.Species == Species.None)
                {
                    waiting++;

                    await Task.Delay(1_500, token).ConfigureAwait(false);
                    pk = await ReadPokemonSV(Offsets.EggData, 344, token).ConfigureAwait(false);
                    if (waiting == 200)
                    {
                        Log("3 minutes have passed without an egg.  Attempting full recovery.");
                        await ReopenPicnic(token).ConfigureAwait(false);
                        await MakeSandwich(token).ConfigureAwait(false);
                        await ReopenPicnic(token).ConfigureAwait(false);
                        wait = TimeSpan.FromMinutes(30);
                        endTime = DateTime.Now + wait;
                        waiting = 0;
                        ctr = 0;
                        if (reset == Settings.ResetGameAfterThisManySandwiches && Settings.ResetGameAfterThisManySandwiches != 0)
                        {
                            reset = 0;
                            await RecoveryReset(token).ConfigureAwait(false);
                        }
                        reset++;
                    }
                }

                while (pk != null && (Species)pk.Species != Species.None && pkprev.EncryptionConstant != pk.EncryptionConstant)
                {
                    waiting = 0;
                    eggcount++;
                    var print = $"Encounter: {eggcount}{Environment.NewLine}{Hub.Config.StopConditions.GetSpecialPrintName(pk)}{Environment.NewLine}Ball: {(Ball)pk.Ball}";
                    Log(print);
                    Settings.AddCompletedEggs();
                    TradeExtensions<PK9>.EncounterLogs(pk, "EncounterLogPretty_EggSV.txt");
                    TradeExtensions<PK9>.EncounterScaleLogs(pk, "EncounterLogScalePretty.txt");
                    ctr++;

                    bool match = await CheckEncounter(print, pk).ConfigureAwait(false);
                    if (!match && Settings.ContinueAfterMatch == ContinueAfterMatch.StopExit)
                    {
                        if (Settings.ForceDump)
                            ForcifyEgg(pk);

                        prevPK = pk;
                        Log("Make sure to pick up your egg in the basket!");
                        await Click(HOME, 0_500, token).ConfigureAwait(false);
                        return;
                    }
                    pkprev = pk!;
                }

                Log($"Basket Count: {ctr}\nWaiting..");
                if (ctr == 10)
                {
                    Log("No match in basket. Resetting picnic..");
                    await ReopenPicnic(token).ConfigureAwait(false);
                    ctr = 0;
                    waiting = 0;
                    Log("Resuming routine..");
                }
            }
            Log("30 minutes have passed, remaking sandwich.");
            if (reset == Settings.ResetGameAfterThisManySandwiches && Settings.ResetGameAfterThisManySandwiches != 0)
            {
                reset = 0;
                await RecoveryReset(token).ConfigureAwait(false);
            }
            reset++;
            await MakeSandwich(token).ConfigureAwait(false);
        }
    }

    private async Task RecoveryReset(CancellationToken token)
    {
        await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
        await InitializeSessionOffsets(token).ConfigureAwait(false);
        await Task.Delay(1_000, token).ConfigureAwait(false);
        await Click(X, 2_000, token).ConfigureAwait(false);
        await Click(DRIGHT, 0_250, token).ConfigureAwait(false);
        await Click(DDOWN, 0_250, token).ConfigureAwait(false);
        await Click(DDOWN, 0_250, token).ConfigureAwait(false);
        await Click(A, 7_000, token).ConfigureAwait(false);
    }

    private async Task<bool> CheckEncounter(string print, PK9 pk)
    {
        var token = CancellationToken.None;
        string? url;

        if (!StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
        {
            if (Hub.Config.StopConditions.ShinyTarget is TargetShinyType.AnyShiny or TargetShinyType.StarOnly or TargetShinyType.SquareOnly && pk.IsShiny)
            {
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                EchoUtil.EchoEmbed("", print, url, "", false);
            }
            return true; //No match, return true to keep scanning
        }

        if (Settings.MinMaxScaleOnly && pk.Scale > 0 && pk.Scale < 255)
        {
            {
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                EchoUtil.EchoEmbed("", print, url, "", false);
            }
            return true;
        }

        // no need to take a video clip of us receiving an egg.
        var mode = Settings.ContinueAfterMatch;
        var msg = $"Result found!\n{print}\n" + mode switch
        {
            ContinueAfterMatch.PauseWaitAcknowledge => "Waiting for instructions to continue.",
            ContinueAfterMatch.Continue => "Continuing..",
            ContinueAfterMatch.StopExit => "Stopping routine execution; restart the bot to search again.",
            _ => throw new ArgumentOutOfRangeException(),
        };

        var ping = string.Empty;
        if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
            ping = Hub.Config.StopConditions.MatchFoundEchoMention;

        if (!string.IsNullOrWhiteSpace(ping))
            msg = $"{ping} {msg}";

        Log(msg);

        if (Settings.OneInOneHundredOnly)
        {
            if ((Species)pk.Species is Species.Dunsparce or Species.Tandemaus && pk.EncryptionConstant % 100 != 0)
            {
                {
                    url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                    EchoUtil.EchoEmbed("", print, url, "", false);
                }
                return true; // 1/100 condition unsatisfied, continue scanning
            }
        }

        if (mode == ContinueAfterMatch.StopExit) // Stop & Exit: Condition satisfied.  Stop scanning and disconnect the bot
        {
            url = TradeExtensions<PK9>.PokeImg(pk, false, false);
            EchoUtil.EchoEmbed(ping, print, url, "", true);
            return false;
        }

        url = TradeExtensions<PK9>.PokeImg(pk, false, false);
        EchoUtil.EchoEmbed(ping, print, url, "", true);

        if (mode == ContinueAfterMatch.PauseWaitAcknowledge)
        {
            Log("Claim your egg before closing the picnic! Alternatively you can manually run to collect all present eggs, go back to the HOME screen, type $toss, and let it continue scanning from there.");
            await Click(HOME, 0_700, token).ConfigureAwait(false);

            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);

            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);
            await Click(HOME, 1_000, token).ConfigureAwait(false);
        }

        return false;
    }

    private async Task<PK9> ReadBoxPokemonSV(ulong offset, int size, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAbsoluteAsync(offset, size, token).ConfigureAwait(false);
        var pk = new PK9(data);
        return pk;
    }

    private async Task<PK9> ReadPokemonSV(uint offset, int size, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesMainAsync(offset, size, token).ConfigureAwait(false);
        var pk = new PK9(data);
        return pk;
    }

    private async Task<int> PicnicState(CancellationToken token)
    {
        var Data = await SwitchConnection.ReadBytesMainAsync(Offsets.LoadedIntoDesiredState, 1, token).ConfigureAwait(false);
        return Data[0]; // 1 when in picnic, 2 in sandwich menu, 3 when eating, 2 when done eating
    }

    private async Task<bool> IsInPicnic(CancellationToken token)
    {
        var Data = await SwitchConnection.ReadBytesMainAsync(Offsets.LoadedIntoDesiredState, 1, token).ConfigureAwait(false);
        return Data[0] == 0x01; // 1 when in picnic, 2 in sandwich menu, 3 when eating, 2 when done eating
    }

    private async Task MakeSandwich(CancellationToken token)
    {
        await Click(MINUS, 0_500, token).ConfigureAwait(false);
        await SetStick(LEFT, 0, 32323, 0_700, token).ConfigureAwait(false); // Face up to table
        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
        await Click(A, 1_500, token).ConfigureAwait(false);
        await Click(A, 8_000, token).ConfigureAwait(false);
        await Click(X, 2_500, token).ConfigureAwait(false);

        for (int i = 0; i < 0; i++) // Select first ingredient
            await Click(Settings.Item1DUP ? DUP : DDOWN, 0_500, token).ConfigureAwait(false);

        await Click(A, 0_800, token).ConfigureAwait(false);
        await Click(PLUS, 0_800, token).ConfigureAwait(false);

        for (int i = 0; i < 4; i++) // Select second ingredient
            await Click(Settings.Item2DUP ? DUP : DDOWN, 0_500, token).ConfigureAwait(false);

        await Click(A, 0_800, token).ConfigureAwait(false);

        for (int i = 0; i < 1; i++) // Select third ingredient            
            await Click(Settings.Item3DUP ? DUP : DDOWN, 0_500, token).ConfigureAwait(false);

        await Click(A, 0_800, token).ConfigureAwait(false);
        await Click(PLUS, 0_800, token).ConfigureAwait(false);
        await Click(A, 8_000, token).ConfigureAwait(false);
        await SetStick(LEFT, 0, 30000, Settings.HoldUpToIngredients, token).ConfigureAwait(false); // Scroll up to the lettuce
        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

        for (int i = 0; i < 12; i++) // If everything is properly positioned
            await Click(A, 0_800, token).ConfigureAwait(false);

        // Sandwich failsafe
        for (int i = 0; i < 5; i++) //Attempt this several times to ensure it goes through
            await SetStick(LEFT, 0, 30000, 1_000, token).ConfigureAwait(false); // Scroll to the absolute top
        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

        while (await PicnicState(token).ConfigureAwait(false) == 2) // Until we start eating the sandwich
        {
            await SetStick(LEFT, 0, -5000, 0_300, token).ConfigureAwait(false); // Scroll down slightly and press A a few times; repeat until un-stuck
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

            for (int i = 0; i < 6; i++)
                await Click(A, 0_800, token).ConfigureAwait(false);
        }

        while (await PicnicState(token).ConfigureAwait(false) == 3)  // eating the sandwich
            await Task.Delay(1_000, token).ConfigureAwait(false);

        sandwichcount++;
        Log($"Sandwiches Made: {sandwichcount}");

        while (!await IsInPicnic(token).ConfigureAwait(false)) // Acknowledge the sandwich and return to the picnic            
            await Click(A, 5_000, token).ConfigureAwait(false); // Wait a long time to give the flag a chance to update and avoid sandwich re-entry            

        await Task.Delay(2_500, token).ConfigureAwait(false);
        await SetStick(LEFT, 0, -10000, 0_500, token).ConfigureAwait(false); // Face down to basket
        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
        await Task.Delay(1_000, token).ConfigureAwait(false);
        await SetStick(LEFT, 0, 5000, 0_200, token).ConfigureAwait(false); // Face up to basket
        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
    }

    private async Task InitializeSessionOffsets(CancellationToken token)
    {
        OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
        Log("Caching offsets complete!");
    }

    private void ForcifyEgg(PK9 pk)
    {
        pk.IsEgg = true;
        pk.Nickname = "Egg";
        pk.Met_Location = 0;
        pk.Egg_Location = 30023;
        pk.MetDate = DateOnly.Parse("2023/02/17");
        pk.EggMetDate = pk.MetDate;
        pk.OT_Name = "ZYZYZYZY";
        pk.HeldItem = 0;
        pk.CurrentLevel = 1;
        pk.EXP = 0;
        pk.Met_Level = 1;
        pk.CurrentHandler = 0;
        pk.OT_Friendship = 1;
        pk.HT_Name = "";
        pk.HT_Friendship = 0;
        pk.ClearMemories();
        pk.StatNature = pk.Nature;
        pk.SetEVs(new int[] { 0, 0, 0, 0, 0, 0 });
        pk.SetMarking(0, 0);
        pk.SetMarking(1, 0);
        pk.SetMarking(2, 0);
        pk.SetMarking(3, 0);
        pk.SetMarking(4, 0);
        pk.SetMarking(5, 0);
        pk.ClearInvalidMoves();

        DumpPokemon(DumpSetting.DumpFolder, "forced-eggs", pk);
    }
}
