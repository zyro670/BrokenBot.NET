using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeX.Core.Species;
using static PKHeX.Core.AutoMod.Aesthetics;
using static PKHeX.Core.AutoMod.Aesthetics.PersonalColor;

namespace SysBot.Pokemon
{
    public abstract class TradeCordBase<T> where T : PKM, new()
    {
        protected static readonly List<EvolutionTemplate> Evolutions = EvolutionRequirements();
        public static int[] Dex { get; private set; } = new int[] { };
        protected TCRng Rng { get; private set; }
        private static bool Connected { get; set; }

        protected static GameVersion Game = typeof(T) == typeof(PK8) ? GameVersion.SWSH : GameVersion.BDSP;
        private static string DatabasePath = string.Empty;
        private static SQLiteConnection Connection = new();
        protected static readonly Random Random = new();

        protected readonly string[] PartnerPikachuHeadache = { "-Original", "-Partner", "-Hoenn", "-Sinnoh", "-Unova", "-Alola", "-Kalos", "-World" };
        protected readonly string[] LGPEBalls = { "Poke", "Premier", "Great", "Ultra", "Master" };
        protected readonly string[] SilvallyMemory = { "", " @ Fighting Memory"," @ Flying Memory", " @ Poison Memory", " @ Ground Memory", " @ Rock Memory",
            " @ Bug Memory", " @ Ghost Memory", " @ Steel Memory", " @ Fire Memory", " @ Water Memory", " @ Grass Memory", " @ Electric Memory", " @ Psychic Memory",
            " @ Ice Memory", " @ Dragon Memory", " @ Dark Memory", " @ Fairy Memory" };
        protected readonly string[] GenesectDrives = { "", " @ Douse Drive", " @ Shock Drive", " @ Burn Drive", " @ Chill Drive" };
        protected readonly int[] CherishOnly = { 719, 721, 801, 802, 807, 893 };
        protected readonly int[] TradeEvo = { (int)Machoke, (int)Haunter, (int)Boldore, (int)Gurdurr, (int)Phantump, (int)Gourgeist };

        protected readonly int[] UMWormhole = { 144, 145, 146, 150, 244, 245, 249, 380, 382, 384, 480, 481, 482, 484, 487, 488, 644, 645, 646, 642, 717, 793, 795, 796, 797, 799 };
        protected readonly int[] USWormhole = { 144, 145, 146, 150, 245, 250, 381, 383, 384, 480, 481, 482, 487, 488, 645, 646, 793, 794, 796, 799, 483, 485, 641, 643, 716, 798 };
        protected readonly int[] GalarFossils = { 880, 881, 882, 883 };

        private static readonly string UsersValues = "@user_id, @username, @catch_count, @time_offset, @last_played, @receive_ping";
        private static readonly string TrainerInfoValues = "@user_id, @ot, @ot_gender, @tid, @sid, @language";
        private static readonly string DexValues = "@user_id, @dex_count, @entries";
        private static readonly string PerksValues = "@user_id, @perks, @species_boost";
        private static readonly string DaycareValues = "@user_id, @shiny1, @id1, @species1, @form1, @ball1, @shiny2, @id2, @species2, @form2, @ball2";
        private static readonly string BuddyValues = "@user_id, @id, @name, @ability";
        protected static readonly string ItemsValues = "@user_id, @id, @count";
        protected static readonly string CatchValues = "@user_id, @id, @is_shiny, @ball, @nickname, @species, @form, @is_egg, @is_favorite, @was_traded, @is_legendary, @is_event, @is_gmax";
        protected static readonly string BinaryCatchesValues = "@user_id, @id, @data";

        private readonly string[] TableCreateCommands =
        {
            "create table if not exists users(user_id integer primary key, username text not null, catch_count int default 0, time_offset int default 0, last_played text default '', receive_ping int default 0)",
            "create table if not exists trainerinfo(user_id integer, ot text default 'Carp', ot_gender text default 'Male', tid int default 12345, sid int default 54321, language text default 'English', foreign key (user_id) references users (user_id) on delete cascade)",
            "create table if not exists dex(user_id integer, dex_count int default 0, entries text default '', foreign key (user_id) references users (user_id) on delete cascade)",
            "create table if not exists dex_flavor(species integer primary key, base text default '', gmax text default '', form1 text default '', form2 text default '', form3 text default '', form4 text default '', form5 text default '', form6 text default '', form7 text default '', form8 text default '', form9 text default '', form10 text default '', form11 text default '', form12 text default '', form13 text default '', form14 text default '', form15 text default '', form16 text default '', form17 text default '', form18 text default '', form19 text default '', form20 text default '', form21 text default '', form22 text default '', form23 text default '', form24 text default '', form25 text default '', form26 text default '', form27 text default '')",
            "create table if not exists perks(user_id integer, perks text default '', species_boost int default 0, foreign key (user_id) references users (user_id) on delete cascade)",
            "create table if not exists daycare(user_id integer, shiny1 int default 0, id1 int default 0, species1 int default 0, form1 text default '', ball1 int default 0, shiny2 int default 0, id2 int default 0, species2 int default 0, form2 text default '', ball2 int default 0, foreign key (user_id) references users (user_id) on delete cascade)",
            "create table if not exists buddy(user_id integer, id int default 0, name text default '', ability int default 0, foreign key (user_id) references users (user_id) on delete cascade)",
            "create table if not exists items(user_id integer, id int default 0, count int default 0, foreign key (user_id) references users (user_id) on delete cascade)",
            "create table if not exists catches(user_id integer, id int default 0, is_shiny int default 0, ball text default '', nickname text default '', species text default '', form text default '', is_egg int default 0, is_favorite int default 0, was_traded int default 0, is_legendary int default 0, is_event int default 0, is_gmax int default 0, foreign key (user_id) references users (user_id) on delete cascade)",
            "create table if not exists binary_catches(user_id integer, id int default 0, data blob default null, foreign key (user_id) references users (user_id) on delete cascade)",
        };

        private readonly string[] TableInsertCommands =
        {
            $"insert into users(user_id, username, catch_count, time_offset, last_played, receive_ping) values({UsersValues})",
            $"insert into trainerinfo(user_id, ot, ot_gender, tid, sid, language) values({TrainerInfoValues})",
            $"insert into dex(user_id, dex_count, entries) values({DexValues})",
            $"insert into perks(user_id, perks, species_boost) values({PerksValues})",
            $"insert into daycare(user_id, shiny1, id1, species1, form1, ball1, shiny2, id2, species2, form2, ball2) values({DaycareValues})",
            $"insert into buddy(user_id, id, name, ability) values({BuddyValues})",
            $"insert into items(user_id, id, count) values({ItemsValues})",
            $"insert into catches(user_id, id, is_shiny, ball, nickname, species, form, is_egg, is_favorite, was_traded, is_legendary, is_event, is_gmax) values({CatchValues})",
        };

        private readonly string[] IndexCreateCommands =
        {
            "create index if not exists catch_index on catches(user_id, id, ball, nickname, species, form, is_shiny, is_egg, is_favorite, was_traded, is_legendary, is_event, is_gmax)",
            "create index if not exists item_index on items(user_id, id)",
            "create index if not exists binary_catches_index on binary_catches(user_id, id)",
        };

        public TradeCordBase()
        {
            if (Dex.Length == 0)
                Dex = GetPokedex();
            Rng = RandomScramble();
        }

        protected A GetRoot<A>(string file) where A : new()
        {
            using StreamReader sr = File.OpenText(file);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            A? root = (A?)serializer.Deserialize(reader, typeof(A));
            return root ?? new();
        }

        private static TCRng RandomScramble()
        {
            return new TCRng()
            {
                CatchRNG = Random.Next(101),
                ShinyRNG = Random.Next(201),
                EggRNG = Random.Next(101),
                EggShinyRNG = Random.Next(201),
                GmaxRNG = Random.Next(101),
                CherishRNG = Random.Next(101),
                SpeciesRNG = Dex[Random.Next(Dex.Length)],
                SpeciesBoostRNG = Random.Next(101),
                ItemRNG = Random.Next(101),
                ShinyCharmRNG = Random.Next(4097),
                LegendaryRNG = Random.Next(101),
            };
        }

        private bool Initialize()
        {
            if (Connected)
                return true;

            try
            {
                DatabasePath = Game == GameVersion.SWSH ? "TradeCord/TradeCordDB_SWSH.db" : "TradeCord/TradeCordDB_BDSP.db";
                Connection = new($"Data Source={DatabasePath};Version=3;");
                Connection.Open();
                Connected = true;
                return true;
            }
            catch (Exception)
            {
                Connection.Dispose();
                Connected = false;
                return false;
            }
        }

        protected TCUser GetCompleteUser(ulong id, string name, bool gift = false)
        {
            TCUser user = new();
            user.UserInfo = GetLookupAsClassObject<TCUserInfo>(id, "users");
            if (user.UserInfo.UserID == 0)
            {
                user.UserInfo.UserID = id;
                user.UserInfo.Username = name;
                InitializeNewUser(id, name);
            }

            user.TrainerInfo = GetLookupAsClassObject<TCTrainerInfo>(id, "trainerinfo");
            user.Buddy = GetLookupAsClassObject<TCBuddy>(id, "buddy");
            user.Daycare = GetLookupAsClassObject<TCDaycare>(id, "daycare");
            user.Items = GetLookupAsClassObject<List<TCItem>>(id, "items");
            user.Dex = GetLookupAsClassObject<TCDex>(id, "dex");
            user.Perks = GetLookupAsClassObject<TCPerks>(id, "perks");
            user.Catches = GetLookupAsClassObject<Dictionary<int, TCCatch>>(id, "catches");

            if (!gift)
            {
                user.UserInfo.LastPlayed = DateTime.Now;
                UpdateRows(id, "users", $"last_played = '{DateTime.Now}'{(name != "" && name != user.UserInfo.Username ? $", username = '{name}'" : "")}");
            }
            return user;
        }

        protected ulong[] GetUsersToPing()
        {
            List<ulong> userIDs = new();
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "select * from users where receive_ping = 1";
            using SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                userIDs.Add(ulong.Parse(reader["user_id"].ToString()));
            return userIDs.ToArray();
        }

        protected bool IsLegendaryOrMythical(int species) => Legal.Legends.Contains(species) || Legal.SubLegends.Contains(species) || Legal.Mythicals.Contains(species);

        protected A GetLookupAsClassObject<A>(ulong id, string table, string filter = "", bool tableJoin = false)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = $"select * from {table} where {(tableJoin ? "c." : "")}user_id = {id} {filter}";
            if (tableJoin)
                table = table.Split(' ')[0];

            using SQLiteDataReader reader = cmd.ExecuteReader();
            object returnObj = table switch
            {
                "dex" => DexReader(reader),
                "perks" => PerkReader(reader),
                "buddy" => BuddyReader(reader),
                "daycare" => DaycareReader(reader),
                "trainerinfo" => TrainerInfoReader(reader),
                "users" => UserInfoReader(reader),
                "items" => ItemReader(reader),
                "catches" => CatchReader(reader, id),
                "binary_catches" => CatchPKMReader(reader),
                _ => throw new NotImplementedException(),
            };
            return (A)returnObj;
        }

