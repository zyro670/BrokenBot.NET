using System;
using PKHeX.Core;
using SysBot.Base;
using System.Linq;
using System.Threading;
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
        private readonly int[] DesiredMinIVs;
        private readonly int[] DesiredMaxIVs;
        private byte[] OtherItemsPouch = { 0 };
        private byte[] BallPouch = { 0 };
        private ulong MainNsoBase;
        private ulong LairMiscScreenCalc;
        private bool StopBot;
        private bool Lost;
        private int LegendFound = 0;
        private int HackyNoteCheck = -1;
        private int Caught;
        private int OldMoveIndex = 0;
        private int LairEncounterCount;
        private int CatchCount = -1;
        private int ResetCount;
        private readonly LairCount AdventureCounts = new();
        private readonly KeepPathTotals KeepPathCounts = new();
        private readonly LairOffsetValues OffsetValues;
        private PK8 LairBoss = new();
        private PK8 PlayerPk = new();

        sealed private class LairCount
        {
            public double AdventureCount { get; set; }
            public double WinCount { get; set; }
        }

        sealed private class KeepPathTotals
        {
            public int KeepPathAdventures { get; set; }
            public int KeepPathWins { get; set; }
        }

        sealed private class LairOffsetValues
        {
            public ushort LairLobby { get; set; }
            public ushort LairAdventurePath { get; set; }
            public ushort LairDmax { get; set; }
            public ushort LairBattleMenu { get; set; }
            public ushort LairMovesMenu { get; set; }
            public ushort LairCatchScreen { get; set; }
            public ushort LairRewardsScreen { get; set; }
        }

        public LairBot(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg)
        {
            Hub = hub;
            Counts = Hub.Counts;
            Settings = hub.Config.Lair;
            DumpSetting = Hub.Config.Folder;
            OffsetValues = ValueParse();
            StopConditionSettings.InitializeTargetIVs(Hub, out DesiredMinIVs, out DesiredMaxIVs);
        }

        public override async Task MainLoop(CancellationToken token)
        {
            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);
            Config.IterateNextRoutine();
            if (Settings.EnableOHKO)
                _ = Task.Run(async () => await CancellationMonitor(token).ConfigureAwait(false));

            var task = Settings.LairBotMode switch
            {
                LairBotModes.OffsetLog => DoOffsetLog(token),
                _ => DoLairBot(token),
            };
            await task.ConfigureAwait(false);

            await DetachController(token).ConfigureAwait(false);
        }

        private async Task DoLairBot(CancellationToken token)
        {
            int raidCount = 1;
            if (LairBotUtil.MoveRoot.Moves.Count == 0)
                LairBotUtil.MoveRoot = LairBotUtil.LoadMoves();

            Log("It's adventure time! Starting main LairBot loop.");
            while (!token.IsCancellationRequested)
            {
                Lost = false;
                OldMoveIndex = 0;
                LairBotUtil.TerrainDur = -1;

                if (raidCount == 1)
                {
                    MainNsoBase = await SwitchConnection.GetMainNsoBaseAsync(token).ConfigureAwait(false);
                    LairMiscScreenCalc = MainNsoBase + LairMiscScreenOffset;
                    Caught = 0;

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

                    var species = await SetHuntedPokemon(token).ConfigureAwait(false);
                    if (!await LairSeedInjector(token).ConfigureAwait(false))
                        return;

                    ulong seed = BitConverter.ToUInt64(await Connection.ReadBytesAsync(AdventureSeedOffset, 8, token).ConfigureAwait(false), 0);
                    Log($"Here is your current Lair Seed: {seed:X16}");

                    var winRate = AdventureCounts.AdventureCount > 0 ? $" {AdventureCounts.WinCount}/{AdventureCounts.AdventureCount} adventures won so far." : "";
                    Log($"Starting a Solo Adventure for {(species == 0 ? "a random Legendary" : (Species)species)}!{winRate}");
                    await RentalRoutine(species, token).ConfigureAwait(false); // Enter rental selection.
                }

                while (!await LairStatusCheck(OffsetValues.LairAdventurePath, CurrentScreenLairOffset, token).ConfigureAwait(false)) // Delay until in path select screen.
                    await Task.Delay(2_000).ConfigureAwait(false);

                await Task.Delay(raidCount == 1 ? 11_000 : 6_000).ConfigureAwait(false); // Because map scroll is slow and random dialogue is annoying.

                if (Settings.EnableOHKO) // Enable dirty OHKO.
                    await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7900E81F), MainNsoBase + DamageOutputOffset, token).ConfigureAwait(false);

                switch (Hub.Config.Lair.SelectPath) // Choose a path to take.
                {
                    case SelectPath.GoLeft: await Click(A, 1_000, token).ConfigureAwait(false); break;
                    case SelectPath.GoRight: await Click(DRIGHT, 1_000, token).ConfigureAwait(false); break;
                };

                while (!await IsInBattle(token).ConfigureAwait(false)) // Will also deal with possible Scientists and Backpackers.
                {
                    await Click(A, 0_500, token).ConfigureAwait(false);
                    if (LairBoss.Species == 0)
                        LairBoss = await ReadUntilPresentAbsolute(await ParsePointer("[[[[[[main+26365B8]+68]+78]+88]+D08]+950]+D0", token).ConfigureAwait(false), 0_500, 0_200, token).ConfigureAwait(false) ?? new();
                }

                if (raidCount == 1 && Settings.UseStopConditionsPathReset)
                {
                    if (!await LegendReset(token).ConfigureAwait(false))
                        continue;
                }

                var lairPk = await ReadUntilPresent(RaidPokemonOffset, 2_000, 0_200, token).ConfigureAwait(false);
                if (lairPk == null)
                    lairPk = new();

