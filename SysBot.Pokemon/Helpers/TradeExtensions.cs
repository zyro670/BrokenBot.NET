using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using Newtonsoft.Json;

namespace SysBot.Pokemon
{
    public class TradeExtensions
    {
        public static Dictionary<ulong, List<DateTime>> UserCommandTimestamps = new();
        private static readonly string InfoBackupPath = "TradeCord\\UserInfo_backup.json";
        private static readonly string InfoPath = "TradeCord\\UserInfo.json";
        public static Dictionary<ulong, DateTime> TradeCordCooldown = new();
        public static HashSet<ulong> MuteList = new();
        public static Dictionary<ulong, string> TradeCordPath = new();
        public static DateTime EventVoteTimer = new();
        private static TCUserInfoRoot UserInfo = new();
        private static readonly object _sync = new();
        private static readonly object _syncLog = new();
        public static readonly Random Random = new();
        private static DateTime ConfigTimer = DateTime.Now;
        private static bool TCRWLockEnable;
        public static bool TCInitialized;

        public static int XCoordStart = 0;
        public static int YCoordStart = 0;

        private static readonly int[] GalarFossils = { 880, 881, 882, 883 };
        public static readonly int[] Pokeball = { 151, 722, 723, 724, 725, 726, 727, 728, 729, 730, 772, 773, 789, 790, 810, 811, 812, 813, 814, 815, 816, 817, 818, 891, 892 };
        public static readonly int[] Amped = { 3, 4, 2, 8, 9, 19, 22, 11, 13, 14, 0, 6, 24 };
        public static readonly int[] LowKey = { 1, 5, 7, 10, 12, 15, 16, 17, 18, 20, 21, 23 };

        public static readonly string[] Characteristics =
        {
            "Takes plenty of siestas",
            "Likes to thrash about",
            "Capable of taking hits",
            "Alert to sounds",
            "Mischievous",
            "Somewhat vain",
        };

        public class TC_CommandContext
        {
            public string Username { get; set; } = string.Empty;
            public ulong ID { get; set; }
            public string GifteeName { get; set; } = string.Empty;
            public ulong GifteeID { get; set; }
            public TCCommandContext Context { get; set; }
        }

        public class TCRng
        {
            public int CatchRNG { get; set; }
            public int ShinyRNG { get; set; }
            public int EggRNG { get; set; }
            public int EggShinyRNG { get; set; }
            public int GmaxRNG { get; set; }
            public int CherishRNG { get; set; }
            public int SpeciesRNG { get; set; }
            public int SpeciesBoostRNG { get; set; }
            public int ItemRNG { get; set; }
        }

        public class TCUserInfoRoot
        {
            public HashSet<TCUserInfo> Users { get; set; } = new();

            public class TCUserInfo
            {
                public string Username { get; set; } = string.Empty;
                public ulong UserID { get; set; }
                public int TimeZoneOffset { get; set; }
                public int CatchCount { get; set; }
                public int SpeciesBoost { get; set; }
                public int DexCompletionCount { get; set; }
                public HashSet<int> Dex { get; set; } = new();
                public List<DexPerks> ActivePerks { get; set; } = new();
                public HashSet<int> Favorites { get; set; } = new();
                public string OTName { get; set; } = "";
                public string OTGender { get; set; } = "";
                public int TID { get; set; }
                public int SID { get; set; }
                public string Language { get; set; } = "";
                public Daycare1 Daycare1 { get; set; } = new();
                public Daycare2 Daycare2 { get; set; } = new();
                public HashSet<Catch> Catches { get; set; } = new();
                public Buddy Buddy { get; set; } = new();
                public HashSet<Items> Items { get; set; } = new();
            }

            public class Catch
            {
                public bool Shiny { get; set; }
                public int ID { get; set; }
                public string Ball { get; set; } = "None";
                public string Species { get; set; } = "None";
                public string Form { get; set; } = "";
                public bool Egg { get; set; }
                public string Path { get; set; } = "";
                public bool Traded { get; set; }
            }

            public class Daycare1
            {
                public bool Shiny { get; set; }
                public int ID { get; set; }
                public int Species { get; set; }
                public string Form { get; set; } = "";
                public int Ball { get; set; }
            }

            public class Daycare2
            {
                public bool Shiny { get; set; }
                public int ID { get; set; }
                public int Species { get; set; }
                public string Form { get; set; } = "";
                public int Ball { get; set; }
            }

            public class Buddy
            {
                public int ID { get; set; }
                public Ability Ability { get; set; }
                public string Nickname { get; set; } = "";
            }

            public class Items
            {
                public TCItems Item { get; set; }
                public int ItemCount { get; set; }
            }
        }

        public static T EnumParse<T>(string input) where T : struct, Enum => !Enum.TryParse(input, true, out T result) ? new() : result;

        public static bool SelfBotScanner(ulong id, int cd)
        {
            if (UserCommandTimestamps.TryGetValue(id, out List<DateTime> timeStamps))
            {
                int[] delta = new int[timeStamps.Count - 1];
                bool[] comp = new bool[delta.Length - 1];

                for (int i = 1; i < timeStamps.Count; i++)
                    delta[i - 1] = (int)(timeStamps[i].Subtract(timeStamps[i - 1]).TotalSeconds - cd);

                for (int i = 1; i < delta.Length; i++)
                    comp[i - 1] = delta[i] == delta[i - 1] || delta[i] - delta[i - 1] == -2 || delta[i] - delta[i - 1] == -1 || delta[i] - delta[i - 1] == 0 || delta[i] - delta[i - 1] == 1 || delta[i] - delta[i - 1] == 2;

                UserCommandTimestamps[id].Clear();
                if (comp.Any(x => x == false))
                    return false;
                else return true;
            }
            return false;
        }