        protected void ProcessBulkCommands(List<SQLCommand> cmds, bool delete = false)
        {
            if (delete)
            {
                var cmd = Connection.CreateCommand();
                cmd.CommandText = cmds[0].CommandText;
                var parameters = ParameterConstructor(cmds[0].Names, cmds[0].Values);
                cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
                return;
            }

            using var tran = Connection.BeginTransaction();
            for (int i = 0; i < cmds.Count; i++)
            {
                var cmd = Connection.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = cmds[i].CommandText;
                var parameters = ParameterConstructor(cmds[i].Names, cmds[i].Values);
                cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }

        private SQLiteParameter[] ParameterConstructor(string[]? parameters, object[]? values)
        {
            if (parameters == null || values == null)
                throw new ArgumentNullException();

            SQLiteParameter[] sqParams = new SQLiteParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                sqParams[i] = new() { ParameterName = parameters[i], Value = values[i] };
            return sqParams;
        }

        protected string GetDexFlavorFromTable(int species, int form, bool gmax)
        {
            var cmd = Connection.CreateCommand();
            var selection = gmax ? "gmax" : form == 0 ? "base" : $"form{form}";
            cmd.CommandText = $"select {selection} from dex_flavor where species = {species}";
            using SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                return reader[selection].ToString();
            return "";
        }

        private void InitializeNewUser(ulong id, string name)
        {
            using var tran = Connection.BeginTransaction();
            var cmd = Connection.CreateCommand();
            cmd.CommandText = $"insert into users (user_id, username, last_played) values (@user_id, @username, @last_played)";
            cmd.Parameters.AddWithValue("@user_id", id);
            cmd.Parameters.AddWithValue("@username", name);
            cmd.Parameters.AddWithValue("@last_played", $"{DateTime.Now}");
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"insert into trainerinfo(user_id) values({id})";
            cmd.ExecuteNonQuery();
            cmd.CommandText = $"insert into dex(user_id) values({id})";
            cmd.ExecuteNonQuery();
            cmd.CommandText = $"insert into perks(user_id) values({id})";
            cmd.ExecuteNonQuery();
            cmd.CommandText = $"insert into daycare(user_id) values({id})";
            cmd.ExecuteNonQuery();
            cmd.CommandText = $"insert into buddy(user_id) values({id})";
            cmd.ExecuteNonQuery();
            tran.Commit();
        }

        protected static void UpdateRows(ulong id, string table, string values, string filter = "")
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = $"update {table} set {values} where user_id = {id} {filter}";
            cmd.ExecuteNonQuery();
        }

