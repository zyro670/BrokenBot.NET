using PKHeX.Core;
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
        private readonly int[] PikaClones = { 25, 26, 172, 587, 702, 777, 877 };
        private readonly int[] CherishOnly = { 719, 721, 801, 802, 807, 893 };
        private readonly int[] TradeEvo = { (int)Species.Machoke, (int)Species.Haunter, (int)Species.Boldore, (int)Species.Gurdurr, (int)Species.Phantump, (int)Species.Gourgeist };
        private readonly int[] ShinyLock = { (int)Species.Victini, (int)Species.Keldeo, (int)Species.Volcanion, (int)Species.Cosmog, (int)Species.Cosmoem, (int)Species.Magearna,
                                             (int)Species.Marshadow, (int)Species.Zacian, (int)Species.Zamazenta, (int)Species.Eternatus, (int)Species.Kubfu, (int)Species.Urshifu,
                                             (int)Species.Zarude, (int)Species.Glastrier, (int)Species.Spectrier, (int)Species.Calyrex };

        private readonly int[] UMWormhole = { 144, 145, 146, 150, 244, 245, 249, 380, 382, 384, 480, 481, 482, 484, 487, 488, 644, 645, 646, 642, 717, 793, 795, 796, 797, 799 };
        private readonly int[] USWormhole = { 144, 145, 146, 150, 245, 250, 381, 383, 384, 480, 481, 482, 487, 488, 645, 646, 793, 794, 796, 799, 483, 485, 641, 643, 716, 798 };

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
            public int PokeID { get; set; } = new();
            public PK8 EggPoke { get; set; } = new();
            public int EggPokeID { get; set; } = new();
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
                user = PerkBoostApplicator(user);
                List<string> trainerInfo = new();
                trainerInfo.AddRange(new string[]
                {
                    user.OTName == "" ? "" : $"OT: {user.OTName}",
                    user.OTGender == "" ? "" : $"OTGender: {user.OTGender}",
                    user.TID == 0 ? "" : $"TID: {user.TID}",
                    user.SID == 0 ? "" : $"SID: {user.SID}",
                    user.Language == "" ? "" : $"Language: {user.Language}"
                });

                bool egg = CanGenerateEgg(user, out int evo1, out int evo2) && Rng.EggRNG >= 100 - Settings.EggRate;
                if (egg)
                {
                    result.EggPoke = EggProcess(user, string.Join("\n", trainerInfo), evo1, evo2, out eggMsg);
                    if (!(result.EggPoke is PK8) || !new LegalityAnalysis(result.EggPoke).Valid)
                        return false;
                    else
                    {
                        result.EggPoke.ResetPartyStats();
                        if (user.DexCompletionCount < 30)
                            eggMsg += DexCount(user, result.EggPoke.Species, false);
                    }
                }

                DateTime.TryParse(Settings.EventEnd, out DateTime endTime);
                bool ended = endTime != default && DateTime.Now > endTime;
                bool boostProc = user.SpeciesBoost != 0 && Rng.SpeciesBoostRNG >= 100 - user.ActivePerks.FindAll(x => x == DexPerks.SpeciesBoost).Count;

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

                    if (TradeEvo.Contains(result.Poke.Species))
                        result.Poke.HeldItem = 229;

                    if (!(result.Poke is PK8) || !new LegalityAnalysis(result.Poke).Valid)
                        return false;

                    result.Poke.ResetPartyStats();
                    result.Message = $"It put up a fight, but you caught {(result.Poke.IsShiny ? $"**{speciesName}**" : $"{speciesName}")}!";
                    if (user.DexCompletionCount < 30)
                        result.Message += DexCount(user, result.Poke.Species, false);
                }

                if (Rng.CatchRNG < 100 - Settings.CatchRate)
                    result.FailedCatch = true;

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
            }

            result.User = user;
            result.EmbedName += $"Results{(result.EggPoke.Species != 0 ? "&^&\nEggs" : "")}";
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
                TradeExtensions.TradeCordPath.Add(match.Path);
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

                var listName = input == "Shinies" ? "Shiny Pokémon" : input == "All" ? "Pokémon" : input == "Egg" ? "Eggs" : $"List For {input}";
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
                if (m_user.DexCompletionCount == 0 || (m_user.DexCompletionCount < 30 && missingEntries <= 50))
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
            result.Message = $"\n**OT:** {(user.OTName == "" ? "Not set." : user.OTName)}" +
                         $"\n**OTGender:** {(user.OTGender == "" ? "Not set." : user.OTGender)}" +
                         $"\n**TID:** {(user.TID == 0 ? "Not set." : user.TID)}" +
                         $"\n**SID:** {(user.SID == 0 ? "Not set." : user.SID)}" +
                         $"\n**Language:** {(user.Language == "" ? "Not set." : user.Language)}";
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
                          $"**EggRateBoost:** {user.ActivePerks.FindAll(x => x == DexPerks.EggRateBoost).Count}\n" +
                          $"**ShinyBoost:** {user.ActivePerks.FindAll(x => x == DexPerks.ShinyBoost).Count}\n" +
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

                if (!Enum.TryParse(perk[0], true, out DexPerks perkVal))
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
                    return true;
                }
                else
                {
                    user.Buddy = new()
                    {
                        ID = id,
                        Nickname = pk.Nickname,
                        Ability = (Ability)pk.Ability,
                        HatchSteps = 0,
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

                if (pk.IsEgg)
                {
                    double status = user.Buddy.HatchSteps / (double)pk.PersonalInfo.HatchCycles;
                    if (status < 1.0)
                    {
                        user.Buddy.HatchSteps += 5;
                        return user;
                    }
                    else
                    {
                        CommonEdits.ForceHatchPKM(pk);
                        File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                        user.Buddy.Nickname = pk.Nickname;
                        user.Buddy.HatchSteps = 0;
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
                        buddyMsg = "\nUh-oh!... You've just hatched an egg!";
                    }
                }
                else if (pk.CurrentLevel < 100 && pk8.Species != 0)
                {
                    var xpMin = Experience.GetEXP(pk.CurrentLevel + 1, pk.PersonalInfo.EXPGrowth);
                    var xpGet = (uint)Math.Round(Math.Pow(pk8.CurrentLevel / 5.0 * ((2.0 * pk8.CurrentLevel + 10.0) / (pk8.CurrentLevel + pk.CurrentLevel + 10.0)), 2.5) * (pk.OT_Name == user.OTName ? 1.0 : 1.5) * (pk8.IsShiny ? 1.3 : 1.0), 0, MidpointRounding.AwayFromZero);
                    if (xpGet < 100)
                        xpGet = 175;

                    pk.EXP += xpGet;
                    while (pk.EXP >= Experience.GetEXP(pk.CurrentLevel + 1, pk.PersonalInfo.EXPGrowth) && pk.CurrentLevel < 100)
                        pk.CurrentLevel++;

                    if (pk.CurrentLevel == 100)
                        pk.EXP = xpMin;

                    File.WriteAllBytes(match.Path, pk.DecryptedPartyData);
                    if (pk.EXP >= xpMin)
                        buddyMsg = $"\n{user.Buddy.Nickname} gained {xpGet} EXP and leveled up to level {pk.CurrentLevel}!";
                    else buddyMsg = $"\n{user.Buddy.Nickname} gained {xpGet} EXP!";
                }
            }
            return user;
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
            else if (evo1 == evo2 && Enum.IsDefined(typeof(ValidEgg), evo1))
                return true;
            else if ((evo1 == 132 || evo2 == 132) && (Enum.IsDefined(typeof(ValidEgg), evo1) || Enum.IsDefined(typeof(ValidEgg), evo2)))
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
            if (user.Dex.Count >= 664 && user.DexCompletionCount < 30)
            {
                user.Dex.Clear();
                user.DexCompletionCount += 1;
                msg += user.DexCompletionCount < 30 ? " Level increased!" : " Highest level achieved!";
            }
            return msg;
        }

        private TradeExtensions.TCUserInfoRoot.TCUserInfo PerkBoostApplicator(TradeExtensions.TCUserInfoRoot.TCUserInfo user)
        {
            Rng.CatchRNG += user.ActivePerks.FindAll(x => x == DexPerks.CatchBoost).Count;
            Rng.CherishRNG += user.ActivePerks.FindAll(x => x == DexPerks.CherishBoost).Count * 2;
            Rng.GmaxRNG += user.ActivePerks.FindAll(x => x == DexPerks.GmaxBoost).Count * 2;
            Rng.EggRNG += user.ActivePerks.FindAll(x => x == DexPerks.EggRateBoost).Count * 2;
            Rng.ShinyRNG += user.ActivePerks.FindAll(x => x == DexPerks.ShinyBoost).Count * 2;
            Rng.EggShinyRNG += user.ActivePerks.FindAll(x => x == DexPerks.ShinyBoost).Count * 2;
            return user;
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
            if (user.DexCompletionCount < 30)
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
                (int)Species.Dracovish or (int)Species.Dracozolt or (int)Species.Arctovish or (int)Species.Arctozolt => _ = formEdgeCaseRng < 5 ? $"\nBall: Poke" : $"\nBall: {(Ball)TradeExtensions.Random.Next(1, 26)}",
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

                if (Settings.PokeEventType != PokeEventType.Legends && Settings.PokeEventType != PokeEventType.EventPoke && Settings.PokeEventType != PokeEventType.PikaClones)
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
                    PokeEventType.PikaClones => PikaClones.Contains(Rng.SpeciesRNG),
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

        private bool ShinyLockCheck(int species, string ball, bool form)
        {
            if (ShinyLock.Contains(species))
                return true;
            else if (form && (species == (int)Species.Zapdos || species == (int)Species.Moltres || species == (int)Species.Articuno))
                return true;
            else if (ball.Contains("Beast") && (species == (int)Species.Poipole || species == (int)Species.Naganadel))
                return true;
            else return false;
        }
    }
}