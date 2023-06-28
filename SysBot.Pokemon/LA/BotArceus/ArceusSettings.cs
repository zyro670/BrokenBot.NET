using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class ArceusBotSettings : IBotStateSettings, ICountSettings
    {
        private const string Arceus = nameof(Arceus);
        private const string Counts = nameof(Counts);
        public override string ToString() => "Arceus Bot Settings";

        [Category(Arceus), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(Arceus), Description("The method by which the bot will scan overworld Pokémon.")]
        public ArceusMode BotType { get; set; } = ArceusMode.PlayerCoordScan;

        [Category(Arceus), Description("Will hunt for the desired outbreak species if not empty. Separate species with a comma. Ex: Eevee,Rotom,Voltorb")]
        public string SpeciesToHunt { get; set; } = string.Empty;

        [Category(Arceus), Description("Special Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SpecialFiltersCategory SpecialConditions { get; set; } = new();

        [Category(Arceus), Description("Outbreak Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public OutbreakFiltersCategory OutbreakConditions { get; set; } = new();

        [Category(Arceus), Description("Distortion Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DistortionFiltersCategory DistortionConditions { get; set; } = new();

        [Category(Arceus), Description("MultiScan Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MultiScanFiltersCategory MultiScanConditions { get; set; } = new();

        [Category(Arceus), Description("AlphaScan Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public AlphaScanFiltersCategory AlphaScanConditions { get; set; } = new();

        [Category(Arceus), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        [Category(Arceus), Description("Enter Discord channel ID(s) to post Arceus embeds to. Feature has to be initialized via \"$arceusEmbed\" after every client restart.")]
        public string ArceusEmbedChannels { get; set; } = string.Empty;

        [Category(Arceus), TypeConverter(typeof(CategoryConverter<OutbreakFiltersCategory>))]
        public class OutbreakFiltersCategory
        {
            public override string ToString() => "Outbreak Conditions";

            [Category(Arceus), Description("Select the type of scan to perform for the Outbreak/MMO Hunter.")]
            public OutbreakScanType TypeOfScan { get; set; } = OutbreakScanType.Both;

            [Category(Arceus), Description("When enabled, the bot will teleport instead of walk/run for Outbreak/MMO Hunter.")]
            public bool TeleportToHunt { get; set; } = false;

            [Category(Arceus), Description("When enabled, the bot will permute all possible results.")]
            public bool Permute { get; set; } = false;

            [Category(Arceus), Description("When enabled, the bot will search for only an alpha shiny from a MMO.")]
            public bool AlphaShinyOnly { get; set; } = false;

            [Category(Arceus), Description("When enabled, the bot will check all Distortion spawns before going into Jubilife to read spawns.")]
            public bool CheckDistortionFirst { get; set; } = false;

            [Category(Arceus), Description("EXPERIMENTAL, does not work 100% of the time. When enabled, the bot will read every box to see if the encounter exists. It checks for Species, Shiny, and IsAlpha.")]
            public bool CheckBoxes { get; set; } = false;

            [Category(Arceus), Description("Toggle true if you just want to stop the bot when a desired species is found in a MMO or MO.")]
            public bool StopIfSpeciesFound { get; set; } = true;
        }

        [Category(Arceus), TypeConverter(typeof(CategoryConverter<DistortionFiltersCategory>))]
        public class DistortionFiltersCategory
        {
            public override string ToString() => "Distortion Conditions";

            [Category(Arceus), Description("Select the Location of the map you are hunting distortions for.")]
            public ArceusMap DistortionLocation { get; set; } = ArceusMap.ObsidianFieldlands;

            [Category(Arceus), Description("Wait time in minutes before starting a new distortion. If one does not spawn initially, stop and start the bot again.")]
            public int WaitTimeDistortion { get; set; } = 1;

            [Category(Arceus), Description("When enabled, the bot will only stop on Alpha Shinies in Distortions.")]
            public bool ShinyAlphaOnly { get; set; } = false;

            [Category(Arceus), Description("When enabled, the bot will stop when an Alpha is found in Distortions.")]
            public bool AnyAlpha { get; set; } = false;

            [Category(Arceus), Description("When enabled, the bot will teleport to the location of the match before pressing HOME.")]
            public bool TeleportToDistortionLocation { get; set; } = false;
        }

        [Category(Arceus), TypeConverter(typeof(CategoryConverter<MultiScanFiltersCategory>))]
        public class MultiScanFiltersCategory
        {
            public override string ToString() => "MultiScan Conditions";

            [Category(Arceus), Description("Enter the max number of advances for pathing.")]
            public int Advances { get; set; } = 20;

            [Category(Arceus), Description("Species for MultiSpawners Group ID")]
            public MultiSpawners MultiSpecies { get; set; } = MultiSpawners.Eevee;
        }

        [Category(Arceus), TypeConverter(typeof(CategoryConverter<AlphaScanFiltersCategory>))]
        public class AlphaScanFiltersCategory
        {
            public override string ToString() => "AlphaScan Conditions";

            [Category(Arceus), Description("Select the Location to Autofill Coords upon running PlayerCoordScan.")]
            public ArceusAutoFill AutoFillCoords { get; set; } = ArceusAutoFill.CampZone;

            [Category(Arceus), Description("Enter number of shiny rolls for Static Alphas.")]
            public ShinyRolls StaticAlphaShinyRolls { get; set; } = ShinyRolls.PerfectCharm;            

            [Category(Arceus), Description("Enter number of advances to do.")]
            public int Advances { get; set; } = 1;

            [Category(Arceus), Description("Toggle true if you just entered the map and didn't spawn the Pokémon")]
            public bool InItSpawn { get; set; } = true;

            [Category(Arceus), Description("Toggle true if the spawn comes out at night only")]
            public bool NightSpawn { get; set; } = true;

            [Category(Arceus), Description("Toggle true if spawn is 100% alpha.")]
            public bool SpawnIsStaticAlpha { get; set; } = false;

            [Category(Arceus), Description("Toggle true if spawn is in water. This is a little tricky as water encounters either go far away or hide under making it a little more difficult to encounter.")]
            public bool IsSpawnInWater { get; set; } = false;

            [Category(Arceus), Description("When enabled, the bot will stop the routine when match found.")]
            public bool HealInCamp { get; set; } = false;

            [Category(Arceus), Description("When enabled, the bot will stop the routine when match found.")]
            public bool StopOnMatch { get; set; } = false;
        }

        [Category(Arceus), TypeConverter(typeof(CategoryConverter<SpecialFiltersCategory>))]
        public class SpecialFiltersCategory
        {
            public override string ToString() => "Special Conditions";

            [Category(Arceus), Description("Toggle true if you want to run to the professor instead of teleport.")]
            public bool RunToProfessor { get; set; } = true;

            [Category(Arceus), Description("Select the Location of the map you are hunting for. Ignore this setting if you are running MMOHunter")]
            public ArceusMap ScanLocation { get; set; } = ArceusMap.ObsidianFieldlands;

            [Category(Arceus), Description("Enter number of advances to search.")]
            public int MaxAdvancesToSearch { get; set; } = 50;

            [Category(Arceus), Description("Injects result seed at advance 0.")]
            public bool InjectResultSeed { get; set; } = false;

            [Category(Arceus), Description("Wait time between teleporting and scanning.")]
            public int WaitMsBetweenTeleports { get; set; } = 1000;

            [Category(Arceus), Description("Enter your player coordinates for Camp Location X Coordinate. Should be 8 characters long.")]
            public string CampZoneX { get; set; } = "";

            [Category(Arceus), Description("Enter your player coordinates for Camp Location Y Coordinate. Should be 8 characters long.")]
            public string CampZoneY { get; set; } = "";

            [Category(Arceus), Description("Enter your player coordinates for Camp Location Z Coordinate. Should be 8 characters long.")]
            public string CampZoneZ { get; set; } = "";

            [Category(Arceus), Description("Enter your player coordinates for Spawn Location X Coordinate. Should be 8 characters long.")]
            public string SpawnZoneX { get; set; } = "";

            [Category(Arceus), Description("Enter your player coordinates for Spawn Location Y Coordinate. Should be 8 characters long.")]
            public string SpawnZoneY { get; set; } = "";

            [Category(Arceus), Description("Enter your player coordinates for Spawn Location Z Coordinate. Should be 8 characters long.")]
            public string SpawnZoneZ { get; set; } = "";
        }

        public class CategoryConverter<T> : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes) => TypeDescriptor.GetProperties(typeof(T));

            public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
        }

        private int _completedShinyAlphasFound;

        [Category(Counts), Description("Arceus Encountered Pokémon")]
        public int CompletedShinyAlphasFound
        {
            get => _completedShinyAlphasFound;
            set => _completedShinyAlphasFound = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedShinyAlphaFound() => Interlocked.Increment(ref _completedShinyAlphasFound);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedShinyAlphasFound != 0)
                yield return $"Shiny Alphas Found: {CompletedShinyAlphasFound}";
        }
    }
}
