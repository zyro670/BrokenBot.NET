using PKHeX.Core;
using SysBot.Base;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.EggSettingsSV;

namespace SysBot.Pokemon
{
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
        private const int InjectBox = 0;
        private const int InjectSlot = 0;
        private readonly uint EggData = 0x04386040;
        private readonly uint PicnicMenu = 0x04416020;
        private static readonly PK9 Blank = new();
        private readonly byte[] BlankVal = { 0x01 };
        private const string TextBox = "[[[[[main+43A7550]+20]+400]+48]+F0]";
        private const string B1S1 = "[[[main+43A77C8]+108]+9B0]";
        private byte[]? TextVal = Array.Empty<byte>();
        private ulong OverworldOffset;

        public override async Task MainLoop(CancellationToken token)
        {
            await InitializeHardware(Hub.Config.EggSWSH, token).ConfigureAwait(false);

            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);

            await SetupBoxState(token).ConfigureAwait(false);

            Log("Starting main EggBot loop.");
            Config.IterateNextRoutine();
            while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.EggFetch)
            {
                try
                {
                    await InnerLoop(token).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
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
            await CleanExit(Hub.Config.Trade, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Return true if we need to stop looping.
        /// </summary>
        private async Task InnerLoop(CancellationToken token)
        {
            await SetCurrentBox(0, token).ConfigureAwait(false);
            await SwitchConnection.WriteBytesMainAsync(BlankVal, PicnicMenu, token).ConfigureAwait(false);

            if (Hub.Config.EggSV.EggBotMode == EggMode.CollectAndDump)
            {
                for (int i = 0; i < 2; i++)
                    await Click(A, 1_000, token).ConfigureAwait(false);

                await GrabValues(token).ConfigureAwait(false);
            }            

            if (Settings.EatFirst == true)
                await MakeSandwich(token).ConfigureAwait(false);

            var task = Hub.Config.EggSV.EggBotMode switch
            {
                EggMode.CollectAndDump => PerformEggRoutine(token),
                EggMode.WaitAndClose => WaitForEggs(token),
                _ => PerformEggRoutine(token),
            };
            await task.ConfigureAwait(false);
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
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false)) //wait until we return to the overworld
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

                else if (overworldWaitCycles >= 53) //if still not in the overworld after ~1 minute of trying, hard reset the game
                {
                    overworldWaitCycles = 0;
                    Log("Failed to return to the overworld after 1 minute.  Forcing a game reset.");
                    await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                    OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);  //re-acquire overworld offset to escape the while loop
                    hasReset = true;
                }
            }
            for (int i = 0; i < 10; i++)
                await Click(A, 0_500, token).ConfigureAwait(false); // Click A alot incase pokemon are not level 100
            await Click(X, 1_500, token).ConfigureAwait(false);
            if (hasReset) //If we are starting fresh, we need to reposition over the picnic button
            {
                await Click(DRIGHT, 0_250, token).ConfigureAwait(false);
                await Click(DDOWN, 0_250, token).ConfigureAwait(false);
                await Click(DDOWN, 0_250, token).ConfigureAwait(false);
            }
            await Click(A, 7_000, token).ConfigureAwait(false); //first picnic *might* take longer.  value originally was 4_500
        }

        private async Task WaitForEggs(CancellationToken token)
        {
            PK9 pkprev = new();
            while (!token.IsCancellationRequested)
            {
                var wait = TimeSpan.FromMinutes(30);
                var endTime = DateTime.Now + wait;
                var ctr = 0;
                var waiting = 0;
                while (DateTime.Now < endTime)
                {
                    var pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                    while (pkprev.EncryptionConstant == pk.EncryptionConstant || pk == null || (Species)pk.Species == Species.None)
                    {
                        waiting++;
                        await Task.Delay(1_500, token).ConfigureAwait(false);
                        pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                        if (waiting == 80)
                        {
                            Log("2 minutes have passed without an egg.  Attempting full recovery.");
                            await ReopenPicnic(token).ConfigureAwait(false);
                            await MakeSandwich(token).ConfigureAwait(false);
                            await ReopenPicnic(token).ConfigureAwait(false);
                            wait = TimeSpan.FromMinutes(30);
                            endTime = DateTime.Now + wait;
                            waiting = 0;
                            ctr = 0;
                        }
                    }

                    while (pk != null && (Species)pk.Species != Species.None && pkprev.EncryptionConstant != pk.EncryptionConstant)
                    {
                        waiting = 0;
                        eggcount++;
                        var print = Hub.Config.StopConditions.GetPrintName(pk);
                        Log($"Encounter: {eggcount}{Environment.NewLine}{print}{Environment.NewLine}");
                        Settings.AddCompletedEggs();
                        TradeExtensions<PK9>.EncounterLogs(pk, "EncounterLogPretty_Egg.txt");
                        ctr++;
                        bool match = CheckEncounter(print, pk);
                        if (!match)
                        {
                            Log("Make sure to pick up your egg in the basket!");
                            return;
                        }
                        pkprev = pk;
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
                await MakeSandwich(token).ConfigureAwait(false);
                await WaitForEggs(token).ConfigureAwait(false);
            }
        }

        private async Task PerformEggRoutine(CancellationToken token)
        {
            PK9 pkprev = new();
            while (!token.IsCancellationRequested)
            {
                var wait = TimeSpan.FromMinutes(30);
                var endTime = DateTime.Now + wait;

                while (DateTime.Now < endTime)
                {
                    var pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                    while (pkprev.EncryptionConstant == pk.EncryptionConstant || pk == null || (Species)pk.Species == Species.None)
                    {
                        await Task.Delay(1_500, token).ConfigureAwait(false);
                        pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                    }

                    while (pk != null && (Species)pk.Species != Species.None && pkprev.EncryptionConstant != pk.EncryptionConstant)
                    {
                        eggcount++;
                        var print = Hub.Config.StopConditions.GetPrintName(pk);
                        Log($"Encounter: {eggcount}{Environment.NewLine}{print}{Environment.NewLine}");
                        Settings.AddCompletedEggs();
                        TradeExtensions<PK9>.EncounterLogs(pk, "EncounterLogPretty_Egg.txt");

                        bool match = CheckEncounter(print, pk);

                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        await Click(A, 2_500, token).ConfigureAwait(false);
                        await Click(A, 1_200, token).ConfigureAwait(false);

                        await RetrieveEgg(token).ConfigureAwait(false);
                        if (!match)
                            return;

                        pkprev = pk;
                    }
                    for (int i = 0; i < 2; i++)
                        await Click(PLUS, 0_500, token).ConfigureAwait(false);
                    await Click(B, 1_000, token).ConfigureAwait(false);
                    Log("Waiting..");
                }
                Log("30 minutes have passed, remaking sandwich.");
                await MakeSandwich(token).ConfigureAwait(false);
                await PerformEggRoutine(token).ConfigureAwait(false);
            }
        }

        private bool CheckEncounter(string print, PK9 pk)
        {
            if (!StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
                return true;

            // no need to take a video clip of us receiving an egg.
            var mode = Settings.ContinueAfterMatch;
            var msg = $"Result found!\n{print}\n" + mode switch
            {
                ContinueAfterMatch.Continue => "Continuing...",
                ContinueAfterMatch.PauseWaitAcknowledge => "Waiting for instructions to continue.",
                ContinueAfterMatch.StopExit => "Stopping routine execution; restart the bot to search again.",
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                msg = $"{Hub.Config.StopConditions.MatchFoundEchoMention} {msg}";

            Log(print);

            if (Settings.OneInOneHundredOnly == true && (Species)pk.Species == Species.Dunsparce && pk.EncryptionConstant % 100 != 0)
                return true;

            if (mode == ContinueAfterMatch.StopExit)
                return false;
            if (mode == ContinueAfterMatch.Continue)
                return true;

            EchoUtil.Echo(msg);

            IsWaiting = true;
            while (IsWaiting)
                Task.Delay(1_000, CancellationToken.None).ConfigureAwait(false);
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

        private async Task<bool> IsInPicnic(CancellationToken token)
        {
            var Data = await SwitchConnection.ReadBytesMainAsync(PicnicMenu, 1, token).ConfigureAwait(false);
            return Data[0] == 0x01; // 1 when in picnic, 2 in sandwich menu, 3 when eating, 2 when done eating
        }

        private async Task MakeSandwich(CancellationToken token)
        {
            await Click(MINUS, 0_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 30000, 0_700, token).ConfigureAwait(false); // Face up to table
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Click(A, 1_500, token).ConfigureAwait(false);
            await Click(A, 4_000, token).ConfigureAwait(false);
            await Click(X, 1_500, token).ConfigureAwait(false);

            for (int i = 0; i < 0; i++)
            {
                if (Settings.Item1DUP == true)
                    await Click(DUP, 0_800, token).ConfigureAwait(false);
                else
                    await Click(DDOWN, 0_800, token).ConfigureAwait(false);
            }

            await Click(A, 0_800, token).ConfigureAwait(false);
            await Click(PLUS, 0_800, token).ConfigureAwait(false);

            for (int i = 0; i < 4; i++)
            {
                if (Settings.Item2DUP == true)
                    await Click(DUP, 0_800, token).ConfigureAwait(false);
                else
                    await Click(DDOWN, 0_800, token).ConfigureAwait(false);
            }

            await Click(A, 0_800, token).ConfigureAwait(false);

            for (int i = 0; i < 1; i++)
            {
                if (Settings.Item3DUP == true)
                    await Click(DUP, 0_800, token).ConfigureAwait(false);
                else
                    await Click(DDOWN, 0_800, token).ConfigureAwait(false);
            }

            await Click(A, 0_800, token).ConfigureAwait(false);
            await Click(PLUS, 0_800, token).ConfigureAwait(false);
            await Click(A, 8_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 30000, Settings.HoldUpToIngredients, token).ConfigureAwait(false); // Navigate to ingredients
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

            sandwichcount++;
            Log($"Sandwiches Made: {sandwichcount}");
            for (int i = 0; i < 5; i++)
                await Click(A, 0_800, token).ConfigureAwait(false);

            bool inPicnic = await IsInPicnic(token).ConfigureAwait(false);

            while (!inPicnic)
            {
                await Click(A, 3_000, token).ConfigureAwait(false);
                inPicnic = await IsInPicnic(token).ConfigureAwait(false);
            }

            if (inPicnic)
            {
                await Task.Delay(2_500, token).ConfigureAwait(false);
                await SetStick(LEFT, 0, -10000, 0_500, token).ConfigureAwait(false); // Face down to basket
                await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
                await Task.Delay(1_000, token).ConfigureAwait(false);
                await SetStick(LEFT, 0, 5000, 0_200, token).ConfigureAwait(false); // Face up to basket
                await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            }
        }

        private async Task GrabValues(CancellationToken token)
        {
            var ofs = await GetPointerAddress(TextBox, token).ConfigureAwait(false);
            TextVal = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);
        }

        private async Task RetrieveEgg(CancellationToken token)
        {
            var b1s1 = await GetPointerAddress(B1S1, token).ConfigureAwait(false);
            var ofs = await GetPointerAddress(TextBox, token).ConfigureAwait(false);
            var text = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false);

            Log("There's an egg!");
            if (TextVal != null)
            {
                while (!text.SequenceEqual(TextVal)) // No egg
                {
                    await Click(A, 1_000, token).ConfigureAwait(false);

                    var dumpmon = await ReadBoxPokemonSV(b1s1, 344, token).ConfigureAwait(false);
                    if (dumpmon != null && (Species)dumpmon.Species != Species.None)
                    {
                        DumpPokemon(DumpSetting.DumpFolder, "eggs", dumpmon);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await SetBoxPokemonEgg(Blank, InjectBox, InjectSlot, token).ConfigureAwait(false);
                    }
                    text = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false);
                }
            }
        }
    }
}
