using PKHeX.Core;
using System;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class DenSettings
    {
        private const string DenSkip = nameof(DenSkip);

        public override string ToString() => "Den Bot Settings";

        [Category(DenSkip), Description("DenBot mode.")]
        public DenMode DenMode { get; set; } = DenMode.SeedSearch;

        [Category(DenSkip), Description("Select Den Type.")]
        public DenType DenType { get; set; } = DenType.Vanilla;

        [Category(DenSkip), Description("Den ID (1 - 100 if Vanilla, 1 - 90 if IoA, 1 - 86 if CT).")]
        public uint DenID { get; set; } = 1;

        [Category(DenSkip), Description("Star level (0 - 4).")]
        public int Star { get; set; } = 4;

        [Category(DenSkip), Description("Randroll (1 - 100).")]
        public int Randroll { get; set; } = 99;

        [Category(DenSkip), Description("Beam type for seed search.")]
        public BeamType DenBeamType { get; set; } = BeamType.CommonWish;

        [Category(DenSkip), Description("Search criteria for target Pokémon."), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DenFiltersCategory DenFilters { get; set; } = new();

        [Category(DenSkip), Description("If enabled, seed result output will display 1-3* Pokémon instead of 3-5* Pokémon.")]
        public bool BabyDen { get; set; } = false;

        [Category(DenSkip), Description("Range to search for the desired frame.")]
        public long SearchRange { get; set; } = 100000;

        [Category(DenSkip), Description("Frames to skip from current seed.")]
        public int SkipCount { get; set; } = 0;

        [Category(DenSkip), Description("Additional delay between skips in milliseconds. Base delay is 360 ms.")]
        public int SkipDelay { get; set; } = 0;

        [Category(DenSkip), Description("Host after skipping?")]
        public bool HostAfterSkip { get; set; } = false;

        [Category(DenSkip), Description("Seed to inject. Please disclose seed-injected raids.")]
        public string SeedToInject { get; set; } = string.Empty;

        [Category(DenSkip), Description("Should we reset network time. \"Reset\" - based on current system time, \"ResetNTP\" - sync with network.")]
        public TimeReset TimeReset { get; set; } = TimeReset.Reset;

        [Category(DenSkip)]
        [TypeConverter(typeof(DenFiltersCategoryConverter))]
        public class DenFiltersCategory
        {
            public override string ToString() => "Pokémon filters.";

            [Category(DenSkip), Description("Shiny type.")]
            public ShinyType ShinyType { get; set; } = ShinyType.NotShiny;

            [Category(DenSkip), Description("Guaranteed IVs.")]
            public uint GuaranteedIVs { get; set; } = 4;

            [Category(DenSkip), Description("Desired IV spread (HP/Atk/Def/SpA/SpD/Spe). Leave \"x\" for any value.")]
            public string IVSpread { get; set; } = "x/x/x/x/x/x";

            [Category(DenSkip), Description("Gender.")]
            public GenderType Gender { get; set; } = GenderType.Any;

            [Category(DenSkip), Description("Gender Ratio.")]
            public GenderRatio GenderRatio { get; set; } = GenderRatio.Even;

            [Category(DenSkip), Description("Ability.")]
            public AbilityType Ability { get; set; } = AbilityType.Any;

            [Category(DenSkip), Description("Nature.")]
            public Nature Nature { get; set; } = Nature.Random;

            [Category(DenSkip), Description("Characteristic.")]
            public Characteristics Characteristic { get; set; } = Characteristics.Any;
        }

        private sealed class DenFiltersCategoryConverter : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) => TypeDescriptor.GetProperties(typeof(DenFiltersCategory));

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
        }

        public uint[] IVParse()
        {
            uint[] IVs = new uint[] { 255, 255, 255, 255, 255, 255 };
            string[] IVfilter = DenFilters.IVSpread.Replace("x", "255").Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (uint i = 0; i < IVfilter.Length && i < 6; i++)
            {
                if (uint.TryParse(IVfilter[i], out uint iv))
                    IVs[i] = iv;
            }
            return IVs;
        }
    }
}