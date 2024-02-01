using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon;

public class OverworldSettingsSV : IBotStateSettings, ICountSettings
{
    private const string Overworld = nameof(Overworld);
    private const string Counts = nameof(Counts);
    public override string ToString() => "OverworldBotSV Settings";

    [Category(Overworld), Description("When enabled, the bot will continue after finding a suitable match.")]
    public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.PauseWaitAcknowledge;

    [Category(Overworld), Description("If not blank will check for a match from Stop Conditions plus the Species listed here. Do not include spaces for Species name and separate species with a comma. Ex: IronThorns,Cetoddle,Pikachu,RoaringMoon")]
    public string SpeciesToHunt { get; set; } = string.Empty;

    [Category(Overworld), Description("Select which type of mark group to hunt for.")]
    public MarkSetting MarkSelection { get; set; } = MarkSetting.AnyMark;

    [Category(Overworld), Description("Select which location you are scanning.")]
    public Location LocationSelection { get; set; } = Location.NonAreaZero;

    [Category(Overworld), Description("When enabled, the bot will stop for 3 Segment Dunsparce or Family of Three Maus and matches StopConditions.")]
    public bool StopOnOneInOneHundredOnly { get; set; } = false;

    [Category(Overworld), Description("Select the type of hunt you want to participate in. [Default: MakeASandwich].")]
    public Selection PicnicSelection { get; set; } = Selection.MakeASandwich;

