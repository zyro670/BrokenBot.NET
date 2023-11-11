using System.ComponentModel;

namespace SysBot.Pokemon
{
    public sealed class PokeTradeHubConfig : BaseConfig
    {
        private const string Bot_Trade = nameof(Bot_Trade);
        private const string Stream_Configuration = nameof(Stream_Configuration);
        private const string Pokémon_Scarlet_Violet_Settings = nameof(Pokémon_Scarlet_Violet_Settings);
        private const string Pokémon_Sword_Shield_Settings = nameof(Pokémon_Sword_Shield_Settings);
        private const string Pokémon_Legends_Arceus_Settings = nameof(Pokémon_Legends_Arceus_Settings);

        [Browsable(false)]
        public override bool Shuffled => Distribution.Shuffled;

        // Trade Bots

        [Category(Bot_Trade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TradeSettings Trade { get; set; } = new();

        [Category(Bot_Trade), Description("Settings for idle distribution trades.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DistributionSettings Distribution { get; set; } = new();

        [Category(Bot_Trade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TradeCordSettings TradeCord { get; set; } = new();

        [Category(Bot_Trade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DistributionSettings SurpriseTrade { get; set; } = new();

        [Category(Bot_Trade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public SeedCheckSettings SeedCheckSWSH { get; set; } = new();

        [Category(Bot_Trade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TradeAbuseSettings TradeAbuse { get; set; } = new();

        [Category(Bot_Trade)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public EtumrepDumpSettings EtumrepDump { get; set; } = new();

        // Operations

        [Category(Operation)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QueueSettings Queues { get; set; } = new();

        [Category(Operation), Description("Add extra time for slower Switches.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TimingSettings Timings { get; set; } = new();

        [Category(Operation), Description("Allows favored users to join the queue with a more favorable position than unfavored users.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public FavoredPrioritySettings Favoritism { get; set; } = new();

        // Pokémon_Scarlet_Violet_Settings

        [Category(Pokémon_Scarlet_Violet_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public OverworldSettingsSV OverworldSV { get; set; } = new();

        [Category(Pokémon_Scarlet_Violet_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TIDResetBotSettingsSV TIDResetSV { get; set; } = new();


        [Category(Pokémon_Scarlet_Violet_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public RaidSettingsSV RaidSV { get; set; } = new();

        [Category(Pokémon_Scarlet_Violet_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public RotatingRaidSettingsSV RotatingRaidSV { get; set; } = new();

        [Category(Pokémon_Scarlet_Violet_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public EggSettingsSV EggSV { get; set; } = new();


        // Pokémon_Sword_Shield_Settings

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public OverworldSettings OverworldSWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public EncounterSettings EncounterSWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public RaidSettings RaidSWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public LairBotSettings LairSWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DenSettings DenSWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public BoolSettings BoolSWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public CurryBotSettings CurrySWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public RollingRaidSettings RollingRaidSWSH { get; set; } = new();

        [Category(Pokémon_Sword_Shield_Settings), Description("Stop conditions for EggBot, FossilBot, and EncounterBot.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public StopConditionSettings StopConditions { get; set; } = new();

        // Pokémon_Legends_Arceus_Settings

        [Category(Pokémon_Legends_Arceus_Settings)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public ArceusBotSettings ArceusLA { get; set; } = new();

        // Stream_Configuration

        [Category(Stream_Configuration)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DiscordSettings Discord { get; set; } = new();

        [Category(Stream_Configuration)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TwitchSettings Twitch { get; set; } = new();

        [Category(Stream_Configuration)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public YouTubeSettings YouTube { get; set; } = new();

        [Category(Stream_Configuration), Description("Configure generation of assets for streaming.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public StreamSettings Stream { get; set; } = new();

        
    }
}