using System;
using System.Collections.Generic;
using static PKHeX.Core.AutoMod.Aesthetics;

namespace SysBot.Pokemon.Discord.Helpers
{
    internal class EggColors
    {
        // Define a dictionary to map PersonalColor enums to their corresponding image URLs
        public static Dictionary<PersonalColor, string> ColorToImageUrl = new Dictionary<PersonalColor, string>
        {
            { PersonalColor.Red, "https://i.imgur.com/bYKR3QR.png" },
            { PersonalColor.Blue, "https://i.imgur.com/L74uJf6.png" },
            { PersonalColor.Yellow, "https://i.imgur.com/As5tZfq.png" },
            { PersonalColor.Green, "https://i.imgur.com/f61zCSc.png" },
            { PersonalColor.Black, "https://i.imgur.com/FxAtz42.png" },
            { PersonalColor.Brown, "https://i.imgur.com/2uZABue.png" },
            { PersonalColor.Purple, "https://i.imgur.com/MFNvS13.png" },
            { PersonalColor.Gray, "https://i.imgur.com/ZTgB7wQ.png" },
            { PersonalColor.White, "https://i.imgur.com/TBrao98.png" },
            { PersonalColor.Pink, "https://i.imgur.com/HvO8MMQ.png" }
        };

        // Define the EggColor enum
        public enum EggColor : byte
        {
            Red,
            Blue,
            Yellow,
            Green,
            Black,
            Brown,
            Purple,
            Gray,
            White,
            Pink
        }

        // Define the mapping from PersonalColor to EggColor
        public static Dictionary<PersonalColor, EggColor> ColorToEggColor = new Dictionary<PersonalColor, EggColor>
{
    { PersonalColor.Red, EggColor.Red },
    { PersonalColor.Blue, EggColor.Blue },
    { PersonalColor.Yellow, EggColor.Yellow },
    { PersonalColor.Green, EggColor.Green },
    { PersonalColor.Black, EggColor.Black },
    { PersonalColor.Brown, EggColor.Brown },
    { PersonalColor.Purple, EggColor.Purple },
    { PersonalColor.Gray, EggColor.Gray },
    { PersonalColor.White, EggColor.White },
    { PersonalColor.Pink, EggColor.Pink }
};
    }
}



