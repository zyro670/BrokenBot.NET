using PKHeX.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.PokeDataOffsets;

namespace SysBot.Pokemon
{
    public sealed class CurryBot : EncounterBot
    {
        private readonly CurryBotSettings Settings;
        private readonly StopConditionSettings StopSettings;
        private int charizardclass;
        private int copperajahclass;
        private int milceryclass;
        private int wobbuffetclass;
        private int koffingclass;
        private byte[] BerryPouch = { 0 };
        private byte[] IngredientPouch = { 0 };
        private int curryCount = 0;
        private bool ScrollUpIngr;
        private bool ScrollUpBerry;
        private int IngredientCount;
        private int BerryCount;

        public CurryBot(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg, hub)
        {
            Settings = Hub.Config.Curry;
            StopSettings = Hub.Config.StopConditions;
        }

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            if (StopSettings.MarkOnly)
                StopSettings.MarkOnly = false;

            await SetCurrentBox(0, token).ConfigureAwait(false);
            var existing = await ReadBoxPokemon(0, 0, token).ConfigureAwait(false);
            if (Hub.Config.Folder.Dump && existing.Species != 0 && existing.ChecksumValid)
            {
                Log("Destination slot is occupied! Dumping the Pokémon found there...");
                DumpPokemon(Hub.Config.Folder.DumpFolder, "saved", existing);
            }

            Log("Clearing destination slot to start the bot.");
            var blank = new PK8();
            await SetBoxPokemon(blank, 0, 0, token).ConfigureAwait(false);

            Log("Logging berry and ingredient counts...");
            int ingrIndex = await GetIngredientIndex(token).ConfigureAwait(false);
            int berryIndex = await GetBerryIndex(token).ConfigureAwait(false);
            int berryCount = BerryCount;
            int ingredientCount = IngredientCount;
            await DoCurryMonEncounter(ingrIndex, berryIndex, berryCount, ingredientCount, token).ConfigureAwait(false);
        }

        private async Task DoCurryMonEncounter(int ingrIndex, int berryIndex, int berryCount, int ingredientCount, CancellationToken token)
        {
            bool firstRun = true;
            PK8? comparison = null;
            while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.CurryBot)
            {
                if (await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
                {
                    Log("Entering camp...");
                    await Click(X, 2_000, token).ConfigureAwait(false);
                    await Click(A, Settings.EnterCamp, token).ConfigureAwait(false);
                }

                if (!await LairStatusCheck(0xFF000000, 0x6B311300, token).ConfigureAwait(false)) // Check if camp screen.
                    await Click(B, 2_000, token).ConfigureAwait(false);

                await CookingCurry(ingrIndex, berryIndex, berryCount, firstRun, token).ConfigureAwait(false);
                ingredientCount--;
                berryCount -= berryCount >= 10 ? 10 : 1;
                firstRun = false;

                Log("Checking for a camper...");
                PK8? camperMon = await GetCampPokemon(token).ConfigureAwait(false);
                if (camperMon != null && string.IsNullOrEmpty(camperMon.OT_Name) && camperMon != comparison)
                {
                    comparison = camperMon;
                    await SetStick(RIGHT, 0, 30_000, 1_000, token).ConfigureAwait(false);
                    await SetStick(RIGHT, 0, 0, 0_100, token).ConfigureAwait(false);
                    Log($"New camper found on curry #{curryCount}.");
                    TradeExtensions<PK8>.EncounterLogs(camperMon, "EncounterLogPretty_Curry.txt");
                    if (await HandleEncounter(camperMon, token).ConfigureAwait(false))
                        return;
                }
                else Log($"No new campers on curry #{curryCount}...");

                if (!Settings.RestorePouches && (ingredientCount <= 0 || berryCount <= 0))
                {
                    Log("Ran out of ingredients to make curry. Stopping...");
                    return;
                }
                else if (Settings.RestorePouches && (ingredientCount <= 1 || berryCount <= 10))
                {
                    Log("Restoring ingredient and berry pouches...");
                    await Connection.WriteBytesAsync(BerryPouch, BerryPouchOffset, token).ConfigureAwait(false);
                    berryCount = BerryCount;
                    await Connection.WriteBytesAsync(IngredientPouch, IngredientPouchOffset, token).ConfigureAwait(false);
                    ingredientCount = IngredientCount;
                }
            }
        }

