using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
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
        public int ResetGameAfterThisManySandwiches { get; set; } = 0;

        [Category(EggFetch), Description("When enabled, the bot will force dump any egg encounters that are a match. These should not be treated as legitimate eggs.")]
        public bool ForceDump { get; set; } = false;

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
    }
}