        public static PK8 RngRoutine(PKM pkm, IBattleTemplate template, Shiny shiny)
        {
            if (pkm.Species == (int)Species.Alcremie)
            {
                var data = pkm.Data;
                var deco = (uint)Random.Next(7);
                pkm.ChangeFormArgument(deco);
            }

            var laInit = new LegalityAnalysis(pkm);
            var nature = pkm.Nature;
            pkm.Nature = pkm.Species switch
            {
                (int)Species.Toxtricity => pkm.Form > 0 ? LowKey[Random.Next(LowKey.Length)] : Amped[Random.Next(Amped.Length)],
                _ => Random.Next(25),
            };

            var la = new LegalityAnalysis(pkm);
            if (laInit.Valid && !la.Valid)
                pkm.Nature = nature;

            pkm.StatNature = pkm.Nature;
            pkm.Move1_PPUps = pkm.Move2_PPUps = pkm.Move3_PPUps = pkm.Move4_PPUps = 0;
            pkm.SetMaximumPPCurrent(pkm.Moves);
            pkm.ClearHyperTraining();

            var enc = la.Info.EncounterMatch;
            var evoChain = la.Info.EvoChainsAllGens[pkm.Format].FirstOrDefault(x => x.Species == pkm.Species);
            pkm.CurrentLevel = enc.LevelMin < evoChain.MinLevel ? evoChain.MinLevel : enc.LevelMin;
            while (!new LegalityAnalysis(pkm).Valid)
            {
                pkm.CurrentLevel += 1;
                if (pkm.CurrentLevel == 100 && !new LegalityAnalysis(pkm).Valid)
                    return (PK8)pkm;
            }

            pkm.SetSuggestedMoves();
            pkm.SetRelearnMoves(pkm.GetSuggestedRelearnMoves(enc));
            if (pkm.Species == 647 && pkm.Form > 0 && !pkm.Moves.Contains(548))
                pkm.Move1 = (int)Move.SecretSword;
            pkm.HealPP();

            var legends = (int[])Enum.GetValues(typeof(Legends));
            if (!GalarFossils.Contains(pkm.Species) && !pkm.FatefulEncounter)
            {
                if (enc is EncounterSlot8 slot8)
                    pkm.SetAbilityIndex(slot8.Ability == -1 ? Random.Next(3) : slot8.Ability == 0 ? Random.Next(2) : slot8.Ability == 1 ? 0 : slot8.Ability == 2 ? 1 : 2);
                else if (enc is EncounterStatic8 static8)
                    pkm.SetAbilityIndex(static8.Ability == -1 ? Random.Next(3) : static8.Ability == 0 ? Random.Next(2) : static8.Ability == 1 ? 0 : static8.Ability == 2 ? 1 : 2);
                else if (enc is EncounterStatic8N static8N)
                    pkm.SetAbilityIndex(static8N.Ability == -1 ? Random.Next(3) : static8N.Ability == 0 ? Random.Next(2) : static8N.Ability == 1 ? 0 : static8N.Ability == 2 ? 1 : 2);
                else if (enc is EncounterStatic8NC static8NC)
                    pkm.SetAbilityIndex(static8NC.Ability == -1 ? Random.Next(3) : static8NC.Ability == 0 ? Random.Next(2) : static8NC.Ability == 1 ? 0 : static8NC.Ability == 2 ? 1 : 2);
                else if (enc is EncounterStatic8ND static8ND)
                    pkm.SetAbilityIndex(static8ND.Ability == -1 ? Random.Next(3) : static8ND.Ability == 0 ? Random.Next(2) : static8ND.Ability == 1 ? 0 : static8ND.Ability == 2 ? 1 : 2);
                else if (enc is EncounterStatic8U static8U)
                    pkm.SetAbilityIndex(static8U.Ability == -1 ? Random.Next(3) : static8U.Ability == 0 ? Random.Next(2) : static8U.Ability == 1 ? 0 : static8U.Ability == 2 ? 1 : 2);
            }

            bool goMew = pkm.Species == (int)Species.Mew && enc.Version == GameVersion.GO && pkm.IsShiny;
            bool goOther = (pkm.Species == (int)Species.Victini || pkm.Species == (int)Species.Jirachi || pkm.Species == (int)Species.Celebi || pkm.Species == (int)Species.Genesect) && enc.Version == GameVersion.GO;
            if (enc is EncounterSlotGO slotGO && !goMew && !goOther)
                pkm.SetRandomIVsGO(slotGO.Type.GetMinIV());
            else if (enc is EncounterStatic8N static8N)
                pkm.SetRandomIVs(static8N.FlawlessIVCount + 1);
            else if (enc is IOverworldCorrelation8 oc)
            {
                var criteria = EncounterCriteria.GetCriteria(template);
                bool owCorr = true;
                List<int> IVs = new() { 0, 0, 0, 0, 0, 0 };
                int i = 0;

                while (i < 1_000)
                {// Loosely adapted from ALM.
                    if (enc is EncounterStatic8 static8)
                    {
                        owCorr = static8.IsOverworldCorrelation;
                        if (!owCorr)
                        {
                            pkm.SetRandomIVs(Random.Next(static8.FlawlessIVCount, 7));
                            break;
                        }

                        var flawless = static8.FlawlessIVCount;
                        while (IVs.FindAll(x => x == 31).Count < flawless)
                            IVs[Random.Next(IVs.Count)] = 31;

                        pkm.IVs = new int[] { IVs[0], IVs[1], IVs[2], IVs[3], IVs[4], IVs[5] };
                        var available = xoroshiro8_wild.GetWildSeedFromIV8(new[] { flawless }, pkm.IVs, out uint seed);
                        if (owCorr)
                            APILegality.FindWildPIDIV8((PK8)pkm, shiny, available, seed);
                    }
                    else if (enc is EncounterSlot8 slot8)
                    {
                        var flawless = Random.Next(4);
                        while (IVs.FindAll(x => x == 31).Count < flawless)
                            IVs[Random.Next(IVs.Count)] = 31;

                        pkm.IVs = new int[] { IVs[0], IVs[1], IVs[2], IVs[3], IVs[4], IVs[5] };
                        var available = xoroshiro8_wild.GetWildSeedFromIV8(new[] { 0, 2, 3 }, pkm.IVs, out uint seed);
                        var req = oc.GetRequirement(pkm);
                        if (req == OverworldCorrelation8Requirement.MustHave)
                            APILegality.FindWildPIDIV8((PK8)pkm, shiny, available, seed);
                        else if (req == OverworldCorrelation8Requirement.MustNotHave)
                        {
                            pkm.SetRandomIVs(Random.Next(4));
                            break;
                        }
                    }

                    i++;
                    if (owCorr && oc.IsOverworldCorrelationCorrect(pkm))
                        break;
                    else
                    {
                        IVs = new() { 0, 0, 0, 0, 0, 0 };
                        continue;
                    }
                }
            }
            else if (enc.Version != GameVersion.GO && enc.Generation >= 6)
                pkm.SetRandomIVs(4);

            var test = BallApplicator.GetLegalBalls(pkm);
            BallApplicator.ApplyBallLegalRandom(pkm);
            if (pkm.Ball == 16)
                BallApplicator.ApplyBallLegalRandom(pkm);

            pkm = TrashBytes(pkm);
            pkm.CurrentFriendship = pkm.PersonalInfo.BaseFriendship;
            return (PK8)pkm;
        }

