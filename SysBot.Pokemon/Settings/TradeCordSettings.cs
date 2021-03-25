using System.ComponentModel;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public class TradeCordSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "TradeCord Settings";

        [Category(FeatureToggle), Description("Enter Channel ID(s) where TradeCord should be active, or leave blank.")]
        public string TradeCordChannels { get; set; } = string.Empty;

        [Category(FeatureToggle), Description("Enter the amount of time in seconds until a user can catch again.")]
        public double TradeCordCooldown { get; set; } = 60;

        [Category(FeatureToggle), Description("Enter the likelihood of a successful catch. Default: 90.")]
        public int CatchRate { get; set; } = 90;

        [Category(FeatureToggle), Description("Enter the likelihood of \"hatching\" an egg. Default: 75.")]
        public int EggRate { get; set; } = 75;

        [Category(FeatureToggle), Description("Enter the likelihood of a Cherish Ball event, if a compatible Pokémon was rolled. Default: 15.")]
        public int CherishRate { get; set; } = 15;

        [Category(FeatureToggle), Description("Enter the likelihood of Gigantamax forms, if a compatible Pokémon was rolled. Default: 40.")]
        public int GmaxRate { get; set; } = 40;

        [Category(FeatureToggle), Description("Enter the likelihood of a star shiny catch or \"hatch\". Default: 5.")]
        public int StarShinyRate { get; set; } = 5;

        [Category(FeatureToggle), Description("Enter the likelihood of a square shiny catch or \"hatch\". Default: 2")]
        public int SquareShinyRate { get; set; } = 2;

        [Category(FeatureToggle), Description("Set to \"true\" if you want to do a TradeCord event.")]
        public bool EnableEvent { get; set; } = false;

        [Category(FeatureToggle), Description("Enter the end date for an event in your local date format. For example, YYYY/MM/DD. Default: DISABLED")]
        public string EventEnd { get; set; } = "DISABLED";

        [Category(FeatureToggle), Description("If \"EnableEvent\" set to \"true\", select which type of event to do.")]
        public PokeEventType PokeEventType { get; set; } = PokeEventType.Normal;
    }

    public enum PokeEventType
    {
        Normal,
        Fighting,
        Flying,
        Poison,
        Ground,
        Rock,
        Bug,
        Ghost,
        Steel,
        Fire,
        Water,
        Grass,
        Electric,
        Psychic,
        Ice,
        Dragon,
        Dark,
        Fairy,
        Legends,
        EventPoke,
    }
}
