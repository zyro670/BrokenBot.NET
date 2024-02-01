using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SQLite;
using SysBot.Base;
using System.Text.RegularExpressions;


namespace SysBot.Pokemon;

public class TradeCordHelper<T> : TradeCordDatabase<T> where T : PKM, new()
{
    private readonly TradeCordSettings Settings;
    public static bool TCInitialized;
    public static bool VacuumLock;
    public static Dictionary<ulong, int> TradeCordTrades = new();
    private static readonly Dictionary<ulong, TCUser> UserDict = new();
    public static readonly Dictionary<ulong, DateTime> TradeCordCooldownDict = new();
    public static readonly Dictionary<ulong, List<DateTime>> UserCommandTimestamps = new();
    public static readonly HashSet<ulong> MuteList = new();
    public static DateTime EventVoteTimer = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public TradeCordHelper(TradeCordSettings settings) : base()
    {
        Settings = settings;
    }

    public sealed class Results
    {
        public string EmbedName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public bool Success { get; set; }
        public bool FailedCatch { get; set; }

        public int PokeID { get; set; }
        public T Poke { get; set; } = new();

        public int EggPokeID { get; set; }
        public T EggPoke { get; set; } = new();

        public T? Shedinja { get; set; }

        public TCUser User { get; set; } = new();
        public TCUser Giftee { get; set; } = new();
        public string Item { get; set; } = string.Empty;

        public List<SQLCommand> SQLCommands { get; set; } = new();
        public ulong[]? UsersToPing { get; set; }
    }

    public record TC_CommandContext
    {
        public string Username { get; set; } = string.Empty;
        public ulong ID { get; set; }
        public string GifteeName { get; set; } = string.Empty;
        public ulong GifteeID { get; set; }
        public TCCommandContext Context { get; set; }
    }

    public static void CleanDB() => CleanDatabase();