        public static PKM EggRngRoutine(TCUserInfoRoot.TCUserInfo info, string trainerInfo, int evo1, int evo2, bool star, bool square)
        {
            var pkm1 = PKMConverter.GetPKMfromBytes(File.ReadAllBytes(info.Catches.FirstOrDefault(x => x.ID == info.Daycare1.ID).Path));
            var pkm2 = PKMConverter.GetPKMfromBytes(File.ReadAllBytes(info.Catches.FirstOrDefault(x => x.ID == info.Daycare2.ID).Path));
            if (pkm1 == null || pkm2 == null)
                return new PK8();

            var ballRng = $"\nBall: {(Ball)Random.Next(2, 27)}";
            var ballRngDC = Random.Next(1, 3);
            var enumVals = (int[])Enum.GetValues(typeof(ValidEgg));
            bool specificEgg = (evo1 == evo2 && Breeding.CanHatchAsEgg(evo1)) || ((evo1 == 132 || evo2 == 132) && (Breeding.CanHatchAsEgg(evo1) || Breeding.CanHatchAsEgg(evo2))) || ((evo1 == 29 || evo1 == 32) && (evo2 == 29 || evo2 == 32));
            var dittoLoc = DittoSlot(evo1, evo2);
            var speciesRng = specificEgg ? SpeciesName.GetSpeciesNameGeneration(dittoLoc == 1 ? evo2 : evo1, 2, 8) : SpeciesName.GetSpeciesNameGeneration(enumVals[Random.Next(enumVals.Length)], 2, 8);
            var speciesRngID = SpeciesName.GetSpeciesID(speciesRng);
            FormOutput(speciesRngID, 0, out string[] forms);

            if (speciesRng.Contains("Nidoran"))
                speciesRng = speciesRng.Remove(speciesRng.Length - 1);

            string formHelper = speciesRng switch
            {
                "Indeedee" => _ = specificEgg && dittoLoc == 1 ? FormOutput(876, pkm2.Form, out _) : specificEgg && dittoLoc == 2 ? FormOutput(876, pkm1.Form, out _) : FormOutput(876, Random.Next(2), out _),
                "Nidoran" => _ = specificEgg && dittoLoc == 1 ? (evo2 == 32 ? "-M" : "-F") : specificEgg && dittoLoc == 2 ? (evo1 == 32 ? "-M" : "-F") : (Random.Next(2) == 0 ? "-M" : "-F"),
                "Meowth" => _ = FormOutput(speciesRngID, specificEgg && (pkm1.Species == 863 || pkm2.Species == 863) ? 2 : specificEgg && dittoLoc == 1 ? pkm2.Form : specificEgg && dittoLoc == 2 ? pkm1.Form : Random.Next(forms.Length), out _),
                "Yamask" => FormOutput(speciesRngID, specificEgg && (pkm1.Species == 867 || pkm2.Species == 867) ? 1 : specificEgg && dittoLoc == 1 ? pkm2.Form : specificEgg && dittoLoc == 2 ? pkm1.Form : Random.Next(forms.Length), out _),
                "Zigzagoon" => _ = FormOutput(speciesRngID, specificEgg && (pkm1.Species == 862 || pkm2.Species == 862) ? 1 : specificEgg && dittoLoc == 1 ? pkm2.Form : specificEgg && dittoLoc == 2 ? pkm1.Form : Random.Next(forms.Length), out _),
                "Farfetch’d" => _ = FormOutput(speciesRngID, specificEgg && (pkm1.Species == 865 || pkm2.Species == 865) ? 1 : specificEgg && dittoLoc == 1 ? pkm2.Form : specificEgg && dittoLoc == 2 ? pkm1.Form : Random.Next(forms.Length), out _),
                "Corsola" => _ = FormOutput(speciesRngID, specificEgg && (pkm1.Species == 864 || pkm2.Species == 864) ? 1 : specificEgg && dittoLoc == 1 ? pkm2.Form : specificEgg && dittoLoc == 2 ? pkm1.Form : Random.Next(forms.Length), out _),
                "Sinistea" or "Milcery" => "",
                _ => FormOutput(speciesRngID, specificEgg && pkm1.Form == pkm2.Form ? pkm1.Form : specificEgg && dittoLoc == 1 ? pkm2.Form : specificEgg && dittoLoc == 2 ? pkm1.Form : Random.Next(forms.Length), out _),
            };

            var set = new ShowdownSet($"Egg({speciesRng}{formHelper}){(ballRng.Contains("Cherish") || Pokeball.Contains(SpeciesName.GetSpeciesID(speciesRng)) ? "\nBall: Poke" : ballRng)}\n{trainerInfo}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo(8);
            var pkm = sav.GetLegal(template, out _);

            if (!specificEgg && pkm.PersonalInfo.HasForms && pkm.Species != (int)Species.Sinistea && pkm.Species != (int)Species.Indeedee)
                pkm.Form = Random.Next(pkm.PersonalInfo.FormCount);
            if (pkm.Species == (int)Species.Rotom)
                pkm.Form = 0;
            if (FormInfo.IsBattleOnlyForm(pkm.Species, pkm.Form, pkm.Format))
                pkm.Form = FormInfo.GetOutOfBattleForm(pkm.Species, pkm.Form, pkm.Format);

            if (ballRngDC == 1)
                pkm.Ball = info.Daycare1.Ball;
            else if (ballRngDC == 2)
                pkm.Ball = info.Daycare2.Ball;

            EggTrade((PK8)pkm);
            pkm.SetAbilityIndex(Random.Next(3));
            pkm.Nature = Random.Next(25);
            pkm.StatNature = pkm.Nature;
            pkm.IVs = pkm.SetRandomIVs(4);

            if (!pkm.ValidBall())
                BallApplicator.ApplyBallLegalRandom(pkm);

            if (square)
                CommonEdits.SetShiny(pkm, Shiny.AlwaysSquare);
            else if (star)
                CommonEdits.SetShiny(pkm, Shiny.AlwaysStar);

            return pkm;
        }

        public static void DittoTrade(PKM pkm)
        {
            var dittoStats = new string[] { "atk", "spe", "spa" };
            var nickname = pkm.Nickname.ToLower();
            pkm.StatNature = pkm.Nature;
            pkm.Met_Location = 162;
            pkm.Ball = 21;
            pkm.IVs = new int[] { 31, nickname.Contains(dittoStats[0]) ? 0 : 31, 31, nickname.Contains(dittoStats[1]) ? 0 : 31, nickname.Contains(dittoStats[2]) ? 0 : 31, 31 };
            pkm.ClearHyperTraining();
            _ = TrashBytes(pkm, new LegalityAnalysis(pkm));
        }

        public static void EggTrade(PK8 pk)
        {
            pk = (PK8)TrashBytes(pk);
            pk.IsNicknamed = true;
            pk.Nickname = pk.Language switch
            {
                1 => "タマゴ",
                3 => "Œuf",
                4 => "Uovo",
                5 => "Ei",
                7 => "Huevo",
                8 => "알",
                9 or 10 => "蛋",
                _ => "Egg",
            };

            pk.IsEgg = true;
            pk.Egg_Location = 60002;
            pk.MetDate = DateTime.Parse("2020/10/20");
            pk.EggMetDate = pk.MetDate;
            pk.HeldItem = 0;
            pk.CurrentLevel = 1;
            pk.EXP = 0;
            pk.DynamaxLevel = 0;
            pk.Met_Level = 1;
            pk.Met_Location = 30002;
            pk.CurrentHandler = 0;
            pk.OT_Friendship = 1;
            pk.HT_Name = "";
            pk.HT_Friendship = 0;
            pk.HT_Language = 0;
            pk.HT_Gender = 0;
            pk.HT_Memory = 0;
            pk.HT_Feeling = 0;
            pk.HT_Intensity = 0;
            pk.StatNature = pk.Nature;
            pk.EVs = new int[] { 0, 0, 0, 0, 0, 0 };
            pk.Markings = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            pk.ClearRecordFlags();
            pk.ClearRelearnMoves();
            pk.Moves = new int[] { 0, 0, 0, 0 };
            var la = new LegalityAnalysis(pk);
            var enc = la.EncounterMatch;
            pk.CurrentFriendship = enc is EncounterStatic s ? s.EggCycles : pk.PersonalInfo.HatchCycles;
            pk.RelearnMoves = MoveBreed.GetExpectedMoves(pk.RelearnMoves, la.EncounterMatch);
            pk.Moves = pk.RelearnMoves;
            pk.Move1_PPUps = pk.Move2_PPUps = pk.Move3_PPUps = pk.Move4_PPUps = 0;
            pk.SetMaximumPPCurrent(pk.Moves);
            pk.SetSuggestedHyperTrainingData();
            pk.SetSuggestedRibbons(la.EncounterMatch);
        }

        private static List<string> SpliceAtWord(string entry, int start, int length)
        {
            int counter = 0;
            var temp = entry.Contains(",") ? entry.Split(',').Skip(start) : entry.Split('\n').Skip(start);
            List<string> list = new();

            if (entry.Length < length)
            {
                list.Add(entry ?? "");
                return list;
            }

            foreach (var line in temp)
            {
                counter += line.Length + 2;
                if (counter < length)
                    list.Add(line.Trim());
                else break;
            }
            return list;
        }

        public static List<string> ListUtilPrep(string entry)
        {
            var index = 0;
            List<string> pageContent = new();
            var emptyList = "No results found.";
            var round = Math.Round((decimal)entry.Length / 1024, MidpointRounding.AwayFromZero);
            if (entry.Length > 1024)
            {
                for (int i = 0; i <= round; i++)
                {
                    var splice = SpliceAtWord(entry, index, 1024);
                    index += splice.Count;
                    if (splice.Count == 0)
                        break;

                    pageContent.Add(string.Join(entry.Contains(",") ? ", " : "\n", splice));
                }
            }
            else pageContent.Add(entry == "" ? emptyList : entry);
            return pageContent;
        }

        public static string FormOutput(int species, int form, out string[] formString)
        {
            var strings = GameInfo.GetStrings(LanguageID.English.GetLanguage2CharName());
            formString = FormConverter.GetFormList(species, strings.Types, strings.forms, GameInfo.GenderSymbolASCII, 8);
            formString[0] = "";

            if (form >= formString.Length)
                form = formString.Length - 1;

            return formString[form].Contains("-") ? formString[form] : formString[form] == "" ? "" : $"-{formString[form]}";
        }

        private static int DittoSlot(int species1, int species2)
        {
            if (species1 == 132 && species2 != 132)
                return 1;
            else if (species2 == 132 && species1 != 132)
                return 2;
            else return 0;
        }

        public static void EncounterLogs(PK8 pk, string filepath = "")
        {
            if (filepath == "")
                filepath = "EncounterLogPretty.txt";

            if (!File.Exists(filepath))
            {
                var blank = "Totals: 0 Pokémon, 0 Eggs, 0 ★, 0 ■, 0 🎀\n_________________________________________________\n";
                File.WriteAllText(filepath, blank);
            }

            lock (_syncLog)
            {
                var content = File.ReadAllText(filepath).Split('\n').ToList();
                var splitTotal = content[0].Split(',');
                content.RemoveRange(0, 3);

                int pokeTotal = int.Parse(splitTotal[0].Split(' ')[1]) + 1;
                int eggTotal = int.Parse(splitTotal[1].Split(' ')[1]) + (pk.IsEgg ? 1 : 0);
                int starTotal = int.Parse(splitTotal[2].Split(' ')[1]) + (pk.IsShiny && pk.ShinyXor > 0 ? 1 : 0);
                int squareTotal = int.Parse(splitTotal[3].Split(' ')[1]) + (pk.IsShiny && pk.ShinyXor == 0 ? 1 : 0);
                int markTotal = int.Parse(splitTotal[4].Split(' ')[1]) + (pk.HasMark() ? 1 : 0);

                var form = FormOutput(pk.Species, pk.Form, out _);
                var speciesName = $"{SpeciesName.GetSpeciesNameGeneration(pk.Species, pk.Language, 8)}{form}".Replace(" ", "");
                var index = content.FindIndex(x => x.Split(':')[0].Equals(speciesName));

                if (index == -1)
                    content.Add($"{speciesName}: 1, {(pk.IsShiny && pk.ShinyXor > 0 ? 1 : 0)}★, {(pk.IsShiny && pk.ShinyXor == 0 ? 1 : 0)}■, {(pk.HasMark() ? 1 : 0)}🎀, {GetPercent(pokeTotal, 1)}%");

                var length = index == -1 ? 1 : 0;
                for (int i = 0; i < content.Count - length; i++)
                {
                    var sanitized = GetSanitizedEncounterLineArray(content[i]);
                    if (i == index)
                    {
                        int speciesTotal = int.Parse(sanitized[1]) + 1;
                        int stTotal = int.Parse(sanitized[2]) + (pk.IsShiny && pk.ShinyXor > 0 ? 1 : 0);
                        int sqTotal = int.Parse(sanitized[3]) + (pk.IsShiny && pk.ShinyXor == 0 ? 1 : 0);
                        int mTotal = int.Parse(sanitized[4]) + (pk.HasMark() ? 1 : 0);
                        content[i] = $"{speciesName}: {speciesTotal}, {stTotal}★, {sqTotal}■, {mTotal}🎀, {GetPercent(pokeTotal, speciesTotal)}%";
                    }
                    else content[i] = $"{sanitized[0]} {sanitized[1]}, {sanitized[2]}★, {sanitized[3]}■, {sanitized[4]}🎀, {GetPercent(pokeTotal, int.Parse(sanitized[1]))}%";
                }

                content.Sort();
                string totalsString =
                    $"Totals: {pokeTotal} Pokémon, " +
                    $"{eggTotal} Eggs ({GetPercent(pokeTotal, eggTotal)}%), " +
                    $"{starTotal} ★ ({GetPercent(pokeTotal, starTotal)}%), " +
                    $"{squareTotal} ■ ({GetPercent(pokeTotal, squareTotal)}%), " +
                    $"{markTotal} 🎀 ({GetPercent(pokeTotal, markTotal)}%)" +
                    "\n_________________________________________________\n";
                content.Insert(0, totalsString);
                File.WriteAllText(filepath, string.Join("\n", content));
            }
        }

        private static string GetPercent(int total, int subtotal) => (100.0 * ((double)subtotal / total)).ToString("N2");

        private static string[] GetSanitizedEncounterLineArray(string content)
        {
            var replace = new Dictionary<string, string> { { ",", "" }, { "★", "" }, { "■", "" }, { "🎀", "" }, { "%", "" } };
            return replace.Aggregate(content, (old, cleaned) => old.Replace(cleaned.Key, cleaned.Value)).Split(' ');
        }

        public static PKM TrashBytes(PKM pkm, LegalityAnalysis? la = null)
        {
            pkm.Nickname = "KOIKOIKOIKOI";
            pkm.IsNicknamed = true;
            if (pkm.Version != (int)GameVersion.GO && !pkm.FatefulEncounter)
                pkm.MetDate = DateTime.Parse("2020/10/20");
            pkm.SetDefaultNickname(la ?? new LegalityAnalysis(pkm));
            return pkm;
        }

        public static string DexFlavor(int species, int form, bool gmax)
        {
            var resourcePath = "SysBot.Pokemon.Helpers.DexFlavor.txt";
            using StreamReader reader = new(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath));

            if (Enum.IsDefined(typeof(Foreign), species) && form > 0)
                form = 0;

            if (!gmax)
            {
                if (form > 0)
                    return reader.ReadToEnd().Split('_')[1].Split('\n')[species].Split('|')[species == 80 && form == 2 ? 0 : form - 1];
                else return reader.ReadToEnd().Split('\n')[species];
            }

            string[] str = reader.ReadToEnd().Split('_')[1].Split('\n')[species].Split('|');
            return str[^1];
        }

