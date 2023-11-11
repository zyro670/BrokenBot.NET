using Discord.Commands;
using System;
using System.Collections.Generic;

namespace Sysbot.Pokemon.Discord;
public class PKMSVShowdownSets
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public string Nickname { get; set; }
    public string OTGender { get; set; }
    public string Item { get; set; }
    public string Ability { get; set; }
    public string TeraType { get; set; }
    public string Level { get; set; }
    public string Shiny { get; set; }
    public string Nature { get; set; }

    public string HP { get; set; }
    public string Atk { get; set; }
    public string Def { get; set; }
    public string SpA { get; set; }
    public string SpD { get; set; }
    public string Spe { get; set; }
    public string GVs { get; set; }
    public string Ball { get; set; }
    public string OT { get; set; }
    public string TID { get; set; }
    public string SID { get; set; }
    public string Form { get; set; }
    public string Language { get; set; }
    public string DynamaxLevel { get; set; }
    public List<string> IVs { get; set; }
    public List<string> EVs { get; set; }
    public List<string> Moves { get; set; }
}

public class PKMSVModule : ModuleBase<SocketCommandContext>
{
    public static Dictionary<string, PKMSVShowdownSets> SVShowdownSetsCollection = new Dictionary<string, PKMSVShowdownSets>(StringComparer.OrdinalIgnoreCase)
    {
// Charmander
{
    "Charmander",
    new PKMSVShowdownSets
    {
        Name = "Charmander",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Charmeleon
{
    "Charmeleon",
    new PKMSVShowdownSets
    {
        Name = "Charmeleon",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Charizard
{
    "Charizard",
    new PKMSVShowdownSets
    {
        Name = "Charizard",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Ekans
{
    "Ekans",
    new PKMSVShowdownSets
    {
        Name = "Ekans",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Wrap", "Leer" }
    }
},

// Arbok
{
    "Arbok",
    new PKMSVShowdownSets
    {
        Name = "Arbok",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Wrap", "Leer" }
    }
},

// Pikachu
{
    "Pikachu",
    new PKMSVShowdownSets
    {
        Name = "Pikachu",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Tail Whip", "Thunder Shock" }
    }
},

// Raichu
{
    "Raichu",
    new PKMSVShowdownSets
    {
        Name = "Raichu",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tail Whip", "Thunder Shock" }
    }
},

// Sandshrew
{
    "Sandshrew",
    new PKMSVShowdownSets
    {
        Name = "Sandshrew",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Scratch", "Defense Curl" }
    }
},

// Sandshrew-Alola (M)
{
    "Sandshrew-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Sandshrew",
        Form = "Alola (M)",
        Ability = "Snow Cloak",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Defense Curl", "Scratch" }
    }
},

// Sandslash
{
    "Sandslash",
    new PKMSVShowdownSets
    {
        Name = "Sandslash",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Scratch", "Defense Curl" }
    }
},

// Sandslash-Alola (M)
{
    "Sandslash-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Sandslash",
        Form = "Alola (M)",
        Ability = "Snow Cloak",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Defense Curl", "Scratch" }
    }
},

// Clefairy
{
    "Clefairy",
    new PKMSVShowdownSets
    {
        Name = "Clefairy",
        Gender = "M",
        Ability = "Cute Charm",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Pound", "Splash", "Copycat" }
    }
},

// Clefable
{
    "Clefable",
    new PKMSVShowdownSets
    {
        Name = "Clefable",
        Gender = "M",
        Ability = "Magic Guard",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Pound", "Splash", "Copycat" }
    }
},

// Vulpix
{
    "Vulpix",
    new PKMSVShowdownSets
    {
        Name = "Vulpix",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tail Whip", "Ember" }
    }
},

// Vulpix-Alola (M)
{
    "Vulpix-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Vulpix",
        Form = "Alola (M)",
        Ability = "Snow Cloak",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tail Whip", "Powder Snow" }
    }
},

// Ninetales
{
    "Ninetales",
    new PKMSVShowdownSets
    {
        Name = "Ninetales",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tail Whip", "Ember" }
    }
},

// Ninetales-Alola (M)
{
    "Ninetales-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Ninetales",
        Form = "Alola (M)",
        Ability = "Snow Cloak",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tail Whip", "Powder Snow" }
    }
},

// Jigglypuff
{
    "Jigglypuff",
    new PKMSVShowdownSets
    {
        Name = "Jigglypuff",
        Gender = "M",
        Ability = "Cute Charm",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Pound", "Sing", "Copycat" }
    }
},

// Wigglytuff
{
    "Wigglytuff",
    new PKMSVShowdownSets
    {
        Name = "Wigglytuff",
        Gender = "M",
        Ability = "Cute Charm",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Pound", "Sing", "Copycat" }
    }
},

// Venonat
{
    "Venonat",
    new PKMSVShowdownSets
    {
        Name = "Venonat",
        Gender = "M",
        Ability = "Compound Eyes",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "Disable" }
    }
},

// Venomoth
{
    "Venomoth",
    new PKMSVShowdownSets
    {
        Name = "Venomoth",
        Gender = "M",
        Ability = "Shield Dust",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Disable" }
    }
},

// Diglett
{
    "Diglett",
    new PKMSVShowdownSets
    {
        Name = "Diglett",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Scratch", "Sand Attack" }
    }
},

// Diglett-Alola (M)
{
    "Diglett-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Diglett",
        Form = "Alola (M)",
        Ability = "Tangling Hair",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Sand Attack", "Metal Claw" }
    }
},

// Dugtrio
{
    "Dugtrio",
    new PKMSVShowdownSets
    {
        Name = "Dugtrio",
        Gender = "M",
        Ability = "Arena Trap",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Scratch", "Sand Attack" }
    }
},

// Dugtrio-Alola (M)
{
    "Dugtrio-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Dugtrio",
        Form = "Alola (M)",
        Ability = "Tangling Hair",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Sand Attack", "Metal Claw" }
    }
},

// Meowth
{
    "Meowth",
    new PKMSVShowdownSets
    {
        Name = "Meowth",
        Gender = "M",
        Ability = "Technician",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Growl", "Fake Out" }
    }
},

// Meowth-Alola (M)
{
    "Meowth-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Meowth",
        Form = "Alola (M)",
        Ability = "Technician",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Growl", "Fake Out" }
    }
},

// Meowth-Galar (M)
{
    "Meowth-Galar (M)",
    new PKMSVShowdownSets
    {
        Name = "Meowth",
        Form = "Galar (M)",
        Ability = "Pickup",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Growl", "Fake Out" }
    }
},

// Persian
{
    "Persian",
    new PKMSVShowdownSets
    {
        Name = "Persian",
        Gender = "M",
        Ability = "Technician",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Growl", "Fake Out" }
    }
},

// Persian-Alola (M)
{
    "Persian-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Persian",
        Form = "Alola (M)",
        Ability = "Fur Coat",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Growl", "Fake Out" }
    }
},

// Psyduck
{
    "Psyduck",
    new PKMSVShowdownSets
    {
        Name = "Psyduck",
        Gender = "M",
        Ability = "Damp",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Scratch", "Tail Whip" }
    }
},

// Golduck
{
    "Golduck",
    new PKMSVShowdownSets
    {
        Name = "Golduck",
        Gender = "M",
        Ability = "Damp",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Scratch", "Tail Whip" }
    }
},

// Mankey
{
    "Mankey",
    new PKMSVShowdownSets
    {
        Name = "Mankey",
        Gender = "M",
        Ability = "Vital Spirit",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Scratch", "Leer", "Focus Energy", "Covet" }
    }
},

// Primeape
{
    "Primeape",
    new PKMSVShowdownSets
    {
        Name = "Primeape",
        Gender = "M",
        Ability = "Vital Spirit",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Scratch", "Leer", "Focus Energy", "Covet" }
    }
},

