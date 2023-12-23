using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon;

public class OverworldSettings : IBotStateSettings, ICountSettings
{
    private const string Overworld = nameof(Overworld);
    private const string Counts = nameof(Counts);
    public override string ToString() => "Overworld Bot Settings";

    [Category(Overworld), Description("When enabled, the bot will continue after finding a suitable match.")]
    public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

    [Category(Overworld), Description("The method by which the bot will scan overworld Pokémon.")]
    public OverworldMode ScanType { get; set; } = OverworldMode.OffsetScan;

    [Category(Overworld), Description("Enter your Overworld offset to use for CustomOffset Scanning. Offset can be found using OffsetScan.")]
    public string Offset { get; set; } = "4505DB00";

    [Category(Overworld), Description("Select the Location to Autofill Coords upon running OffsetScan or PlayerCoordScan.")]
    public CoordsAutoFill AutoFillCoords { get; set; } = CoordsAutoFill.NoAutoFill;

    [Category(Overworld), Description("If not blank will check for a match from Stop Conditions plus the Species listed here. Do not include spaces for Species name and separate species with a comma. Ex: IronThorns,Cetoddle,Pikachu,RoaringMoon")]
    public string SpeciesToHunt { get; set; } = string.Empty;

    [Category(Overworld), Description("Extra Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public OverworldConditionsCategory Conditions { get; set; } = new();

    [Category(Overworld), Description("Teleport Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public OverworldTeleportFiltersCategory TeleportConditions { get; set; } = new();

    [Category(Overworld), Description("Movement Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public OverworldMovementFiltersCategory MovementConditions { get; set; } = new();

    [Category(Overworld), Description("Battle Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public TosserCategory BallTosser { get; set; } = new();

    [Category(Overworld), Description("Select the method by which you'd like to navigate through the Overworld to refresh Overworld data.")]
    public NavigationType NavigationType { get; set; } = NavigationType.Run;

    [Category(Overworld), Description("Select the order of your navigation if Navigation is set to Run.")]
    public NavigationOrder NavigationOrder { get; set; } = NavigationOrder.DownUp;

    [Category(Overworld), Description("Toggle \"True\" to true to put the Switch to sleep once a match is found.")]
    public bool Sleep { get; set; } = false;

    [Category(Overworld), Description("Amount of time to pass before rolling back time to amount of hours desired. Default is wait 1 hour to rollback 1 hour. Useful for Mark hunting at specific times. Do not set to 0. Optimized for Encounter and Overworld bots.")]
    public int RollBackTime { get; set; } = 1;

    [Category(Overworld), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
    public bool ScreenOff { get; set; } = false;

    private int _completedScans;

    [Category(Counts), Description("Scanned Overworld Pokémon")]
    public int CompletedScans
    {
        get => _completedScans;
        set => _completedScans = value;
    }

    [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public int AddCompletedScans() => Interlocked.Increment(ref _completedScans);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedScans != 0)
            yield return $"Overworld Scans: {CompletedScans}";
    }

    [Category(Overworld)]
    [TypeConverter(typeof(OverworldConditionsCategoryConverter))]
    public class OverworldConditionsCategory
    {
        public override string ToString() => "Extra Conditions";

        [Category(Overworld), Description("Leave blank to hunt Any Mark, otherwise list marks to ignore. Use commas as a separator. Do not include the word Mark. Example: Uncommon,Dawn,Dusk")]
        public string UnwantedMarks { get; set; } = "";

        [Category(Overworld), Description("Amount of times to scan for a Pokémon from the Offset. Do not make 0. Example: If you see 4 Pokémon spawned in the Overworld, find the starting offset then set this value to 4 to just scan 4 times instead of scanning 10 times when there are only 4 Pokémon present.")]
        public int IncrementFromOffset { get; set; } = 8;

        [Category(Overworld), Description("Select the method by which you'd like to reset from an unwanted encounter.")]
        public UnwantedCorrection UnwantedEncounterCorrection { get; set; } = UnwantedCorrection.ResetGame;

        [Category(Overworld), Description("Toggle \"True\" to stop on any: Brilliant aura, 0 Atk, 0 Spa, or 0 Spe shiny encounter that has a mark. If true, will ignore the Unwanted Marks category.")]
        public bool StopOnBrilliantOrZeroStat { get; set; } = false;

        [Category(Overworld), Description("Toggle \"True\" to throw our desired ball endlessly until we catch our shiny. Will reset the game if the encounter dies or we lose the battle. If this is true, ensure Ball Tosser Conditions are set to your desired parameters.")]
        public bool BallTosser { get; set; } = false;

    }
    [Category(Overworld)]
    [TypeConverter(typeof(TosserCategoryConverter))]
    public class TosserCategory
    {
        public override string ToString() => "Battle Conditions";
        [Category(Overworld), Description("Toggle \"True\" to use Hp and Sleep hax while throwing our desired ball endlessly until we catch our shiny. Will reset the game if the encounter dies or we lose the battle.")]
        public bool BattleHax { get; set; } = false;

        [Category(Overworld), Description("Toggle \"True\" to inject a Battle Ready Ball Tossing Mon into Box 1 Slot 1 before going into battle. If this is true, make sure your Pokemon icon is the second icon in the menu and cursor should be hovered in the top left icon.")]
        public bool RefreshBattleFighter { get; set; } = false;

        [Category(Overworld), Description("Toggle \"True\" to inject a Battle Ready Ball Tossing Gallade into Box 1 Slot 1 before going into battle. Will save your Box 1 Slot 1 Pokémon to the saved folder. If using this setting ensure that the Save button is your first Menu Icon, and the Pokemon icon is the second one. Cursor should be hovered over the Save button when menu is opened.")]
        public BattleInjector DesiredFighter { get; set; } = BattleInjector.Gallade;

        [Category(Overworld), Description("Select your desired nature to sync with. Only viable if you are refreshing Gardevoir.")]
        public Nature DesiredNature { get; set; } = Nature.Hardy;

        [Category(Overworld), Description("Select your desired ball to catch Pokémon with.")]
        public LairBall DesiredBall { get; set; } = LairBall.Poke;

        [Category(Overworld), Description("Toggle \"True\" to spam False Swipe until encounter has 1 Hp! Your first Pokémon must know FalseSwipe and have it in Move Slot 1, recommend using Gallade as it can learn Thunder Wave AND FalseSwipe.")]
        public bool WhackTil1Hp { get; set; } = false;

        [Category(Overworld), Description("Toggle \"True\" to use Thunder Wave to put our encounter to sleep. Your first Pokémon must know Thunder Wave and have it in Move Slot 2, recommend using Gallade as it can learn Thunder Wave AND FalseSwipe. Only one status condition can be applied, if this is true set ApplySleepHax to false.")]
        public bool ApplyParalysis { get; set; } = false;

        [Category(Overworld), Description("Select the party slot to replace with a Gallade.")]
        public RefreshAndReplace ReplacePartySlot { get; set; } = RefreshAndReplace.PartySlot1;
    }        

    [Category(Overworld)]
    [TypeConverter(typeof(OverworldMovementFiltersCategoryConverter))]
    public class OverworldMovementFiltersCategory
    {
        public override string ToString() => "Movement Conditions";

        [Category(Overworld), Description("Indicates how long the character will move north before every scan.")]
        public int MoveUpMs { get; set; } = 3000;

        [Category(Overworld), Description("Indicates how long the character will move south before every scan.")]
        public int MoveDownMs { get; set; } = 3000;

        [Category(Overworld), Description("Indicates how long the character will move east before every scan. Will cause issues in Wild Area, IOA, or CT.")]
        public int MoveRightMs { get; set; } = 3000;

        [Category(Overworld), Description("Indicates how long the character will move west before every scan. Will cause issues in Wild Area, IOA, or CT.")]
        public int MoveLeftMs { get; set; } = 3000;
    }
    [Category(Overworld)]
    [TypeConverter(typeof(OverworldTeleportFiltersCategoryConverter))]
    public class OverworldTeleportFiltersCategory
    {
        public override string ToString() => "Teleport Conditions";

        [Category(Overworld), Description("Wait time between teleporting and scanning.")]
        public int WaitMsBetweenTeleports { get; set; } = 3000;

        [Category(Overworld), Description("Toggle \"True\" to teleport to a shiny when a match is found.")]
        public bool TeleportToMatch { get; set; } = false;

        [Category(Overworld), Description("Enter your player coordinates for Scan Location X Coordinate. Should be 8 characters long.")]
        public string ScanZoneX { get; set; } = "";

        [Category(Overworld), Description("Enter your player coordinates for Scan Location Y Coordinate. Should be 8 characters long.")]
        public string ScanZoneY { get; set; } = "";

        [Category(Overworld), Description("Enter your player coordinates for Scan Location Z Coordinate. Should be 8 characters long.")]
        public string ScanZoneZ { get; set; } = "";

        [Category(Overworld), Description("Enter your player coordinates for Despawn Location X Coordinate. Should be 8 characters long.")]
        public string DespawnZoneX { get; set; } = "";

        [Category(Overworld), Description("Enter your player coordinates for Despawn Location Y Coordinate. Should be 8 characters long.")]
        public string DespawnZoneY { get; set; } = "";

        [Category(Overworld), Description("Enter your player coordinates for Despawn Location Z Coordinate. Should be 8 characters long.")]
        public string DespawnZoneZ { get; set; } = "";

        [Category(Overworld), Description("If a match is found, this field should be populated with their offset. Ignore this field otherwise.")]
        public string MatchFoundOffset { get; set; } = "4505DB00";
    }
    public class OverworldTeleportFiltersCategoryConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object? value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(OverworldTeleportFiltersCategory));

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    }
    public class OverworldMovementFiltersCategoryConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object? value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(OverworldMovementFiltersCategory));

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    }
    public class TosserCategoryConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object? value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(TosserCategory));

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    }
    public class OverworldConditionsCategoryConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object? value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(OverworldConditionsCategory));

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
    }
}
