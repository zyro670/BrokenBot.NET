using PKHeX.Core;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class LairSettings
    {
        private const string Lair = nameof(Lair);
        public override string ToString() => "Lair Bot Settings";

        [Category(Lair), Description("Legendary Pokémon to be hunted.")]
        public LairSpecies LairSpecies { get; set; } = LairSpecies.None;

        [Category(Lair), Description("Toggle \"True\" to reset the flag of the legendary you JUST caught. It is best to start on a save with all legends not caught, as this reads all the legend flags before the adventure, then just restores it to the previous state after the adventure.")]
        public bool ResetLegendaryCaughtFlag { get; set; } = false;

        [Category(Lair), Description("Select path of choice.")]
        public SelectPath SelectPath { get; set; } = SelectPath.GoLeft;

        [Category(Lair), Description("Toggle \"True\" to use \"StopConditions\" to only hunt legendaries with specific stop conditions.")]
        public bool UseStopConditionsPathReset { get; set; } = false;

        [Category(Lair), Description("Toggle \"False\" to continue doing random lairs after a shiny legendary is caught.")]
        public bool StopOnLegendary { get; set; } = true;

        [Category(Lair), Description("Toggle \"True\" to catch Pokémon. Default is false for speed routes.")]
        public bool CatchLairPokémon { get; set; } = false;

        [Category(Lair), Description("Toggle \"True\" to inject a desired adventure seed.")]
        public bool InjectSeed { get; set; } = false;

        [Category(Lair), Description("Enter your desired Lair Seed in HEX to inject. MUST be 16 characters long.")]
        public string SeedToInject { get; set; } = string.Empty;

        [Category(Lair), Description("Select your desired ball to catch Pokémon with.")]
        public Ball LairBall { get; set; } = Ball.None;

        [Category(Lair), Description("Output Showdown Set for all catches regardless of match.")]
        public bool AlwaysOutputShowdown { get; set; } = false;

        [Category(Lair), Description("Enter a Discord channel ID(s) to post shiny result embeds to. Feature has to be initialized via \"$lairEmbed\" after every client restart.")]
        public string ResultsEmbedChannels { get; set; } = string.Empty;

        [Category(Lair), Description("\"A\" button mash delay in milliseconds. Default is 600ms.")]
        public int MashDelay { get; set; } = 0;
    }
}