using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.EggSettingsSV;

namespace SysBot.Pokemon
{
    public class EggBotSV : PokeRoutineExecutor9SV, IEncounterBot
    {
        private readonly PokeTradeHub<PK9> Hub;
        private readonly IDumper DumpSetting;
        private readonly EggSettingsSV Settings;
        public ICountSettings Counts => Settings;

        public static CancellationTokenSource EmbedSource = new();
        public static bool EmbedsInitialized;
        public static (PK9?, bool) EmbedMon;

        public EggBotSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
        {
            Hub = hub;
            Settings = Hub.Config.EggSV;
            DumpSetting = Hub.Config.Folder;
        }

        private int eggcount = 0;
        private int sandwichcount = 0;
        private const int InjectBox = 0;
        private const int InjectSlot = 0;
        private readonly uint EggData = 0x044AAE00;
        private static readonly PK9 Blank = new();
        private readonly byte[] BlankVal = { 0x01 };
        private const string TextBox = "[[[[[main+44CEE30]+10]+3D8]+4C8]+30]";
        private const string B1S1 = "[[[main+44A98C8]+130]+9B0]";
        private byte[]? TextVal = Array.Empty<byte>();
        private ulong OverworldOffset;

        public override async Task MainLoop(CancellationToken token)
        {
            await InitializeHardware(Hub.Config.EggSV, token).ConfigureAwait(false);

            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);

            if (Settings.EggBotMode == EggMode.CollectAndDump)
                await SetupBoxState(token).ConfigureAwait(false);

            Log("Starting main EggBot loop.");
            Config.IterateNextRoutine();
            while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.EggFetch)
            {
                try
                {
                    if (!await InnerLoop(token).ConfigureAwait(false))
                        break;
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }
            }

            Log($"Ending {nameof(EggBot)} loop.");
            await HardStop().ConfigureAwait(false);
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
        private async Task<bool> InnerLoop(CancellationToken token)
        {

            await SetCurrentBox(0, token).ConfigureAwait(false);
            await SwitchConnection.WriteBytesMainAsync(BlankVal, Offsets.LoadedIntoRaid, token).ConfigureAwait(false);

            var mode = Settings.EggBotMode;
            if (mode == EggMode.WaitAndClose && Settings.ContinueAfterMatch == ContinueAfterMatch.Continue)
            {
                Log("The Continue setting is not recommended for this mode, please change it to PauseWaitAcknowledge. Close and reopen exe to save changes.");
                return false;
            }

            if (mode == EggMode.CollectAndDump)
            {
                for (int i = 0; i < 2; i++)
                    await Click(A, 1_000, token).ConfigureAwait(false);

                await GrabValues(token).ConfigureAwait(false);
            }

            if (Settings.EatFirst == true)
                await MakeSandwich(token).ConfigureAwait(false);

            await WaitForEggs(mode, token).ConfigureAwait(false);
            return false;
        }

        private async Task SetupBoxState(CancellationToken token)
        {
            var existing = await ReadBoxPokemon(InjectBox, InjectSlot, token).ConfigureAwait(false);
            if (existing.Species != 0 && existing.ChecksumValid)
            {
                Log("Destination slot is occupied! Dumping the Pokémon found there...");
                DumpPokemon(DumpSetting.DumpFolder, "saved", existing);
            }

            Log("Clearing destination slot to start the bot.");
            await SetBoxPokemonEgg(Blank, InjectBox, InjectSlot, token).ConfigureAwait(false);
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
                    OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);  // Re-acquire overworld offset to escape the while loop
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

