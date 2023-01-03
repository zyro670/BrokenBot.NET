using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class EggSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Counts = nameof(Counts);
        public override string ToString() => "Egg Bot Settings";

        [Category(FeatureToggle), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.PauseWaitAcknowledge;

        [Category(FeatureToggle), Description("When enabled, the bot will make a sandwich on start.")]
        public bool EatFirst { get; set; } = true;

        [Category(FeatureToggle), Description("When enabled, the bot will click DUP on Item 1.")]
        public bool Item1DUP { get; set; } = false;

        [Category(FeatureToggle), Description("When enabled, the bot will click DUP on Item 2.")]
        public bool Item2DUP { get; set; } = true;

        [Category(FeatureToggle), Description("When enabled, the bot will click DUP on Item 3.")]
        public bool Item3DUP { get; set; } = true;

        [Category(FeatureToggle), Description("Amount of time to hold L stick up to ingredients for sandwich. [Default: 700ms]")]
        public int HoldUpToIngredients { get; set; } = 700;

        [Category(FeatureToggle), Description("When enabled, the bot will look for 3 Segment Dunsparce.")]
        public bool OneInOneHundredOnly { get; set; } = true;

        [Category(FeatureToggle), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; } = false;

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
