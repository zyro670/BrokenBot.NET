using PKHeX.Core;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon
{
    public class EncounterSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Counts = nameof(Counts);

        public override string ToString() => "Encounter Bot SV Settings";

        [Category(FeatureToggle), Description("When enabled, the 100% catch cheat will be enabled.")]
        public bool EnableCatchCheat { get; set; }

        [Category(FeatureToggle), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        [Category(FeatureToggle), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        private int _completedWild;
        private int _completedLegend;
        private int _completedGimmighoul;

        [Category(Counts), Description("Encountered Wild Pokémon")]
        public int CompletedEncounters
        {
            get => _completedWild;
            set => _completedWild = value;
        }

        [Category(Counts), Description("Encountered Legendary Pokémon")]
        public int CompletedLegends
        {
            get => _completedLegend;
            set => _completedLegend = value;
        }

        [Category(Counts), Description("Encountered Gimmighoul Pokémon")]
        public int CompletedGimmighouls
        {
            get => _completedGimmighoul;
            set => _completedGimmighoul = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedEncounters() => Interlocked.Increment(ref _completedWild);
        public int AddCompletedLegends() => Interlocked.Increment(ref _completedLegend);
        public int AddCompletedGimmighouls() => Interlocked.Increment(ref _completedLegend);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedEncounters != 0)
                yield return $"Wild Encounters: {CompletedEncounters}";
            if (CompletedLegends != 0)
                yield return $"Legendary Encounters: {CompletedLegends}";
            if (CompletedGimmighouls != 0)
                yield return $"Gimmighoul Encounters: {CompletedGimmighouls}";
        }
    }
}