        private async Task WaitForEggs(EggMode mode, CancellationToken token)
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
                    var pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                    while (pk == null || pkprev.EncryptionConstant == pk.EncryptionConstant || (Species)pk.Species == Species.None)
                    {
                        waiting++;
                        await Task.Delay(1_500, token).ConfigureAwait(false);
                        pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                        if (mode == EggMode.WaitAndClose)
                        {
                            if (waiting == 120)
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
                    }

                    while (pk != null && (Species)pk.Species != Species.None && pkprev.EncryptionConstant != pk.EncryptionConstant)
                    {                        
                        waiting = 0;
                        eggcount++;
                        var print = Hub.Config.StopConditions.GetSpecialPrintName(pk);
                        Log($"Encounter: {eggcount}{Environment.NewLine}{print}{Environment.NewLine}Scale: {PokeSizeDetailedUtil.GetSizeRating(pk.Scale)}");
                        Settings.AddCompletedEggs();
                        TradeExtensions<PK9>.EncounterLogs(pk, "EncounterLogPretty_EggSV.txt");
                        TradeExtensions<PK9>.EncounterScaleLogs(pk, "EncounterLogScalePretty.txt");
                        ctr++;

                        bool match = await CheckEncounter(print, pk).ConfigureAwait(false);
                        if (mode == EggMode.CollectAndDump)
                        {

                            await Task.Delay(0_500, token).ConfigureAwait(false);
                            await Click(A, 2_500, token).ConfigureAwait(false);
                            await Click(A, 1_200, token).ConfigureAwait(false);

                            await RetrieveEgg(match, token).ConfigureAwait(false);
                        }
                        if (!match && Settings.ContinueAfterMatch == ContinueAfterMatch.StopExit)
                        {
                            if (mode == EggMode.WaitAndClose)
                                Log("Make sure to pick up your egg in the basket!");
                            else if (mode == EggMode.CollectAndDump)
                                Log("Egg should be claimed!");
                            await Click(HOME, 0_500, token).ConfigureAwait(false);
                            return;
                        }
                        pkprev = pk;
                    }
                    if (mode == EggMode.WaitAndClose)
                    {
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
                    else if (mode == EggMode.CollectAndDump)
                    {
                        for (int i = 0; i < 2; i++)
                            await Click(PLUS, 0_500, token).ConfigureAwait(false);
                        await Click(B, 1_000, token).ConfigureAwait(false);
                    }
                }
                Log("30 minutes have passed, remaking sandwich.");
                if (reset == Settings.ResetGameAfterThisManySandwiches && Settings.ResetGameAfterThisManySandwiches != 0)
                {
                    reset = 0;
                    if (mode == EggMode.WaitAndClose)
                        await RecoveryReset(token).ConfigureAwait(false);
                }
                reset++;
                await MakeSandwich(token).ConfigureAwait(false);
            }
        }

        private async Task RecoveryReset(CancellationToken token)
        {        
            await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
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

            if (!StopConditionSettings.EncounterFound(pk, Hub.Config.StopConditions, null))
            {
                if (Hub.Config.StopConditions.ShinyTarget is TargetShinyType.AnyShiny or TargetShinyType.StarOnly or TargetShinyType.SquareOnly && pk.IsShiny)
                    EmbedMon = (pk, false);

                return true; //No match, return true to keep scanning
            }

            if (Settings.MinMaxScaleOnly && pk.Scale > 0 && pk.Scale < 255)
            {
                EmbedMon = (pk, false);
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

            if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                msg = $"{Hub.Config.StopConditions.MatchFoundEchoMention} {msg}";

            Log(print);

            if (Settings.OneInOneHundredOnly)
            {
                if ((Species)pk.Species is Species.Dunsparce or Species.Tandemaus && pk.EncryptionConstant % 100 != 0)
                {
                    EmbedMon = (pk, false);
                    return true; // 1/100 condition unsatisfied, continue scanning
                }
            }

            if (mode == ContinueAfterMatch.StopExit) // Stop & Exit: Condition satisfied.  Stop scanning and disconnect the bot
            {
                EmbedMon = (pk, true);
                return false;
            }

            EmbedMon = (pk, true);
            EchoUtil.Echo(msg);

            if (mode == ContinueAfterMatch.PauseWaitAcknowledge)
            {
                Log("Claim your egg before closing the picnic! Alternatively you can manually run to collect all present eggs, go back to the HOME screen, type $toss, and let it continue scanning from there.");
                await Click(HOME, 0_700, token).ConfigureAwait(false);

                IsWaiting = true;
                while (IsWaiting)
                    await Task.Delay(1_000, token).ConfigureAwait(false);

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
            var Data = await SwitchConnection.ReadBytesMainAsync(Offsets.LoadedIntoRaid, 1, token).ConfigureAwait(false);
            return Data[0]; // 1 when in picnic, 2 in sandwich menu, 3 when eating, 2 when done eating
        }

        private async Task<bool> IsInPicnic(CancellationToken token)
        {
            var Data = await SwitchConnection.ReadBytesMainAsync(Offsets.LoadedIntoRaid, 1, token).ConfigureAwait(false);
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
            {
                await Click(A, 5_000, token).ConfigureAwait(false); // Wait a long time to give the flag a chance to update and avoid sandwich re-entry
            }

            await Task.Delay(2_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -10000, 0_500, token).ConfigureAwait(false); // Face down to basket
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 5000, 0_200, token).ConfigureAwait(false); // Face up to basket
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
        }

        private async Task GrabValues(CancellationToken token)
        {
            var ofs = await GetPointerAddress(TextBox, token).ConfigureAwait(false);
            TextVal = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);
        }

        private async Task RetrieveEgg(bool match, CancellationToken token)
        {
            var b1s1 = await GetPointerAddress(B1S1, token).ConfigureAwait(false);
            var ofs = await GetPointerAddress(TextBox, token).ConfigureAwait(false);
            var text = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false);

            Log("There's an egg!");
            if (TextVal != null)
            {
                while (!text.SequenceEqual(TextVal)) // No egg
                {
                    await Click(A, 1_500, token).ConfigureAwait(false);

                    var dumpmon = await ReadBoxPokemonSV(b1s1, 344, token).ConfigureAwait(false);
                    if (dumpmon != null && (Species)dumpmon.Species != Species.None)
                    {
                        DumpPokemon(DumpSetting.DumpFolder, "eggs", dumpmon);
                        await Task.Delay(1_500, token).ConfigureAwait(false);
                        if (match)
                            await SetBoxPokemonEgg(Blank, InjectBox, InjectSlot, token).ConfigureAwait(false);
                    }
                    text = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false);
                }
            }
        }
    }
}