        private async Task CookingCurry(int ingr, int berry, int berryCount, bool firstRun, CancellationToken token)
        {
            var sw = new Stopwatch();
            await Click(X, 0_500, token).ConfigureAwait(false);
            if (firstRun)
                await Click(DRIGHT, 0_250, token).ConfigureAwait(false);

            Log("Let's make curry!");
            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 5_000, token).ConfigureAwait(false);

            Log("Selecting ingredients...");
            for (int i = 0; i < ingr; i++)
                await Click(ScrollUpIngr ? DUP : DDOWN, 0_150, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);

            Log("Selecting berries...");
            for (int i = 0; i < berry; i++)
                await Click(ScrollUpBerry ? DUP : DDOWN, 0_200, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);
            bool bunchOfBerries = berryCount >= 10;
            if (bunchOfBerries)
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);

            await Click(A, bunchOfBerries ? 2_000 : 1_000, token).ConfigureAwait(false);
            if (!bunchOfBerries)
                await Click(PLUS, 1_000, token).ConfigureAwait(false);

            Log("Dropping ingredients in!");
            await Click(A, Settings.IngredientDrop, token).ConfigureAwait(false);

            Log("Time to cook!");
            sw.Start();
            while (sw.ElapsedMilliseconds < Settings.FanningDuration - 4_000)
                Click(A, 0_100, token).Wait();

            while (sw.ElapsedMilliseconds < Settings.FanningDuration)
                Click(A, 0_150, token).Wait();

            Log("Stirring the pot!");
            sw.Restart();
            while (sw.ElapsedMilliseconds < Settings.StirringDuration)
            {
                SetStick(RIGHT, -30_000, 0, 0_050, token).Wait(); // ←
                SetStick(RIGHT, 0, 30_000, 0_050, token).Wait(); // ↑
                SetStick(RIGHT, 30_000, 0, 0_050, token).Wait(); // →
                SetStick(RIGHT, 0, -30_000, 0_050, token).Wait(); // ↓
            }
            sw.Stop();

            await Task.Delay(Settings.SprinkleOfLove).ConfigureAwait(false);
            Log("Adding a sprinkle of love!");
            await Click(A, Settings.CurryChowCutscene, token).ConfigureAwait(false); // Delay until we can present our curry.
            await SetStick(RIGHT, 0, 0, 0_100, token).ConfigureAwait(false);

            Log("Presenting our curry!");
            curryCount++;
            Settings.AddCompletedCurries();
            string msg = await GetCurryRating(token).ConfigureAwait(false);
            Log($"The curry has a {msg} class taste rating!\n" +
                $"Current Cooking Session Totals\n" +
                $"- Charizard: {charizardclass}\n" +
                $"- Copperajah: {copperajahclass}\n" +
                $"- Milcery: {milceryclass}\n" +
                $"- Wobbuffet: {wobbuffetclass}\n" +
                $"- Koffing: {koffingclass}");

            await Click(A, 12_000, token).ConfigureAwait(false); // Delay until cutscene is over.
            await Click(A, 3_000, token).ConfigureAwait(false); // Delay until camp.
        }

        private async Task<PK8?> GetCampPokemon(CancellationToken token)
        {
            string[] campers =
            {
                "[[[[[[main+2636120]+280]+D8]+78]+10]+98]",
                "[[[[[main+2636170]+2F0]+58]+130]+138]+D0",
                "[[[[main+28ED668]+68]+1E8]+1D0]+128",
                "[[[[[main+296C030]+60]+40]+1B0]+58]"
            };

            for (int i = 0; i < campers.Length; i++)
            {
                var pointer = campers[i];
                var ofs = await ParsePointer(pointer, token).ConfigureAwait(false);
                var pk = await ReadUntilPresentAbsolute(ofs, 0_500, 0_250, token).ConfigureAwait(false);
                if (pk != null)
                    return pk;
            }
            return null;
        }

        private async Task<string> GetCurryRating(CancellationToken token)
        {
            var rating = await Connection.ReadBytesAsync(0x2C2A909E, 1, token).ConfigureAwait(false);
            string result = System.Text.Encoding.ASCII.GetString(rating);
            string[] expectedVals = { "m", "v", "g", "n", "b" };
            if (!expectedVals.Contains(result))
            {
                Log("Checking backup rating offset...");
                rating = await Connection.ReadBytesAsync(0x2C2B009E, 1, token).ConfigureAwait(false);
                result = System.Text.Encoding.ASCII.GetString(rating);
            }

            string message = string.Empty;
            switch (result)
            {
                case "m": message = "Charizard"; charizardclass++; break;
                case "v": message = "Copperajah"; copperajahclass++; break;
                case "g": message = "Milcery"; milceryclass++; break;
                case "n": message = "Wobbuffet"; wobbuffetclass++; break;
                case "b": message = "Koffing"; koffingclass++; break;
            };
            return message;
        }

        private async Task<int> GetIngredientIndex(CancellationToken token)
        {
            ushort[] ingredients =
            {
                1084, 1085, 1086, 1087, 1088, 1089, 1090, 1091, 1092, 1093,
                1094, 1095, 1096, 1097, 1098, 1099, 1256, 1257, 1258, 1259,
                1260, 1261, 1262, 1263, 1264,
            };

            IngredientPouch = await Connection.ReadBytesAsync(IngredientPouchOffset, 100, token).ConfigureAwait(false);
            var pouch = GetItemPouch(IngredientPouch, InventoryType.Ingredients, ingredients, 999, 0, ingredients.Length);
            var item = pouch.Items.FirstOrDefault(x => x.Index == (int)Settings.Ingredient && x.Count > 0);
            if (item == default)
                item = pouch.Items.FirstOrDefault(x => x.Count > 0);

            IngredientCount = item.Count;
            var index = pouch.Items.ToList().IndexOf(item);
            ScrollUpIngr = pouch.Items.Length - index < index;
            return ScrollUpIngr ? pouch.Items.Length - index : index;
        }

        private async Task<int> GetBerryIndex(CancellationToken token)
        {
            ushort[] berries =
            {
                149, 150, 151, 152, 153, 154, 155, 156, 157, 158,
                159, 160, 161, 162, 163, 169, 170, 171, 172, 173,
                174, 184, 185, 186, 187, 188, 189, 190, 191, 192,
                193, 194, 195, 196, 197, 198, 199, 200, 201, 202,
                203, 204, 205, 206, 207, 208, 209, 210, 211, 212,
                686, 687, 688,
            };

            BerryPouch = await Connection.ReadBytesAsync(BerryPouchOffset, 212, token).ConfigureAwait(false);
            var pouch = GetItemPouch(BerryPouch, InventoryType.Berries, berries, 999, 0, berries.Length);
            var item = pouch.Items.FirstOrDefault(x => x.Index == (int)Settings.Berry && x.Count > 0);
            if (item == default)
                item = pouch.Items.FirstOrDefault(x => x.Count >= 10);

            BerryCount = item.Count;
            var index = pouch.Items.ToList().IndexOf(item);
            ScrollUpBerry = pouch.Items.Length - index < index;
            return ScrollUpBerry ? pouch.Items.Length - index : index;
        }

        private InventoryPouch8 GetItemPouch(byte[] data, InventoryType type, ushort[] items, int maxCount, int offset, int length)
        {
            var pouch = new InventoryPouch8(type, items, maxCount, offset, length);
            pouch.GetPouch(data);
            return pouch;
        }
    }
}
