using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public abstract class TradeCordUserBase
    {
        public class TCUserInfo
        {
            public ulong UserID { get; set; }
            public string Username { get; set; } = "";
            public int CatchCount { get; set; }
            public int TimeZoneOffset { get; set; }
            public DateTime LastPlayed { get; set; } = DateTime.Now;
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

        protected T GetRoot<T>(string file) where T : new()
        {
            using StreamReader sr = File.OpenText(file);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            T? root = (T?)serializer.Deserialize(reader, typeof(T));
            return root ?? new();
        }
    }
}