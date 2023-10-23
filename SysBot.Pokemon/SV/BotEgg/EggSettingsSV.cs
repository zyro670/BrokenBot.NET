using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class EggSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string EggFetch = nameof(EggFetch);
        private const string Counts = nameof(Counts);
        public override string ToString() => "EggBotSV Settings";

        [Category(EggFetch), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(EggFetch), Description("EggFetch Paremeters"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<EggFetchStopConditionsCatgeory> StopConditions { get; set; } = new();

        [Category(EggFetch), Description("When enabled, the bot will make a sandwich on start.")]
        public bool EatFirst { get; set; } = true;

        [Category(EggFetch), Description("When enabled, the bot will click DUP on Item 1.")]
        public bool Item1DUP { get; set; } = false;

        [Category(EggFetch), Description("When enabled, the bot will click DUP on Item 2.")]
        public bool Item2DUP { get; set; } = true;

        [Category(EggFetch), Description("When enabled, the bot will click DUP on Item 3.")]
        public bool Item3DUP { get; set; } = true;

        [Category(EggFetch), Description("Amount of time to hold L stick up to ingredients for sandwich. [Default: 700ms]")]
        public int HoldUpToIngredients { get; set; } = 700;

        [Category(EggFetch), Description("When enabled, the bot will only stop when encounter has a Scale of XXXS or XXXL.")]
        public bool MinMaxScaleOnly { get; set; } = false;

        [Category(EggFetch), Description("When enabled, the bot will look for 3 Segment Dunsparce or Family of Three Maus.")]
        public bool OneInOneHundredOnly { get; set; } = true;

        [Category(EggFetch), Description("Resets game after making this amount of sandwiches.")]
        public int ResetGameAfterThisManySandwiches { get; set; } = 2;

        [Category(EggFetch), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        private int _completedEggs;

        [Category(Counts), Description("Eggs Retrieved")]
        public int CompletedEggs
        {
            get => _completedEggs;
            set => _completedEggs = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedEggs() => Interlocked.Increment(ref _completedEggs);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedEggs != 0)
                yield return $"Eggs Received: {CompletedEggs}";
        }

        [Category(EggFetch), TypeConverter(typeof(CategoryConverter<EggFetchStopConditionsCatgeory>))]
        public class EggFetchStopConditionsCatgeory
        {
            public override string ToString() => $"{Description}";
            public string Description { get; set; } = "";
            public GenderType TargetGender { get; set; } = GenderType.Any;
            public Nature TargetNature { get; set; } = Nature.Random;
            public string TargetIVs { get; set; } = "";
            public TargetShinyType ShinyTarget { get; set; } = TargetShinyType.DisableOption;
            public bool MatchShinyAndIV { get; set; } = true;
            public bool TargetScaleIsJumboOrMini { get; set; } = false;
        }

        public class CategoryConverter<T> : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(T));

            public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
        }

        private static List<int[]> ReadTargetIVs(EggSettingsSV settings, bool min)
        {
            List<int[]> desiredIVS = new();
            for (int c = 0; c < settings.StopConditions.Count; c++)
            {

                int[] targetIVs = new int[6];
                char[] split = { '/' };

                string[] splitIVs = settings.StopConditions[c].TargetIVs.Split(split, StringSplitOptions.RemoveEmptyEntries);

                // Only accept up to 6 values.  Fill it in with default values if they don't provide 6.
                // Anything that isn't an integer will be a wild card.
                for (int i = 0; i < 6; i++)
                {
                    if (i < splitIVs.Length)
                    {
                        var str = splitIVs[i];
                        if (int.TryParse(str, out var val))
                        {
                            targetIVs[i] = val;
                            continue;
                        }
                    }
                    targetIVs[i] = min ? 0 : 31;
                }
                desiredIVS.Add(targetIVs);
            }
            return desiredIVS;
        }

        public static void InitializeTargetIVs(EggSettingsSV config, out List<int[]> desired)
        {
            desired = ReadTargetIVs(config, true);
        }

        public static bool[] EggFound(PK9 pk, List<int[]> targetIVs, EggSettingsSV settings)
        {
            bool[] isMet = new bool[settings.StopConditions.Count];
            for (int c = 0; c < settings.StopConditions.Count; c++)
            {
                var currIVs = targetIVs[c];
                if (settings.StopConditions[c].TargetGender != GenderType.Any && (int)settings.StopConditions[c].TargetGender != pk.Gender)
                {
                    isMet[c] = false;
                    continue;
                }

                if (settings.StopConditions[c].TargetNature != Nature.Random && settings.StopConditions[c].TargetNature != (Nature)pk.Nature)
                {
                    isMet[c] = false;
                    continue;
                }

                if (settings.StopConditions[c].ShinyTarget != TargetShinyType.DisableOption)
                {
                    bool shinymatch = settings.StopConditions[c].ShinyTarget switch
                    {
                        TargetShinyType.AnyShiny => pk.IsShiny,
                        TargetShinyType.NonShiny => !pk.IsShiny,
                        TargetShinyType.StarOnly => pk.IsShiny && pk.ShinyXor != 0,
                        TargetShinyType.SquareOnly => pk.ShinyXor == 0,
                        TargetShinyType.DisableOption => true,
                        _ => throw new ArgumentException(nameof(TargetShinyType)),
                    };

                    if (!settings.StopConditions[c].MatchShinyAndIV && shinymatch)
                    {
                        isMet[c] = true;
                        continue;
                    }
                    if (settings.StopConditions[c].MatchShinyAndIV && !shinymatch)
                    {
                        isMet[c] = false;
                        continue;
                    }
                }

                if (pk.Scale != 0 && pk.Scale != 255 && settings.StopConditions[c].TargetScaleIsJumboOrMini)
                {
                    isMet[c] = false;
                    continue;
                }

                // Reorder the speed to be last.
                int[] pkIVList = new int[6];
                pk.GetIVs(pkIVList);
                (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);

                if (!currIVs.SequenceEqual(pkIVList))
                {
                    isMet[c] = false;
                    continue;
                }

                isMet[c] = true;
            }
            return isMet;
        }
    }
}