        protected static void RemoveRows(ulong id, string table, string filter = "")
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = $"delete from {table} where user_id = {id} {filter}";
            cmd.ExecuteNonQuery();
        }

        private Dictionary<int, TCCatch> CatchReader(SQLiteDataReader reader, ulong id)
        {
            Dictionary<int, TCCatch> catches = new();
            while (reader.Read())
            {
                TCCatch entry = new();
                entry.ID = (int)reader["id"];
                entry.Shiny = (int)reader["is_shiny"] != 0;
                entry.Ball = reader["ball"].ToString();
                entry.Nickname = reader["nickname"].ToString();
                entry.Species = reader["species"].ToString();
                entry.Form = reader["form"].ToString();
                entry.Egg = (int)reader["is_egg"] != 0;
                entry.Favorite = (int)reader["is_favorite"] != 0;
                entry.Traded = (int)reader["was_traded"] != 0;
                entry.Legendary = (int)reader["is_legendary"] != 0;
                entry.Event = (int)reader["is_event"] != 0;
                entry.Gmax = (int)reader["is_gmax"] != 0;
                try
                {
                    catches.Add(entry.ID, entry);
                }
                catch
                {
                    Base.LogUtil.LogError("Duplicate entry found, removing...", "[SQL Catch Reader]");
                    RemoveRows(id, "catches", $"and id = {entry.ID}");
                    RemoveRows(id, "binary_catches", $"and id = {entry.ID}");
                }
            }
            return catches;
        }

        private T CatchPKMReader(SQLiteDataReader reader)
        {
            T? pk = null;
            if (reader.Read())
                pk = (T?)PKMConverter.GetPKMfromBytes((byte[])reader["data"]);
            return pk ?? new();
        }

        private List<TCItem> ItemReader(SQLiteDataReader reader)
        {
            List<TCItem> items = new();
            while (reader.Read())
            {
                TCItem item = new();
                item.Item = (TCItems)reader["id"];
                item.ItemCount = (int)reader["count"];
                items.Add(item);
            }
            return items;
        }

        private TCPerks PerkReader(SQLiteDataReader reader)
        {
            TCPerks perks = new();
            if (reader.Read())
            {
                var perkArr = reader["perks"].ToString().Split(',');
                var boost = (int)reader["species_boost"];
                if (perkArr[0] != "")
                {
                    for (int i = 0; i < perkArr.Length; i++)
                        perks.ActivePerks.Add((DexPerks)int.Parse(perkArr[i]));
                }
                perks.SpeciesBoost = boost;
            }
            return perks;
        }

        private TCBuddy BuddyReader(SQLiteDataReader reader)
        {
            TCBuddy buddy = new();
            if (reader.Read())
            {
                buddy.ID = (int)reader["id"];
                buddy.Nickname = reader["name"].ToString();
                buddy.Ability = (Ability)(int)reader["ability"];
            }
            return buddy;
        }

        private TCDex DexReader(SQLiteDataReader reader)
        {
            TCDex dex = new();
            if (reader.Read())
            {
                var dexEntries = reader["entries"].ToString().Split(',');
                var count = (int)reader["dex_count"];
                if (dexEntries[0] != "")
                {
                    for (int i = 0; i < dexEntries.Length; i++)
                        dex.Entries.Add(int.Parse(dexEntries[i]));
                }
                dex.DexCompletionCount = count;
            }
            return dex;
        }

        private TCTrainerInfo TrainerInfoReader(SQLiteDataReader reader)
        {
            TCTrainerInfo info = new();
            if (reader.Read())
            {
                info.OTName = reader["ot"].ToString();
                info.OTGender = reader["ot_gender"].ToString();
                info.TID = (int)reader["tid"];
                info.SID = (int)reader["sid"];
                info.Language = reader["language"].ToString();
            }
            return info;
        }

        private TCDaycare DaycareReader(SQLiteDataReader reader)
        {
            TCDaycare dc = new();
            if (reader.Read())
            {
                dc.ID1 = (int)reader["id1"];
                dc.Species1 = (int)reader["species1"];
                dc.Form1 = reader["form1"].ToString();
                dc.Ball1 = (int)reader["ball1"];
                dc.Shiny1 = (int)reader["shiny1"] != 0;

                dc.ID2 = (int)reader["id2"];
                dc.Species2 = (int)reader["species2"];
                dc.Form2 = reader["form2"].ToString();
                dc.Ball2 = (int)reader["ball2"];
                dc.Shiny2 = (int)reader["shiny2"] != 0;
            }
            return dc;
        }

        private TCUserInfo UserInfoReader(SQLiteDataReader reader)
        {
            TCUserInfo info = new();
            if (reader.Read())
            {
                info.UserID = ulong.Parse(reader["user_id"].ToString());
                info.Username = reader["username"].ToString();
                info.CatchCount = (int)reader["catch_count"];
                info.TimeZoneOffset = (int)reader["time_offset"];
                info.LastPlayed = DateTime.Parse(reader["last_played"].ToString());
                info.ReceiveEventPing = (int)reader["receive_ping"] != 0;
            }
            return info;
        }

        protected static void CleanDatabase()
        {
            try
            {
                var dbPath = typeof(T) == typeof(PK8) ? "TradeCord/TradeCordDB_SWSH.db" : "TradeCord/TradeCordDB_BDSP.db";
                var bckPath = typeof(T) == typeof(PK8) ? "TradeCord/TradeCordDB_SWSH_backup.db" : "TradeCord/TradeCordDB_BDSP_backup.db";
                var bckPath2 = $"{bckPath}2";
                TradeCordHelper<T>.VacuumLock = true;
                Thread.Sleep(0_500);

                if (File.Exists(bckPath))
                {
                    File.Copy(bckPath, bckPath2, true);
                    File.Delete(bckPath);
                }

                var cmd = Connection.CreateCommand();
                cmd.CommandText = $"vacuum main into '{bckPath}'";
                cmd.ExecuteNonQuery();
                Connection.Dispose();
                Connected = false;

                File.Copy(bckPath, dbPath, true);
                if (File.Exists(bckPath2))
                    File.Delete(bckPath2);
            }
            catch (Exception ex)
            {
                Base.LogUtil.LogError($"Failed to vacuum and back up the database.\n{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}", "[SQLite]");
                Connection.Dispose();
                Connected = false;
            }
        }

        protected void ClearInactiveUsers()
        {
            List<ulong> ids = new();
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "select * from users";

            Base.EchoUtil.Echo("Checking for inactive TradeCord users...");
            using SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var date = DateTime.Parse(reader["last_played"].ToString());
                var id = ulong.Parse(reader["user_id"].ToString());
                if (DateTime.Now.Subtract(date).TotalDays >= 30)
                    ids.Add(id);
            }
            reader.Close();

            if (ids.Count > 0)
            {
                var users = string.Join(",", ids);
                cmd.CommandText = $"PRAGMA foreign_keys = ON;delete from users where user_id in ({users})";
                cmd.ExecuteNonQuery();
                Base.EchoUtil.Echo($"Removed {ids.Count} inactive TradeCord users.");
            }
            else Base.EchoUtil.Echo("No inactive TradeCord users found to remove.");
        }

        protected bool CreateDB()
        {
            if (!Initialize())
                return false;

            bool exists = new FileInfo(DatabasePath).Length > 0;
            using var tran = Connection.BeginTransaction();
            var cmd = Connection.CreateCommand();
            if (!exists)
            {
                Base.LogUtil.LogInfo("Beginning to migrate TradeCord to SQLite...", "[SQLite]");
                for (int i = 0; i < TableCreateCommands.Length; i++)
                {
                    cmd.CommandText = TableCreateCommands[i];
                    cmd.ExecuteNonQuery();
                }

                var dex = GetPokedex();
                for (int i = 0; i < dex.Length; i++)
                {
                    var species = dex[i];
                    cmd.CommandText = "insert into dex_flavor(species) values(@species)";
                    cmd.Parameters.AddWithValue("@species", species);
                    cmd.ExecuteNonQuery();

                    TradeExtensions<PK8>.FormOutput(species, 0, out string[] forms);
                    for (int f = 0; f < forms.Length; f++)
                    {
                        var name = SpeciesName.GetSpeciesNameGeneration(species, 2, 8);
                        bool gmax = Game == GameVersion.SWSH && new ShowdownSet($"{name}{forms[f]}").CanToggleGigantamax(species, f);
                        string gmaxFlavor = gmax ? DexText(species, f, true) : "";
                        string flavor = DexText(species, f, false);

                        var vals = gmax && f > 0 ? $"set gmax = '{gmaxFlavor}', form{f} = '{flavor}'" : gmax ? $"set base = '{flavor}', gmax = '{gmaxFlavor}'" : f == 0 ? $"set base = '{flavor}'" : $"set form{f} = '{flavor}'";
                        cmd.CommandText = $"update dex_flavor {vals} where species = {species}";
                        cmd.ExecuteNonQuery();
                    }
                }

                if (!File.Exists("TradeCord/UserInfo.json"))
                {
                    for (int i = 0; i < IndexCreateCommands.Length; i++)
                    {
                        cmd = Connection.CreateCommand();
                        cmd.Transaction = tran;
                        cmd.CommandText = IndexCreateCommands[i];
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            if (File.Exists("TradeCord/UserInfo.json"))
            {
                if (!MigrateToDB())
                {
                    Connection.Dispose();
                    Connected = false;
                    return false;
                }
            }

            Base.LogUtil.LogInfo("Checking if database needs to be updated...", "[SQLite]");
            if (Game == GameVersion.SWSH)
            {
                cmd.CommandText = "create table if not exists legality_fix(issue text not null, fixed int default 0)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into legality_fix(issue,fixed) select 'ht_var', 0 where not exists(select 1 from legality_fix where issue = 'ht_var')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into legality_fix(issue,fixed) select 'egg_bug', 0 where not exists(select 1 from legality_fix where issue = 'egg_bug')";
                cmd.ExecuteNonQuery();

                bool wasFixedHT = false;
                cmd.CommandText = "select * from legality_fix where issue = 'ht_var'";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                    wasFixedHT = (int)reader["fixed"] == 1;
                reader.Close();

                if (!wasFixedHT)
                {
                    Base.LogUtil.LogInfo("Checking for SwSh HT errors...", "[SQLite]");
                    LegalityFixPK8();
                }

                bool wasFixedEgg = false;
                cmd.CommandText = "select * from legality_fix where issue = 'egg_bug'";
                reader = cmd.ExecuteReader();
                if (reader.Read())
                    wasFixedEgg = (int)reader["fixed"] == 1;
                reader.Close();

                if (!wasFixedEgg)
                {
                    Base.LogUtil.LogInfo("Checking for nickname bugs...", "[SQLite]");
                    EggBug();
                }
            }
            else if (Game == GameVersion.BDSP)
            {
                cmd.CommandText = "create table if not exists legality_fix(issue text not null, fixed int default 0)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into legality_fix(issue,fixed) select 'poke_balls', 0 where not exists(select 1 from legality_fix where issue = 'poke_balls')";
                cmd.ExecuteNonQuery();

                bool wasFixedBalls = false;
                cmd.CommandText = "select * from legality_fix where issue = 'poke_balls'";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                    wasFixedBalls = (int)reader["fixed"] == 1;
                reader.Close();

                if (!wasFixedBalls)
                {
                    Base.LogUtil.LogInfo("Checking for BDSP Poke Ball and contest stat errors...", "[SQLite]");
                    LegalityFixBDSP();
                }
            }

            cmd.CommandText = "select receive_ping from users";
            try
            {
                var reader = cmd.ExecuteReader();
                reader.Close();
            }
            catch
            {
                Base.LogUtil.LogInfo("Adding missing columns to the database...", "[SQLite]");
                cmd.CommandText = "alter table users add column receive_ping int default 0";
                cmd.ExecuteNonQuery();
            }

            cmd.CommandText = "select is_gmax from catches";
            try
            {
                var reader = cmd.ExecuteReader();
                reader.Close();
            }
            catch
            {
                cmd.CommandText = "alter table catches add column is_gmax int default 0";
                cmd.ExecuteNonQuery();

                Dictionary<int, (ulong, int, PKM)> catches = new();
                int index = 0;
                cmd.CommandText = "select * from binary_catches";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ulong id = ulong.Parse(reader["user_id"].ToString());
                    int catchID = (int)reader["id"];
                    var pk = (T?)PKMConverter.GetPKMfromBytes((byte[])reader["data"]);
                    if (pk == null)
                        continue;

                    catches.Add(index, (id, catchID, pk));
                    index++;
                }
                reader.Close();

                foreach (var entry in catches)
                {
                    ShowdownSet? set = ShowdownUtil.ConvertToShowdown(ShowdownParsing.GetShowdownText(entry.Value.Item3));
                    if (set == null)
                        continue;

                    cmd.CommandText = "update catches set is_gmax = ? where user_id = ? and id = ?";
                    cmd.Parameters.AddWithValue("@is_gmax", set.CanGigantamax);
                    cmd.Parameters.AddWithValue("@user_id", entry.Value.Item1);
                    cmd.Parameters.AddWithValue("@id", entry.Value.Item2);
                    cmd.ExecuteNonQuery();
                }
            }

            cmd.CommandText = "insert into legality_fix(issue,fixed) select 'gmax', 0 where not exists(select 1 from legality_fix where issue = 'gmax')";
            cmd.ExecuteNonQuery();

            bool wasFixedGmax = false;
            cmd.CommandText = "select * from legality_fix where issue = 'gmax'";
            var readerG = cmd.ExecuteReader();
            if (readerG.Read())
                wasFixedGmax = (int)readerG["fixed"] == 1;
            readerG.Close();

            if (!wasFixedGmax)
            {
                Base.LogUtil.LogInfo("Checking for incorrect Gmax flags...", "[SQLite]");
                GmaxFix();
            }

            tran.Commit();
            Base.LogUtil.LogInfo("Database checks complete.", "[SQLite]");
            return true;
        }

        private bool MigrateToDB()
        {
            // Check if mode is set to BDSP. SQLite migration came before BDSP, so only SwSh can migrate.
            if (Game == GameVersion.BDSP)
            {
                Base.LogUtil.LogError("In order to migrate a SwSh database correctly, please switch mode to \"1\" before initializing migration.", "[SQLite]");
                return false;
            }

            try
            {
                var users = GetRoot<TCUserInfoRoot>("TradeCord/UserInfo.json").Users;
                using var tran = Connection.BeginTransaction();
                for (int u = 0; u < users.Count; u++)
                {
                    var dir = $"TradeCord\\{users[u].UserID}";
                    string dexStr = "";

                    var dexArr = users[u].Dex.ToArray();
                    for (int i = 0; i < dexArr.Length; i++)
                        dexStr += $"{dexArr[i]}{(i + 1 < dexArr.Length ? "," : "")}";

                    string perkStr = "";
                    for (int i = 0; i < users[u].ActivePerks.Count; i++)
                        perkStr += $"{(int)users[u].ActivePerks[i]}{(i + 1 < users[u].ActivePerks.Count ? "," : "")}";

                    string favStr = "";
                    var favArr = users[u].Favorites.ToArray();
                    for (int i = 0; i < favArr.Length; i++)
                        favStr += $"{favArr[i]}{(i + 1 < favArr.Length ? "," : "")}";

                    var itemList = users[u].Items.ToList();
                    if (users[u].DexCompletionCount >= 1 && itemList.FirstOrDefault(x => x.Item == TCItems.ShinyCharm) == default)
                        itemList.Add(new() { Item = TCItems.ShinyCharm, ItemCount = 1 });

                    var cmd = Connection.CreateCommand();
                    cmd.Transaction = tran;
                    for (int i = 0; i < TableInsertCommands.Length; i++)
                    {
                        var enumType = (TableEnum)i;
                        switch (enumType)
                        {
                            case TableEnum.Users:
                                {
                                    cmd.CommandText = TableInsertCommands[i];
                                    cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                    cmd.Parameters.AddWithValue("@username", users[u].Username);
                                    cmd.Parameters.AddWithValue("@catch_count", users[u].CatchCount);
                                    cmd.Parameters.AddWithValue("@time_offset", users[u].TimeZoneOffset);
                                    cmd.Parameters.AddWithValue("@last_played", $"{DateTime.Now}");
                                    cmd.Parameters.AddWithValue("@receive_ping", 0);
                                    cmd.ExecuteNonQuery();
                                }; break;
                            case TableEnum.TrainerInfo:
                                {
                                    cmd.CommandText = TableInsertCommands[i];
                                    cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                    cmd.Parameters.AddWithValue("@ot", users[u].OTName == "" ? "Carp" : users[u].OTName);
                                    cmd.Parameters.AddWithValue("@ot_gender", users[u].OTGender == "" ? "Male" : users[u].OTGender);
                                    cmd.Parameters.AddWithValue("@tid", users[u].TID);
                                    cmd.Parameters.AddWithValue("@sid", users[u].SID);
                                    cmd.Parameters.AddWithValue("@language", users[u].Language == "" ? "English" : users[u].Language);
                                    cmd.ExecuteNonQuery();
                                }; break;
                            case TableEnum.Dex:
                                {
                                    cmd.CommandText = TableInsertCommands[i];
                                    cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                    cmd.Parameters.AddWithValue("@dex_count", users[u].DexCompletionCount);
                                    cmd.Parameters.AddWithValue("@entries", dexStr);
                                    cmd.ExecuteNonQuery();
                                }; break;
                            case TableEnum.Perks:
                                {
                                    cmd.CommandText = TableInsertCommands[i];
                                    cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                    cmd.Parameters.AddWithValue("@perks", perkStr);
                                    cmd.Parameters.AddWithValue("@species_boost", users[u].SpeciesBoost);
                                    cmd.ExecuteNonQuery();
                                }; break;
                            case TableEnum.Daycare:
                                {
                                    cmd.CommandText = TableInsertCommands[i];
                                    cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                    cmd.Parameters.AddWithValue("@shiny1", users[u].Daycare1.Shiny);
                                    cmd.Parameters.AddWithValue("@id1", users[u].Daycare1.ID);
                                    cmd.Parameters.AddWithValue("@species1", users[u].Daycare1.Species);
                                    cmd.Parameters.AddWithValue("@form1", users[u].Daycare1.Form);
                                    cmd.Parameters.AddWithValue("@ball1", users[u].Daycare1.Ball);

                                    cmd.Parameters.AddWithValue("@shiny2", users[u].Daycare2.Shiny);
                                    cmd.Parameters.AddWithValue("@id2", users[u].Daycare2.ID);
                                    cmd.Parameters.AddWithValue("@species2", users[u].Daycare2.Species);
                                    cmd.Parameters.AddWithValue("@form2", users[u].Daycare2.Form);
                                    cmd.Parameters.AddWithValue("@ball2", users[u].Daycare2.Ball);
                                    cmd.ExecuteNonQuery();
                                }; break;
                            case TableEnum.Buddy:
                                {
                                    cmd.CommandText = TableInsertCommands[i];
                                    cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                    cmd.Parameters.AddWithValue("@id", users[u].Buddy.ID);
                                    cmd.Parameters.AddWithValue("@name", users[u].Buddy.Nickname);
                                    cmd.Parameters.AddWithValue("@ability", users[u].Buddy.Ability);
                                    cmd.ExecuteNonQuery();
                                }; break;
                            case TableEnum.Items:
                                {
                                    if (itemList.Count > 0)
                                    {
                                        for (int it = 0; it < itemList.Count; it++)
                                        {
                                            if (itemList[it].Item == TCItems.ShinyCharm)
                                                continue;

                                            cmd.CommandText = TableInsertCommands[i];
                                            cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                            cmd.Parameters.AddWithValue("@id", (int)itemList[it].Item);
                                            cmd.Parameters.AddWithValue("@count", itemList[it].ItemCount);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }

                                    var sc = itemList.FirstOrDefault(x => x.Item == TCItems.ShinyCharm);
                                    var count = users[u].DexCompletionCount + users[u].ActivePerks.Count + (sc != default ? sc.ItemCount : 0);
                                    if (count > 0)
                                    {
                                        cmd.CommandText = TableInsertCommands[i];
                                        cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                        cmd.Parameters.AddWithValue("@id", (int)TCItems.ShinyCharm);
                                        cmd.Parameters.AddWithValue("@count", count);
                                        cmd.ExecuteNonQuery();
                                    }
                                }; break;
                            case TableEnum.Catches:
                                {
                                    var catches = users[u].Catches.ToList();
                                    if (catches.Count > 0)
                                    {
                                        for (int c = 0; c < catches.Count; c++)
                                        {
                                            bool dupe = catches.FindAll(x => x.ID == catches[c].ID).Count > 1;
                                            if (dupe)
                                            {
                                                var array = Directory.GetFiles(dir).Where(x => x.Contains(".pk")).Select(x => int.Parse(x.Split('\\')[2].Split('-', '_')[0].Replace("★", "").Trim())).ToArray();
                                                array = array.OrderBy(x => x).ToArray();
                                                catches[c].ID = Indexing(array);
                                            }

                                            T? pk = null;
                                            if (File.Exists(catches[c].Path))
                                                pk = (T?)PKMConverter.GetPKMfromBytes(File.ReadAllBytes(catches[c].Path));
                                            if (pk == null)
                                                continue;

                                            ShowdownSet? set = ShowdownUtil.ConvertToShowdown(ShowdownParsing.GetShowdownText(pk));
                                            if (set == null)
                                                continue;

                                            cmd.CommandText = "insert into binary_catches(user_id, id, data) values(@user_id, @id, @data)";
                                            cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                            cmd.Parameters.AddWithValue("@id", catches[c].ID);
                                            cmd.Parameters.AddWithValue("@data", pk.DecryptedPartyData);
                                            cmd.ExecuteNonQuery();

                                            cmd.CommandText = TableInsertCommands[i];
                                            cmd.Parameters.AddWithValue("@user_id", users[u].UserID);
                                            cmd.Parameters.AddWithValue("@is_shiny", catches[c].Shiny);
                                            cmd.Parameters.AddWithValue("@id", catches[c].ID);
                                            cmd.Parameters.AddWithValue("@ball", catches[c].Ball);
                                            cmd.Parameters.AddWithValue("@nickname", pk.Nickname);
                                            cmd.Parameters.AddWithValue("@species", catches[c].Species);
                                            cmd.Parameters.AddWithValue("@form", catches[c].Form);
                                            cmd.Parameters.AddWithValue("@is_egg", catches[c].Egg);
                                            cmd.Parameters.AddWithValue("@is_favorite", favArr.Contains(catches[c].ID));
                                            cmd.Parameters.AddWithValue("@was_traded", catches[c].Traded);
                                            cmd.Parameters.AddWithValue("@is_legendary", IsLegendaryOrMythical(pk.Species));
                                            cmd.Parameters.AddWithValue("@is_event", pk.FatefulEncounter);
                                            cmd.Parameters.AddWithValue("@is_gmax", set.CanGigantamax);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }; break;
                            default: throw new NotImplementedException();
                        };
                    }
                }

                for (int i = 0; i < IndexCreateCommands.Length; i++)
                {
                    var cmd = Connection.CreateCommand();
                    cmd.Transaction = tran;
                    cmd.CommandText = IndexCreateCommands[i];
                    cmd.ExecuteNonQuery();
                }
                tran.Commit();

                var dirs = Directory.GetDirectories("TradeCord");
                for (int i = 0; i < dirs.Length; i++)
                    Directory.Delete(dirs[i], true);

                if (File.Exists("TradeCord/UserInfo_sqlbackup.json"))
                    File.Delete("TradeCord/UserInfo_sqlbackup.json");
                File.Move("TradeCord/UserInfo.json", "TradeCord/UserInfo_sqlbackup.json");
                return true;
            }
            catch (Exception ex)
            {
                Base.LogUtil.LogError($"Failed to migrate database.\n{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}", "[SQLite Migration]");
                return false;
            }
        }

        private string DexText(int species, int form, bool gmax)
        {
            bool patterns = form > 0 && species is (int)Arceus or (int)Unown or (int)Deoxys or (int)Burmy or (int)Wormadam or (int)Mothim or (int)Vivillon or (int)Furfrou;
            if (FormInfo.IsBattleOnlyForm(species, form, 8) || FormInfo.IsFusedForm(species, form, 8) || FormInfo.IsTotemForm(species, form, 8) || patterns)
                return "";

            var resourcePath = "SysBot.Pokemon.TradeCord.Resources.DexFlavor.txt";
            using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath));
            if (form > 0 && IsLegendaryOrMythical(species))
                form = 0;

            if (!gmax)
            {
                var index = species == (int)Slowbro && form == 2 ? 0 : form - 1;
                if (form > 0)
                    return reader.ReadToEnd().Split('_')[1].Split('\n')[species].Split('|')[index].Replace("'", "''");
                else return reader.ReadToEnd().Split('\n')[species].Replace("'", "''");
            }

            string[] str = reader.ReadToEnd().Split('_')[1].Split('\n')[species].Split('|');
            return str[^1].Replace("'", "''");
        }

        private int[] GetPokedex()
        {
            List<int> dex = new();
            for (int i = 1; i < (Game == GameVersion.BDSP ? 494 : 899); i++)
            {
                var entry = Game == GameVersion.SWSH ? PersonalTable.SWSH.GetFormEntry(i, 0) : PersonalTable.BDSP.GetFormEntry(i, 0);
                if ((Game == GameVersion.SWSH && entry is PersonalInfoSWSH { IsPresentInGame: false }) || (Game == GameVersion.BDSP && entry is PersonalInfoBDSP { IsPresentInGame: false }))
                    continue;

                var species = SpeciesName.GetSpeciesNameGeneration(i, 2, 8);
                var set = new ShowdownSet($"{species}{(i == (int)NidoranF ? "-F" : i == (int)NidoranM ? "-M" : "")}");
                var template = AutoLegalityWrapper.GetTemplate(set);
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                _ = (T)sav.GetLegal(template, out string result);

                if (result == "Regenerated")
                    dex.Add(i);
            }
            return dex.ToArray();
        }

        protected bool BaseCanBeEgg(int species, int form, out int baseForm, out int baseSpecies)
        {
            baseSpecies = -1;
            baseForm = 0;
            var name = SpeciesName.GetSpeciesNameGeneration(species, 2, 8);
            var formStr = TradeExtensions<PK8>.FormOutput(species, form, out _);
            if (name.Contains("Nidoran"))
                name = name.Remove(name.Length - 1);

            var set = new ShowdownSet($"{name}{(species == (int)NidoranF ? "-F" : species == (int)NidoranM ? "-M" : formStr)}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pkm = (T)sav.GetLegal(template, out string result);
            if (result != "Regenerated")
                return false;

            var table = EvolutionTree.GetEvolutionTree(pkm, 8);
            var evos = table.GetValidPreEvolutions(pkm, 100, 8, true);
            var encs = EncounterEggGenerator.GenerateEggs(pkm, evos, 8, true).ToArray();
            if (encs.Length == 0 || !Breeding.CanHatchAsEgg(species) || !Breeding.CanHatchAsEgg(species, form, 8))
                return false;

            baseSpecies = encs[^1].Species;
            if (GameData.GetPersonal(Game).GetFormEntry(baseSpecies, form).IsFormWithinRange(form) && Breeding.CanHatchAsEgg(baseSpecies, form, 8))
                baseForm = species is (int)Darmanitan && form <= 1 ? 0 : form;
            else baseForm = encs[^1].Form;
            return true;
        }

        private void LegalityFixPK8()
        {
            Base.EchoUtil.Echo("Beginning to scan for and fix legality errors. This may take a while.");
            int updated = 0;
            List<SQLCommand> cmds = new();
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "select * from binary_catches";

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bool write = false;
                ulong user_id = ulong.Parse(reader["user_id"].ToString());
                int catch_id = (int)reader["id"];
                var pk = (T?)PKMConverter.GetPKMfromBytes((byte[])reader["data"]) ?? new();

                var la = new LegalityAnalysis(pk);
                if (!la.Valid)
                {
                    var sav = new SimpleTrainerInfo() { OT = pk.OT_Name, Gender = pk.OT_Gender, Generation = pk.Generation, Language = pk.Language, SID = pk.TrainerSID7, TID = pk.TrainerID7 };
                    var results = la.Results.FirstOrDefault(x => !x.Valid && x.Identifier != CheckIdentifier.Memory);
                    pk.SetHandlerandMemory(sav);
                    if (results != default)
                    {
                        switch (results.Identifier)
                        {
                            case CheckIdentifier.Evolution:
                                {
                                    if (pk.Species == (int)Lickilicky && pk.Met_Location != 162 && pk.Met_Location != 244 && !pk.RelearnMoves.Contains(205))
                                        SetMoveOrRelearnByIndex(pk, 205, true);
                                }; break;
                            case CheckIdentifier.Encounter:
                                {
                                    if (pk.Met_Location == 162)
                                    {
                                        pk.SetAbilityIndex(0);
                                        while (!new LegalityAnalysis(pk).Valid && pk.Met_Level < 60)
                                        {
                                            pk.Met_Level += 1;
                                            if (pk.CurrentLevel < pk.Met_Level)
                                                pk.CurrentLevel = pk.Met_Level + 1;
                                        }
                                    }
                                }; break;
                            case CheckIdentifier.Form:
                                {
                                    if (pk.Species == (int)Keldeo && pk.Form == 1 && !pk.Moves.Contains(548))
                                        SetMoveOrRelearnByIndex(pk, 548, false);
                                }; break;
                            case CheckIdentifier.Nickname:
                                {
                                    if (la.EncounterMatch is MysteryGift mg)
                                    {
                                        var mgPkm = mg.ConvertToPKM(sav);
                                        if (mgPkm.IsNicknamed)
                                            pk.SetNickname(mgPkm.Nickname);
                                        else pk.SetDefaultNickname(la);
                                        pk.SetHandlerandMemory(sav);
                                    }
                                    else pk.SetDefaultNickname(la);
                                }; break;
                        };
                    }

                    la = new LegalityAnalysis(pk);
                    if (!la.Valid)
                    {
                        Base.LogUtil.LogError($"Catch {catch_id} (user {user_id}) is illegal, trying to legalize.", "[SQLite]");
                        pk = (T)AutoLegalityWrapper.LegalizePokemon(pk);
                        if (!new LegalityAnalysis(pk).Valid)
                        {
                            Base.LogUtil.LogError($"Failed to legalize, removing entry...\n{la.Report()}", "[SQLite]");
                            var namesR = new string[] { "@user_id", "@id" };
                            var objR = new object[] { user_id, catch_id };
                            cmds.Add(new() { CommandText = "delete from binary_catches where user_id = ? and id = ?", Names = namesR, Values = objR });
                            cmds.Add(new() { CommandText = "delete from catches where user_id = ? and id = ?", Names = namesR, Values = objR });
                            updated++;
                            continue;
                        }
                    }
                    else write = true;
                }

                if (write)
                {
                    var names = new string[] { "@data", "@user_id", "@id" };
                    var obj = new object[] { pk.DecryptedPartyData, user_id, catch_id };
                    cmds.Add(new() { CommandText = "update binary_catches set data = ? where user_id = ? and id = ?", Names = names, Values = obj });

                    names = new string[] { "@is_shiny", "@ball", "@nickname", "@form", "@is_egg", "@is_event", "@user_id", "@id" };
                    obj = new object[] { pk.IsShiny, $"{(Ball)pk.Ball}", pk.Nickname, TradeExtensions<PK8>.FormOutput(pk.Species, pk.Form, out _), pk.IsEgg, pk.FatefulEncounter, user_id, catch_id };
                    cmds.Add(new() { CommandText = "update catches set is_shiny = ?, ball = ?, nickname = ?, form = ?, is_egg = ?, is_event = ? where user_id = ? and id = ?", Names = names, Values = obj });
                    updated++;
                }
            }
            reader.Close();

            if (updated > 0)
            {
                using var tran = Connection.BeginTransaction();
                for (int i = 0; i < cmds.Count; i++)
                {
                    cmd.Transaction = tran;
                    cmd.CommandText = cmds[i].CommandText;
                    var parameters = ParameterConstructor(cmds[i].Names, cmds[i].Values);
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = $"update legality_fix set fixed = 1 where issue = 'ht_var'";
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
            Base.EchoUtil.Echo($"Scan complete! Updated {updated} records.");
        }

        private void EggBug()
        {
            Base.EchoUtil.Echo("Beginning to scan for species nicknamed \"Egg\". This may take a while.");
            List<SQLCommand> cmds = new();
            int updated = 0;

            var cmd = Connection.CreateCommand();
            cmd.CommandText = "select * from binary_catches b inner join catches c on b.user_id = c.user_id and b.id = c.id";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ulong user_id = ulong.Parse(reader["user_id"].ToString());
                int catch_id = (int)reader["id"];
                string nickname = reader["nickname"].ToString();
                var pk = (T?)PKMConverter.GetPKMfromBytes((byte[])reader["data"]) ?? new();
                var nick = pk.Language switch
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

                if ((pk.Nickname == nick || nickname == nick || nickname == "Egg") && !pk.IsEgg)
                {
                    pk.IsNicknamed = false;
                    pk.Nickname = SpeciesName.GetSpeciesNameGeneration(pk.Species, pk.Language, 8);
                    var la = new LegalityAnalysis(pk);
                    if (la.Valid)
                    {
                        var names = new string[] { "@data", "@user_id", "@id" };
                        var obj = new object[] { pk.DecryptedPartyData, user_id, catch_id };
                        cmds.Add(new() { CommandText = "update binary_catches set data = ? where user_id = ? and id = ?", Names = names, Values = obj });

                        names = new string[] { "@nickname", "@user_id", "@id" };
                        obj = new object[] { pk.Nickname, user_id, catch_id };
                        cmds.Add(new() { CommandText = "update catches set nickname = ? where user_id = ? and id = ?", Names = names, Values = obj });

                        names = new string[] { "@name", "@ability", "@user_id", "@id" };
                        obj = new object[] { pk.Nickname, pk.Ability, user_id, catch_id };
                        cmds.Add(new() { CommandText = "update buddy set name = ?, ability = ? where user_id = ? and id = ?", Names = names, Values = obj });
                        updated++;
                    }
                    else Base.LogUtil.LogError($"Catch {catch_id} (user {user_id}) is illegal.", "[SQLite]");
                }
            }
            reader.Close();

            if (updated >= 0)
            {
                using var tran = Connection.BeginTransaction();
                for (int i = 0; i < cmds.Count; i++)
                {
                    cmd = Connection.CreateCommand();
                    cmd.Transaction = tran;
                    cmd.CommandText = cmds[i].CommandText;
                    var parameters = ParameterConstructor(cmds[i].Names, cmds[i].Values);
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = $"update legality_fix set fixed = 1 where issue = 'egg_bug'";
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
            Base.EchoUtil.Echo($"Scan complete! Updated {updated} records.");
        }

        private void LegalityFixBDSP()
        {
            Base.EchoUtil.Echo("Beginning to scan for and fix legality errors. This may take a while.");
            int updated = 0;
            List<SQLCommand> cmds = new();
            var cmd = Connection.CreateCommand();

            cmd.CommandText = "select * from binary_catches";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bool write = false;
                ulong user_id = ulong.Parse(reader["user_id"].ToString());
                int catch_id = (int)reader["id"];
                var pk = (T?)PKMConverter.GetPKMfromBytes((byte[])reader["data"], 8) ?? new();

                var la = new LegalityAnalysis(pk);
                if (!la.Valid)
                {
                    var results = la.Results.ToList().FindAll(x => !x.Valid && x.Identifier == CheckIdentifier.Ball || x.Identifier == CheckIdentifier.Memory || x.Identifier == CheckIdentifier.Encounter);
                    if (results.Count > 0)
                    {
                        foreach (var result in results)
                        {
                            switch (result.Identifier)
                            {
                                case CheckIdentifier.Ball:
                                    {
                                        if (user_id == 459118697949298689 && catch_id == 2604)
                                            Thread.Sleep(1);

                                        var balls = TradeExtensions<T>.GetLegalBalls(ShowdownParsing.GetShowdownText(pk)).ToList();
                                        if ((balls.Contains(Ball.Master) || balls.Contains(Ball.Cherish)) && (pk.WasEgg || pk.WasTradedEgg))
                                        {
                                            balls.Remove(Ball.Master);
                                            balls.Remove(Ball.Cherish);
                                        }
                                        pk.Ball = (int)balls[Random.Next(balls.Count)];
                                    }; break;
                                case CheckIdentifier.Memory: pk.SetSuggestedMemories(); pk.SetSuggestedContestStats(la.EncounterMatch); break;
                                case CheckIdentifier.Encounter when results.FirstOrDefault(x => x.Identifier == CheckIdentifier.Ball || x.Identifier == CheckIdentifier.Memory) == default:
                                    {
                                        List<string> extra = new();
                                        extra.AddRange(new string[]
                                        {
                                            $"Ball: {(Ball)pk.Ball}",
                                            $"OT: {pk.OT_Name}",
                                            $"OTGender: {(Gender)pk.OT_Gender}",
                                            $"TID: {pk.TrainerID7}",
                                            $"SID: {pk.TrainerSID7}",
                                            $"Language: {(LanguageID)pk.Language}",
                                        });

                                        var showdown = ShowdownParsing.GetShowdownText(pk).Replace("\r", "").Split('\n').ToList();
                                        showdown.InsertRange(1, extra);
                                        var set = new ShowdownSet(string.Join("\r\n", showdown));
                                        var template = AutoLegalityWrapper.GetTemplate(set);
                                        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                                        pk = (T)sav.GetLegal(template, out _);
                                    }; break;
                            }
                        }
                    }

                    la = new LegalityAnalysis(pk);
                    if (!la.Valid)
                    {
                        Base.LogUtil.LogError($"Catch {catch_id} (user {user_id}) is illegal, trying to legalize.", "[SQLite]");
                        pk = (T)AutoLegalityWrapper.LegalizePokemon(pk);
                        if (!new LegalityAnalysis(pk).Valid)
                        {
                            Base.LogUtil.LogError($"Failed to legalize, removing entry...\n{la.Report()}", "[SQLite]");
                            var namesR = new string[] { "@user_id", "@id" };
                            var objR = new object[] { user_id, catch_id };
                            cmds.Add(new() { CommandText = "delete from binary_catches where user_id = ? and id = ?", Names = namesR, Values = objR });
                            cmds.Add(new() { CommandText = "delete from catches where user_id = ? and id = ?", Names = namesR, Values = objR });
                            updated++;
                            continue;
                        }
                    }
                    else write = true;
                }

                if (write)
                {
                    var form = TradeExtensions<PB8>.FormOutput(pk.Species, pk.Form, out _);
                    var names = new string[] { "@data", "@user_id", "@id" };
                    var obj = new object[] { pk.DecryptedPartyData, user_id, catch_id };
                    cmds.Add(new() { CommandText = "update binary_catches set data = ? where user_id = ? and id = ?", Names = names, Values = obj });

                    names = new string[] { "@is_shiny", "@ball", "@nickname", "@form", "@is_egg", "@is_event", "@user_id", "@id" };
                    obj = new object[] { pk.IsShiny, $"{(Ball)pk.Ball}", pk.Nickname, form, pk.IsEgg, pk.FatefulEncounter, user_id, catch_id };
                    cmds.Add(new() { CommandText = "update catches set is_shiny = ?, ball = ?, nickname = ?, form = ?, is_egg = ?, is_event = ? where user_id = ? and id = ?", Names = names, Values = obj });
                    updated++;
                }
            }
            reader.Close();

            if (updated >= 0)
            {
                using var tran = Connection.BeginTransaction();
                for (int i = 0; i < cmds.Count; i++)
                {
                    cmd = Connection.CreateCommand();
                    cmd.Transaction = tran;
                    cmd.CommandText = cmds[i].CommandText;
                    var parameters = ParameterConstructor(cmds[i].Names, cmds[i].Values);
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = $"update legality_fix set fixed = 1 where issue = 'poke_balls'";
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
            Base.EchoUtil.Echo($"Scan complete! Updated {updated} records.");
        }

        private void GmaxFix()
        {
            Base.EchoUtil.Echo("Beginning to scan for improper Gmax flags. This may take a while.");
            int updated = 0;
            List<SQLCommand> cmds = new();
            var cmd = Connection.CreateCommand();
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();

            cmd.CommandText = "select * from catches where is_gmax = 1";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string result = string.Empty;
                if (Game == GameVersion.SWSH)
                {
                    var species = reader["species"].ToString();
                    var form = reader["form"].ToString();
                    var set = new ShowdownSet($"{species}{form}-Gmax");
                    var template = AutoLegalityWrapper.GetTemplate(set);
                    _ = (T)sav.GetLegal(template, out result);
                }

                if (result != "Regenerated")
                {
                    var user = ulong.Parse(reader["user_id"].ToString());
                    var id = (int)reader["id"];
                    var names = new string[] { "@is_gmax", "@user_id", "@id" };
                    var obj = new object[] { false, user, id };
                    cmds.Add(new() { CommandText = "update catches set is_gmax = ? where user_id = ? and id = ?", Names = names, Values = obj });
                    updated++;
                }
            }
            reader.Close();

            if (updated >= 0)
            {
                using var tran = Connection.BeginTransaction();
                for (int i = 0; i < cmds.Count; i++)
                {
                    cmd = Connection.CreateCommand();
                    cmd.Transaction = tran;
                    cmd.CommandText = cmds[i].CommandText;
                    var parameters = ParameterConstructor(cmds[i].Names, cmds[i].Values);
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = $"update legality_fix set fixed = 1 where issue = 'gmax'";
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
            Base.EchoUtil.Echo($"Scan complete! Updated {updated} records.");
        }

        private void SetMoveOrRelearnByIndex(T pk, int move, bool relearn)
        {
            int index = relearn ? pk.RelearnMoves.ToList().IndexOf(0) : pk.Moves.ToList().IndexOf(0);
            if (index == -1 && !relearn)
                pk.Move4 = move;
            else if (index == -1 && relearn)
                return;

            switch (index)
            {
                case 0: _ = relearn ? pk.RelearnMove1 = move : pk.Move1 = move; break;
                case 1: _ = relearn ? pk.RelearnMove2 = move : pk.Move2 = move; break;
                case 2: _ = relearn ? pk.RelearnMove3 = move : pk.Move3 = move; break;
                case 3: _ = relearn ? pk.RelearnMove4 = move : pk.Move4 = move; break;
            };
            pk.HealPP();
        }

        protected int Indexing(int[] array)
        {
            var i = 0;
            return array.Where(x => x > 0).Distinct().OrderBy(x => x).Any(x => x != (i += 1)) ? i : i + 1;
        }

        private static List<EvolutionTemplate> EvolutionRequirements()
        {
            var list = new List<EvolutionTemplate>();
            for (int i = 1; i < (Game == GameVersion.BDSP ? 494 : 899); i++)
            {
                var temp = new T { Species = i };
                for (int f = 0; f < temp.PersonalInfo.FormCount; f++)
                {
                    if (i is (int)Pikachu && f is 8)
                        continue;

                    string gender = i switch
                    {
                        (int)Meowstic or (int)Wormadam when f > 0 => " (F)",
                        (int)Salazzle or (int)Froslass or (int)Vespiquen => " (F)",
                        (int)Gallade => "(M)",
                        (int)Mothim => "(M)",
                        _ => "",
                    };

                    var set = new ShowdownSet($"{SpeciesName.GetSpeciesNameGeneration(i, 2, 8)}{TradeExtensions<T>.FormOutput(i, f, out _)}{gender}");
                    var templateS = AutoLegalityWrapper.GetTemplate(set);
                    var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                    var blank = sav.GetLegal(templateS, out string result);
                    if (result != "Regenerated")
                        continue;

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

                            if (evoType is EvolutionType.LevelUpElectric or EvolutionType.LevelUpForest or EvolutionType.LevelUpCold or EvolutionType.LevelUpSummit or EvolutionType.LevelUpWeather or EvolutionType.LevelUpWithTeammate or EvolutionType.LevelUpBeauty)
                                evoType = EvolutionType.LevelUp;

                            if (evoType == EvolutionType.TradeHeldItem || evoType == EvolutionType.UseItem || evoType == EvolutionType.UseItemFemale || evoType == EvolutionType.UseItemMale || evoType == EvolutionType.LevelUpHeldItemDay || evoType == EvolutionType.LevelUpHeldItemNight || evoType == EvolutionType.Spin)
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

                            if (preEvos[c].Species == (int)Cosmoem)
                                template.EvolvesAtLevel = 53;
                            else if (preEvos[c].Species == (int)Tangrowth)
                                template.EvolvesAtLevel = 24;
                            else if (preEvos[c].Species == (int)Ambipom)
                                template.EvolvesAtLevel = 31;

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
                (int)Vaporeon or (int)Poliwrath or (int)Cloyster or (int)Starmie or (int)Ludicolo or (int)Simipour => TCItems.WaterStone,
                (int)Jolteon or (int)Raichu or (int)Magnezone or (int)Eelektross or (int)Vikavolt => TCItems.ThunderStone,
                (int)Ninetales or (int)Sandshrew when form > 0 => TCItems.IceStone,
                (int)Flareon or (int)Ninetales or (int)Arcanine or (int)Simisear => TCItems.FireStone,
                (int)Leafeon or (int)Vileplume or (int)Victreebel or (int)Exeggutor or (int)Shiftry or (int)Simisage => TCItems.LeafStone,
                (int)Glaceon => TCItems.IceStone,
                (int)Darmanitan when form == 2 => TCItems.IceStone,
                (int)Nidoqueen or (int)Nidoking or (int)Clefable or (int)Wigglytuff or (int)Delcatty or (int)Musharna => TCItems.MoonStone,
                (int)Bellossom or (int)Sunflora or (int)Whimsicott or (int)Lilligant or (int)Heliolisk or (int)Sunflora => TCItems.SunStone,
                (int)Togekiss or (int)Roserade or (int)Cinccino or (int)Florges => TCItems.ShinyStone,
                (int)Honchkrow or (int)Mismagius or (int)Chandelure or (int)Aegislash => TCItems.DuskStone,
                (int)Gallade or (int)Froslass => TCItems.DawnStone,
                (int)Polteageist => form == 0 ? TCItems.CrackedPot : TCItems.ChippedPot,
                (int)Appletun => TCItems.SweetApple,
                (int)Flapple => TCItems.TartApple,
                (int)Slowbro when form > 0 => TCItems.GalaricaCuff,
                (int)Slowking when form > 0 => TCItems.GalaricaWreath,
                (int)Slowking or (int)Politoed => TCItems.KingsRock,

                // Held item
                (int)Kingdra => TCItems.DragonScale,
                (int)PorygonZ => TCItems.DubiousDisc,
                (int)Electivire => TCItems.Electirizer,
                (int)Magmortar => TCItems.Magmarizer,
                (int)Steelix or (int)Scizor => TCItems.MetalCoat,
                (int)Chansey => TCItems.OvalStone,
                (int)Milotic => TCItems.PrismScale,
                (int)Huntail => TCItems.DeepSeaTooth,
                (int)Gorebyss => TCItems.DeepSeaScale,
                (int)Rhyperior => TCItems.Protector,
                (int)Weavile => TCItems.RazorClaw,
                (int)Dusknoir => TCItems.ReaperCloth,
                (int)Aromatisse => TCItems.Sachet,
                (int)Porygon2 => TCItems.Upgrade,
                (int)Slurpuff => TCItems.WhippedDream,
                (int)Alcremie => TCItems.Sweets,
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

        protected class EvolutionTemplate
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

        public sealed class SQLCommand
        {
            public string CommandText { get; set; } = string.Empty;
            public string[]? Names { get; set; } = null;
            public object[]? Values { get; set; } = null;
        }

        public sealed class TCUser
        {
            public TCUserInfo UserInfo { get; set; } = new();
            public TCTrainerInfo TrainerInfo { get; set; } = new();
            public TCDaycare Daycare { get; set; } = new();
            public TCBuddy Buddy { get; set; } = new();
            public TCDex Dex { get; set; } = new();
            public TCPerks Perks { get; set; } = new();
            public List<TCItem> Items { get; set; } = new();
            public Dictionary<int, TCCatch> Catches { get; set; } = new();
        }

        protected class TCRng
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
            public int LegendaryRNG { get; set; }
        }

        public class TCUserInfo
        {
            public ulong UserID { get; set; }
            public string Username { get; set; } = "";
            public int CatchCount { get; set; }
            public int TimeZoneOffset { get; set; }
            public DateTime LastPlayed { get; set; } = DateTime.Now;
            public bool ReceiveEventPing { get; set; }
        }

        public class TCTrainerInfo
        {
            public string OTName { get; set; } = "Carp";
            public string OTGender { get; set; } = "Male";
            public int TID { get; set; } = 12345;
            public int SID { get; set; } = 54321;
            public string Language { get; set; } = "English";
        }

        public class TCCatch
        {
            public bool Shiny { get; set; }
            public int ID { get; set; }
            public string Ball { get; set; } = "None";
            public string Nickname { get; set; } = "";
            public string Species { get; set; } = "None";
            public string Form { get; set; } = "";
            public bool Egg { get; set; }
            public bool Favorite { get; set; }
            public bool Traded { get; set; }
            public bool Legendary { get; set; }
            public bool Event { get; set; }
            public bool Gmax { get; set; }
        }

        public class TCDaycare
        {
            public bool Shiny1 { get; set; }
            public int ID1 { get; set; }
            public int Species1 { get; set; }
            public string Form1 { get; set; } = "";
            public int Ball1 { get; set; }
            public bool Shiny2 { get; set; }
            public int ID2 { get; set; }
            public int Species2 { get; set; }
            public string Form2 { get; set; } = "";
            public int Ball2 { get; set; }
        }

        public class TCBuddy
        {
            public int ID { get; set; }
            public Ability Ability { get; set; }
            public string Nickname { get; set; } = "";
        }

        public class TCItem
        {
            public TCItems Item { get; set; } = new();
            public int ItemCount { get; set; }
        }

        public class TCPerks
        {
            public List<DexPerks> ActivePerks { get; set; } = new();
            public int SpeciesBoost { get; set; }
        }

        public class TCDex
        {
            public int DexCompletionCount { get; set; }
            public List<int> Entries { get; set; } = new();
        }

        // Deprecated JSON class structure, needed for migration to SQLite.
        protected class TCUserInfoRoot
        {
            public List<TCUserInfo> Users { get; set; } = new();

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

        // Taken from ALM.
        protected static readonly Dictionary<Species, PersonalColor> ShinyMap = new()
        {
            { Bulbasaur, Green },
            { Ivysaur, Green },
            { Venusaur, Green },
            { Charmander, Yellow },
            { Charmeleon, Yellow },
            { Charizard, Black },
            { Squirtle, Blue },
            { Wartortle, Purple },
            { Blastoise, Purple },
            { Caterpie, Yellow },
            { Metapod, Red },
            { Butterfree, Purple },
            { Weedle, Yellow },
            { Kakuna, Green },
            { Beedrill, Green },
            { Pidgey, Brown },
            { Pidgeotto, Yellow },
            { Pidgeot, Yellow },
            { Rattata, Green },
            { Raticate, Red },
            { Spearow, Green },
            { Fearow, Green },
            { Ekans, Green },
            { Arbok, Yellow },
            { Pikachu, Yellow },
            { Raichu, Yellow },
            { Sandshrew, Green },
            { Sandslash, Red },
            { NidoranF, Purple },
            { Nidorina, Purple },
            { Nidoqueen, Green },
            { NidoranM, Blue },
            { Nidorino, Blue },
            { Nidoking, Blue },
            { Clefairy, Pink },
            { Clefable, Pink },
            { Vulpix, Yellow },
            { Ninetales, White },
            { Jigglypuff, Pink },
            { Wigglytuff, Pink },
            { Zubat, Green },
            { Golbat, Green },
            { Oddish, Green },
            { Gloom, Green },
            { Vileplume, Green },
            { Paras, Red },
            { Parasect, Red },
            { Venonat, Purple },
            { Venomoth, Blue },
            { Diglett, Brown },
            { Dugtrio, Brown },
            { Meowth, White },
            { Persian, White },
            { Psyduck, Blue },
            { Golduck, Blue },
            { Mankey, Green },
            { Primeape, White },
            { Growlithe, Yellow },
            { Arcanine, Yellow },
            { Poliwag, Blue },
            { Poliwhirl, Blue },
            { Poliwrath, Green },
            { Abra, Yellow },
            { Kadabra, Yellow },
            { Alakazam, Yellow },
            { Machop, Brown },
            { Machoke, Green },
            { Machamp, Green },
            { Bellsprout, Yellow },
            { Weepinbell, Yellow },
            { Victreebel, Yellow },
            { Tentacool, Blue },
            { Tentacruel, Blue },
            { Geodude, Yellow },
            { Graveler, Brown },
            { Golem, Brown },
            { Ponyta, Blue },
            { Rapidash, Black },
            { Slowpoke, Pink },
            { Slowbro, Purple },
            { Magnemite, Gray },
            { Magneton, Gray },
            { Farfetchd, Pink },
            { Doduo, Green },
            { Dodrio, Green },
            { Seel, White },
            { Dewgong, White },
            { Grimer, Green },
            { Muk, Green },
            { Shellder, Red },
            { Cloyster, Blue },
            { Gastly, Purple },
            { Haunter, Purple },
            { Gengar, Purple },
            { Onix, Green },
            { Drowzee, Pink },
            { Hypno, Pink },
            { Krabby, Yellow },
            { Kingler, Green },
            { Voltorb, Blue },
            { Electrode, Blue },
            { Exeggcute, Yellow },
            { Exeggutor, Red },
            { Cubone, Green },
            { Marowak, Green },
            { Hitmonlee, Green },
            { Hitmonchan, Green },
            { Lickitung, Yellow },
            { Koffing, Green },
            { Weezing, Green },
            { Rhyhorn, Red },
            { Rhydon, Gray },
            { Chansey, Green },
            { Tangela, Green },
            { Kangaskhan, Brown },
            { Horsea, Blue },
            { Seadra, Blue },
            { Goldeen, White },
            { Seaking, Red },
            { Staryu, Yellow },
            { Starmie, Blue },
            { MrMime, White },
            { Scyther, Green },
            { Jynx, Pink },
            { Electabuzz, Yellow },
            { Magmar, Red },
            { Pinsir, Purple },
            { Tauros, Green },
            { Magikarp, Yellow },
            { Gyarados, Red },
            { Lapras, Purple },
            { Ditto, Blue },
            { Eevee, White },
            { Vaporeon, Purple },
            { Jolteon, Green },
            { Flareon, Yellow },
            { Porygon, Pink },
            { Omanyte, Purple },
            { Omastar, Purple },
            { Kabuto, Green },
            { Kabutops, Green },
            { Aerodactyl, Purple },
            { Snorlax, Blue },
            { Articuno, Blue },
            { Zapdos, Yellow },
            { Moltres, Red },
            { Dratini, Pink },
            { Dragonair, Pink },
            { Dragonite, Green },
            { Mewtwo, White },
            { Mew, Blue },
            { Chikorita, Green },
            { Bayleef, Red },
            { Meganium, Green },
            { Cyndaquil, Brown },
            { Quilava, Brown },
            { Typhlosion, Brown },
            { Totodile, Blue },
            { Croconaw, Blue },
            { Feraligatr, Blue },
            { Sentret, Yellow },
            { Furret, Pink },
            { Hoothoot, Yellow },
            { Noctowl, Brown },
            { Ledyba, Red },
            { Ledian, Red },
            { Spinarak, Blue },
            { Ariados, Pink },
            { Crobat, Pink },
            { Chinchou, Blue },
            { Lanturn, Purple },
            { Pichu, Yellow },
            { Cleffa, Pink },
            { Igglybuff, Pink },
            { Togepi, White },
            { Togetic, White },
            { Natu, Green },
            { Xatu, Green },
            { Mareep, Pink },
            { Flaaffy, Pink },
            { Ampharos, Pink },
            { Bellossom, Purple },
            { Marill, Green },
            { Azumarill, Yellow },
            { Sudowoodo, Brown },
            { Politoed, Blue },
            { Hoppip, Green },
            { Skiploom, Pink },
            { Jumpluff, Red },
            { Aipom, Red },
            { Sunkern, Yellow },
            { Sunflora, Yellow },
            { Yanma, Blue },
            { Wooper, Pink },
            { Quagsire, Pink },
            { Espeon, Green },
            { Umbreon, Black },
            { Murkrow, Purple },
            { Slowking, Purple },
            { Misdreavus, Green },
            { Unown, Blue },
            { Wobbuffet, Pink },
            { Girafarig, Yellow },
            { Pineco, Yellow },
            { Forretress, Yellow },
            { Dunsparce, Yellow },
            { Gligar, Blue },
            { Steelix, Yellow },
            { Snubbull, Purple },
            { Granbull, Pink },
            { Qwilfish, Pink },
            { Scizor, Green },
            { Shuckle, Blue },
            { Heracross, Pink },
            { Sneasel, Pink },
            { Teddiursa, Green },
            { Ursaring, Green },
            { Slugma, Gray },
            { Magcargo, Pink },
            { Swinub, Green },
            { Piloswine, Yellow },
            { Corsola, Blue },
            { Remoraid, Purple },
            { Octillery, Brown },
            { Delibird, Purple },
            { Mantine, Blue },
            { Skarmory, Brown },
            { Houndour, Blue },
            { Houndoom, Blue },
            { Kingdra, Purple },
            { Phanpy, Blue },
            { Donphan, Red },
            { Porygon2, Blue },
            { Stantler, Green },
            { Smeargle, Yellow },
            { Tyrogue, Brown },
            { Hitmontop, Brown },
            { Smoochum, Pink },
            { Elekid, Yellow },
            { Magby, Red },
            { Miltank, Blue },
            { Blissey, Pink },
            { Raikou, Yellow },
            { Entei, Brown },
            { Suicune, Blue },
            { Larvitar, Green },
            { Pupitar, Purple },
            { Tyranitar, Brown },
            { Lugia, White },
            { HoOh, Yellow },
            { Celebi, Pink },
            { Treecko, Green },
            { Grovyle, Green },
            { Sceptile, Green },
            { Torchic, Yellow },
            { Combusken, Yellow },
            { Blaziken, Red },
            { Mudkip, Purple },
            { Marshtomp, Purple },
            { Swampert, Purple },
            { Poochyena, Yellow },
            { Mightyena, Yellow },
            { Zigzagoon, Red },
            { Linoone, Red },
            { Wurmple, Purple },
            { Silcoon, Yellow },
            { Beautifly, Yellow },
            { Cascoon, Green },
            { Dustox, Brown },
            { Lotad, Red },
            { Lombre, Green },
            { Ludicolo, Yellow },
            { Seedot, Red },
            { Nuzleaf, Red },
            { Shiftry, Red },
            { Taillow, Red },
            { Swellow, Red },
            { Wingull, White },
            { Pelipper, Green },
            { Ralts, Blue },
            { Kirlia, White },
            { Gardevoir, White },
            { Surskit, Black },
            { Masquerain, Green },
            { Shroomish, Red },
            { Breloom, Red },
            { Slakoth, Pink },
            { Vigoroth, White },
            { Slaking, Brown },
            { Nincada, Yellow },
            { Ninjask, Yellow },
            { Shedinja, Yellow },
            { Whismur, Purple },
            { Loudred, Purple },
            { Exploud, Purple },
            { Makuhita, Yellow },
            { Hariyama, Purple },
            { Azurill, Green },
            { Nosepass, Yellow },
            { Skitty, Red },
            { Delcatty, Red },
            { Sableye, Yellow },
            { Mawile, Black },
            { Aron, White },
            { Lairon, Gray },
            { Aggron, Gray },
            { Meditite, Red },
            { Medicham, Blue },
            { Electrike, Blue },
            { Manectric, Black },
            { Plusle, Red },
            { Minun, Green },
            { Volbeat, Purple },
            { Illumise, Blue },
            { Roselia, Green },
            { Gulpin, Blue },
            { Swalot, Blue },
            { Carvanha, Green },
            { Sharpedo, Purple },
            { Wailmer, Purple },
            { Wailord, Purple },
            { Numel, Yellow },
            { Camerupt, Black },
            { Torkoal, Red },
            { Spoink, Yellow },
            { Grumpig, Black },
            { Spinda, Green },
            { Trapinch, Green },
            { Vibrava, Red },
            { Flygon, Green },
            { Cacnea, Red },
            { Cacturne, Red },
            { Swablu, Yellow },
            { Altaria, Yellow },
            { Zangoose, White },
            { Seviper, Black },
            { Lunatone, Yellow },
            { Solrock, Red },
            { Barboach, Blue },
            { Whiscash, Blue },
            { Corphish, Red },
            { Crawdaunt, Red },
            { Baltoy, Yellow },
            { Claydol, Black },
            { Lileep, Green },
            { Cradily, Red },
            { Anorith, Brown },
            { Armaldo, Red },
            { Feebas, Purple },
            { Milotic, Blue },
            { Castform, Purple },
            { Kecleon, Green },
            { Shuppet, Blue },
            { Banette, Black },
            { Duskull, Red },
            { Dusclops, Red },
            { Tropius, Green },
            { Chimecho, Blue },
            { Absol, Red },
            { Wynaut, Purple },
            { Snorunt, Blue },
            { Glalie, White },
            { Spheal, Purple },
            { Sealeo, Purple },
            { Walrein, Purple },
            { Clamperl, Purple },
            { Huntail, Green },
            { Gorebyss, Yellow },
            { Relicanth, Blue },
            { Luvdisc, Yellow },
            { Bagon, Green },
            { Shelgon, Green },
            { Salamence, Green },
            { Beldum, Gray },
            { Metang, Gray },
            { Metagross, Gray },
            { Regirock, Red },
            { Regice, Blue },
            { Registeel, Black },
            { Latias, Yellow },
            { Latios, Green },
            { Kyogre, Purple },
            { Groudon, Green },
            { Rayquaza, Black },
            { Jirachi, Yellow },
            { Deoxys, Yellow },
            { Turtwig, Blue },
            { Grotle, Blue },
            { Torterra, Green },
            { Chimchar, Red },
            { Monferno, Red },
            { Infernape, Red },
            { Piplup, Blue },
            { Prinplup, Blue },
            { Empoleon, Blue },
            { Starly, Brown },
            { Staravia, Brown },
            { Staraptor, Brown },
            { Bidoof, Yellow },
            { Bibarel, Yellow },
            { Kricketot, Yellow },
            { Kricketune, Yellow },
            { Shinx, Yellow },
            { Luxio, Yellow },
            { Luxray, Black },
            { Budew, Green },
            { Roserade, Green },
            { Cranidos, Red },
            { Rampardos, Red },
            { Shieldon, Black },
            { Bastiodon, Black },
            { Burmy, Green },
            { Wormadam, Green },
            { Mothim, Yellow },
            { Combee, Red },
            { Vespiquen, Red },
            { Pachirisu, Pink },
            { Buizel, Yellow },
            { Floatzel, Yellow },
            { Cherubi, Red },
            { Cherrim, Green },
            { Shellos, Blue },
            { Gastrodon, Blue },
            { Ambipom, Pink },
            { Drifloon, Yellow },
            { Drifblim, Yellow },
            { Buneary, Red },
            { Lopunny, Red },
            { Mismagius, Green },
            { Honchkrow, Purple },
            { Glameow, Purple },
            { Purugly, Purple },
            { Chingling, Yellow },
            { Stunky, Red },
            { Skuntank, Red },
            { Bronzor, Green },
            { Bronzong, Green },
            { Bonsly, Brown },
            { MimeJr, Pink },
            { Happiny, Pink },
            { Chatot, Black },
            { Spiritomb, Blue },
            { Gible, Blue },
            { Gabite, Blue },
            { Garchomp, Black },
            { Munchlax, Blue },
            { Riolu, Yellow },
            { Lucario, Yellow },
            { Hippopotas, Yellow },
            { Hippowdon, Yellow },
            { Skorupi, Red },
            { Drapion, Red },
            { Croagunk, Blue },
            { Toxicroak, Blue },
            { Carnivine, Green },
            { Finneon, Black },
            { Lumineon, Black },
            { Mantyke, Blue },
            { Snover, White },
            { Abomasnow, White },
            { Weavile, Pink },
            { Magnezone, Gray },
            { Lickilicky, Yellow },
            { Rhyperior, Yellow },
            { Tangrowth, Green },
            { Electivire, Yellow },
            { Magmortar, Red },
            { Togekiss, White },
            { Yanmega, Blue },
            { Leafeon, Yellow },
            { Glaceon, Blue },
            { Gliscor, Blue },
            { Mamoswine, Brown },
            { PorygonZ, Blue },
            { Gallade, Blue },
            { Probopass, Yellow },
            { Dusknoir, Black },
            { Froslass, White },
            { Rotom, Red },
            { Uxie, Yellow },
            { Mesprit, Red },
            { Azelf, Blue },
            { Dialga, Green },
            { Palkia, Pink },
            { Heatran, Red },
            { Regigigas, Blue },
            { Giratina, Blue },
            { Cresselia, Purple },
            { Phione, Blue },
            { Manaphy, Blue },
            { Darkrai, Black },
            { Shaymin, Green },
            { Arceus, Yellow },
            { Victini, White },
            { Snivy, Green },
            { Servine, Green },
            { Serperior, Green },
            { Tepig, Yellow },
            { Pignite, Red },
            { Emboar, Blue },
            { Oshawott, Blue },
            { Dewott, Blue },
            { Samurott, Blue },
            { Patrat, Brown },
            { Watchog, Red },
            { Lillipup, Yellow },
            { Herdier, Yellow },
            { Stoutland, Yellow },
            { Purrloin, Blue },
            { Liepard, Red },
            { Pansage, Green },
            { Simisage, Green },
            { Pansear, Red },
            { Simisear, Red },
            { Panpour, Blue },
            { Simipour, Blue },
            { Munna, Yellow },
            { Musharna, Purple },
            { Pidove, Gray },
            { Tranquill, Green },
            { Unfezant, Brown },
            { Blitzle, Blue },
            { Zebstrika, Black },
            { Roggenrola, Purple },
            { Boldore, Purple },
            { Gigalith, Blue },
            { Woobat, Green },
            { Swoobat, Yellow },
            { Drilbur, Red },
            { Excadrill, Red },
            { Audino, Purple },
            { Timburr, Yellow },
            { Gurdurr, Yellow },
            { Conkeldurr, Red },
            { Tympole, Yellow },
            { Palpitoad, Blue },
            { Seismitoad, Blue },
            { Throh, Red },
            { Sawk, Blue },
            { Sewaddle, Green },
            { Swadloon, Green },
            { Leavanny, Green },
            { Venipede, Red },
            { Whirlipede, Purple },
            { Scolipede, Red },
            { Cottonee, Yellow },
            { Whimsicott, White },
            { Petilil, Yellow },
            { Lilligant, Yellow },
            { Basculin, Green },
            { Sandile, Yellow },
            { Krokorok, Brown },
            { Krookodile, Brown },
            { Darumaka, Red },
            { Darmanitan, Red },
            { Maractus, Green },
            { Dwebble, Red },
            { Crustle, Green },
            { Scraggy, Yellow },
            { Scrafty, Green },
            { Sigilyph, Black },
            { Yamask, Blue },
            { Cofagrigus, Gray },
            { Tirtouga, Blue },
            { Carracosta, Blue },
            { Archen, Red },
            { Archeops, Yellow },
            { Trubbish, Blue },
            { Garbodor, Brown },
            { Zorua, Black },
            { Zoroark, Black },
            { Minccino, Red },
            { Cinccino, Brown },
            { Gothita, Red },
            { Gothorita, Black },
            { Gothitelle, Black },
            { Solosis, Green },
            { Duosion, Green },
            { Reuniclus, Blue },
            { Ducklett, Pink },
            { Swanna, White },
            { Vanillite, White },
            { Vanillish, White },
            { Vanilluxe, White },
            { Deerling, Pink },
            { Sawsbuck, Yellow },
            { Emolga, White },
            { Karrablast, Green },
            { Escavalier, Gray },
            { Foongus, Blue },
            { Amoonguss, Blue },
            { Frillish, Blue },
            { Jellicent, Blue },
            { Alomomola, Purple },
            { Joltik, Yellow },
            { Galvantula, Yellow },
            { Ferroseed, Gray },
            { Ferrothorn, Yellow },
            { Klink, Gray },
            { Klang, Gray },
            { Klinklang, Yellow },
            { Tynamo, White },
            { Eelektrik, Yellow },
            { Eelektross, Green },
            { Elgyem, Blue },
            { Beheeyem, Red },
            { Litwick, White },
            { Lampent, Black },
            { Chandelure, Red },
            { Axew, Brown },
            { Fraxure, Black },
            { Haxorus, Black },
            { Cubchoo, White },
            { Beartic, White },
            { Cryogonal, Blue },
            { Shelmet, Yellow },
            { Accelgor, Yellow },
            { Stunfisk, Yellow },
            { Mienfoo, Blue },
            { Mienshao, Pink },
            { Druddigon, Green },
            { Golett, Gray },
            { Golurk, Gray },
            { Pawniard, Blue },
            { Bisharp, Blue },
            { Bouffalant, Red },
            { Rufflet, Brown },
            { Braviary, Blue },
            { Vullaby, Red },
            { Mandibuzz, Red },
            { Heatmor, Red },
            { Durant, Gray },
            { Deino, Green },
            { Zweilous, Green },
            { Hydreigon, Green },
            { Larvesta, Yellow },
            { Volcarona, Yellow },
            { Cobalion, Blue },
            { Terrakion, Red },
            { Virizion, Red },
            { Tornadus, Green },
            { Thundurus, Blue },
            { Reshiram, White },
            { Zekrom, Black },
            { Landorus, Yellow },
            { Kyurem, Black },
            { Keldeo, Green },
            { Meloetta, Green },
            { Genesect, Red },
            { Chespin, Red },
            { Quilladin, Red },
            { Chesnaught, Green },
            { Fennekin, Gray },
            { Braixen, Purple },
            { Delphox, Purple },
            { Froakie, Blue },
            { Frogadier, Blue },
            { Greninja, Black },
            { Bunnelby, Gray },
            { Diggersby, Gray },
            { Fletchling, Red },
            { Fletchinder, Red },
            { Talonflame, Red },
            { Scatterbug, White },
            { Spewpa, Gray },
            { Vivillon, Red },
            { Litleo, Red },
            { Pyroar, Red },
            { Flabébé, White },
            { Floette, White },
            { Florges, Purple },
            { Skiddo, Green },
            { Gogoat, Green },
            { Pancham, Black },
            { Pangoro, Black },
            { Furfrou, Black },
            { Espurr, Pink },
            { Meowstic, Yellow },
            { Honedge, Red },
            { Doublade, Red },
            { Aegislash, Red },
            { Spritzee, Purple },
            { Aromatisse, Purple },
            { Swirlix, Yellow },
            { Slurpuff, Yellow },
            { Inkay, Brown },
            { Malamar, Brown },
            { Binacle, Blue },
            { Barbaracle, Blue },
            { Skrelp, Purple },
            { Dragalge, Purple },
            { Clauncher, Red },
            { Clawitzer, Red },
            { Helioptile, Red },
            { Heliolisk, Red },
            { Tyrunt, Blue },
            { Tyrantrum, Blue },
            { Amaura, White },
            { Aurorus, White },
            { Sylveon, Blue },
            { Hawlucha, Black },
            { Dedenne, Brown },
            { Carbink, Black },
            { Goomy, Yellow },
            { Sliggoo, Yellow },
            { Goodra, Yellow },
            { Klefki, Yellow },
            { Phantump, Gray },
            { Trevenant, Gray },
            { Pumpkaboo, Purple },
            { Gourgeist, Purple },
            { Bergmite, Blue },
            { Avalugg, Blue },
            { Noibat, Blue },
            { Noivern, Blue },
            { Xerneas, Blue },
            { Yveltal, Red },
            { Zygarde, White },
            { Diancie, Pink },
            { Hoopa, Yellow },
            { Volcanion, Yellow },
            { Rowlet, Green },
            { Dartrix, Green },
            { Decidueye, Black },
            { Litten, White },
            { Torracat, White },
            { Incineroar, White },
            { Popplio, Blue },
            { Brionne, Blue },
            { Primarina, Blue },
            { Pikipek, Black },
            { Trumbeak, Black },
            { Toucannon, Black },
            { Yungoos, Brown },
            { Gumshoos, Brown },
            { Grubbin, Red },
            { Charjabug, Red },
            { Vikavolt, Gray },
            { Crabrawler, Purple },
            { Crabominable, White },
            { Oricorio, Black },
            { Cutiefly, Pink },
            { Ribombee, Pink },
            { Rockruff, Blue },
            { Lycanroc, Blue },
            { Wishiwashi, Blue },
            { Mareanie, Red },
            { Toxapex, Red },
            { Mudbray, Yellow },
            { Mudsdale, Yellow },
            { Dewpider, Purple },
            { Araquanid, Purple },
            { Fomantis, Green },
            { Lurantis, Green },
            { Morelull, Brown },
            { Shiinotic, Brown },
            { Salandit, White },
            { Salazzle, White },
            { Stufful, Yellow },
            { Bewear, Yellow },
            { Bounsweet, Red },
            { Steenee, Purple },
            { Tsareena, Purple },
            { Comfey, Blue },
            { Oranguru, Pink },
            { Passimian, Blue },
            { Wimpod, Red },
            { Golisopod, White },
            { Sandygast, Black },
            { Palossand, Black },
            { Pyukumuku, Green },
            { TypeNull, Brown },
            { Silvally, Yellow },
            { Minior, Black },
            { Komala, Blue },
            { Turtonator, Blue },
            { Togedemaru, White },
            { Mimikyu, Gray },
            { Bruxish, Red },
            { Drampa, Yellow },
            { Dhelmise, Red },
            { Jangmoo, Yellow },
            { Hakamoo, Green },
            { Kommoo, Green },
            { TapuKoko, Black },
            { TapuLele, Black },
            { TapuBulu, Black },
            { TapuFini, Black },
            { Cosmog, Purple },
            { Cosmoem, Yellow },
            { Solgaleo, Red },
            { Lunala, Red },
            { Nihilego, Yellow },
            { Buzzwole, Green },
            { Pheromosa, Black },
            { Xurkitree, Blue },
            { Celesteela, White },
            { Kartana, White },
            { Guzzlord, White },
            { Necrozma, Blue },
            { Magearna, Gray },
            { Marshadow, Black },
            { Poipole, White },
            { Naganadel, Yellow },
            { Stakataka, Yellow },
            { Blacephalon, Blue },
            { Zeraora, White },
            { Meltan, Gray },
            { Melmetal, Gray },
            { Grookey, Green },
            { Thwackey, Yellow },
            { Rillaboom, Brown },
            { Scorbunny, White },
            { Raboot, Gray },
            { Cinderace, Gray },
            { Sobble, Blue },
            { Drizzile, Blue },
            { Inteleon, Blue },
            { Skwovet, Red },
            { Greedent, Red },
            { Rookidee, Yellow },
            { Corvisquire, Gray },
            { Corviknight, Gray },
            { Blipbug, Blue },
            { Dottler, Blue },
            { Orbeetle, Blue },
            { Nickit, Brown },
            { Thievul, Brown },
            { Gossifleur, Blue },
            { Eldegoss, White },
            { Wooloo, Black },
            { Dubwool, Black },
            { Chewtle, Green },
            { Drednaw, Green },
            { Yamper, Pink },
            { Boltund, Yellow },
            { Rolycoly, Black },
            { Carkol, Black },
            { Coalossal, Black },
            { Applin, Green },
            { Flapple, Green },
            { Appletun, Green },
            { Silicobra, Yellow },
            { Sandaconda, Black },
            { Cramorant, Red },
            { Arrokuda, Blue },
            { Barraskewda, Blue },
            { Toxel, Red },
            { Toxtricity, Purple },
            { Sizzlipede, Red },
            { Centiskorch, Red },
            { Clobbopus, Blue },
            { Grapploct, Red },
            { Sinistea, Purple },
            { Polteageist, Purple },
            { Hatenna, White },
            { Hattrem, White },
            { Hatterene, White },
            { Impidimp, Blue },
            { Morgrem, Blue },
            { Grimmsnarl, White },
            { Obstagoon, Red },
            { Perrserker, Yellow },
            { Cursola, Pink },
            { Sirfetchd, Yellow },
            { MrRime, Black },
            { Runerigus, White },
            { Milcery, White },
            { Alcremie, White },
            { Falinks, Brown },
            { Pincurchin, Black },
            { Snom, White },
            { Frosmoth, White },
            { Stonjourner, Black },
            { Eiscue, Purple },
            { Indeedee, Black },
            { Morpeko, White },
            { Cufant, Yellow },
            { Copperajah, Black },
            { Dracozolt, Brown },
            { Arctozolt, White },
            { Dracovish, Brown },
            { Arctovish, White },
            { Duraludon, White },
            { Dreepy, Green },
            { Drakloak, Gray },
            { Dragapult, Green },
            { Zacian, Blue },
            { Zamazenta, Red },
            { Eternatus, Red },
            { Kubfu, White },
            { Urshifu, Black },
            { Zarude, Black },
            { Regieleki, Yellow },
            { Regidrago, Red },
            { Glastrier, White },
            { Spectrier, Black },
            { Calyrex, White },
        };
    }
}
