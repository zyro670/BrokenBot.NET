using PKHeX.Core;
using SysBot.Base;
using System.Text.RegularExpressions;
using System.Text;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

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
        private uint EggData = 0x04386040;
        private uint PicnicMenu = 0x04416020;
        private static readonly PK9 Blank = new();
        private byte[] BlankVal = { 0x01 };
        private const string TextBox = "[[[[[main+43A7550]+20]+400]+48]+F0]";
        private const string B1S1 = "[[[main+43A77C8]+108]+9B0]";

        public override async Task MainLoop(CancellationToken token)
        {
            await InitializeHardware(Hub.Config.EggSWSH, token).ConfigureAwait(false);

            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);

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
            var textval = await GetDialogueAction(token).ConfigureAwait(false);
            await Click(A, 1_500, token).ConfigureAwait(false);
            if (Settings.EatFirst == true)
            {
                await MakeSandwich(token).ConfigureAwait(false);
                await PerformEggRoutine(textval, token).ConfigureAwait(false);
            }
            else
                await PerformEggRoutine(textval, token).ConfigureAwait(false);
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

        private async Task PerformEggRoutine(byte[]? textval, CancellationToken token)
        {
            PK9 pk = new();
            PK9 pkprev = new();
            var b1s1 = await GetPointerAddress(B1S1, token).ConfigureAwait(false);
            var ofs = await GetPointerAddress(TextBox, token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                DateTime currentTime = DateTime.Now;
                DateTime TimeLater = currentTime.AddMinutes(30);

                while (TimeLater > DateTime.Now)
                {
                    pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                    pkprev = pk;
                    while (pkprev.EncryptionConstant == pk.EncryptionConstant || pk == null || (Species)pk.Species == Species.None)
                    {
                        await Task.Delay(2_500, token).ConfigureAwait(false);
                        pk = await ReadPokemonSV(EggData, 344, token).ConfigureAwait(false);
                    }

                    while (pk != null && (Species)pk.Species != Species.None && pkprev.EncryptionConstant != pk.EncryptionConstant)
                    {
                        eggcount++;
                        Log($"There's an egg!");
                        var print = Hub.Config.StopConditions.GetPrintName(pk);
                        Log($"Encounter: {eggcount}{Environment.NewLine}{print}{Environment.NewLine}");
                        Settings.AddCompletedEggs();
                        TradeExtensions<PK9>.EncounterLogs(pk, "EncounterLogPretty_Egg.txt");

                        bool match = CheckEncounter(print, pk);
                        if (!match)
                            return;

                        Log("Grabbing egg...");
                        await Click(A, 1_500, token).ConfigureAwait(false);
                        var currenttext = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 16, token).ConfigureAwait(false);

                        while (!currenttext.SequenceEqual(textval))
                        {
                            await Click(A, 1_500, token).ConfigureAwait(false);
                            var dumpmon = await ReadBoxPokemonSV(b1s1, 344, token).ConfigureAwait(false);
                            if (dumpmon != null && (Species)dumpmon.Species != Species.None)
                            {
                                DumpPokemon(DumpSetting.DumpFolder, "eggs", dumpmon);

                                await Task.Delay(0_500, token).ConfigureAwait(false);
                                await SetBoxPokemonEgg(Blank, InjectBox, InjectSlot, token).ConfigureAwait(false);
                            }
                            currenttext = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 16, token).ConfigureAwait(false);
                        }

                        pkprev = pk;

                        Log("Waiting..");
                        await Click(A, 1_000, token).ConfigureAwait(false);
                    }
                }

                await MakeSandwich(token).ConfigureAwait(false);
                continue;
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

        private async Task<byte[]?> GetDialogueAction(CancellationToken token)
        {
            // Click A twice to get us to the dialogue where no egg is present
            for (int i = 0; i < 2; i++)
                await Click(A, 1_000, token).ConfigureAwait(false);

            var ofs = await GetPointerAddress(TextBox, token).ConfigureAwait(false);
            var text = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 16, token).ConfigureAwait(false);
            string result = Encoding.ASCII.GetString(text);
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            result = rgx.Replace(result, "");
            Log($"Dialogue text for no egg present: {result}");

            return text;
        }
    }
}
