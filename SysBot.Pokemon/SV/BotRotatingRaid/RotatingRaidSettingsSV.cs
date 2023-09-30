using PKHeX.Core;
using System;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon
{
    public class RotatingRaidSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string Hosting = nameof(Hosting);
        private const string Counts = nameof(Counts);
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "RotatingRaidtSV Settings";

        [Category(FeatureToggle), Description("URL to Pokémon Automation's Tera Ban List json (or one matching the required structure).")]
        public string BanListURL { get; set; } = "https://raw.githubusercontent.com/PokemonAutomation/ServerConfigs-PA-SHA/main/PokemonScarletViolet/TeraAutoHost-BanList.json";

        [Category(Hosting), Description("Amount of raids before updating the ban list. If you want the global ban list off, set this to -1.")]
        public int RaidsBetweenUpdate { get; set; } = 3;

        [Category(Hosting), Description("When enabled, the bot will attempt to auto-generate Raid Parameters from the \"raidsv.txt\" file on botstart.")]
        public bool GenerateParametersFromFile { get; set; } = true;

        [Category(Hosting), Description("RotatingRaid Preset Filters"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RotatingRaidPresetFiltersCategory PresetFilters { get; set; } = new();

        [Category(Hosting), Description("Raid embed parameters.")]
        public List<RotatingRaidParameters> RaidEmbedParameters { get; set; } = new();

        [Category(Hosting), Description("Enter the total number of raids to host before the bot automatically stops. Default is 0 to ignore this setting.")]
        public int TotalRaidsToHost { get; set; } = 0;

        [Category(Hosting), Description("Catch limit per player before they get added to the ban list automatically. If set to 0 this setting will be ignored.")]
        public int CatchLimit { get; set; } = 0;

        [Category(Hosting), Description("Minimum amount of seconds to wait before starting a raid.")]
        public int TimeToWait { get; set; } = 90;

        [Category(FeatureToggle), Description("When enabled, the embed will countdown the amount of seconds in \"TimeToWait\" until starting the raid.")]
        public bool IncludeCountdown { get; set; } = false;

        [Category(Hosting), Description("Lobby Options"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LobbyFiltersCategory LobbyOptions { get; set; } = new();

        [Category(FeatureToggle), Description("When enabled, the bot will attempt take screenshots for the Raid Embeds. If you experience crashes often about \"Size/Parameter\" try setting this to false.")]
        public bool TakeScreenshot { get; set; } = true;

        [Category(FeatureToggle), Description("When enabled, the bot will hide the raid code from the Discord embed.")]
        public bool HideRaidCode { get; set; } = false;

        [Category(Hosting), Description("Users NIDs here are banned raiders.")]
        public RemoteControlAccessList RaiderBanList { get; set; } = new() { AllowIfEmpty = false };

        [Category(Hosting), Description("When enabled, the bot will inject the current day seed to tomorrow's day seed.")]
        public bool KeepDaySeed { get; set; } = false;

        [Category(FeatureToggle), Description("Set your Switch Date/Time format in the Date/Time settings. The day will automatically rollback by 1 if the Date changes.")]
        public DTFormat DateTimeFormat { get; set; } = DTFormat.MMDDYY;

        [Category(Hosting), Description("When enabled, the bot will use the overshoot method to apply rollover correction, otherwise will use DDOWN clicks.")]
        public bool UseOvershoot { get; set; } = false;

        [Category(Hosting), Description("Amount of times to hit DDOWN for accessing date/time settings during rollover correction. [Default: 39 Clicks]")]
        public int DDOWNClicks { get; set; } = 39;

        [Category(Hosting), Description("Time to scroll down duration in milliseconds for accessing date/time settings during rollover correction. You want to have it overshoot the Date/Time setting by 1, as it will click DUP after scrolling down. [Default: 930ms]")]
        public int HoldTimeForRollover { get; set; } = 900;

        [Category(Hosting), Description("When enabled, start the bot when you are on the HOME screen with the game closed. The bot will only run the rollover routine so you can try to configure accurate timing.")]
        public bool ConfigureRolloverCorrection { get; set; } = false;

        [Category(FeatureToggle), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        private int _completedRaids;

        [Category(Counts), Description("Raids Started")]
        public int CompletedRaids
        {
            get => _completedRaids;
            set => _completedRaids = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedRaids() => Interlocked.Increment(ref _completedRaids);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedRaids != 0)
                yield return $"Started Raids: {CompletedRaids}";
        }

        public class RotatingRaidParameters
        {
            public override string ToString() => $"{Title}";
            public bool ActiveInRotation { get; set; } = true;
            public TeraCrystalType CrystalType { get; set; } = TeraCrystalType.Base;
            public string[] Description { get; set; } = Array.Empty<string>();
            public bool IsCoded { get; set; } = true;
            public bool IsSet { get; set; } = false;
            public bool IsShiny { get; set; } = true;
            public Species Species { get; set; } = Species.None;
            public int SpeciesForm { get; set; } = 0;
            public string[] PartyPK { get; set; } = Array.Empty<string>();
            public bool SpriteAlternateArt { get; set; } = false;
            public string Seed { get; set; } = "0";
            public string Title { get; set; } = string.Empty;
        }

        [Category(Hosting), TypeConverter(typeof(CategoryConverter<RotatingRaidPresetFiltersCategory>))]
        public class RotatingRaidPresetFiltersCategory
        {
            public override string ToString() => "Preset filters.";

            [Category(Hosting), Description("If true, the bot will attempt to auto-generate Raid Embeds based on the \"preset.txt\" file.")]
            public bool UsePresetFile { get; set; } = true;

            [Category(Hosting), Description("If true, the bot will use the first line of preset as title.")]
            public bool TitleFromPreset { get; set; } = true;

            [Category(Hosting), Description("If true, the bot will overwrite any set Title with the new one.")]
            public bool ForceTitle { get; set; } = true;

            [Category(Hosting), Description("If true, the bot will overwrite any set Description with the new one.")]
            public bool ForceDescription { get; set; } = true;

            [Category(Hosting), Description("If true, the bot will append the moves to the preset Description.")]
            public bool IncludeMoves { get; set; } = true;

            [Category(Hosting), Description("If true, the bot will append the Special Rewards to the preset Description.")]
            public bool IncludeRewards { get; set; } = true;
        }

        [Category(Hosting), TypeConverter(typeof(CategoryConverter<LobbyFiltersCategory>))]
        public class LobbyFiltersCategory
        {
            public override string ToString() => "Lobby Filters";

            [Category(Hosting), Description("OpenLobby - Opens the Lobby after x Empty Lobbies\nSkipRaid - Moves on after x losses/empty Lobbies\nContinue - Continues hosting the raid")]
            public LobbyMethodOptions LobbyMethodOptions { get; set; } = LobbyMethodOptions.OpenLobby;

            [Category(Hosting), Description("Empty raid limit per parameter before the bot hosts an uncoded raid. Default is 3 raids.")]
            public int EmptyRaidLimit { get; set; } = 3;

            [Category(Hosting), Description("Empty/Lost raid limit per parameter before the bot moves on to the next one. Default is 3 raids.")]
            public int SkipRaidLimit { get; set; } = 3;

            [Category(FeatureToggle), Description("Set the action you would want your bot to perform. MashA presses A every 3.5s, while TurboA will press A every 1.5s.")]
            public RaidAction Action { get; set; } = RaidAction.AFK;
        }

        public class CategoryConverter<T> : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(T));

            public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
        }
    }    
}