// Growlithe
{
    "Growlithe",
    new PKMSVShowdownSets
    {
        Name = "Growlithe",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Growlithe-Hisui (M)
{
    "Growlithe-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Growlithe",
        Form = "Hisui (M)",
        Ability = "Intimidate",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Arcanine
{
    "Arcanine",
    new PKMSVShowdownSets
    {
        Name = "Arcanine",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Arcanine-Hisui (M)
{
    "Arcanine-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Arcanine",
        Form = "Hisui (M)",
        Ability = "Intimidate",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Poliwag
{
    "Poliwag",
    new PKMSVShowdownSets
    {
        Name = "Poliwag",
        Gender = "M",
        Ability = "Damp",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Water Gun", "Hypnosis" }
    }
},

// Poliwhirl
{
    "Poliwhirl",
    new PKMSVShowdownSets
    {
        Name = "Poliwhirl",
        Gender = "M",
        Ability = "Damp",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Water Gun", "Hypnosis" }
    }
},

// Poliwrath
{
    "Poliwrath",
    new PKMSVShowdownSets
    {
        Name = "Poliwrath",
        Gender = "M",
        Ability = "Water Absorb",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Water Gun", "Hypnosis" }
    }
},

// Bellsprout
{
    "Bellsprout",
    new PKMSVShowdownSets
    {
        Name = "Bellsprout",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Vine Whip" }
    }
},

// Weepinbell
{
    "Weepinbell",
    new PKMSVShowdownSets
    {
        Name = "Weepinbell",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Vine Whip" }
    }
},

// Victreebel
{
    "Victreebel",
    new PKMSVShowdownSets
    {
        Name = "Victreebel",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Vine Whip" }
    }
},

// Geodude
{
    "Geodude",
    new PKMSVShowdownSets
    {
        Name = "Geodude",
        Gender = "M",
        Ability = "Sturdy",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Geodude-Alola (M)
{
    "Geodude-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Geodude",
        Form = "Alola (M)",
        Ability = "Magnet Pull",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Graveler
{
    "Graveler",
    new PKMSVShowdownSets
    {
        Name = "Graveler",
        Gender = "M",
        Ability = "Rock Head",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Graveler-Alola (M)
{
    "Graveler-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Graveler",
        Form = "Alola (M)",
        Ability = "Magnet Pull",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Golem
{
    "Golem",
    new PKMSVShowdownSets
    {
        Name = "Golem",
        Gender = "M",
        Ability = "Sturdy",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Golem-Alola (M)
{
    "Golem-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Golem",
        Form = "Alola (M)",
        Ability = "Sturdy",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Slowpoke
{
    "Slowpoke",
    new PKMSVShowdownSets
    {
        Name = "Slowpoke",
        Gender = "M",
        Ability = "Own Tempo",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Curse" }
    }
},

// Slowpoke-Galar (M)
{
    "Slowpoke-Galar (M)",
    new PKMSVShowdownSets
    {
        Name = "Slowpoke",
        Form = "Galar (M)",
        Ability = "Gluttony",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Curse" }
    }
},

// Slowbro
{
    "Slowbro",
    new PKMSVShowdownSets
    {
        Name = "Slowbro",
        Gender = "M",
        Ability = "Oblivious",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Tackle", "Curse" }
    }
},

// Slowbro-Galar (M)
{
    "Slowbro-Galar (M)",
    new PKMSVShowdownSets
    {
        Name = "Slowbro",
        Form = "Galar (M)",
        Ability = "Own Tempo",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Curse" }
    }
},

// Magnemite
{
    "Magnemite",
    new PKMSVShowdownSets
    {
        Name = "Magnemite",
        Ability = "Sturdy",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "Thunder Shock" }
    }
},

// Magneton
{
    "Magneton",
    new PKMSVShowdownSets
    {
        Name = "Magneton",
        Ability = "Sturdy",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle", "Thunder Shock" }
    }
},

// Grimer
{
    "Grimer",
    new PKMSVShowdownSets
    {
        Name = "Grimer",
        Gender = "M",
        Ability = "Stench",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Pound", "Poison Gas" }
    }
},

// Grimer-Alola (M)
{
    "Grimer-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Grimer",
        Form = "Alola (M)",
        Ability = "Poison Touch",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Pound", "Poison Gas" }
    }
},

// Muk
{
    "Muk",
    new PKMSVShowdownSets
    {
        Name = "Muk",
        Gender = "M",
        Ability = "Sticky Hold",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Pound", "Poison Gas" }
    }
},

// Muk-Alola (M)
{
    "Muk-Alola (M)",
    new PKMSVShowdownSets
    {
        Name = "Muk",
        Form = "Alola (M)",
        Ability = "Poison Touch",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Pound", "Poison Gas" }
    }
},

// Shellder
{
    "Shellder",
    new PKMSVShowdownSets
    {
        Name = "Shellder",
        Gender = "M",
        Ability = "Shell Armor",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tackle", "Water Gun" }
    }
},

// Cloyster
{
    "Cloyster",
    new PKMSVShowdownSets
    {
        Name = "Cloyster",
        Gender = "M",
        Ability = "Skill Link",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Water Gun" }
    }
},

// Gastly
{
    "Gastly",
    new PKMSVShowdownSets
    {
        Name = "Gastly",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Confuse Ray", "Lick" }
    }
},

// Haunter
{
    "Haunter",
    new PKMSVShowdownSets
    {
        Name = "Haunter",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Confuse Ray", "Lick" }
    }
},

// Gengar
{
    "Gengar",
    new PKMSVShowdownSets
    {
        Name = "Gengar",
        Gender = "M",
        Ability = "Cursed Body",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Confuse Ray", "Lick" }
    }
},

// Drowzee
{
    "Drowzee",
    new PKMSVShowdownSets
    {
        Name = "Drowzee",
        Gender = "M",
        Ability = "Forewarn",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Pound", "Hypnosis" }
    }
},

// Hypno
{
    "Hypno",
    new PKMSVShowdownSets
    {
        Name = "Hypno",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Pound", "Hypnosis" }
    }
},

// Voltorb
{
    "Voltorb",
    new PKMSVShowdownSets
    {
        Name = "Voltorb",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Charge", "Tackle" }
    }
},

// Voltorb-Hisui
{
    "Voltorb-Hisui",
    new PKMSVShowdownSets
    {
        Name = "Voltorb",
        Form = "Hisui",
        Ability = "Soundproof",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Charge", "Tackle" }
    }
},

// Electrode
{
    "Electrode",
    new PKMSVShowdownSets
    {
        Name = "Electrode",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Charge", "Tackle" }
    }
},

// Electrode-Hisui
{
    "Electrode-Hisui",
    new PKMSVShowdownSets
    {
        Name = "Electrode",
        Form = "Hisui",
        Ability = "Soundproof",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Charge", "Tackle" }
    }
},

// Koffing
{
    "Koffing",
    new PKMSVShowdownSets
    {
        Name = "Koffing",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Poison Gas" }
    }
},

// Weezing
{
    "Weezing",
    new PKMSVShowdownSets
    {
        Name = "Weezing",
        Gender = "M",
        Ability = "Neutralizing Gas",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Poison Gas" }
    }
},

// Weezing-Galar (M)
{
    "Weezing-Galar (M)",
    new PKMSVShowdownSets
    {
        Name = "Weezing",
        Form = "Galar (M)",
        Ability = "Levitate",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tackle", "Poison Gas" }
    }
},

// Chansey
{
    "Chansey",
    new PKMSVShowdownSets
    {
        Name = "Chansey",
        Gender = "F",
        Ability = "Natural Cure",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Pound", "Copycat" }
    }
},

// Scyther
{
    "Scyther",
    new PKMSVShowdownSets
    {
        Name = "Scyther",
        Gender = "M",
        Ability = "Technician",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Leer", "Quick Attack" }
    }
},

// Tauros
{
    "Tauros",
    new PKMSVShowdownSets
    {
        Name = "Tauros",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Tauros-Paldea-Combat (M)
{
    "Tauros-Paldea-Combat (M)",
    new PKMSVShowdownSets
    {
        Name = "Tauros",
        Form = "Paldea-Combat (M)",
        Ability = "Intimidate",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Tauros-Paldea-Blaze (M)
{
    "Tauros-Paldea-Blaze (M)",
    new PKMSVShowdownSets
    {
        Name = "Tauros",
        Form = "Paldea-Blaze (M)",
        Ability = "Anger Point",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Tauros-Paldea-Aqua (M)
{
    "Tauros-Paldea-Aqua (M)",
    new PKMSVShowdownSets
    {
        Name = "Tauros",
        Form = "Paldea-Aqua (M)",
        Ability = "Anger Point",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Magikarp
{
    "Magikarp",
    new PKMSVShowdownSets
    {
        Name = "Magikarp",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Splash" }
    }
},

// Gyarados
{
    "Gyarados",
    new PKMSVShowdownSets
    {
        Name = "Gyarados",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Splash" }
    }
},

// Ditto
{
    "Ditto",
    new PKMSVShowdownSets
    {
        Name = "Ditto",
        Ability = "Imposter",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Transform" }
    }
},

// Eevee
{
    "Eevee",
    new PKMSVShowdownSets
    {
        Name = "Eevee",
        Gender = "M",
        Ability = "Adaptability",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Vaporeon
{
    "Vaporeon",
    new PKMSVShowdownSets
    {
        Name = "Vaporeon",
        Gender = "M",
        Ability = "Water Absorb",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Jolteon
{
    "Jolteon",
    new PKMSVShowdownSets
    {
        Name = "Jolteon",
        Gender = "M",
        Ability = "Volt Absorb",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Flareon
{
    "Flareon",
    new PKMSVShowdownSets
    {
        Name = "Flareon",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Snorlax
{
    "Snorlax",
    new PKMSVShowdownSets
    {
        Name = "Snorlax",
        Gender = "M",
        Ability = "Thick Fat",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Lick" }
    }
},

// Dratini
{
    "Dratini",
    new PKMSVShowdownSets
    {
        Name = "Dratini",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Wrap", "Leer" }
    }
},

// Dragonair
{
    "Dragonair",
    new PKMSVShowdownSets
    {
        Name = "Dragonair",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Wrap", "Leer" }
    }
},

// Dragonite
{
    "Dragonite",
    new PKMSVShowdownSets
    {
        Name = "Dragonite",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Wrap", "Leer" }
    }
},

// Mewtwo
{
    "Mewtwo",
    new PKMSVShowdownSets
    {
        Name = "Mewtwo",
        Ability = "Unnerve",
        TeraType = "Psychic",
        Level = "95",
        Moves = new List<string> { "Aura Sphere", "Ice Beam", "Calm Mind" }
    }
},

// Mew
{
    "Mew",
    new PKMSVShowdownSets
    {
        Name = "Mew",
        Ability = "Synchronize",
        TeraType = "Bug",
        Level = "95",
        Moves = new List<string> { "Swift", "Light Screen", "Life Dew" }
    }
},

// Cyndaquil
{
    "Cyndaquil",
    new PKMSVShowdownSets
    {
        Name = "Cyndaquil",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Quilava
{
    "Quilava",
    new PKMSVShowdownSets
    {
        Name = "Quilava",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Typhlosion
{
    "Typhlosion",
    new PKMSVShowdownSets
    {
        Name = "Typhlosion",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Typhlosion-Hisui (M)
{
    "Typhlosion-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Typhlosion",
        Form = "Hisui (M)",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Sentret
{
    "Sentret",
    new PKMSVShowdownSets
    {
        Name = "Sentret",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Scratch" }
    }
},

// Furret
{
    "Furret",
    new PKMSVShowdownSets
    {
        Name = "Furret",
        Gender = "M",
        Ability = "Run Away",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Scratch" }
    }
},

// Hoothoot
{
    "Hoothoot",
    new PKMSVShowdownSets
    {
        Name = "Hoothoot",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Noctowl
{
    "Noctowl",
    new PKMSVShowdownSets
    {
        Name = "Noctowl",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Spinarak
{
    "Spinarak",
    new PKMSVShowdownSets
    {
        Name = "Spinarak",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Poison Sting", "String Shot" }
    }
},

// Ariados
{
    "Ariados",
    new PKMSVShowdownSets
    {
        Name = "Ariados",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Poison Sting", "String Shot" }
    }
},

// Pichu
{
    "Pichu",
    new PKMSVShowdownSets
    {
        Name = "Pichu",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tail Whip", "Thunder Shock" }
    }
},

// Cleffa
{
    "Cleffa",
    new PKMSVShowdownSets
    {
        Name = "Cleffa",
        Gender = "M",
        Ability = "Magic Guard",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Pound", "Splash", "Copycat" }
    }
},

// Igglybuff
{
    "Igglybuff",
    new PKMSVShowdownSets
    {
        Name = "Igglybuff",
        Gender = "M",
        Ability = "Competitive",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Pound", "Sing", "Copycat" }
    }
},

// Mareep
{
    "Mareep",
    new PKMSVShowdownSets
    {
        Name = "Mareep",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Flaaffy
{
    "Flaaffy",
    new PKMSVShowdownSets
    {
        Name = "Flaaffy",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Ampharos
{
    "Ampharos",
    new PKMSVShowdownSets
    {
        Name = "Ampharos",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Marill
{
    "Marill",
    new PKMSVShowdownSets
    {
        Name = "Marill",
        Gender = "M",
        Ability = "Huge Power",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tail Whip", "Water Gun", "Splash" }
    }
},

// Azumarill
{
    "Azumarill",
    new PKMSVShowdownSets
    {
        Name = "Azumarill",
        Gender = "M",
        Ability = "Thick Fat",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tail Whip", "Water Gun", "Splash" }
    }
},

// Sudowoodo
{
    "Sudowoodo",
    new PKMSVShowdownSets
    {
        Name = "Sudowoodo",
        Gender = "M",
        Ability = "Rock Head",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Fake Tears", "Copycat" }
    }
},

// Politoed
{
    "Politoed",
    new PKMSVShowdownSets
    {
        Name = "Politoed",
        Gender = "M",
        Ability = "Water Absorb",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Water Gun", "Hypnosis" }
    }
},

// Hoppip
{
    "Hoppip",
    new PKMSVShowdownSets
    {
        Name = "Hoppip",
        Gender = "M",
        Ability = "Leaf Guard",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Splash", "Tackle" }
    }
},

// Skiploom
{
    "Skiploom",
    new PKMSVShowdownSets
    {
        Name = "Skiploom",
        Gender = "M",
        Ability = "Leaf Guard",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Splash", "Tackle" }
    }
},

// Jumpluff
{
    "Jumpluff",
    new PKMSVShowdownSets
    {
        Name = "Jumpluff",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Splash", "Tackle" }
    }
},

// Aipom
{
    "Aipom",
    new PKMSVShowdownSets
    {
        Name = "Aipom",
        Gender = "M",
        Ability = "Run Away",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Scratch", "Tail Whip" }
    }
},

// Sunkern
{
    "Sunkern",
    new PKMSVShowdownSets
    {
        Name = "Sunkern",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Tackle", "Growth" }
    }
},

// Sunflora
{
    "Sunflora",
    new PKMSVShowdownSets
    {
        Name = "Sunflora",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tackle", "Growth" }
    }
},

// Yanma
{
    "Yanma",
    new PKMSVShowdownSets
    {
        Name = "Yanma",
        Gender = "M",
        Ability = "Speed Boost",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle" }
    }
},

// Wooper
{
    "Wooper",
    new PKMSVShowdownSets
    {
        Name = "Wooper",
        Gender = "M",
        Ability = "Damp",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tail Whip", "Water Gun" }
    }
},

// Wooper-Paldea (M)
{
    "Wooper-Paldea (M)",
    new PKMSVShowdownSets
    {
        Name = "Wooper",
        Form = "Paldea (M)",
        Ability = "Water Absorb",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tail Whip", "Mud Shot" }
    }
},

// Quagsire
{
    "Quagsire",
    new PKMSVShowdownSets
    {
        Name = "Quagsire",
        Gender = "M",
        Ability = "Water Absorb",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tail Whip", "Water Gun" }
    }
},

// Espeon
{
    "Espeon",
    new PKMSVShowdownSets
    {
        Name = "Espeon",
        Gender = "M",
        Ability = "Synchronize",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Umbreon
{
    "Umbreon",
    new PKMSVShowdownSets
    {
        Name = "Umbreon",
        Gender = "M",
        Ability = "Synchronize",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Murkrow
{
    "Murkrow",
    new PKMSVShowdownSets
    {
        Name = "Murkrow",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Peck", "Astonish" }
    }
},

// Slowking
{
    "Slowking",
    new PKMSVShowdownSets
    {
        Name = "Slowking",
        Gender = "M",
        Ability = "Oblivious",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Curse" }
    }
},

// Slowking-Galar (M)
{
    "Slowking-Galar (M)",
    new PKMSVShowdownSets
    {
        Name = "Slowking",
        Form = "Galar (M)",
        Ability = "Own Tempo",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Curse" }
    }
},

// Misdreavus
{
    "Misdreavus",
    new PKMSVShowdownSets
    {
        Name = "Misdreavus",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Growl", "Confusion" }
    }
},

// Girafarig
{
    "Girafarig",
    new PKMSVShowdownSets
    {
        Name = "Girafarig",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Growl", "Astonish", "Power Swap", "Guard Swap" }
    }
},

// Pineco
{
    "Pineco",
    new PKMSVShowdownSets
    {
        Name = "Pineco",
        Gender = "M",
        Ability = "Sturdy",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Protect" }
    }
},

// Forretress
{
    "Forretress",
    new PKMSVShowdownSets
    {
        Name = "Forretress",
        Gender = "M",
        Ability = "Sturdy",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Protect" }
    }
},

// Dunsparce
{
    "Dunsparce",
    new PKMSVShowdownSets
    {
        Name = "Dunsparce",
        Gender = "M",
        Ability = "Serene Grace",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Defense Curl", "Flail" }
    }
},

// Gligar
{
    "Gligar",
    new PKMSVShowdownSets
    {
        Name = "Gligar",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Poison Sting" }
    }
},

// Qwilfish
{
    "Qwilfish",
    new PKMSVShowdownSets
    {
        Name = "Qwilfish",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Tackle", "Poison Sting" }
    }
},

// Qwilfish-Hisui (M)
{
    "Qwilfish-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Qwilfish",
        Form = "Hisui (M)",
        Ability = "Swift Swim",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tackle", "Poison Sting" }
    }
},

// Scizor
{
    "Scizor",
    new PKMSVShowdownSets
    {
        Name = "Scizor",
        Gender = "M",
        Ability = "Technician",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Leer", "Quick Attack" }
    }
},

// Heracross
{
    "Heracross",
    new PKMSVShowdownSets
    {
        Name = "Heracross",
        Gender = "M",
        Ability = "Guts",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Leer", "Arm Thrust" }
    }
},

// Sneasel
{
    "Sneasel",
    new PKMSVShowdownSets
    {
        Name = "Sneasel",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Sneasel-Hisui (M)
{
    "Sneasel-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Sneasel",
        Form = "Hisui (M)",
        Ability = "Inner Focus",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Scratch", "Leer", "Rock Smash" }
    }
},

// Teddiursa
{
    "Teddiursa",
    new PKMSVShowdownSets
    {
        Name = "Teddiursa",
        Gender = "M",
        Ability = "Pickup",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Lick", "Covet", "Fling", "Baby-Doll Eyes" }
    }
},

// Ursaring
{
    "Ursaring",
    new PKMSVShowdownSets
    {
        Name = "Ursaring",
        Gender = "M",
        Ability = "Quick Feet",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Lick", "Covet", "Fling", "Baby-Doll Eyes" }
    }
},

// Slugma
{
    "Slugma",
    new PKMSVShowdownSets
    {
        Name = "Slugma",
        Gender = "M",
        Ability = "Magma Armor",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Smog", "Yawn" }
    }
},

// Magcargo
{
    "Magcargo",
    new PKMSVShowdownSets
    {
        Name = "Magcargo",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Smog", "Yawn" }
    }
},

// Swinub
{
    "Swinub",
    new PKMSVShowdownSets
    {
        Name = "Swinub",
        Gender = "M",
        Ability = "Oblivious",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle", "Mud-Slap" }
    }
},

// Piloswine
{
    "Piloswine",
    new PKMSVShowdownSets
    {
        Name = "Piloswine",
        Gender = "M",
        Ability = "Oblivious",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Mud-Slap" }
    }
},

// Delibird
{
    "Delibird",
    new PKMSVShowdownSets
    {
        Name = "Delibird",
        Gender = "M",
        Ability = "Vital Spirit",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Present" }
    }
},

// Houndour
{
    "Houndour",
    new PKMSVShowdownSets
    {
        Name = "Houndour",
        Gender = "M",
        Ability = "Early Bird",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Houndoom
{
    "Houndoom",
    new PKMSVShowdownSets
    {
        Name = "Houndoom",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Phanpy
{
    "Phanpy",
    new PKMSVShowdownSets
    {
        Name = "Phanpy",
        Gender = "M",
        Ability = "Pickup",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Growl", "Defense Curl" }
    }
},

// Donphan
{
    "Donphan",
    new PKMSVShowdownSets
    {
        Name = "Donphan",
        Gender = "M",
        Ability = "Sturdy",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Growl", "Defense Curl" }
    }
},

// Stantler
{
    "Stantler",
    new PKMSVShowdownSets
    {
        Name = "Stantler",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tackle" }
    }
},

// Blissey
{
    "Blissey",
    new PKMSVShowdownSets
    {
        Name = "Blissey",
        Gender = "F",
        Ability = "Natural Cure",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Pound", "Copycat" }
    }
},

// Larvitar
{
    "Larvitar",
    new PKMSVShowdownSets
    {
        Name = "Larvitar",
        Gender = "M",
        Ability = "Guts",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Pupitar
{
    "Pupitar",
    new PKMSVShowdownSets
    {
        Name = "Pupitar",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Tyranitar
{
    "Tyranitar",
    new PKMSVShowdownSets
    {
        Name = "Tyranitar",
        Gender = "M",
        Ability = "Sand Stream",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Poochyena
{
    "Poochyena",
    new PKMSVShowdownSets
    {
        Name = "Poochyena",
        Gender = "M",
        Ability = "Run Away",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle" }
    }
},

// Mightyena
{
    "Mightyena",
    new PKMSVShowdownSets
    {
        Name = "Mightyena",
        Gender = "M",
        Ability = "Quick Feet",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle" }
    }
},

// Lotad
{
    "Lotad",
    new PKMSVShowdownSets
    {
        Name = "Lotad",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Growl", "Astonish" }
    }
},

// Lombre
{
    "Lombre",
    new PKMSVShowdownSets
    {
        Name = "Lombre",
        Gender = "M",
        Ability = "Rain Dish",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Growl", "Astonish" }
    }
},

// Ludicolo
{
    "Ludicolo",
    new PKMSVShowdownSets
    {
        Name = "Ludicolo",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Growl", "Astonish" }
    }
},

// Seedot
{
    "Seedot",
    new PKMSVShowdownSets
    {
        Name = "Seedot",
        Gender = "M",
        Ability = "Early Bird",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "Harden" }
    }
},

// Nuzleaf
{
    "Nuzleaf",
    new PKMSVShowdownSets
    {
        Name = "Nuzleaf",
        Gender = "M",
        Ability = "Early Bird",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tackle", "Harden" }
    }
},

// Shiftry
{
    "Shiftry",
    new PKMSVShowdownSets
    {
        Name = "Shiftry",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Harden" }
    }
},

// Wingull
{
    "Wingull",
    new PKMSVShowdownSets
    {
        Name = "Wingull",
        Gender = "M",
        Ability = "Hydration",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Growl", "Water Gun" }
    }
},

// Pelipper
{
    "Pelipper",
    new PKMSVShowdownSets
    {
        Name = "Pelipper",
        Gender = "M",
        Ability = "Drizzle",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Growl", "Water Gun" }
    }
},

// Ralts
{
    "Ralts",
    new PKMSVShowdownSets
    {
        Name = "Ralts",
        Gender = "M",
        Ability = "Synchronize",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Growl", "Disarming Voice" }
    }
},

// Kirlia
{
    "Kirlia",
    new PKMSVShowdownSets
    {
        Name = "Kirlia",
        Gender = "M",
        Ability = "Trace",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Growl", "Disarming Voice" }
    }
},

// Gardevoir
{
    "Gardevoir",
    new PKMSVShowdownSets
    {
        Name = "Gardevoir",
        Gender = "M",
        Ability = "Synchronize",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Growl", "Disarming Voice" }
    }
},

// Surskit
{
    "Surskit",
    new PKMSVShowdownSets
    {
        Name = "Surskit",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Water Gun" }
    }
},

// Masquerain
{
    "Masquerain",
    new PKMSVShowdownSets
    {
        Name = "Masquerain",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Water Gun" }
    }
},

// Shroomish
{
    "Shroomish",
    new PKMSVShowdownSets
    {
        Name = "Shroomish",
        Gender = "M",
        Ability = "Poison Heal",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Breloom
{
    "Breloom",
    new PKMSVShowdownSets
    {
        Name = "Breloom",
        Gender = "M",
        Ability = "Poison Heal",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Slakoth
{
    "Slakoth",
    new PKMSVShowdownSets
    {
        Name = "Slakoth",
        Gender = "M",
        Ability = "Truant",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Scratch", "Yawn" }
    }
},

// Vigoroth
{
    "Vigoroth",
    new PKMSVShowdownSets
    {
        Name = "Vigoroth",
        Gender = "M",
        Ability = "Vital Spirit",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Scratch", "Yawn" }
    }
},

// Slaking
{
    "Slaking",
    new PKMSVShowdownSets
    {
        Name = "Slaking",
        Gender = "M",
        Ability = "Truant",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Scratch", "Yawn" }
    }
},

// Makuhita
{
    "Makuhita",
    new PKMSVShowdownSets
    {
        Name = "Makuhita",
        Gender = "M",
        Ability = "Thick Fat",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Focus Energy" }
    }
},

// Hariyama
{
    "Hariyama",
    new PKMSVShowdownSets
    {
        Name = "Hariyama",
        Gender = "M",
        Ability = "Guts",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "Focus Energy" }
    }
},

// Azurill
{
    "Azurill",
    new PKMSVShowdownSets
    {
        Name = "Azurill",
        Gender = "M",
        Ability = "Thick Fat",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tail Whip", "Water Gun", "Splash" }
    }
},

// Nosepass
{
    "Nosepass",
    new PKMSVShowdownSets
    {
        Name = "Nosepass",
        Gender = "M",
        Ability = "Magnet Pull",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Tackle" }
    }
},

// Sableye
{
    "Sableye",
    new PKMSVShowdownSets
    {
        Name = "Sableye",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Meditite
{
    "Meditite",
    new PKMSVShowdownSets
    {
        Name = "Meditite",
        Gender = "M",
        Ability = "Pure Power",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Confusion", "Work Up" }
    }
},

// Medicham
{
    "Medicham",
    new PKMSVShowdownSets
    {
        Name = "Medicham",
        Gender = "M",
        Ability = "Pure Power",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Confusion", "Work Up" }
    }
},

// Volbeat
{
    "Volbeat",
    new PKMSVShowdownSets
    {
        Name = "Volbeat",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle" }
    }
},

// Illumise
{
    "Illumise",
    new PKMSVShowdownSets
    {
        Name = "Illumise",
        Gender = "F",
        Ability = "Oblivious",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "Play Nice" }
    }
},

// Gulpin
{
    "Gulpin",
    new PKMSVShowdownSets
    {
        Name = "Gulpin",
        Gender = "M",
        Ability = "Liquid Ooze",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Pound" }
    }
},

// Swalot
{
    "Swalot",
    new PKMSVShowdownSets
    {
        Name = "Swalot",
        Gender = "M",
        Ability = "Liquid Ooze",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Pound" }
    }
},

// Numel
{
    "Numel",
    new PKMSVShowdownSets
    {
        Name = "Numel",
        Gender = "M",
        Ability = "Simple",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Camerupt
{
    "Camerupt",
    new PKMSVShowdownSets
    {
        Name = "Camerupt",
        Gender = "M",
        Ability = "Magma Armor",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Torkoal
{
    "Torkoal",
    new PKMSVShowdownSets
    {
        Name = "Torkoal",
        Gender = "M",
        Ability = "Drought",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Ember", "Smog" }
    }
},

// Spoink
{
    "Spoink",
    new PKMSVShowdownSets
    {
        Name = "Spoink",
        Gender = "M",
        Ability = "Thick Fat",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Splash" }
    }
},

// Grumpig
{
    "Grumpig",
    new PKMSVShowdownSets
    {
        Name = "Grumpig",
        Gender = "M",
        Ability = "Own Tempo",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Splash" }
    }
},

// Cacnea
{
    "Cacnea",
    new PKMSVShowdownSets
    {
        Name = "Cacnea",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Poison Sting", "Leer" }
    }
},

// Cacturne
{
    "Cacturne",
    new PKMSVShowdownSets
    {
        Name = "Cacturne",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Poison Sting", "Leer" }
    }
},

// Swablu
{
    "Swablu",
    new PKMSVShowdownSets
    {
        Name = "Swablu",
        Gender = "M",
        Ability = "Natural Cure",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Altaria
{
    "Altaria",
    new PKMSVShowdownSets
    {
        Name = "Altaria",
        Gender = "M",
        Ability = "Natural Cure",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Zangoose
{
    "Zangoose",
    new PKMSVShowdownSets
    {
        Name = "Zangoose",
        Gender = "M",
        Ability = "Immunity",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Seviper
{
    "Seviper",
    new PKMSVShowdownSets
    {
        Name = "Seviper",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Wrap", "Swagger" }
    }
},

// Barboach
{
    "Barboach",
    new PKMSVShowdownSets
    {
        Name = "Barboach",
        Gender = "M",
        Ability = "Anticipation",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Water Gun", "Mud-Slap" }
    }
},

// Whiscash
{
    "Whiscash",
    new PKMSVShowdownSets
    {
        Name = "Whiscash",
        Gender = "M",
        Ability = "Anticipation",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Water Gun", "Mud-Slap" }
    }
},

// Corphish
{
    "Corphish",
    new PKMSVShowdownSets
    {
        Name = "Corphish",
        Gender = "M",
        Ability = "Hyper Cutter",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Water Gun", "Harden" }
    }
},

// Crawdaunt
{
    "Crawdaunt",
    new PKMSVShowdownSets
    {
        Name = "Crawdaunt",
        Gender = "M",
        Ability = "Hyper Cutter",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Water Gun", "Harden" }
    }
},

// Feebas
{
    "Feebas",
    new PKMSVShowdownSets
    {
        Name = "Feebas",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Splash" }
    }
},

// Milotic
{
    "Milotic",
    new PKMSVShowdownSets
    {
        Name = "Milotic",
        Gender = "M",
        Ability = "Marvel Scale",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Splash" }
    }
},

// Shuppet
{
    "Shuppet",
    new PKMSVShowdownSets
    {
        Name = "Shuppet",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Astonish" }
    }
},

// Banette
{
    "Banette",
    new PKMSVShowdownSets
    {
        Name = "Banette",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Astonish" }
    }
},

// Duskull
{
    "Duskull",
    new PKMSVShowdownSets
    {
        Name = "Duskull",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Leer", "Astonish" }
    }
},

// Dusclops
{
    "Dusclops",
    new PKMSVShowdownSets
    {
        Name = "Dusclops",
        Gender = "M",
        Ability = "Pressure",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Leer", "Astonish" }
    }
},

// Tropius
{
    "Tropius",
    new PKMSVShowdownSets
    {
        Name = "Tropius",
        Gender = "M",
        Ability = "Solar Power",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Leer", "Growth", "Razor Leaf", "Leaf Storm" }
    }
},

// Chimecho
{
    "Chimecho",
    new PKMSVShowdownSets
    {
        Name = "Chimecho",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Wrap" }
    }
},

// Snorunt
{
    "Snorunt",
    new PKMSVShowdownSets
    {
        Name = "Snorunt",
        Gender = "M",
        Ability = "Ice Body",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Headbutt", "Powder Snow", "Astonish" }
    }
},

// Glalie
{
    "Glalie",
    new PKMSVShowdownSets
    {
        Name = "Glalie",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Headbutt", "Powder Snow", "Astonish" }
    }
},

// Luvdisc
{
    "Luvdisc",
    new PKMSVShowdownSets
    {
        Name = "Luvdisc",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tackle", "Charm" }
    }
},

// Bagon
{
    "Bagon",
    new PKMSVShowdownSets
    {
        Name = "Bagon",
        Gender = "M",
        Ability = "Rock Head",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Shelgon
{
    "Shelgon",
    new PKMSVShowdownSets
    {
        Name = "Shelgon",
        Gender = "M",
        Ability = "Rock Head",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Salamence
{
    "Salamence",
    new PKMSVShowdownSets
    {
        Name = "Salamence",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Leer", "Ember" }
    }
},

// Turtwig
{
    "Turtwig",
    new PKMSVShowdownSets
    {
        Name = "Turtwig",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle" }
    }
},

// Grotle
{
    "Grotle",
    new PKMSVShowdownSets
    {
        Name = "Grotle",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Tackle" }
    }
},

// Torterra
{
    "Torterra",
    new PKMSVShowdownSets
    {
        Name = "Torterra",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle" }
    }
},

// Chimchar
{
    "Chimchar",
    new PKMSVShowdownSets
    {
        Name = "Chimchar",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Monferno
{
    "Monferno",
    new PKMSVShowdownSets
    {
        Name = "Monferno",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Infernape
{
    "Infernape",
    new PKMSVShowdownSets
    {
        Name = "Infernape",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Piplup
{
    "Piplup",
    new PKMSVShowdownSets
    {
        Name = "Piplup",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Pound" }
    }
},

// Prinplup
{
    "Prinplup",
    new PKMSVShowdownSets
    {
        Name = "Prinplup",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Pound" }
    }
},

// Empoleon
{
    "Empoleon",
    new PKMSVShowdownSets
    {
        Name = "Empoleon",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Pound" }
    }
},

// Starly
{
    "Starly",
    new PKMSVShowdownSets
    {
        Name = "Starly",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Staravia
{
    "Staravia",
    new PKMSVShowdownSets
    {
        Name = "Staravia",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Staraptor
{
    "Staraptor",
    new PKMSVShowdownSets
    {
        Name = "Staraptor",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Kricketot
{
    "Kricketot",
    new PKMSVShowdownSets
    {
        Name = "Kricketot",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Kricketune
{
    "Kricketune",
    new PKMSVShowdownSets
    {
        Name = "Kricketune",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Shinx
{
    "Shinx",
    new PKMSVShowdownSets
    {
        Name = "Shinx",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Luxio
{
    "Luxio",
    new PKMSVShowdownSets
    {
        Name = "Luxio",
        Gender = "M",
        Ability = "Rivalry",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Luxray
{
    "Luxray",
    new PKMSVShowdownSets
    {
        Name = "Luxray",
        Gender = "M",
        Ability = "Rivalry",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Combee
{
    "Combee",
    new PKMSVShowdownSets
    {
        Name = "Combee",
        Gender = "M",
        Ability = "Honey Gather",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Gust", "Sweet Scent", "Bug Bite", "Struggle Bug" }
    }
},

// Vespiquen
{
    "Vespiquen",
    new PKMSVShowdownSets
    {
        Name = "Vespiquen",
        Gender = "F",
        Ability = "Pressure",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Gust", "Sweet Scent", "Bug Bite", "Struggle Bug" }
    }
},

// Pachirisu
{
    "Pachirisu",
    new PKMSVShowdownSets
    {
        Name = "Pachirisu",
        Gender = "M",
        Ability = "Run Away",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Growl", "Thunder Shock" }
    }
},

// Buizel
{
    "Buizel",
    new PKMSVShowdownSets
    {
        Name = "Buizel",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tackle" }
    }
},

// Floatzel
{
    "Floatzel",
    new PKMSVShowdownSets
    {
        Name = "Floatzel",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Tackle" }
    }
},

// Shellos
{
    "Shellos",
    new PKMSVShowdownSets
    {
        Name = "Shellos",
        Gender = "M",
        Ability = "Storm Drain",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Water Gun", "Mud-Slap" }
    }
},

// Shellos-East (M)
{
    "Shellos-East (M)",
    new PKMSVShowdownSets
    {
        Name = "Shellos",
        Form = "East (M)",
        Ability = "Sticky Hold",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Water Gun", "Mud-Slap" }
    }
},

// Gastrodon
{
    "Gastrodon",
    new PKMSVShowdownSets
    {
        Name = "Gastrodon",
        Gender = "M",
        Ability = "Storm Drain",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Water Gun", "Mud-Slap" }
    }
},

// Gastrodon-East (M)
{
    "Gastrodon-East (M)",
    new PKMSVShowdownSets
    {
        Name = "Gastrodon",
        Form = "East (M)",
        Ability = "Storm Drain",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Water Gun", "Mud-Slap" }
    }
},

// Ambipom
{
    "Ambipom",
    new PKMSVShowdownSets
    {
        Name = "Ambipom",
        Gender = "M",
        Ability = "Pickup",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Scratch", "Tail Whip" }
    }
},

// Drifloon
{
    "Drifloon",
    new PKMSVShowdownSets
    {
        Name = "Drifloon",
        Gender = "M",
        Ability = "Unburden",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Minimize", "Astonish" }
    }
},

// Drifblim
{
    "Drifblim",
    new PKMSVShowdownSets
    {
        Name = "Drifblim",
        Gender = "M",
        Ability = "Aftermath",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Minimize", "Astonish" }
    }
},

// Mismagius
{
    "Mismagius",
    new PKMSVShowdownSets
    {
        Name = "Mismagius",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Growl", "Confusion" }
    }
},

// Honchkrow
{
    "Honchkrow",
    new PKMSVShowdownSets
    {
        Name = "Honchkrow",
        Gender = "M",
        Ability = "Super Luck",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Peck", "Astonish" }
    }
},

// Chingling
{
    "Chingling",
    new PKMSVShowdownSets
    {
        Name = "Chingling",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Wrap" }
    }
},

// Stunky
{
    "Stunky",
    new PKMSVShowdownSets
    {
        Name = "Stunky",
        Gender = "M",
        Ability = "Stench",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Scratch", "Poison Gas" }
    }
},

// Skuntank
{
    "Skuntank",
    new PKMSVShowdownSets
    {
        Name = "Skuntank",
        Gender = "M",
        Ability = "Aftermath",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Scratch", "Poison Gas" }
    }
},

// Bronzor
{
    "Bronzor",
    new PKMSVShowdownSets
    {
        Name = "Bronzor",
        Ability = "Levitate",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "Confusion" }
    }
},

// Bronzong
{
    "Bronzong",
    new PKMSVShowdownSets
    {
        Name = "Bronzong",
        Ability = "Levitate",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Confusion" }
    }
},

// Bonsly
{
    "Bonsly",
    new PKMSVShowdownSets
    {
        Name = "Bonsly",
        Gender = "M",
        Ability = "Rock Head",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Fake Tears", "Copycat" }
    }
},

// Happiny
{
    "Happiny",
    new PKMSVShowdownSets
    {
        Name = "Happiny",
        Gender = "F",
        Ability = "Natural Cure",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Pound", "Copycat" }
    }
},

// Spiritomb
{
    "Spiritomb",
    new PKMSVShowdownSets
    {
        Name = "Spiritomb",
        Gender = "M",
        Ability = "Pressure",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Night Shade", "Confuse Ray" }
    }
},

// Gible
{
    "Gible",
    new PKMSVShowdownSets
    {
        Name = "Gible",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "Sand Tomb" }
    }
},

// Gabite
{
    "Gabite",
    new PKMSVShowdownSets
    {
        Name = "Gabite",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Sand Tomb" }
    }
},

// Garchomp
{
    "Garchomp",
    new PKMSVShowdownSets
    {
        Name = "Garchomp",
        Gender = "M",
        Ability = "Sand Veil",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tackle", "Sand Tomb" }
    }
},

// Munchlax
{
    "Munchlax",
    new PKMSVShowdownSets
    {
        Name = "Munchlax",
        Gender = "M",
        Ability = "Pickup",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Lick" }
    }
},

// Riolu
{
    "Riolu",
    new PKMSVShowdownSets
    {
        Name = "Riolu",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Quick Attack", "Endure" }
    }
},

// Lucario
{
    "Lucario",
    new PKMSVShowdownSets
    {
        Name = "Lucario",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Quick Attack", "Endure" }
    }
},

// Hippopotas
{
    "Hippopotas",
    new PKMSVShowdownSets
    {
        Name = "Hippopotas",
        Gender = "M",
        Ability = "Sand Stream",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Sand Attack", "Tackle" }
    }
},

// Hippowdon
{
    "Hippowdon",
    new PKMSVShowdownSets
    {
        Name = "Hippowdon",
        Gender = "M",
        Ability = "Sand Stream",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Sand Attack", "Tackle" }
    }
},

// Croagunk
{
    "Croagunk",
    new PKMSVShowdownSets
    {
        Name = "Croagunk",
        Gender = "M",
        Ability = "Dry Skin",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Poison Sting", "Mud-Slap" }
    }
},

// Toxicroak
{
    "Toxicroak",
    new PKMSVShowdownSets
    {
        Name = "Toxicroak",
        Gender = "M",
        Ability = "Dry Skin",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Poison Sting", "Mud-Slap" }
    }
},

// Finneon
{
    "Finneon",
    new PKMSVShowdownSets
    {
        Name = "Finneon",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Pound" }
    }
},

// Lumineon
{
    "Lumineon",
    new PKMSVShowdownSets
    {
        Name = "Lumineon",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Pound" }
    }
},

// Snover
{
    "Snover",
    new PKMSVShowdownSets
    {
        Name = "Snover",
        Gender = "M",
        Ability = "Snow Warning",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Leer", "Powder Snow" }
    }
},

// Abomasnow
{
    "Abomasnow",
    new PKMSVShowdownSets
    {
        Name = "Abomasnow",
        Gender = "M",
        Ability = "Snow Warning",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Leer", "Powder Snow" }
    }
},

// Weavile
{
    "Weavile",
    new PKMSVShowdownSets
    {
        Name = "Weavile",
        Gender = "M",
        Ability = "Pressure",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Magnezone
{
    "Magnezone",
    new PKMSVShowdownSets
    {
        Name = "Magnezone",
        Ability = "Sturdy",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle", "Thunder Shock" }
    }
},

// Yanmega
{
    "Yanmega",
    new PKMSVShowdownSets
    {
        Name = "Yanmega",
        Gender = "M",
        Ability = "Speed Boost",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle" }
    }
},

// Leafeon
{
    "Leafeon",
    new PKMSVShowdownSets
    {
        Name = "Leafeon",
        Gender = "M",
        Ability = "Leaf Guard",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Glaceon
{
    "Glaceon",
    new PKMSVShowdownSets
    {
        Name = "Glaceon",
        Gender = "M",
        Ability = "Snow Cloak",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Gliscor
{
    "Gliscor",
    new PKMSVShowdownSets
    {
        Name = "Gliscor",
        Gender = "M",
        Ability = "Hyper Cutter",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Poison Sting" }
    }
},

// Mamoswine
{
    "Mamoswine",
    new PKMSVShowdownSets
    {
        Name = "Mamoswine",
        Gender = "M",
        Ability = "Oblivious",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Mud-Slap" }
    }
},

// Gallade
{
    "Gallade",
    new PKMSVShowdownSets
    {
        Name = "Gallade",
        Gender = "M",
        Ability = "Sharpness",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Growl", "Disarming Voice" }
    }
},

// Probopass
{
    "Probopass",
    new PKMSVShowdownSets
    {
        Name = "Probopass",
        Gender = "M",
        Ability = "Magnet Pull",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Tackle" }
    }
},

// Dusknoir
{
    "Dusknoir",
    new PKMSVShowdownSets
    {
        Name = "Dusknoir",
        Gender = "M",
        Ability = "Pressure",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Leer", "Astonish" }
    }
},

// Froslass
{
    "Froslass",
    new PKMSVShowdownSets
    {
        Name = "Froslass",
        Gender = "F",
        Ability = "Snow Cloak",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Headbutt", "Powder Snow", "Astonish" }
    }
},

// Rotom
{
    "Rotom",
    new PKMSVShowdownSets
    {
        Name = "Rotom",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Double Team", "Astonish" }
    }
},

// Rotom-Heat
{
    "Rotom-Heat",
    new PKMSVShowdownSets
    {
        Name = "Rotom",
        Form = "Heat",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Double Team", "Astonish" }
    }
},

// Rotom-Wash
{
    "Rotom-Wash",
    new PKMSVShowdownSets
    {
        Name = "Rotom",
        Form = "Wash",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Double Team", "Astonish" }
    }
},

// Rotom-Frost
{
    "Rotom-Frost",
    new PKMSVShowdownSets
    {
        Name = "Rotom",
        Form = "Frost",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Double Team", "Astonish" }
    }
},

// Rotom-Fan
{
    "Rotom-Fan",
    new PKMSVShowdownSets
    {
        Name = "Rotom",
        Form = "Fan",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Double Team", "Astonish" }
    }
},

// Rotom-Mow
{
    "Rotom-Mow",
    new PKMSVShowdownSets
    {
        Name = "Rotom",
        Form = "Mow",
        Ability = "Levitate",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Double Team", "Astonish" }
    }
},

// Phione
{
    "Phione",
    new PKMSVShowdownSets
    {
        Name = "Phione",
        Ability = "Hydration",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Water Gun" }
    }
},

// Oshawott
{
    "Oshawott",
    new PKMSVShowdownSets
    {
        Name = "Oshawott",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle" }
    }
},

// Dewott
{
    "Dewott",
    new PKMSVShowdownSets
    {
        Name = "Dewott",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle" }
    }
},

// Samurott
{
    "Samurott",
    new PKMSVShowdownSets
    {
        Name = "Samurott",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tackle" }
    }
},

// Samurott-Hisui (M)
{
    "Samurott-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Samurott",
        Form = "Hisui (M)",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Tackle" }
    }
},

// Timburr
{
    "Timburr",
    new PKMSVShowdownSets
    {
        Name = "Timburr",
        Gender = "M",
        Ability = "Guts",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Pound", "Leer" }
    }
},

// Gurdurr
{
    "Gurdurr",
    new PKMSVShowdownSets
    {
        Name = "Gurdurr",
        Gender = "M",
        Ability = "Guts",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Pound", "Leer" }
    }
},

// Conkeldurr
{
    "Conkeldurr",
    new PKMSVShowdownSets
    {
        Name = "Conkeldurr",
        Gender = "M",
        Ability = "Guts",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Pound", "Leer" }
    }
},

// Sewaddle
{
    "Sewaddle",
    new PKMSVShowdownSets
    {
        Name = "Sewaddle",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Swadloon
{
    "Swadloon",
    new PKMSVShowdownSets
    {
        Name = "Swadloon",
        Gender = "M",
        Ability = "Leaf Guard",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Leavanny
{
    "Leavanny",
    new PKMSVShowdownSets
    {
        Name = "Leavanny",
        Gender = "M",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Petilil
{
    "Petilil",
    new PKMSVShowdownSets
    {
        Name = "Petilil",
        Gender = "F",
        Ability = "Own Tempo",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Absorb", "Growth" }
    }
},

// Lilligant
{
    "Lilligant",
    new PKMSVShowdownSets
    {
        Name = "Lilligant",
        Gender = "F",
        Ability = "Own Tempo",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Absorb", "Growth" }
    }
},

// Lilligant-Hisui (F)
{
    "Lilligant-Hisui (F)",
    new PKMSVShowdownSets
    {
        Name = "Lilligant",
        Form = "Hisui (F)",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Absorb", "Growth" }
    }
},

// Basculin
{
    "Basculin",
    new PKMSVShowdownSets
    {
        Name = "Basculin",
        Gender = "M",
        Ability = "Adaptability",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Tail Whip", "Water Gun" }
    }
},

// Basculin-Blue-Striped (M)
{
    "Basculin-Blue-Striped (M)",
    new PKMSVShowdownSets
    {
        Name = "Basculin",
        Form = "Blue-Striped (M)",
        Ability = "Adaptability",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tail Whip", "Water Gun" }
    }
},

// Basculin-White (M)
{
    "Basculin-White (M)",
    new PKMSVShowdownSets
    {
        Name = "Basculin",
        Form = "White (M)",
        Ability = "Adaptability",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tail Whip", "Water Gun" }
    }
},

// Sandile
{
    "Sandile",
    new PKMSVShowdownSets
    {
        Name = "Sandile",
        Gender = "M",
        Ability = "Moxie",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Leer", "Power Trip" }
    }
},

// Krokorok
{
    "Krokorok",
    new PKMSVShowdownSets
    {
        Name = "Krokorok",
        Gender = "M",
        Ability = "Moxie",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Leer", "Power Trip" }
    }
},

// Krookodile
{
    "Krookodile",
    new PKMSVShowdownSets
    {
        Name = "Krookodile",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Leer", "Power Trip" }
    }
},

// Zorua
{
    "Zorua",
    new PKMSVShowdownSets
    {
        Name = "Zorua",
        Gender = "M",
        Ability = "Illusion",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Zorua-Hisui (M)
{
    "Zorua-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Zorua",
        Form = "Hisui (M)",
        Ability = "Illusion",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Zoroark
{
    "Zoroark",
    new PKMSVShowdownSets
    {
        Name = "Zoroark",
        Gender = "M",
        Ability = "Illusion",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Zoroark-Hisui (M)
{
    "Zoroark-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Zoroark",
        Form = "Hisui (M)",
        Ability = "Illusion",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Gothita
{
    "Gothita",
    new PKMSVShowdownSets
    {
        Name = "Gothita",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Pound", "Confusion" }
    }
},

// Gothorita
{
    "Gothorita",
    new PKMSVShowdownSets
    {
        Name = "Gothorita",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Pound", "Confusion" }
    }
},

// Gothitelle
{
    "Gothitelle",
    new PKMSVShowdownSets
    {
        Name = "Gothitelle",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Pound", "Confusion" }
    }
},

// Ducklett
{
    "Ducklett",
    new PKMSVShowdownSets
    {
        Name = "Ducklett",
        Gender = "M",
        Ability = "Big Pecks",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Water Gun" }
    }
},

// Swanna
{
    "Swanna",
    new PKMSVShowdownSets
    {
        Name = "Swanna",
        Gender = "M",
        Ability = "Big Pecks",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Water Gun" }
    }
},

// Deerling
{
    "Deerling",
    new PKMSVShowdownSets
    {
        Name = "Deerling",
        Gender = "M",
        Ability = "Sap Sipper",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle" }
    }
},

// Deerling-Summer (M)
{
    "Deerling-Summer (M)",
    new PKMSVShowdownSets
    {
        Name = "Deerling",
        Form = "Summer (M)",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle" }
    }
},

// Deerling-Autumn (M)
{
    "Deerling-Autumn (M)",
    new PKMSVShowdownSets
    {
        Name = "Deerling",
        Form = "Autumn (M)",
        Ability = "Sap Sipper",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tackle" }
    }
},

// Deerling-Winter (M)
{
    "Deerling-Winter (M)",
    new PKMSVShowdownSets
    {
        Name = "Deerling",
        Form = "Winter (M)",
        Ability = "Sap Sipper",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle" }
    }
},

// Sawsbuck
{
    "Sawsbuck",
    new PKMSVShowdownSets
    {
        Name = "Sawsbuck",
        Gender = "M",
        Ability = "Sap Sipper",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle" }
    }
},

// Sawsbuck-Summer (M)
{
    "Sawsbuck-Summer (M)",
    new PKMSVShowdownSets
    {
        Name = "Sawsbuck",
        Form = "Summer (M)",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Tackle" }
    }
},

// Sawsbuck-Autumn (M)
{
    "Sawsbuck-Autumn (M)",
    new PKMSVShowdownSets
    {
        Name = "Sawsbuck",
        Form = "Autumn (M)",
        Ability = "Sap Sipper",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Tackle" }
    }
},

// Sawsbuck-Winter (M)
{
    "Sawsbuck-Winter (M)",
    new PKMSVShowdownSets
    {
        Name = "Sawsbuck",
        Form = "Winter (M)",
        Ability = "Chlorophyll",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle" }
    }
},

// Foongus
{
    "Foongus",
    new PKMSVShowdownSets
    {
        Name = "Foongus",
        Gender = "M",
        Ability = "Effect Spore",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Absorb", "Astonish" }
    }
},

// Amoonguss
{
    "Amoonguss",
    new PKMSVShowdownSets
    {
        Name = "Amoonguss",
        Gender = "M",
        Ability = "Effect Spore",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Absorb", "Astonish" }
    }
},

// Alomomola
{
    "Alomomola",
    new PKMSVShowdownSets
    {
        Name = "Alomomola",
        Gender = "M",
        Ability = "Healer",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Pound", "Play Nice" }
    }
},

// Tynamo
{
    "Tynamo",
    new PKMSVShowdownSets
    {
        Name = "Tynamo",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Tackle", "Thunder Wave", "Spark", "Charge Beam" }
    }
},

// Eelektrik
{
    "Eelektrik",
    new PKMSVShowdownSets
    {
        Name = "Eelektrik",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Thunder Wave", "Spark", "Charge Beam" }
    }
},

// Eelektross
{
    "Eelektross",
    new PKMSVShowdownSets
    {
        Name = "Eelektross",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "Thunder Wave", "Spark", "Charge Beam" }
    }
},

// Litwick
{
    "Litwick",
    new PKMSVShowdownSets
    {
        Name = "Litwick",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Smog", "Astonish" }
    }
},

// Lampent
{
    "Lampent",
    new PKMSVShowdownSets
    {
        Name = "Lampent",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Smog", "Astonish" }
    }
},

// Chandelure
{
    "Chandelure",
    new PKMSVShowdownSets
    {
        Name = "Chandelure",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Smog", "Astonish" }
    }
},

// Axew
{
    "Axew",
    new PKMSVShowdownSets
    {
        Name = "Axew",
        Gender = "M",
        Ability = "Mold Breaker",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Fraxure
{
    "Fraxure",
    new PKMSVShowdownSets
    {
        Name = "Fraxure",
        Gender = "M",
        Ability = "Mold Breaker",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Haxorus
{
    "Haxorus",
    new PKMSVShowdownSets
    {
        Name = "Haxorus",
        Gender = "M",
        Ability = "Rivalry",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Cubchoo
{
    "Cubchoo",
    new PKMSVShowdownSets
    {
        Name = "Cubchoo",
        Gender = "M",
        Ability = "Snow Cloak",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Growl", "Powder Snow" }
    }
},

// Beartic
{
    "Beartic",
    new PKMSVShowdownSets
    {
        Name = "Beartic",
        Gender = "M",
        Ability = "Snow Cloak",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Growl", "Powder Snow" }
    }
},

// Cryogonal
{
    "Cryogonal",
    new PKMSVShowdownSets
    {
        Name = "Cryogonal",
        Ability = "Levitate",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Bind", "Ice Shard" }
    }
},

// Mienfoo
{
    "Mienfoo",
    new PKMSVShowdownSets
    {
        Name = "Mienfoo",
        Gender = "M",
        Ability = "Regenerator",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Pound", "Detect" }
    }
},

// Mienshao
{
    "Mienshao",
    new PKMSVShowdownSets
    {
        Name = "Mienshao",
        Gender = "M",
        Ability = "Regenerator",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Pound", "Detect" }
    }
},

// Pawniard
{
    "Pawniard",
    new PKMSVShowdownSets
    {
        Name = "Pawniard",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Bisharp
{
    "Bisharp",
    new PKMSVShowdownSets
    {
        Name = "Bisharp",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Rufflet
{
    "Rufflet",
    new PKMSVShowdownSets
    {
        Name = "Rufflet",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Leer", "Peck" }
    }
},

// Braviary
{
    "Braviary",
    new PKMSVShowdownSets
    {
        Name = "Braviary",
        Gender = "M",
        Ability = "Sheer Force",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Leer", "Peck" }
    }
},

// Braviary-Hisui (M)
{
    "Braviary-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Braviary",
        Form = "Hisui (M)",
        Ability = "Sheer Force",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Leer", "Peck" }
    }
},

// Vullaby
{
    "Vullaby",
    new PKMSVShowdownSets
    {
        Name = "Vullaby",
        Gender = "F",
        Ability = "Overcoat",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Gust", "Leer" }
    }
},

// Mandibuzz
{
    "Mandibuzz",
    new PKMSVShowdownSets
    {
        Name = "Mandibuzz",
        Gender = "F",
        Ability = "Overcoat",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Gust", "Leer" }
    }
},

// Deino
{
    "Deino",
    new PKMSVShowdownSets
    {
        Name = "Deino",
        Gender = "M",
        Ability = "Hustle",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Focus Energy" }
    }
},

// Zweilous
{
    "Zweilous",
    new PKMSVShowdownSets
    {
        Name = "Zweilous",
        Gender = "M",
        Ability = "Hustle",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "Focus Energy" }
    }
},

// Hydreigon
{
    "Hydreigon",
    new PKMSVShowdownSets
    {
        Name = "Hydreigon",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Tackle", "Focus Energy" }
    }
},

// Larvesta
{
    "Larvesta",
    new PKMSVShowdownSets
    {
        Name = "Larvesta",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Ember", "String Shot" }
    }
},

// Volcarona
{
    "Volcarona",
    new PKMSVShowdownSets
    {
        Name = "Volcarona",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Ember", "String Shot" }
    }
},

// Chespin
{
    "Chespin",
    new PKMSVShowdownSets
    {
        Name = "Chespin",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Vine Whip", "Growl" }
    }
},

// Quilladin
{
    "Quilladin",
    new PKMSVShowdownSets
    {
        Name = "Quilladin",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Vine Whip", "Growl" }
    }
},

// Chesnaught
{
    "Chesnaught",
    new PKMSVShowdownSets
    {
        Name = "Chesnaught",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Vine Whip", "Growl" }
    }
},

// Fennekin
{
    "Fennekin",
    new PKMSVShowdownSets
    {
        Name = "Fennekin",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Scratch", "Tail Whip" }
    }
},

// Braixen
{
    "Braixen",
    new PKMSVShowdownSets
    {
        Name = "Braixen",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Scratch", "Tail Whip" }
    }
},

// Delphox
{
    "Delphox",
    new PKMSVShowdownSets
    {
        Name = "Delphox",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Scratch", "Tail Whip" }
    }
},

// Froakie
{
    "Froakie",
    new PKMSVShowdownSets
    {
        Name = "Froakie",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Pound", "Growl" }
    }
},

// Frogadier
{
    "Frogadier",
    new PKMSVShowdownSets
    {
        Name = "Frogadier",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Pound", "Growl" }
    }
},

// Greninja
{
    "Greninja",
    new PKMSVShowdownSets
    {
        Name = "Greninja",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Pound", "Growl" }
    }
},

// Fletchling
{
    "Fletchling",
    new PKMSVShowdownSets
    {
        Name = "Fletchling",
        Gender = "M",
        Ability = "Big Pecks",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Fletchinder
{
    "Fletchinder",
    new PKMSVShowdownSets
    {
        Name = "Fletchinder",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Talonflame
{
    "Talonflame",
    new PKMSVShowdownSets
    {
        Name = "Talonflame",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Scatterbug-Fancy (M)
{
    "Scatterbug-Fancy (M)",
    new PKMSVShowdownSets
    {
        Name = "Scatterbug",
        Form = "Fancy (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Spewpa-Fancy (M)
{
    "Spewpa-Fancy (M)",
    new PKMSVShowdownSets
    {
        Name = "Spewpa",
        Form = "Fancy (M)",
        Ability = "Shed Skin",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon
{
    "Vivillon",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Gender = "M",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Polar (M)
{
    "Vivillon-Polar (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Polar (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Tundra (M)
{
    "Vivillon-Tundra (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Tundra (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Continental (M)
{
    "Vivillon-Continental (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Continental (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Garden (M)
{
    "Vivillon-Garden (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Garden (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Elegant (M)
{
    "Vivillon-Elegant (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Elegant (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Meadow (M)
{
    "Vivillon-Meadow (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Meadow (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Modern (M)
{
    "Vivillon-Modern (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Modern (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Marine (M)
{
    "Vivillon-Marine (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Marine (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Archipelago (M)
{
    "Vivillon-Archipelago (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Archipelago (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-High-Plains (M)
{
    "Vivillon-High-Plains (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "High-Plains (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Sandstorm (M)
{
    "Vivillon-Sandstorm (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Sandstorm (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-River (M)
{
    "Vivillon-River (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "River (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Monsoon (M)
{
    "Vivillon-Monsoon (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Monsoon (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Savanna (M)
{
    "Vivillon-Savanna (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Savanna (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Sun (M)
{
    "Vivillon-Sun (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Sun (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Ocean (M)
{
    "Vivillon-Ocean (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Ocean (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Jungle (M)
{
    "Vivillon-Jungle (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Jungle (M)",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Vivillon-Fancy (M)
{
    "Vivillon-Fancy (M)",
    new PKMSVShowdownSets
    {
        Name = "Vivillon",
        Form = "Fancy (M)",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Litleo
{
    "Litleo",
    new PKMSVShowdownSets
    {
        Name = "Litleo",
        Gender = "M",
        Ability = "Unnerve",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Pyroar
{
    "Pyroar",
    new PKMSVShowdownSets
    {
        Name = "Pyroar",
        Gender = "M",
        Ability = "Unnerve",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Flabébé
{
    "Flabébé",
    new PKMSVShowdownSets
    {
        Name = "Flabébé",
        Gender = "F",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Flabébé-Yellow (F)
{
    "Flabébé-Yellow (F)",
    new PKMSVShowdownSets
    {
        Name = "Flabébé",
        Form = "Yellow (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Flabébé-Orange (F)
{
    "Flabébé-Orange (F)",
    new PKMSVShowdownSets
    {
        Name = "Flabébé",
        Form = "Orange (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Flabébé-Blue (F)
{
    "Flabébé-Blue (F)",
    new PKMSVShowdownSets
    {
        Name = "Flabébé",
        Form = "Blue (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Flabébé-White (F)
{
    "Flabébé-White (F)",
    new PKMSVShowdownSets
    {
        Name = "Flabébé",
        Form = "White (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Floette
{
    "Floette",
    new PKMSVShowdownSets
    {
        Name = "Floette",
        Gender = "F",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Floette-Yellow (F)
{
    "Floette-Yellow (F)",
    new PKMSVShowdownSets
    {
        Name = "Floette",
        Form = "Yellow (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Floette-Orange (F)
{
    "Floette-Orange (F)",
    new PKMSVShowdownSets
    {
        Name = "Floette",
        Form = "Orange (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Floette-Blue (F)
{
    "Floette-Blue (F)",
    new PKMSVShowdownSets
    {
        Name = "Floette",
        Form = "Blue (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Floette-White (F)
{
    "Floette-White (F)",
    new PKMSVShowdownSets
    {
        Name = "Floette",
        Form = "White (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Florges
{
    "Florges",
    new PKMSVShowdownSets
    {
        Name = "Florges",
        Gender = "F",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Florges-Yellow (F)
{
    "Florges-Yellow (F)",
    new PKMSVShowdownSets
    {
        Name = "Florges",
        Form = "Yellow (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Florges-Orange (F)
{
    "Florges-Orange (F)",
    new PKMSVShowdownSets
    {
        Name = "Florges",
        Form = "Orange (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Florges-Blue (F)
{
    "Florges-Blue (F)",
    new PKMSVShowdownSets
    {
        Name = "Florges",
        Form = "Blue (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Florges-White (F)
{
    "Florges-White (F)",
    new PKMSVShowdownSets
    {
        Name = "Florges",
        Form = "White (F)",
        Ability = "Flower Veil",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Vine Whip", "Tackle" }
    }
},

// Skiddo
{
    "Skiddo",
    new PKMSVShowdownSets
    {
        Name = "Skiddo",
        Gender = "M",
        Ability = "Sap Sipper",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Tackle", "Growth" }
    }
},

// Gogoat
{
    "Gogoat",
    new PKMSVShowdownSets
    {
        Name = "Gogoat",
        Gender = "M",
        Ability = "Sap Sipper",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "Growth" }
    }
},

// Skrelp
{
    "Skrelp",
    new PKMSVShowdownSets
    {
        Name = "Skrelp",
        Gender = "M",
        Ability = "Poison Touch",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "Smokescreen" }
    }
},

// Dragalge
{
    "Dragalge",
    new PKMSVShowdownSets
    {
        Name = "Dragalge",
        Gender = "M",
        Ability = "Poison Point",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle", "Smokescreen" }
    }
},

// Clauncher
{
    "Clauncher",
    new PKMSVShowdownSets
    {
        Name = "Clauncher",
        Gender = "M",
        Ability = "Mega Launcher",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Water Gun", "Splash" }
    }
},

// Clawitzer
{
    "Clawitzer",
    new PKMSVShowdownSets
    {
        Name = "Clawitzer",
        Gender = "M",
        Ability = "Mega Launcher",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Water Gun", "Splash" }
    }
},

// Sylveon
{
    "Sylveon",
    new PKMSVShowdownSets
    {
        Name = "Sylveon",
        Gender = "M",
        Ability = "Cute Charm",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tail Whip", "Growl", "Helping Hand", "Covet" }
    }
},

// Hawlucha
{
    "Hawlucha",
    new PKMSVShowdownSets
    {
        Name = "Hawlucha",
        Gender = "M",
        Ability = "Unburden",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "Hone Claws" }
    }
},

// Dedenne
{
    "Dedenne",
    new PKMSVShowdownSets
    {
        Name = "Dedenne",
        Gender = "M",
        Ability = "Pickup",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tail Whip", "Nuzzle" }
    }
},

// Carbink
{
    "Carbink",
    new PKMSVShowdownSets
    {
        Name = "Carbink",
        Ability = "Clear Body",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle", "Harden" }
    }
},

// Goomy
{
    "Goomy",
    new PKMSVShowdownSets
    {
        Name = "Goomy",
        Gender = "M",
        Ability = "Hydration",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Sliggoo
{
    "Sliggoo",
    new PKMSVShowdownSets
    {
        Name = "Sliggoo",
        Gender = "M",
        Ability = "Sap Sipper",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Sliggoo-Hisui (M)
{
    "Sliggoo-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Sliggoo",
        Form = "Hisui (M)",
        Ability = "Shell Armor",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Goodra
{
    "Goodra",
    new PKMSVShowdownSets
    {
        Name = "Goodra",
        Gender = "M",
        Ability = "Hydration",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Goodra-Hisui (M)
{
    "Goodra-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Goodra",
        Form = "Hisui (M)",
        Ability = "Sap Sipper",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Klefki
{
    "Klefki",
    new PKMSVShowdownSets
    {
        Name = "Klefki",
        Gender = "M",
        Ability = "Prankster",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Astonish" }
    }
},

// Phantump
{
    "Phantump",
    new PKMSVShowdownSets
    {
        Name = "Phantump",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "Astonish" }
    }
},

// Trevenant
{
    "Trevenant",
    new PKMSVShowdownSets
    {
        Name = "Trevenant",
        Gender = "M",
        Ability = "Natural Cure",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "Astonish" }
    }
},

// Bergmite
{
    "Bergmite",
    new PKMSVShowdownSets
    {
        Name = "Bergmite",
        Gender = "M",
        Ability = "Own Tempo",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Harden", "Rapid Spin" }
    }
},

// Avalugg
{
    "Avalugg",
    new PKMSVShowdownSets
    {
        Name = "Avalugg",
        Gender = "M",
        Ability = "Own Tempo",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Harden", "Rapid Spin" }
    }
},

// Avalugg-Hisui (M)
{
    "Avalugg-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Avalugg",
        Form = "Hisui (M)",
        Ability = "Strong Jaw",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Harden", "Rapid Spin" }
    }
},

// Noibat
{
    "Noibat",
    new PKMSVShowdownSets
    {
        Name = "Noibat",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Noivern
{
    "Noivern",
    new PKMSVShowdownSets
    {
        Name = "Noivern",
        Gender = "M",
        Ability = "Infiltrator",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Absorb" }
    }
},

// Rowlet
{
    "Rowlet",
    new PKMSVShowdownSets
    {
        Name = "Rowlet",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Dartrix
{
    "Dartrix",
    new PKMSVShowdownSets
    {
        Name = "Dartrix",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Decidueye
{
    "Decidueye",
    new PKMSVShowdownSets
    {
        Name = "Decidueye",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Decidueye-Hisui (M)
{
    "Decidueye-Hisui (M)",
    new PKMSVShowdownSets
    {
        Name = "Decidueye",
        Form = "Hisui (M)",
        Ability = "Overgrow",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Yungoos
{
    "Yungoos",
    new PKMSVShowdownSets
    {
        Name = "Yungoos",
        Gender = "M",
        Ability = "Strong Jaw",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Tackle" }
    }
},

// Gumshoos
{
    "Gumshoos",
    new PKMSVShowdownSets
    {
        Name = "Gumshoos",
        Gender = "M",
        Ability = "Strong Jaw",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle" }
    }
},

// Grubbin
{
    "Grubbin",
    new PKMSVShowdownSets
    {
        Name = "Grubbin",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Vise Grip", "Mud-Slap" }
    }
},

// Charjabug
{
    "Charjabug",
    new PKMSVShowdownSets
    {
        Name = "Charjabug",
        Gender = "M",
        Ability = "Battery",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Vise Grip", "Mud-Slap" }
    }
},

// Vikavolt
{
    "Vikavolt",
    new PKMSVShowdownSets
    {
        Name = "Vikavolt",
        Gender = "M",
        Ability = "Levitate",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Vise Grip", "Mud-Slap" }
    }
},

// Crabrawler
{
    "Crabrawler",
    new PKMSVShowdownSets
    {
        Name = "Crabrawler",
        Gender = "M",
        Ability = "Iron Fist",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Vise Grip" }
    }
},

// Crabominable
{
    "Crabominable",
    new PKMSVShowdownSets
    {
        Name = "Crabominable",
        Gender = "M",
        Ability = "Hyper Cutter",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Vise Grip" }
    }
},

// Oricorio
{
    "Oricorio",
    new PKMSVShowdownSets
    {
        Name = "Oricorio",
        Gender = "M",
        Ability = "Dancer",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Pound" }
    }
},

// Oricorio-Pom-Pom (M)
{
    "Oricorio-Pom-Pom (M)",
    new PKMSVShowdownSets
    {
        Name = "Oricorio",
        Form = "Pom-Pom (M)",
        Ability = "Dancer",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Pound" }
    }
},

// Oricorio-Pa’u (M)
{
    "Oricorio-Pa’u (M)",
    new PKMSVShowdownSets
    {
        Name = "Oricorio",
        Form = "Pa’u (M)",
        Ability = "Dancer",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Pound" }
    }
},

// Oricorio-Sensu (M)
{
    "Oricorio-Sensu (M)",
    new PKMSVShowdownSets
    {
        Name = "Oricorio",
        Form = "Sensu (M)",
        Ability = "Dancer",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Pound" }
    }
},

// Cutiefly
{
    "Cutiefly",
    new PKMSVShowdownSets
    {
        Name = "Cutiefly",
        Gender = "M",
        Ability = "Honey Gather",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Absorb", "Fairy Wind" }
    }
},

// Ribombee
{
    "Ribombee",
    new PKMSVShowdownSets
    {
        Name = "Ribombee",
        Gender = "M",
        Ability = "Shield Dust",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Absorb", "Fairy Wind" }
    }
},

// Rockruff
{
    "Rockruff",
    new PKMSVShowdownSets
    {
        Name = "Rockruff",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Lycanroc
{
    "Lycanroc",
    new PKMSVShowdownSets
    {
        Name = "Lycanroc",
        Gender = "M",
        Ability = "Sand Rush",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Lycanroc-Midnight (M)
{
    "Lycanroc-Midnight (M)",
    new PKMSVShowdownSets
    {
        Name = "Lycanroc",
        Form = "Midnight (M)",
        Ability = "Vital Spirit",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Lycanroc-Dusk (M)
{
    "Lycanroc-Dusk (M)",
    new PKMSVShowdownSets
    {
        Name = "Lycanroc",
        Form = "Dusk (M)",
        Ability = "Tough Claws",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Mareanie
{
    "Mareanie",
    new PKMSVShowdownSets
    {
        Name = "Mareanie",
        Gender = "M",
        Ability = "Merciless",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Poison Sting", "Peck" }
    }
},

// Toxapex
{
    "Toxapex",
    new PKMSVShowdownSets
    {
        Name = "Toxapex",
        Gender = "M",
        Ability = "Limber",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Poison Sting", "Peck" }
    }
},

// Mudbray
{
    "Mudbray",
    new PKMSVShowdownSets
    {
        Name = "Mudbray",
        Gender = "M",
        Ability = "Stamina",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Mud-Slap", "Rock Smash" }
    }
},

// Mudsdale
{
    "Mudsdale",
    new PKMSVShowdownSets
    {
        Name = "Mudsdale",
        Gender = "M",
        Ability = "Own Tempo",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Mud-Slap", "Rock Smash" }
    }
},

// Fomantis
{
    "Fomantis",
    new PKMSVShowdownSets
    {
        Name = "Fomantis",
        Gender = "M",
        Ability = "Leaf Guard",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Fury Cutter", "Leafage" }
    }
},

// Lurantis
{
    "Lurantis",
    new PKMSVShowdownSets
    {
        Name = "Lurantis",
        Gender = "M",
        Ability = "Leaf Guard",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Fury Cutter", "Leafage" }
    }
},

// Salandit
{
    "Salandit",
    new PKMSVShowdownSets
    {
        Name = "Salandit",
        Gender = "M",
        Ability = "Corrosion",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Scratch", "Poison Gas" }
    }
},

// Salazzle
{
    "Salazzle",
    new PKMSVShowdownSets
    {
        Name = "Salazzle",
        Gender = "F",
        Ability = "Corrosion",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Scratch", "Poison Gas" }
    }
},

// Bounsweet
{
    "Bounsweet",
    new PKMSVShowdownSets
    {
        Name = "Bounsweet",
        Gender = "F",
        Ability = "Oblivious",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Splash" }
    }
},

// Steenee
{
    "Steenee",
    new PKMSVShowdownSets
    {
        Name = "Steenee",
        Gender = "F",
        Ability = "Oblivious",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Calm",
        Moves = new List<string> { "Splash" }
    }
},

// Tsareena
{
    "Tsareena",
    new PKMSVShowdownSets
    {
        Name = "Tsareena",
        Gender = "F",
        Ability = "Queenly Majesty",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Splash" }
    }
},

// Oranguru
{
    "Oranguru",
    new PKMSVShowdownSets
    {
        Name = "Oranguru",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Confusion", "Taunt" }
    }
},

// Passimian
{
    "Passimian",
    new PKMSVShowdownSets
    {
        Name = "Passimian",
        Gender = "M",
        Ability = "Receiver",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Sandygast
{
    "Sandygast",
    new PKMSVShowdownSets
    {
        Name = "Sandygast",
        Gender = "M",
        Ability = "Water Compaction",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Absorb", "Harden" }
    }
},

// Palossand
{
    "Palossand",
    new PKMSVShowdownSets
    {
        Name = "Palossand",
        Gender = "M",
        Ability = "Water Compaction",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Absorb", "Harden" }
    }
},

// Komala
{
    "Komala",
    new PKMSVShowdownSets
    {
        Name = "Komala",
        Gender = "M",
        Ability = "Comatose",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Defense Curl", "Rollout" }
    }
},

// Mimikyu
{
    "Mimikyu",
    new PKMSVShowdownSets
    {
        Name = "Mimikyu",
        Gender = "M",
        Ability = "Disguise",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Splash", "Astonish", "Copycat", "Wood Hammer" }
    }
},

// Bruxish
{
    "Bruxish",
    new PKMSVShowdownSets
    {
        Name = "Bruxish",
        Gender = "M",
        Ability = "Dazzling",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Water Gun" }
    }
},

// Jangmo-o (M)
{
    "Jangmo-o (M)",
    new PKMSVShowdownSets
    {
        Name = "Jangmo",
        Form = "o (M)",
        Ability = "Bulletproof",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Hakamo-o (M)
{
    "Hakamo-o (M)",
    new PKMSVShowdownSets
    {
        Name = "Hakamo",
        Form = "o (M)",
        Ability = "Soundproof",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Kommo-o (M)
{
    "Kommo-o (M)",
    new PKMSVShowdownSets
    {
        Name = "Kommo",
        Form = "o (M)",
        Ability = "Bulletproof",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Grookey
{
    "Grookey",
    new PKMSVShowdownSets
    {
        Name = "Grookey",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Thwackey
{
    "Thwackey",
    new PKMSVShowdownSets
    {
        Name = "Thwackey",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Rillaboom
{
    "Rillaboom",
    new PKMSVShowdownSets
    {
        Name = "Rillaboom",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Scorbunny
{
    "Scorbunny",
    new PKMSVShowdownSets
    {
        Name = "Scorbunny",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Raboot
{
    "Raboot",
    new PKMSVShowdownSets
    {
        Name = "Raboot",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Cinderace
{
    "Cinderace",
    new PKMSVShowdownSets
    {
        Name = "Cinderace",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Sobble
{
    "Sobble",
    new PKMSVShowdownSets
    {
        Name = "Sobble",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Pound", "Growl" }
    }
},

// Drizzile
{
    "Drizzile",
    new PKMSVShowdownSets
    {
        Name = "Drizzile",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Pound", "Growl" }
    }
},

// Inteleon
{
    "Inteleon",
    new PKMSVShowdownSets
    {
        Name = "Inteleon",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Pound", "Growl" }
    }
},

// Skwovet
{
    "Skwovet",
    new PKMSVShowdownSets
    {
        Name = "Skwovet",
        Gender = "M",
        Ability = "Cheek Pouch",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Greedent
{
    "Greedent",
    new PKMSVShowdownSets
    {
        Name = "Greedent",
        Gender = "M",
        Ability = "Cheek Pouch",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Rookidee
{
    "Rookidee",
    new PKMSVShowdownSets
    {
        Name = "Rookidee",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Leer", "Peck" }
    }
},

// Corvisquire
{
    "Corvisquire",
    new PKMSVShowdownSets
    {
        Name = "Corvisquire",
        Gender = "M",
        Ability = "Keen Eye",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Leer", "Peck" }
    }
},

// Corviknight
{
    "Corviknight",
    new PKMSVShowdownSets
    {
        Name = "Corviknight",
        Gender = "M",
        Ability = "Pressure",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Leer", "Peck" }
    }
},

// Chewtle
{
    "Chewtle",
    new PKMSVShowdownSets
    {
        Name = "Chewtle",
        Gender = "M",
        Ability = "Shell Armor",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Water Gun" }
    }
},

// Drednaw
{
    "Drednaw",
    new PKMSVShowdownSets
    {
        Name = "Drednaw",
        Gender = "M",
        Ability = "Strong Jaw",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tackle", "Water Gun" }
    }
},

// Rolycoly
{
    "Rolycoly",
    new PKMSVShowdownSets
    {
        Name = "Rolycoly",
        Gender = "M",
        Ability = "Heatproof",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "Smokescreen" }
    }
},

// Carkol
{
    "Carkol",
    new PKMSVShowdownSets
    {
        Name = "Carkol",
        Gender = "M",
        Ability = "Flame Body",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Smokescreen" }
    }
},

// Coalossal
{
    "Coalossal",
    new PKMSVShowdownSets
    {
        Name = "Coalossal",
        Gender = "M",
        Ability = "Steam Engine",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tackle", "Smokescreen" }
    }
},

// Applin
{
    "Applin",
    new PKMSVShowdownSets
    {
        Name = "Applin",
        Gender = "M",
        Ability = "Gluttony",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Astonish", "Withdraw" }
    }
},

// Flapple
{
    "Flapple",
    new PKMSVShowdownSets
    {
        Name = "Flapple",
        Gender = "M",
        Ability = "Ripen",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Astonish", "Withdraw" }
    }
},

// Appletun
{
    "Appletun",
    new PKMSVShowdownSets
    {
        Name = "Appletun",
        Gender = "M",
        Ability = "Gluttony",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Astonish", "Withdraw" }
    }
},

// Silicobra
{
    "Silicobra",
    new PKMSVShowdownSets
    {
        Name = "Silicobra",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Sand Attack", "Wrap" }
    }
},

// Sandaconda
{
    "Sandaconda",
    new PKMSVShowdownSets
    {
        Name = "Sandaconda",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Sand Attack", "Wrap" }
    }
},

// Cramorant
{
    "Cramorant",
    new PKMSVShowdownSets
    {
        Name = "Cramorant",
        Gender = "M",
        Ability = "Gulp Missile",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Peck", "Stockpile", "Spit Up", "Swallow" }
    }
},

// Arrokuda
{
    "Arrokuda",
    new PKMSVShowdownSets
    {
        Name = "Arrokuda",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Peck", "Aqua Jet" }
    }
},

// Barraskewda
{
    "Barraskewda",
    new PKMSVShowdownSets
    {
        Name = "Barraskewda",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Peck", "Aqua Jet" }
    }
},

// Toxel
{
    "Toxel",
    new PKMSVShowdownSets
    {
        Name = "Toxel",
        Gender = "M",
        Ability = "Static",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Flail", "Belch", "Nuzzle", "Tearful Look" }
    }
},

// Toxtricity
{
    "Toxtricity",
    new PKMSVShowdownSets
    {
        Name = "Toxtricity",
        Gender = "M",
        Ability = "Plus",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Flail", "Belch", "Nuzzle", "Tearful Look" }
    }
},

// Toxtricity-Low-Key (M)
{
    "Toxtricity-Low-Key (M)",
    new PKMSVShowdownSets
    {
        Name = "Toxtricity",
        Form = "Low-Key (M)",
        Ability = "Punk Rock",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Flail", "Belch", "Nuzzle", "Tearful Look" }
    }
},

// Sinistea
{
    "Sinistea",
    new PKMSVShowdownSets
    {
        Name = "Sinistea",
        Ability = "Weak Armor",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Withdraw", "Astonish" }
    }
},

// Sinistea-Antique
{
    "Sinistea-Antique",
    new PKMSVShowdownSets
    {
        Name = "Sinistea",
        Form = "Antique",
        Ability = "Weak Armor",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Mega Drain", "Sucker Punch", "Sweet Scent", "Giga Drain" }
    }
},

// Polteageist
{
    "Polteageist",
    new PKMSVShowdownSets
    {
        Name = "Polteageist",
        Ability = "Weak Armor",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Withdraw", "Astonish" }
    }
},

// Polteageist-Antique
{
    "Polteageist-Antique",
    new PKMSVShowdownSets
    {
        Name = "Polteageist",
        Form = "Antique",
        Ability = "Weak Armor",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Mega Drain", "Sucker Punch", "Sweet Scent", "Giga Drain" }
    }
},

// Hatenna
{
    "Hatenna",
    new PKMSVShowdownSets
    {
        Name = "Hatenna",
        Gender = "F",
        Ability = "Anticipation",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Confusion", "Play Nice" }
    }
},

// Hattrem
{
    "Hattrem",
    new PKMSVShowdownSets
    {
        Name = "Hattrem",
        Gender = "F",
        Ability = "Healer",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Confusion", "Play Nice" }
    }
},

// Hatterene
{
    "Hatterene",
    new PKMSVShowdownSets
    {
        Name = "Hatterene",
        Gender = "F",
        Ability = "Anticipation",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Confusion", "Play Nice" }
    }
},

// Impidimp
{
    "Impidimp",
    new PKMSVShowdownSets
    {
        Name = "Impidimp",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Fake Out", "Confide" }
    }
},

// Morgrem
{
    "Morgrem",
    new PKMSVShowdownSets
    {
        Name = "Morgrem",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Fake Out", "Confide" }
    }
},

// Grimmsnarl
{
    "Grimmsnarl",
    new PKMSVShowdownSets
    {
        Name = "Grimmsnarl",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Fake Out", "Confide" }
    }
},

// Perrserker
{
    "Perrserker",
    new PKMSVShowdownSets
    {
        Name = "Perrserker",
        Gender = "M",
        Ability = "Tough Claws",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Growl", "Fake Out" }
    }
},

// Falinks
{
    "Falinks",
    new PKMSVShowdownSets
    {
        Name = "Falinks",
        Ability = "Battle Armor",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tackle", "Protect" }
    }
},

// Pincurchin
{
    "Pincurchin",
    new PKMSVShowdownSets
    {
        Name = "Pincurchin",
        Gender = "M",
        Ability = "Lightning Rod",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Peck", "Thunder Shock" }
    }
},

// Snom
{
    "Snom",
    new PKMSVShowdownSets
    {
        Name = "Snom",
        Gender = "M",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Struggle Bug", "Powder Snow" }
    }
},

// Frosmoth
{
    "Frosmoth",
    new PKMSVShowdownSets
    {
        Name = "Frosmoth",
        Gender = "M",
        Ability = "Shield Dust",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Struggle Bug", "Powder Snow" }
    }
},

// Stonjourner
{
    "Stonjourner",
    new PKMSVShowdownSets
    {
        Name = "Stonjourner",
        Gender = "M",
        Ability = "Power Spot",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Rock Throw", "Block" }
    }
},

// Eiscue
{
    "Eiscue",
    new PKMSVShowdownSets
    {
        Name = "Eiscue",
        Gender = "M",
        Ability = "Ice Face",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Powder Snow" }
    }
},

// Indeedee
{
    "Indeedee",
    new PKMSVShowdownSets
    {
        Name = "Indeedee",
        Gender = "M",
        Ability = "Inner Focus",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Stored Power", "Play Nice" }
    }
},

// Morpeko
{
    "Morpeko",
    new PKMSVShowdownSets
    {
        Name = "Morpeko",
        Gender = "M",
        Ability = "Hunger Switch",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tail Whip", "Thunder Shock" }
    }
},

// Cufant
{
    "Cufant",
    new PKMSVShowdownSets
    {
        Name = "Cufant",
        Gender = "M",
        Ability = "Sheer Force",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Copperajah
{
    "Copperajah",
    new PKMSVShowdownSets
    {
        Name = "Copperajah",
        Gender = "M",
        Ability = "Sheer Force",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Dreepy
{
    "Dreepy",
    new PKMSVShowdownSets
    {
        Name = "Dreepy",
        Gender = "M",
        Ability = "Clear Body",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Bite", "Quick Attack", "Astonish", "Infestation" }
    }
},

// Drakloak
{
    "Drakloak",
    new PKMSVShowdownSets
    {
        Name = "Drakloak",
        Gender = "M",
        Ability = "Clear Body",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Bite", "Quick Attack", "Astonish", "Infestation" }
    }
},

// Dragapult
{
    "Dragapult",
    new PKMSVShowdownSets
    {
        Name = "Dragapult",
        Gender = "M",
        Ability = "Clear Body",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Bite", "Quick Attack", "Astonish", "Infestation" }
    }
},

// Wyrdeer
{
    "Wyrdeer",
    new PKMSVShowdownSets
    {
        Name = "Wyrdeer",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle" }
    }
},

// Kleavor
{
    "Kleavor",
    new PKMSVShowdownSets
    {
        Name = "Kleavor",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Leer", "Quick Attack" }
    }
},

// Ursaluna
{
    "Ursaluna",
    new PKMSVShowdownSets
    {
        Name = "Ursaluna",
        Gender = "M",
        Ability = "Guts",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Lick", "Covet", "Fling", "Baby-Doll Eyes" }
    }
},

// Ursaluna-Bloodmoon (M)
{
    "Ursaluna-Bloodmoon (M)",
    new PKMSVShowdownSets
    {
        Name = "Ursaluna",
        Form = "Bloodmoon (M)",
        Ability = "Mind’s Eye",
        TeraType = "Normal",
        Level = "95",
        Moves = new List<string> { "Earth Power", "Slash", "Calm Mind" }
    }
},

// Basculegion
{
    "Basculegion",
    new PKMSVShowdownSets
    {
        Name = "Basculegion",
        Gender = "M",
        Ability = "Adaptability",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tail Whip", "Water Gun" }
    }
},

// Basculegion-F (F)
{
    "Basculegion-F (F)",
    new PKMSVShowdownSets
    {
        Name = "Basculegion",
        Form = "F (F)",
        Ability = "Swift Swim",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tail Whip", "Water Gun" }
    }
},

// Sneasler
{
    "Sneasler",
    new PKMSVShowdownSets
    {
        Name = "Sneasler",
        Gender = "M",
        Ability = "Unburden",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Scratch", "Leer", "Rock Smash" }
    }
},

// Overqwil
{
    "Overqwil",
    new PKMSVShowdownSets
    {
        Name = "Overqwil",
        Gender = "M",
        Ability = "Swift Swim",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tackle", "Poison Sting" }
    }
},

// Sprigatito
{
    "Sprigatito",
    new PKMSVShowdownSets
    {
        Name = "Sprigatito",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Scratch", "Tail Whip", "Leafage" }
    }
},

// Floragato
{
    "Floragato",
    new PKMSVShowdownSets
    {
        Name = "Floragato",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Scratch", "Tail Whip", "Leafage" }
    }
},

// Meowscarada
{
    "Meowscarada",
    new PKMSVShowdownSets
    {
        Name = "Meowscarada",
        Gender = "M",
        Ability = "Overgrow",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Scratch", "Tail Whip", "Leafage" }
    }
},

// Fuecoco
{
    "Fuecoco",
    new PKMSVShowdownSets
    {
        Name = "Fuecoco",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Leer", "Ember" }
    }
},

// Crocalor
{
    "Crocalor",
    new PKMSVShowdownSets
    {
        Name = "Crocalor",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Leer", "Ember" }
    }
},

// Skeledirge
{
    "Skeledirge",
    new PKMSVShowdownSets
    {
        Name = "Skeledirge",
        Gender = "M",
        Ability = "Blaze",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle", "Leer", "Ember" }
    }
},

// Quaxly
{
    "Quaxly",
    new PKMSVShowdownSets
    {
        Name = "Quaxly",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Pound", "Growl", "Water Gun" }
    }
},

// Quaxwell
{
    "Quaxwell",
    new PKMSVShowdownSets
    {
        Name = "Quaxwell",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Pound", "Growl", "Water Gun" }
    }
},

// Quaquaval
{
    "Quaquaval",
    new PKMSVShowdownSets
    {
        Name = "Quaquaval",
        Gender = "M",
        Ability = "Torrent",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Pound", "Growl", "Water Gun" }
    }
},

// Lechonk
{
    "Lechonk",
    new PKMSVShowdownSets
    {
        Name = "Lechonk",
        Gender = "M",
        Ability = "Gluttony",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Oinkologne
{
    "Oinkologne",
    new PKMSVShowdownSets
    {
        Name = "Oinkologne",
        Gender = "M",
        Ability = "Lingering Aroma",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Oinkologne-F (F)
{
    "Oinkologne-F (F)",
    new PKMSVShowdownSets
    {
        Name = "Oinkologne",
        Form = "F (F)",
        Ability = "Gluttony",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Tail Whip" }
    }
},

// Tarountula
{
    "Tarountula",
    new PKMSVShowdownSets
    {
        Name = "Tarountula",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Relaxed",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Spidops
{
    "Spidops",
    new PKMSVShowdownSets
    {
        Name = "Spidops",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "String Shot" }
    }
},

// Nymble
{
    "Nymble",
    new PKMSVShowdownSets
    {
        Name = "Nymble",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Lokix
{
    "Lokix",
    new PKMSVShowdownSets
    {
        Name = "Lokix",
        Gender = "M",
        Ability = "Swarm",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Tackle", "Leer" }
    }
},

// Pawmi
{
    "Pawmi",
    new PKMSVShowdownSets
    {
        Name = "Pawmi",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Pawmo
{
    "Pawmo",
    new PKMSVShowdownSets
    {
        Name = "Pawmo",
        Gender = "M",
        Ability = "Natural Cure",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Pawmot
{
    "Pawmot",
    new PKMSVShowdownSets
    {
        Name = "Pawmot",
        Gender = "M",
        Ability = "Volt Absorb",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Scratch", "Growl" }
    }
},

// Tandemaus
{
    "Tandemaus",
    new PKMSVShowdownSets
    {
        Name = "Tandemaus",
        Ability = "Run Away",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Pound", "Baby-Doll Eyes" }
    }
},

// Maushold
{
    "Maushold",
    new PKMSVShowdownSets
    {
        Name = "Maushold",
        Ability = "Friend Guard",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Pound", "Baby-Doll Eyes", "Echoed Voice", "Helping Hand" }
    }
},

// Maushold-Four
{
    "Maushold-Four",
    new PKMSVShowdownSets
    {
        Name = "Maushold",
        Form = "Four",
        Ability = "Friend Guard",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Pound", "Baby-Doll Eyes" }
    }
},

// Fidough
{
    "Fidough",
    new PKMSVShowdownSets
    {
        Name = "Fidough",
        Gender = "M",
        Ability = "Own Tempo",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Dachsbun
{
    "Dachsbun",
    new PKMSVShowdownSets
    {
        Name = "Dachsbun",
        Gender = "M",
        Ability = "Well-Baked Body",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Smoliv
{
    "Smoliv",
    new PKMSVShowdownSets
    {
        Name = "Smoliv",
        Gender = "M",
        Ability = "Early Bird",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Tackle", "Sweet Scent" }
    }
},

// Dolliv
{
    "Dolliv",
    new PKMSVShowdownSets
    {
        Name = "Dolliv",
        Gender = "M",
        Ability = "Early Bird",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Sweet Scent" }
    }
},

// Arboliva
{
    "Arboliva",
    new PKMSVShowdownSets
    {
        Name = "Arboliva",
        Gender = "M",
        Ability = "Seed Sower",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Sweet Scent" }
    }
},

// Squawkabilly
{
    "Squawkabilly",
    new PKMSVShowdownSets
    {
        Name = "Squawkabilly",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Growl", "Peck", "Mimic" }
    }
},

// Squawkabilly-Blue (M)
{
    "Squawkabilly-Blue (M)",
    new PKMSVShowdownSets
    {
        Name = "Squawkabilly",
        Form = "Blue (M)",
        Ability = "Hustle",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Growl", "Peck", "Mimic" }
    }
},

// Squawkabilly-Yellow (M)
{
    "Squawkabilly-Yellow (M)",
    new PKMSVShowdownSets
    {
        Name = "Squawkabilly",
        Form = "Yellow (M)",
        Ability = "Intimidate",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Growl", "Peck", "Mimic" }
    }
},

// Squawkabilly-White (M)
{
    "Squawkabilly-White (M)",
    new PKMSVShowdownSets
    {
        Name = "Squawkabilly",
        Form = "White (M)",
        Ability = "Hustle",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Growl", "Peck", "Mimic" }
    }
},

// Nacli
{
    "Nacli",
    new PKMSVShowdownSets
    {
        Name = "Nacli",
        Gender = "M",
        Ability = "Sturdy",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Harden" }
    }
},

// Naclstack
{
    "Naclstack",
    new PKMSVShowdownSets
    {
        Name = "Naclstack",
        Gender = "M",
        Ability = "Purifying Salt",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Harden" }
    }
},

// Garganacl
{
    "Garganacl",
    new PKMSVShowdownSets
    {
        Name = "Garganacl",
        Gender = "M",
        Ability = "Purifying Salt",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Tackle", "Harden" }
    }
},

// Charcadet
{
    "Charcadet",
    new PKMSVShowdownSets
    {
        Name = "Charcadet",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Leer", "Ember", "Astonish" }
    }
},

// Armarouge
{
    "Armarouge",
    new PKMSVShowdownSets
    {
        Name = "Armarouge",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Leer", "Ember", "Astonish" }
    }
},

// Ceruledge
{
    "Ceruledge",
    new PKMSVShowdownSets
    {
        Name = "Ceruledge",
        Gender = "M",
        Ability = "Flash Fire",
        TeraType = "Fire",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Leer", "Ember", "Astonish" }
    }
},

// Tadbulb
{
    "Tadbulb",
    new PKMSVShowdownSets
    {
        Name = "Tadbulb",
        Gender = "M",
        Ability = "Static",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Tackle", "Mud-Slap" }
    }
},

// Bellibolt
{
    "Bellibolt",
    new PKMSVShowdownSets
    {
        Name = "Bellibolt",
        Gender = "M",
        Ability = "Electromorphosis",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Mud-Slap" }
    }
},

// Wattrel
{
    "Wattrel",
    new PKMSVShowdownSets
    {
        Name = "Wattrel",
        Gender = "M",
        Ability = "Wind Power",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Kilowattrel
{
    "Kilowattrel",
    new PKMSVShowdownSets
    {
        Name = "Kilowattrel",
        Gender = "M",
        Ability = "Wind Power",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Maschiff
{
    "Maschiff",
    new PKMSVShowdownSets
    {
        Name = "Maschiff",
        Gender = "M",
        Ability = "Intimidate",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Leer", "Scary Face" }
    }
},

// Mabosstiff
{
    "Mabosstiff",
    new PKMSVShowdownSets
    {
        Name = "Mabosstiff",
        Gender = "M",
        Ability = "Guard Dog",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Tackle", "Leer", "Scary Face" }
    }
},

// Shroodle
{
    "Shroodle",
    new PKMSVShowdownSets
    {
        Name = "Shroodle",
        Gender = "M",
        Ability = "Unburden",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Grafaiai
{
    "Grafaiai",
    new PKMSVShowdownSets
    {
        Name = "Grafaiai",
        Gender = "M",
        Ability = "Unburden",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Bramblin
{
    "Bramblin",
    new PKMSVShowdownSets
    {
        Name = "Bramblin",
        Gender = "M",
        Ability = "Wind Rider",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Defense Curl", "Rollout", "Astonish" }
    }
},

// Brambleghast
{
    "Brambleghast",
    new PKMSVShowdownSets
    {
        Name = "Brambleghast",
        Gender = "M",
        Ability = "Wind Rider",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Defense Curl", "Rollout", "Astonish" }
    }
},

// Toedscool
{
    "Toedscool",
    new PKMSVShowdownSets
    {
        Name = "Toedscool",
        Gender = "M",
        Ability = "Mycelium Might",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Wrap", "Mud-Slap" }
    }
},

// Toedscruel
{
    "Toedscruel",
    new PKMSVShowdownSets
    {
        Name = "Toedscruel",
        Gender = "M",
        Ability = "Mycelium Might",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Wrap", "Mud-Slap" }
    }
},

// Klawf
{
    "Klawf",
    new PKMSVShowdownSets
    {
        Name = "Klawf",
        Gender = "M",
        Ability = "Anger Shell",
        TeraType = "Rock",
        Level = "95",
        Shiny = "Yes",
        Nature = "Rash",
        Moves = new List<string> { "Vise Grip", "Rock Throw" }
    }
},

// Capsakid
{
    "Capsakid",
    new PKMSVShowdownSets
    {
        Name = "Capsakid",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Leer", "Leafage" }
    }
},

// Scovillain
{
    "Scovillain",
    new PKMSVShowdownSets
    {
        Name = "Scovillain",
        Gender = "M",
        Ability = "Insomnia",
        TeraType = "Grass",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Leer", "Leafage" }
    }
},

// Rellor
{
    "Rellor",
    new PKMSVShowdownSets
    {
        Name = "Rellor",
        Gender = "M",
        Ability = "Compound Eyes",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Rabsca
{
    "Rabsca",
    new PKMSVShowdownSets
    {
        Name = "Rabsca",
        Gender = "M",
        Ability = "Synchronize",
        TeraType = "Bug",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Tackle", "Defense Curl" }
    }
},

// Flittle
{
    "Flittle",
    new PKMSVShowdownSets
    {
        Name = "Flittle",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Espathra
{
    "Espathra",
    new PKMSVShowdownSets
    {
        Name = "Espathra",
        Gender = "M",
        Ability = "Frisk",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Growl", "Peck" }
    }
},

// Tinkatink
{
    "Tinkatink",
    new PKMSVShowdownSets
    {
        Name = "Tinkatink",
        Gender = "F",
        Ability = "Own Tempo",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Astonish", "Fairy Wind" }
    }
},

// Tinkatuff
{
    "Tinkatuff",
    new PKMSVShowdownSets
    {
        Name = "Tinkatuff",
        Gender = "F",
        Ability = "Mold Breaker",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Astonish", "Fairy Wind" }
    }
},

// Tinkaton
{
    "Tinkaton",
    new PKMSVShowdownSets
    {
        Name = "Tinkaton",
        Gender = "F",
        Ability = "Mold Breaker",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Astonish", "Fairy Wind" }
    }
},

// Wiglett
{
    "Wiglett",
    new PKMSVShowdownSets
    {
        Name = "Wiglett",
        Gender = "M",
        Ability = "Rattled",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Mild",
        Moves = new List<string> { "Sand Attack", "Water Gun" }
    }
},

// Wugtrio
{
    "Wugtrio",
    new PKMSVShowdownSets
    {
        Name = "Wugtrio",
        Gender = "M",
        Ability = "Gooey",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Sand Attack", "Water Gun" }
    }
},

// Bombirdier
{
    "Bombirdier",
    new PKMSVShowdownSets
    {
        Name = "Bombirdier",
        Gender = "M",
        Ability = "Big Pecks",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Leer", "Peck", "Memento", "Hone Claws" }
    }
},

// Finizen
{
    "Finizen",
    new PKMSVShowdownSets
    {
        Name = "Finizen",
        Gender = "M",
        Ability = "Water Veil",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Supersonic", "Water Gun" }
    }
},

// Palafin
{
    "Palafin",
    new PKMSVShowdownSets
    {
        Name = "Palafin",
        Gender = "M",
        Ability = "Zero to Hero",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Supersonic", "Water Gun" }
    }
},

// Varoom
{
    "Varoom",
    new PKMSVShowdownSets
    {
        Name = "Varoom",
        Gender = "M",
        Ability = "Overcoat",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Brave",
        Moves = new List<string> { "Lick", "Poison Gas" }
    }
},

// Revavroom
{
    "Revavroom",
    new PKMSVShowdownSets
    {
        Name = "Revavroom",
        Gender = "M",
        Ability = "Overcoat",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Lick", "Poison Gas" }
    }
},

// Cyclizar
{
    "Cyclizar",
    new PKMSVShowdownSets
    {
        Name = "Cyclizar",
        Gender = "M",
        Ability = "Shed Skin",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Orthworm
{
    "Orthworm",
    new PKMSVShowdownSets
    {
        Name = "Orthworm",
        Gender = "M",
        Ability = "Earth Eater",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naive",
        Moves = new List<string> { "Tackle", "Wrap", "Harden" }
    }
},

// Glimmet
{
    "Glimmet",
    new PKMSVShowdownSets
    {
        Name = "Glimmet",
        Gender = "M",
        Ability = "Toxic Debris",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Rock Throw", "Harden", "Smack Down" }
    }
},

// Glimmora
{
    "Glimmora",
    new PKMSVShowdownSets
    {
        Name = "Glimmora",
        Gender = "M",
        Ability = "Toxic Debris",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Timid",
        Moves = new List<string> { "Rock Throw", "Harden", "Smack Down" }
    }
},

// Greavard
{
    "Greavard",
    new PKMSVShowdownSets
    {
        Name = "Greavard",
        Gender = "M",
        Ability = "Pickup",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bold",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Houndstone
{
    "Houndstone",
    new PKMSVShowdownSets
    {
        Name = "Houndstone",
        Gender = "M",
        Ability = "Sand Rush",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Modest",
        Moves = new List<string> { "Tackle", "Growl" }
    }
},

// Flamigo
{
    "Flamigo",
    new PKMSVShowdownSets
    {
        Name = "Flamigo",
        Gender = "M",
        Ability = "Tangled Feet",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Peck", "Copycat" }
    }
},

// Cetoddle
{
    "Cetoddle",
    new PKMSVShowdownSets
    {
        Name = "Cetoddle",
        Gender = "M",
        Ability = "Thick Fat",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Tackle", "Powder Snow" }
    }
},

// Cetitan
{
    "Cetitan",
    new PKMSVShowdownSets
    {
        Name = "Cetitan",
        Gender = "M",
        Ability = "Thick Fat",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Tackle", "Powder Snow" }
    }
},

// Veluza
{
    "Veluza",
    new PKMSVShowdownSets
    {
        Name = "Veluza",
        Gender = "M",
        Ability = "Mold Breaker",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Tackle", "Aqua Jet" }
    }
},

// Dondozo
{
    "Dondozo",
    new PKMSVShowdownSets
    {
        Name = "Dondozo",
        Gender = "M",
        Ability = "Oblivious",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tackle", "Supersonic", "Water Gun" }
    }
},

// Tatsugiri
{
    "Tatsugiri",
    new PKMSVShowdownSets
    {
        Name = "Tatsugiri",
        Gender = "M",
        Ability = "Commander",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Water Gun", "Splash" }
    }
},

// Tatsugiri-Droopy (M)
{
    "Tatsugiri-Droopy (M)",
    new PKMSVShowdownSets
    {
        Name = "Tatsugiri",
        Form = "Droopy (M)",
        Ability = "Commander",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Water Gun", "Splash" }
    }
},

// Tatsugiri-Stretchy (M)
{
    "Tatsugiri-Stretchy (M)",
    new PKMSVShowdownSets
    {
        Name = "Tatsugiri",
        Form = "Stretchy (M)",
        Ability = "Commander",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Water Gun", "Splash" }
    }
},

// Annihilape
{
    "Annihilape",
    new PKMSVShowdownSets
    {
        Name = "Annihilape",
        Gender = "M",
        Ability = "Vital Spirit",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Scratch", "Leer", "Focus Energy", "Covet" }
    }
},

// Clodsire
{
    "Clodsire",
    new PKMSVShowdownSets
    {
        Name = "Clodsire",
        Gender = "M",
        Ability = "Water Absorb",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Tail Whip", "Mud Shot" }
    }
},

// Farigiraf
{
    "Farigiraf",
    new PKMSVShowdownSets
    {
        Name = "Farigiraf",
        Gender = "M",
        Ability = "Cud Chew",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Growl", "Astonish", "Power Swap", "Guard Swap" }
    }
},

// Dudunsparce
{
    "Dudunsparce",
    new PKMSVShowdownSets
    {
        Name = "Dudunsparce",
        Gender = "M",
        Ability = "Serene Grace",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Impish",
        Moves = new List<string> { "Defense Curl", "Flail" }
    }
},

// Dudunsparce-Three-Segment (M)
{
    "Dudunsparce-Three-Segment (M)",
    new PKMSVShowdownSets
    {
        Name = "Dudunsparce",
        Form = "Three-Segment (M)",
        Ability = "Serene Grace",
        TeraType = "Normal",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Flail", "Mud-Slap", "Rollout", "Glare" }
    }
},

// Kingambit
{
    "Kingambit",
    new PKMSVShowdownSets
    {
        Name = "Kingambit",
        Gender = "M",
        Ability = "Supreme Overlord",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lax",
        Moves = new List<string> { "Scratch", "Leer" }
    }
},

// Great Tusk
{
    "Great Tusk",
    new PKMSVShowdownSets
    {
        Name = "Great Tusk",
        Ability = "Protosynthesis",
        TeraType = "Steel",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Close Combat", "Earthquake", "Rock Slide", "Ice Spinner" }
    }
},

// Scream Tail
{
    "Scream Tail",
    new PKMSVShowdownSets
    {
        Name = "Scream Tail",
        Ability = "Protosynthesis",
        TeraType = "Psychic",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hardy",
        Moves = new List<string> { "Body Slam", "Rest", "Play Rough", "Hyper Voice" }
    }
},

// Brute Bonnet
{
    "Brute Bonnet",
    new PKMSVShowdownSets
    {
        Name = "Brute Bonnet",
        Ability = "Protosynthesis",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Clear Smog", "Payback", "Thrash", "Giga Drain" }
    }
},

// Flutter Mane
{
    "Flutter Mane",
    new PKMSVShowdownSets
    {
        Name = "Flutter Mane",
        Ability = "Protosynthesis",
        TeraType = "Fairy",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quiet",
        Moves = new List<string> { "Wish", "Dazzling Gleam", "Shadow Ball", "Mystical Fire" }
    }
},

// Slither Wing
{
    "Slither Wing",
    new PKMSVShowdownSets
    {
        Name = "Slither Wing",
        Ability = "Protosynthesis",
        TeraType = "Poison",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Superpower", "First Impression", "Flare Blitz", "Heavy Slam" }
    }
},

// Sandy Shocks
{
    "Sandy Shocks",
    new PKMSVShowdownSets
    {
        Name = "Sandy Shocks",
        Ability = "Protosynthesis",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Tri Attack", "Screech", "Heavy Slam", "Metal Sound" }
    }
},

// Iron Treads
{
    "Iron Treads",
    new PKMSVShowdownSets
    {
        Name = "Iron Treads",
        Ability = "Quark Drive",
        TeraType = "Ground",
        Level = "95",
        Shiny = "Yes",
        Nature = "Serious",
        Moves = new List<string> { "Iron Head", "Earthquake", "Megahorn", "Wild Charge" }
    }
},

// Iron Bundle
{
    "Iron Bundle",
    new PKMSVShowdownSets
    {
        Name = "Iron Bundle",
        Ability = "Quark Drive",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Gentle",
        Moves = new List<string> { "Drill Peck", "Helping Hand", "Freeze-Dry", "Flip Turn" }
    }
},

// Iron Hands
{
    "Iron Hands",
    new PKMSVShowdownSets
    {
        Name = "Iron Hands",
        Ability = "Quark Drive",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Slam", "Force Palm", "Seismic Toss", "Charge" }
    }
},

// Iron Jugulis
{
    "Iron Jugulis",
    new PKMSVShowdownSets
    {
        Name = "Iron Jugulis",
        Ability = "Quark Drive",
        TeraType = "Flying",
        Level = "95",
        Shiny = "Yes",
        Nature = "Lonely",
        Moves = new List<string> { "Dragon Breath", "Snarl", "Crunch", "Hyper Voice" }
    }
},

// Iron Moth
{
    "Iron Moth",
    new PKMSVShowdownSets
    {
        Name = "Iron Moth",
        Ability = "Quark Drive",
        TeraType = "Water",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Energy Ball", "Overheat", "Sludge Wave", "Acid Spray" }
    }
},

// Iron Thorns
{
    "Iron Thorns",
    new PKMSVShowdownSets
    {
        Name = "Iron Thorns",
        Ability = "Quark Drive",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Bite", "Charge", "Rock Slide", "Sandstorm" }
    }
},

// Frigibax
{
    "Frigibax",
    new PKMSVShowdownSets
    {
        Name = "Frigibax",
        Gender = "M",
        Ability = "Thermal Exchange",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Tackle", "Leer", "Dragon Tail" }
    }
},

// Arctibax
{
    "Arctibax",
    new PKMSVShowdownSets
    {
        Name = "Arctibax",
        Gender = "M",
        Ability = "Thermal Exchange",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Jolly",
        Moves = new List<string> { "Tackle", "Leer", "Dragon Tail" }
    }
},

// Baxcalibur
{
    "Baxcalibur",
    new PKMSVShowdownSets
    {
        Name = "Baxcalibur",
        Gender = "M",
        Ability = "Thermal Exchange",
        TeraType = "Ice",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Tackle", "Leer", "Dragon Tail" }
    }
},

// Gimmighoul
{
    "Gimmighoul",
    new PKMSVShowdownSets
    {
        Name = "Gimmighoul",
        Ability = "Rattled",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Adamant",
        Moves = new List<string> { "Take Down", "Shadow Ball", "Hex", "Power Gem" }
    }
},

// Gholdengo
{
    "Gholdengo",
    new PKMSVShowdownSets
    {
        Name = "Gholdengo",
        Ability = "Good as Gold",
        TeraType = "Electric",
        Level = "95",
        Shiny = "Yes",
        Nature = "Hasty",
        Moves = new List<string> { "Take Down", "Shadow Ball", "Hex", "Power Gem" }
    }
},

// Wo-Chien
{
    "Wo-Chien",
    new PKMSVShowdownSets
    {
        Name = "Wo",
        Form = "Chien",
        Ability = "Tablets of Ruin",
        TeraType = "Grass",
        Level = "95",
        Moves = new List<string> { "Ruination", "Foul Play", "Power Whip" }
    }
},

// Chien-Pao
{
    "Chien-Pao",
    new PKMSVShowdownSets
    {
        Name = "Chien",
        Form = "Pao",
        Ability = "Sword of Ruin",
        TeraType = "Ice",
        Level = "95",
        Moves = new List<string> { "Ruination", "Sucker Punch", "Sacred Sword" }
    }
},

// Ting-Lu
{
    "Ting-Lu",
    new PKMSVShowdownSets
    {
        Name = "Ting",
        Form = "Lu",
        Ability = "Vessel of Ruin",
        TeraType = "Dark",
        Level = "95",
        Moves = new List<string> { "Ruination", "Throat Chop", "Rock Slide" }
    }
},

// Chi-Yu
{
    "Chi-Yu",
    new PKMSVShowdownSets
    {
        Name = "Chi",
        Form = "Yu",
        Ability = "Beads of Ruin",
        TeraType = "Dark",
        Level = "95",
        Moves = new List<string> { "Ruination", "Bounce", "Swagger" }
    }
},

// Roaring Moon
{
    "Roaring Moon",
    new PKMSVShowdownSets
    {
        Name = "Roaring Moon",
        Ability = "Protosynthesis",
        TeraType = "Dark",
        Level = "95",
        Shiny = "Yes",
        Nature = "Careful",
        Moves = new List<string> { "Dragon Claw", "Zen Headbutt", "Flamethrower", "Night Slash" }
    }
},

// Iron Valiant
{
    "Iron Valiant",
    new PKMSVShowdownSets
    {
        Name = "Iron Valiant",
        Ability = "Quark Drive",
        TeraType = "Fighting",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Dazzling Gleam", "Psycho Cut", "Night Slash", "Leaf Blade" }
    }
},

// Koraidon
{
    "Koraidon",
    new PKMSVShowdownSets
    {
        Name = "Koraidon",
        Ability = "Orichalcum Pulse",
        TeraType = "Fighting",
        Level = "95",
        Moves = new List<string> { "Bulk Up", "Collision Course", "Flamethrower" }
    }
},

// Miraidon
{
    "Miraidon",
    new PKMSVShowdownSets
    {
        Name = "Miraidon",
        Ability = "Hadron Engine",
        TeraType = "Electric",
        Level = "95",
        Moves = new List<string> { "Charge", "Electro Drift", "Power Gem" }
    }
},

// Walking Wake
{
    "Walking Wake",
    new PKMSVShowdownSets
    {
        Name = "Walking Wake",
        Ability = "Protosynthesis",
        TeraType = "Water",
        Level = "95",
        Moves = new List<string> { "Dragon Pulse", "Noble Roar", "Flamethrower" }
    }
},

// Iron Leaves
{
    "Iron Leaves",
    new PKMSVShowdownSets
    {
        Name = "Iron Leaves",
        Ability = "Quark Drive",
        TeraType = "Psychic",
        Level = "95",
        Moves = new List<string> { "Leaf Blade", "Megahorn", "Swords Dance" }
    }
},

// Dipplin
{
    "Dipplin",
    new PKMSVShowdownSets
    {
        Name = "Dipplin",
        Gender = "M",
        Ability = "Gluttony",
        TeraType = "Dragon",
        Level = "95",
        Shiny = "Yes",
        Nature = "Quirky",
        Moves = new List<string> { "Astonish", "Withdraw" }
    }
},

// Poltchageist
{
    "Poltchageist",
    new PKMSVShowdownSets
    {
        Name = "Poltchageist",
        Ability = "Hospitality",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Bashful",
        Moves = new List<string> { "Stun Spore", "Withdraw", "Astonish" }
    }
},

// Poltchageist-Artisan
{
    "Poltchageist-Artisan",
    new PKMSVShowdownSets
    {
        Name = "Poltchageist",
        Form = "Artisan",
        Ability = "Hospitality",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Naughty",
        Moves = new List<string> { "Astonish", "Absorb", "Life Dew", "Foul Play" }
    }
},

// Sinistcha
{
    "Sinistcha",
    new PKMSVShowdownSets
    {
        Name = "Sinistcha",
        Ability = "Hospitality",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Docile",
        Moves = new List<string> { "Stun Spore", "Withdraw", "Astonish" }
    }
},

// Sinistcha-Masterpiece
{
    "Sinistcha-Masterpiece",
    new PKMSVShowdownSets
    {
        Name = "Sinistcha",
        Form = "Masterpiece",
        Ability = "Heatproof",
        TeraType = "Ghost",
        Level = "95",
        Shiny = "Yes",
        Nature = "Sassy",
        Moves = new List<string> { "Energy Ball", "Shadow Ball", "Stun Spore", "Scald" }
    }
},

// Okidogi
{
    "Okidogi",
    new PKMSVShowdownSets
    {
        Name = "Okidogi",
        Gender = "M",
        Ability = "Toxic Chain",
        TeraType = "Poison",
        Level = "95",
        Moves = new List<string> { "Brutal Swing", "Crunch", "Superpower" }
    }
},

// Munkidori
{
    "Munkidori",
    new PKMSVShowdownSets
    {
        Name = "Munkidori",
        Gender = "M",
        Ability = "Toxic Chain",
        TeraType = "Poison",
        Level = "95",
        Moves = new List<string> { "Sludge Wave", "Nasty Plot", "Future Sight" }
    }
},

// Fezandipiti
{
    "Fezandipiti",
    new PKMSVShowdownSets
    {
        Name = "Fezandipiti",
        Gender = "M",
        Ability = "Toxic Chain",
        TeraType = "Poison",
        Level = "95",
        Moves = new List<string> { "Swagger", "Flatter", "Roost" }
    }
},

// Ogerpon
   {
    "Ogerpon",
    new PKMSVShowdownSets
    {
        Name = "Ogerpon",
        Form = "",
        Gender = "F",
        Item = "",
        Ability = "Defiant",
        TeraType = "Grass",
        Level = "95",
        Nature = "Lonely",
        Moves = new List<string> { "Ivy Cudgel", "Low Kick", "Slam", "Grassy Terrain" }
    }
},
   // Ogerpon-Cornerstone
{
    "Ogerpon-Cornerstone",
    new PKMSVShowdownSets
    {
        Name = "Ogerpon",
        Form = "Cornerstone",
        Gender = "F",
        Item = "Cornerstone Mask",
        Ability = "Sturdy",
        TeraType = "Rock",
        Level = "95",
        Shiny = "No",
        Nature = "Lonely",
        Moves = new List<string> { "Ivy Cudgel", "Low Kick", "Slam", "Grassy Terrain" }
    }
},
// Ogerpon-WellSpring
{
    "Ogerpon-Wellspring",
    new PKMSVShowdownSets
    {
        Name = "Ogerpon",
        Form = "Wellspring",
        Gender = "F",
        Item = "Wellspring Mask",
        Ability = "Water Absorb",
        TeraType = "Water",
        Level = "95",
        Nature = "Lonely",
        Moves = new List<string> { "Ivy Cudgel", "Low Kick", "Slam", "Grassy Terrain" }
    }
},
// Ogerpon-Hearthflame
{
    "Ogerpon-Hearthflame",
    new PKMSVShowdownSets
    {
        Name = "Ogerpon",
        Form = "Hearthflame",
        Gender = "F",
        Item = "Hearthflame Mask",
        Ability = "Mold Breaker",
        TeraType = "Fire",
        Level = "95",
        Nature = "Lonely",
        Moves = new List<string> { "Ivy Cudgel", "Low Kick", "Slam", "Grassy Terrain" }
    }
},
    };
};
