using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
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
        private readonly int[] DesiredMinIVs;
        private readonly int[] DesiredMaxIVs;
        private readonly List<int[]> DesiredIVs = new();
        private readonly EggSettingsSV Settings;
        public ICountSettings Counts => Settings;

        public EggBotSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
        {
            Hub = hub;
            Settings = Hub.Config.EggSV;
            DumpSetting = Hub.Config.Folder;
            StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
            InitializeTargetIVs(Settings, out DesiredIVs);
        }

        private int eggcount = 0;
        private int sandwichcount = 0;
        private PK9 prevShiny = new();
        private static readonly PK9 Blank = new();
        private readonly byte[] BlankVal = { 0x01 };
        private ulong BoxStartOffset;
        private ulong OverworldOffset;

        public override async Task MainLoop(CancellationToken token)
        {
            await InitializeHardware(Hub.Config.EggSV, token).ConfigureAwait(false);
            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);
            await InitializeSessionOffsets(token).ConfigureAwait(false);
            await SetupBoxState(token).ConfigureAwait(false);
            await SetCurrentBox(0, token).ConfigureAwait(false);

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
            if (Settings.StopConditions.Count < 1)
            {
                Log("EggBotSV StopConditions criteria is empty. Please add items to the collection and try again.");
                return;
            }

            await SwitchConnection.WriteBytesMainAsync(BlankVal, Offsets.LoadedIntoDesiredState, token).ConfigureAwait(false);
            if (Settings.EatFirst == true)
                await MakeSandwich(token).ConfigureAwait(false);

            await WaitForEggs(token).ConfigureAwait(false);
            return;
        }

        private async Task SetupBoxState(CancellationToken token)
        {
            var existing = await ReadBoxPokemon(0, 0, token).ConfigureAwait(false);
            if (existing.Species != 0 && existing.ChecksumValid)
            {
                Log("Destination slot is occupied! Dumping the Pokémon found there...");
                DumpPokemon(DumpSetting.DumpFolder, "saved", existing);
            }

            Log("Clearing destination slot to start the bot.");
            await SetBoxPokemonEgg(Blank, BoxStartOffset, token).ConfigureAwait(false);
        }

        private bool IsWaiting;
        public void Acknowledge() => IsWaiting = false;

        private async Task WaitForEggs(CancellationToken token)
        {
            PK9 pkprev = new();
            bool timerDone = false;
            while (!token.IsCancellationRequested)
            {
                var wait = TimeSpan.FromMinutes(30);
                var endTime = DateTime.Now + wait;
                var waiting = 0;
                while (DateTime.Now < endTime || timerDone == true)
                {
                    var pk = await ReadPokemonSV(Offsets.EggData, 344, token).ConfigureAwait(false);
                    while (pk == prevShiny || pk == null || pkprev!.EncryptionConstant == pk.EncryptionConstant || (Species)pk.Species == Species.None)
                    {
                        if (DateTime.Now >= endTime)
                            break;

                        await Task.Delay(1_500, token).ConfigureAwait(false);
                        pk = await ReadPokemonSV(Offsets.EggData, 344, token).ConfigureAwait(false);
                        waiting++;

                        if (waiting == 120)
                        {
                            await Click(A, 1_500, token).ConfigureAwait(false);
                            waiting = 0;
                        }
                    }

                    Log("Possible egg generated..checking the basket..");
                    bool nomatch = await RetrieveEgg(token).ConfigureAwait(false);
                    if (nomatch)
                        await SetBoxPokemonEgg(Blank, BoxStartOffset, token).ConfigureAwait(false);

                    if (!nomatch && Settings.ContinueAfterMatch == ContinueAfterMatch.StopExit)
                    {
                        prevShiny = pk!;
                        await Click(HOME, 0_500, token).ConfigureAwait(false);
                        return;
                    }
                    pkprev = pk!;
                    waiting = 0;
                }
                Log("30 minutes have passed, remaking sandwich.");
                await MakeSandwich(token).ConfigureAwait(false);
            }
        }

        private async Task<bool> CheckEncounter(string print, PK9 pk)
        {
            var token = CancellationToken.None;
            string? url;

            bool invalid = false;
            bool[] results = EggFound(pk, DesiredIVs, Settings);
            List<EggFetchStopConditionsCatgeory> list = Settings.StopConditions;
            for (int r = 0; r < results.Length; r++)
            {
                if (results[r] == false)
                    continue;

                invalid = true;
            }

            if (invalid == false)
            {
                foreach (var param in list)
                {
                    if (param.ShinyTarget is TargetShinyType.AnyShiny or TargetShinyType.StarOnly or TargetShinyType.SquareOnly && pk.IsShiny)
                    {
                        url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                        EchoUtil.EchoEmbed("", print, url, "", false);
                        break;
                    }
                }
                return true;
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
                Log("Match found! Make sure to move your match to a different spot from Box 1 Slot 1 or it will be deleted on the next bot start.");
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
            BoxStartOffset = await SwitchConnection.PointerAll(Offsets.BoxStartPokemonPointer, token).ConfigureAwait(false);
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
            Log("Caching offsets complete!");
        }

        private async Task<bool> RetrieveEgg(CancellationToken token)
        {
            bool match = false;
            for (int a = 0; a < 4; a++)
                await Click(A, 1_500, token).ConfigureAwait(false);

            var pk = await ReadBoxPokemonSV(BoxStartOffset, 344, token).ConfigureAwait(false);
            if (pk != null && (Species)pk.Species != Species.None)
            {
                Log("There's an egg!");
                eggcount++;
                var print = $"Encounter: {eggcount}{Environment.NewLine}{Hub.Config.StopConditions.GetSpecialPrintName(pk)}{Environment.NewLine}Ball: {(Ball)pk.Ball}";
                Log(print);
                Settings.AddCompletedEggs();
                TradeExtensions<PK9>.EncounterLogs(pk, "EncounterLogPretty_EggSV.txt");
                TradeExtensions<PK9>.EncounterScaleLogs(pk, "EncounterLogScalePretty.txt");
                match = await CheckEncounter(print, pk).ConfigureAwait(false);
                DumpPokemon(DumpSetting.DumpFolder, "eggs", pk);
                await Click(MINUS, 1_500, token).ConfigureAwait(false);
                for (int a = 0; a < 2; a++)
                    await Click(A, 1_500, token).ConfigureAwait(false);
                return match;
            }

            Log("No egg..darn item check :(");
            for (int a = 0; a < 2; a++)
                await Click(A, 1_500, token).ConfigureAwait(false);
            return match;
        }
    }
}
