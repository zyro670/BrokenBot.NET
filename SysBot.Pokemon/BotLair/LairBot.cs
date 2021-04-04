using System;
using PKHeX.Core;
using SysBot.Base;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsets;

namespace SysBot.Pokemon
{
    // Thanks to Anubis and Zyro for providing offsets and ideas for LairBot, and Elvis for endless testing with PinkBot!
    public class LairBot : PokeRoutineExecutor
    {
        private StopConditionSettings NewSCSettings = new();
        private readonly BotCompleteCounts Counts;
        private readonly PokeTradeHub<PK8> Hub;
        private readonly LairSettings Settings;
        private readonly IDumper DumpSetting;
        private readonly int[] DesiredIVs;
        private byte[] OtherItemsPouch = { 0 };
        private byte[] BallPouch = { 0 };
        private ulong MainNsoBase;
        private bool StopBot;
        private bool Upgrade;
        private int encounterCount;
        private int catchCount = -1;
        private int resetCount;
        private int lairCount;

        public LairBot(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg)
        {
            Hub = hub;
            Counts = Hub.Counts;
            Settings = hub.Config.Lair;
            DumpSetting = Hub.Config.Folder;
            DesiredIVs = StopConditionSettings.InitializeTargetIVs(Hub);
        }

        public override async Task MainLoop(CancellationToken token)
        {
            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);
            Log("It's adventure time! Starting main LairBot loop.");
            Config.IterateNextRoutine();
            MainNsoBase = await SwitchConnection.GetMainNsoBaseAsync(token).ConfigureAwait(false);
            _ = Task.Run(async () => await CancellationMonitor(token).ConfigureAwait(false));
            await DoLairBot(token).ConfigureAwait(false);
            await DetachController(token).ConfigureAwait(false);
        }
        