        public static PK8 CherishHandler(MysteryGift mg, ITrainerInfo info)
        {
            var mgPkm = mg.ConvertToPKM(info);
            mgPkm = PKMConverter.IsConvertibleToFormat(mgPkm, 8) ? PKMConverter.ConvertToType(mgPkm, typeof(PK8), out _) : mgPkm;
            if (mgPkm != null)
                mgPkm.SetHandlerandMemory(info);
            else return new();

            mgPkm.CurrentLevel = mg.LevelMin;
            if (mgPkm.Species != (int)Species.Giratina)
                mgPkm.HeldItem = 0;

            var la = new LegalityAnalysis(mgPkm);
            if (!la.Valid)
            {
                mgPkm.SetRandomIVs(6);
                var showdown = ShowdownParsing.GetShowdownText(mgPkm);
                return (PK8)AutoLegalityWrapper.GetLegal(info, new ShowdownSet(showdown), out _);
            }
            else return (PK8)mgPkm;
        }

        public static T GetRoot<T>(string file, TextReader? textReader = null) where T : new()
        {
            if (textReader == null)
            {
                if (!File.Exists(file))
                {
                    Directory.CreateDirectory(file.Split('\\')[0]);
                    File.Create(file).Close();
                }

                T? root;
                string tmp = $"{file}_tmp";
                while (true)
                {
                    try
                    {
                        if (File.Exists(tmp))
                        {
                            var size = new FileInfo(tmp).Length;
                            if (size < 1)
                                File.Copy(file, tmp, true);
                        }
                        else File.Copy(file, tmp);

                        using StreamReader sr = File.OpenText(file);
                        using JsonReader reader = new JsonTextReader(sr);
                        JsonSerializer serializer = new();
                        root = (T?)serializer.Deserialize(reader, typeof(T));

                        if (File.Exists(InfoBackupPath))
                            File.Delete(tmp);
                        return root ?? new();
                    }
                    catch
                    {
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                JsonSerializer serializer = new();
                T? root = (T?)serializer.Deserialize(textReader, typeof(T));
                return root ?? new();
            }
        }

        public static TradeCordHelper.Results ProcessTradeCord(TC_CommandContext ctx, string[] input, bool update, TradeCordSettings settings)
        {
            if (!TCInitialized)
            {
                var current = Process.GetCurrentProcess();
                var all = Process.GetProcessesByName(current.ProcessName);
                bool sameExe = all.Count(x => x.MainModule.FileName == current.MainModule.FileName) > 1;
                if (!sameExe)
                {
                    TCInitialized = true;
                    UserInfo = GetRoot<TCUserInfoRoot>(InfoPath);
                }
                else
                {
                    Base.LogUtil.LogText("Another TradeCord instance is already running! Killing the process.");
                    Environment.Exit(0);
                }
            }

            lock (_sync)
            {
                try
                {
                    var user = GetUserInfo(ctx, false);
                    if (user.DexCompletionCount >= 1)
                        ShinyCharmReward(user);

                    var traded = user.Catches.ToList().FindAll(x => x.Traded);
                    bool exists = TradeCordPath.TryGetValue(user.UserID, out string path);
                    if (traded.Count != 0 && !exists)
                    {
                        foreach (var trade in traded)
                        {
                            if (!File.Exists(trade.Path))
                                user.Catches.Remove(trade);
                            else trade.Traded = false;
                        }
                        UpdateUserInfo(user);
                    }

                    TCUserInfoRoot.TCUserInfo giftee = new();
                    if (ctx.Context == TCCommandContext.Gift || ctx.Context == TCCommandContext.GiftItem)
                        giftee = GetUserInfo(ctx, true);

                    var helper = new TradeCordHelper(settings);
                    var task = ctx.Context switch
                    {
                        TCCommandContext.Catch => helper.CatchHandler(user),
                        TCCommandContext.Trade => helper.TradeHandler(user, input[0]),
                        TCCommandContext.List => helper.ListHandler(user, input[0]),
                        TCCommandContext.Info => helper.InfoHandler(user, input[0]),
                        TCCommandContext.MassRelease => helper.MassReleaseHandler(user, input[0]),
                        TCCommandContext.Release => helper.ReleaseHandler(user, input[0]),
                        TCCommandContext.DaycareInfo => helper.DaycareInfoHandler(user),
                        TCCommandContext.Daycare => helper.DaycareHandler(user, input[0], input[1]),
                        TCCommandContext.Gift => helper.GiftHandler(user, giftee, input[0]),
                        TCCommandContext.TrainerInfoSet => helper.TrainerInfoSetHandler(user, input),
                        TCCommandContext.TrainerInfo => helper.TrainerInfoHandler(user),
                        TCCommandContext.FavoritesInfo => helper.FavoritesInfoHandler(user),
                        TCCommandContext.Favorites => helper.FavoritesHandler(user, input[0]),
                        TCCommandContext.Dex => helper.DexHandler(user, input[0]),
                        TCCommandContext.Perks => helper.PerkHandler(user, input[0]),
                        TCCommandContext.Boost => helper.SpeciesBoostHandler(user, input[0]),
                        TCCommandContext.Buddy => helper.BuddyHandler(user, input[0]),
                        TCCommandContext.Nickname => helper.NicknameHandler(user, input[0]),
                        TCCommandContext.Evolution => helper.EvolutionHandler(user, input[0]),
                        TCCommandContext.GiveItem => helper.GiveItemHandler(user, input[0]),
                        TCCommandContext.GiftItem => helper.GiftItemHandler(user, giftee, input[0], input[1]),
                        TCCommandContext.TakeItem => helper.TakeItemHandler(user),
                        TCCommandContext.ItemList => helper.ItemListHandler(user, input[0]),
                        TCCommandContext.DropItem => helper.ItemDropHandler(user, input[0]),
                        TCCommandContext.TimeZone => helper.TimeZoneHandler(user, input[0]),
                        _ => throw new NotImplementedException(),
                    };
                    var result = Task.Run(() => task).Result;

                    if (update && result.Success)
                    {
                        UpdateUserInfo(result.User);
                        if (ctx.Context == TCCommandContext.Gift || ctx.Context == TCCommandContext.GiftItem)
                            UpdateUserInfo(result.Giftee);
                    }

                    var delta = (DateTime.Now - ConfigTimer).TotalSeconds;
                    if (delta >= settings.ConfigUpdateInterval)
                    {
                        SerializeInfo();
                        ConfigTimer = DateTime.Now;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Base.LogUtil.LogText($"Something went wrong during {ctx.Context} execution for {ctx.Username}.\nMessage: {ex.Message}\nStack: {ex.StackTrace}\nInner: {ex.InnerException}");
                    return new TradeCordHelper.Results();
                }
            }
        }

        private static TCUserInfoRoot.TCUserInfo GetUserInfo(TC_CommandContext ctx, bool gift)
        {
            TCUserInfoRoot.TCUserInfo user;
            user = UserInfo.Users.FirstOrDefault(x => x.UserID == (gift ? ctx.GifteeID : ctx.ID));

            if (user == null)
                user = new TCUserInfoRoot.TCUserInfo { UserID = gift ? ctx.GifteeID : ctx.ID, Username = gift ? ctx.GifteeName : ctx.Username };

            if (user.Username == string.Empty || user.Username != ctx.Username)
                user.Username = gift ? ctx.GifteeName : ctx.Username;

            return user;
        }

        public static void UpdateUserInfo(TCUserInfoRoot.TCUserInfo user)
        {
            UserInfo.Users.RemoveWhere(x => x.UserID == user.UserID);
            UserInfo.Users.Add(user);
        }

        public static void SerializeInfo()
        {
            TCRWLockEnable = true;
            var fileSize = new FileInfo(InfoPath).Length;
            if (File.Exists(InfoPath) && fileSize > 2)
                File.Copy(InfoPath, InfoBackupPath, true);

            while (true)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(UserInfo, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    File.WriteAllText(InfoPath, json);
                    if (TestJsonIntegrity())
                        break;
                }
                catch
                {
                    Task.Delay(0_100);
                }
            }
            TCRWLockEnable = false;
        }

        private static bool TestJsonIntegrity()
        {
            try
            {
                using StreamReader sr = File.OpenText(InfoPath);
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new();
                    TCUserInfoRoot? root = (TCUserInfoRoot?)serializer.Deserialize(reader, typeof(TCUserInfoRoot));
                    if (root == null)
                        return false;
                    else if (UserInfo.Users.Count > 0 && root.Users.Count == 0)
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void TradeStatusUpdate(ulong id, bool cancelled = false)
        {
            bool exists = TradeCordPath.TryGetValue(id, out string path);
            if (!cancelled && exists)
            {
                var tradedPath = Path.Combine($"TradeCord\\Backup\\{id}", path.Split('\\')[2]);
                try
                {
                    File.Move(path, tradedPath);
                }
                catch (IOException)
                {
                    File.Move(path, tradedPath.Insert(tradedPath.IndexOf(".") - 1, "ex"));
                }
            }

            if (exists)
                TradeCordPath.Remove(id);
        }

        public static string PokeImg(PKM pkm, bool canGmax, bool fullSize)
        {
            bool md = false;
            bool fd = false;
            string[] baseLink;
            if (fullSize)
                baseLink = "https://projectpokemon.org/images/sprites-models/homeimg/poke_capture_0001_000_mf_n_00000000_f_n.png".Split('_');
            else baseLink = "https://raw.githubusercontent.com/BakaKaito/HomeImages/main/homeimg/128x128/poke_capture_0001_000_mf_n_00000000_f_n.png".Split('_');

            if (Enum.IsDefined(typeof(GenderDependent), pkm.Species) && !canGmax && pkm.Form == 0)
            {
                if (pkm.Gender == 0)
                    md = true;
                else fd = true;
            }

            baseLink[2] = pkm.Species < 10 ? $"000{pkm.Species}" : pkm.Species < 100 && pkm.Species > 9 ? $"00{pkm.Species}" : $"0{pkm.Species}";
            baseLink[3] = pkm.Form < 10 ? $"00{pkm.Form}" : $"0{pkm.Form}";
            baseLink[4] = pkm.PersonalInfo.OnlyFemale ? "fo" : pkm.PersonalInfo.OnlyMale ? "mo" : pkm.PersonalInfo.Genderless ? "uk" : fd ? "fd" : md ? "md" : "mf";
            baseLink[5] = canGmax ? "g" : "n";
            baseLink[6] = "0000000" + (pkm.Species == (int)Species.Alcremie ? pkm.Data[0xE4] : 0);
            baseLink[8] = pkm.IsShiny ? "r.png" : "n.png";
            return string.Join("_", baseLink);
        }

        public static TCRng RandomInit()
        {
            var enumVals = (int[])Enum.GetValues(typeof(Gen8Dex));
            return new TCRng()
            {
                CatchRNG = Random.Next(101),
                ShinyRNG = Random.Next(101),
                EggRNG = Random.Next(101),
                EggShinyRNG = Random.Next(101),
                GmaxRNG = Random.Next(101),
                CherishRNG = Random.Next(101),
                SpeciesRNG = enumVals[Random.Next(enumVals.Length)],
                SpeciesBoostRNG = Random.Next(101),
                ItemRNG = Random.Next(101),
            };
        }

        public static bool DeleteUserData(ulong id)
        {
            while (TCRWLockEnable)
                Task.Delay(0_100);

            TCRWLockEnable = true;
            var user = UserInfo.Users.FirstOrDefault(x => x.UserID == id);
            if (user == default)
            {
                TCRWLockEnable = false;
                return false;
            }
            else UserInfo.Users.Remove(user);

            var path = $"TradeCord\\{id}";
            var pathBackup = $"TradeCord\\Backup\\{id}";
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            if (Directory.Exists(pathBackup))
                Directory.Delete(pathBackup, true);

            TCRWLockEnable = false;
            return true;
        }

        private static void ShinyCharmReward(TCUserInfoRoot.TCUserInfo user)
        {
            var hasSC = user.Items.FirstOrDefault(x => x.Item == TCItems.ShinyCharm) != default;
            if (!hasSC)
            {
                var eggBoost = user.ActivePerks.FindAll(x => x == DexPerks.EggRateBoost).Count;
                var shinyBoost = user.ActivePerks.FindAll(x => x == DexPerks.ShinyBoost).Count;

                user.DexCompletionCount += eggBoost;
                user.DexCompletionCount += shinyBoost;

                user.Items.Add(new() { Item = TCItems.ShinyCharm, ItemCount = 1 });
                user.ActivePerks.RemoveAll(x => x == DexPerks.EggRateBoost || x == DexPerks.ShinyBoost);
                UpdateUserInfo(user);
            }
        }
    }
}