#pragma warning disable CS8601 // Possible null reference assignment.
                var party = new PK8[3] { await ReadUntilPresent(LairPartyP2Offset, 0_500, 0_200, token).ConfigureAwait(false), await ReadUntilPresent(LairPartyP3Offset, 0_500, 0_200, token).ConfigureAwait(false), await ReadUntilPresent(LairPartyP4Offset, 0_500, 0_200, token).ConfigureAwait(false) };
                PlayerPk = await ReadUntilPresent(LairPartyP1Offset, 0_500, 0_200, token).ConfigureAwait(false);
#pragma warning restore CS8601 // Possible null reference assignment.

                LairEncounterCount++;
                Log($"Raid Battle {raidCount}. Encounter {LairEncounterCount}: {SpeciesName.GetSpeciesNameGeneration(lairPk.Species, 2, 8)}{TradeCordHelperUtil.FormOutput(lairPk.Species, lairPk.Form, out _)}.");
                if (PlayerPk == null)
                    PlayerPk = new();

                Log($"Sending out: {SpeciesName.GetSpeciesNameGeneration(PlayerPk.Species, 2, 8)}{TradeCordHelperUtil.FormOutput(PlayerPk.Species, PlayerPk.Form, out _)}.");
                await BattleRoutine(party, lairPk, token).ConfigureAwait(false);

                if (raidCount == 4 || Lost)
                {
                    AdventureCounts.AdventureCount++;
                    if (!Settings.InjectSeed && !Settings.EnableOHKO && !Settings.CatchLairPokémon && Settings.KeepPath)
                        KeepPathCounts.KeepPathAdventures++;
                }

                if (Lost) // We've lost the battle, exit back to main loop.
                {
                    Log($"Lost Adventure {AdventureCounts.AdventureCount}.");
                    if (Caught > 0)
                        await Results(token).ConfigureAwait(false);
                    raidCount = 1;
                    continue;
                }

                await CatchRoutine(raidCount, party, lairPk, token).ConfigureAwait(false);
                if (raidCount == 4) // Final raid complete.
                {
                    if (!Settings.InjectSeed && !Settings.EnableOHKO && !Settings.CatchLairPokémon && Settings.KeepPath)
                        KeepPathCounts.KeepPathWins++;

                    AdventureCounts.WinCount++;
                    Log($"Adventure {AdventureCounts.AdventureCount} completed.");
                    await Results(token).ConfigureAwait(false);
                    raidCount = 1;
                    continue;
                }
                raidCount++;
            }
        }

        private async Task RentalRoutine(int noteSpecies, CancellationToken token)
        {
            uint[] RentalOfsList = { RentalMon1, RentalMon2, RentalMon3 };
            PK8 lairPk = new() { Species = noteSpecies };
            List<int> speedStat = new();
            List<PK8> pkList = new();
            List<double> damage = new();
            int monIndex = -1;

            await LairEntry(token).ConfigureAwait(false);
            for (int i = 0; i < RentalOfsList.Length; i++)
            {
                var pk = await ReadUntilPresent(RentalOfsList[i], 2_000, 0_200, token).ConfigureAwait(false);
                if (pk == null)
                {
                    Log("Entered the lobby too fast, correcting...");
                    while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
                        await Click(B, 0_500, token).ConfigureAwait(false);

                    await LairEntry(token).ConfigureAwait(false);
                    continue;
                }

                pkList.Add(pk);
                int moveIndex = LairBotUtil.PriorityIndex(pk);
                if (Settings.EnableOHKO)
                {
                    if (moveIndex != -1) // Add Ditto override because Imposter is fun?
                    {
                        monIndex = i;
                        break;
                    }
                    else speedStat.Add(LairBotUtil.CalculateEffectiveStat(pk.IV_SPE, pk.EV_SPE, pk.PersonalInfo.SPE, pk.CurrentLevel));
                }
                else damage.Add(LairBotUtil.WeightedDamage(new PK8[] { new() }, pk, lairPk, false).Max());
            }

            if (!Settings.EnableOHKO)
                monIndex = damage.IndexOf(damage.Max());

            var speedIndex = speedStat.Count > 0 ? speedStat.IndexOf(speedStat.Max()) : 0;
            var selection = pkList[monIndex == -1 ? speedIndex : monIndex];
            Log($"Selecting {SpeciesName.GetSpeciesNameGeneration(selection.Species, 2, 8)}{TradeCordHelperUtil.FormOutput(selection.Species, selection.Form, out _)}.");
            await MoveAndRentalClicks(monIndex == -1 ? speedIndex : monIndex, token).ConfigureAwait(false);
        }

        private async Task<LairBotUtil.MoveInfo> SelectMove(PK8[] party, PK8 lairMon, bool stuck, int turn, bool dmax, bool dmaxEnded, CancellationToken token)
        {
            int[] movePP = new int[] { PlayerPk.Move1_PP, PlayerPk.Move2_PP, PlayerPk.Move3_PP, PlayerPk.Move4_PP };
            var dmgWeight = LairBotUtil.WeightedDamage(party, PlayerPk, lairMon, dmax).ToList();
            var priorityMove = PlayerPk.Moves.ToList().IndexOf(PlayerPk.Moves.Intersect((IEnumerable<int>)Enum.GetValues(typeof(PriorityMoves))).FirstOrDefault());
            bool priority = Settings.EnableOHKO && priorityMove != -1 && dmgWeight[priorityMove] > 0 && lairMon.Ability != (int)Ability.PsychicSurge && lairMon.Ability != (int)Ability.QueenlyMajesty && lairMon.Ability != (int)Ability.Dazzling;

            var bestMove = dmgWeight.IndexOf(dmgWeight.Max());
            bool movePass = false;
            while (!movePass)
            {
                var move = LairBotUtil.MoveRoot.Moves.FirstOrDefault(x => x.MoveID == PlayerPk.Moves[priority ? priorityMove : bestMove]);
                bool recoil = move.Recoil >= 206 && move.EffectSequence >= 48;
                if ((stuck && (OldMoveIndex == (priority ? priorityMove : bestMove))) || (Settings.EnableOHKO && (recoil || move.Charge)) || move.MoveID == (int)Move.Belch)
                {
                    dmgWeight[priority ? priorityMove : bestMove] = 0.0;
                    bestMove = dmgWeight.IndexOf(dmgWeight.Max());
                    priority = false;
                    stuck = false;
                    continue;
                }
                else if (priority)
                    bestMove = priorityMove;
                movePass = true;
            }

            var finalMove = LairBotUtil.MoveRoot.Moves.FirstOrDefault(x => x.MoveID == PlayerPk.Moves[bestMove]);
            int dmaxMove = finalMove.Category != MoveCategory.Status ? (int)finalMove.Type : 18;
            Log($"Turn {turn}: Selecting {(dmax ? (DmaxMoves)dmaxMove : (Move)PlayerPk.Moves[bestMove])}.");
            var index = bestMove - OldMoveIndex;
            if (dmaxEnded)
                index = bestMove;
            else if (index < 0)
                index = index + OldMoveIndex + 1;

            await MoveAndRentalClicks(index, token).ConfigureAwait(false);
            OldMoveIndex = bestMove;
            return finalMove;
        }

        private bool CheckIfUpgrade(PK8[] party, PK8 lairPk)
        {
            bool upgrade = false;
            var dmgWeightPlayer = LairBotUtil.WeightedDamage(party, PlayerPk.Species == 132 ? LairBoss : PlayerPk, LairBoss, false);
            var dmgWeightLair = LairBotUtil.WeightedDamage(new PK8[] { new() }, lairPk, LairBoss, false);

            if (Settings.EnableOHKO)
            {
                var ourSpeed = LairBotUtil.CalculateEffectiveStat(PlayerPk.IV_SPE, PlayerPk.EV_SPE, PlayerPk.PersonalInfo.SPE, PlayerPk.CurrentLevel);
                bool noPriority = LairBotUtil.PriorityIndex(PlayerPk) == -1;
                var lairPkSpeed = LairBotUtil.CalculateEffectiveStat(lairPk.IV_SPE, lairPk.EV_SPE, lairPk.PersonalInfo.SPE, lairPk.CurrentLevel);
                bool lairPkPriority = LairBotUtil.PriorityIndex(lairPk) != -1;

                var maxDmgMoveIndex = dmgWeightPlayer.ToList().IndexOf(dmgWeightPlayer.Max());
                var move = LairBotUtil.MoveRoot.Moves.FirstOrDefault(x => x.MoveID == PlayerPk.Moves[maxDmgMoveIndex]);

                if (move.Charge || move.MoveID == (int)Move.Belch)
                {
                    dmgWeightPlayer[maxDmgMoveIndex] = 0.0;
                    if (!dmgWeightPlayer.Any(x => x > 0.0))
                        return false;
                }

                if ((noPriority && (lairPkSpeed > ourSpeed)) || (lairPkPriority && noPriority))
                    upgrade = true;
            }
            else if (dmgWeightLair.Max() > dmgWeightPlayer.Max())
                upgrade = true;

            if (upgrade && lairPk.Species != LairBoss.Species)
                Log("Lair encounter is better than our current Pokémon. Going to catch it and swap our current Pokémon.");

            return upgrade;
        }

        private async Task BattleRoutine(PK8[] party, PK8 lairPk, CancellationToken token)
        {
            int turn = 0;
            int dmaxEnd = 0;
            bool stuck = false;
            bool fainted = false;
            bool dmax = false;
            bool canDmax = false;
            LairBotUtil.MoveInfo move = new();

            while (true)
            {
                while (!await LairStatusCheckMain(OffsetValues.LairMovesMenu, LairMiscScreenCalc, token).ConfigureAwait(false))
                {
                    if (await LairStatusCheckMain(OffsetValues.LairBattleMenu, LairMiscScreenCalc, token).ConfigureAwait(false))
                    {
                        turn++;
                        await Click(A, 1_000, token).ConfigureAwait(false);
                        if (!await LairStatusCheckMain(OffsetValues.LairMovesMenu, LairMiscScreenCalc, token).ConfigureAwait(false) && !fainted)
                        {
                            Log($"Turn {turn}: Cheering on...");
                            fainted = true;
                            canDmax = false;
                            dmaxEnd = 0;
                        }
                    }
                    else
                    {
                        Lost = await LairStatusCheck(Caught > 0 ? OffsetValues.LairRewardsScreen : OffsetValues.LairLobby, CurrentScreenLairOffset, token).ConfigureAwait(false) || !await IsInBattle(token).ConfigureAwait(false);
                        if (await LairStatusCheckMain(OffsetValues.LairCatchScreen, LairMiscScreenCalc, token).ConfigureAwait(false) || Lost)
                            return;

                        if (!Settings.EnableOHKO && !canDmax && await LairStatusCheckMain(OffsetValues.LairDmax, LairMiscScreenCalc, token).ConfigureAwait(false))
                        {
                            await Task.Delay(2_000, token).ConfigureAwait(false);
                            if (await LairStatusCheckMain(OffsetValues.LairBattleMenu, LairMiscScreenCalc, token).ConfigureAwait(false))
                                canDmax = true;
                        }

                        await Click(B, 0_300, token).ConfigureAwait(false);
                    }
                }

                fainted = false;
                if (stuck)
                {
                    Log($"{(dmax ? (DmaxMoves)move.Type : (Move)PlayerPk.Moves[OldMoveIndex])} cannot be executed, trying to select a different move.");
                    for (int i = 0; i < 2; i++)
                        await Click(B, 1_000, token).ConfigureAwait(false);
                    await Click(A, 1_000, token).ConfigureAwait(false);
                }

                var newPlayerPk = await ReadUntilPresent(LairPartyP1Offset, 2_000, 0_200, token).ConfigureAwait(false);
                var newLairPk = await ReadUntilPresent(RaidPokemonOffset, 2_000, 0_200, token).ConfigureAwait(false);
                if (newPlayerPk != null && newLairPk != null)
                {
                    PlayerPk = newPlayerPk.Species == 132 ? newLairPk : newPlayerPk;
                    lairPk = newLairPk.Species == 132 ? newPlayerPk : newLairPk;
                    if (newPlayerPk.Species == 132)
                    {
                        PlayerPk.Move1_PP = await GetPPCount(0, token).ConfigureAwait(false);
                        PlayerPk.Move2_PP = await GetPPCount(1, token).ConfigureAwait(false);
                        PlayerPk.Move3_PP = await GetPPCount(2, token).ConfigureAwait(false);
                        PlayerPk.Move4_PP = await GetPPCount(3, token).ConfigureAwait(false);
                    }
                }

                bool dmaxEnded = dmax && dmaxEnd == 0;
                if (dmaxEnded)
                    dmax = false;

                if (!Settings.EnableOHKO && !dmax && canDmax)
                {
                    await Click(DLEFT, 0_400, token).ConfigureAwait(false);
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    Log(PlayerPk.CanGigantamax ? "Gigantamaxing..." : "Dynamaxing...");
                    dmax = true;
                    canDmax = false;
                    dmaxEnd = 3;
                }

                move = await SelectMove(party, lairPk, stuck, turn, dmax, dmaxEnded, token).ConfigureAwait(false);
                await Click(B, 1_000, token).ConfigureAwait(false);
                await Click(B, 1_000, token).ConfigureAwait(false);
                await Click(A, 1_000, token).ConfigureAwait(false);

                if (await LairStatusCheckMain(OffsetValues.LairMovesMenu, LairMiscScreenCalc, token).ConfigureAwait(false))
                    stuck = true;
                else
                {
                    stuck = false;
                    if (dmax)
                        dmaxEnd--;
                }
            }
        }

        private async Task CatchRoutine(int raidCount, PK8[] party, PK8 lairPk, CancellationToken token)
        {
            bool upgrade = false;
            if (Settings.UpgradePokemon && raidCount != 4)
                upgrade = CheckIfUpgrade(party, lairPk);

            await Task.Delay(6_000).ConfigureAwait(false);
            if (Settings.CatchLairPokémon || upgrade || raidCount == 4) // We want to catch the legendary regardless of settings for catching.
            {
                await SelectCatchingBall(token).ConfigureAwait(false); // Select ball to catch with.
                Log($"Catching {(raidCount < 4 ? "encounter" : "legendary")}...");
                await Task.Delay(raidCount == 4 ? 35_000 : 25_000).ConfigureAwait(false);
                if (raidCount < 4)
                {
                    if (!upgrade)
                        await Click(DDOWN, 1_000, token).ConfigureAwait(false);
                    await Click(A, 1_000, token).ConfigureAwait(false);
                }
                CatchCount--;
                Caught++;
            }
            else
            {
                await Click(DDOWN, 1_000, token).ConfigureAwait(false);
                await Click(A, 1_000, token).ConfigureAwait(false);
            }

            Log($"{(raidCount == 4 || Settings.CatchLairPokémon || upgrade ? "Caught" : "Defeated")} {SpeciesName.GetSpeciesNameGeneration(lairPk.Species, 2, 8)}{TradeCordHelperUtil.FormOutput(lairPk.Species, lairPk.Form, out _)}.");
        }

        private async Task Results(CancellationToken token)
        {
            Counts.AddCompletedAdventures();
            int index = -1;

            while (!await LairStatusCheck(OffsetValues.LairRewardsScreen, CurrentScreenLairOffset, token).ConfigureAwait(false))
                await Task.Delay(4_000).ConfigureAwait(false);

            for (int i = 0; i < Caught; i++)
            {
                var jumpAdj = i == 0 ? 0 : i == 1 ? 2 : i == 2 ? 10 : 12;
                var pointer = $"[[[[[main+28F4060]+1B0]+68]+{58 + jumpAdj}]+58]";
                var pk = await ReadUntilPresentAbsolute(await ParsePointer(pointer, token), 2_000, 0_200, token).ConfigureAwait(false);

                if (pk != null)
                {
                    if (pk.IsShiny)
                        index = Settings.CatchLairPokémon ? i : Caught - (Caught - i);

                    bool caughtLegend = !Lost && (Caught - 1 == index);
                    if (caughtLegend)
                    {
                        HackyNoteCheck = -1;
                        LegendFound = pk.Species;
                    }

                    bool caughtRegular = !caughtLegend && pk.IsShiny;
                    if (caughtLegend && (Settings.UseStopConditionsPathReset && StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, NewSCSettings) || Settings.StopOnLegendary))
                        StopBot = true;

                    TradeExtensions.EncounterLogs(pk);
                    if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
                        DumpPokemon(DumpSetting.DumpFolder, "lairs", pk);

                    if (Settings.AlwaysOutputShowdown)
                        Log($"Adventure {AdventureCounts.AdventureCount}.{Environment.NewLine}{ShowdownParsing.GetShowdownText(pk)}{Environment.NewLine}");

                    if (LairBotUtil.EmbedsInitialized && Settings.ResultsEmbedChannels != string.Empty && (caughtLegend || caughtRegular))
                        LairBotUtil.EmbedMon = (pk, caughtLegend);
                    else
                    {
                        if (caughtLegend)
                            EchoUtil.Echo($"{(!NewSCSettings.PingOnMatch.Equals(string.Empty) ? $"<@{NewSCSettings.PingOnMatch}>\n" : "")}Shiny Legendary found!\nEncounter {LairEncounterCount}. Adventure {AdventureCounts.AdventureCount}.{Environment.NewLine}{ShowdownParsing.GetShowdownText(pk)}{Environment.NewLine}");
                        else if (caughtRegular)
                            EchoUtil.Echo($"{(!NewSCSettings.PingOnMatch.Equals(string.Empty) ? $"<@{NewSCSettings.PingOnMatch}>\n" : "")}Found a shiny, but it's not quite legendary...\nEncounter {LairEncounterCount}. Adventure {AdventureCounts.AdventureCount}.{Environment.NewLine}{ShowdownParsing.GetShowdownText(pk)}{Environment.NewLine}");
                    }
                }
            }

            if (Settings.EnableOHKO)
                await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7900E808), MainNsoBase + DamageOutputOffset, token).ConfigureAwait(false);

            if (!Settings.InjectSeed && !Settings.EnableOHKO && Settings.KeepPath && !Settings.CatchLairPokémon && LegendFound == 0)
            {
                double winRate = KeepPathCounts.KeepPathWins / KeepPathCounts.KeepPathAdventures;
                if (KeepPathCounts.KeepPathAdventures < 5 || (KeepPathCounts.KeepPathAdventures >= 5 && winRate >= 0.3))
                {
                    Log($"{(Lost ? "" : "No shiny legendary found. ")}Resetting the game to keep the seed.");
                    await GameRestart(token).ConfigureAwait(false);
                    if (await GetDyniteCount(token).ConfigureAwait(false) < 10)
                    {
                        Log("Restoring Dynite Ore...");
                        await SwitchConnection.WriteBytesAsync(OtherItemsPouch, OtherItemAddress, token).ConfigureAwait(false);
                    }
                }
                else if (KeepPathCounts.KeepPathAdventures >= 5 && winRate < 0.3)
                {
                    KeepPathCounts.KeepPathWins = 0;
                    KeepPathCounts.KeepPathAdventures = 0;
                    await Click(B, 1_000, token).ConfigureAwait(false);
                    Log("Our win ratio isn't looking too good... Rolling our path.");
                }
                return;
            }

            if (index == -1)
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                Log("No results found... Going deeper into the lair...");
                return;
            }

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
        }

        private async Task<bool> LegendReset(CancellationToken token)
        {
            ResetCount++;
            var originalSetting = NewSCSettings.ShinyTarget;
            NewSCSettings.ShinyTarget = TargetShinyType.DisableOption;
            Log("Reading legendary Pokémon offset...");
            TradeExtensions.EncounterLogs(LairBoss);
            Log($"Reset {ResetCount} {Environment.NewLine}{ShowdownParsing.GetShowdownText(LairBoss)}{Environment.NewLine}");

            if (!StopConditionSettings.EncounterFound(LairBoss, DesiredMinIVs, DesiredMaxIVs, NewSCSettings))
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
            return counts.PossibleCatches((Ball)Settings.LairBall);
        }

        private async Task SelectCatchingBall(CancellationToken token)
        {
            Log($"Selecting {Settings.LairBall} Ball...");
            await Click(A, 0_500, token).ConfigureAwait(false);
            var lairBall = (Ball)Settings.LairBall;
            var index = EncounterCount.BallIndex((int)Settings.LairBall);
            var ofs = await ParsePointer("[[[[[[main+2951270]+1D8]+818]+2B0]+2E0]+200]", token).ConfigureAwait(false);
            while (true)
            {
                int ball = BitConverter.ToInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
                if (ball == index)
                    break;
                if (lairBall.IsApricornBall())
                    await Click(DLEFT, 0_050, token).ConfigureAwait(false);
                else await Click(DRIGHT, 0_050, token).ConfigureAwait(false);
            }
            await Click(A, 0_500, token).ConfigureAwait(false);
        }

        private async Task ResetLegendaryFlag(int species, CancellationToken token)
        {
            if (species == 0)
                return;

            await Connection.WriteBytesAsync(new byte[1], GetFlagOffset(species), token).ConfigureAwait(false);
        }

        private uint GetFlagOffset(int species)
        {
            if (species == 0)
                return 0;

            var index = Array.IndexOf(Enum.GetValues(typeof(LairSpeciesBlock)), Enum.Parse(typeof(LairSpeciesBlock), $"{(Species)species}"));
            return (uint)(ResetLegendFlagOffset + (index * 0x38));
        }

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

        private async Task<int> SetHuntedPokemon(CancellationToken token)
        {
            if (HackyNoteCheck == -1 || LairBotUtil.DiscordQueueOverride)
            {
                for (int i = 0; i < 4; i++) // First note shifts due to yet unknown reasons, just clear possible slots, check which note to use on startup and after catching a legendary.
                    await Connection.WriteBytesAsync(new byte[] { 0 }, i == 0 ? LairSpeciesNote1 : i == 1 ? LairSpeciesNote2 : i == 2 ? LairSpeciesNote3 : LairSpeciesNote4, token);

                var control = BitConverter.GetBytes((ushort)LairSpecies.Moltres);
                await Connection.WriteBytesAsync(control, LairSpeciesNote3, token).ConfigureAwait(false);
                await Click(A, 0_250, token).ConfigureAwait(false);

                var note1 = BitConverter.ToUInt16(await Connection.ReadBytesAsync(LairSpeciesNote1, 2, token).ConfigureAwait(false), 0);
                for (int i = 0; i < 3; i++)
                    await Click(B, 0_250, token).ConfigureAwait(false);

                bool firstNoteIsFirst = note1 == (ushort)LairSpecies.Moltres;
                await Connection.WriteBytesAsync(new byte[] { 0 }, firstNoteIsFirst ? LairSpeciesNote1 : LairSpeciesNote2, token).ConfigureAwait(false);
                HackyNoteCheck = firstNoteIsFirst ? 1 : 2;
            }

            if (LegendFound != 0)
            {
                LegendFound = 0;
                if (Settings.ResetLegendaryCaughtFlag)
                    await ResetLegendaryFlag(LegendFound, token).ConfigureAwait(false);

                if (Settings.LairSpeciesQueue[0] != LairSpecies.None)
                {
                    Settings.LairSpeciesQueue[0] = Settings.LairSpeciesQueue[1];
                    Settings.LairSpeciesQueue[1] = Settings.LairSpeciesQueue[2];
                    Settings.LairSpeciesQueue[2] = LairSpecies.None;
                }
            }

            var note = BitConverter.ToUInt16(await Connection.ReadBytesAsync(HackyNoteCheck == 1 ? LairSpeciesNote1 : LairSpeciesNote2, 2, token).ConfigureAwait(false), 0);
            if (LairBotUtil.DiscordQueueOverride || ((ushort)Settings.LairSpeciesQueue[0] != note))
            {
                LairBotUtil.DiscordQueueOverride = false;
                for (int i = 0; i < Settings.LairSpeciesQueue.Length; i++)
                {
                    var caughtFlag = await Connection.ReadBytesAsync(GetFlagOffset((int)Settings.LairSpeciesQueue[i]), 2, token).ConfigureAwait(false);
                    if (caughtFlag[0] != 0 && !Settings.ResetLegendaryCaughtFlag)
                    {
                        Log($"{(int)Settings.LairSpeciesQueue[i]} was caught prior and \"ResetLegendaryCaughtFlag\" is disabled. Skipping this note.");
                        continue;
                    }
                    else await ResetLegendaryFlag((int)Settings.LairSpeciesQueue[i], token).ConfigureAwait(false);

                    if (HackyNoteCheck == 1)
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((ushort)Settings.LairSpeciesQueue[i]), i == 0 ? LairSpeciesNote1 : i == 1 ? LairSpeciesNote2 : LairSpeciesNote3, token);
                    else await Connection.WriteBytesAsync(BitConverter.GetBytes((ushort)Settings.LairSpeciesQueue[i]), i == 0 ? LairSpeciesNote2 : i == 1 ? LairSpeciesNote3 : LairSpeciesNote4, token);
                }

                Log($"Lair Notes set to {string.Join(", ", Settings.LairSpeciesQueue)}!");
                return (int)Settings.LairSpeciesQueue[0];
            }
            else return note;
        }

        private async Task MoveAndRentalClicks(int clicks, CancellationToken token)
        {
            for (int i = 0; i < clicks; i++)
                await Click(DDOWN, 0_300, token);

            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);
        }

        private async Task GameRestart(CancellationToken token)
        {
            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token, false, true).ConfigureAwait(false);
        }

        private async Task LairEntry(CancellationToken token)
        {
            var ofsVal = BitConverter.ToUInt16(await Connection.ReadBytesAsync(CurrentScreenLairOffset, 2, token).ConfigureAwait(false), 0);
            while (await LairStatusCheck(ofsVal, CurrentScreenLairOffset, token).ConfigureAwait(false))
                await Click(A, 0_300, token).ConfigureAwait(false);

            await Task.Delay(2_000).ConfigureAwait(false);
            await Click(DDOWN, 0_250, token).ConfigureAwait(false);
            await Click(A, 2_000, token).ConfigureAwait(false);
        }

        private async Task<bool> SettingsCheck(CancellationToken token)
        {
            NewSCSettings = Hub.Config.StopConditions;
            if (NewSCSettings.ShinyTarget == TargetShinyType.SquareOnly)
                NewSCSettings.ShinyTarget = TargetShinyType.AnyShiny;
            if (NewSCSettings.MarkOnly)
                NewSCSettings.MarkOnly = false;

            if (BallPouch.Length == 1)
            {
                Log("Checking Poké Ball Pouch...");
                CatchCount = await GetPokeBallCount(token).ConfigureAwait(false);
                if (CatchCount < 5)
                {
                    Log($"Insufficient {Settings.LairBall} Ball count.");
                    return false;
                }
            }
            else if (CatchCount < 5)
            {
                Log("Restoring original Ball Pouch...");
                await Connection.WriteBytesAsync(BallPouch, PokeBallOffset, token).ConfigureAwait(false);
                CatchCount = await GetPokeBallCount(token).ConfigureAwait(false);
            }

            if (OtherItemsPouch.Length == 1 && (Settings.UseStopConditionsPathReset || Settings.KeepPath))
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

        private async Task DoOffsetLog(CancellationToken token)
        {
            MainNsoBase = await SwitchConnection.GetMainNsoBaseAsync(token).ConfigureAwait(false);
            LairMiscScreenCalc = MainNsoBase + LairMiscScreenOffset;

            if (Settings.EnableOHKO) // Enable dirty OHKO.
                await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7900E81F), MainNsoBase + DamageOutputOffset, token).ConfigureAwait(false);

            string instructions =
                "\n\n1. LairLobbyValue (CurrentScreen): screen where you select \"Don't invite others\"." +
                "\n2. LairAdventurePathValue (CurrentScreen): screen where you choose a path." +
                "\n3. LairDmaxValue (MiscScreen): during the first battle, when your wristband glows." +
                "\n4. LairBattleMenuValue (MiscScreen): main in-battle screen." +
                "\n5. LairMovesMenuValue (MiscScreen): move selection screen." +
                "\n6. LairCatchScreenValue (MiscScreen): screen where it says \"Catch\" and \"Don't catch\"." +
                "\n7. LairRewardsScreenValue (CurrentScreen): screen at the end of an adventure where you can select which caught Pokémon to bring home.\n\n";

            Log($"Starting main OffsetLog loop. Please progress through an adventure while paying close attention to value changes.{instructions}");
            while (!token.IsCancellationRequested)
            {
                var valCur = BitConverter.ToUInt16(await Connection.ReadBytesAsync(CurrentScreenLairOffset, 2, token).ConfigureAwait(false), 0);
                var valMisc = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(LairMiscScreenCalc, 2, token).ConfigureAwait(false), 0);
                var hexCur = string.Format("0x{0:X8}", valCur);
                var hexMisc = string.Format("0x{0:X8}", valMisc);
                Log($"\nCurrentScreen offset value: {hexCur}\nMiscScreen offset value: {hexMisc}");
                await Task.Delay(2_000).ConfigureAwait(false);
            }

            if (Settings.EnableOHKO)
                await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7900E808), MainNsoBase + DamageOutputOffset, token).ConfigureAwait(false);
        }

        private async Task<int> GetPPCount(int move, CancellationToken token) => BitConverter.ToInt32(await Connection.ReadBytesAsync((uint)(LairMove1Offset + (move * 0xC)), 4, token).ConfigureAwait(false), 0);

        private LairOffsetValues ValueParse()
        {
            ushort.TryParse(Settings.LairScreenValues.LairLobbyValue.Replace("0x", ""), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out ushort lobby);
            ushort.TryParse(Settings.LairScreenValues.LairAdventurePathValue.Replace("0x", ""), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out ushort path);
            ushort.TryParse(Settings.LairScreenValues.LairDmaxValue.Replace("0x", ""), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out ushort dmax);
            ushort.TryParse(Settings.LairScreenValues.LairBattleMenuValue.Replace("0x", ""), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out ushort battle);
            ushort.TryParse(Settings.LairScreenValues.LairMovesMenuValue.Replace("0x", ""), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out ushort moves);
            ushort.TryParse(Settings.LairScreenValues.LairCatchScreenValue.Replace("0x", ""), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out ushort catchScreen);
            ushort.TryParse(Settings.LairScreenValues.LairRewardsScreenValue.Replace("0x", ""), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out ushort rewards);
            return new()
            {
                LairLobby = lobby,
                LairAdventurePath = path,
                LairDmax = dmax,
                LairBattleMenu = battle,
                LairMovesMenu = moves,
                LairCatchScreen = catchScreen,
                LairRewardsScreen = rewards,
            };
        }
    }
}