    public async Task<Results> ProcessTradeCord(TC_CommandContext ctx, string[] input)
    {
        while (VacuumLock)
            await Task.Delay(100).ConfigureAwait(false);

        await _semaphore.WaitAsync(60_000).ConfigureAwait(false);
        try
        {
            if (!TCInitialized)
            {
                bool pla = typeof(T) == typeof(PA8);
                if (pla)
                {
                    LogUtil.LogError("PLA is currently not supported, shutting down.", "[TradeCord]");
                    Environment.Exit(0);
                }

                var current = Process.GetCurrentProcess();
                var all = Process.GetProcessesByName(current.ProcessName);
                bool sameExe = all.Count(x => x.MainModule?.FileName == current.MainModule?.FileName) > 1;
                if (!sameExe)
                    TCInitialized = true;
                else
                {
                    LogUtil.LogError("Another TradeCord instance is already running! Killing the process.", "[TradeCord]");
                    Environment.Exit(0);
                }

                if (!CreateDB())
                    throw new SQLiteException();

                if (Settings.ClearInactive)
                    ClearInactiveUsers();
            }

            TCUser user = new();
            TCUser giftee = new();
            if (ctx.Context != TCCommandContext.DeleteUser)
            {
                if (UserDict.ContainsKey(ctx.ID))
                    user = UserDict[ctx.ID];
                else
                {
                    user = GetCompleteUser(ctx.ID, ctx.Username);
                    UserDict.Add(ctx.ID, user);
                }

                HandleTradedCatches(ctx.ID, false, user);
                if (ctx.Context == TCCommandContext.Gift || ctx.Context == TCCommandContext.GiftItem)
                {
                    if (UserDict.ContainsKey(ctx.GifteeID))
                        giftee = UserDict[ctx.GifteeID];
                    else
                    {
                        giftee = GetCompleteUser(ctx.GifteeID, ctx.GifteeName, true);
                        UserDict.Add(ctx.GifteeID, giftee);
                    }
                }
            }

            if (ctx.Context == TCCommandContext.EventVote)
            {
                Results res = new()
                {
                    UsersToPing = GetUsersToPing()
                };

                res.Success = res.UsersToPing != null && res.UsersToPing.Length > 0;
                return res;
            }

            var task = ctx.Context switch
            {
                TCCommandContext.Catch => CatchHandler(user),
                TCCommandContext.Trade => TradeHandler(user, input[0]),
                TCCommandContext.List => ListHandler(user, input[0]),
                TCCommandContext.Info => InfoHandler(user, input[0]),
                TCCommandContext.MassRelease => MassReleaseHandler(user, input[0]),
                TCCommandContext.Release => ReleaseHandler(user, input[0]),
                TCCommandContext.DaycareInfo => DaycareInfoHandler(user),
                TCCommandContext.Daycare => DaycareHandler(user, input[0], input[1]),
                TCCommandContext.Gift => GiftHandler(user, giftee, input[0]),
                TCCommandContext.TrainerInfoSet => TrainerInfoSetHandler(user, input),
                TCCommandContext.TrainerInfo => TrainerInfoHandler(user),
                TCCommandContext.FavoritesInfo => FavoritesInfoHandler(user.Catches),
                TCCommandContext.Favorites => FavoritesHandler(user, input[0]),
                TCCommandContext.Dex => DexHandler(user.Dex, user.Perks, input[0]),
                TCCommandContext.Perks => PerkHandler(user, input[0]),
                TCCommandContext.Boost => SpeciesBoostHandler(user, input[0]),
                TCCommandContext.Buddy => BuddyHandler(user, input[0]),
                TCCommandContext.Nickname => NicknameHandler(user, input[0]),
                TCCommandContext.Evolution => EvolutionHandler(user, input[0]),
                TCCommandContext.GiveItem => GiveItemHandler(user, input[0]),
                TCCommandContext.GiftItem => GiftItemHandler(user, giftee, input[0], input[1]),
                TCCommandContext.TakeItem => TakeItemHandler(user),
                TCCommandContext.ItemList => ItemListHandler(user, input[0]),
                TCCommandContext.DropItem => ItemDropHandler(user, input[0]),
                TCCommandContext.TimeZone => TimeZoneHandler(user, input[0]),
                TCCommandContext.EventPing => EventPingHandler(user),
                TCCommandContext.DeleteUser => DeleteUserData(input[0]),
                _ => throw new NotImplementedException(),
            };

            var result = Task.Run(() => task).Result;
            InfoProcessing(ctx, user, result);
            return result;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Something went wrong during {ctx.Context} execution for {ctx.Username}.\nTarget: {ex.TargetSite}\nMessage: {ex.Message}\nStack: {ex.StackTrace}\nInner: {ex.InnerException}", "[TradeCord]");
            return new Results()
            {
                EmbedName = "Oops!",
                Message = $"Something went wrong when executing command `{ctx.Context}` for user {ctx.Username}({ctx.ID})!",
                Success = false,
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void InfoProcessing(TC_CommandContext ctx, TCUser user, Results result)
    {
        if (result.Success && (ctx.Context is TCCommandContext.Catch || (ctx.Context is TCCommandContext.Evolution && result.Shedinja is not null)))
            user = HandleNewCatches(result);

        if (result.Success && result.SQLCommands.Count > 0)
            ProcessBulkCommands(result.SQLCommands, ctx.Context is TCCommandContext.DeleteUser);

        if (result.Success && ctx.Context is not TCCommandContext.DeleteUser)
            UserDict[ctx.ID] = user;

        if (ctx.Context is TCCommandContext.Gift || ctx.Context is TCCommandContext.GiftItem)
            UserDict[ctx.GifteeID] = result.Giftee;
    }

    public static void HandleTradedCatches(ulong id, bool delete, TCUser? user = null)
    {
        bool exists = TradeCordTrades.TryGetValue(id, out int catchID);
        if (exists && user == null)
        {
            user = UserDict[id];
            if (delete)
            {
                RemoveRows(id, "catches", $"and id = {catchID}");
                RemoveRows(id, "binary_catches", $"and id = {catchID}");
                user.Catches.Remove(catchID);
            }
            else
            {
                UpdateRows(id, "catches", "was_traded = 0", $"and id = {catchID}");
                user.Catches[catchID].Traded = false;
            }
            TradeCordTrades.Remove(id);
        }
        else if (!exists && user != null)
        {
            var traded = user.Catches.ToList().FindAll(x => x.Value.Traded == true);
            int[] arr = new int[traded.Count];
            for (int i = 0; i < traded.Count; i++)
            {
                arr[i] = traded[i].Value.ID;
                user.Catches[traded[i].Key].Traded = false;
            }
            UpdateRows(id, "catches", "was_traded = 0", $"and id in ({string.Join(",", arr)})");
        }
    }

    private Results CatchHandler(TCUser user, int format = 9)
    {
        Results result = new();
        string eggMsg = string.Empty;
        string buddyMsg = string.Empty;
        SimpleTrainerInfo info = new();
        bool FuncCatch()
        {
            PerkBoostApplicator(user);
            var buddyAbil = user.Buddy.Ability;
            if (buddyAbil is Ability.FlameBody || buddyAbil is Ability.SteamEngine)
                Rng.EggRNG += 10;
            else if (buddyAbil is Ability.Pickup || buddyAbil is Ability.Pickpocket)
                Rng.ItemRNG += 10;

            bool canGenerate = CanGenerateEgg(ref user, out IReadOnlyList<EvoCriteria> evos, out int[] balls, out bool update);
            if (update)
            {
                var names = new string[] { "@id1", "@species1", "@form1", "@ball1", "@shiny1", "@id2", "@species2", "@form2", "@ball2", "@shiny2", "@user_id" };
                var obj = new object[] { user.Daycare.ID1, user.Daycare.Species1, user.Daycare.Form1, user.Daycare.Ball1, user.Daycare.Shiny1, user.Daycare.ID2, user.Daycare.Species2, user.Daycare.Form2, user.Daycare.Ball2, user.Daycare.Shiny2, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("daycare", "id1 = ?, species1 = ?, form1 = ?, ball1 = ?, shiny1 = ?, id2 = ?, species2 = ?, form2 = ?, ball2 = ?, shiny2 = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
            }

            if (!ValidateOT(user.TrainerInfo))
            {
                user.TrainerInfo.OTName = "Carp";
                var names = new string[] { "@ot", "@user_id" };
                var obj = new object[] { user.TrainerInfo.OTName, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("trainerinfo", "ot = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
            }

            var trainerInfo = TrainerInfoToStringArray(user.TrainerInfo, Game);
            if (Rng.EggRNG >= 100 - Settings.EggRate && canGenerate)
            {
                result.EggPoke = EggProcess(user.Daycare, evos, balls, Game is GameVersion.SV ? 9 : 8, trainerInfo, out eggMsg);
                if (!new LegalityAnalysis(result.EggPoke).Valid)
                {
                    result.Message = $"Oops, something went wrong when generating an egg!" +
                        $"\nParent 1: {(Species)evos[0].Species}-{evos[0].Form} | DC ID: {user.Daycare.ID1} ({user.Daycare.Species1}{user.Daycare.Form1})" +
                        $"\nParent 2: {(Species)evos[1].Species}-{evos[1].Form} | DC ID: {user.Daycare.ID2} ({user.Daycare.Species2}{user.Daycare.Form2})" +
                        $"\nResult: {(Species)result.EggPoke.Species}-{result.EggPoke.Form} ({(Ball)result.EggPoke.Ball})";
                    return false;
                }
                else
                {
                    result.EggPoke.ResetPartyStats();
                    eggMsg += DexCount(user, result, result.EggPoke.Species);
                }
            }

            if (Rng.CatchRNG >= 100 - Settings.CatchRate)
            {
                if (Rng.LegendaryRNG <= 100 - Settings.LegendaryRate && IsLegendaryOrMythical(Rng.SpeciesRNG))
                {
                    var keys = Dex.Keys.ToArray();
                    while (IsLegendaryOrMythical(Rng.SpeciesRNG))
                        Rng.SpeciesRNG = keys[Random.Next(Dex.Count)];
                }

                _ = DateTime.TryParse(Settings.EventEnd, out DateTime endTime);
                bool ended = endTime != default && DateTime.Now > endTime;
                bool boostProc = user.Perks.SpeciesBoost != 0 && Rng.SpeciesBoostRNG >= 99;
                MysteryGift mg = default!;
                byte eventForm = 0;

                if (Settings.EnableEvent && !ended)
                    EventHandler(Settings, out mg!, out eventForm);
                else if (boostProc)
                    Rng.SpeciesRNG = user.Perks.SpeciesBoost;

                var speciesName = $"{SpeciesName.GetSpeciesNameGeneration(Rng.SpeciesRNG, 2, 9)}{TradeExtensions<T>.FormOutput(result.Poke.Species, result.Poke.Form, out _)}";
                if (CherishOnly.Contains(Rng.SpeciesRNG) || Rng.CherishRNG >= 100 - Settings.CherishRate || mg != default)
                {
                    var mgRng = mg == default ? MysteryGiftRng(Settings) : mg;
                    if (mgRng != default)
                    {
                        _ = Enum.TryParse(user.TrainerInfo.OTGender, out Gender gender);
                        _ = Enum.TryParse(user.TrainerInfo.Language, out LanguageID language);
                        info = new SimpleTrainerInfo { Gender = (int)gender, Language = (int)language, OT = user.TrainerInfo.OTName, TID16 = user.TrainerInfo.TID16, SID16 = user.TrainerInfo.SID16, Context = Game is GameVersion.BDSP ? EntityContext.Gen8b : Game is GameVersion.SV ? EntityContext.Gen9 : EntityContext.Gen8, Generation = format };
                        result.Poke = TradeExtensions<T>.CherishHandler(mgRng!, info);
                    }
                }

                if (result.Poke.Species is 0)
                    result.Poke = Game is GameVersion.BDSP ? SetProcessBDSP(speciesName, trainerInfo, eventForm) : Game is GameVersion.SV ? SetProcessSV(speciesName, trainerInfo, eventForm) : SetProcessSWSH(speciesName, trainerInfo, eventForm);

                if (result.Poke.Species is ((ushort)Species.Hoopa) or ((ushort)Species.Meloetta) or ((ushort)Species.Koraidon) or ((ushort)Species.Miraidon))
                    result.Poke.Form = 0;

                if (result.Poke.Species is >= (ushort)Species.Sprigatito and <= (ushort)Species.Quaquaval && result.Poke.Ball == (int)Ball.Premier)
                    result.Poke.Ball = (int)Ball.Poke;

                if (Settings.PokeEventType is PokeEventType.Anonymyths)
                {
                    if (TradeExtensions<PK9>.Anonymyths.Contains(result.Poke.Species))
                        result.Poke.Nickname = TradeExtensions<PK9>.ReturnAnonymythsNickname(result.Poke);
                }


                if (!new LegalityAnalysis(result.Poke).Valid)
                    result.Poke = LegalityFixFailure(result.Poke);

                if (!new LegalityAnalysis(result.Poke).Valid)
                    result.Poke = (T)AutoLegalityWrapper.LegalizePokemon(result.Poke);

                if (!new LegalityAnalysis(result.Poke).Valid)
                    result.Poke = (T)result.Poke.Legalize();

                if (!new LegalityAnalysis(result.Poke).Valid)
                {
                    result.Message = $"Something went wrong when generating a catch!" +
                        $"\nSpecies: {speciesName}" +
                        $"\nForm: {result.Poke.Form}" +
                        $"\nShiny: {Rng.ShinyRNG >= 200 - Settings.StarShinyRate}";

                    return false;
                }

                result.Poke.ResetPartyStats();
                result.Message = $"It put up a fight, but you caught {(result.Poke.IsShiny ? $"**{speciesName}**" : $"{speciesName}")}!";
                result.Message += DexCount(user, result, result.Poke.Species);
            }
            else result.FailedCatch = true;

            if (Rng.ItemRNG >= 100 - Settings.ItemRate)
            {
                TCItems item;
                if (Rng.ShinyCharmRNG > 10)
                {
                    var vals = Enum.GetValues(typeof(TCItems));
                    do
                    {
                        item = (TCItems)vals.GetValue(new Random().Next(vals.Length))!;
                    } while (Game is GameVersion.BDSP ? (int)item <= 0 || (int)item >= 537 : (int)item <= 0 || (int)item == 226 || (int)item == 227);
                }
                else
                {
                    var sc = user.Items.FirstOrDefault(x => x.Item == TCItems.ShinyCharm);
                    if (sc == default || (sc != default && sc.ItemCount < 20))
                        item = TCItems.ShinyCharm;
                    else item = Game is GameVersion.BDSP ? TCItems.Everstone : TCItems.LoveSweet;
                }

                result.Item = item is TCItems.ShinyCharm ? "★**Shiny Charm**★" : GetItemString((int)item);
                var userItem = user.Items.FirstOrDefault(x => x.Item == item);
                if (userItem == default)
                {
                    user.Items.Add(new() { Item = item, ItemCount = 1 });
                    var names = ItemsValues.Replace(" ", "").Split(',');
                    var obj = new object[] { user.UserInfo.UserID, (int)item, 1 };
                    result.SQLCommands.Add(DBCommandConstructor("items", ItemsValues, "", names, obj, SQLTableContext.Insert));
                }
                else
                {
                    userItem.ItemCount++;
                    var names = new string[] { "@count", "@user_id", "@id" };
                    var obj = new object[] { userItem.ItemCount, user.UserInfo.UserID, (int)item };
                    result.SQLCommands.Add(DBCommandConstructor("items", "count = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));
                }
            }
            return true;
        }

        result.Success = FuncCatch();
        if (result.Success)
        {
            if (result.Poke.Species != 0)
                user.UserInfo.CatchCount++;

            BuddySystem(user, result, out buddyMsg);
            result.Message += buddyMsg;

            if (result.EggPoke.Species != 0)
            {
                user.UserInfo.CatchCount++;
                result.Message += eggMsg;
            }

            var namesC = new string[] { "@catch_count", "@user_id" };
            var objC = new object[] { user.UserInfo.CatchCount, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("users", "catch_count = ?", "where user_id = ?", namesC, objC, SQLTableContext.Update));

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

    private Results TradeHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncTrade()
        {
            if (!int.TryParse(input, out int id))
            {
                result.Message = "Please enter a numerical catch ID.";
                return false;
            }

            var found = user.Catches.TryGetValue(id, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "There is no Pokémon with this ID.";
                return false;
            }

            var dcfavCheck = user.Daycare.ID1 == id || user.Daycare.ID2 == id || match.Favorite || user.Buddy.ID == id;
            if (dcfavCheck)
            {
                result.Message = "Please remove your Pokémon from favorites and daycare before trading!";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species == 0)
            {
                result.Message = "Oops, something happened when converting your Pokémon!";
                return false;
            }

            var la = new LegalityAnalysis(pk);
            if (!la.Valid || pk is null)
            {
                result.Message = "Oops, I cannot trade this Pokémon!";
                return false;
            }

            result.Poke = pk;
            result.PokeID = match.ID;
            user.Catches[match.ID].Traded = true;
            var names = new string[] { "@was_traded", "@user_id", "@id" };
            var obj = new object[] { true, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("catches", "was_traded = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));
            return true;
        }

        result.Success = FuncTrade();
        result.User = user;
        return result;
    }

    private Results ListHandler(TCUser user, string input)
    {
        Results result = new()
        {
            EmbedName = $"{user.UserInfo.Username}'s List"
        };

        bool FuncList()
        {
            List<string> filters = input.Contains('=') ? input.Split('=').ToList() : new();
            if (filters.Count > 0)
            {
                filters.RemoveAt(0);
                input = input.Split('=')[0].Trim();
            }

            for (int i = 0; i < filters.Count; i++)
            {
                filters[i] = filters[i].ToLower().Trim();
                filters[i] = ListNameSanitize(filters[i]);
            }

            string nickname = input;
            input = ListNameSanitize(input);
            bool speciesAndForm = input.Contains('-');
            var speciesID = TradeExtensions<T>.EnumParse<Species>(speciesAndForm ? input.Split('-')[0] : input); //var speciesID = ParseSpeciesFromSanitizedLabel(input);
            bool isSpecies = (ushort)speciesID > 0;
            bool isBall = Enum.TryParse(input, true, out Ball enumBall);
            bool isShiny = filters.FirstOrDefault(x => x == "Shiny") != default;
            bool gmax = filters.FirstOrDefault(x => x == "Gmax") != default;
            var filterBall = filters.FirstOrDefault(x => x != "Shiny" && x != "Gmax");
            var formStrings = GameInfo.GetStrings(LanguageID.English.GetLanguage2CharName()).forms;
            bool isForm = formStrings.Contains(input);
            //bool isParadox = IsParadox((ushort)speciesID);

            if (input == "")
            {
                result.Message = "In order to filter a Pokémon, we need to know which Pokémon to filter.";
                return false;
            }

            var dict = GetLookupAsClassObject<Dictionary<int, TCCatch>>(user.UserInfo.UserID, "catches");
            if (dict.Count == 0)
            {
                result.Message = "You do not have any catches yet.";
                return false;
            }

            var catches = dict.Values.ToList();
            List<TCCatch> matches = [];
            matches = filters.Count switch
            {
                1 => catches.FindAll(x => !x.Traded && (isShiny ? x.Shiny : filterBall != default ? x.Ball == filterBall : x.Gmax) && (input == "All" ? x.Species != "" : input == "Legendaries" ? x.Legendary : input == "Events" ? x.Event : input == "Eggs" ? x.Egg : input == "Shinies" ? x.Shiny : speciesAndForm ? x.Species + x.Form == input : isSpecies ? x.Species == input : isForm ? x.Form == $"-{input}" : x.Nickname == nickname)),
                2 => catches.FindAll(x => !x.Traded && (isShiny && filterBall != default ? x.Shiny && x.Ball == filterBall : isShiny && gmax ? x.Shiny && x.Gmax : x.Ball == filterBall && x.Gmax) && (input == "All" ? x.Species != "" : input == "Legendaries" ? x.Legendary : input == "Events" ? x.Event : input == "Eggs" ? x.Egg : speciesAndForm ? x.Species + x.Form == input : isSpecies ? x.Species == input : isForm ? x.Form == $"-{input}" : x.Nickname == nickname)),
                3 => catches.FindAll(x => !x.Traded && x.Shiny && x.Ball == filterBall && x.Gmax && (input == "All" ? x.Species != "" : input == "Legendaries" ? x.Legendary : input == "Events" ? x.Event : input == "Eggs" ? x.Egg : speciesAndForm ? x.Species + x.Form == input : isSpecies ? x.Species == input : isForm ? x.Form == $"-{input}" : x.Nickname == nickname)),
                _ => catches.FindAll(x => !x.Traded && (input == "All" ? x.Species != "" : input == "Legendaries" ? x.Legendary : input == "Events" ? x.Event : input == "Eggs" ? x.Egg : input == "Shinies" ? x.Shiny : input == "Gmax" ? x.Gmax : isBall ? x.Ball == $"{enumBall}" : speciesAndForm ? x.Species + x.Form == input : isSpecies ? x.Species == input : isForm ? x.Form == $"-{input}" : x.Nickname == nickname)),
            };

            if (matches.Count == 0)
            {
                result.Message = "No results found.";
                return false;
            }

            HashSet<string> count = [], countSh = [];
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
            var listName = input == "Shinies" ? "Shiny Pokémon" : input == "All" ? "Pokémon" : input == "Egg" ? "Eggs" : $"{input} List";
            var listCount = input == "Shinies" ? $"★{countSh.Count}" : $"{count.Count}, ★{countSh.Count}";
            result.EmbedName = $"{user.UserInfo.Username}'s {listName} (Total: {listCount})";
            return true;
        }

        result.Success = FuncList();
        return result;
    }

    private Results InfoHandler(TCUser user, string input)
    {
        Results result = new()
        {
            EmbedName = $"{user.UserInfo.Username}'s Pokémon Info"
        };

        bool FuncInfo()
        {
            if (!int.TryParse(input, out int id))
            {
                result.Message = "Please enter a numerical catch ID.";
                return false;
            }

            var found = user.Catches.TryGetValue(id, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Could not find this ID.";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species == 0)
            {
                result.Message = "Oops, something happened when converting your Pokémon!";
                return false;
            }

            result.Poke = pk;
            result.EmbedName = $"{user.UserInfo.Username}'s {(match.Shiny ? "★" : "")}{match.Species}{match.Form} (ID: {match.ID})";
            return true;
        }

        result.Success = FuncInfo();
        return result;
    }

    private Results MassReleaseHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncMassRelease()
        {
            input = ListNameSanitize(input);
            if (Enum.TryParse(input, out Ball ball) && ((int)ball < 0 || (int)ball > 26))
            {
                result.Message = "Invalid ball input.";
                return false;
            }
            else if (int.TryParse(input, out _))
            {
                result.Message = "Invalid input.";
                return false;
            }

            bool speciesAndForm = input.Contains('-');
            string tableJoin = "catches c inner join daycare d on c.user_id = d.user_id inner join buddy b on c.user_id = b.user_id";
            string ballSearch = $"and c.is_favorite = 0 and c.was_traded = 0 and c.is_shiny = 0 and c.species != 'Ditto' and c.id != d.id1 and c.id != d.id2 and c.id != b.id and c.ball = '{ball}' and c.is_legendary = 0";
            string shinySearch = "and c.is_favorite = 0 and c.was_traded = 0 and c.is_shiny = 1 and c.species != 'Ditto' and c.id != d.id1 and c.id != d.id2 and c.id != b.id and c.is_event = 0 and c.is_legendary = 0";
            string legendarySearch = "and c.is_favorite = 0 and c.was_traded = 0 and c.is_shiny = 0 and c.species != 'Ditto' and c.id != d.id1 and c.id != d.id2 and c.id != b.id and c.is_event = 0 and c.is_legendary = 1";
            string eventSearch = "and c.is_favorite = 0 and c.was_traded = 0 and c.is_shiny = 0 and c.species != 'Ditto' and c.id != d.id1 and c.id != d.id2 and c.id != b.id and c.is_event = 1 and c.is_legendary = 0";
            string defaultSearch = "and c.is_favorite = 0 and c.was_traded = 0 and c.is_shiny = 0 and c.species != 'Ditto' and c.id != d.id1 and c.id != d.id2 and c.id != b.id and c.is_event = 0 and c.is_legendary = 0";

            Dictionary<int, TCCatch> catches;
            if (ball != Ball.None)
                catches = GetLookupAsClassObject<Dictionary<int, TCCatch>>(user.UserInfo.UserID, tableJoin, ballSearch, true);
            else if (input == "Shinies")
                catches = GetLookupAsClassObject<Dictionary<int, TCCatch>>(user.UserInfo.UserID, tableJoin, shinySearch, true);
            else if (input == "Legendaries")
                catches = GetLookupAsClassObject<Dictionary<int, TCCatch>>(user.UserInfo.UserID, tableJoin, legendarySearch, true);
            else if (input == "Events")
                catches = GetLookupAsClassObject<Dictionary<int, TCCatch>>(user.UserInfo.UserID, tableJoin, eventSearch, true);
            else if (input != "")
            {
                string speciesSearch = $"and {(speciesAndForm ? $"c.species||c.form = '{input}'" : $"c.species = '{input}' and c.form = ''")} and c.is_favorite = 0 and c.was_traded = 0 and c.is_shiny = 0 and c.id != d.id1 and c.id != d.id2 and c.id != b.id and c.ball != 'Cherish'";
                catches = GetLookupAsClassObject<Dictionary<int, TCCatch>>(user.UserInfo.UserID, tableJoin, speciesSearch, true);
            }
            else catches = GetLookupAsClassObject<Dictionary<int, TCCatch>>(user.UserInfo.UserID, tableJoin, defaultSearch, true);

            if (catches.Count == 0)
            {
                result.Message = input == "" ? "Cannot find any more non-shiny, non-Ditto, non-favorite, non-event, non-buddy, non-legendary Pokémon to release." : "Cannot find anything that could be released with the specified criteria.";
            }

            var matches = catches.Values.ToList();
            List<int> arr = [];
            foreach (var val in matches)
            {
                user.Catches.Remove(val.ID);
                arr.Add(val.ID);
            }

            var names = new string[arr.Count + 1];
            var obj = new object[arr.Count + 1];
            names[0] = "@user_id";
            obj[0] = user.UserInfo.UserID;

            string questionM = string.Empty;
            for (int i = 0; i < arr.Count; i++)
            {
                names[i + 1] = $"@id{i}";
                obj[i + 1] = arr[i];
                questionM += i + 1 == arr.Count ? "?" : "?, ";
            }
            result.SQLCommands.Add(DBCommandConstructor("catches", "", $"where user_id = ? and id in ({questionM})", names, obj, SQLTableContext.Delete));
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "", $"where user_id = ? and id in ({questionM})", names, obj, SQLTableContext.Delete));

            var speciesID = TradeExtensions<T>.EnumParse<Species>(speciesAndForm ? input.Split('-')[0] : input);
            bool isLegend = IsLegendaryOrMythical((ushort)speciesID);
            bool isParadox = IsParadox((ushort)speciesID);
            string ballStr = ball != Ball.None ? $"Pokémon in {ball} Ball" : "";
            string generalOutput = input == "Shinies" ? "shiny Pokémon" : input == "Events" ? "non-shiny event Pokémon" : input == "Legendaries" ? "non-shiny legendary Pokémon" : input == "Paradoxes" ? "non-shiny paradox Pokémon" : ball != Ball.None ? ballStr : $"non-shiny {input}";
            string exclude = ball is Ball.Cherish || input == "Events" ? ", legendaries, paradoxes" : input == "Legendaries" ? ", events, paradoxes," : input == "Paradoxes" ? ", events, legendaries," : $", events,{(isLegend ? "" : " legendaries,")}{(isParadox ? "" : " paradoxes,")}"; 
            result.Message = input == "" ? "Every non-shiny Pokémon was released, excluding Ditto, favorites, events, buddy, legendaries, and those in daycare." : $"Every {generalOutput} was released, excluding favorites, buddy{exclude} and those in daycare.";
            return true;
        }

        result.Success = FuncMassRelease();
        result.User = user;
        return result;
    }

    private static Results ReleaseHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncRelease()
        {
            if (!int.TryParse(input, out int id))
            {
                result.Message = "Please enter a numerical catch ID.";
                return false;
            }

            var found = user.Catches.TryGetValue(id, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Cannot find this Pokémon.";
                return false;
            }

            if (user.Daycare.ID1 == id || user.Daycare.ID2 == id || match.Favorite || user.Buddy.ID == id)
            {
                result.Message = "Cannot release a Pokémon in daycare, favorites, or if it's your buddy.";
                return false;
            }

            var names = new string[] { "@user_id", "@id" };
            var obj = new object[] { user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("catches", "", "where user_id = ? and id = ?", names, obj, SQLTableContext.Delete));
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "", "where user_id = ? and id = ?", names, obj, SQLTableContext.Delete));
            user.Catches.Remove(match.ID);

            result.Message = $"You release your {(match.Shiny ? "★" : "")}{match.Species}{match.Form}.";
            return true;
        }

        result.Success = FuncRelease();
        result.User = user;
        return result;
    }

    private Results DaycareInfoHandler(TCUser user)
    {
        Results result = new();
        bool canBreed = CanGenerateEgg(ref user, out _, out _, out bool update);
        if (update)
        {
            var names = new string[] { "@id1", "@species1", "@form1", "@ball1", "@shiny1", "@id2", "@species2", "@form2", "@ball2", "@shiny2", "@user_id" };
            var obj = new object[] { user.Daycare.ID1, user.Daycare.Species1, user.Daycare.Form1, user.Daycare.Ball1, user.Daycare.Shiny1, user.Daycare.ID2, user.Daycare.Species2, user.Daycare.Form2, user.Daycare.Ball2, user.Daycare.Shiny2, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("daycare", "id1 = ?, species1 = ?, form1 = ?, ball1 = ?, shiny1 = ?, id2 = ?, species2 = ?, form2 = ?, ball2 = ?, shiny2 = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
        }

        var dc = user.Daycare;
        if (dc.ID1 is 0 && dc.ID2 is 0)
            result.Message = "You do not have anything in daycare.";
        else
        {
            var dcSpecies1 = dc.ID1 is 0 ? "" : $"(ID: {dc.ID1}) {(dc.Shiny1 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(dc.Species1, 2, 9)}{(dc.Species1 is 29 || dc.Species1 is 32 ? "" : dc.Form1)} ({(Ball)dc.Ball1})";
            var dcSpecies2 = dc.ID2 is 0 ? "" : $"(ID: {dc.ID2}) {(dc.Shiny2 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(dc.Species2, 2, 9)}{(dc.Species2 is 29 || dc.Species2 is 32 ? "" : dc.Form2)} ({(Ball)dc.Ball2})";
            if (dc.ID1 is 132 && dc.ID2 is 132)
                result.Message = $"{(dc.ID1 is 0 ? dcSpecies2 : dcSpecies1)}\n\nThese two seem pretty diverse. You might get a surprise sooner or later.";
            if (dc.ID1 != 0 && dc.ID2 != 0 && dc.ID1 != 132 && dc.ID2 != 132)
                result.Message = $"{dcSpecies1}\n{dcSpecies2}{(canBreed ? "\n\nThey seem to really like each other." : "\n\nThey don't really seem to be fond of each other. Make sure they're of the same evolution tree, can be eggs, and have been hatched!")}";
            else if (dc.ID1 is 0 || dc.ID2 is 0)
                result.Message = $"{(dc.ID1 is 0 ? dcSpecies2 : dcSpecies1)}\n\nIt seems lonely.";
        }

        result.Success = true;
        result.User = user;
        return result;
    }

    private static Results DaycareHandler(TCUser user, string action, string id)
    {
        Results result = new()
        {
            EmbedName = $"{user.UserInfo.Username}'s Daycare"
        };

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
            bool deposit = action == "d" || action == "deposit";
            bool withdraw = action == "w" || action == "withdraw";
            bool found = user.Catches.TryGetValue(_id, out TCCatch? match);
            if (deposit && (!found || match is null || match.Traded))
            {
                result.Message = "There is no Pokémon with this ID.";
                return false;
            }

            var names = new string[] { "@id1", "@species1", "@form1", "@ball1", "@shiny1", "@id2", "@species2", "@form2", "@ball2", "@shiny2", "@user_id" };
            if (withdraw)
            {
                if (user.Daycare.ID1 is 0 && user.Daycare.ID2 is 0)
                {
                    result.Message = "You do not have anything in daycare.";
                    return false;
                }

                var form1 = user.Daycare.Species1 is 29 || user.Daycare.Species1 is 32 ? "" : user.Daycare.Form1;
                var form2 = user.Daycare.Species1 is 29 || user.Daycare.Species1 is 32 ? "" : user.Daycare.Form1;
                if (id != "all")
                {
                    if (user.Daycare.ID1.Equals(int.Parse(id)))
                    {
                        speciesString = $"(ID: {user.Daycare.ID1}) {(user.Daycare.Shiny1 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare.Species1, 2, 9)}{form1}";
                        user.Daycare = new() { ID2 = user.Daycare.ID2, Species2 = user.Daycare.Species2, Form2 = user.Daycare.Form2, Ball2 = user.Daycare.Ball2, Shiny2 = user.Daycare.Shiny2 };
                        var obj = new object[] { 0, 0, string.Empty, 0, 0, user.Daycare.ID2, user.Daycare.Species2, user.Daycare.Form2, user.Daycare.Ball2, user.Daycare.Shiny2, user.UserInfo.UserID };
                        result.SQLCommands.Add(DBCommandConstructor("daycare", "id1 = ?, species1 = ?, form1 = ?, ball1 = ?, shiny1 = ?, id2 = ?, species2 = ?, form2 = ?, ball2 = ?, shiny2 = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
                    }
                    else if (user.Daycare.ID2.Equals(int.Parse(id)))
                    {
                        speciesString = $"(ID: {user.Daycare.ID2}) {(user.Daycare.Shiny2 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare.Species2, 2, 9)}{form2}";
                        user.Daycare = new() { ID1 = user.Daycare.ID1, Species1 = user.Daycare.Species1, Form1 = user.Daycare.Form1, Ball1 = user.Daycare.Ball1, Shiny1 = user.Daycare.Shiny1 };
                        var obj = new object[] { user.Daycare.ID1, user.Daycare.Species1, user.Daycare.Form1, user.Daycare.Ball1, user.Daycare.Shiny1, 0, 0, string.Empty, 0, 0, user.UserInfo.UserID };
                        result.SQLCommands.Add(DBCommandConstructor("daycare", "id1 = ?, species1 = ?, form1 = ?, ball1 = ?, shiny1 = ?, id2 = ?, species2 = ?, form2 = ?, ball2 = ?, shiny2 = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
                    }
                    else
                    {
                        result.Message = "You do not have that Pokémon in daycare.";
                        return false;
                    }
                }
                else
                {
                    bool fullDC = user.Daycare.ID1 is not 0 && user.Daycare.ID2 is not 0;
                    speciesString = !fullDC ? $"(ID: {(user.Daycare.ID1 is not 0 ? user.Daycare.ID1 : user.Daycare.ID2)}) {(user.Daycare.ID1 is not 0 && user.Daycare.Shiny1 ? "★" : user.Daycare.ID2 is not 0 && user.Daycare.Shiny2 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare.ID1 is not 0 ? user.Daycare.Species1 : user.Daycare.Species2, 2, 9)}{(user.Daycare.ID1 is not 0 ? form1 : form2)}" :
                        $"(ID: {user.Daycare.ID1}) {(user.Daycare.Shiny1 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare.Species1, 2, 9)}{form1} and (ID: {user.Daycare.ID2}) {(user.Daycare.Shiny2 ? "★" : "")}{SpeciesName.GetSpeciesNameGeneration(user.Daycare.Species2, 2, 9)}{form2}";
                    user.Daycare = new();
                    var obj = new object[] { 0, 0, string.Empty, 0, 0, 0, 0, string.Empty, 0, 0, user.UserInfo.UserID };
                    result.SQLCommands.Add(DBCommandConstructor("daycare", "id1 = ?, species1 = ?, form1 = ?, ball1 = ?, shiny1 = ?, id2 = ?, species2 = ?, form2 = ?, ball2 = ?, shiny2 = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
                }
            }
            else if (deposit && match is not null)
            {
                if (user.Daycare.ID1 is not 0 && user.Daycare.ID2 is not 0)
                {
                    result.Message = "Daycare full, please withdraw something first.";
                    return false;
                }

                var speciesStr = string.Join("", match.Species.Split('-', ' ', '’', '.'));
                speciesStr += match.Species + match.Form == "Nidoran-M" ? "M" : match.Species + match.Form == "Nidoran-F" ? "F" : "";
                _ = Enum.TryParse(match.Ball, out Ball ball);
                _ = Enum.TryParse(speciesStr, out Species species);

                if ((user.Daycare.ID1 is 0 && user.Daycare.ID2 is 0) || (user.Daycare.ID1 is 0 && user.Daycare.ID2 != _id))
                    user.Daycare = new() { Ball1 = (int)ball, Form1 = match.Form, ID1 = match.ID, Shiny1 = match.Shiny, Species1 = (ushort)species, Ball2 = user.Daycare.Ball2, Form2 = user.Daycare.Form2, ID2 = user.Daycare.ID2, Shiny2 = user.Daycare.Shiny2, Species2 = user.Daycare.Species2 };
                else if (user.Daycare.ID2 is 0 && user.Daycare.ID1 != _id)
                    user.Daycare = new() { Ball2 = (int)ball, Form2 = match.Form, ID2 = match.ID, Shiny2 = match.Shiny, Species2 = (ushort)species, Ball1 = user.Daycare.Ball1, Form1 = user.Daycare.Form1, ID1 = user.Daycare.ID1, Shiny1 = user.Daycare.Shiny1, Species1 = user.Daycare.Species1 };
                else
                {
                    result.Message = "You've already deposited that Pokémon to daycare.";
                    return false;
                }
                var obj = new object[] { user.Daycare.ID1, user.Daycare.Species1, user.Daycare.Form1, user.Daycare.Ball1, user.Daycare.Shiny1, user.Daycare.ID2, user.Daycare.Species2, user.Daycare.Form2, user.Daycare.Ball2, user.Daycare.Shiny2, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("daycare", "id1 = ?, species1 = ?, form1 = ?, ball1 = ?, shiny1 = ?, id2 = ?, species2 = ?, form2 = ?, ball2 = ?, shiny2 = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
            }
            else
            {
                result.Message = "Invalid command.";
                return false;
            }

            result.EmbedName += $"{(deposit ? " Deposit" : " Withdraw")}";
            result.Message = deposit && found ? $"Deposited your {(match!.Shiny ? "★" : "")}{match.Species}{match.Form}({match.Ball}) to daycare!" : $"You withdrew your {speciesString} from the daycare.";
            return true;
        }

        result.Success = FuncDaycare();
        result.User = user;
        return result;
    }

    private Results GiftHandler(TCUser user, TCUser m_user, string input)
    {
        Results result = new();
        bool FuncGift()
        {
            if (!int.TryParse(input, out int id))
            {
                result.Message = "Please enter a numerical catch ID.";
                return false;
            }

            var found = user.Catches.TryGetValue(id, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Cannot find this Pokémon.";
                return false;
            }

            var dcfavCheck = user.Daycare.ID1 == id || user.Daycare.ID2 == id || match.Favorite || user.Buddy.ID == id;
            if (dcfavCheck)
            {
                result.Message = "Please remove your Pokémon from favorites, daycare, and make sure it's not an active buddy before gifting!";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species is 0)
            {
                result.Message = "Cannot find this Pokémon.";
                return false;
            }

            HashSet<int> newIDParse = new();
            foreach (var caught in m_user.Catches)
                newIDParse.Add(caught.Key);

            var newID = Indexing(newIDParse.OrderBy(x => x).ToArray());
            bool isLegend = IsLegendaryOrMythical(pk.Species);

            var names = CatchValues.Replace(" ", "").Split(',');
            var obj = new object[] { m_user.UserInfo.UserID, newID, match.Shiny, match.Ball, match.Nickname, match.Species, match.Form, match.Egg, false, false, isLegend, match.Event, match.Gmax }; 
            result.SQLCommands.Add(DBCommandConstructor("catches", CatchValues, "", names, obj, SQLTableContext.Insert));

            names = BinaryCatchesValues.Replace(" ", "").Split(',');
            obj = [m_user.UserInfo.UserID, newID, pk.DecryptedPartyData];
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", BinaryCatchesValues, "", names, obj, SQLTableContext.Insert));
            m_user.Catches.Add(newID, new() { Ball = match.Ball, Egg = match.Egg, Form = match.Form, ID = newID, Shiny = match.Shiny, Species = match.Species, Nickname = match.Nickname, Favorite = false, Traded = false, Legendary = isLegend, Event = match.Event, Gmax = match.Gmax });

            names = ["@user_id", "@id"];
            obj = [user.UserInfo.UserID, match.ID];
            result.SQLCommands.Add(DBCommandConstructor("catches", "", "where user_id = ? and id = ?", names, obj, SQLTableContext.Delete));
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "", "where user_id = ? and id = ?", names, obj, SQLTableContext.Delete));
            user.Catches.Remove(match.ID);

            var parse = TradeExtensions<T>.EnumParse<Species>(match.Species);
            var missingEntries = GetMissingDexEntries(m_user.Dex.Entries).Count;

            result.Message = $"You gifted your {(match.Shiny ? "★" : "")}{match.Species}{match.Form} to {m_user.UserInfo.Username}. New ID is {newID}.";
            if (m_user.Dex.DexCompletionCount is 0 || (m_user.Dex.DexCompletionCount < 20 && missingEntries <= 50))
                result.Message += DexCount(m_user, result, (ushort)parse, m_user.UserInfo.Username);

            return true;
        }

        result.Success = FuncGift();
        result.User = user;
        result.Giftee = m_user;
        return result;
    }

    private Results TrainerInfoSetHandler(TCUser user, string[] input)
    {
        Results result = new();
        user.TrainerInfo.OTName = input[0];
        user.TrainerInfo.OTGender = input[1];
        user.TrainerInfo.TID16 = ushort.Parse(input[2]);
        user.TrainerInfo.SID16 = ushort.Parse(input[3]);
        user.TrainerInfo.Language = input[4];
        var names = new string[] { "@ot", "@ot_gender", "@tid", "@sid", "@language", "@user_id" };
        var obj = new object[] { user.TrainerInfo.OTName, user.TrainerInfo.OTGender, user.TrainerInfo.TID16, user.TrainerInfo.SID16, user.TrainerInfo.Language, user.UserInfo.UserID };
        result.SQLCommands.Add(DBCommandConstructor("trainerinfo", "ot = ?, ot_gender = ?, tid = ?, sid = ?, language = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

        var tr = new SimpleTrainerInfo(Game) { TID16 = user.TrainerInfo.TID16, SID16 = user.TrainerInfo.SID16 };
        result.Message = $"\nYour trainer info was set to the following:" +
                         $"\n**OT:** {user.TrainerInfo.OTName}" +
                         $"\n**OTGender:** {user.TrainerInfo.OTGender}" +
                         $"\n**Display TID:** {tr.GetTrainerTID7()}" +
                         $"\n**Display SID:** {tr.GetTrainerSID7()}" +
                         $"\n**Language:** {user.TrainerInfo.Language}";
        result.Success = true;
        result.User = user;
        return result;
    }

    private Results TrainerInfoHandler(TCUser user)
    {
        Results result = new();
        var sc = user.Items.FirstOrDefault(x => x.Item == TCItems.ShinyCharm);
        var count = sc == default ? 0 : sc.ItemCount;

        var tr = new SimpleTrainerInfo(Game) { TID16 = user.TrainerInfo.TID16, SID16 = user.TrainerInfo.SID16 };
        result.Message = $"\n**OT:** {user.TrainerInfo.OTName}" +
                         $"\n**OTGender:** {user.TrainerInfo.OTGender}" +
                         $"\n**Display TID:** {tr.GetTrainerTID7()}" +
                         $"\n**Display SID:** {tr.GetTrainerSID7()}" +
                         $"\n**Language:** {user.TrainerInfo.Language}" +
                         $"\n**Shiny Charm:** {count}" +
                         $"\n**UTC Time Offset:** {user.UserInfo.TimeZoneOffset}" +
                         $"\n**Event Notifications:** {(user.UserInfo.ReceiveEventPing ? "Enabled" : "Disabled")}";
        result.Success = true;
        return result;
    }

    private static Results FavoritesInfoHandler(Dictionary<int, TCCatch> catches)
    {
        Results result = new();
        bool FuncFavoritesInfo()
        {
            var favs = catches.Values.ToList().FindAll(x => x.Favorite);
            if (favs.Count == 0)
            {
                result.Message = "You don't have anything in favorites yet!";
                return false;
            }

            List<string> names = new();
            foreach (var fav in favs)
            {
                var match = catches[fav.ID];
                names.Add(match.Shiny ? $"(__{match.ID}__) {match.Species}{match.Form}" : $"({match.ID}) {match.Species}{match.Form}");
            }

            result.Message = string.Join(", ", names.OrderBy(x => int.Parse(x.Split(' ')[0].Trim(new char[] { '(', '_', ')' }))));
            return true;
        }

        result.Success = FuncFavoritesInfo();
        return result;
    }

    private static Results FavoritesHandler(TCUser user, string input)
    {
        Results result = new()
        {
            EmbedName = $"{user.UserInfo.Username}'s Favorite"
        };

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
                var names = new string[] { "@is_favorite", "@user_id" };
                var obj = new object[] { false, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("catches", "is_favorite = ?", "where user_id = ? and is_favorite = 1", names, obj, SQLTableContext.Update));

                var favs = user.Catches.ToList().FindAll(x => x.Value.Favorite == true);
                for (int i = 0; i < favs.Count; i++)
                    user.Catches[favs[i].Key].Favorite = false;

                result.Message = $"{user.UserInfo.Username}, all of your favorites were cleared!";
                result.EmbedName += " Clear";
                return true;
            }

            var found = user.Catches.TryGetValue(id, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Cannot find this Pokémon.";
                return false;
            }

            if (!match.Favorite)
            {
                var names = new string[] { "@is_favorite", "@user_id", "@id" };
                var obj = new object[] { true, user.UserInfo.UserID, id };
                result.SQLCommands.Add(DBCommandConstructor("catches", "is_favorite = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));

                match.Favorite = true;
                result.Message = $"{user.UserInfo.Username}, added your {(match.Shiny ? "★" : "")}{match.Species}{match.Form} to favorites!";
                result.EmbedName += " Addition";
            }
            else
            {
                var names = new string[] { "@is_favorite", "@user_id", "@id" };
                var obj = new object[] { false, user.UserInfo.UserID, id };
                result.SQLCommands.Add(DBCommandConstructor("catches", "is_favorite = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));

                match.Favorite = false;
                result.Message = $"{user.UserInfo.Username}, removed your {(match.Shiny ? "★" : "")}{match.Species}{match.Form} from favorites!";
                result.EmbedName += " Removal";
            }

            return true;
        }

        result.Success = FuncFavorites();
        result.User = user;
        return result;
    }

    private Results DexHandler(TCDex dex, TCPerks perks, string input)
    {
        Results result = new();
        var speciesBoost = perks.SpeciesBoost != 0 ? $"\n**Pokémon Boost:** {SpeciesName.GetSpeciesNameGeneration(perks.SpeciesBoost, 2, 8)}" : "\n**Pokémon Boost:** N/A";

        if (input == "missing")
        {
            List<string> missing = GetMissingDexEntries(dex.Entries);
            result.Message = string.Join(", ", missing.OrderBy(x => x));
            result.Success = true;
            return result;
        }

        result.Message = $"\n**Pokédex:** {dex.Entries.Count}/{Dex.Count}\n**Level:** {dex.DexCompletionCount + perks.ActivePerks.Count}{speciesBoost}";
        result.Success = true;
        return result;
    }

    private Results PerkHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncDexPerks()
        {
            if (input == "" && (user.Dex.DexCompletionCount > 0 || user.Perks.ActivePerks.Count > 0))
            {
                result.Message = $"**CatchBoost:** {user.Perks.ActivePerks.FindAll(x => x is DexPerks.CatchBoost).Count}\n" +
                                 $"**ItemBoost:** {user.Perks.ActivePerks.FindAll(x => x is DexPerks.ItemBoost).Count}\n" +
                                 $"**SpeciesBoost:** {user.Perks.ActivePerks.FindAll(x => x is DexPerks.SpeciesBoost).Count}\n" +
                                 $"**GmaxBoost:** {user.Perks.ActivePerks.FindAll(x => x is DexPerks.GmaxBoost).Count}\n" +
                                 $"**CherishBoost:** {user.Perks.ActivePerks.FindAll(x => x is DexPerks.CherishBoost).Count}";
                return true;
            }
            else if (input == "clear")
            {
                user.Dex.DexCompletionCount += user.Perks.ActivePerks.Count;
                user.Perks.ActivePerks = new();
                user.Perks.SpeciesBoost = 0;
                var namesC = new string[] { "@perks", "@species_boost", "@user_id" };
                var objC = new object[] { string.Empty, 0, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("perks", "perks = ?, species_boost = ?", "where user_id = ?", namesC, objC, SQLTableContext.Update));

                namesC = new string[] { "@dex_count", "@user_id" };
                objC = new object[] { user.Dex.DexCompletionCount, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("dex", "dex_count = ?", "where user_id = ?", namesC, objC, SQLTableContext.Update));

                result.Message = "All active perks cleared!";
                return true;
            }

            if (user.Dex.DexCompletionCount == 0)
            {
                result.Message = "No perks available. Unassign a perk or complete the Dex to get more!";
                return false;
            }

            string[] perk = input.Split(',', ' ');
            if (perk.Length < 2)
            {
                result.Message = "Not enough parameters provided.";
                return false;
            }

            if (!int.TryParse(perk[1], out int count))
            {
                result.Message = "Incorrect input, could not parse perk point amount.";
                return false;
            }
            else if (count > user.Dex.DexCompletionCount)
            {
                result.Message = "Not enough points available to assign all requested perks.";
                return false;
            }
            else if (count == 0)
            {
                result.Message = "Please enter a non-zero amount";
                return false;
            }

            if (!Enum.TryParse(perk[0], true, out DexPerks perkVal) || perkVal is DexPerks.ShinyBoost || perkVal is DexPerks.EggRateBoost)
            {
                result.Message = "Perk name was not recognized.";
                return false;
            }

            var activeCount = user.Perks.ActivePerks.FindAll(x => x == perkVal).Count;
            if (activeCount + count > 5)
                count = 5 - activeCount;

            if (count == 0)
            {
                result.Message = "Perk is already maxed out.";
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                user.Perks.ActivePerks.Add(perkVal);
                user.Dex.DexCompletionCount -= 1;
            }

            var arrStr = ArrayStringify(user.Perks.ActivePerks.ToArray());
            var names = new string[] { "@perks", "@user_id" };
            var obj = new object[] { arrStr, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("perks", "perks = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

            names = new string[] { "@dex_count", "@user_id" };
            obj = new object[] { user.Dex.DexCompletionCount, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("dex", "dex_count = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

            result.Message = $"{(count > 1 ? $"Added {count} perk {(count > 1 ? "points" : "point")} to {perkVal}!" : $"{perkVal} perk added!")}";
            return true;
        }

        result.Success = FuncDexPerks();
        result.User = user;
        return result;
    }

    private Results SpeciesBoostHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncSpeciesBoost()
        {
            if (!user.Perks.ActivePerks.Contains(DexPerks.SpeciesBoost))
            {
                result.Message = "SpeciesBoost perk isn't active.";
                return false;
            }
            else if (int.TryParse(input, out _))
            {
                result.Message = "Please enter a valid species name.";
                return false;
            }

            input = ListNameSanitize(input).Replace("'", "").Replace("-", "").Replace(" ", "").Replace(".", "");
            if (!Enum.TryParse(input, out Species species) || !Dex.ContainsKey((ushort)species))
            {
                result.Message = "Entered species was not recognized.";
                return false;
            }

            user.Perks.SpeciesBoost = (ushort)species;
            var names = new string[] { "@species_boost", "@user_id" };
            var obj = new object[] { (int)species, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("perks", "species_boost = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

            result.Message = $"Catch chance for {species} was slightly boosted!";
            return true;
        }

        result.Success = FuncSpeciesBoost();
        result.User = user;
        return result;
    }

    private Results BuddyHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncBuddy()
        {
            int id = 0;
            if (input == "remove" && user.Buddy.ID != 0)
            {
                user.Buddy = new();
                result.Message = "Buddy removed!";
                var names = new string[] { "@id", "@name", "@ability", "@user_id" };
                var obj = new object[] { 0, string.Empty, 0, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("buddy", "id = ?, name = ?, ability = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

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

            var found = user.Catches.TryGetValue(input != string.Empty ? id : user.Buddy.ID, out TCCatch? match);
            if (!found || match == null)
            {
                if (input == string.Empty)
                {
                    var names = new string[] { "@id", "@name", "@ability", "@user_id" };
                    var obj = new object[] { 0, string.Empty, string.Empty, user.UserInfo.UserID };
                    result.SQLCommands.Add(DBCommandConstructor("buddy", "id = ?, name = ?, ability = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
                    result.Message = "Could not find this Pokémon. Clearing buddy data.";
                    user.Buddy = new();
                    return false;
                }

                result.Message = "Could not find this Pokémon.";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species == 0)
            {
                result.Message = "Oops, something happened when converting your Pokémon!";
                return false;
            }

            result.Poke = pk;
            if (input == string.Empty)
            {
                result.EmbedName = $"{user.UserInfo.Username}'s {(match.Shiny ? "★" : "")}{(pk.IsNicknamed ? $"{match.Nickname}" : $"{match.Species}{match.Form}")}";
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

                var names = new string[] { "@id", "@name", "@ability", "@user_id" };
                var obj = new object[] { id, pk.Nickname, pk.Ability, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("buddy", "id = ?, name = ?, ability = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

                result.Message = $"Set your {(match.Shiny ? "★" : "")}{(pk.IsNicknamed ? $"{user.Buddy.Nickname}" : $"{match.Species}{match.Form}")} as your new buddy!";
                return true;
            }
        }

        result.Success = FuncBuddy();
        result.User = user;
        return result;
    }

    private Results NicknameHandler(TCUser user, string input)
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

            var found = user.Catches.TryGetValue(user.Buddy.ID, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Could not find this Pokémon.";
                return false;
            }
            else if (match.Egg)
            {
                result.Message = "Cannot nickname eggs.";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species == 0)
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

            user.Buddy.Nickname = clear ? pk.Nickname : input;
            result.Message = clear ? "Your buddy's nickname was cleared!" : "Your buddy's nickname was updated!";

            var names = new string[] { "@name", "@user_id" };
            var obj = new object[] { user.Buddy.Nickname, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("buddy", "name = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

            names = new string[] { "@nickname", "@user_id", "@id" };
            obj = new object[] { user.Buddy.Nickname, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("catches", "nickname = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));

            names = new string[] { "@data", "@user_id", "@id" };
            obj = new object[] { pk.DecryptedPartyData, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "data = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));

            return true;
        }

        result.Success = FuncNickname();
        result.User = user;
        return result;
    }

    private Results EvolutionHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncEvolution()
        {
            if (user.Buddy.ID == 0)
            {
                result.Message = "You don't have an active buddy.";
                return false;
            }
            else if (input != "" && int.TryParse(input, out _))
            {
                result.Message = "Input cannot be numerical.";
                return false;
            }

            bool alc = Enum.TryParse(input, true, out AlcremieForms alcremie) && (int)alcremie <= 8;
            if (!alc)
                alcremie = AlcremieForms.None;

            bool reg = Enum.TryParse(input, true, out RegionalFormArgument regional) && (int)regional <= 4;
            if (!reg)
                regional = RegionalFormArgument.None;

            if (input != "" && alcremie < 0 && regional < 0)
            {
                result.Message = "Unable to parse input.";
                return false;
            }

            var found = user.Catches.TryGetValue(user.Buddy.ID, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Could not find this Pokémon.";
                return false;
            }
            else if (match.Egg)
            {
                result.Message = "Eggs cannot evolve.";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species == 0)
            {
                result.Message = "Oops, something happened when converting your Pokémon!";
                return false;
            }
            else if (pk.HeldItem == (int)TCItems.Everstone)
            {
                result.Message = "Your buddy cannot evolve while holding an Everstone.";
                return false;
            }

            var oldName = pk.IsNicknamed ? pk.Nickname : $"{SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8)}{TradeExtensions<T>.FormOutput(pk.Species, pk.Form, out _)}";
            var timeStr = TimeOfDayString(user.UserInfo.TimeZoneOffset, false);
            var tod = TradeExtensions<T>.EnumParse<TimeOfDay>(timeStr);
            if (tod is TimeOfDay.Dawn)
                tod = TimeOfDay.Morning;

            if (!EvolvePK(pk, tod, out string message, out T? shedinja, alcremie, regional))
            {
                result.Message = message;
                if (message.Contains("Failed to evolve"))
                    result.Message += $"\nUser ID: {user.UserInfo.UserID}\nBuddy ID: {match.ID}";
                return false;
            }

            var form = TradeExtensions<T>.FormOutput(pk.Species, pk.Form, out _);
            var species = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8);

            user.Catches[match.ID].Species = species;
            user.Catches[match.ID].Form = form;
            user.Catches[match.ID].Nickname = pk.Nickname;
            if (user.Daycare.ID1 == match.ID)
            {
                user.Daycare.Species1 = pk.Species;
                user.Daycare.Form1 = form;
                var namesDC = new string[] { "@form1", "@species1", "@user_id", "@id1" };
                var objDC = new object[] { form, pk.Species, user.UserInfo.UserID, match.ID };
                result.SQLCommands.Add(DBCommandConstructor("daycare", "form1 = ?, species1 = ?", "where user_id = ? and id1 = ?", namesDC, objDC, SQLTableContext.Update));
            }
            else if (user.Daycare.ID2 == match.ID)
            {
                user.Daycare.Form2 = form;
                user.Daycare.Species2 = pk.Species;
                var namesDC = new string[] { "@form2", "@species2", "@user_id", "@id2" };
                var objDC = new object[] { form, pk.Species, user.UserInfo.UserID, match.ID };
                result.SQLCommands.Add(DBCommandConstructor("daycare", "form2 = ?, species2 = ?", "where user_id = ? and id2 = ?", namesDC, objDC, SQLTableContext.Update));
            }

            var names = new string[] { "@form", "@species", "@nickname", "@user_id", "@id" };
            var obj = new object[] { form, species, pk.Nickname, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("catches", "form = ?, species = ?, nickname = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));

            if (shedinja != null)
                result.Shedinja = shedinja;

            user.Buddy.Ability = (Ability)pk.Ability;
            user.Buddy.Nickname = pk.Nickname;
            names = new string[] { "@name", "@ability", "@user_id" };
            obj = new object[] { pk.Nickname, pk.Ability, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("buddy", "name = ?, ability = ?", "where user_id = ?", names, obj, SQLTableContext.Update));

            names = new string[] { "@data", "@user_id", "@id" };
            obj = new object[] { pk.DecryptedPartyData, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "data = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));

            result.Message = $"{oldName} evolved into {(pk.IsShiny ? $"**{species + form}**" : species + form)}!";
            result.Message += DexCount(user, result, pk.Species);
            result.Poke = pk;
            return true;
        }

        result.Success = FuncEvolution();
        result.User = user;
        return result;
    }

    private Results GiveItemHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncGiveItem()
        {
            if (user.Buddy.ID == 0)
            {
                result.Message = "You don't have an active buddy to give an item to.";
                return false;
            }

            var item = TradeExtensions<T>.EnumParse<TCItems>(input);
            var userItem = user.Items.FirstOrDefault(x => x.Item == item);
            if (userItem == default || userItem.ItemCount == 0)
            {
                result.Message = "You do not have this item.";
                return false;
            }

            var found = user.Catches.TryGetValue(user.Buddy.ID, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Could not find this Pokémon.";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species == 0)
            {
                result.Message = "Oops, something happened when converting your Pokémon!";
                return false;
            }
            else if (pk.IsEgg)
            {
                result.Message = "Eggs cannot hold items!";
                return false;
            }

            if (pk.HeldItem != 0)
            {
                var itemCheck = (TCItems)pk.HeldItem;
                if (itemCheck > 0)
                {
                    var heldItem = user.Items.FirstOrDefault(x => x.Item == itemCheck);
                    if (heldItem == default)
                    {
                        user.Items.Add(new() { Item = itemCheck, ItemCount = 1 });
                        var namesI = ItemsValues.Replace(" ", "").Split(',');
                        var objI = new object[] { user.UserInfo.UserID, (int)itemCheck, 1 };
                        result.SQLCommands.Add(DBCommandConstructor("items", ItemsValues, "", namesI, objI, SQLTableContext.Insert));
                    }
                    else
                    {
                        heldItem.ItemCount++;
                        var namesH = new string[] { "@count", "@user_id", "@id" };
                        var objH = new object[] { heldItem.ItemCount, user.UserInfo.UserID, (int)itemCheck };
                        result.SQLCommands.Add(DBCommandConstructor("items", "count = ?", "where user_id = ? and id = ?", namesH, objH, SQLTableContext.Update));
                    }
                }
            }

            pk.HeldItem = (int)item;
            bool updateBuddy = false;
            if ((int)item >= 904 && (int)item <= 920 && pk.Species is (ushort)Species.Silvally)
                pk.Form = SilvallyFormMath(0, (int)item);
            else if (item is TCItems.GriseousOrb && pk.Species is (ushort)Species.Giratina)
            {
                updateBuddy = true;
                int index = pk.PersonalInfo.GetIndexOfAbility(pk.Ability);
                pk.Form = 1;
                pk.RefreshAbility(index);
            }

            var la = new LegalityAnalysis(pk);
            if (!la.Valid)
            {
                result.Message = $"Oops, something went wrong while giving an item to {pk.Nickname}!";
                return false;
            }

            if (updateBuddy)
            {
                var namesB = new string[] { "@ability", "@user_id" };
                var objB = new object[] { pk.Ability, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("buddy", "ability = ?", "where user_id = ?", namesB, objB, SQLTableContext.Update));
            }

            userItem.ItemCount--;
            if (userItem.ItemCount == 0)
            {
                var names = new string[] { "@user_id", "@id" };
                var obj = new object[] { user.UserInfo.UserID, (int)item };
                result.SQLCommands.Add(DBCommandConstructor("items", "", "where user_id = ? and id = ?", names, obj, SQLTableContext.Delete));
            }
            else
            {
                var names = new string[] { "@count", "@user_id", "@id" };
                var obj = new object[] { userItem.ItemCount, user.UserInfo.UserID, (int)item };
                result.SQLCommands.Add(DBCommandConstructor("items", "count = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));
            }

            var namesU = new string[] { "@data", "@user_id", "@id" };
            var objU = new object[] { pk.DecryptedPartyData, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "data = ?", "where user_id = ? and id = ?", namesU, objU, SQLTableContext.Update));

            var itemStr = GetItemString((int)item);
            result.Message = $"You gave {(ArticleChoice(itemStr[0]) ? "an" : "a")} {itemStr} to your buddy!";
            return true;
        }

        result.Success = FuncGiveItem();
        result.User = user;
        return result;
    }

    private Results GiftItemHandler(TCUser user, TCUser m_user, string input, string countInput)
    {
        Results result = new();
        bool FuncGiftItem()
        {
            var item = TradeExtensions<T>.EnumParse<TCItems>(input);
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

            var gifteeItem = m_user.Items.FirstOrDefault(x => x.Item == item);
            if (gifteeItem == default)
            {
                m_user.Items.Add(new() { Item = item, ItemCount = count });
                var namesN = ItemsValues.Replace(" ", "").Split(',');
                var objN = new object[] { m_user.UserInfo.UserID, (int)item, 1 };
                result.SQLCommands.Add(DBCommandConstructor("items", ItemsValues, "", namesN, objN, SQLTableContext.Insert));
            }
            else
            {
                gifteeItem.ItemCount += count;
                var namesU = new string[] { "@count", "@user_id", "@id" };
                var objU = new object[] { gifteeItem.ItemCount, m_user.UserInfo.UserID, (int)item };
                result.SQLCommands.Add(DBCommandConstructor("items", "count = ?", "where user_id = ? and id = ?", namesU, objU, SQLTableContext.Update));
            }

            userItem.ItemCount -= count;
            if (userItem.ItemCount == 0)
            {
                var names = new string[] { "@user_id", "@id" };
                var obj = new object[] { user.UserInfo.UserID, (int)item };
                result.SQLCommands.Add(DBCommandConstructor("items", "", "where user_id = ? and id = ?", names, obj, SQLTableContext.Delete));
            }
            else
            {
                var names = new string[] { "@count", "@user_id", "@id" };
                var obj = new object[] { userItem.ItemCount, user.UserInfo.UserID, (int)item };
                result.SQLCommands.Add(DBCommandConstructor("items", "count = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));
            }

            var itemStr = GetItemString((int)item);
            result.Message = $"You gifted {count} {itemStr}{(count == 1 ? "" : "s")} to {m_user.UserInfo.Username}!";
            return true;
        }

        result.Success = FuncGiftItem();
        result.User = user;
        result.Giftee = m_user;
        return result;
    }

    private Results TakeItemHandler(TCUser user)
    {
        Results result = new();
        bool FuncTakeItem()
        {
            if (user.Buddy.ID == 0)
            {
                result.Message = "You don't have an active buddy to take an item from.";
                return false;
            }

            var found = user.Catches.TryGetValue(user.Buddy.ID, out TCCatch? match);
            if (!found || match == null || match.Traded)
            {
                result.Message = "Could not find this Pokémon.";
                return false;
            }

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species == 0)
            {
                result.Message = "Oops, something happened when converting your Pokémon!";
                return false;
            }
            else if (pk.HeldItem == 0)
            {
                result.Message = "Your buddy isn't holding an item.";
                return false;
            }
            else if (!Enum.IsDefined(typeof(TCItems), pk.HeldItem))
            {
                result.Message = "Oops, this item is not yet available!";
                return false;
            }

            var item = (TCItems)pk.HeldItem;
            var heldItem = user.Items.FirstOrDefault(x => x.Item == item);
            if (heldItem == default)
            {
                user.Items.Add(new() { Item = item, ItemCount = 1 });
                var namesH = ItemsValues.Replace(" ", "").Split(',');
                var objH = new object[] { user.UserInfo.UserID, (int)item, 1 };
                result.SQLCommands.Add(DBCommandConstructor("items", ItemsValues, "", namesH, objH, SQLTableContext.Insert));
            }
            else
            {
                heldItem.ItemCount++;
                var namesU = new string[] { "@count", "@user_id", "@id" };
                var objU = new object[] { heldItem.ItemCount, user.UserInfo.UserID, (int)item };
                result.SQLCommands.Add(DBCommandConstructor("items", "count = ?", "where user_id = ? and id = ?", namesU, objU, SQLTableContext.Update));
            }

            var itemStr = GetItemString(pk.HeldItem);
            result.Message = $"You took {(ArticleChoice(itemStr[0]) ? "an" : "a")} {itemStr} from your buddy!";

            pk.HeldItem = 0;
            bool updateBuddy = false;
            if ((int)item >= 904 && (int)item <= 920 && pk.Species is (ushort)Species.Silvally)
                pk.Form = 0;
            else if (item is TCItems.GriseousOrb && pk.Species is (ushort)Species.Giratina)
            {
                updateBuddy = true;
                pk.Form = 0;
                pk.RefreshAbility(pk.AbilityNumber);
            }

            if (updateBuddy)
            {
                var namesB = new string[] { "@ability", "@user_id" };
                var objB = new object[] { pk.Ability, user.UserInfo.UserID };
                result.SQLCommands.Add(DBCommandConstructor("buddy", "ability = ?", "where user_id = ?", namesB, objB, SQLTableContext.Update));
            }

            var names = new string[] { "@data", "@user_id", "@id" };
            var obj = new object[] { pk.DecryptedPartyData, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "data = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));
            return true;
        }

        result.Success = FuncTakeItem();
        result.User = user;
        return result;
    }

    private Results ItemListHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncItemList()
        {
            var item = TradeExtensions<T>.EnumParse<TCItems>(input);
            if (input != "all" && item <= 0)
            {
                result.Message = input == "" ? "Nothing to search for." : "Unrecognized item.";
                return false;
            }

            var list = user.Items;
            List<TCItem> items = input switch
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

            result.EmbedName = item <= 0 ? $"{user.UserInfo.Username}'s Item List" : $"{user.UserInfo.Username}'s {GetItemString((int)item)} List";
            result.Message = content;
            return true;
        }

        result.Success = FuncItemList();
        return result;
    }

    private Results ItemDropHandler(TCUser user, string input)
    {
        Results result = new();
        bool FuncItemDrop()
        {
            var item = TradeExtensions<T>.EnumParse<TCItems>(input);
            if (input != "all" && item <= 0)
            {
                result.Message = input == "" ? "Nothing specified to drop." : "Unrecognized item.";
                return false;
            }

            var list = user.Items;
            List<TCItem> items = input switch
            {
                "all" => list.FindAll(x => x.ItemCount >= 0),
                _ => list.FindAll(x => x.Item == item && x.ItemCount > 0),
            };

            var count = items.Count;
            if (count == 0)
            {
                result.Message = "Nothing found that meets the search criteria, or you have no items.";
                return false;
            }

            List<int> idList = new();
            foreach (var entry in items)
            {
                user.Items.Remove(entry);
                idList.Add((int)entry.Item);
            }

            var names = new string[idList.Count + 1];
            var obj = new object[idList.Count + 1];
            names[0] = "@user_id";
            obj[0] = user.UserInfo.UserID;

            string questionM = string.Empty;
            for (int i = 0; i < idList.Count; i++)
            {
                names[i + 1] = $"@id{i}";
                obj[i + 1] = idList[i];
                questionM += i + 1 == idList.Count ? "?" : "?, ";
            }

            result.SQLCommands.Add(DBCommandConstructor("items", "", $"where user_id = ? and id in ({questionM})", names, obj, SQLTableContext.Delete));

            result.Message = count > 1 ? "Dropped all items!" : $"Dropped all {GetItemString((int)item)}{(items.First().ItemCount > 1 ? "s" : "")}!";
            return true;
        }

        result.Success = FuncItemDrop();
        result.User = user;
        return result;
    }

    private static Results TimeZoneHandler(TCUser user, string input)
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
            user.UserInfo.TimeZoneOffset = offset;

            var names = new string[] { "@time_offset", "@user_id" };
            var obj = new object[] { offset, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("users", "time_offset = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
            return true;
        }

        result.Success = FuncTimeZone();
        result.User = user;
        return result;
    }

    private static Results EventPingHandler(TCUser user)
    {
        Results result = new();
        bool enabled = user.UserInfo.ReceiveEventPing;
        var names = new string[] { "@receive_ping", "@user_id" };
        var obj = new object[] { !enabled, user.UserInfo.UserID };
        result.SQLCommands.Add(DBCommandConstructor("users", "receive_ping = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
        user.UserInfo.ReceiveEventPing = !enabled;

        result.Message = $"Event notifications were {(enabled ? "disabled" : "enabled")}!";
        result.User = user;
        result.Success = true;
        return result;
    }

    private Results DeleteUserData(string input)
    {
        Results result = new()
        {
            EmbedName = "User Deletion"
        };

        bool FuncDelete()
        {
            var id = ulong.Parse(input);
            var user = GetLookupAsClassObject<TCUserInfo>(id, "users");
            if (user.UserID == 0)
                return false;

            var names = new string[] { "@user_id" };
            var obj = new object[] { id };
            result.SQLCommands.Add(DBCommandConstructor("users", "", "where user_id = ?", names, obj, SQLTableContext.Delete));
            UserDict.Remove(id);
            return true;
        }

        result.Success = FuncDelete();
        return result;
    }

    private void PerkBoostApplicator(TCUser user)
    {
        Rng.SpeciesBoostRNG += (ushort)user.Perks.ActivePerks.FindAll(x => x is DexPerks.SpeciesBoost).Count;
        Rng.CatchRNG += (ushort)user.Perks.ActivePerks.FindAll(x => x is DexPerks.CatchBoost).Count;
        Rng.ItemRNG += (ushort)user.Perks.ActivePerks.FindAll(x => x is DexPerks.ItemBoost).Count;
        Rng.CherishRNG += (ushort)(user.Perks.ActivePerks.FindAll(x => x is DexPerks.CherishBoost).Count * 2);
        Rng.GmaxRNG += (ushort)(user.Perks.ActivePerks.FindAll(x => x is DexPerks.GmaxBoost).Count * 2);

        var sc = user.Items.FirstOrDefault(x => x.Item is TCItems.ShinyCharm);
        double count = sc == default ? 0 : (double)sc.ItemCount / 2;
        Rng.ShinyRNG += count;
        Rng.EggShinyRNG += count;
    }

    private T EggProcess(TCDaycare dc, IReadOnlyList<EvoCriteria> evos, int[] balls, int generation, string[] trainerInfo, out string msg)
    {
        msg = string.Empty;
        if (evos.Any(x => x.Species is 0))
            return new();

        Shiny shiny = Shiny.Never;
        if (Game is GameVersion.BDSP && Rng.EggShinyRNG + (dc.Shiny1 && dc.Shiny2 ? 5 : 0) >= 200 - Settings.SquareShinyRate - Settings.StarShinyRate)
            shiny = Shiny.Always;
        else if (Rng.EggShinyRNG + (dc.Shiny1 && dc.Shiny2 ? 5 : 0) >= 200 - Settings.SquareShinyRate)
            shiny = Shiny.AlwaysSquare;
        else if (Rng.EggShinyRNG + (dc.Shiny1 && dc.Shiny2 ? 5 : 0) >= 200 - Settings.StarShinyRate)
            shiny = Shiny.AlwaysStar;

        var pk = EggRngRoutine(evos, balls, generation, string.Join("", trainerInfo), shiny);
        var eggSpeciesName = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8);
        var eggForm = TradeExtensions<T>.FormOutput(pk.Species, pk.Form, out _);
        var finalEggName = eggSpeciesName + eggForm;

        pk.ResetPartyStats();
        pk.ClearHyperTraining();
        msg = $"&^&You got {(pk.IsShiny ? "a **shiny egg**" : "an egg")} from the daycare! Welcome, {(pk.IsShiny ? $"**{finalEggName}**" : $"{finalEggName}")}!";
        return pk;
    }

    private T SetProcessSWSH(string speciesName, string[] trainerInfo, byte eventForm)
    {
        string formHack = string.Empty;
        var formEdgeCaseRng = Random.Next(11);
        string[] mewOverride = { "\n.Version=34", "\n.Version=3" };
        int[] ignoreForm = { 382, 383, 646, 716, 717, 778, 800, 845, 875, 877, 888, 889, 890, 898 };
        Shiny shiny = Rng.ShinyRNG >= 200 - Settings.SquareShinyRate ? Shiny.AlwaysSquare : Rng.ShinyRNG >= 200 - Settings.StarShinyRate ? Shiny.AlwaysStar : Shiny.Never;
        string shinyType = shiny is Shiny.AlwaysSquare ? "\nShiny: Square" : shiny is Shiny.AlwaysStar ? "\nShiny: Star" : "";
        if (Rng.SpeciesRNG is (ushort)Species.NidoranF or (ushort)Species.NidoranM)
            speciesName = speciesName.Remove(speciesName.Length - 1);

        TradeExtensions<T>.FormOutput(Rng.SpeciesRNG, 0, out string[] forms);
        var formIDs = Dex[Rng.SpeciesRNG].ToArray();
        var formRng = formIDs[Random.Next(formIDs.Length)];
        var formIndex = eventForm is 255 ? formRng : eventForm;
        var form = forms[formIndex];

        if (!ignoreForm.Contains(Rng.SpeciesRNG))
        {
            formHack = Rng.SpeciesRNG switch
            {
                (ushort)Species.Meowstic or (ushort)Species.Indeedee => formEdgeCaseRng < 5 ? "-M" : "-F",
                (ushort)Species.NidoranF or (ushort)Species.NidoranM => Rng.SpeciesRNG is (ushort)Species.NidoranF ? "-F" : "-M",
                (ushort)Species.Sinistea or (ushort)Species.Polteageist => formEdgeCaseRng < 5 ? "" : "-Antique",
                (ushort)Species.Dracovish or (ushort)Species.Dracozolt => formEdgeCaseRng < 5 ? "" : "\nAbility: Sand Rush",
                (ushort)Species.Arctovish or (ushort)Species.Arctozolt => formEdgeCaseRng < 5 ? "" : "\nAbility: Slush Rush",
                (ushort)Species.Giratina => formEdgeCaseRng < 5 ? "" : "-Origin @ Griseous Orb",
                (ushort)Species.Keldeo => formEdgeCaseRng < 5 ? "" : "-Resolute",
                _ when form is not "" => $"-{form}",
                _ => "",
            };
        }

        if (formHack != "" && (Rng.SpeciesRNG is (ushort)Species.Silvally or (ushort)Species.Genesect))
        {
            switch (Rng.SpeciesRNG)
            {
                case 649: formHack += GenesectDrives[eventForm is not 255 ? eventForm : formRng]; break;
                case 773: formHack += SilvallyMemory[eventForm is not 255 ? eventForm : formRng]; break;
            };
        }

        if (TradeExtensions<T>.ShinyLockCheck(Rng.SpeciesRNG, formHack))
        {
            shinyType = "";
            shiny = Shiny.Never;
        }

        string gameVer = Rng.SpeciesRNG switch
        {
            (ushort)Species.Exeggutor or (ushort)Species.Marowak => "\n.Version=33",
            (ushort)Species.Mew => shiny is not Shiny.Never ? $"{mewOverride[Random.Next(2)]}" : "",
            _ => UMWormhole.Contains(Rng.SpeciesRNG) && shiny is Shiny.AlwaysSquare ? "\n.Version=33" : USWormhole.Contains(Rng.SpeciesRNG) && shiny is Shiny.AlwaysSquare ? "\n.Version=32" : "",
        };

        if (Rng.SpeciesRNG is (ushort)Species.Mew && gameVer == mewOverride[1])
            trainerInfo[4] = "";

        var tInfo = $"\n{string.Join("", trainerInfo)}";
        var showdown = $"{speciesName}{formHack}{shinyType}{tInfo}{gameVer}";
        var balls = TradeExtensions<T>.GetLegalBalls(showdown);
        string ball = balls.Length > 0 ? $"\nBall: {balls[Random.Next(balls.Length)]}" : "";

        var set = new ShowdownSet($"{showdown}{ball}");
        if (Gigantamax.CanToggle(set.Species) && Rng.GmaxRNG >= 100 - Settings.GmaxRate)
            set = new($"{showdown}{ball}\nGigantamax: Yes");

        var template = AutoLegalityWrapper.GetTemplate(set);
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pk = (T)sav.GetLegal(template, out string result);

        if (pk.FatefulEncounter || result != "Regenerated")
            return pk;
        else return RngRoutineSWSH(pk, template, shiny);
    }

    private T SetProcessBDSP(string speciesName, string[] trainerInfo, byte eventForm)
    {
        Shiny shiny = Rng.ShinyRNG >= 200 - Settings.SquareShinyRate ? Shiny.AlwaysSquare : Rng.ShinyRNG >= 200 - Settings.StarShinyRate ? Shiny.AlwaysStar : Shiny.Never;
        string shinyType = shiny is not Shiny.Never ? "\nShiny: Yes" : "";

        if (Rng.SpeciesRNG is (ushort)Species.NidoranF or (ushort)Species.NidoranM)
            speciesName = speciesName.Remove(speciesName.Length - 1);

        TradeExtensions<T>.FormOutput(Rng.SpeciesRNG, 0, out string[] forms);
        var formIDs = Dex[Rng.SpeciesRNG].ToArray();
        var formRng = formIDs[Random.Next(formIDs.Length)];
        var form = eventForm is 255 ? forms[formRng] : forms[eventForm];

        string formHack = Rng.SpeciesRNG switch
        {
            (ushort)Species.NidoranF or (ushort)Species.NidoranM => Rng.SpeciesRNG is (ushort)Species.NidoranF ? "-F" : "-M",
            (ushort)Species.Giratina => Random.Next(11) < 5 ? "" : "-Origin @ Griseous Orb",
            _ when form is not "" => $"-{form}",
            _ => "",
        };

        if (formHack != "" && Rng.SpeciesRNG is (ushort)Species.Arceus)
            formHack += ArceusPlates[eventForm is not 255 ? eventForm : formRng];

        if (TradeExtensions<T>.ShinyLockCheck(Rng.SpeciesRNG, formHack))
        {
            shinyType = "";
            shiny = Shiny.Never;
        }

        var showdown = $"{speciesName}{formHack}{shinyType}\n{string.Join("", trainerInfo)}";
        var balls = TradeExtensions<T>.GetLegalBalls(showdown);
        var ball = balls.Length > 0 ? $"\nBall: {balls[Random.Next(balls.Length)]}" : "";

        var set = new ShowdownSet($"{showdown}{ball}");
        var template = AutoLegalityWrapper.GetTemplate(set);
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pk = (T)sav.GetLegal(template, out string result);

        int attempts = 0;
        while (result != "Regenerated" && attempts < 5)
        {
            set = new ShowdownSet($"{showdown}{ball}");
            template = AutoLegalityWrapper.GetTemplate(set);
            sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            pk = (T)sav.GetLegal(template, out result);
            attempts++;
        }

        if (pk.FatefulEncounter || result != "Regenerated")
            return pk;
        else return RngRoutineBDSP(pk, shiny);
    }

    private T SetProcessSV(string speciesName, string[] trainerInfo, byte eventForm)
    {
        Shiny shiny = Rng.ShinyRNG >= 200 - Settings.SquareShinyRate ? Shiny.AlwaysSquare : Rng.ShinyRNG >= 200 - Settings.StarShinyRate ? Shiny.AlwaysStar : Shiny.Never;
        string shinyType = shiny is not Shiny.Never ? "\nShiny: Yes" : "";

        if (Rng.SpeciesRNG is (ushort)Species.NidoranF or (ushort)Species.NidoranM)
            speciesName = speciesName.Remove(speciesName.Length - 1);

        TradeExtensions<T>.FormOutput(Rng.SpeciesRNG, 0, out string[] forms);
        var formIDs = Dex[Rng.SpeciesRNG].ToArray();
        var formRng = formIDs[Random.Next(formIDs.Length)];
        var form = eventForm is 255 ? forms[formRng] : forms[eventForm];

        string formHack = Rng.SpeciesRNG switch
        {
            (ushort)Species.NidoranF or (ushort)Species.NidoranM => Rng.SpeciesRNG is (ushort)Species.NidoranF ? "-F" : "-M",
            (ushort)Species.Giratina => Random.Next(11) < 5 ? "" : "-Origin @ Griseous Core",
            _ when form is not "" => $"-{form}",
            _ => "",
        };

        if (formHack != "" && Rng.SpeciesRNG is (ushort)Species.Arceus)
            formHack += ArceusPlates[eventForm is not 255 ? eventForm : formRng];

        if (TradeExtensions<T>.ShinyLockCheck(Rng.SpeciesRNG, formHack))
        {
            shinyType = "";
            shiny = Shiny.Never;
        }

        var showdown = $"{speciesName}{formHack}{shinyType}\n{string.Join("", trainerInfo)}";
        var balls = TradeExtensions<T>.GetLegalBalls(showdown);
        var ball = balls.Length > 0 ? $"\nBall: {balls[Random.Next(balls.Length)]}" : "";

        var set = new ShowdownSet($"{showdown}{ball}");
        var template = AutoLegalityWrapper.GetTemplate(set);
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pk = (T)sav.GetLegal(template, out string result);

        int attempts = 0;
        while (result != "Regenerated" && attempts < 5)
        {
            set = new ShowdownSet($"{showdown}{ball}");
            template = AutoLegalityWrapper.GetTemplate(set);
            sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            pk = (T)sav.GetLegal(template, out result);
            attempts++;
        }

        if (pk.FatefulEncounter || result != "Regenerated")
            return pk;
        else return RngRoutineSV(pk, shiny);
    }

    private void BuddySystem(TCUser user, Results result, out string buddyMsg)
    {
        buddyMsg = string.Empty;
        if (user.Buddy.ID is not 0)
        {
            var found = user.Catches.TryGetValue(user.Buddy.ID, out TCCatch? match);
            if (!found || match == null || match.Traded)
                return;

            var pk = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {match.ID}");
            if (pk.Species is 0)
                return;

            if (pk.IsEgg)
            {
                double status = (pk.CurrentFriendship - 5) / (double)pk.PersonalInfo.HatchCycles;
                if (status > 0)
                    pk.CurrentFriendship -= 5;
                else
                {
                    buddyMsg = "\nUh-oh!... You've just hatched an egg!";
                    CommonEdits.ForceHatchPKM(pk);
                    pk.CurrentFriendship = pk.PersonalInfo.BaseFriendship;

                    var namesH = new string[] { "@name", "@user_id" };
                    var objH = new object[] { pk.Nickname, user.UserInfo.UserID };
                    result.SQLCommands.Add(DBCommandConstructor("buddy", "name = ?", "where user_id = ?", namesH, objH, SQLTableContext.Update));

                    namesH = new string[] { "@nickname", "@is_egg", "@user_id", "@id" };
                    objH = new object[] { pk.Nickname, 0, user.UserInfo.UserID, match.ID };
                    result.SQLCommands.Add(DBCommandConstructor("catches", "nickname = ?, is_egg = ?", "where user_id = ? and id = ?", namesH, objH, SQLTableContext.Update));

                    user.Buddy.Nickname = pk.Nickname;
                    user.Catches[match.ID].Egg = false;
                    user.Catches[match.ID].Nickname = pk.Nickname;
                }
            }
            else if (pk.CurrentLevel < 100 && result.Poke.Species is not 0)
            {
                var enc = result.Poke;
                var sootheBell = pk.HeldItem is 218 && pk.CurrentFriendship + 2 <= 255 ? 2 : 0;
                var shiny = enc.IsShiny && pk.CurrentFriendship + 5 + sootheBell <= 255 ? 5 : 0;
                pk.CurrentFriendship += sootheBell + shiny;

                int levelOld = pk.CurrentLevel;
                var xpMin = Experience.GetEXP(pk.CurrentLevel + 1, pk.PersonalInfo.EXPGrowth);
                var calc = enc.PersonalInfo.BaseEXP * enc.CurrentLevel / 5.0 * Math.Pow((2.0 * enc.CurrentLevel + 10.0) / (enc.CurrentLevel + pk.CurrentLevel + 10.0), 2.5);
                var bonus = enc.IsShiny ? 1.1 : 1.0;
                var xpGet = (uint)Math.Round(calc * bonus, 0, MidpointRounding.AwayFromZero);
                if (xpGet < 100)
                    xpGet = 175;

                pk.EXP += xpGet;
                while (pk.EXP >= Experience.GetEXP(pk.CurrentLevel + 1, pk.PersonalInfo.EXPGrowth) && pk.CurrentLevel < 100)
                    pk.CurrentLevel++;

                if (pk.CurrentLevel is 100)
                    pk.EXP = xpMin;

                if (pk.EXP >= xpMin)
                {
                    buddyMsg = $"\n{user.Buddy.Nickname} gained {xpGet} EXP and leveled up to level {pk.CurrentLevel}!";
                    if (pk.CurrentFriendship < 255)
                    {
                        var delta = pk.CurrentLevel - levelOld;
                        for (int i = 0; i < delta; i++)
                        {
                            if (pk.CurrentFriendship + 2 >= 255)
                            {
                                pk.CurrentFriendship = 255;
                                break;
                            }
                            pk.CurrentFriendship += 2;
                        }
                    }
                }
                else buddyMsg = $"\n{user.Buddy.Nickname} gained {xpGet} EXP!";
            }

            var names = new string[] { "@data", "@user_id", "@id" };
            var obj = new object[] { pk.DecryptedPartyData, user.UserInfo.UserID, match.ID };
            result.SQLCommands.Add(DBCommandConstructor("binary_catches", "data = ?", "where user_id = ? and id = ?", names, obj, SQLTableContext.Update));
            result.User = user;
        }
    }

    private string DexCount(TCUser user, Results result, ushort species, string gifteeName = "")
    {
        if (user.Dex.DexCompletionCount >= 20)
            return "";

        bool entry = !user.Dex.Entries.Contains(species);
        if (entry)
        {
            user.Dex.Entries.Add(species);
            string entryStr = ArrayStringify(user.Dex.Entries.ToArray());
            string[] names = new string[] { "@entries", "@user_id" };
            var obj = new object[] { entryStr, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("dex", "entries = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
        }

        string msg = gifteeName != "" && entry ? $"\n{gifteeName} registered a new entry to the Pokédex!" : entry ? "\nRegistered to the Pokédex." : "";
        if (user.Dex.Entries.Count >= Dex.Count && user.Dex.DexCompletionCount < 20)
        {
            user.Dex.Entries.Clear();
            user.Dex.DexCompletionCount += 1;
            bool sc = false;
            if (user.Dex.DexCompletionCount is 1 && user.Items.FirstOrDefault(x => x.Item is TCItems.ShinyCharm) == default)
            {
                sc = true;
                user.Items.Add(new() { Item = TCItems.ShinyCharm, ItemCount = 1 });
                var namesI = ItemsValues.Replace(" ", "").Split(',');
                var objI = new object[] { user.UserInfo.UserID, (int)TCItems.ShinyCharm, 1 };
                result.SQLCommands.Add(DBCommandConstructor("items", ItemsValues, "", namesI, objI, SQLTableContext.Insert));
            }

            msg += user.Dex.DexCompletionCount < 20 ? $" Level increased!{(sc ? " Received a ★**Shiny Charm**★" : "")}" : " Highest level achieved!";
            string[] names = new string[] { "@entries", "@dex_count", "@user_id" };
            var obj = new object[] { string.Empty, user.Dex.DexCompletionCount, user.UserInfo.UserID };
            result.SQLCommands.Add(DBCommandConstructor("dex", "entries = ?, dex_count = ?", "where user_id = ?", names, obj, SQLTableContext.Update));
        }
        result.User = user;
        return msg;
    }

    private void CatchRegister(Results result, T pk, out int index)
    {
        var speciesName = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 9).Replace(" ", "").Replace("-", "");
        var form = TradeExtensions<T>.FormOutput(pk.Species, pk.Form, out _);
        bool isLegend = IsLegendaryOrMythical(pk.Species);
        bool isParadox = IsParadox(pk.Species);
        var set = ShowdownUtil.ConvertToShowdown(ShowdownParsing.GetShowdownText(pk));
        bool canGmax = set is not null && set.CanGigantamax;
        if (speciesName.Contains("Nidoran"))
        {
            speciesName = speciesName.Remove(speciesName.Length - 1);
            form = pk.Species is (ushort)Species.NidoranF ? "-F" : "-M";
        }

        int[] array = result.User.Catches.Select(x => x.Value.ID).ToArray();
        array = array.OrderBy(x => x).ToArray();
        index = Indexing(array);
        result.User.Catches.Add(index, new() { Species = speciesName, Nickname = pk.Nickname, Ball = $"{(Ball)pk.Ball}", Egg = pk.IsEgg, Form = form, ID = index, Shiny = pk.IsShiny, Traded = false, Favorite = false, Legendary = isLegend, Event = pk.FatefulEncounter, Gmax = canGmax});

        var names = CatchValues.Replace(" ", "").Replace("-", "").Split(',');
        var obj = new object[] { result.User.UserInfo.UserID, index, pk.IsShiny, $"{(Ball)pk.Ball}", pk.Nickname, speciesName, form, pk.IsEgg, false, false, isLegend, pk.FatefulEncounter, canGmax, isParadox };
        result.SQLCommands.Add(DBCommandConstructor("catches", CatchValues, "", names, obj, SQLTableContext.Insert));

        names = BinaryCatchesValues.Replace(" ", "").Split(',');
        obj = new object[] { result.User.UserInfo.UserID, index, pk.DecryptedPartyData };
        result.SQLCommands.Add(DBCommandConstructor("binary_catches", BinaryCatchesValues, "", names, obj, SQLTableContext.Insert));
    }

    private TCUser HandleNewCatches(Results result)
    {
        if (result.Shedinja is not null)
        {
            CatchRegister(result, result.Shedinja, out int shedinjaID);
            var shedStr = $"\n\nA spare Poké Ball in your bag clicks quietly... You also caught {(result.Shedinja.IsShiny ? "**Shedinja**" : "Shedinja")} (ID: {shedinjaID})!";
            shedStr += DexCount(result.User, result, result.Shedinja.Species);
            result.Message += shedStr;
        }

        if (result.Poke.Species is not 0)
        {
            CatchRegister(result, result.Poke, out int pokeID);
            result.PokeID = pokeID;
        }

        if (result.EggPoke.Species is not 0)
        {
            CatchRegister(result, result.EggPoke, out int eggID);
            result.EggPokeID = eggID;
        }
        return result.User;
    }

    public string GetDexFlavorText(ushort species, byte form, bool gmax) => GetDexFlavorFromTable(species, form, gmax);

    private static bool ValidateOT(TCTrainerInfo info) => info.OTName.Length <= Legal.GetMaxLengthOT(8, (LanguageID)Enum.Parse(typeof(LanguageID), info.Language));

    private static SQLCommand DBCommandConstructor(string table, string vals, string filter, string[] names, object[] values, SQLTableContext ctx)
    {
        string cmd = ctx switch
        {
            SQLTableContext.Update => $"update {table} set {vals} {filter}",
            SQLTableContext.Insert => $"insert into {table} values ({vals}) {filter}",
            SQLTableContext.Delete => $"PRAGMA foreign_keys = ON;delete from {table} {filter}",
            SQLTableContext.Select => $"select * from {table} {filter}",
            _ => "",
        };

        return new()
        {
            CommandText = cmd,
            Names = names,
            Values = values,
        };
    }
}