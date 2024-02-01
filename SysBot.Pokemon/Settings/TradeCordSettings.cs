using System.ComponentModel;

namespace SysBot.Pokemon;

public class TradeCordSettings
{
    private const string FeatureToggle = nameof(FeatureToggle);
    public override string ToString() => "TradeCord Settings";

    [Category(FeatureToggle), Description("If enabled, every time the program is started up, inactive users will be deleted (no commands used for at least a month).")]
    public bool ClearInactive { get; set; } = false;

    [Category(FeatureToggle), Description("Choose whether to use full-size Home images from ProjectPokémon or downsized ones.")]
    public bool UseFullSizeImages { get; set; } = true;

    [Category(FeatureToggle), Description("Choose whether to use larger Poké Balls, or smaller ones.")]
    public bool UseLargerPokeBalls { get; set; } = true;

    [Category(FeatureToggle), Description("Enter the amount of time in seconds until a user can catch again.")]
    public int TradeCordCooldown { get; set; } = 60;

    [Category(FeatureToggle), Description("Enter the amount of time in minutes until users can invoke the vote command again.")]
    public int TradeCordEventCooldown { get; set; } = 60;

    [Category(FeatureToggle), Description("Enter the amount of time in minutes a user-invoked event will last.")]
    public int TradeCordEventDuration { get; set; } = 30;

    [Category(FeatureToggle), Description("Enter the likelihood of a successful catch. Default: 90")]
    public int CatchRate { get; set; } = 90;

    [Category(FeatureToggle), Description("Enter the likelihood of obtaining an egg. Default: 30")]
    public int EggRate { get; set; } = 30;

    [Category(FeatureToggle), Description("Enter the likelihood of finding an item. Default: 20")]
    public int ItemRate { get; set; } = 20;

    [Category(FeatureToggle), Description("Enter the likelihood of a Cherish Ball event, if a compatible Pokémon was rolled. Default: 15")]
    public int CherishRate { get; set; } = 15;

    [Category(FeatureToggle), Description("Enter the likelihood of Gigantamax forms, if a compatible Pokémon was rolled. Default: 40")]
    public int GmaxRate { get; set; } = 40;

    [Category(FeatureToggle), Description("Enter the likelihood of a star shiny catch or \"hatch\". Default: 5")]
    public int StarShinyRate { get; set; } = 5;

    [Category(FeatureToggle), Description("Enter the likelihood of a square shiny catch or \"hatch\". Default: 2")]
    public int SquareShinyRate { get; set; } = 2;

    [Category(FeatureToggle), Description("Enter the likelihood of Mythical and Legendary spawns. Default: 20")]
    public int LegendaryRate { get; set; } = 20;

    [Category(FeatureToggle), Description("Set to \"true\" if you want to do a TradeCord event.")]
    public bool EnableEvent { get; set; } = false;

    [Category(FeatureToggle), Description("Enter the end date for an event in your local date format. For example, YYYY/MM/DD. Default: DISABLED")]
    public string EventEnd { get; set; } = "DISABLED";

    [Category(FeatureToggle), Description("If \"EnableEvent\" set to \"true\", select which type of event to do.")]
    public PokeEventType PokeEventType { get; set; } = PokeEventType.Normal;
}

public enum PokeEventType
{
    Normal = 0,
    Fighting = 1,
    Flying = 2,
    Poison = 3,
    Ground = 4,
    Rock = 5,
    Bug = 6,
    Ghost = 7,
    Steel = 8,
    Fire = 9,
    Water = 10,
    Grass = 11,
    Electric = 12,
    Psychic = 13,
    Ice = 14,
    Dragon = 15,
    Dark = 16,
    Fairy = 17,
    Legends = 18,
    EventPoke = 19,
    RodentLite = 20,
    ClickbaitArticle = 21,
    Babies = 22,
    Halloween = 23,
    CottonCandy = 24,
    PokePets = 25,
    HouseParty = 26,
    LegendOfKoi = 27,
    Paradox = 28,
    FroggyFrenzy = 29,
    Anonymyths = 30,
}
