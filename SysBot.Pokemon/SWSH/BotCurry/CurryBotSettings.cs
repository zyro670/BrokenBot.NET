using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class CurryBotSettings : IBotStateSettings, ICountSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Curry = nameof(Curry);
        private const string Counts = nameof(Counts);
        public override string ToString() => "Curry Bot Settings";

        [Category(FeatureToggle), Description("Enable to restore ingredient and berry pouches once we run out. Default is \"False\".")]
        public bool RestorePouches { get; set; } = false;

        [Category(Curry), Description("Which berry to use for cooking curry. Default is \"Starf\".")]
        public CurryBerries Berry { get; set; } = CurryBerries.Starf;

        [Category(Curry), Description("Which ingredient to use for cooking curry. Default is \"Gigantamix\".")]
        public CurryIngredients Ingredient { get; set; } = CurryIngredients.Gigantamix;

        [Category(Curry), Description("Time to wait before pressing \"A\" to enter camp if starting in overworld (in milliseconds). Default is 8000.")]
        public int EnterCamp { get; set; } = 8000;

        [Category(Curry), Description("Time to wait until fanning starts after confirming ingredients (in milliseconds). Default is 8000.")]
        public int IngredientDrop { get; set; } = 8000;

        [Category(Curry), Description("Duration we should be fanning for (in milliseconds). Default is 21000.")]
        public int FanningDuration { get; set; } = 21000;

        [Category(Curry), Description("Duration we should be stirring for (in milliseconds). Default is 18000.")]
        public int StirringDuration { get; set; } = 18000;

        [Category(Curry), Description("Time to wait before pressing \"A\" to add a sprinkle of love. You want to have it land on the inner green ring (in milliseconds). Default is 3750.")]
        public int SprinkleOfLove { get; set; } = 3750;

        [Category(Curry), Description("Time to wait until we start eating curry after throwing in a sprinkle of love (in milliseconds). Default is 19000.")]
        public int CurryChowCutscene { get; set; } = 19000;

        [Category(FeatureToggle), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; } = false;

        private int _completedCurries;

        [Category(Counts), Description("Curries Cooked")]
        public int CompletedCurries
        {
            get => _completedCurries;
            set => _completedCurries = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedCurries() => Interlocked.Increment(ref _completedCurries);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedCurries != 0)
                yield return $"Adventures Completed: {CompletedCurries}";
        }
    }
}