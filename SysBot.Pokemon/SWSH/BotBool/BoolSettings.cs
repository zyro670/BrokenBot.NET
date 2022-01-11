using System;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class BoolSettings
    {
        private const string Bool = nameof(Bool);

        public override string ToString() => "Bool Bot Settings";

        [Category(Bool), Description("The method by which the bot will reset booleans of Pokémon. If you are skipping for a location, select a desired location under \"LocationTarget\" then run Skipper.  If you are skipping for a species, select the species under \"SpeciesTarget\" then run Skipper. Make sure your menu icon is hovered over the Pokédex when the menu opens.")]
        public BoolMode BoolType { get; set; } = BoolMode.Skipper;

        [Category(Bool), Description("Extra Conditions"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DexRecConditionsCategory DexRecConditions { get; set; } = new();

        [Category(Bool)]
        [TypeConverter(typeof(DexRecConditionsCategoryConverter))]
        public class DexRecConditionsCategory
        {
            public override string ToString() => "DexRec Conditions";

            [Category(Bool), Description("When set to \"Skipper\" and location is set to \"None\", it will skip indefinitely. If a location is selected it will stop skipping when location matches the target. Otherwise, select \"Injector\" to set a location of choice to have it injected.")]
            public DexRecLoc LocationTarget { get; set; } = DexRecLoc.None;

            [Category(Bool), Description("When set to \"Skipper\" and all species are set to \"None\", will skip indefinitely. If any slot has a species, will stop when species matches the target. Otherwise, select \"Injector\" to set the desired species. Leaving a slot as \"None\" will empty it.")]
            public DexRecSpecies[] SpeciesTargets { get; set; } = { DexRecSpecies.None, DexRecSpecies.None, DexRecSpecies.None, DexRecSpecies.None };
        }

        private sealed class DexRecConditionsCategoryConverter : TypeConverter
        {
            public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) => TypeDescriptor.GetProperties(typeof(DexRecConditionsCategory));

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType != typeof(string) && base.CanConvertTo(context, destinationType);
        }

    }
}