        private async Task DoLairBot(CancellationToken token)
        {
            int raidCount = 1;
            int caught = 0;
            while (!token.IsCancellationRequested)
            {
                bool lost = false;
                Upgrade = false;
                if (raidCount == 1)
                {
                    while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
                        await Click(A, 0_500, token).ConfigureAwait(false);

                    Log($"{(StopBot ? "Waiting for next Legendary Adventure... Use \"$hunt (Species)\" to select a new Legendary!" : $"Starting a new Adventure...")}");
                    if (StopBot)
                    {
                        StopBot = false;
                        return;
                    }

                    if (!await SettingsCheck(token).ConfigureAwait(false))
                        return;

                    await SetHuntedPokemon(token).ConfigureAwait(false);
                    if (!await LairSeedInjector(token).ConfigureAwait(false))
                        return;

                    ulong seed = BitConverter.ToUInt64(await Connection.ReadBytesAsync(AdventureSeedOffset, 8, token).ConfigureAwait(false), 0);
                    Log($"Here is your current Lair Seed: {seed:X16}");
                    await Click(A, 0_500, token).ConfigureAwait(false);
                    while (true)
                    {
                        if (await LairStatusCheck(CurrentScreen_LairMenu, CurrentScreenLairOffset, token).ConfigureAwait(false))
                            break;
                        await Click(A, 0_600 + Settings.MashDelay, token).ConfigureAwait(false);
                    }

                    var huntedSpecies = BitConverter.ToUInt16(await Connection.ReadBytesAsync(LairSpeciesNote1, 2, token).ConfigureAwait(false), 0);
                    Log($"Starting a Solo Adventure for {(huntedSpecies == 0 ? "a random Legendary" : (LairSpecies)huntedSpecies)}!");
                    await Task.Delay(2_000).ConfigureAwait(false);
                    await RentalRoutine(token).ConfigureAwait(false); // Select a rental.
                }

                while (!await LairStatusCheck(AdventurePathBytes, CurrentScreenLairOffset, token).ConfigureAwait(false)) // Delay until in path select screen.
                    await Task.Delay(0_500).ConfigureAwait(false);

                await Task.Delay(raidCount == 1 ? 8_000 : 2_500).ConfigureAwait(false); // Because map scroll is slow.
                await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7900E81F), MainNsoBase + DamageOutputOffset, token).ConfigureAwait(false); // Enable dirty OHKO.
                switch (Hub.Config.Lair.SelectPath) // Choose a path to take.
                {
                    case SelectPath.GoLeft: await Click(A, 1_000, token).ConfigureAwait(false); break;
                    case SelectPath.GoRight: await Click(DRIGHT, 1_000, token).ConfigureAwait(false); break;
                };

                if (raidCount == 1 && !await LegendReset(token).ConfigureAwait(false))
                    continue;

                while (!await IsInBattle(token).ConfigureAwait(false)) // Wait until we're in battle. Also deals with NPCs.
                    await Click(A, 0_500, token).ConfigureAwait(false);

                var lairPkm = await ReadUntilPresent(RaidPokemonOffset, 2_000, 0_200, token).ConfigureAwait(false);
                if (lairPkm == null) // Shouldn't ever be null, but just in case.
                {
                    Log("No clue what just happened. Restarting game...");
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await StartGame(Hub.Config, token).ConfigureAwait(false);
                    continue;
                }
                encounterCount++;
                Log($"Raid Battle {raidCount}. Encounter {encounterCount}: {SpeciesName.GetSpeciesNameGeneration(lairPkm.Species, 2, 8)}{TradeExtensions.FormOutput(lairPkm.Species, lairPkm.Form, out _)}.");

                var ourPkm = await ReadUntilPresent(LairPartyP1Offset, 2_000, 0_200, token).ConfigureAwait(false);
                //PK8?[] party = { await ReadUntilPresent(LairPartyP2Offset, 2_000, 0_200, token).ConfigureAwait(false), await ReadUntilPresent(LairPartyP3Offset, 2_000, 0_200, token).ConfigureAwait(false), await ReadUntilPresent(LairPartyP4Offset, 2_000, 0_200, token).ConfigureAwait(false) };
                if (ourPkm != null)
                    Log($"Sending out: {SpeciesName.GetSpeciesNameGeneration(ourPkm.Species, 2, 8)}{TradeExtensions.FormOutput(ourPkm.Species, ourPkm.Form, out _)}.");
                else ourPkm = new();

                await BattleRoutine(ourPkm, lairPkm, raidCount, token).ConfigureAwait(false);
                while (!await LairStatusCheck(raidCount == 4 ? LegendCatchScreenBytes : LairCatchScreenBytes, CurrentScreenLairOffset, token).ConfigureAwait(false) && !lost)
                {
                    await Task.Delay(1_000).ConfigureAwait(false);
                    lost = !await IsInBattle(token).ConfigureAwait(false) && (await LairStatusCheck(LairRewardsScreenBytes, CurrentScreenLairOffset2, token).ConfigureAwait(false) || await LairStatusCheck(LairDialogueBytes, CurrentScreenLairDialogue, token).ConfigureAwait(false));
                }

                if (lost) // We've lost the battle, exit back to main loop.
                {
                    lairCount++;
                    Log($"Lost Adventure #{lairCount}.");
                    if (caught > 0)
                        await Results(caught, token).ConfigureAwait(false);
                    caught = 0;
                    raidCount = 1;
                    continue;
                }

                caught = await CatchRoutine(raidCount, caught, token).ConfigureAwait(false);
                Log($"{(raidCount == 4 || Settings.CatchLairPokémon || Upgrade ? "Caught" : "Defeated")} {SpeciesName.GetSpeciesNameGeneration(lairPkm.Species, 2, 8)}{TradeExtensions.FormOutput(lairPkm.Species, lairPkm.Form, out _)}.");
                if (raidCount == 4) // Final raid complete.
                {
                    lairCount++;
                    Log($"Adventure #{lairCount} completed.");
                    await Results(caught, token).ConfigureAwait(false);
                    caught = 0;
                    raidCount = 1;
                    continue;
                }
                raidCount++;
            }
        }

        private async Task RentalRoutine(CancellationToken token)
        {
            List<uint> RentalOfsList = new List<uint> { RentalMon1, RentalMon2, RentalMon3 };
            List<int> speedStat = new();
            List<PK8> pkList = new();
            int monIndex = -1;
            int moveIndex = -1;
            await Click(DDOWN, 0_250, token).ConfigureAwait(false);
            await Click(A, 2_000, token).ConfigureAwait(false);

            for (int i = 0; i < RentalOfsList.Count; i++)
            {
                var pk = await ReadUntilPresent(RentalOfsList[i], 2_000, 0_200, token).ConfigureAwait(false);
                if (pk == null)
                {
                    Log("Entered the lobby too fast, correcting...");
                    await CodePink(token).ConfigureAwait(false);
                    return;
                }
                else
                {
                    pkList.Add(pk);
                    moveIndex = LairBotUtil.PriorityIndex(pk);
                    if (moveIndex != -1) // Add Ditto override because Imposter is fun?
                    {
                        monIndex = i;
                        break;
                    }
                    else speedStat.Add(LairBotUtil.CalculateSpeed(pk));
                }
            }
            var speedIndex = speedStat.Count > 0 ? speedStat.IndexOf(speedStat.Max()) : 0;
            if (monIndex == -1)
                Log($"Selecting {SpeciesName.GetSpeciesNameGeneration(pkList[speedIndex].Species, 2, 8)}{TradeExtensions.FormOutput(pkList[speedIndex].Species, pkList[speedIndex].Form, out _)}.");
            else Log($"Selecting {SpeciesName.GetSpeciesNameGeneration(pkList[monIndex].Species, 2, 8)}{TradeExtensions.FormOutput(pkList[monIndex].Species, pkList[monIndex].Form, out _)} with {(Move)pkList[monIndex].Moves[moveIndex]}");
            await MoveAndRentalClicks(monIndex == -1 ? speedIndex : monIndex, token).ConfigureAwait(false);
        }

        private async Task CodePink(CancellationToken token)
        {
            await Click(B, 0_500, token).ConfigureAwait(false);
            while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
                await Task.Delay(1_000).ConfigureAwait(false);
            for (int p = 0; p < 15; p++)
            {
                if (await LairStatusCheck(CurrentScreen_LairMenu, CurrentScreenLairOffset, token).ConfigureAwait(false))
                    break;
                await Click(A, 1_000, token).ConfigureAwait(false);
            }
            await RentalRoutine(token).ConfigureAwait(false);
        }

        private async Task<int> SelectMove(PK8 pk, PK8 lairMon, bool stuck, int oldMove, CancellationToken token)
        {
            int selectIndex = -1;
            var priorityMove = pk.Moves.ToList().IndexOf(pk.Moves.Intersect((IEnumerable<int>)Enum.GetValues(typeof(PriorityMoves))).FirstOrDefault());
            bool priority = priorityMove != -1 && !LairBotUtil.TypeImmunity(pk, lairMon, priorityMove) && lairMon.Ability != (int)Ability.PsychicSurge && lairMon.Ability != (int)Ability.QueenlyMajesty && lairMon.Ability != (int)Ability.Dazzling;
            for (int i = 0; i < pk.Moves.Length; i++) // Select either a priority move, or a move that deals damage (if in battle).
            {
                if (await GetPPCount(priority ? priorityMove : i, token).ConfigureAwait(false) == 0)
                {
                    priority = false;
                    continue;
                }
                else if ((stuck && oldMove == i) || (priorityMove == i && !priority))
                    continue;
                else if (priority)
                    return priorityMove;

                bool recoil = LairBotUtil.Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[i] && x.Recoil >= 206 && x.EffectSequence >= 48) != default;
                if (!LairBotUtil.TypeImmunity(pk, lairMon, i) && pk.Moves[i] != (int)Move.Belch && !recoil && LairBotUtil.Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[i] && x.Power > 0 && !x.Charge) != default)
                {
                    //Log($"Gravity: {gravity}, ImmuneType: {immuneType}, ImmunePriority: {immunePriority}, PPCount: {PPCount}, Move: {(Move)pk.Moves[i]}"); // Debug logging
                    selectIndex = i;
                    break;
                }
            }
            return selectIndex == -1 ? new Random().Next(4) : selectIndex;
        }

        private async Task BattleRoutine(PK8 playerPk, PK8 lairPkm, int raidCount, CancellationToken token)
        {
            if (playerPk.Species == 132 && playerPk.Ability == (int)Ability.Imposter)
                playerPk = lairPkm;
            else if (lairPkm.Species == 132 && lairPkm.Ability == (int)Ability.Imposter)
                lairPkm = playerPk;

            int move = -1;
            var sw = new Stopwatch();
            var ourSpeed = LairBotUtil.CalculateSpeed(playerPk);
            bool noPriority = LairBotUtil.PriorityIndex(playerPk) == -1;
            var lairPkmSpeed = LairBotUtil.CalculateSpeed(lairPkm);
            bool lairPkmPriority = LairBotUtil.PriorityIndex(lairPkm) != -1;
            if ((noPriority && (lairPkmSpeed > ourSpeed)) || (lairPkmPriority && noPriority))
                Upgrade = true;

            while (await IsInBattle(token).ConfigureAwait(false))
            {
                while (!await LairStatusCheck(LairMovesBytes, CurrentScreenLairOffset2, token).ConfigureAwait(false) && await IsInBattle(token).ConfigureAwait(false))
                {
                    if (await LairStatusCheck(raidCount == 4 ? LegendCatchScreenBytes : LairCatchScreenBytes, CurrentScreenLairOffset, token).ConfigureAwait(false))
                        return;
                    else await Click(A, 1_000, token).ConfigureAwait(false);
                }

                if (await LairStatusCheck(LairMovesBytes, CurrentScreenLairOffset2, token).ConfigureAwait(false))
                {
                    bool stuck;
                    if (sw.IsRunning && sw.ElapsedMilliseconds <= 4_000)
                    {
                        Log($"{(Move)playerPk.Moves[move]} cannot be executed, trying to select a different move.");
                        stuck = true;
                        sw.Reset();
                    }
                    else stuck = false;

                    move = await SelectMove(playerPk, lairPkm, stuck, move, token).ConfigureAwait(false);
                    Log($"Selecting {(Move)playerPk.Moves[move]}.");
                    await MoveAndRentalClicks(move, token).ConfigureAwait(false);
                    sw.Start();
                }
            }
            sw.Stop();
        }

        private async Task<int> CatchRoutine(int raidCount, int caught, CancellationToken token)
        {
            await Task.Delay(1_000).ConfigureAwait(false);
            if (Settings.CatchLairPokémon || Upgrade || raidCount == 4) // We want to catch the legendary regardless of settings for catching.
            {
                await SelectCatchingBall(token).ConfigureAwait(false); // Select ball to catch with.
                Log($"Catching {(raidCount < 4 ? "encounter" : "legendary")}...");
                if (raidCount < 4)
                {
                    while (!await LairStatusCheck(LairMonSelectScreenBytes, CurrentScreenLairOffset, token).ConfigureAwait(false)) // Spam A until we're back in a menu.
                        await Task.Delay(1_000).ConfigureAwait(false);
                    
                    if (!Upgrade)
                        await Click(DDOWN, 1_000, token).ConfigureAwait(false);
                    else Log($"Lair encounter is better than our current Pokémon, switching...");
                    await Click(A, 1_000, token).ConfigureAwait(false);
                }
                catchCount--;
                return ++caught;
            }
            else
            {
                await Click(DDOWN, 1_000, token).ConfigureAwait(false);
                await Click(A, 1_000, token).ConfigureAwait(false);
                return 0;
            }
        }

        private async Task Results(int caught, CancellationToken token)
        {
            Counts.AddCompletedAdventures();
            int index = -1;
            int legendSpecies = 0;
            bool stopCond = false;
            while (!await LairStatusCheck(LairRewardsScreenBytes, CurrentScreenLairOffset2, token).ConfigureAwait(false))
                await Task.Delay(1_000).ConfigureAwait(false);

            for (int i = 0; i < caught; i++)
            {
                var jumpAdj = i == 0 ? 0 : i == 1 ? 2 : i == 2 ? 10 : 12;
                var pointer = $"[[[[[main+28F4060]+1B0]+68]+{58 + jumpAdj}]+58]";
                var pk = await ReadUntilPresentAbsolute(await ParsePointer(pointer, token), 2_000, 0_200, token).ConfigureAwait(false);
                if (pk != null)
                {
                    if (pk.IsShiny)
                        index = Settings.CatchLairPokémon ? i : ++index;

                    var caughtLegend = (!Settings.CatchLairPokémon && pk.IsShiny) || (Settings.CatchLairPokémon && index == 3 && pk.IsShiny);
                    var caughtRegular = Settings.CatchLairPokémon && pk.IsShiny && index != 3;
                    legendSpecies = caughtLegend ? pk.Species : 0;
                    if (caughtLegend && StopConditionSettings.EncounterFound(pk, DesiredIVs, NewSCSettings) && Settings.UseStopConditionsPathReset)
                        stopCond = true;

                    if (stopCond || (caughtLegend && Settings.StopOnLegendary))
                        StopBot = true;

                    TradeExtensions.EncounterLogs(pk);
                    if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
                        DumpPokemon(DumpSetting.DumpFolder, "lairs", pk);

                    if (Settings.AlwaysOutputShowdown)
                        Log($"Adventure #{lairCount}.{Environment.NewLine}{ShowdownParsing.GetShowdownText(pk)}{Environment.NewLine}");

                    if (LairBotUtil.EmbedsInitialized && Settings.ResultsEmbedChannels != string.Empty && (caughtLegend || caughtRegular))
                        LairBotUtil.EmbedMon = (pk, caughtLegend);
                    else
                    {
                        if (caughtLegend)
                            EchoUtil.Echo($"{(!NewSCSettings.PingOnMatch.Equals(string.Empty) ? $"<@{NewSCSettings.PingOnMatch}>\n" : "")}Shiny Legendary found!\nEncounter #{encounterCount}. Adventure #{lairCount}.{Environment.NewLine}{ShowdownParsing.GetShowdownText(pk)}{Environment.NewLine}");
                        else if (caughtRegular)
                            EchoUtil.Echo($"{(!NewSCSettings.PingOnMatch.Equals(string.Empty) ? $"<@{NewSCSettings.PingOnMatch}>\n" : "")}Found a shiny, but it's not quite legendary...\nEncounter #{encounterCount}. Adventure #{lairCount}.{Environment.NewLine}{ShowdownParsing.GetShowdownText(pk)}{Environment.NewLine}");
                    }
                }
            }

            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7900E808), MainNsoBase + DamageOutputOffset, token).ConfigureAwait(false);
            if (Settings.LairBall != Ball.None && catchCount < 5 && catchCount != -1)
            {
                Log("Restoring original ball pouch...");
                await Connection.WriteBytesAsync(BallPouch, PokeBallOffset, token).ConfigureAwait(false);
                catchCount = await GetPokeBallCount(token).ConfigureAwait(false);
            }

            if (index == -1)
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                Log("No results found... Going deeper into the lair...");
                return;
            }

            if (index > -1)
            {              
                for (int y = 0; y < index; y++)
                    await Click(DDOWN, 0_250, token).ConfigureAwait(false);

                if (Hub.Config.StopConditions.CaptureVideoClip)
                {
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    await Click(DDOWN, 1_000, token).ConfigureAwait(false);
                    await Click(A, 2_000, token).ConfigureAwait(false);
                    await PressAndHold(CAPTURE, 2_000, 10_000, token).ConfigureAwait(false);
                    await Click(B, 4_000, token).ConfigureAwait(false);
                }

                if (Settings.ResetLegendaryCaughtFlag && ((caught == 1 && index == 0) || (caught == 4 && index == 3)))
                {
                    Log("Resetting Legendary Flag!");
                    await ResetLegendaryFlag(legendSpecies, token).ConfigureAwait(false);
                }
                else if (!Settings.ResetLegendaryCaughtFlag && !StopBot && ((caught == 1 && index == 0) || (caught == 4 && index == 3)))
                    Settings.LairSpecies = LairSpecies.None;
            }
        }

        private async Task<bool> LegendReset(CancellationToken token)
        {
            if (!Settings.UseStopConditionsPathReset)
                return true;

            var originalSetting = NewSCSettings.ShinyTarget;
            NewSCSettings.ShinyTarget = TargetShinyType.DisableOption;
            Log("Reading legendary Pokémon offset...");
            PK8? pk = null;
            while (pk == null)
            {
                await Click(A, 0_500, token).ConfigureAwait(false);
                pk = await ReadUntilPresentAbsolute(await ParsePointer("[[[[[[main+26365B8]+68]+78]+88]+D08]+950]+D0", token).ConfigureAwait(false), 1_000, 0_200, token).ConfigureAwait(false);
            }

            resetCount++;
            TradeExtensions.EncounterLogs(pk);
            Log($"Reset #{resetCount} {Environment.NewLine}{ShowdownParsing.GetShowdownText(pk)}{Environment.NewLine}");
            if (!StopConditionSettings.EncounterFound(pk, DesiredIVs, NewSCSettings))
            {
                Log("No match found, restarting the game...");
                await GameRestart(token).ConfigureAwait(false);
                if (await GetDyniteCount(token).ConfigureAwait(false) < 10)
                {
                    Log("Restoring Dynite Ore...");
                    await SwitchConnection.WriteBytesAsync(OtherItemsPouch, OtherItemAddress, token).ConfigureAwait(false);
                }
                return false;
            }
            Log("Stats match conditions, now let's continue the adventure and check if it's shiny...");
            NewSCSettings.ShinyTarget = originalSetting;
            return true;
        }

        private async Task<int> GetDyniteCount(CancellationToken token)
        {
            OtherItemsPouch = await Connection.ReadBytesAsync(OtherItemAddress, 2184, token).ConfigureAwait(false);
            var pouch = new InventoryPouch8(InventoryType.Items, LairBotUtil.Pouch_Regular_SWSH, 999, 0, 546);
            pouch.GetPouch(OtherItemsPouch);
            return pouch.Items.FirstOrDefault(x => x.Index == 1604).Count;
        }

        private async Task<int> GetPokeBallCount(CancellationToken token)
        {
            BallPouch = await Connection.ReadBytesAsync(PokeBallOffset, 116, token).ConfigureAwait(false);
            var counts = EncounterCount.GetBallCounts(BallPouch);
            return counts.PossibleCatches(Settings.LairBall);
        }

        private async Task SelectCatchingBall(CancellationToken token)
        {
            if (Settings.LairBall == Ball.None)
            {
                for (int i = 0; i < 3; i++)
                    await Click(A, 1_000, token).ConfigureAwait(false);
                return;
            }

            Log($"Selecting {Settings.LairBall} Ball...");
            await Click(A, 1_000, token).ConfigureAwait(false);
            var index = EncounterCount.BallIndex((int)Settings.LairBall);
            var ofs = await ParsePointer("[[[[[[main+2951270]+1D8]+818]+2B0]+2E0]+200]", token).ConfigureAwait(false);
            while (true)
            {
                int ball = BitConverter.ToInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
                if (ball == index)
                    break;
                if (Settings.LairBall.IsApricornBall())
                    await Click(DLEFT, 0_050, token).ConfigureAwait(false);
                else await Click(DRIGHT, 0_050, token).ConfigureAwait(false);
            }
            await Click(A, 1_000, token).ConfigureAwait(false);
        }

        private async Task ResetLegendaryFlag(int species, CancellationToken token)
        {
            if (species == 0)
                return;

            while (!await LairStatusCheck(LairDialogueBytes, CurrentScreenLairDialogue, token).ConfigureAwait(false))
                await Click(A, 0_400, token).ConfigureAwait(false);
            await Connection.WriteBytesAsync(new byte[1], GetFlagOffset(species), token).ConfigureAwait(false);
        }

        private uint GetFlagOffset(int species)
        {
            if (species == 0)
                return 0;

            var index = Array.IndexOf(Enum.GetValues(typeof(LairSpeciesBlock)), Enum.Parse(typeof(LairSpeciesBlock), $"{(Species)species}"));
            return (uint)(ResetLegendFlagOffset + (index * 0x38));
        }

        private async Task<uint> GetPPCount(int move, CancellationToken token) => BitConverter.ToUInt32(await Connection.ReadBytesAsync((uint)(LairMove1Offset + (move * 0xC)), 4, token).ConfigureAwait(false), 0);

        private async Task<bool> LairSeedInjector(CancellationToken token)
        {
            if (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
                await Click(A, 2_000, token).ConfigureAwait(false);
            if (!Settings.InjectSeed || Settings.SeedToInject == string.Empty)
                return true;

            Log("Injecting specified Lair Seed...");
            if (!ulong.TryParse(Settings.SeedToInject, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ulong seedInj))
            {
                Log("Entered seed is invalid, stopping LairBot.");
                return false;
            }
            await Connection.WriteBytesAsync(BitConverter.GetBytes(seedInj), AdventureSeedOffset, token).ConfigureAwait(false);
            return true;
        }

        private async Task SetHuntedPokemon(CancellationToken token)
        {
            byte[] note = await Connection.ReadBytesAsync(LairSpeciesNote1, 2, token).ConfigureAwait(false);
            byte[] wanted = BitConverter.GetBytes((ushort)Settings.LairSpecies);
            if (LairBotUtil.NoteRequest.Count > 0)
            {
                for (int i = 0; i < LairBotUtil.NoteRequest.Count; i ++)
                {
                    var caughtFlag = await Connection.ReadBytesAsync(GetFlagOffset((int)LairBotUtil.NoteRequest[i]), 2, token).ConfigureAwait(false);
                    if (caughtFlag[0] != 0)
                    {
                        Log($"{LairBotUtil.NoteRequest[i]} was already caught prior, skipping!");
                        LairBotUtil.NoteRequest.Remove(LairBotUtil.NoteRequest[i]);
                        --i;
                        continue;
                    }
                    else await Connection.WriteBytesAsync(BitConverter.GetBytes((ushort)LairBotUtil.NoteRequest[i]), i == 0 ? LairSpeciesNote1 : i == 1 ? LairSpeciesNote2 : LairSpeciesNote3, token);
                }
                Log($"Lair Notes set to {string.Join(", ", LairBotUtil.NoteRequest)}!");
                LairBotUtil.NoteRequest = new();
                return;
            }
            else if (!note.SequenceEqual(wanted) && Settings.LairSpecies != LairSpecies.None)
            {
                var caughtFlag = await Connection.ReadBytesAsync(GetFlagOffset((int)Settings.LairSpecies), 2, token).ConfigureAwait(false);
                if (caughtFlag[0] != 0)
                {
                    Log($"{Settings.LairSpecies} was already caught prior, ignoring the request.");
                    Settings.LairSpecies = LairSpecies.None;
                    return;
                }
                await Connection.WriteBytesAsync(wanted, LairSpeciesNote1, token);
                Log($"{Settings.LairSpecies} is ready to be hunted.");
            }
        }

        private async Task MoveAndRentalClicks(int clicks, CancellationToken token)
        {
            for (int i = 0; i < clicks; i++)
                await Click(DDOWN, 0_250, token);
            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);
        }

        private async Task GameRestart(CancellationToken token)
        {
            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token, false, true).ConfigureAwait(false);
        }

        private async Task<bool> SettingsCheck(CancellationToken token)
        {
            NewSCSettings = Hub.Config.StopConditions;
            if (NewSCSettings.ShinyTarget == TargetShinyType.SquareOnly)
                NewSCSettings.ShinyTarget = TargetShinyType.AnyShiny;
            if (NewSCSettings.MarkOnly)
                NewSCSettings.MarkOnly = false;

            if (BallPouch.Length == 1 && Settings.LairBall != Ball.None)
            {
                Log("Checking Poké Ball pouch...");
                catchCount = await GetPokeBallCount(token).ConfigureAwait(false);
                if (catchCount <= 4)
                {
                    Log($"Insufficient {Settings.LairBall} Ball count.");
                    return false;
                }
            }

            if (OtherItemsPouch.Length == 1 && Settings.UseStopConditionsPathReset)
            {
                Log("Checking Dynite Ore count...");
                var dyniteCount = await GetDyniteCount(token).ConfigureAwait(false);
                if (dyniteCount < 10)
                {
                    Log($"{(dyniteCount == 0 ? "No" : $"Only {dyniteCount}")} Dynite Ore found. To be on the safe side, obtain more and restart the bot.");
                    return false;
                }
            }
            return true;
        }

        private async Task CancellationMonitor(CancellationToken oldToken)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            while (!oldToken.IsCancellationRequested)
                await Task.Delay(1_000).ConfigureAwait(false);

            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7900E808), MainNsoBase + DamageOutputOffset, token).ConfigureAwait(false);
            source.Cancel();
        }
    }
}