    [Category(Overworld), Description("Picnic Filters"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public PicnicFiltersCategory PicnicFilters { get; set; } = new();

    [Category(Overworld), Description("Rollover Filters"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public RolloverFiltersCategory RolloverFilters { get; set; } = new();

    [Category(Overworld), Description("Movement Filters"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public MovementFiltersCategory MovementFilters { get; set; } = new();

    [Category(Overworld), Description("Minimum amount of milliseconds to wait before scanning for spawns.")]
    public int TimeToWaitBetweenSpawns { get; set; } = 1000;

    [Category(Overworld), Description("When enabled, the bot will attempt to collide to your match. As collision is not 100% in SV unlike other games, results may vary. Restart your game if you are somewhere weird.")]
    public bool CollideToMatch { get; set; } = false;

    [Category(Overworld), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
    public bool ScreenOff { get; set; }

    [Category(Overworld), TypeConverter(typeof(CategoryConverter<PicnicFiltersCategory>))]
    public class PicnicFiltersCategory
    {
        public override string ToString() => "Picnic Conditions";

        [Category(Overworld), Description("Select the type of sandwich you want to make. Refer to the wiki for important information.")]
        public SandwichSelection TypeOfSandwich { get; set; } = SandwichSelection.Normal;

        [Category(Overworld), Description("Select the flavor of sandwich you want to make. Refer to the wiki for important information.")]
        public SandwichFlavor SandwichFlavor { get; set; } = SandwichFlavor.Encounter;

        [Category(Overworld), Description("When enabled, the bot will click DUP on Item 1.")]
        public bool Item1DUP { get; set; } = false;

        [Category(Overworld), Description("Item 1.")]
        public PicnicFillings Item1 { get; set; } = PicnicFillings.Lettuce;

        [Category(Overworld), Description("When enabled, the bot will click DUP on Item 2.")]
        public bool Item2DUP { get; set; } = true;

        [Category(Overworld), Description("Item 2.")]
        public PicnicCondiments Item2 { get; set; } = PicnicCondiments.Mayonnaise;

        [Category(Overworld), Description("When enabled, the bot will click DUP on Item 3.")]
        public bool Item3DUP { get; set; } = true;

        [Category(Overworld), Description("Item 3.")]
        public PicnicCondiments Item3 { get; set; } = PicnicCondiments.Mayonnaise;

        [Category(Overworld), Description("When enabled, the bot will click DUP on Item 4.")]
        public bool Item4DUP { get; set; } = true;

        [Category(Overworld), Description("Item 4.")]
        public PicnicCondiments Item4 { get; set; } = PicnicCondiments.Mayonnaise;

        [Category(Overworld), Description("Amount of ingredients to hold.")]
        public int AmountOfIngredientsToHold { get; set; } = 3;

        [Category(Overworld), Description("Amount of time to hold L stick up to ingredients for sandwich. [Default: 630ms]")]
        public int HoldUpToIngredients { get; set; } = 630;

    }

    [Category(Overworld), TypeConverter(typeof(CategoryConverter<MovementFiltersCategory>))]
    public class MovementFiltersCategory
    {
        public override string ToString() => "Movement Conditions";

        [Category(Overworld), Description("Indicates how long the character will move north before every scan.")]
        public int MoveUpMs { get; set; } = 3000;

        [Category(Overworld), Description("Indicates how long the character will move south before every scan.")]
        public int MoveDownMs { get; set; } = 3000;
    }

    [Category(Overworld), TypeConverter(typeof(CategoryConverter<RolloverFiltersCategory>))]
    public class RolloverFiltersCategory
    {
        public override string ToString() => "Rollover Conditions";

        [Category(Overworld), Description("When enabled, the bot will check if our dayseed changes to attempt preventing a lost outbreak.")]
        public bool PreventRollover { get; set; } = false;

        [Category(Overworld), Description("When PreventRollover is enabled, the bot will attempt to go back 1 hour every 2 sandwiches. You must use zyro's usb-botbase release and Sync your Date/Time Settings if you select TimeSkip, otherwise Date/Time should be unsynced for the other options.")]
        public RolloverPrevention RolloverPrevention { get; set; } = RolloverPrevention.TimeSkip;

        [Category(Overworld), Description("Set your Switch Date/Time format in the Date/Time settings. The day will automatically rollback by 1 if the Date changes.")]
        public DTFormat DateTimeFormat { get; set; } = DTFormat.MMDDYY;

        [Category(Overworld), Description("Time to scroll down duration in milliseconds for accessing date/time settings during rollover correction. You want to have it overshoot the Date/Time setting by 1, as it will click DUP after scrolling down. [Default: 930ms]")]
        public int HoldTimeForRollover { get; set; } = 900;

        [Category(Overworld), Description("Amount of times to hit DDOWN for accessing date/time settings during rollover correction. [Default: 39 Clicks]")]
        public int DDOWNClicks { get; set; } = 39;

        [Category(Overworld), Description("If true, start the bot when you are on the HOME screen with the game closed. The bot will only run the rollover routine so you can try to configure accurate timing.")]
        public bool ConfigureRolloverCorrection { get; set; } = false;
    }

    public class CategoryConverter<T> : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(T));

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    }

    private int _completedScans;
    private int _completedShinyScans;

    [Category(Counts), Description("Encounters Scanned")]
    public int CompletedScans
    {
        get => _completedScans;
        set => _completedScans = value;
    }

    [Category(Counts), Description("Shiny Encounters Scanned")]
    public int CompletedShinyScans
    {
        get => _completedShinyScans;
        set => _completedShinyScans = value;
    }

    [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public int AddCompletedScans() => Interlocked.Increment(ref _completedScans);

    public int AddShinyScans() => Interlocked.Increment(ref _completedShinyScans);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedScans != 0)
            yield return $"Encounters Scanned: {CompletedScans}";
        if (CompletedShinyScans != 0)
            yield return $"Shiny Encounters Scanned: {CompletedShinyScans}";
    }

    public enum Location
    {
        NonAreaZero = 0,
        ResearchStation1 = 1,
        ResearchStation2 = 2,
        ResearchStation3 = 3,
        ResearchStation4 = 4,
        SecretCave = 5,
        TownBorder = 6,
    }

    public enum Selection
    {
        MakeASandwich = 0,
        StopAt30Min = 1,
    }

    public enum RolloverPrevention
    {
        TimeSkip = 0,
        DDOWN = 1,
        Overshoot = 2,
    }

    public enum MarkSetting
    {
        AnyMark = 0,
        PersonalityAndUpANDScalar = 1,
        PersonalityAndUpORScalar = 2,
        PersonalityAndUp = 3,
        WeatherAndUpAndScalar = 4,
        WeatherAndUp = 5,
        TimeAndUpANDScalar = 6,
        TimeAndUp = 7,
        Scalar = 8,
        DisableMarkCheck = 9,
    }
}
