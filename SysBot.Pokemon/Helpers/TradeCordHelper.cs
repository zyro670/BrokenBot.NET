using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SysBot.Pokemon
{
    public class TradeCordHelper
    {
        private readonly TradeCordSettings Settings;
        private int EventPokeForm = -1;
        private MysteryGift? MGRngEvent = default;
        private readonly TradeExtensions.TCRng Rng = TradeExtensions.RandomInit();

        private readonly string[] PartnerPikachuHeadache = { "-Original", "-Partner", "-Hoenn", "-Sinnoh", "-Unova", "-Alola", "-Kalos", "-World" };
        private readonly string[] LGPEBalls = { "Poke", "Premier", "Great", "Ultra", "Master" };
        private static readonly string[] SilvallyMemory = { "", " @ Fighting Memory"," @ Flying Memory", " @ Poison Memory", " @ Ground Memory", " @ Rock Memory",
            " @ Bug Memory", " @ Ghost Memory", " @ Steel Memory", " @ Fire Memory", " @ Water Memory", " @ Grass Memory", " @ Electric Memory", " @ Psychic Memory",
            " @ Ice Memory", " @ Dragon Memory", " @ Dark Memory", " @ Fairy Memory" };
        private static readonly string[] GenesectDrives = { "", " @ Douse Drive", " @ Shock Drive", " @ Burn Drive", " @ Chill Drive" };
        private readonly int[] RodentLite = { 25, 26, 27, 28, 29, 30, 32, 33, 50, 51, 172, 183, 263, 264, 298, 427, 428, 529, 530, 572, 573, 587, 659, 660, 702, 777, 778, 819, 820, 877 };
        private readonly int[] CherishOnly = { 719, 721, 801, 802, 807, 893 };
        private readonly int[] TradeEvo = { (int)Species.Machoke, (int)Species.Haunter, (int)Species.Boldore, (int)Species.Gurdurr, (int)Species.Phantump, (int)Species.Gourgeist };
        private static readonly int[] ShinyLock = { (int)Species.Victini, (int)Species.Keldeo, (int)Species.Volcanion, (int)Species.Cosmog, (int)Species.Cosmoem, (int)Species.Magearna,
                                             (int)Species.Marshadow, (int)Species.Zacian, (int)Species.Zamazenta, (int)Species.Eternatus, (int)Species.Kubfu, (int)Species.Urshifu,
                                             (int)Species.Zarude, (int)Species.Glastrier, (int)Species.Spectrier, (int)Species.Calyrex };

        private readonly int[] UMWormhole = { 144, 145, 146, 150, 244, 245, 249, 380, 382, 384, 480, 481, 482, 484, 487, 488, 644, 645, 646, 642, 717, 793, 795, 796, 797, 799 };
        private readonly int[] USWormhole = { 144, 145, 146, 150, 245, 250, 381, 383, 384, 480, 481, 482, 487, 488, 645, 646, 793, 794, 796, 799, 483, 485, 641, 643, 716, 798 };

        private static readonly List<EvolutionTemplate> Evolutions = EvolutionRequirements();

        public TradeCordHelper(TradeCordSettings settings)
        {
            Settings = settings;
        }

        public sealed class Results
        {
            public string EmbedName { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public bool Success { get; set; }
            public bool FailedCatch { get; set; }
            public PK8 Poke { get; set; } = new();
            public int PokeID { get; set; }
            public PK8 EggPoke { get; set; } = new();
            public int EggPokeID { get; set; }
            public string Item { get; set; } = string.Empty;
            public TradeExtensions.TCUserInfoRoot.TCUserInfo User { get; set; } = new();
            public TradeExtensions.TCUserInfoRoot.TCUserInfo Giftee { get; set; } = new();
        }

        private PK8 TradeCordPK(int species) => (PK8)AutoLegalityWrapper.GetTrainerInfo(8).GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet(SpeciesName.GetSpeciesNameGeneration(species, 2, 8))), out _);

        public Results CatchHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user)
        {
            Results result = new();
            string eggMsg = string.Empty;
            string buddyMsg = string.Empty;

            bool FuncCatch()
            {
                PerkBoostApplicator(user);
                List<string> trainerInfo = new();
                trainerInfo.AddRange(new string[]
                {
                    user.OTName == "" ? "" : $"OT: {user.OTName}",
                    user.OTGender == "" ? "" : $"OTGender: {user.OTGender}",
                    user.TID == 0 ? "" : $"TID: {user.TID}",
                    user.SID == 0 ? "" : $"SID: {user.SID}",
                    user.Language == "" ? "" : $"Language: {user.Language}",
                });

                var buddyAbil = user.Buddy.Ability;
                if (buddyAbil == Ability.FlameBody || buddyAbil == Ability.SteamEngine)
                    Rng.EggRNG += 10;

                int evo1 = 0, evo2 = 0;
                bool egg = Rng.EggRNG >= 100 - Settings.EggRate && CanGenerateEgg(user, out evo1, out evo2);
                if (egg)
                {
                    result.EggPoke = EggProcess(user, string.Join("\n", trainerInfo), evo1, evo2, out eggMsg);
                    if (!(result.EggPoke is PK8) || !new LegalityAnalysis(result.EggPoke).Valid)
                        return false;
                    else
                    {
                        result.EggPoke.ResetPartyStats();
                        if (user.DexCompletionCount < 20)
                            eggMsg += DexCount(user, result.EggPoke.Species, false);
                    }
                }

                DateTime.TryParse(Settings.EventEnd, out DateTime endTime);
                bool ended = endTime != default && DateTime.Now > endTime;
                bool boostProc = user.SpeciesBoost != 0 && Rng.SpeciesBoostRNG >= 99;

                if (Settings.EnableEvent && !ended)
                    EventHandler();
                else if (boostProc)
                    Rng.SpeciesRNG = user.SpeciesBoost;

                if (Rng.CatchRNG >= 100 - Settings.CatchRate)
                {
                    var speciesName = SpeciesName.GetSpeciesNameGeneration(Rng.SpeciesRNG, 2, 8);
                    var mgRng = MGRngEvent == default ? MysteryGiftRng() : MGRngEvent;
                    bool melmetalHack = Rng.SpeciesRNG == (int)Species.Melmetal && Rng.GmaxRNG >= 100 - Settings.GmaxRate;
                    if ((CherishOnly.Contains(Rng.SpeciesRNG) || Rng.CherishRNG >= 100 - Settings.CherishRate || MGRngEvent != default || melmetalHack) && mgRng != default)
                    {
                        Enum.TryParse(user.OTGender, out Gender gender);
                        Enum.TryParse(user.Language, out LanguageID language);
                        var info = user.OTName != "" ? new SimpleTrainerInfo { Gender = (int)gender, Language = (int)language, OT = user.OTName, TID = user.TID, SID = user.SID } : AutoLegalityWrapper.GetTrainerInfo(8);
                        result.Poke = TradeExtensions.CherishHandler(mgRng, info);
                    }

                    if (result.Poke.Species == 0)
                        result.Poke = SetProcess(speciesName, trainerInfo);

                    if (!(result.Poke is PK8) || !new LegalityAnalysis(result.Poke).Valid)
                        return false;

                    result.Poke.ResetPartyStats();
                    result.Message = $"It put up a fight, but you caught {(result.Poke.IsShiny ? $"**{speciesName}**" : $"{speciesName}")}!";
                    if (user.DexCompletionCount < 20)
                        result.Message += DexCount(user, result.Poke.Species, false);
                }

                if (Rng.CatchRNG < 100 - Settings.CatchRate)
                    result.FailedCatch = true;

                if (Rng.ItemRNG >= 100 - Settings.ItemRate)
                {
                    var scRng = new Random((int)result.Poke.EncryptionConstant).Next(1, 4097);
                    TCItems item = TCItems.None;
                    if (scRng > 50)
                    {
                        var vals = Enum.GetValues(typeof(TCItems));
                        do
                        {
                            item = (TCItems)vals.GetValue(new Random().Next(vals.Length));
                        } while (item <= 0);
                    }
                    else item = TCItems.ShinyCharm;

                    result.Item = GetItemString((int)item);
                    var userItem = user.Items.FirstOrDefault(x => x.Item == item);
                    if (userItem == default)
                        user.Items.Add(new TradeExtensions.TCUserInfoRoot.Items() { Item = item, ItemCount = 1 });
                    else userItem.ItemCount++;
                }

                return true;
            }

            result.Success = FuncCatch();
            if (result.Success)
            {
                if (result.Poke.Species != 0)
                {
                    user.CatchCount++;
                    user = TradeCordDump(user, $"{user.UserID}", result.Poke, out int index);
                    result.PokeID = index;
                }

                user = BuddySystem(user, result.Poke, out buddyMsg);
                result.Message += buddyMsg;

                if (result.EggPoke.Species != 0)
                {
                    user.CatchCount++;
                    user = TradeCordDump(user, $"{user.UserID}", result.EggPoke, out int eggIndex);
                    result.EggPokeID = eggIndex;
                    result.Message += eggMsg;
                }

                if (result.Item != string.Empty)
                {
                    bool article = ArticleChoice(result.Item[0]);
                    result.Message += result.FailedCatch ? $"&^&\nAs it fled it dropped {(article ? "an" : "a")} {result.Item}! Added to the items pouch." : $"&^&\nOh? It was holding {(article ? "an" : "a")} {result.Item}! Added to the items pouch.";
                }
            }

            result.User = user;
            result.EmbedName += $"Results{(result.EggPoke.Species != 0 ? "&^&\nEggs" : "")}{(result.Item != string.Empty ? "&^&\nItems" : "")}";
            return result;
        }

        public Results TradeHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncTrade()
            {
                if (!int.TryParse(input, out int id))
                {
                    result.Message = "Please enter a numerical catch ID.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == id && !x.Traded);
                if (match == null)
                {
                    result.Message = "There is no Pokémon with this ID.";
                    return false;
                }

                var dcfavCheck = user.Daycare1.ID == id || user.Daycare2.ID == id || user.Favorites.FirstOrDefault(x => x == id) != default || user.Buddy.ID == id;
                if (dcfavCheck)
                {
                    result.Message = "Please remove your Pokémon from favorites and daycare before trading!";
                    return false;
                }

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                {
                    result.Message = "Oops, something happened when converting your Pokémon!";
                    return false;
                }
                result.Poke = pk;

                var la = new LegalityAnalysis(result.Poke);
                if (!la.Valid || !(result.Poke is PK8))
                {
                    result.Message = "Oops, I cannot trade this Pokémon!";
                    return false;
                }

                match.Traded = true;
                TradeExtensions.TradeCordPath.Add(user.UserID, match.Path);
                result.User = user;
                return true;
            }

            result.Success = FuncTrade();
            return result;
        }

        public Results ListHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();
            result.EmbedName = $"{user.Username}'s List";

            bool FuncList()
            {
                List<string> filters = input.Contains("=") ? input.Split('=').ToList() : new();
                if (filters.Count > 0)
                {
                    filters.RemoveAt(0);
                    input = input.Split('=')[0].Trim();
                }

                for (int i = 0; i < filters.Count; i++)
                    filters[i] = filters[i].ToLower().Trim();

                input = ListNameSanitize(input);
                if (input == "")
                {
                    result.Message = "In order to filter a Pokémon, we need to know which Pokémon to filter.";
                    return false;
                }

                var catches = user.Catches.ToList();
                var ball = filters.FirstOrDefault(x => x != "shiny");
                bool shiny = filters.FirstOrDefault(x => x == "shiny") != default;
                IEnumerable<TradeExtensions.TCUserInfoRoot.Catch> matches = filters.Count switch
                {
                    1 => catches.FindAll(x => (input == "All" ? x.Species != "" : input == "Legendaries" ? Enum.IsDefined(typeof(Legends), SpeciesName.GetSpeciesID(x.Species)) : input == "Egg" ? x.Egg : input == "Shinies" ? x.Shiny : (x.Species == input || (x.Species + x.Form == input) || x.Form.Replace("-", "") == input)) && (ball != default ? ball == x.Ball.ToLower() : x.Shiny) && !x.Traded),
                    2 => catches.FindAll(x => (input == "All" ? x.Species != "" : input == "Legendaries" ? Enum.IsDefined(typeof(Legends), SpeciesName.GetSpeciesID(x.Species)) : input == "Egg" ? x.Egg : x.Species == input || (x.Species + x.Form == input) || x.Form.Replace("-", "") == input) && x.Shiny && ball == x.Ball.ToLower() && !x.Traded),
                    _ => catches.FindAll(x => (input == "All" ? x.Species != "" : input == "Legendaries" ? Enum.IsDefined(typeof(Legends), SpeciesName.GetSpeciesID(x.Species)) : input == "Egg" ? x.Egg : input == "Shinies" ? x.Shiny : x.Ball == input || x.Species == input || (x.Species + x.Form == input) || x.Form.Replace("-", "") == input) && !x.Traded),
                };

                HashSet<string> count = new(), countSh = new();
                if (input == "Shinies")
                {
                    foreach (var result in matches)
                        countSh.Add($"(__{result.ID}__) {result.Species}{result.Form}");
                }
                else
                {
                    foreach (var result in matches)
                    {
                        var speciesString = result.Shiny ? $"(__{result.ID}__) {result.Species}{result.Form}" : $"({result.ID}) {result.Species}{result.Form}";
                        if (result.Shiny)
                            countSh.Add(speciesString);
                        count.Add(speciesString);
                    }
                }

                result.Message = string.Join(", ", input == "Shinies" ? countSh.OrderBy(x => int.Parse(x.Split(' ')[0].Trim(new char[] { '(', '_', ')' }))) : count.OrderBy(x => int.Parse(x.Split(' ')[0].Trim(new char[] { '(', '_', ')' }))));
                if (result.Message == "")
                {
                    result.Message = "No results found.";
                    return false;
                }

                var listName = input == "Shinies" ? "Shiny Pokémon" : input == "All" ? "Pokémon" : input == "Egg" ? "Eggs" : $"{input} List";
                var listCount = input == "Shinies" ? $"★{countSh.Count}" : $"{count.Count}, ★{countSh.Count}";
                result.EmbedName = $"{user.Username}'s {listName} (Total: {listCount})";
                return true;
            }

            result.Success = FuncList();
            return result;
        }

        public Results InfoHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();
            result.EmbedName = $"{user.Username}'s Pokémon Info";

            bool FuncInfo()
            {
                if (!int.TryParse(input, out int id))
                {
                    result.Message = "Please enter a numerical catch ID.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == id && !x.Traded);
                if (match == null)
                {
                    result.Message = "Could not find this ID.";
                    return false;
                }

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                {
                    result.Message = "Oops, something happened when converting your Pokémon!";
                    return false;
                }

                result.Poke = pk;
                result.EmbedName = $"{user.Username}'s {(match.Shiny ? "★" : "")}{match.Species}{match.Form} (ID: {match.ID})";
                return true;
            }

            result.Success = FuncInfo();
            return result;
        }

        public Results MassReleaseHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();
            bool FuncMassRelease()
            {
                IEnumerable<TradeExtensions.TCUserInfoRoot.Catch> matches;
                var list = user.Catches.ToList();
                var ballStr = input != "" ? input.Substring(0, 1).ToUpper() + input[1..].ToLower() : "None";
                bool ballRelease = Enum.TryParse(ballStr, out Ball ball);

                if (ballRelease && ball != Ball.None)
                    matches = list.FindAll(x => !x.Traded && !x.Shiny && x.Ball == ball.ToString() && x.Species != "Ditto" && x.ID != user.Daycare1.ID && x.ID != user.Daycare2.ID && user.Favorites.FirstOrDefault(z => z == x.ID) == default && x.ID != user.Buddy.ID);
                else if (input.ToLower() == "shiny")
                    matches = list.FindAll(x => !x.Traded && x.Shiny && x.Ball != "Cherish" && x.Species != "Ditto" && x.ID != user.Daycare1.ID && x.ID != user.Daycare2.ID && user.Favorites.FirstOrDefault(z => z == x.ID) == default && x.ID != user.Buddy.ID);
                else if (input != "")
                {
                    input = ListNameSanitize(input);
                    matches = list.FindAll(x => !x.Traded && !x.Shiny && x.Ball != "Cherish" && $"{x.Species}{x.Form}".Equals(input) && x.ID != user.Daycare1.ID && x.ID != user.Daycare2.ID && user.Favorites.FirstOrDefault(z => z == x.ID) == default && x.ID != user.Buddy.ID);
                }
                else matches = list.FindAll(x => !x.Traded && !x.Shiny && x.Ball != "Cherish" && x.Species != "Ditto" && x.ID != user.Daycare1.ID && x.ID != user.Daycare2.ID && user.Favorites.FirstOrDefault(z => z == x.ID) == default && x.ID != user.Buddy.ID);

                if (matches.Count() == 0)
                {
                    result.Message = input == "" ? "Cannot find any more non-shiny, non-Ditto, non-favorite, non-event, non-buddy Pokémon to release." : "Cannot find anything that could be released with the specified criteria.";
                    return false;
                }

                foreach (var val in matches)
                {
                    File.Delete(val.Path);
                    user.Catches.Remove(val);
                }

                if (ballRelease && ball != Ball.None)
                    input = $"Pokémon in {ball} Ball";

                result.User = user;
                result.Message = input == "" ? "Every non-shiny Pokémon was released, excluding Ditto, favorites, events, buddy, and those in daycare." : $"Every {(input.ToLower() == "shiny" ? "shiny Pokémon" : ballStr == "Cherish" ? "non-shiny event Pokémon" : $"non-shiny {input}")} was released, excluding favorites, buddy{(ballStr == "Cherish" ? "" : ", events,")} and those in daycare.";
                return true;
            }

            result.Success = FuncMassRelease();
            return result;
        }

        public Results ReleaseHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();
            bool FuncRelease()
            {
                if (!int.TryParse(input, out int id))
                {
                    result.Message = "Please enter a numerical catch ID.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == id && !x.Traded);
                if (match == null)
                {
                    result.Message = "Cannot find this Pokémon.";
                    return false;
                }

                if (user.Daycare1.ID == id || user.Daycare2.ID == id || user.Favorites.FirstOrDefault(x => x == id) != default || user.Buddy.ID == id)
                {
                    result.Message = "Cannot release a Pokémon in daycare, favorites, or if it's your buddy.";
                    return false;
                }

                result.Message = $"You release your {(match.Shiny ? "★" : "")}{match.Species}{match.Form}.";
                File.Delete(match.Path);
                user.Catches.Remove(match);
                result.User = user;
                return true;
            }

            result.Success = FuncRelease();
            return result;
        }

        public Results DaycareInfoHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user)
        {
            Results result = new();
            if (user.Daycare1.ID == 0 && user.Daycare2.ID == 0)
                result.Message = "You do not have anything in daycare.";
            else
            {
                var dcSpecies1 = user.Daycare1.ID == 0 ? "" : $"(ID: {user.Daycare1.ID}) {(user.Daycare1.Shiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare1.Species, 2, 8)}{user.Daycare1.Form} ({(Ball)user.Daycare1.Ball})";
                var dcSpecies2 = user.Daycare2.ID == 0 ? "" : $"(ID: {user.Daycare2.ID}) {(user.Daycare2.Shiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare2.Species, 2, 8)}{user.Daycare2.Form} ({(Ball)user.Daycare2.Ball})";

                if (user.Daycare1.ID != 0 && user.Daycare2.ID != 0)
                    result.Message = $"{dcSpecies1}\n{dcSpecies2}{(CanGenerateEgg(user, out _, out _) ? "\n\nThey seem to really like each other." : "\n\nThey don't really seem to be fond of each other. Make sure they're of the same evolution tree and can be eggs!")}";
                else if (user.Daycare1.ID == 0 || user.Daycare2.ID == 0)
                    result.Message = $"{(user.Daycare1.ID == 0 ? dcSpecies2 : dcSpecies1)}\n\nIt seems lonely.";
            }

            result.Success = true;
            return result;
        }

        public Results DaycareHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string action, string id)
        {
            Results result = new();
            result.EmbedName = $"{user.Username}'s Daycare";
            bool deposit = false;
            bool withdraw = false;

            bool FuncDaycare()
            {
                id = id.ToLower();
                action = action.ToLower();
                if (!int.TryParse(id, out int _id) && id != "all")
                {
                    result.Message = "Please enter a numerical catch ID.";
                    return false;
                }

                string speciesString = string.Empty;
                deposit = action == "d" || action == "deposit";
                withdraw = action == "w" || action == "withdraw";
                var match = deposit ? user.Catches.FirstOrDefault(x => x.ID == _id && !x.Traded) : null;
                if (deposit && match == null)
                {
                    result.Message = "There is no Pokémon with this ID.";
                    return false;
                }

                if (withdraw)
                {
                    if (user.Daycare1.ID == 0 && user.Daycare2.ID == 0)
                    {
                        result.Message = "You do not have anything in daycare.";
                        return false;
                    }

                    if (id != "all")
                    {
                        if (user.Daycare1.ID.Equals(int.Parse(id)))
                        {
                            speciesString = $"(ID: {user.Daycare1.ID}) {(user.Daycare1.Shiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare1.Species, 2, 8)}{user.Daycare1.Form}";
                            user.Daycare1 = new();
                        }
                        else if (user.Daycare2.ID.Equals(int.Parse(id)))
                        {
                            speciesString = $"(ID: {user.Daycare2.ID}) {(user.Daycare2.Shiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare2.Species, 2, 8)}{user.Daycare2.Form}";
                            user.Daycare2 = new();
                        }
                        else
                        {
                            result.Message = "You do not have that Pokémon in daycare.";
                            return false;
                        }
                    }
                    else
                    {
                        bool fullDC = user.Daycare1.ID != 0 && user.Daycare2.ID != 0;
                        speciesString = !fullDC ? $"(ID: {(user.Daycare1.ID != 0 ? user.Daycare1.ID : user.Daycare2.ID)}) {(user.Daycare1.ID != 0 && user.Daycare1.Shiny ? "★" : user.Daycare2.ID != 0 && user.Daycare2.Shiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare1.ID != 0 ? user.Daycare1.Species : user.Daycare2.Species, 2, 8)}{(user.Daycare1.ID != 0 ? user.Daycare1.Form : user.Daycare2.Form)}" :
                            $"(ID: {user.Daycare1.ID}) {(user.Daycare1.Shiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare1.Species, 2, 8)}{user.Daycare1.Form} and (ID: {user.Daycare2.ID}) {(user.Daycare2.Shiny ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare2.Species, 2, 8)}{user.Daycare2.Form}";
                        user.Daycare1 = new();
                        user.Daycare2 = new();
                    }
                }
                else if (deposit && match != null)
                {
                    if (user.Daycare1.ID != 0 && user.Daycare2.ID != 0)
                    {
                        result.Message = "Daycare full, please withdraw something first.";
                        return false;
                    }

                    var speciesStr = string.Join("", match.Species.Split('-', ' ', '’', '.'));
                    speciesStr += match.Path.Contains("Nidoran-M") ? "M" : match.Path.Contains("Nidoran-F") ? "F" : "";
                    Enum.TryParse(match.Ball, out Ball ball);
                    Enum.TryParse(speciesStr, out Species species);
                    if ((user.Daycare1.ID == 0 && user.Daycare2.ID == 0) || (user.Daycare1.ID == 0 && user.Daycare2.ID != int.Parse(id)))
                        user.Daycare1 = new() { Ball = (int)ball, Form = match.Form, ID = match.ID, Shiny = match.Shiny, Species = (int)species };
                    else if (user.Daycare2.ID == 0 && user.Daycare1.ID != int.Parse(id))
                        user.Daycare2 = new() { Ball = (int)ball, Form = match.Form, ID = match.ID, Shiny = match.Shiny, Species = (int)species };
                    else
                    {
                        result.Message = "You've already deposited that Pokémon to daycare.";
                        return false;
                    }
                }
                else
                {
                    result.Message = "Invalid command.";
                    return false;
                }

                result.EmbedName = $"{(deposit ? " Deposit" : " Withdraw")}";
                result.User = user;
                result.Message = deposit && match != null ? $"Deposited your {(match.Shiny ? "★" : "")}{match.Species}{match.Form}({match.Ball}) to daycare!" : $"You withdrew your {speciesString} from the daycare.";
                return true;
            }

            result.Success = FuncDaycare();
            return result;
        }

        public Results GiftHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, TradeExtensions.TCUserInfoRoot.TCUserInfo m_user, string input)
        {
            Results result = new();
            bool FuncGift()
            {
                if (!int.TryParse(input, out int id))
                {
                    result.Message = "Please enter a numerical catch ID.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == id && !x.Traded);
                var dir = Path.Combine("TradeCord", $"{m_user.UserID}");
                if (match == null)
                {
                    result.Message = "Cannot find this Pokémon.";
                    return false;
                }
                else if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var dcfavCheck = user.Daycare1.ID == id || user.Daycare2.ID == id || user.Favorites.FirstOrDefault(x => x == id) != default || user.Buddy.ID == id;
                if (dcfavCheck)
                {
                    result.Message = "Please remove your Pokémon from favorites and daycare before gifting!";
                    return false;
                }

                HashSet<int> newIDParse = new();
                foreach (var caught in m_user.Catches)
                    newIDParse.Add(caught.ID);

                var newID = Indexing(newIDParse.OrderBy(x => x).ToArray());
                var newPath = $"{dir}\\{match.Path.Split('\\')[2].Replace(match.ID.ToString(), newID.ToString())}";
                File.Move(match.Path, newPath);
                user.Catches.Remove(match);

                m_user.Catches.Add(new() { Ball = match.Ball, Egg = match.Egg, Form = match.Form, ID = newID, Shiny = match.Shiny, Species = match.Species, Path = newPath, Traded = false });
                var specID = SpeciesName.GetSpeciesID(match.Species);
                var dex = (int[])Enum.GetValues(typeof(Gen8Dex));
                var missingEntries = GetMissingDexEntries(dex, m_user).Count;

                result.Message = $"You gifted your {(match.Shiny ? "★" : "")}{match.Species}{match.Form} to {m_user.Username}. New ID is {newID}.";
                if (m_user.DexCompletionCount == 0 || (m_user.DexCompletionCount < 20 && missingEntries <= 50))
                    result.Message += DexCount(m_user, specID, true);

                result.User = user;
                result.Giftee = m_user;
                return true;
            }

            result.Success = FuncGift();
            return result;
        }

        public Results TrainerInfoSetHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string[] input)
        {
            Results result = new();
            user.OTName = input[0];
            user.OTGender = input[1];
            user.TID = int.Parse(input[2]);
            user.SID = int.Parse(input[3]);
            user.Language = input[4];

            result.Message = $"\nYour trainer info was set to the following: \n**OT:** {user.OTName}\n**OTGender:** {user.OTGender}\n**TID:** {user.TID}\n**SID:** {user.SID}\n**Language:** {user.Language}";
            result.Success = true;
            result.User = user;
            return result;
        }

        public Results TrainerInfoHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user)
        {
            Results result = new();
            result.Success = true;
            var sc = user.Items.FirstOrDefault(x => x.Item == TCItems.ShinyCharm);
            var count = sc == default ? 0 : sc.ItemCount;
            result.Message = $"\n**OT:** {(user.OTName == "" ? "Not set." : user.OTName)}" +
                             $"\n**OTGender:** {(user.OTGender == "" ? "Not set." : user.OTGender)}" +
                             $"\n**TID:** {(user.TID == 0 ? "Not set." : user.TID)}" +
                             $"\n**SID:** {(user.SID == 0 ? "Not set." : user.SID)}" +
                             $"\n**Language:** {(user.Language == "" ? "Not set." : user.Language)}" +
                             $"\n**Shiny Charm:** {count}" +
                             $"\n**UTC Time Offset:** {user.TimeZoneOffset}";
            return result;
        }

        public Results FavoritesInfoHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user)
        {
            Results result = new();
            bool FuncFavoritesInfo()
            {
                if (user.Favorites.Count == 0)
                {
                    result.Message = "You don't have anything in favorites yet!";
                    return false;
                }

                List<string> names = new();
                foreach (var fav in user.Favorites)
                {
                    var match = user.Catches.FirstOrDefault(x => x.ID == fav);
                    names.Add(match.Shiny ? $"(__{match.ID}__) {match.Species}{match.Form}" : $"({match.ID}) {match.Species}{match.Form}");
                }

                result.Message = string.Join(", ", names.OrderBy(x => int.Parse(x.Split(' ')[0].Trim(new char[] { '(', '_', ')' }))));
                return true;
            }

            result.Success = FuncFavoritesInfo();
            return result;
        }

        public Results FavoritesHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();
            result.EmbedName = $"{user.Username}'s Favorite";

            bool FuncFavorites()
            {
                var arg = input.ToLower();
                if (!int.TryParse(input, out int id) && arg != "clear")
                {
                    result.Message = "Please enter a numerical catch ID.";
                    return false;
                }

                if (arg == "clear")
                {
                    user.Favorites.Clear();
                    result.Message = $"{user.Username}, all of your favorites were cleared!";
                    result.EmbedName += " Clear";
                    result.User = user;
                    return true;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == id && !x.Traded);
                if (match == null)
                {
                    result.Message = "Cannot find this Pokémon.";
                    return false;
                }

                var fav = user.Favorites.FirstOrDefault(x => x == id);
                if (fav == default)
                {
                    user.Favorites.Add(id);
                    result.Message = $"{user.Username}, added your {(match.Shiny ? "★" : "")}{match.Species}{match.Form} to favorites!";
                    result.EmbedName += " Addition";
                }
                else if (fav == id)
                {
                    user.Favorites.Remove(fav);
                    result.Message = $"{user.Username}, removed your {(match.Shiny ? "★" : "")}{match.Species}{match.Form} from favorites!";
                    result.EmbedName += " Removal";
                }

                result.User = user;
                return true;
            }

            result.Success = FuncFavorites();
            return result;
        }

        public Results DexHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();
            var entries = (int[])Enum.GetValues(typeof(Gen8Dex));
            var speciesBoost = user.SpeciesBoost != 0 ? $"\n**Pokémon Boost:** {SpeciesName.GetSpeciesNameGeneration(user.SpeciesBoost, 2, 8)}" : "\n**Pokémon Boost:** N/A";

            if (input == "missing")
            {
                List<string> missing = GetMissingDexEntries(entries, user);
                result.Message = string.Join(", ", missing.OrderBy(x => x));
                result.Success = true;
                return result;
            }

            result.User = user;
            result.Message = $"\n**Pokédex:** {user.Dex.Count}/{entries.Length}\n**Level:** {user.DexCompletionCount + user.ActivePerks.Count}{speciesBoost}";
            result.Success = true;
            return result;
        }

        public Results PerkHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncDexPerks()
            {
                if (input == "" && (user.DexCompletionCount > 0 || user.ActivePerks.Count > 0))
                {
                    result.Message = $"**CatchBoost:** {user.ActivePerks.FindAll(x => x == DexPerks.CatchBoost).Count}\n" +
                                     $"**ItemBoost:** {user.ActivePerks.FindAll(x => x == DexPerks.ItemBoost).Count}\n" +
                                     $"**SpeciesBoost:** {user.ActivePerks.FindAll(x => x == DexPerks.SpeciesBoost).Count}\n" +
                                     $"**GmaxBoost:** {user.ActivePerks.FindAll(x => x == DexPerks.GmaxBoost).Count}\n" +
                                     $"**CherishBoost:** {user.ActivePerks.FindAll(x => x == DexPerks.CherishBoost).Count}";
                    return true;
                }
                else if (input == "clear")
                {
                    user.DexCompletionCount += user.ActivePerks.Count;
                    user.ActivePerks = new();
                    user.SpeciesBoost = 0;
                    result.Message = "All active perks cleared!";
                    return true;
                }

                if (user.DexCompletionCount == 0)
                {
                    result.Message = "No perks available. Unassign a perk or complete the Dex to get more!";
                    return false;
                }

                string[] perk = input.Split(',', ' ');
                if (!int.TryParse(perk[1], out int count))
                {
                    result.Message = "Incorrect input, could not parse perk point amount.";
                    return false;
                }
                else if (count > user.DexCompletionCount)
                {
                    result.Message = "Not enough points available to assign all requested perks.";
                    return false;
                }
                else if (count == 0)
                {
                    result.Message = "Please enter a non-zero amount";
                    return false;
                }

                if (!Enum.TryParse(perk[0], true, out DexPerks perkVal) || perkVal == DexPerks.ShinyBoost || perkVal == DexPerks.EggRateBoost)
                {
                    result.Message = "Perk name was not recognized.";
                    return false;
                }

                var activeCount = user.ActivePerks.FindAll(x => x == perkVal).Count;
                if (activeCount + count > 5)
                    count = 5 - activeCount;

                if (count == 0)
                {
                    result.Message = "Perk is already maxed out.";
                    return false;
                }

                for (int i = 0; i < count; i++)
                {
                    user.ActivePerks.Add(perkVal);
                    user.DexCompletionCount -= 1;
                }

                result.Message = $"{(count > 1 ? $"Added {count} perk {(count > 1 ? "points" : "point")} to {perkVal}!" : $"{perkVal} perk added!")}";
                return true;
            }

            result.Success = FuncDexPerks();
            result.User = user;
            return result;
        }

        public Results SpeciesBoostHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncSpeciesBoost()
            {
                if (!user.ActivePerks.Contains(DexPerks.SpeciesBoost))
                {
                    result.Message = "SpeciesBoost perk isn't active.";
                    return false;
                }

                input = ListNameSanitize(input).Replace("'", "").Replace("-", "").Replace(" ", "").Replace(".", "");
                if (!Enum.TryParse(input, out Gen8Dex species))
                {
                    result.Message = "Entered species was not recognized.";
                    return false;
                }

                user.SpeciesBoost = (int)species;
                result.User = user;
                result.Message = $"Catch chance for {species} was slightly boosted!";
                return true;
            }

            result.Success = FuncSpeciesBoost();
            return result;
        }

        public Results BuddyHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncBuddy()
            {
                int id = 0;
                if (input == "remove" && user.Buddy.ID != 0)
                {
                    user.Buddy = new();
                    result.Message = "Buddy removed!";
                    return true;
                }
                else if (input != string.Empty && !int.TryParse(input, out id))
                {
                    result.Message = "Please enter a numerical catch ID.";
                    return false;
                }
                else if (input == string.Empty && user.Buddy.ID == 0)
                {
                    result.Message = "You don't have an active buddy.";
                    return false;
                }
                else if (id == user.Buddy.ID)
                {
                    result.Message = "This is already your buddy!";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == (input != string.Empty ? id : user.Buddy.ID) && !x.Traded);
                if (match == null)
                {
                    result.Message = "Could not find this Pokémon.";
                    return false;
                }

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                {
                    result.Message = "Oops, something happened when converting your Pokémon!";
                    return false;
                }

                result.Poke = pk;
                if (input == string.Empty)
                {
                    result.EmbedName = $"{user.Username}'s {(match.Shiny ? "★" : "")}{(pk.IsNicknamed ? $"{user.Buddy.Nickname}" : $"{match.Species}{match.Form}")}";
                    result.EmbedName += $" (ID: {match.ID})";
                    return true;
                }
                else
                {
                    user.Buddy = new()
                    {
                        ID = id,
                        Nickname = pk.Nickname,
                        Ability = (Ability)pk.Ability,
                    };

                    result.Message = $"Set your {(match.Shiny ? "★" : "")}{(pk.IsNicknamed ? $"{user.Buddy.Nickname}" : $"{match.Species}{match.Form}")} as your new buddy!";
                    return true;
                }
            }

            result.Success = FuncBuddy();
            result.User = user;
            return result;
        }

        public Results NicknameHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncNickname()
            {
                if (user.Buddy.ID == 0)
                {
                    result.Message = "You don't have an active buddy!";
                    return false;
                }               
                else if (WordFilter.IsFiltered(input, out _))
                {
                    result.Message = "Nickname triggered the word filter. Please choose a different nickname.";
                    return false;
                }
                else if (input.Length > 12)
                {
                    result.Message = "Nickname is too long.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == user.Buddy.ID && !x.Traded);
                if (match == null)
                {
                    result.Message = "Could not find this Pokémon.";
                    return false;
                }

                if (match.Egg)
                {
                    result.Message = "Cannot nickname eggs.";
                    return false;
                }

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                {
                    result.Message = "Oops, something happened when converting your Pokémon!";
                    return false;
                }

                bool clear = input.ToLower() == "clear";
                if (clear)
                    pk.ClearNickname();
                else pk.SetNickname(input);

                var la = new LegalityAnalysis(pk);
                if (!la.Valid)
                {
                    result.Message = "Nickname is not valid.";
                    return false;
                }

                File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                user.Buddy.Nickname = clear ? pk.Nickname : input;
                result.User = user;
                result.Message = clear ? "Your buddy's nickname was cleared!" : "Your buddy's nickname was updated!";
                return true;
            }

            result.Success = FuncNickname();
            return result;
        }

        public Results EvolutionHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncEvolution()
            {
                if (user.Buddy.ID == 0)
                {
                    result.Message = "You don't have an active buddy.";
                    return false;
                }

                var item = TradeExtensions.EnumParse<TCItems>(input);
                AlcremieForms alcremie = AlcremieForms.None;
                if (input != "" && item <= 0)
                {
                    alcremie = TradeExtensions.EnumParse<AlcremieForms>(input);
                    if (alcremie == AlcremieForms.None)
                    {
                        result.Message = "Unable to parse input.";
                        return false;
                    }
                }

                var itemMatch = user.Items.FirstOrDefault(x => x.Item == item && x.ItemCount > 0);
                if (input != "" && itemMatch == default && alcremie == AlcremieForms.None)
                {
                    result.Message = "You do not have this item.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == user.Buddy.ID && !x.Traded);
                if (match == null)
                {
                    result.Message = "Could not find this Pokémon.";
                    return false;
                }
                else if (match.Egg)
                {
                    result.Message = "Eggs cannot evolve.";
                    return false;
                }

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                {
                    result.Message = "Oops, something happened when converting your Pokémon!";
                    return false;
                }

                var oldName = pk.IsNicknamed ? pk.Nickname : $"{SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8)}{TradeExtensions.FormOutput(pk.Species, pk.Form, out _)}";
                var timeStr = TimeOfDayString(user.TimeZoneOffset, false);
                var tod = TradeExtensions.EnumParse<TimeOfDay>(timeStr);
                if (tod == TimeOfDay.Dawn)
                    tod = TimeOfDay.Morning;

                if (!EvolvePK(pk, item, alcremie, tod, out string message, out PK8? shedinja))
                {
                    result.Message = message;
                    return false;
                }

                if (itemMatch != default)
                {
                    user.Items.Remove(itemMatch);
                    user.Items.Add(new() { Item = itemMatch.Item, ItemCount = itemMatch.ItemCount - 1 });
                }

                user.Catches.Remove(match);
                user.Catches.Add(new()
                {
                    Ball = match.Ball,
                    Egg = match.Egg,
                    Form = TradeExtensions.FormOutput(pk.Species, pk.Form, out _),
                    Species = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8),
                    Path = match.Path,
                    ID = match.ID,
                    Shiny = match.Shiny,
                    Traded = match.Traded,
                });

                result.Message += DexCount(user, pk.Species, false);
                string shedStr = string.Empty;
                if (shedinja != null)
                {
                    user = TradeCordDump(user, $"{user.UserID}", shedinja, out int id);
                    shedStr = $"\n\nA spare Poké Ball in your bag clicks quietly... You also caught {(shedinja.IsShiny ? "**Shedinja**" : "Shedinja")} (ID: {id})!";
                    shedStr += DexCount(user, shedinja.Species, false);
                }

                user.Buddy.Ability = (Ability)pk.Ability;
                user.Buddy.Nickname = pk.Nickname;
                File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                var speciesStr = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8) + TradeExtensions.FormOutput(pk.Species, pk.Form, out _);
                result.Message = $"{oldName} evolved into {(pk.IsShiny ? $"**{speciesStr}**" : speciesStr)}!{shedStr}";
                result.Poke = pk;
                return true;
            }

            result.Success = FuncEvolution();
            result.User = user;
            return result;
        }

        public Results GiveItemHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncGiveItem()
            {
                if (user.Buddy.ID == 0)
                {
                    result.Message = "You don't have an active buddy to give an item to.";
                    return false;
                }

                var item = TradeExtensions.EnumParse<TCItems>(input);
                var userItem = user.Items.FirstOrDefault(x => x.Item == item);
                if (userItem == default || userItem.ItemCount == 0)
                {
                    result.Message = "You do not have this item.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == user.Buddy.ID && !x.Traded);
                if (match == null)
                {
                    result.Message = "Could not find this Pokémon.";
                    return false;
                }

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                {
                    result.Message = "Oops, something happened when converting your Pokémon!";
                    return false;
                }

                if (pk.HeldItem != 0)
                {
                    var itemCheck = (TCItems)pk.HeldItem;
                    if (itemCheck > 0)
                    {
                        var heldItem = user.Items.FirstOrDefault(x => x.Item == itemCheck);
                        if (heldItem == default)
                            user.Items.Add(new() { Item = itemCheck, ItemCount = 1 });
                        else heldItem.ItemCount++;
                    }
                }

                pk.HeldItem = (int)item;
                userItem.ItemCount--;
                result.User = user;

                var itemStr = GetItemString((int)item);
                result.Message = $"You gave {(ArticleChoice(itemStr[0]) ? "an" : "a")} {itemStr} to your buddy!";
                File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                return true;
            }

            result.Success = FuncGiveItem();
            return result;
        }

        public Results GiftItemHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, TradeExtensions.TCUserInfoRoot.TCUserInfo m_user, string input, string countInput)
        {
            Results result = new();

            bool FuncGiftItem()
            {
                var item = TradeExtensions.EnumParse<TCItems>(input);
                var userItem = user.Items.FirstOrDefault(x => x.Item == item);
                var count = int.Parse(countInput);
                if (userItem == default || userItem.ItemCount == 0)
                {
                    result.Message = "You do not have this item.";
                    return false;
                }
                else if (count > userItem.ItemCount)
                {
                    result.Message = "You do not have enough of this item.";
                    return false;
                }

                userItem.ItemCount -= count;
                var gifteeItem = m_user.Items.FirstOrDefault(x => x.Item == item);
                if (gifteeItem == default)
                    m_user.Items.Add(new() { Item = item, ItemCount = count });
                else gifteeItem.ItemCount += count;

                var itemStr = GetItemString((int)item);
                result.Message = $"You gifted {count} {itemStr}{(count == 1 ? "" : "s")} to {m_user.Username}!";
                result.User = user;
                result.Giftee = m_user;
                return true;
            }

            result.Success = FuncGiftItem();
            return result;
        }

        public Results TakeItemHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user)
        {
            Results result = new();

            bool FuncTakeItem()
            {
                if (user.Buddy.ID == 0)
                {
                    result.Message = "You don't have an active buddy to give an item to.";
                    return false;
                }

                var match = user.Catches.FirstOrDefault(x => x.ID == user.Buddy.ID && !x.Traded);
                if (match == null)
                {
                    result.Message = "Could not find this Pokémon.";
                    return false;
                }

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                {
                    result.Message = "Oops, something happened when converting your Pokémon!";
                    return false;
                }

                var item = (TCItems)pk.HeldItem;
                if (item <= 0)
                {
                    result.Message = "Oops, this item is not yet available!";
                    return false;
                }

                var heldItem = user.Items.FirstOrDefault(x => x.Item == item);
                if (heldItem == default)
                    user.Items.Add(new() { Item = item, ItemCount = 1 });
                else heldItem.ItemCount++;

                var itemStr = GetItemString(pk.HeldItem);
                result.Message = $"You took {(ArticleChoice(itemStr[0]) ? "an" : "a")} {itemStr} from your buddy!";

                result.User = user;
                pk.HeldItem = 0;
                File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                return true;
            }

            result.Success = FuncTakeItem();
            return result;
        }

        public Results ItemListHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncItemList()
            {
                var item = TradeExtensions.EnumParse<TCItems>(input);
                if (input != "all" && item <= 0)
                {
                    result.Message = input == "" ? "Nothing to search for." : "Unrecognized item.";
                    return false;
                }

                var list = user.Items.ToList();
                List<TradeExtensions.TCUserInfoRoot.Items> items = input switch
                {
                    "all" => list.FindAll(x => x.ItemCount > 0),
                    _ => list.FindAll(x => x.Item == item && x.ItemCount > 0),
                };

                if (items.Count == 0)
                {
                    result.Message = "Nothing found that meets the search criteria, or you have no items left.";
                    return false;
                }

                string content = string.Empty;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Item <= 0)
                        continue;

                    var name = GetItemString((int)items[i].Item);
                    content += $"**{name}**: {items[i].ItemCount}{(i + 1 < items.Count ? " | " : "")}";
                }

                result.EmbedName = item <= 0 ? $"{user.Username}'s Item List" : $"{user.Username}'s {GetItemString((int)item)} List";
                result.Message = content;
                return true;
            }

            result.Success = FuncItemList();
            return result;
        }

        public Results ItemDropHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncItemDrop()
            {
                var item = TradeExtensions.EnumParse<TCItems>(input);
                if (input != "all" && item <= 0)
                {
                    result.Message = input == "" ? "Nothing specified to drop." : "Unrecognized item.";
                    return false;
                }

                var list = user.Items.ToList();
                IEnumerable<TradeExtensions.TCUserInfoRoot.Items> items = input switch
                {
                    "all" => list.FindAll(x => x.ItemCount >= 0),
                    _ => list.FindAll(x => x.Item == item && x.ItemCount > 0),
                };

                var count = items.Count();
                if (count == 0)
                {
                    result.Message = "Nothing found that meets the search criteria, or you have no items.";
                    return false;
                }

                foreach (var entry in items)
                    user.Items.Remove(entry);

                result.User = user;
                result.Message = count > 1 ? "Dropped all items!" : $"Dropped all {GetItemString((int)item)}{(items.First().ItemCount > 1 ? "s" : "")}!";
                return true;
            }

            result.Success = FuncItemDrop();
            return result;
        }

        public Results TimeZoneHandler(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string input)
        {
            Results result = new();

            bool FuncTimeZone()
            {
                if (!int.TryParse(input, out int offset))
                {
                    result.Message = "Input must be a number (i.e. -2, 5...), or a zero.";
                    return false;
                }
                else if (offset < -12 || offset > 14)
                {
                    result.Message = "Invalid UTC time offset.";
                    return false;
                }

                result.Message = $"UTC time offset set to **{offset}**. Your current time should be **{DateTime.UtcNow.AddHours(offset)}**.";
                user.TimeZoneOffset = offset;
                result.User = user;
                return true;
            }

            result.Success = FuncTimeZone();
            return result;
        }

        private TradeExtensions.TCUserInfoRoot.TCUserInfo BuddySystem(TradeExtensions.TCUserInfoRoot.TCUserInfo user, PK8 pk8, out string buddyMsg)
        {
            buddyMsg = string.Empty;
            if (user.Buddy.ID != 0)
            {
                var match = user.Catches.FirstOrDefault(x => x.ID == user.Buddy.ID && !x.Traded);
                if (match == null)
                    return user;

                var pk = (PK8?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(match.Path));
                if (pk == null)
                    return user;

                if (pk8.IsShiny && !pk.IsEgg && pk.CurrentFriendship + 5 <= 255)
                    pk.CurrentFriendship += 5;

                if (pk.IsEgg)
                {
                    double status = pk.CurrentFriendship / (double)pk.PersonalInfo.HatchCycles;
                    if (status < 1.0)
                    {
                        pk.CurrentFriendship -= 5;
                        File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                        return user;
                    }
                    else
                    {
                        CommonEdits.ForceHatchPKM(pk);
                        pk.CurrentFriendship = pk.PersonalInfo.BaseFriendship;
                        File.WriteAllBytes(match.Path, pk.DecryptedPartyData);

                        buddyMsg = "\nUh-oh!... You've just hatched an egg!";
                        user.Buddy.Nickname = pk.Nickname;
                        user.Catches.Remove(match);
                        user.Catches.Add(new()
                        {
                            Ball = match.Ball,
                            Egg = false,
                            Form = match.Form,
                            ID = match.ID,
                            Path = match.Path,
                            Shiny = match.Shiny,
                            Species = match.Species,
                            Traded = false,
                        });
                    }
                }
                else if (pk.CurrentLevel < 100 && pk8.Species != 0)
                {
                    var levelOld = pk.CurrentLevel;
                    var xpMin = Experience.GetEXP(pk.CurrentLevel + 1, pk.PersonalInfo.EXPGrowth);
                    var xpGet = (uint)Math.Round(Math.Pow(pk8.CurrentLevel / 5.0 * ((2.0 * pk8.CurrentLevel + 10.0) / (pk8.CurrentLevel + pk.CurrentLevel + 10.0)), 2.5) * (pk.OT_Name == user.OTName ? 1.0 : 1.5) * (pk8.IsShiny ? 1.3 : 1.0), 0, MidpointRounding.AwayFromZero);
                    if (xpGet < 100)
                        xpGet = 175;

                    pk.EXP += xpGet;
                    while (pk.EXP >= Experience.GetEXP(pk.CurrentLevel + 1, pk.PersonalInfo.EXPGrowth) && pk.CurrentLevel < 100)
                        pk.CurrentLevel++;

                    if (pk.CurrentLevel == 100)
                        pk.EXP = xpMin;

                    if (pk.EXP >= xpMin)
                    {
                        buddyMsg = $"\n{user.Buddy.Nickname} gained {xpGet} EXP and leveled up to level {pk.CurrentLevel}!";
                        if (pk.CurrentFriendship < 255)
                        {
                            var delta = pk.CurrentLevel - levelOld;
                            for (int i = 0; i < delta; i++)
                                pk.CurrentFriendship += 2;
                        }
                    }
                    else buddyMsg = $"\n{user.Buddy.Nickname} gained {xpGet} EXP!";

                    if (pk.CurrentFriendship >= 255)
                        pk.CurrentFriendship = 255;

                    File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                }
            }
            return user;
        }

        public bool EvolvePK(PK8 pk, TCItems item, AlcremieForms alcremie, TimeOfDay tod, out string msg, out PK8? shedinja)
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
            bool heldUsed = false;
            if (item <= 0 && heldItem > 0)
            {
                heldUsed = true;
                item = heldItem;
            }

            var evoList = Evolutions.FindAll(x => x.Species == pk.Species && x.Item == (alcremie != AlcremieForms.None ? TCItems.Sweets : item));
            if (evoList.Count == 0)
            {
                msg = "No evolution results found for this Pokémon or criteria not met.";
                return false;
            }

            EvolutionTemplate result = pk.Species switch
            {
                (int)Species.Tyrogue => pk.Stat_ATK == pk.Stat_DEF ? evoList.Find(x => x.EvoType == EvolutionType.LevelUpAeqD) : pk.Stat_ATK > pk.Stat_DEF ? evoList.Find(x => x.EvoType == EvolutionType.LevelUpATK) : evoList.Find(x => x.EvoType == EvolutionType.LevelUpDEF),
                (int)Species.Eevee => item > 0 ? evoList.First() : pk.CurrentFriendship >= 250 ? evoList.Find(x => x.EvoType == EvolutionType.LevelUpAffection50MoveType) : evoList.Find(x => x.DayTime == tod),
                (int)Species.Toxel => TradeExtensions.LowKey.Contains(pk.Nature) ? evoList.Find(x => x.EvolvedForm == 1) : evoList.Find(x => x.EvolvedForm == 0),
                (int)Species.Milcery => evoList.Find(x => x.EvolvedForm == (int)alcremie),
                (int)Species.Yamask => pk.Form > 0 ? evoList.Find(x => x.EvoType == EvolutionType.HPDownBy49) : evoList.First(),
                (int)Species.Cosmoem => pk.Version == 45 ? evoList.Find(x => x.EvolvesInto == (int)Species.Lunala) : evoList.Find(x => x.EvolvesInto == (int)Species.Solgaleo),
                (int)Species.Nincada => evoList.Find(x => x.EvolvesInto == (int)Species.Ninjask),
                (int)Species.Espurr => evoList.Find(x => x.EvolvedForm == (pk.Gender == (int)Gender.Male ? 0 : 1)),
                _ => evoList.First(),
            };

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
            else if (heldUsed && (result.EvoType == EvolutionType.UseItem || result.EvoType == EvolutionType.UseItemFemale || result.EvoType == EvolutionType.UseItemMale))
            {
                msg = "This item needs to be used, not held.";
                return false;
            }
            else if (!heldUsed && (result.EvoType == EvolutionType.LevelUpHeldItemDay || result.EvoType == EvolutionType.LevelUpHeldItemNight || result.EvoType == EvolutionType.TradeHeldItem))
            {
                msg = "This item needs to be held, not used.";
                return false;
            }
            else if (pk.CanGigantamax && (pk.Species == (int)Species.Meowth || pk.Species == (int)Species.Pikachu || pk.Species == (int)Species.Eevee))
            {
                msg = $"Gigantamax {SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8)} cannot evolve.";
                return false;
            }

            switch (result.EvoType)
            {
                case EvolutionType.SpinType:
                    {
                        if ((int)item >= 1109 && (int)item <= 1115)
                            pk.FormArgument = GetAlcremieDeco(item);
                    }; break;
                case EvolutionType.LevelUpFriendship:
                case EvolutionType.LevelUpFriendshipMorning:
                case EvolutionType.LevelUpFriendshipNight:
                    {
                        if (pk.CurrentFriendship < 179)
                        {
                            msg = "Your Pokémon isn't friendly enough yet.";
                            return false;
                        }
                        pk.CurrentLevel++;
                    }; break;
                case EvolutionType.LevelUpAffection50MoveType:
                    {
                        if (pk.CurrentFriendship < 250)
                        {
                            msg = "Your Pokémon isn't affectionate enough yet.";
                            return false;
                        }
                        pk.CurrentLevel++;
                    }; break;
                case EvolutionType.LevelUpKnowMove: pk.CurrentLevel++; break;
            };

            if (pk.Species == (int)Species.Nincada)
            {
                shedinja = ShedinjaGenerator(pk);
                var laShed = new LegalityAnalysis(shedinja);
                if (!laShed.Valid)
                {
                    msg = $"Failed to evolve Nincada: \n{laShed.Report()}";
                    return false;
                }
            }

            var index = pk.PersonalInfo.GetAbilityIndex(pk.Ability);
            pk.Species = result.EvolvesInto;
            pk.Form = result.EvolvedForm;
            pk.SetAbilityIndex(index);
            pk.Nickname = pk.IsNicknamed ? pk.Nickname : pk.ClearNickname();

            var la = new LegalityAnalysis(pk);
            if (!la.Valid)
            {
                msg = $"Failed to evolve: \n{la.Report()}";
                return false;
            }

            if (heldUsed)
                pk.HeldItem = 0;

            return true;
        }

        private TradeExtensions.TCUserInfoRoot.TCUserInfo TradeCordDump(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string subfolder, PK8 pk, out int index)
        {
            var dir = Path.Combine("TradeCord", subfolder);
            Directory.CreateDirectory(dir);
            var speciesName = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8);
            var form = TradeExtensions.FormOutput(pk.Species, pk.Form, out _);
            if (speciesName.Contains("Nidoran"))
            {
                speciesName = speciesName.Remove(speciesName.Length - 1);
                form = pk.Species == (int)Species.NidoranF ? "-F" : "-M";
            }

            var array = Directory.GetFiles(dir).Where(x => x.Contains(".pk")).Select(x => int.Parse(x.Split('\\')[2].Split('-', '_')[0].Replace("★", "").Trim())).ToArray();
            array = array.OrderBy(x => x).ToArray();
            index = Indexing(array);
            var newname = (pk.IsShiny ? "★" + index.ToString() : index.ToString()) + $"_{(Ball)pk.Ball}" + " - " + speciesName + form + $"{(pk.IsEgg ? " (Egg)" : "")}" + ".pk8";
            var fn = Path.Combine(dir, Util.CleanFileName(newname));
            File.WriteAllBytes(fn, pk.DecryptedPartyData);

            user.Catches.Add(new() { Species = speciesName, Ball = ((Ball)pk.Ball).ToString(), Egg = pk.IsEgg, Form = form, ID = index, Path = fn, Shiny = pk.IsShiny, Traded = false });
            return user;
        }

        private int Indexing(int[] array)
        {
            var i = 0;
            return array.Where(x => x > 0).Distinct().OrderBy(x => x).Any(x => x != (i += 1)) ? i : i + 1;
        }

        private string ListNameSanitize(string name)
        {
            if (name == "")
                return name;

            name = name.Substring(0, 1).ToUpper().Trim() + name[1..].ToLower().Trim();
            if (name.Contains("'"))
                name = name.Replace("'", "’");

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

        private bool CanGenerateEgg(TradeExtensions.TCUserInfoRoot.TCUserInfo user, out int evo1, out int evo2)
        {
            evo1 = evo2 = 0;
            if (user.Daycare1.ID == 0 || user.Daycare2.ID == 0)
                return false;

            var pkm1 = AutoLegalityWrapper.GetTrainerInfo(8).GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet(SpeciesName.GetSpeciesNameGeneration(user.Daycare1.Species, 2, 8))), out _);
            evo1 = EvolutionTree.GetEvolutionTree(8).GetValidPreEvolutions(pkm1, 100).LastOrDefault().Species;
            var pkm2 = AutoLegalityWrapper.GetTrainerInfo(8).GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet(SpeciesName.GetSpeciesNameGeneration(user.Daycare2.Species, 2, 8))), out _);
            evo2 = EvolutionTree.GetEvolutionTree(8).GetValidPreEvolutions(pkm2, 100).LastOrDefault().Species;

            if (evo1 == 132 && evo2 == 132)
                return true;
            else if (evo1 == evo2 && Breeding.CanHatchAsEgg(evo1))
                return true;
            else if ((evo1 == 132 || evo2 == 132) && (Breeding.CanHatchAsEgg(evo1) || Breeding.CanHatchAsEgg(evo2)))
                return true;
            else if ((evo1 == 29 && evo2 == 32) || (evo1 == 32 && evo2 == 29))
                return true;
            else return false;
        }

        private List<string> GetMissingDexEntries(int[] entries, TradeExtensions.TCUserInfoRoot.TCUserInfo info)
        {
            List<string> missing = new();
            foreach (var entry in entries)
            {
                if (!info.Dex.Contains(entry))
                    missing.Add(SpeciesName.GetSpeciesNameGeneration(entry, 2, 8));
            }
            return missing;
        }

        private string DexCount(TradeExtensions.TCUserInfoRoot.TCUserInfo user, int species, bool gift)
        {
            bool entry = !user.Dex.Contains(species);
            if (entry)
                user.Dex.Add(species);

            string msg = gift && entry ? $"\n{user.Username} registered a new entry to the Pokédex!" : entry ? "\nRegistered to the Pokédex." : "";
            if (user.Dex.Count >= 664 && user.DexCompletionCount < 20)
            {
                user.Dex.Clear();
                user.DexCompletionCount += 1;
                msg += user.DexCompletionCount < 20 ? " Level increased!" : " Highest level achieved!";
            }
            return msg;
        }

        private void PerkBoostApplicator(TradeExtensions.TCUserInfoRoot.TCUserInfo user)
        {
            Rng.SpeciesBoostRNG += user.ActivePerks.FindAll(x => x == DexPerks.SpeciesBoost).Count;
            Rng.CatchRNG += user.ActivePerks.FindAll(x => x == DexPerks.CatchBoost).Count;
            Rng.ItemRNG += user.ActivePerks.FindAll(x => x == DexPerks.ItemBoost).Count;
            Rng.CherishRNG += user.ActivePerks.FindAll(x => x == DexPerks.CherishBoost).Count * 2;
            Rng.GmaxRNG += user.ActivePerks.FindAll(x => x == DexPerks.GmaxBoost).Count * 2;

            var sc = user.Items.FirstOrDefault(x => x.Item == TCItems.ShinyCharm);
            var count = sc == default ? 0 : sc.ItemCount;
            Rng.ShinyRNG += count;
            Rng.EggShinyRNG += count;
        }

        private PK8 EggProcess(TradeExtensions.TCUserInfoRoot.TCUserInfo user, string trainerInfo, int evo1, int evo2, out string msg)
        {
            bool star = false, square = false;
            if (Rng.EggShinyRNG + (user.Daycare1.Shiny && user.Daycare2.Shiny ? 5 : 0) >= 100 - Settings.SquareShinyRate)
                square = true;
            else if (Rng.EggShinyRNG + (user.Daycare1.Shiny && user.Daycare2.Shiny ? 5 : 0) >= 100 - Settings.StarShinyRate)
                star = true;

            var pk = (PK8)TradeExtensions.EggRngRoutine(user, trainerInfo, evo1, evo2, star, square);
            var eggSpeciesName = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8);
            var eggForm = TradeExtensions.FormOutput(pk.Species, pk.Form, out _);
            var finalEggName = eggSpeciesName + eggForm;

            pk.ResetPartyStats();
            msg = $"&^&You got {(pk.IsShiny ? "a **shiny egg**" : "an egg")} from the daycare! Welcome, {(pk.IsShiny ? $"**{finalEggName}**" : $"{finalEggName}")}!";
            if (user.DexCompletionCount < 20)
                msg += DexCount(user, pk.Species, false);
            return pk;
        }

        private PK8 SetProcess(string speciesName, List<string> trainerInfo)
        {
            string formHack = string.Empty;
            var formEdgeCaseRng = TradeExtensions.Random.Next(11);
            string[] poipoleRng = { "Poke", "Beast" };
            string[] mewOverride = { "\n.Version=34", "\n.Version=3" };
            string[] mewEmeraldBalls = { "Poke", "Great", "Ultra", "Dive", "Luxury", "Master", "Nest", "Net", "Premier", "Repeat", "Timer" };
            int[] ignoreForm = { 382, 383, 646, 716, 717, 778, 800, 845, 875, 877, 888, 889, 890, 898 };
            Shiny shiny = Rng.ShinyRNG >= 100 - Settings.SquareShinyRate ? Shiny.AlwaysSquare : Rng.ShinyRNG >= 100 - Settings.StarShinyRate ? Shiny.AlwaysStar : Shiny.Never;
            string shinyType = shiny == Shiny.AlwaysSquare ? "\nShiny: Square" : shiny == Shiny.AlwaysStar ? "\nShiny: Star" : "";

            if (Rng.SpeciesRNG == (int)Species.NidoranF || Rng.SpeciesRNG == (int)Species.NidoranM)
                speciesName = speciesName.Remove(speciesName.Length - 1);

            TradeExtensions.FormOutput(Rng.SpeciesRNG, 0, out string[] forms);
            var formRng = TradeExtensions.Random.Next(Rng.SpeciesRNG == (int)Species.Zygarde ? forms.Length - 1 : forms.Length);

            if (!ignoreForm.Contains(Rng.SpeciesRNG))
            {
                formHack = Rng.SpeciesRNG switch
                {
                    (int)Species.Meowstic or (int)Species.Indeedee => formEdgeCaseRng < 5 ? "-M" : "-F",
                    (int)Species.NidoranF or (int)Species.NidoranM => Rng.SpeciesRNG == (int)Species.NidoranF ? "-F (F)" : "-M (M)",
                    (int)Species.Sinistea or (int)Species.Polteageist => formEdgeCaseRng < 5 ? "" : "-Antique",
                    (int)Species.Pikachu => _ = formEdgeCaseRng < 5 ? "" : PartnerPikachuHeadache[TradeExtensions.Random.Next(PartnerPikachuHeadache.Length)],
                    (int)Species.Dracovish or (int)Species.Dracozolt => formEdgeCaseRng < 5 ? "" : "\nAbility: Sand Rush",
                    (int)Species.Arctovish or (int)Species.Arctozolt => formEdgeCaseRng < 5 ? "" : "\nAbility: Slush Rush",
                    (int)Species.Giratina => formEdgeCaseRng < 5 ? "" : "-Origin @ Griseous Orb",
                    (int)Species.Keldeo => "-Resolute",
                    _ => EventPokeForm == -1 ? $"-{forms[formRng]}" : $"-{forms[EventPokeForm]}",
                };

                formHack = formHack == "-" ? "" : formHack;
            }

            if (formHack != "" && (Rng.SpeciesRNG == (int)Species.Silvally || Rng.SpeciesRNG == (int)Species.Genesect))
            {
                switch (Rng.SpeciesRNG)
                {
                    case 649: formHack += GenesectDrives[EventPokeForm != -1 ? EventPokeForm : formRng]; break;
                    case 773: formHack += SilvallyMemory[EventPokeForm != -1 ? EventPokeForm : formRng]; break;
                };
            }

            bool birbs = ShinyLockCheck(Rng.SpeciesRNG, "", formHack != "");
            string gameVer = Rng.SpeciesRNG switch
            {
                (int)Species.Exeggutor or (int)Species.Marowak => "\n.Version=33",
                (int)Species.Mew => shiny != Shiny.Never ? $"{mewOverride[TradeExtensions.Random.Next(2)]}" : "",
                _ => UMWormhole.Contains(Rng.SpeciesRNG) && shiny == Shiny.AlwaysSquare && !birbs ? "\n.Version=33" : USWormhole.Contains(Rng.SpeciesRNG) && shiny == Shiny.AlwaysSquare && !birbs ? "\n.Version=32" : "",
            };

            if (Rng.SpeciesRNG == (int)Species.Mew && gameVer == mewOverride[1] && trainerInfo[4] != "")
                trainerInfo[4] = "";

            bool hatchu = Rng.SpeciesRNG == 25 && formHack != "" && formHack != "-Partner";
            string ballRng = Rng.SpeciesRNG switch
            {
                (int)Species.Poipole or (int)Species.Naganadel => $"\nBall: {poipoleRng[TradeExtensions.Random.Next(poipoleRng.Length)]}",
                (int)Species.Meltan or (int)Species.Melmetal => $"\nBall: {LGPEBalls[TradeExtensions.Random.Next(LGPEBalls.Length)]}",
                (int)Species.Dracovish or (int)Species.Dracozolt or (int)Species.Arctovish or (int)Species.Arctozolt => _ = formEdgeCaseRng < 5 ? $"\nBall: Poke" : $"\nBall: {(Ball)TradeExtensions.Random.Next(1, 27)}",
                (int)Species.Treecko or (int)Species.Torchic or (int)Species.Mudkip => $"\nBall: {(Ball)TradeExtensions.Random.Next(2, 27)}",
                (int)Species.Pikachu or (int)Species.Victini or (int)Species.Celebi or (int)Species.Jirachi or (int)Species.Genesect or (int)Species.Silvally => "\nBall: Poke",
                (int)Species.Mew => gameVer == mewOverride[1] ? $"\nBall: {mewEmeraldBalls[TradeExtensions.Random.Next(mewEmeraldBalls.Length)]}" : "\nBall: Poke",
                _ => TradeExtensions.Pokeball.Contains(Rng.SpeciesRNG) || gameVer == "\n.Version=33" || gameVer == "\n.Version=32" ? "\nBall: Poke" : $"\nBall: {(Ball)TradeExtensions.Random.Next(1, 27)}",
            };

            if (ballRng.Contains("Cherish"))
                ballRng = ballRng.Replace("Cherish", "Poke");

            if (ShinyLockCheck(Rng.SpeciesRNG, ballRng, formHack != "") || hatchu)
            {
                shinyType = "";
                shiny = Shiny.Never;
            }

            var set = new ShowdownSet($"{speciesName}{formHack}{ballRng}{shinyType}\n{string.Join("\n", trainerInfo)}{gameVer}");
            if (set.CanToggleGigantamax(set.Species, set.Form) && Rng.GmaxRNG >= 100 - Settings.GmaxRate)
                set.CanGigantamax = true;

            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo(8);
            var pk = (PK8)sav.GetLegal(template, out string result);

            if (pk.FatefulEncounter || result != "Regenerated")
                return pk;
            else return TradeExtensions.RngRoutine(pk, template, shiny);
        }

        private void EventHandler()
        {
            string type = string.Empty;
            var enumVals = (int[])Enum.GetValues(typeof(Gen8Dex));
            var eventType = $"{Settings.PokeEventType}";
            bool match;

            do
            {
                if (Settings.PokeEventType == PokeEventType.EventPoke)
                    MGRngEvent = MysteryGiftRng();

                if (Settings.PokeEventType != PokeEventType.Legends && Settings.PokeEventType != PokeEventType.EventPoke && Settings.PokeEventType != PokeEventType.RodentLite)
                {
                    var temp = TradeCordPK(Rng.SpeciesRNG);
                    for (int i = 0; i < temp.PersonalInfo.FormCount; i++)
                    {
                        var isPresent = PersonalTable.SWSH.GetFormEntry(temp.Species, i).IsFormWithinRange(i);
                        if (!isPresent)
                            continue;

                        temp.Form = i;
                        type = GameInfo.Strings.Types[temp.PersonalInfo.Type1] == eventType ? GameInfo.Strings.Types[temp.PersonalInfo.Type1] : GameInfo.Strings.Types[temp.PersonalInfo.Type2] == eventType ? GameInfo.Strings.Types[temp.PersonalInfo.Type2] : "";
                        EventPokeForm = type != "" ? temp.Form : -1;
                        if (EventPokeForm != -1)
                            break;
                    }
                }

                match = Settings.PokeEventType switch
                {
                    PokeEventType.Legends => Enum.IsDefined(typeof(Legends), Rng.SpeciesRNG),
                    PokeEventType.RodentLite => RodentLite.Contains(Rng.SpeciesRNG),
                    PokeEventType.EventPoke => MGRngEvent != default,
                    _ => type == eventType,
                };
                if (!match)
                    Rng.SpeciesRNG = enumVals[TradeExtensions.Random.Next(enumVals.Length)];
            }
            while (!match);
        }

        private MysteryGift? MysteryGiftRng()
        {
            var mg = EncounterEvent.GetAllEvents().Where(x => x.Species == Rng.SpeciesRNG).ToList();
            mg.RemoveAll(x => x.GetDescription().Count() < 3);
            MysteryGift? mgRng = default;
            if (mg.Count > 0)
            {
                if (Rng.ShinyRNG >= 100 - Settings.SquareShinyRate || Rng.ShinyRNG >= 100 - Settings.StarShinyRate)
                {
                    var mgSh = mg.FindAll(x => x.IsShiny);
                    mgRng = mgSh.Count > 0 ? mgSh.ElementAt(TradeExtensions.Random.Next(mgSh.Count)) : mg.ElementAt(TradeExtensions.Random.Next(mg.Count));
                }
                else mgRng = mg.ElementAt(TradeExtensions.Random.Next(mg.Count));
            }
            return mgRng;
        }

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

        private protected class EvolutionTemplate
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
                (int)Species.Flareon or (int)Species.Ninetales or (int)Species.Arcanine or (int)Species.Simisear => form == 0 ? TCItems.FireStone : TCItems.IceStone,
                (int)Species.Leafeon or (int)Species.Vileplume or (int)Species.Victreebel or (int)Species.Exeggutor or (int)Species.Shiftry or (int)Species.Simisage => TCItems.LeafStone,
                (int)Species.Glaceon or (int)Species.Sandshrew or (int)Species.Darmanitan => TCItems.IceStone,
                (int)Species.Nidoqueen or (int)Species.Nidoking or (int)Species.Clefable or (int)Species.Wigglytuff or (int)Species.Delcatty or (int)Species.Musharna => TCItems.MoonStone,
                (int)Species.Bellossom or (int)Species.Sunflora or (int)Species.Whimsicott or (int)Species.Lilligant or (int)Species.Heliolisk => TCItems.SunStone,
                (int)Species.Togekiss or (int)Species.Roserade or (int)Species.Cinccino or (int)Species.Florges => TCItems.ShinyStone,
                (int)Species.Honchkrow or (int)Species.Mismagius or (int)Species.Chandelure or (int)Species.Aegislash => TCItems.DuskStone,
                (int)Species.Gallade or (int)Species.Froslass => TCItems.DawnStone,
                (int)Species.Polteageist => form == 0 ? TCItems.CrackedPot : TCItems.ChippedPot,
                (int)Species.Appletun => TCItems.SweetApple,
                (int)Species.Flapple => TCItems.TartApple,
                (int)Species.Slowbro => TCItems.GalaricaCuff,
                (int)Species.Slowking or (int)Species.Politoed => form == 0 ? TCItems.KingsRock : TCItems.GalaricaWreath,

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

        public static string GetItemString(int item) => GameInfo.Strings.itemlist[item];

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

        private PK8 ShedinjaGenerator(PK8 pk)
        {
            PK8 shedinja = (PK8)pk.Clone();
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
            return shedinja;
        }
    }
}