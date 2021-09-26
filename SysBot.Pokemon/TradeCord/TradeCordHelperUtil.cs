using System;
using System.Linq;
using System.Collections.Generic;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon
{
    public class TradeCordHelperUtil : TradeCordDatabase
    {
        private static readonly List<EvolutionTemplate> Evolutions = EvolutionRequirements();
        private static readonly Random Random = new();
        public readonly TCRng Rng = RandomScramble();
        public static readonly Dictionary<ulong, List<DateTime>> UserCommandTimestamps = new();
        public static readonly Dictionary<ulong, DateTime> TradeCordCooldownDict = new();
        public static readonly HashSet<ulong> MuteList = new();
        public static DateTime EventVoteTimer = new();

        private readonly string[] PartnerPikachuHeadache = { "-Original", "-Partner", "-Hoenn", "-Sinnoh", "-Unova", "-Alola", "-Kalos", "-World" };
        private readonly string[] LGPEBalls = { "Poke", "Premier", "Great", "Ultra", "Master" };
        private readonly string[] SilvallyMemory = { "", " @ Fighting Memory"," @ Flying Memory", " @ Poison Memory", " @ Ground Memory", " @ Rock Memory",
            " @ Bug Memory", " @ Ghost Memory", " @ Steel Memory", " @ Fire Memory", " @ Water Memory", " @ Grass Memory", " @ Electric Memory", " @ Psychic Memory",
            " @ Ice Memory", " @ Dragon Memory", " @ Dark Memory", " @ Fairy Memory" };
        private readonly string[] GenesectDrives = { "", " @ Douse Drive", " @ Shock Drive", " @ Burn Drive", " @ Chill Drive" };
        private readonly int[] RodentLite = { 25, 26, 27, 28, 29, 30, 32, 33, 50, 51, 172, 183, 263, 264, 298, 427, 428, 529, 530, 572, 573, 587, 659, 660, 702, 777, 778, 819, 820, 877 };
        private readonly int[] ClickbaitArticle = { 1, 6, 7, 25, 38, 81, 94, 95, 129, 130, 131, 132, 133, 143, 150, 151, 248, 249, 282, 293, 302, 330, 363, 384, 385, 405, 428, 445, 448, 545, 549, 573, 609, 689, 702, 705, 778, 794, 818, 849, 887 };
        public readonly int[] CherishOnly = { 719, 721, 801, 802, 807, 893 };
        private readonly int[] TradeEvo = { (int)Species.Machoke, (int)Species.Haunter, (int)Species.Boldore, (int)Species.Gurdurr, (int)Species.Phantump, (int)Species.Gourgeist };
        private static readonly int[] ShinyLock = { (int)Species.Victini, (int)Species.Keldeo, (int)Species.Volcanion, (int)Species.Cosmog, (int)Species.Cosmoem, (int)Species.Magearna,
                                             (int)Species.Marshadow, (int)Species.Zacian, (int)Species.Zamazenta, (int)Species.Eternatus, (int)Species.Kubfu, (int)Species.Urshifu,
                                             (int)Species.Zarude, (int)Species.Glastrier, (int)Species.Spectrier, (int)Species.Calyrex };

        private readonly int[] UMWormhole = { 144, 145, 146, 150, 244, 245, 249, 380, 382, 384, 480, 481, 482, 484, 487, 488, 644, 645, 646, 642, 717, 793, 795, 796, 797, 799 };
        private readonly int[] USWormhole = { 144, 145, 146, 150, 245, 250, 381, 383, 384, 480, 481, 482, 487, 488, 645, 646, 793, 794, 796, 799, 483, 485, 641, 643, 716, 798 };
        private readonly int[] GalarFossils = { 880, 881, 882, 883 };
        public static readonly int[] Pokeball = { 151, 722, 723, 724, 725, 726, 727, 728, 729, 730, 772, 773, 789, 790, 810, 811, 812, 813, 814, 815, 816, 817, 818, 891, 892 };
        public static readonly int[] Amped = { 3, 4, 2, 8, 9, 19, 22, 11, 13, 14, 0, 6, 24 };
        public static readonly int[] LowKey = { 1, 5, 7, 10, 12, 15, 16, 17, 18, 20, 21, 23 };

        private class EvolutionTemplate
        {
            public int Species { get; set; }
            public int BaseForm { get; set; }
            public int EvolvesInto { get; set; }
            public int EvolvedForm { get; set; }
            public int EvolvesAtLevel { get; set; }
            public EvolutionType EvoType { get; set; }
            public TCItems Item { get; set; }
            public TimeOfDay DayTime { get; set; }
        }

        public class TCRng
        {
            public int CatchRNG { get; set; }
            public double ShinyRNG { get; set; }
            public int EggRNG { get; set; }
            public double EggShinyRNG { get; set; }
            public int GmaxRNG { get; set; }
            public int CherishRNG { get; set; }
            public int SpeciesRNG { get; set; }
            public int SpeciesBoostRNG { get; set; }
            public int ItemRNG { get; set; }
            public int ShinyCharmRNG { get; set; }
        }

        private static TCRng RandomScramble()
        {
            var enumVals = (int[])Enum.GetValues(typeof(Gen8Dex));
            return new TCRng()
            {
                CatchRNG = Random.Next(101),
                ShinyRNG = Random.Next(151),
                EggRNG = Random.Next(101),
                EggShinyRNG = Random.Next(151),
                GmaxRNG = Random.Next(101),
                CherishRNG = Random.Next(101),
                SpeciesRNG = enumVals[Random.Next(enumVals.Length)],
                SpeciesBoostRNG = Random.Next(101),
                ItemRNG = Random.Next(101),
                ShinyCharmRNG = Random.Next(4097),
            };
        }

        private PK8 TradeCordPK(int species) => (PK8)AutoLegalityWrapper.GetTrainerInfo(8).GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet(SpeciesName.GetSpeciesNameGeneration(species, 2, 8))), out _);

        public PK8 RngRoutine(PKM pkm, IBattleTemplate template, Shiny shiny)
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

            pkm = TradeExtensions.TrashBytes(pkm);
            pkm.CurrentFriendship = pkm.PersonalInfo.BaseFriendship;
            return (PK8)pkm;
        }

        public PK8 EggRngRoutine(EvoCriteria evo1, EvoCriteria evo2, int ball1, int ball2, string trainerInfo, bool star, bool square)
        {
            var shinyRng = square ? "\nShiny: Square" : star ? "\nShiny: Star" : "";
            var enumVals = (int[])Enum.GetValues(typeof(ValidEgg));
            var dittoLoc = DittoSlot(evo1.Species, evo2.Species);
            bool random = evo1.Species == 132 && evo2.Species == 132;
            var speciesRng = random ? SpeciesName.GetSpeciesNameGeneration(enumVals[Random.Next(enumVals.Length)], 2, 8) : SpeciesName.GetSpeciesNameGeneration(dittoLoc == 1 ? evo2.Species : evo1.Species, 2, 8);
            var speciesRngID = SpeciesName.GetSpeciesID(speciesRng);

            var ballRngDC = Random.Next(1, 3);
            if ((ballRngDC == 1 && (ball1 == (int)Ball.Master || ball1 == (int)Ball.Cherish)) || (ballRngDC == 2 && (ball2 == (int)Ball.Master || ball2 == (int)Ball.Cherish)))
                ballRngDC = 0;

            var ballRng = ballRngDC == 1 ? $"\nBall: {(Ball)ball1}" : ballRngDC == 2 ? $"\nBall: {(Ball)ball2}" : $"\nBall: {(Ball)Random.Next(2, 27)}";
            if (Pokeball.Contains(speciesRngID) || ballRng.Contains("Cherish") || ballRng.Contains("Master"))
                ballRng = "\nBall: Poke";

            if (speciesRng.Contains("Nidoran"))
                speciesRng = speciesRng.Remove(speciesRng.Length - 1);

            FormOutput(speciesRngID, 0, out string[] forms);
            var formRng = Random.Next(2) == 0 ? evo1.Form : evo2.Form;
            string formHelper = speciesRng switch
            {
                "Nidoran" => _ = !random && dittoLoc == 1 ? (evo2.Species == 32 ? "-M" : "-F") : !random && dittoLoc == 2 ? (evo1.Species == 32 ? "-M" : "-F") : (Random.Next(2) == 0 ? "-M" : "-F"),
                _ => FormOutput(speciesRngID, !random && ((!random && dittoLoc == 2) || (evo1.Form == evo2.Form)) ? evo1.Form : !random && dittoLoc == 1 ? evo2.Form : !random ? formRng : Random.Next(forms.Length), out _),
            };

            bool rotom = speciesRngID == (int)Species.Rotom && formHelper != "";
            int formIndex = forms.ToList().IndexOf(formHelper.Replace("-", ""));
            if (formHelper != "" && (!Breeding.CanHatchAsEgg(speciesRngID, formIndex, 8) || FormInfo.IsBattleOnlyForm(speciesRngID, formIndex, 8) || rotom))
                formHelper = "";

            var set = new ShowdownSet($"Egg({speciesRng}{formHelper}){ballRng}{shinyRng}\n{trainerInfo}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo(8);
            var pk = (PK8)sav.GetLegal(template, out _);

            TradeExtensions.EggTrade(pk);
            pk.SetAbilityIndex(Random.Next(3));
            pk.Nature = Random.Next(25);
            pk.StatNature = pk.Nature;
            pk.IVs = pk.SetRandomIVs(4);
            return pk;
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

        private int DittoSlot(int species1, int species2)
        {
            if (species1 == 132 && species2 != 132)
                return 1;
            else if (species2 == 132 && species1 != 132)
                return 2;
            else return 0;
        }

        private static List<EvolutionTemplate> EvolutionRequirements()
        {
            var list = new List<EvolutionTemplate>();
            for (int i = 1; i < 900; i++)
            {
                var temp = new PK8 { Species = i };
                for (int f = 0; f < temp.PersonalInfo.FormCount; f++)
                {
                    var blank = new PK8 { Species = i, Form = f };
                    if (i == (int)Species.Meowstic && f > 0)
                        blank = new PK8 { Species = i, Form = f, Gender = 1 };
                    else if (i == (int)Species.Salazzle || i == (int)Species.Froslass || i == (int)Species.Vespiquen)
                        blank = new PK8 { Species = i, Gender = 1 };
                    else if (i == (int)Species.Gallade)
                        blank = new PK8 { Species = i, Gender = 0 };

                    var evoTree = EvolutionTree.GetEvolutionTree(blank, 8);
                    var preEvos = evoTree.GetValidPreEvolutions(blank, 100, 8, true);
                    var evos = evoTree.GetEvolutions(blank.Species, blank.Form);

                    if (preEvos.Count >= 2 && evos.Count() == 0)
                    {
                        for (int c = 0; c < preEvos.Count; c++)
                        {
                            var evoType = (EvolutionType)preEvos[c].Method;
                            TCItems item = TCItems.None;
                            bool baseSp = c - 1 < 0;

                            if (evoType == EvolutionType.TradeHeldItem || evoType == EvolutionType.UseItem || evoType == EvolutionType.UseItemFemale || evoType == EvolutionType.UseItemMale || evoType == EvolutionType.LevelUpHeldItemDay || evoType == EvolutionType.LevelUpHeldItemNight || evoType == EvolutionType.SpinType)
                                item = GetEvoItem(baseSp ? -1 : preEvos[c - 1].Species, f);

                            var template = new EvolutionTemplate
                            {
                                Species = preEvos[c].Species,
                                BaseForm = preEvos[c].Form,
                                EvolvesInto = baseSp ? -1 : preEvos[c - 1].Species,
                                EvolvedForm = baseSp ? -1 : preEvos[c - 1].Form,
                                EvolvesAtLevel = baseSp ? -1 : preEvos[c - 1].MinLevel,
                                EvoType = (int)evoType == 255 ? EvolutionType.None : evoType,
                                Item = item,
                                DayTime = GetEvoTime(evoType),
                            };

                            if (preEvos[c].Species == (int)Species.Cosmoem)
                                template.EvolvesAtLevel = 53;
                            else if (preEvos[c].Species == (int)Species.Tangrowth)
                                template.EvolvesAtLevel = 24;

                            list.Add(template);
                        }
                    }
                }
            }
            return list;
        }

        private static TCItems GetEvoItem(int species, int form)
        {
            return species switch
            {
                // Use item
                (int)Species.Vaporeon or (int)Species.Poliwrath or (int)Species.Cloyster or (int)Species.Starmie or (int)Species.Ludicolo or (int)Species.Simipour => TCItems.WaterStone,
                (int)Species.Jolteon or (int)Species.Raichu or (int)Species.Magnezone or (int)Species.Eelektross or (int)Species.Vikavolt => TCItems.ThunderStone,
                (int)Species.Flareon or (int)Species.Arcanine or (int)Species.Simisear => TCItems.FireStone,
                (int)Species.Leafeon or (int)Species.Vileplume or (int)Species.Victreebel or (int)Species.Exeggutor or (int)Species.Shiftry or (int)Species.Simisage => TCItems.LeafStone,
                (int)Species.Ninetales or (int)Species.Sandshrew when form > 0 => TCItems.IceStone,
                (int)Species.Glaceon => TCItems.IceStone,
                (int)Species.Darmanitan when form == 2 => TCItems.IceStone,
                (int)Species.Nidoqueen or (int)Species.Nidoking or (int)Species.Clefable or (int)Species.Wigglytuff or (int)Species.Delcatty or (int)Species.Musharna => TCItems.MoonStone,
                (int)Species.Bellossom or (int)Species.Sunflora or (int)Species.Whimsicott or (int)Species.Lilligant or (int)Species.Heliolisk => TCItems.SunStone,
                (int)Species.Togekiss or (int)Species.Roserade or (int)Species.Cinccino or (int)Species.Florges => TCItems.ShinyStone,
                (int)Species.Honchkrow or (int)Species.Mismagius or (int)Species.Chandelure or (int)Species.Aegislash => TCItems.DuskStone,
                (int)Species.Gallade or (int)Species.Froslass => TCItems.DawnStone,
                (int)Species.Polteageist => form == 0 ? TCItems.CrackedPot : TCItems.ChippedPot,
                (int)Species.Appletun => TCItems.SweetApple,
                (int)Species.Flapple => TCItems.TartApple,
                (int)Species.Slowbro when form > 0 => TCItems.GalaricaCuff,
                (int)Species.Slowking when form > 0 => TCItems.GalaricaWreath,
                (int)Species.Slowking or (int)Species.Politoed => TCItems.KingsRock,

                // Held item
                (int)Species.Kingdra => TCItems.DragonScale,
                (int)Species.PorygonZ => TCItems.DubiousDisc,
                (int)Species.Electivire => TCItems.Electirizer,
                (int)Species.Magmortar => TCItems.Magmarizer,
                (int)Species.Steelix or (int)Species.Scizor => TCItems.MetalCoat,
                (int)Species.Chansey => TCItems.OvalStone,
                (int)Species.Milotic => TCItems.PrismScale,
                (int)Species.Rhyperior => TCItems.Protector,
                (int)Species.Weavile => TCItems.RazorClaw,
                (int)Species.Dusknoir => TCItems.ReaperCloth,
                (int)Species.Aromatisse => TCItems.Sachet,
                (int)Species.Porygon2 => TCItems.Upgrade,
                (int)Species.Slurpuff => TCItems.WhippedDream,
                (int)Species.Alcremie => TCItems.Sweets,
                _ => TCItems.None,
            };
        }

        private static TimeOfDay GetEvoTime(EvolutionType type)
        {
            return type switch
            {
                EvolutionType.LevelUpFriendshipMorning or EvolutionType.LevelUpMorning => TimeOfDay.Morning,
                EvolutionType.LevelUpHeldItemDay or EvolutionType.LevelUpVersionDay => TimeOfDay.Day,
                EvolutionType.LevelUpFriendshipNight or EvolutionType.LevelUpHeldItemNight or EvolutionType.LevelUpNight or EvolutionType.LevelUpVersionNight => TimeOfDay.Night,
                EvolutionType.LevelUpDusk => TimeOfDay.Dusk,
                _ => TimeOfDay.Any,
            };
        }

        public string GetItemString(int item) => GameInfo.Strings.itemlist[item];

        public static bool ArticleChoice(char letter)
        {
            letter = char.ToLowerInvariant(letter);
            return letter switch
            {
                'a' or 'e' or 'i' or 'o' or 'u' or 'y' => true,
                _ => false,
            };
        }

        public static string TimeOfDayString(int offset, bool icon = true)
        {
            var tod = GetTimeOfDay(offset);
            return tod switch
            {
                TimeOfDay.Dawn => icon ? "https://i.imgur.com/hSQR4MT.png" : "Dawn",
                TimeOfDay.Morning => icon ? "https://i.imgur.com/tZiPlen.png" : "Morning",
                TimeOfDay.Day => icon ? "https://i.imgur.com/tZiPlen.png" : "Day",
                TimeOfDay.Dusk => icon ? "https://i.imgur.com/hSQR4MT.png" : "Dusk",
                _ => icon ? "https://i.imgur.com/ZL7sCqW.png" : "Night",
            };
        }

        private static TimeOfDay GetTimeOfDay(int offset)
        {
            var time = (offset < 0 ? DateTime.UtcNow.Subtract(TimeSpan.FromHours(offset * -1)) : DateTime.UtcNow.AddHours(offset)).Hour;
            if (time < 6 && time >= 5)
                return TimeOfDay.Dawn;
            else if (time >= 6 && time < 12)
                return TimeOfDay.Morning;
            else if (time >= 12 && time < 19)
                return TimeOfDay.Day;
            if (time >= 19 && time < 20)
                return TimeOfDay.Dusk;
            else return TimeOfDay.Night;
        }

        private uint GetAlcremieDeco(TCItems item)
        {
            return item switch
            {
                TCItems.StrawberrySweet => 0,
                TCItems.BerrySweet => 1,
                TCItems.LoveSweet => 2,
                TCItems.StarSweet => 3,
                TCItems.CloverSweet => 4,
                TCItems.FlowerSweet => 5,
                TCItems.RibbonSweet => 6,
                _ => 0,
            };
        }

        private PK8? ShedinjaGenerator(PK8 pk, out string msg)
        {
            PK8? shedinja = (PK8)pk.Clone();
            var index = shedinja.PersonalInfo.GetAbilityIndex(shedinja.Ability);
            shedinja.Species = (int)Species.Shedinja;
            shedinja.SetGender(2);
            shedinja.Ball = 4;
            shedinja.SetAbilityIndex(index);
            shedinja.ClearNickname();
            shedinja.Move1_PPUps = shedinja.Move2_PPUps = shedinja.Move3_PPUps = shedinja.Move4_PPUps = 0;
            shedinja.SetSuggestedMoves();
            shedinja.SetMaximumPPCurrent(shedinja.Moves);
            shedinja.HealPP();

            var la = new LegalityAnalysis(shedinja);
            var enc = la.Info.EncounterMatch;
            shedinja.SetRelearnMoves(shedinja.GetSuggestedRelearnMoves(enc));

            msg = string.Empty;
            la = new LegalityAnalysis(shedinja);
            if (!la.Valid)
            {
                msg = $"Failed to evolve Nincada: \n{la.Report()}";
                shedinja = null;
            }
            return shedinja;
        }

        public bool EvolvePK(PK8 pk, TimeOfDay tod, out string msg, out PK8? shedinja, AlcremieForms alcremie = AlcremieForms.None, RegionalFormArgument arg = RegionalFormArgument.None)
        {
            msg = string.Empty;
            shedinja = null;
            var tree = EvolutionTree.GetEvolutionTree(pk, 8);
            var evos = tree.GetEvolutions(pk.Species, pk.Form).ToArray();

            bool hasEvo = evos.Length > 0;
            if (!hasEvo)
            {
                msg = "This Pokémon cannot evolve.";
                return false;
            }

            var heldItem = (TCItems)pk.HeldItem;
            var form = arg != RegionalFormArgument.None && pk.Species != (int)Species.Meowth && (int)arg > 1 ? (int)arg - 1 : (int)arg;
            var evoList = Evolutions.FindAll(x => x.Species == pk.Species && x.Item == (alcremie != AlcremieForms.None ? TCItems.Sweets : heldItem));
            if (evoList.Count == 0)
            {
                msg = "No evolution results found for this Pokémon or criteria not met.";
                return false;
            }

            var result = EdgeCaseEvolutions(evoList, pk, (int)alcremie, form, (int)heldItem, tod);
            if (result != default && result.DayTime != TimeOfDay.Any && result.DayTime != tod)
            {
                msg = $"This Pokémon seems to like the {Enum.GetName(typeof(TimeOfDay), result.DayTime).ToLower()}.";
                return false;
            }
            else if (result == default)
            {
                msg = "Criteria not met or this Pokémon cannot evolve further.";
                return false;
            }
            else if (pk.CurrentLevel < result.EvolvesAtLevel)
            {
                msg = $"Current level is too low, needs to be at least level {result.EvolvesAtLevel}.";
                return false;
            }
            else if (pk.CanGigantamax && (pk.Species == (int)Species.Meowth || pk.Species == (int)Species.Pikachu || pk.Species == (int)Species.Eevee))
            {
                msg = $"Gigantamax {SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8)} cannot evolve.";
                return false;
            }

            switch (result.EvoType)
            {
                case EvolutionType.Trade:
                case EvolutionType.TradeHeldItem:
                case EvolutionType.TradeSpecies:
                    {
                        var clone = pk.Clone();
                        clone.OT_Name = "Nishikigoi";
                        var trainer = new PokeTrainerDetails(clone);
                        pk.Trade(trainer, 20, 10, 2020);
                    }; break;
                case EvolutionType.SpinType:
                    {
                        if ((int)heldItem >= 1109 && (int)heldItem <= 1115)
                            pk.FormArgument = GetAlcremieDeco(heldItem);
                    }; break;
                case EvolutionType.LevelUpFriendship:
                case EvolutionType.LevelUpFriendshipMorning:
                case EvolutionType.LevelUpFriendshipNight:
                    {
                        pk.CurrentLevel++;
                        if (pk.CurrentFriendship < 179)
                        {
                            msg = "Your Pokémon isn't friendly enough yet.";
                            return false;
                        }
                    }; break;
                case EvolutionType.LevelUpAffection50MoveType:
                    {
                        pk.CurrentLevel++;
                        if (pk.CurrentFriendship < 250)
                        {
                            msg = "Your Pokémon isn't affectionate enough yet.";
                            return false;
                        }
                    }; break;
                case EvolutionType.UseItemFemale:
                case EvolutionType.LevelUpFemale:
                    {
                        if (pk.Gender != 1)
                        {
                            msg = "Incompatible gender for evolution type.";
                            return false;
                        }

                        if (result.EvoType == EvolutionType.LevelUpFemale)
                            pk.CurrentLevel++;
                    }; break;
                case EvolutionType.UseItemMale:
                case EvolutionType.LevelUpMale:
                    {
                        if (pk.Gender != 0)
                        {
                            msg = "Incompatible gender for evolution type.";
                            return false;
                        }

                        if (result.EvoType == EvolutionType.LevelUpMale)
                            pk.CurrentLevel++;
                    }; break;
                case EvolutionType.LevelUpKnowMove or EvolutionType.LevelUp: pk.CurrentLevel++; break;
            };

            if (pk.Species == (int)Species.Nincada)
            {
                shedinja = ShedinjaGenerator(pk, out msg);
                if (shedinja == null)
                    return false;
            }

            bool applyMoves = false;
            if (pk.Generation == 8 && ((pk.Species == (int)Species.Koffing && result.EvolvedForm == 0) || ((pk.Species == (int)Species.Exeggcute || pk.Species == (int)Species.Pikachu || pk.Species == (int)Species.Cubone) && result.EvolvedForm > 0)))
            {
                applyMoves = true;
                int version = pk.Version;
                pk.Version = (int)GameVersion.UM;
                pk.Met_Location = 78; // Paniola Ranch
                pk.Met_Level = 1;
                pk.SetEggMetData(GameVersion.UM, (GameVersion)version);
                var sav = new SimpleTrainerInfo() { OT = pk.OT_Name, Gender = pk.OT_Gender, Generation = version, Language = pk.Language, SID = pk.TrainerSID7, TID = pk.TrainerID7 };
                pk.SetHandlerandMemory(sav);
                pk.HeightScalar = 0;
                pk.WeightScalar = 0;
                if (pk.Ball == (int)Ball.Sport || (pk.WasEgg && pk.Ball == (int)Ball.Master))
                    pk.SetSuggestedBall(true);
            }

            var index = pk.PersonalInfo.GetAbilityIndex(pk.Ability);
            pk.Species = result.EvolvesInto;
            pk.Form = result.EvolvedForm;
            pk.SetAbilityIndex(index);
            pk.Nickname = pk.IsNicknamed ? pk.Nickname : pk.ClearNickname();
            if (pk.Species == (int)Species.Runerigus)
                pk.SetSuggestedFormArgument((int)Species.Yamask);

            var la = new LegalityAnalysis(pk);
            if (!la.Valid && result.EvoType == EvolutionType.LevelUpKnowMove || applyMoves)
            {
                EdgeCaseRelearnMoves(pk, la);
                la = new LegalityAnalysis(pk);
            }

            if (!la.Valid)
            {
                msg = $"Failed to evolve! Legality report: \n{la.Report()}\n\nWere all evolution requirements and conditions satisfied?";
                return false;
            }

            if (heldItem > 0)
                pk.HeldItem = 0;

            return true;
        }

        private void EdgeCaseRelearnMoves(PK8 pk, LegalityAnalysis la)
        {
            if (pk.Met_Location == 162 || pk.Met_Location == 244)
                return;

            pk.Moves = la.GetMoveSet();
            pk.RelearnMoves = (int[])pk.GetSuggestedRelearnMoves(la.EncounterMatch);
            var indexEmpty = pk.RelearnMoves.ToList().IndexOf(0);
            if (indexEmpty != -1)
            {
                int move = pk.Species switch
                {
                    (int)Species.Tangrowth when !pk.RelearnMoves.Contains(246) => 246, // Ancient Power
                    (int)Species.Grapploct when !pk.RelearnMoves.Contains(269) => 269, // Taunt
                    (int)Species.Lickilicky when !pk.RelearnMoves.Contains(205) => 205, // Rollout
                    _ => 0,
                };

                switch (indexEmpty)
                {
                    case 0: pk.RelearnMove1 = move; break;
                    case 1: pk.RelearnMove2 = move; break;
                    case 2: pk.RelearnMove3 = move; break;
                    case 3: pk.RelearnMove4 = move; break;
                };
            }
            pk.HealPP();
        }

        private EvolutionTemplate EdgeCaseEvolutions(List<EvolutionTemplate> evoList, PK8 pk, int alcremieForm, int form, int item, TimeOfDay tod)
        {
            EvolutionTemplate result = pk.Species switch
            {
                (int)Species.Tyrogue => pk.Stat_ATK == pk.Stat_DEF ? evoList.Find(x => x.EvoType == EvolutionType.LevelUpAeqD) : pk.Stat_ATK > pk.Stat_DEF ? evoList.Find(x => x.EvoType == EvolutionType.LevelUpATK) : evoList.Find(x => x.EvoType == EvolutionType.LevelUpDEF),
                (int)Species.Eevee when item > 0 => evoList.Find(x => x.Item == (TCItems)item),
                (int)Species.Eevee when pk.CurrentFriendship >= 250 => evoList.Find(x => x.EvoType == EvolutionType.LevelUpAffection50MoveType),
                (int)Species.Eevee when item <= 0 => evoList.Find(x => x.DayTime == tod),
                (int)Species.Toxel => LowKey.Contains(pk.Nature) ? evoList.Find(x => x.EvolvedForm == 1) : evoList.Find(x => x.EvolvedForm == 0),
                (int)Species.Milcery when alcremieForm >= 0 => evoList.Find(x => x.EvolvedForm == alcremieForm),
                (int)Species.Cosmoem => pk.Version == 45 ? evoList.Find(x => x.EvolvesInto == (int)Species.Lunala) : evoList.Find(x => x.EvolvesInto == (int)Species.Solgaleo),
                (int)Species.Nincada => evoList.Find(x => x.EvolvesInto == (int)Species.Ninjask),
                (int)Species.Espurr => evoList.Find(x => x.EvolvedForm == (pk.Gender == (int)Gender.Male ? 0 : 1)),
                (int)Species.Combee => evoList.Find(x => x.EvolvesInto == (pk.Gender == (int)Gender.Male ? -1 : (int)Species.Vespiquen)),
                (int)Species.Koffing or (int)Species.Exeggcute or (int)Species.Pikachu or (int)Species.Cubone when form != -1 => evoList.Find(x => x.EvolvedForm == form),
                (int)Species.Meowth when pk.Form == 2 => evoList.Find(x => x.EvolvesInto == (int)Species.Perrserker),
                (int)Species.Zigzagoon or (int)Species.Linoone or (int)Species.Yamask or (int)Species.Corsola or (int)Species.Diglett when pk.Form > 0 => evoList.Find(x => x.BaseForm > 0),
                (int)Species.Darumaka => pk.Form == 1 ? evoList.Find(x => x.EvolvedForm == 2 && x.Item == (TCItems)item) : evoList.Find(x => x.EvolvedForm == 0),
                _ => evoList.Find(x => x.BaseForm == pk.Form),
            };
            return result;
        }

        public int Indexing(int[] array)
        {
            var i = 0;
            return array.Where(x => x > 0).Distinct().OrderBy(x => x).Any(x => x != (i += 1)) ? i : i + 1;
        }

        public string ListNameSanitize(string name)
        {
            if (name == "")
                return name;

            name = name.Substring(0, 1).ToUpper().Trim() + name[1..].ToLower().Trim();
            if (name.Contains("'"))
                name = name.Replace("'", "’");
            else if (name.Contains(" - "))
                name = name.Replace(" - ", "-");

            if (name.Contains('-'))
            {
                var split = name.Split('-');
                bool exceptions = split[1] == "z" || split[1] == "m" || split[1] == "f";
                name = split[0] + "-" + (split[1].Length < 2 && !exceptions ? split[1] : split[1].Substring(0, 1).ToUpper() + split[1][1..].ToLower() + (split.Length > 2 ? "-" + split[2].ToUpper() : ""));
            }

            if (name.Contains(' '))
            {
                var split = name.Split(' ');
                name = split[0] + " " + split[1].Substring(0, 1).ToUpper() + split[1][1..].ToLower();
                if (name.Contains("-"))
                    name = name.Split('-')[0] + "-" + name.Split('-')[1].Substring(0, 1).ToUpper() + name.Split('-')[1][1..];
            }
            return name;
        }

        public bool CanGenerateEgg(TCDaycare dc, ulong userID, out EvoCriteria criteria1, out EvoCriteria criteria2, out int ball1, out int ball2)
        {
            criteria1 = criteria2 = new(0, 0);
            ball1 = ball2 = 0;
            if (dc.Species1 == 0 || dc.Species2 == 0)
                return false;

            var pk1 = GetLookupAsClassObject<PK8>(userID, "binary_catches", $"and id = {dc.ID1}");
            var pk2 = GetLookupAsClassObject<PK8>(userID, "binary_catches", $"and id = {dc.ID2}");
            if (pk1.IsEgg || pk2.IsEgg)
                return false;

            var tree1 = EvolutionTree.GetEvolutionTree(pk1, 8);
            var tree2 = EvolutionTree.GetEvolutionTree(pk2, 8);
            bool sameTree = tree1.IsSpeciesDerivedFrom(pk1.Species, pk1.Form, pk2.Species, pk2.Form) || tree2.IsSpeciesDerivedFrom(pk2.Species, pk2.Form, pk1.Species, pk1.Form);
            bool breedable = (Breeding.CanHatchAsEgg(pk1.Species) || pk1.Species == 132) && (Breeding.CanHatchAsEgg(pk2.Species) || pk2.Species == 132);
            if (!sameTree && !breedable)
                return false;

            List<EvoCriteria> criteria = EggEvoCriteria(pk1, pk2);
            if (criteria.Count < 2)
                return false;

            criteria1 = criteria[0];
            criteria2 = criteria[1];
            ball1 = pk1.Ball;
            ball2 = pk2.Ball;
            return true;
        }

        private List<EvoCriteria> EggEvoCriteria(PKM pk1, PKM pk2)
        {
            List<PKM> list = new() { pk1, pk2 };
            List<EvoCriteria> criteriaList = new();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Form = list[i].Species switch
                {
                    (int)Species.Sinistea or (int)Species.Polteageist or (int)Species.Rotom or (int)Species.Pikachu or (int)Species.Raichu or (int)Species.Marowak or (int)Species.Exeggutor or (int)Species.Weezing or (int)Species.Alcremie => 0,
                    _ => list[i].Form,
                };

                int form = list[i].Species switch
                {
                    (int)Species.Obstagoon or (int)Species.Cursola or (int)Species.Runerigus or (int)Species.Sirfetchd => 1,
                    (int)Species.Perrserker => 2,
                    (int)Species.Lycanroc when list[i].Form == 2 => 1,
                    (int)Species.Lycanroc when list[i].Form == 1 => 0,
                    (int)Species.Slowbro when list[i].Form == 2 => 1,
                    _ => -1,
                };

                EvoCriteria? evo = default;
                var preEvos = EvolutionTree.GetEvolutionTree(8).GetValidPreEvolutions(list[i], 100, 8, true).FindAll(x => x.MinLevel == 1);
                if (preEvos.Count == 0)
                    continue;
                else evo = preEvos.LastOrDefault(x => x.Form == (form > -1 ? form : list[i].Form));

                if (evo != default)
                    criteriaList.Add(evo);
            }
            return criteriaList;
        }

        public List<string> GetMissingDexEntries(int[] entries, List<int> dex)
        {
            List<string> missing = new();
            foreach (var entry in entries)
            {
                if (!dex.Contains(entry))
                    missing.Add(SpeciesName.GetSpeciesNameGeneration(entry, 2, 8));
            }
            return missing;
        }

        public PK8 SetProcess(string speciesName, List<string> trainerInfo, int eventForm, TradeCordSettings settings)
        {
            string formHack = string.Empty;
            var formEdgeCaseRng = Random.Next(11);
            string[] poipoleRng = { "Poke", "Beast" };
            string[] mewOverride = { "\n.Version=34", "\n.Version=3" };
            string[] mewEmeraldBalls = { "Poke", "Great", "Ultra", "Dive", "Luxury", "Master", "Nest", "Net", "Premier", "Repeat", "Timer" };
            int[] ignoreForm = { 382, 383, 646, 716, 717, 778, 800, 845, 875, 877, 888, 889, 890, 898 };
            Shiny shiny = Rng.ShinyRNG >= 150 - settings.SquareShinyRate ? Shiny.AlwaysSquare : Rng.ShinyRNG >= 150 - settings.StarShinyRate ? Shiny.AlwaysStar : Shiny.Never;
            string shinyType = shiny == Shiny.AlwaysSquare ? "\nShiny: Square" : shiny == Shiny.AlwaysStar ? "\nShiny: Star" : "";

            if (Rng.SpeciesRNG == (int)Species.NidoranF || Rng.SpeciesRNG == (int)Species.NidoranM)
                speciesName = speciesName.Remove(speciesName.Length - 1);

            FormOutput(Rng.SpeciesRNG, 0, out string[] forms);
            var formRng = Random.Next(Rng.SpeciesRNG == (int)Species.Zygarde ? forms.Length - 1 : forms.Length);

            if (!ignoreForm.Contains(Rng.SpeciesRNG))
            {
                formHack = Rng.SpeciesRNG switch
                {
                    (int)Species.Meowstic or (int)Species.Indeedee => formEdgeCaseRng < 5 ? "-M" : "-F",
                    (int)Species.NidoranF or (int)Species.NidoranM => Rng.SpeciesRNG == (int)Species.NidoranF ? "-F (F)" : "-M (M)",
                    (int)Species.Sinistea or (int)Species.Polteageist => formEdgeCaseRng < 5 ? "" : "-Antique",
                    (int)Species.Pikachu => _ = formEdgeCaseRng < 5 ? "" : PartnerPikachuHeadache[Random.Next(PartnerPikachuHeadache.Length)],
                    (int)Species.Dracovish or (int)Species.Dracozolt => formEdgeCaseRng < 5 ? "" : "\nAbility: Sand Rush",
                    (int)Species.Arctovish or (int)Species.Arctozolt => formEdgeCaseRng < 5 ? "" : "\nAbility: Slush Rush",
                    (int)Species.Giratina => formEdgeCaseRng < 5 ? "" : "-Origin @ Griseous Orb",
                    (int)Species.Keldeo => formEdgeCaseRng < 5 ? "" : "-Resolute",
                    _ => eventForm == -1 ? $"-{forms[formRng]}" : $"-{forms[eventForm]}",
                };

                formHack = formHack == "-" ? "" : formHack;
            }

            if (formHack != "" && (Rng.SpeciesRNG == (int)Species.Silvally || Rng.SpeciesRNG == (int)Species.Genesect))
            {
                switch (Rng.SpeciesRNG)
                {
                    case 649: formHack += GenesectDrives[eventForm != -1 ? eventForm : formRng]; break;
                    case 773: formHack += SilvallyMemory[eventForm != -1 ? eventForm : formRng]; break;
                };
            }

            bool birbs = ShinyLockCheck(Rng.SpeciesRNG, "", formHack != "");
            string gameVer = Rng.SpeciesRNG switch
            {
                (int)Species.Exeggutor or (int)Species.Marowak => "\n.Version=33",
                (int)Species.Mew => shiny != Shiny.Never ? $"{mewOverride[Random.Next(2)]}" : "",
                _ => UMWormhole.Contains(Rng.SpeciesRNG) && shiny == Shiny.AlwaysSquare && !birbs ? "\n.Version=33" : USWormhole.Contains(Rng.SpeciesRNG) && shiny == Shiny.AlwaysSquare && !birbs ? "\n.Version=32" : "",
            };

            if (Rng.SpeciesRNG == (int)Species.Mew && gameVer == mewOverride[1] && trainerInfo[4] != "")
                trainerInfo[4] = "";

            bool hatchu = Rng.SpeciesRNG == 25 && formHack != "" && formHack != "-Partner";
            string ballRng = Rng.SpeciesRNG switch
            {
                (int)Species.Poipole or (int)Species.Naganadel => $"\nBall: {poipoleRng[Random.Next(poipoleRng.Length)]}",
                (int)Species.Meltan or (int)Species.Melmetal => $"\nBall: {LGPEBalls[Random.Next(LGPEBalls.Length)]}",
                (int)Species.Dracovish or (int)Species.Dracozolt or (int)Species.Arctovish or (int)Species.Arctozolt => _ = formEdgeCaseRng < 5 ? $"\nBall: Poke" : $"\nBall: {(Ball)Random.Next(1, 27)}",
                (int)Species.Treecko or (int)Species.Torchic or (int)Species.Mudkip => $"\nBall: {(Ball)Random.Next(2, 27)}",
                (int)Species.Pikachu or (int)Species.Victini or (int)Species.Celebi or (int)Species.Jirachi or (int)Species.Genesect or (int)Species.Silvally => "\nBall: Poke",
                (int)Species.Mew => gameVer == mewOverride[1] ? $"\nBall: {mewEmeraldBalls[Random.Next(mewEmeraldBalls.Length)]}" : "\nBall: Poke",
                _ => Pokeball.Contains(Rng.SpeciesRNG) || gameVer == "\n.Version=33" || gameVer == "\n.Version=32" ? "\nBall: Poke" : $"\nBall: {(Ball)Random.Next(1, 27)}",
            };

            if (ballRng.Contains("Cherish"))
                ballRng = ballRng.Replace("Cherish", "Poke");

            if (ShinyLockCheck(Rng.SpeciesRNG, ballRng, formHack != "") || hatchu)
            {
                shinyType = "";
                shiny = Shiny.Never;
            }

            var set = new ShowdownSet($"{speciesName}{formHack}{ballRng}{shinyType}\n{string.Join("\n", trainerInfo)}{gameVer}");
            if (set.CanToggleGigantamax(set.Species, set.Form) && Rng.GmaxRNG >= 100 - settings.GmaxRate)
                set.CanGigantamax = true;

            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo(8);
            var pk = (PK8)sav.GetLegal(template, out string result);

            if (pk.FatefulEncounter || result != "Regenerated")
                return pk;
            else return RngRoutine(pk, template, shiny);
        }

        public void EventHandler(TradeCordSettings settings, out MysteryGift? mg, out int form)
        {
            string type = string.Empty;
            var enumVals = (int[])Enum.GetValues(typeof(Gen8Dex));
            var enumEggs = (int[])Enum.GetValues(typeof(ValidEgg));
            var halloween = (string[])Enum.GetNames(typeof(Halloween));
            var eventType = $"{settings.PokeEventType}";
            mg = default;
            form = -1;
            bool match;

            do
            {
                if (settings.PokeEventType == PokeEventType.EventPoke)
                    mg = MysteryGiftRng(settings);

                if ((int)settings.PokeEventType <= 17)
                {
                    var temp = TradeCordPK(Rng.SpeciesRNG);
                    for (int i = 0; i < temp.PersonalInfo.FormCount; i++)
                    {
                        temp.Form = i;
                        var isPresent = PersonalTable.SWSH.GetFormEntry(temp.Species, temp.Form).IsFormWithinRange(i);
                        if (!isPresent)
                            continue;

                        var type1 = GameInfo.Strings.Types[temp.PersonalInfo.Type1];
                        var type2 = GameInfo.Strings.Types[temp.PersonalInfo.Type2];
                        type = type1 == eventType ? type1 : type2 == eventType ? type2 : "";
                        form = type != "" ? temp.Form : -1;
                        if (form != -1)
                            break;
                    }
                }
                else if (settings.PokeEventType == PokeEventType.Halloween)
                {
                    if (Rng.SpeciesRNG == (int)Species.Corsola || Rng.SpeciesRNG == (int)Species.Marowak || Rng.SpeciesRNG == (int)Species.Moltres)
                        form = 1;
                }

                match = settings.PokeEventType switch
                {
                    PokeEventType.Legends => Enum.IsDefined(typeof(Legends), Rng.SpeciesRNG),
                    PokeEventType.RodentLite => RodentLite.Contains(Rng.SpeciesRNG),
                    PokeEventType.ClickbaitArticle => ClickbaitArticle.Contains(Rng.SpeciesRNG),
                    PokeEventType.EventPoke => mg != default,
                    PokeEventType.Babies => enumEggs.Contains(Rng.SpeciesRNG),
                    PokeEventType.Halloween => halloween.Contains(SpeciesName.GetSpeciesNameGeneration(Rng.SpeciesRNG, 2, 8)),
                    _ => type == eventType,
                };
                if (!match)
                    Rng.SpeciesRNG = enumVals[Random.Next(enumVals.Length)];
            }
            while (!match);
        }

        public MysteryGift? MysteryGiftRng(TradeCordSettings settings)
        {
            var mg = EncounterEvent.GetAllEvents().Where(x => x.Species == Rng.SpeciesRNG).ToList();
            mg.RemoveAll(x => x.GetDescription().Count() < 3);
            MysteryGift? mgRng = default;
            if (mg.Count > 0)
            {
                if (Rng.ShinyRNG >= 150 - settings.SquareShinyRate || Rng.ShinyRNG >= 150 - settings.StarShinyRate)
                {
                    var mgSh = mg.FindAll(x => x.IsShiny);
                    mgRng = mgSh.Count > 0 ? mgSh.ElementAt(Random.Next(mgSh.Count)) : mg.ElementAt(Random.Next(mg.Count));
                }
                else mgRng = mg.ElementAt(Random.Next(mg.Count));
            }

            if (mgRng != default && mgRng.Species == (int)Species.Giratina && mgRng.Form > 0)
                mgRng.HeldItem = 112;
            else if (mgRng != default && mgRng.Species == (int)Species.Silvally && mgRng.Form > 0)
                mgRng.HeldItem = SilvallyFormMath(mgRng.Form, 0);

            return mgRng;
        }

        public int SilvallyFormMath(int form, int item) => item > 0 ? item - 903 : item == 0 && form == 0 ? 0 : form + 903;

        public static bool ShinyLockCheck(int species, string ball, bool form)
        {
            if (ShinyLock.Contains(species))
                return true;
            else if (form && (species == (int)Species.Zapdos || species == (int)Species.Moltres || species == (int)Species.Articuno))
                return true;
            else if (ball.Contains("Beast") && (species == (int)Species.Poipole || species == (int)Species.Naganadel))
                return true;
            else return false;
        }

        public string ArrayStringify(Array array)
        {
            int[] newArray = (int[])array;
            var result = "";
            for (int i = 0; i < array.Length; i++)
                result += $"{newArray[i]}{(i + 1 == array.Length ? "" : ",")}";
            return result;
        }

        public bool IsLegendaryOrMythical(string species) => Enum.IsDefined(typeof(Legends), SpeciesName.GetSpeciesID(species));

        public static bool SelfBotScanner(ulong id, int cd)
        {
            if (UserCommandTimestamps.TryGetValue(id, out List<DateTime> timeStamps))
            {
                int[] delta = new int[timeStamps.Count - 1];
                bool[] comp = new bool[delta.Length - 1];

                for (int i = 1; i < timeStamps.Count; i++)
                    delta[i - 1] = (int)(timeStamps[i].Subtract(timeStamps[i - 1]).TotalSeconds - cd);

                for (int i = 1; i < delta.Length; i++)
                    comp[i - 1] = delta[i] == delta[i - 1] || (delta[i] - delta[i - 1] >= -2 && delta[i] - delta[i - 1] <= 2);

                UserCommandTimestamps[id].Clear();
                if (comp.Any(x => x == false))
                    return false;
                else return true;
            }
            return false;
        }
    }
}
