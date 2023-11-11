using Discord.Commands;
using System;
using System.Collections.Generic;

namespace Sysbot.Pokemon.Discord;
public class PKMSWSHShowdownSets
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public string HeldItem { get; set; }
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

public class PKMSWSHModule : ModuleBase<SocketCommandContext>
{
    public static Dictionary<string, PKMSWSHShowdownSets> SWSHShowdownSetsCollection = new Dictionary<string, PKMSWSHShowdownSets>(StringComparer.OrdinalIgnoreCase)
    {// Bulbasaur
        {
            "Bulbasaur",
            new PKMSWSHShowdownSets
            {
                Name = "Bulbasaur",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Ivysaur
        {
            "Ivysaur",
            new PKMSWSHShowdownSets
            {
                Name = "Ivysaur",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Venusaur
        {
            "Venusaur",
            new PKMSWSHShowdownSets
            {
                Name = "Venusaur",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Charmander
        {
            "Charmander",
            new PKMSWSHShowdownSets
            {
                Name = "Charmander",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Charmeleon
        {
            "Charmeleon",
            new PKMSWSHShowdownSets
            {
                Name = "Charmeleon",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Charizard
        {
            "Charizard",
            new PKMSWSHShowdownSets
            {
                Name = "Charizard",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Squirtle
        {
            "Squirtle",
            new PKMSWSHShowdownSets
            {
                Name = "Squirtle",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Wartortle
        {
            "Wartortle",
            new PKMSWSHShowdownSets
            {
                Name = "Wartortle",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Blastoise
        {
            "Blastoise",
            new PKMSWSHShowdownSets
            {
                Name = "Blastoise",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Caterpie
        {
            "Caterpie",
            new PKMSWSHShowdownSets
            {
                Name = "Caterpie",
                Gender = "M",
                Ability = "Shield Dust",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Tackle", "String Shot" }
            }
        },

        // Metapod
        {
            "Metapod",
            new PKMSWSHShowdownSets
            {
                Name = "Metapod",
                Gender = "M",
                Ability = "Shed Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Tackle", "String Shot" }
            }
        },

        // Butterfree
        {
            "Butterfree",
            new PKMSWSHShowdownSets
            {
                Name = "Butterfree",
                Gender = "M",
                Ability = "Compound Eyes",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Tackle", "String Shot" }
            }
        },

        // Pikachu
        {
            "Pikachu",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Thunder Shock", "Tail Whip" }
            }
        },

        // Pikachu-Original (M)
        {
            "Pikachu-Original (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "Original (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Pikachu-Hoenn (M)
        {
            "Pikachu-Hoenn (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "Hoenn (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Pikachu-Sinnoh (M)
        {
            "Pikachu-Sinnoh (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "Sinnoh (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Pikachu-Unova (M)
        {
            "Pikachu-Unova (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "Unova (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Pikachu-Kalos (M)
        {
            "Pikachu-Kalos (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "Kalos (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Pikachu-Alola (M)
        {
            "Pikachu-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "Alola (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Pikachu-Partner (M)
        {
            "Pikachu-Partner (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "Partner (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Pikachu-World (M)
        {
            "Pikachu-World (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pikachu",
                Form = "World (M)",
                Ability = "Static",
                Level = "95",
                Shiny = "No",
                Nature = "Hardy",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Iron Tail", "Electroweb" }
            }
        },

        // Raichu
        {
            "Raichu",
            new PKMSWSHShowdownSets
            {
                Name = "Raichu",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Thunder Shock", "Tail Whip" }
            }
        },

        // Raichu-Alola (M)
        {
            "Raichu-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Raichu",
                Form = "Alola (M)",
                Ability = "Surge Surfer",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Thunder Punch", "Swift", "Brick Break", "Charm" }
            }
        },

        // Sandshrew
        {
            "Sandshrew",
            new PKMSWSHShowdownSets
            {
                Name = "Sandshrew",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Scratch", "Defense Curl" }
            }
        },

        // Sandshrew-Alola (M)
        {
            "Sandshrew-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Sandshrew",
                Form = "Alola (M)",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Scratch", "Defense Curl" }
            }
        },

        // Sandslash
        {
            "Sandslash",
            new PKMSWSHShowdownSets
            {
                Name = "Sandslash",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Scratch", "Defense Curl" }
            }
        },

        // Sandslash-Alola (M)
        {
            "Sandslash-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Sandslash",
                Form = "Alola (M)",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Scratch", "Defense Curl" }
            }
        },

        // Nidoran-F (F)
        {
            "Nidoran-F (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Nidoran",
                Form = "F (F)",
                Ability = "Poison Point",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Growl", "Poison Sting" }
            }
        },

        // Nidorina
        {
            "Nidorina",
            new PKMSWSHShowdownSets
            {
                Name = "Nidorina",
                Gender = "F",
                Ability = "Poison Point",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Growl", "Poison Sting" }
            }
        },

        // Nidoqueen
        {
            "Nidoqueen",
            new PKMSWSHShowdownSets
            {
                Name = "Nidoqueen",
                Gender = "F",
                Ability = "Rivalry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Growl", "Poison Sting" }
            }
        },

        // Nidoran-M (M)
        {
            "Nidoran-M (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Nidoran",
                Form = "M (M)",
                Ability = "Poison Point",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Leer", "Poison Sting" }
            }
        },

        // Nidorino
        {
            "Nidorino",
            new PKMSWSHShowdownSets
            {
                Name = "Nidorino",
                Gender = "M",
                Ability = "Rivalry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Leer", "Poison Sting" }
            }
        },

        // Nidoking
        {
            "Nidoking",
            new PKMSWSHShowdownSets
            {
                Name = "Nidoking",
                Gender = "M",
                Ability = "Rivalry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Leer", "Poison Sting" }
            }
        },

        // Clefairy
        {
            "Clefairy",
            new PKMSWSHShowdownSets
            {
                Name = "Clefairy",
                Gender = "M",
                Ability = "Cute Charm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Splash", "Pound", "Copycat" }
            }
        },

        // Clefable
        {
            "Clefable",
            new PKMSWSHShowdownSets
            {
                Name = "Clefable",
                Gender = "M",
                Ability = "Magic Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Splash", "Pound", "Copycat" }
            }
        },

        // Vulpix
        {
            "Vulpix",
            new PKMSWSHShowdownSets
            {
                Name = "Vulpix",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Ember", "Tail Whip" }
            }
        },

        // Vulpix-Alola (M)
        {
            "Vulpix-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Vulpix",
                Form = "Alola (M)",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Powder Snow", "Tail Whip" }
            }
        },

        // Ninetales
        {
            "Ninetales",
            new PKMSWSHShowdownSets
            {
                Name = "Ninetales",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Ember", "Tail Whip" }
            }
        },

        // Ninetales-Alola (M)
        {
            "Ninetales-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Ninetales",
                Form = "Alola (M)",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Powder Snow", "Tail Whip" }
            }
        },

        // Jigglypuff
        {
            "Jigglypuff",
            new PKMSWSHShowdownSets
            {
                Name = "Jigglypuff",
                Gender = "M",
                Ability = "Competitive",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Sing", "Pound", "Copycat" }
            }
        },

        // Wigglytuff
        {
            "Wigglytuff",
            new PKMSWSHShowdownSets
            {
                Name = "Wigglytuff",
                Gender = "M",
                Ability = "Cute Charm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Sing", "Pound", "Copycat" }
            }
        },

        // Zubat
        {
            "Zubat",
            new PKMSWSHShowdownSets
            {
                Name = "Zubat",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Absorb", "Supersonic" }
            }
        },

        // Golbat
        {
            "Golbat",
            new PKMSWSHShowdownSets
            {
                Name = "Golbat",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Absorb", "Supersonic" }
            }
        },

        // Oddish
        {
            "Oddish",
            new PKMSWSHShowdownSets
            {
                Name = "Oddish",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Absorb", "Growth" }
            }
        },

        // Gloom
        {
            "Gloom",
            new PKMSWSHShowdownSets
            {
                Name = "Gloom",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Absorb", "Growth" }
            }
        },

        // Vileplume
        {
            "Vileplume",
            new PKMSWSHShowdownSets
            {
                Name = "Vileplume",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Absorb", "Growth" }
            }
        },

        // Diglett
        {
            "Diglett",
            new PKMSWSHShowdownSets
            {
                Name = "Diglett",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Sand Attack", "Scratch" }
            }
        },

        // Diglett-Alola (M)
        {
            "Diglett-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Diglett",
                Form = "Alola (M)",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Sand Attack", "Metal Claw" }
            }
        },

        // Dugtrio
        {
            "Dugtrio",
            new PKMSWSHShowdownSets
            {
                Name = "Dugtrio",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Sand Attack", "Scratch" }
            }
        },

        // Dugtrio-Alola (M)
        {
            "Dugtrio-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Dugtrio",
                Form = "Alola (M)",
                Ability = "Tangling Hair",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Sand Attack", "Metal Claw" }
            }
        },

        // Meowth
        {
            "Meowth",
            new PKMSWSHShowdownSets
            {
                Name = "Meowth",
                Gender = "M",
                Ability = "Technician",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Fake Out", "Growl" }
            }
        },

        // Meowth-Alola (M)
        {
            "Meowth-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Meowth",
                Form = "Alola (M)",
                Ability = "Technician",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Fake Out", "Growl" }
            }
        },

        // Meowth-Galar (M)
        {
            "Meowth-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Meowth",
                Form = "Galar (M)",
                Ability = "Tough Claws",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Fake Out", "Growl" }
            }
        },

        // Persian
        {
            "Persian",
            new PKMSWSHShowdownSets
            {
                Name = "Persian",
                Gender = "M",
                Ability = "Limber",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Fake Out", "Growl" }
            }
        },

        // Persian-Alola (M)
        {
            "Persian-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Persian",
                Form = "Alola (M)",
                Ability = "Technician",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Fake Out", "Growl" }
            }
        },

        // Psyduck
        {
            "Psyduck",
            new PKMSWSHShowdownSets
            {
                Name = "Psyduck",
                Gender = "M",
                Ability = "Cloud Nine",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Scratch", "Tail Whip" }
            }
        },

        // Golduck
        {
            "Golduck",
            new PKMSWSHShowdownSets
            {
                Name = "Golduck",
                Gender = "M",
                Ability = "Damp",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Scratch", "Tail Whip" }
            }
        },

        // Growlithe
        {
            "Growlithe",
            new PKMSWSHShowdownSets
            {
                Name = "Growlithe",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Ember", "Leer" }
            }
        },

        // Arcanine
        {
            "Arcanine",
            new PKMSWSHShowdownSets
            {
                Name = "Arcanine",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Ember", "Leer" }
            }
        },

        // Poliwag
        {
            "Poliwag",
            new PKMSWSHShowdownSets
            {
                Name = "Poliwag",
                Gender = "M",
                Ability = "Damp",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Water Gun", "Hypnosis" }
            }
        },

        // Poliwhirl
        {
            "Poliwhirl",
            new PKMSWSHShowdownSets
            {
                Name = "Poliwhirl",
                Gender = "M",
                Ability = "Damp",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Water Gun", "Hypnosis" }
            }
        },

        // Poliwrath
        {
            "Poliwrath",
            new PKMSWSHShowdownSets
            {
                Name = "Poliwrath",
                Gender = "M",
                Ability = "Damp",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Water Gun", "Hypnosis" }
            }
        },

        // Abra
        {
            "Abra",
            new PKMSWSHShowdownSets
            {
                Name = "Abra",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Teleport" }
            }
        },

        // Kadabra
        {
            "Kadabra",
            new PKMSWSHShowdownSets
            {
                Name = "Kadabra",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Teleport" }
            }
        },

        // Alakazam
        {
            "Alakazam",
            new PKMSWSHShowdownSets
            {
                Name = "Alakazam",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Teleport" }
            }
        },

        // Machop
        {
            "Machop",
            new PKMSWSHShowdownSets
            {
                Name = "Machop",
                Gender = "M",
                Ability = "No Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Low Kick", "Leer" }
            }
        },

        // Machoke
        {
            "Machoke",
            new PKMSWSHShowdownSets
            {
                Name = "Machoke",
                Gender = "M",
                Ability = "No Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Low Kick", "Leer" }
            }
        },

        // Machamp
        {
            "Machamp",
            new PKMSWSHShowdownSets
            {
                Name = "Machamp",
                Gender = "M",
                Ability = "Guts",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Low Kick", "Leer" }
            }
        },

        // Tentacool
        {
            "Tentacool",
            new PKMSWSHShowdownSets
            {
                Name = "Tentacool",
                Gender = "M",
                Ability = "Liquid Ooze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Poison Sting", "Water Gun" }
            }
        },

        // Tentacruel
        {
            "Tentacruel",
            new PKMSWSHShowdownSets
            {
                Name = "Tentacruel",
                Gender = "M",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Poison Sting", "Water Gun" }
            }
        },

        // Ponyta
        {
            "Ponyta",
            new PKMSWSHShowdownSets
            {
                Name = "Ponyta",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Ponyta-Galar (M)
        {
            "Ponyta-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Ponyta",
                Form = "Galar (M)",
                Ability = "Pastel Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Rapidash
        {
            "Rapidash",
            new PKMSWSHShowdownSets
            {
                Name = "Rapidash",
                Gender = "M",
                Ability = "Run Away",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Rapidash-Galar (M)
        {
            "Rapidash-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Rapidash",
                Form = "Galar (M)",
                Ability = "Pastel Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Slowpoke
        {
            "Slowpoke",
            new PKMSWSHShowdownSets
            {
                Name = "Slowpoke",
                Gender = "M",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Curse" }
            }
        },

        // Slowpoke-Galar (M)
        {
            "Slowpoke-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Slowpoke",
                Form = "Galar (M)",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Tackle", "Curse" }
            }
        },

        // Slowbro
        {
            "Slowbro",
            new PKMSWSHShowdownSets
            {
                Name = "Slowbro",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Tackle", "Curse" }
            }
        },

        // Slowbro-Galar (M)
        {
            "Slowbro-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Slowbro",
                Form = "Galar (M)",
                Ability = "Quick Draw",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Curse" }
            }
        },

        // Magnemite
        {
            "Magnemite",
            new PKMSWSHShowdownSets
            {
                Name = "Magnemite",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Thunder Shock", "Tackle" }
            }
        },

        // Magneton
        {
            "Magneton",
            new PKMSWSHShowdownSets
            {
                Name = "Magneton",
                Ability = "Magnet Pull",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Thunder Shock", "Tackle" }
            }
        },

        // Farfetch’d
        {
            "Farfetch’d",
            new PKMSWSHShowdownSets
            {
                Name = "Farfetch’d",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Peck", "Sand Attack" }
            }
        },

        // Farfetch’d-Galar (M)
        {
            "Farfetch’d-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Farfetch’d",
                Form = "Galar (M)",
                Ability = "Steadfast",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Peck", "Sand Attack" }
            }
        },

        // Shellder
        {
            "Shellder",
            new PKMSWSHShowdownSets
            {
                Name = "Shellder",
                Gender = "M",
                Ability = "Skill Link",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Tackle", "Water Gun" }
            }
        },

        // Cloyster
        {
            "Cloyster",
            new PKMSWSHShowdownSets
            {
                Name = "Cloyster",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Tackle", "Water Gun" }
            }
        },

        // Gastly
        {
            "Gastly",
            new PKMSWSHShowdownSets
            {
                Name = "Gastly",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Lick", "Confuse Ray" }
            }
        },

        // Haunter
        {
            "Haunter",
            new PKMSWSHShowdownSets
            {
                Name = "Haunter",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Lick", "Confuse Ray" }
            }
        },

        // Gengar
        {
            "Gengar",
            new PKMSWSHShowdownSets
            {
                Name = "Gengar",
                Gender = "M",
                Ability = "Cursed Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Lick", "Confuse Ray" }
            }
        },

        // Onix
        {
            "Onix",
            new PKMSWSHShowdownSets
            {
                Name = "Onix",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Harden", "Bind", "Rock Throw" }
            }
        },

        // Krabby
        {
            "Krabby",
            new PKMSWSHShowdownSets
            {
                Name = "Krabby",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Water Gun", "Leer" }
            }
        },

        // Kingler
        {
            "Kingler",
            new PKMSWSHShowdownSets
            {
                Name = "Kingler",
                Gender = "M",
                Ability = "Hyper Cutter",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Water Gun", "Leer" }
            }
        },

        // Exeggcute
        {
            "Exeggcute",
            new PKMSWSHShowdownSets
            {
                Name = "Exeggcute",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Absorb", "Hypnosis" }
            }
        },

        // Exeggutor
        {
            "Exeggutor",
            new PKMSWSHShowdownSets
            {
                Name = "Exeggutor",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Absorb", "Hypnosis" }
            }
        },

        // Exeggutor-Alola (M)
        {
            "Exeggutor-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Exeggutor",
                Form = "Alola (M)",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Absorb", "Hypnosis", "Reflect", "Leech Seed" }
            }
        },

        // Cubone
        {
            "Cubone",
            new PKMSWSHShowdownSets
            {
                Name = "Cubone",
                Gender = "M",
                Ability = "Lightning Rod",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Mud-Slap", "Growl" }
            }
        },

        // Marowak
        {
            "Marowak",
            new PKMSWSHShowdownSets
            {
                Name = "Marowak",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Mud-Slap", "Growl" }
            }
        },

        // Marowak-Alola (M)
        {
            "Marowak-Alola (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Marowak",
                Form = "Alola (M)",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Flare Blitz", "Shadow Bone", "Will-O-Wisp", "Iron Head" }
            }
        },

        // Hitmonlee
        {
            "Hitmonlee",
            new PKMSWSHShowdownSets
            {
                Name = "Hitmonlee",
                Gender = "M",
                Ability = "Reckless",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Helping Hand", "Fake Out", "Focus Energy" }
            }
        },

        // Hitmonchan
        {
            "Hitmonchan",
            new PKMSWSHShowdownSets
            {
                Name = "Hitmonchan",
                Gender = "M",
                Ability = "Iron Fist",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Tackle", "Helping Hand", "Fake Out", "Focus Energy" }
            }
        },

        // Lickitung
        {
            "Lickitung",
            new PKMSWSHShowdownSets
            {
                Name = "Lickitung",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Lick", "Defense Curl" }
            }
        },

        // Koffing
        {
            "Koffing",
            new PKMSWSHShowdownSets
            {
                Name = "Koffing",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Poison Gas", "Tackle" }
            }
        },

        // Weezing
        {
            "Weezing",
            new PKMSWSHShowdownSets
            {
                Name = "Weezing",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Poison Gas", "Tackle" }
            }
        },

        // Weezing-Galar (M)
        {
            "Weezing-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Weezing",
                Form = "Galar (M)",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Poison Gas", "Tackle" }
            }
        },

        // Rhyhorn
        {
            "Rhyhorn",
            new PKMSWSHShowdownSets
            {
                Name = "Rhyhorn",
                Gender = "M",
                Ability = "Lightning Rod",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Rhydon
        {
            "Rhydon",
            new PKMSWSHShowdownSets
            {
                Name = "Rhydon",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Chansey
        {
            "Chansey",
            new PKMSWSHShowdownSets
            {
                Name = "Chansey",
                Gender = "F",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Minimize", "Pound", "Copycat" }
            }
        },

        // Tangela
        {
            "Tangela",
            new PKMSWSHShowdownSets
            {
                Name = "Tangela",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Absorb", "Bind" }
            }
        },

        // Kangaskhan
        {
            "Kangaskhan",
            new PKMSWSHShowdownSets
            {
                Name = "Kangaskhan",
                Gender = "F",
                Ability = "Scrappy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Pound", "Tail Whip" }
            }
        },

        // Horsea
        {
            "Horsea",
            new PKMSWSHShowdownSets
            {
                Name = "Horsea",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Water Gun", "Leer" }
            }
        },

        // Seadra
        {
            "Seadra",
            new PKMSWSHShowdownSets
            {
                Name = "Seadra",
                Gender = "M",
                Ability = "Sniper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Water Gun", "Leer" }
            }
        },

        // Goldeen
        {
            "Goldeen",
            new PKMSWSHShowdownSets
            {
                Name = "Goldeen",
                Gender = "M",
                Ability = "Water Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Peck", "Tail Whip" }
            }
        },

        // Seaking
        {
            "Seaking",
            new PKMSWSHShowdownSets
            {
                Name = "Seaking",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Peck", "Tail Whip" }
            }
        },

        // Staryu
        {
            "Staryu",
            new PKMSWSHShowdownSets
            {
                Name = "Staryu",
                Ability = "Illuminate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Starmie
        {
            "Starmie",
            new PKMSWSHShowdownSets
            {
                Name = "Starmie",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Mr. Mime
        {
            "Mr. Mime",
            new PKMSWSHShowdownSets
            {
                Name = "Mr. Mime",
                Gender = "M",
                Ability = "Filter",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Pound", "Copycat" }
            }
        },

        // Mr. Mime-Galar (M)
        {
            "Mr. Mime-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Mr. Mime",
                Form = "Galar (M)",
                Ability = "Vital Spirit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Pound", "Copycat" }
            }
        },

        // Scyther
        {
            "Scyther",
            new PKMSWSHShowdownSets
            {
                Name = "Scyther",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Jynx
        {
            "Jynx",
            new PKMSWSHShowdownSets
            {
                Name = "Jynx",
                Gender = "F",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Lick", "Pound" }
            }
        },

        // Electabuzz
        {
            "Electabuzz",
            new PKMSWSHShowdownSets
            {
                Name = "Electabuzz",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Magmar
        {
            "Magmar",
            new PKMSWSHShowdownSets
            {
                Name = "Magmar",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Smog", "Leer" }
            }
        },

        // Pinsir
        {
            "Pinsir",
            new PKMSWSHShowdownSets
            {
                Name = "Pinsir",
                Gender = "M",
                Ability = "Hyper Cutter",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Vise Grip", "Harden" }
            }
        },        
      
        // Crobat
        {
            "Crobat",
            new PKMSWSHShowdownSets
            {
                Name = "Crobat",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Absorb", "Supersonic" }
            }
        },

        // Tauros
        {
            "Tauros",
            new PKMSWSHShowdownSets
            {
                Name = "Tauros",
                Gender = "M",
                Ability = "Anger Point",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Magikarp
        {
            "Magikarp",
            new PKMSWSHShowdownSets
            {
                Name = "Magikarp",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Splash" }
            }
        },

        // Gyarados
        {
            "Gyarados",
            new PKMSWSHShowdownSets
            {
                Name = "Gyarados",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Splash" }
            }
        },

        // Lapras
        {
            "Lapras",
            new PKMSWSHShowdownSets
            {
                Name = "Lapras",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Growl", "Water Gun" }
            }
        },

        // Ditto
        {
            "Ditto",
            new PKMSWSHShowdownSets
            {
                Name = "Ditto",
                Ability = "Limber",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Transform" }
            }
        },

        // Eevee
        {
            "Eevee",
            new PKMSWSHShowdownSets
            {
                Name = "Eevee",
                Gender = "M",
                Ability = "Run Away",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Vaporeon
        {
            "Vaporeon",
            new PKMSWSHShowdownSets
            {
                Name = "Vaporeon",
                Gender = "M",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Jolteon
        {
            "Jolteon",
            new PKMSWSHShowdownSets
            {
                Name = "Jolteon",
                Gender = "M",
                Ability = "Volt Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Flareon
        {
            "Flareon",
            new PKMSWSHShowdownSets
            {
                Name = "Flareon",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Porygon
        {
            "Porygon",
            new PKMSWSHShowdownSets
            {
                Name = "Porygon",
                Ability = "Trace",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Tackle", "Conversion" }
            }
        },

        // Omanyte
        {
            "Omanyte",
            new PKMSWSHShowdownSets
            {
                Name = "Omanyte",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Bind", "Withdraw" }
            }
        },

        // Omastar
        {
            "Omastar",
            new PKMSWSHShowdownSets
            {
                Name = "Omastar",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Bind", "Withdraw" }
            }
        },

        // Kabuto
        {
            "Kabuto",
            new PKMSWSHShowdownSets
            {
                Name = "Kabuto",
                Gender = "M",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Absorb", "Harden" }
            }
        },

        // Kabutops
        {
            "Kabutops",
            new PKMSWSHShowdownSets
            {
                Name = "Kabutops",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Absorb", "Harden" }
            }
        },

        // Aerodactyl
        {
            "Aerodactyl",
            new PKMSWSHShowdownSets
            {
                Name = "Aerodactyl",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Bite", "Ancient Power" }
            }
        },

        // Snorlax
        {
            "Snorlax",
            new PKMSWSHShowdownSets
            {
                Name = "Snorlax",
                Gender = "M",
                Ability = "Immunity",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Lick", "Tackle" }
            }
        },

        // Articuno
        {
            "Articuno",
            new PKMSWSHShowdownSets
            {
                Name = "Articuno",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Ice Beam", "Freeze-Dry", "Hurricane", "Mist" }
            }
        },

        // Articuno-Galar
        {
            "Articuno-Galar",
            new PKMSWSHShowdownSets
            {
                Name = "Articuno",
                Form = "Galar",
                Ability = "Competitive",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Freezing Glare", "Hurricane", "Psycho Cut", "Psycho Shift" }
            }
        },

        // Zapdos
        {
            "Zapdos",
            new PKMSWSHShowdownSets
            {
                Name = "Zapdos",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Thunder", "Drill Peck", "Brave Bird", "Agility" }
            }
        },

        // Zapdos-Galar
        {
            "Zapdos-Galar",
            new PKMSWSHShowdownSets
            {
                Name = "Zapdos",
                Form = "Galar",
                Ability = "Defiant",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Thunderous Kick", "Drill Peck", "Reversal", "Focus Energy" }
            }
        },

        // Moltres
        {
            "Moltres",
            new PKMSWSHShowdownSets
            {
                Name = "Moltres",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Heat Wave", "Wing Attack", "Leer", "Fire Spin" }
            }
        },

        // Moltres-Galar
        {
            "Moltres-Galar",
            new PKMSWSHShowdownSets
            {
                Name = "Moltres",
                Form = "Galar",
                Ability = "Berserk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Fiery Wrath", "Hurricane", "Sucker Punch", "Nasty Plot" }
            }
        },

        // Dratini
        {
            "Dratini",
            new PKMSWSHShowdownSets
            {
                Name = "Dratini",
                Gender = "M",
                Ability = "Shed Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Wrap", "Leer" }
            }
        },

        // Dragonair
        {
            "Dragonair",
            new PKMSWSHShowdownSets
            {
                Name = "Dragonair",
                Gender = "M",
                Ability = "Shed Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Wrap", "Leer" }
            }
        },

        // Dragonite
        {
            "Dragonite",
            new PKMSWSHShowdownSets
            {
                Name = "Dragonite",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Wrap", "Leer" }
            }
        },

        // Mewtwo
        {
            "Mewtwo",
            new PKMSWSHShowdownSets
            {
                Name = "Mewtwo",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Psychic", "Disable", "Recover", "Blizzard" }
            }
        },

        // Mew
        {
            "Mew",
            new PKMSWSHShowdownSets
            {
                Name = "Mew",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "No",
                Nature = "Careful",
                Moves = new List<string> { "Pound" }
            }
        },

        // Hoothoot
        {
            "Hoothoot",
            new PKMSWSHShowdownSets
            {
                Name = "Hoothoot",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Peck", "Growl" }
            }
        },

        // Noctowl
        {
            "Noctowl",
            new PKMSWSHShowdownSets
            {
                Name = "Noctowl",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Peck", "Growl" }
            }
        },      

        // Chinchou
        {
            "Chinchou",
            new PKMSWSHShowdownSets
            {
                Name = "Chinchou",
                Gender = "M",
                Ability = "Illuminate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Water Gun", "Supersonic" }
            }
        },

        // Lanturn
        {
            "Lanturn",
            new PKMSWSHShowdownSets
            {
                Name = "Lanturn",
                Gender = "M",
                Ability = "Illuminate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Water Gun", "Supersonic" }
            }
        },

        // Pichu
        {
            "Pichu",
            new PKMSWSHShowdownSets
            {
                Name = "Pichu",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Thunder Shock", "Tail Whip" }
            }
        },

        // Cleffa
        {
            "Cleffa",
            new PKMSWSHShowdownSets
            {
                Name = "Cleffa",
                Gender = "M",
                Ability = "Cute Charm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Splash", "Pound", "Copycat" }
            }
        },

        // Igglybuff
        {
            "Igglybuff",
            new PKMSWSHShowdownSets
            {
                Name = "Igglybuff",
                Gender = "M",
                Ability = "Competitive",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Sing", "Pound", "Copycat" }
            }
        },

        // Togepi
        {
            "Togepi",
            new PKMSWSHShowdownSets
            {
                Name = "Togepi",
                Gender = "M",
                Ability = "Serene Grace",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Growl", "Pound" }
            }
        },

        // Togetic
        {
            "Togetic",
            new PKMSWSHShowdownSets
            {
                Name = "Togetic",
                Gender = "M",
                Ability = "Serene Grace",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Growl", "Pound" }
            }
        },

        // Natu
        {
            "Natu",
            new PKMSWSHShowdownSets
            {
                Name = "Natu",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Xatu
        {
            "Xatu",
            new PKMSWSHShowdownSets
            {
                Name = "Xatu",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Bellossom
        {
            "Bellossom",
            new PKMSWSHShowdownSets
            {
                Name = "Bellossom",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Absorb", "Growth" }
            }
        },

        // Marill
        {
            "Marill",
            new PKMSWSHShowdownSets
            {
                Name = "Marill",
                Gender = "M",
                Ability = "Huge Power",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Splash", "Water Gun", "Tail Whip" }
            }
        },

        // Azumarill
        {
            "Azumarill",
            new PKMSWSHShowdownSets
            {
                Name = "Azumarill",
                Gender = "M",
                Ability = "Thick Fat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Splash", "Water Gun", "Tail Whip" }
            }
        },

        // Sudowoodo
        {
            "Sudowoodo",
            new PKMSWSHShowdownSets
            {
                Name = "Sudowoodo",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Fake Tears", "Copycat" }
            }
        },

        // Politoed
        {
            "Politoed",
            new PKMSWSHShowdownSets
            {
                Name = "Politoed",
                Gender = "M",
                Ability = "Damp",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Water Gun", "Hypnosis" }
            }
        },

        // Wooper
        {
            "Wooper",
            new PKMSWSHShowdownSets
            {
                Name = "Wooper",
                Gender = "M",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Water Gun", "Tail Whip" }
            }
        },

        // Quagsire
        {
            "Quagsire",
            new PKMSWSHShowdownSets
            {
                Name = "Quagsire",
                Gender = "M",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Water Gun", "Tail Whip" }
            }
        },

        // Espeon
        {
            "Espeon",
            new PKMSWSHShowdownSets
            {
                Name = "Espeon",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Umbreon
        {
            "Umbreon",
            new PKMSWSHShowdownSets
            {
                Name = "Umbreon",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Slowking
        {
            "Slowking",
            new PKMSWSHShowdownSets
            {
                Name = "Slowking",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Tackle", "Curse" }
            }
        },

        // Slowking-Galar (M)
        {
            "Slowking-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Slowking",
                Form = "Galar (M)",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Curse" }
            }
        },

        // Wobbuffet
        {
            "Wobbuffet",
            new PKMSWSHShowdownSets
            {
                Name = "Wobbuffet",
                Gender = "M",
                Ability = "Shadow Tag",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Splash", "Charm", "Encore", "Amnesia" }
            }
        },

        // Dunsparce
        {
            "Dunsparce",
            new PKMSWSHShowdownSets
            {
                Name = "Dunsparce",
                Gender = "M",
                Ability = "Run Away",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Flail", "Defense Curl" }
            }
        },

        // Steelix
        {
            "Steelix",
            new PKMSWSHShowdownSets
            {
                Name = "Steelix",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Harden", "Bind", "Rock Throw" }
            }
        },

        // Qwilfish
        {
            "Qwilfish",
            new PKMSWSHShowdownSets
            {
                Name = "Qwilfish",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Poison Sting", "Tackle" }
            }
        },

        // Scizor
        {
            "Scizor",
            new PKMSWSHShowdownSets
            {
                Name = "Scizor",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Shuckle
        {
            "Shuckle",
            new PKMSWSHShowdownSets
            {
                Name = "Shuckle",
                Gender = "M",
                Ability = "Gluttony",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Withdraw", "Wrap" }
            }
        },

        // Heracross
        {
            "Heracross",
            new PKMSWSHShowdownSets
            {
                Name = "Heracross",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Sneasel
        {
            "Sneasel",
            new PKMSWSHShowdownSets
            {
                Name = "Sneasel",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Swinub
        {
            "Swinub",
            new PKMSWSHShowdownSets
            {
                Name = "Swinub",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Tackle", "Mud-Slap" }
            }
        },

        // Piloswine
        {
            "Piloswine",
            new PKMSWSHShowdownSets
            {
                Name = "Piloswine",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Tackle", "Mud-Slap" }
            }
        },

        // Corsola
        {
            "Corsola",
            new PKMSWSHShowdownSets
            {
                Name = "Corsola",
                Gender = "M",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Corsola-Galar (M)
        {
            "Corsola-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Corsola",
                Form = "Galar (M)",
                Ability = "Weak Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Remoraid
        {
            "Remoraid",
            new PKMSWSHShowdownSets
            {
                Name = "Remoraid",
                Gender = "M",
                Ability = "Sniper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Water Gun", "Helping Hand" }
            }
        },

        // Octillery
        {
            "Octillery",
            new PKMSWSHShowdownSets
            {
                Name = "Octillery",
                Gender = "M",
                Ability = "Suction Cups",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Water Gun", "Helping Hand" }
            }
        },

        // Delibird
        {
            "Delibird",
            new PKMSWSHShowdownSets
            {
                Name = "Delibird",
                Gender = "M",
                Ability = "Vital Spirit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Present" }
            }
        },

        // Mantine
        {
            "Mantine",
            new PKMSWSHShowdownSets
            {
                Name = "Mantine",
                Gender = "M",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Water Gun" }
            }
        },

        // Skarmory
        {
            "Skarmory",
            new PKMSWSHShowdownSets
            {
                Name = "Skarmory",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Kingdra
        {
            "Kingdra",
            new PKMSWSHShowdownSets
            {
                Name = "Kingdra",
                Gender = "M",
                Ability = "Sniper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Water Gun", "Leer" }
            }
        },

        // Porygon2
        {
            "Porygon2",
            new PKMSWSHShowdownSets
            {
                Name = "Porygon2",
                Ability = "Download",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Tackle", "Conversion" }
            }
        },

        // Tyrogue
        {
            "Tyrogue",
            new PKMSWSHShowdownSets
            {
                Name = "Tyrogue",
                Gender = "M",
                Ability = "Guts",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Helping Hand", "Fake Out", "Focus Energy" }
            }
        },

        // Hitmontop
        {
            "Hitmontop",
            new PKMSWSHShowdownSets
            {
                Name = "Hitmontop",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Helping Hand", "Fake Out", "Focus Energy" }
            }
        },

        // Smoochum
        {
            "Smoochum",
            new PKMSWSHShowdownSets
            {
                Name = "Smoochum",
                Gender = "F",
                Ability = "Forewarn",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Lick", "Pound" }
            }
        },

        // Elekid
        {
            "Elekid",
            new PKMSWSHShowdownSets
            {
                Name = "Elekid",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Magby
        {
            "Magby",
            new PKMSWSHShowdownSets
            {
                Name = "Magby",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Smog", "Leer" }
            }
        },

        // Miltank
        {
            "Miltank",
            new PKMSWSHShowdownSets
            {
                Name = "Miltank",
                Gender = "F",
                Ability = "Scrappy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Blissey
        {
            "Blissey",
            new PKMSWSHShowdownSets
            {
                Name = "Blissey",
                Gender = "F",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Minimize", "Pound", "Copycat" }
            }
        },

        // Raikou
        {
            "Raikou",
            new PKMSWSHShowdownSets
            {
                Name = "Raikou",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Thunderbolt", "Howl", "Extreme Speed", "Weather Ball" }
            }
        },

        // Entei
        {
            "Entei",
            new PKMSWSHShowdownSets
            {
                Name = "Entei",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Flamethrower", "Scary Face", "Extreme Speed", "Crunch" }
            }
        },

        // Suicune
        {
            "Suicune",
            new PKMSWSHShowdownSets
            {
                Name = "Suicune",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Liquidation", "Extrasensory", "Extreme Speed", "Calm Mind" }
            }
        },

        // Larvitar
        {
            "Larvitar",
            new PKMSWSHShowdownSets
            {
                Name = "Larvitar",
                Gender = "M",
                Ability = "Guts",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Pupitar
        {
            "Pupitar",
            new PKMSWSHShowdownSets
            {
                Name = "Pupitar",
                Gender = "M",
                Ability = "Shed Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Tyranitar
        {
            "Tyranitar",
            new PKMSWSHShowdownSets
            {
                Name = "Tyranitar",
                Gender = "M",
                Ability = "Sand Stream",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Lugia
        {
            "Lugia",
            new PKMSWSHShowdownSets
            {
                Name = "Lugia",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Dragon Pulse", "Extrasensory", "Whirlpool", "Ancient Power" }
            }
        },

        // Ho-Oh
        {
            "Ho-Oh",
            new PKMSWSHShowdownSets
            {
                Name = "Ho",
                Form = "Oh",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Flare Blitz", "Extrasensory", "Sunny Day", "Ancient Power" }
            }
        },

        // Celebi
        {
            "Celebi",
            new PKMSWSHShowdownSets
            {
                Name = "Celebi",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Magical Leaf", "Future Sight", "Life Dew", "Heal Bell" }
            }
        },

        // Treecko
        {
            "Treecko",
            new PKMSWSHShowdownSets
            {
                Name = "Treecko",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Pound", "Leer" }
            }
        },

        // Grovyle
        {
            "Grovyle",
            new PKMSWSHShowdownSets
            {
                Name = "Grovyle",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Pound", "Leer" }
            }
        },

        // Sceptile
        {
            "Sceptile",
            new PKMSWSHShowdownSets
            {
                Name = "Sceptile",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Pound", "Leer" }
            }
        },

        // Torchic
        {
            "Torchic",
            new PKMSWSHShowdownSets
            {
                Name = "Torchic",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Combusken
        {
            "Combusken",
            new PKMSWSHShowdownSets
            {
                Name = "Combusken",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Blaziken
        {
            "Blaziken",
            new PKMSWSHShowdownSets
            {
                Name = "Blaziken",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Mudkip
        {
            "Mudkip",
            new PKMSWSHShowdownSets
            {
                Name = "Mudkip",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Marshtomp
        {
            "Marshtomp",
            new PKMSWSHShowdownSets
            {
                Name = "Marshtomp",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Swampert
        {
            "Swampert",
            new PKMSWSHShowdownSets
            {
                Name = "Swampert",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Zigzagoon
        {
            "Zigzagoon",
            new PKMSWSHShowdownSets
            {
                Name = "Zigzagoon",
                Gender = "M",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Zigzagoon-Galar (M)
        {
            "Zigzagoon-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Zigzagoon",
                Form = "Galar (M)",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Linoone
        {
            "Linoone",
            new PKMSWSHShowdownSets
            {
                Name = "Linoone",
                Gender = "M",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Linoone-Galar (M)
        {
            "Linoone-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Linoone",
                Form = "Galar (M)",
                Ability = "Gluttony",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Lotad
        {
            "Lotad",
            new PKMSWSHShowdownSets
            {
                Name = "Lotad",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Astonish", "Growl" }
            }
        },

        // Lombre
        {
            "Lombre",
            new PKMSWSHShowdownSets
            {
                Name = "Lombre",
                Gender = "M",
                Ability = "Rain Dish",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Astonish", "Growl" }
            }
        },

        // Ludicolo
        {
            "Ludicolo",
            new PKMSWSHShowdownSets
            {
                Name = "Ludicolo",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Astonish", "Growl" }
            }
        },

        // Seedot
        {
            "Seedot",
            new PKMSWSHShowdownSets
            {
                Name = "Seedot",
                Gender = "M",
                Ability = "Early Bird",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Nuzleaf
        {
            "Nuzleaf",
            new PKMSWSHShowdownSets
            {
                Name = "Nuzleaf",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Shiftry
        {
            "Shiftry",
            new PKMSWSHShowdownSets
            {
                Name = "Shiftry",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Wingull
        {
            "Wingull",
            new PKMSWSHShowdownSets
            {
                Name = "Wingull",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Growl", "Water Gun" }
            }
        },

        // Pelipper
        {
            "Pelipper",
            new PKMSWSHShowdownSets
            {
                Name = "Pelipper",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Growl", "Water Gun" }
            }
        },

        // Ralts
        {
            "Ralts",
            new PKMSWSHShowdownSets
            {
                Name = "Ralts",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Growl", "Disarming Voice" }
            }
        },

        // Kirlia
        {
            "Kirlia",
            new PKMSWSHShowdownSets
            {
                Name = "Kirlia",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Growl", "Disarming Voice" }
            }
        },

        // Gardevoir
        {
            "Gardevoir",
            new PKMSWSHShowdownSets
            {
                Name = "Gardevoir",
                Gender = "M",
                Ability = "Trace",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Growl", "Disarming Voice" }
            }
        },

        // Nincada
        {
            "Nincada",
            new PKMSWSHShowdownSets
            {
                Name = "Nincada",
                Gender = "M",
                Ability = "Compound Eyes",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Sand Attack", "Scratch" }
            }
        },

        // Ninjask
        {
            "Ninjask",
            new PKMSWSHShowdownSets
            {
                Name = "Ninjask",
                Gender = "M",
                Ability = "Speed Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Sand Attack", "Scratch" }
            }
        },

        // Shedinja
        {
            "Shedinja",
            new PKMSWSHShowdownSets
            {
                Name = "Shedinja",
                Ability = "Wonder Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Sand Attack", "Scratch" }
            }
        },

        // Whismur
        {
            "Whismur",
            new PKMSWSHShowdownSets
            {
                Name = "Whismur",
                Gender = "M",
                Ability = "Soundproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Astonish", "Pound" }
            }
        },

        // Loudred
        {
            "Loudred",
            new PKMSWSHShowdownSets
            {
                Name = "Loudred",
                Gender = "M",
                Ability = "Soundproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Astonish", "Pound" }
            }
        },

        // Exploud
        {
            "Exploud",
            new PKMSWSHShowdownSets
            {
                Name = "Exploud",
                Gender = "M",
                Ability = "Soundproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Astonish", "Pound" }
            }
        },

        // Azurill
        {
            "Azurill",
            new PKMSWSHShowdownSets
            {
                Name = "Azurill",
                Gender = "M",
                Ability = "Thick Fat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Splash", "Water Gun", "Tail Whip" }
            }
        },

        // Sableye
        {
            "Sableye",
            new PKMSWSHShowdownSets
            {
                Name = "Sableye",
                Gender = "M",
                Ability = "Stall",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Leer", "Scratch" }
            }
        },

        // Mawile
        {
            "Mawile",
            new PKMSWSHShowdownSets
            {
                Name = "Mawile",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Astonish", "Growl" }
            }
        },

        // Aron
        {
            "Aron",
            new PKMSWSHShowdownSets
            {
                Name = "Aron",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Lairon
        {
            "Lairon",
            new PKMSWSHShowdownSets
            {
                Name = "Lairon",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Aggron
        {
            "Aggron",
            new PKMSWSHShowdownSets
            {
                Name = "Aggron",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Electrike
        {
            "Electrike",
            new PKMSWSHShowdownSets
            {
                Name = "Electrike",
                Gender = "M",
                Ability = "Lightning Rod",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Tackle", "Thunder Wave" }
            }
        },

        // Manectric
        {
            "Manectric",
            new PKMSWSHShowdownSets
            {
                Name = "Manectric",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Thunder Wave" }
            }
        },

        // Roselia
        {
            "Roselia",
            new PKMSWSHShowdownSets
            {
                Name = "Roselia",
                Gender = "M",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Absorb", "Growth", "Stun Spore", "Worry Seed" }
            }
        },

        // Carvanha
        {
            "Carvanha",
            new PKMSWSHShowdownSets
            {
                Name = "Carvanha",
                Gender = "M",
                Ability = "Rough Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Aqua Jet", "Leer" }
            }
        },

        // Sharpedo
        {
            "Sharpedo",
            new PKMSWSHShowdownSets
            {
                Name = "Sharpedo",
                Gender = "M",
                Ability = "Rough Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Aqua Jet", "Leer" }
            }
        },

        // Wailmer
        {
            "Wailmer",
            new PKMSWSHShowdownSets
            {
                Name = "Wailmer",
                Gender = "M",
                Ability = "Water Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Splash" }
            }
        },

        // Wailord
        {
            "Wailord",
            new PKMSWSHShowdownSets
            {
                Name = "Wailord",
                Gender = "M",
                Ability = "Water Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Splash" }
            }
        },

        // Torkoal
        {
            "Torkoal",
            new PKMSWSHShowdownSets
            {
                Name = "Torkoal",
                Gender = "M",
                Ability = "Drought",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Smog", "Ember" }
            }
        },

        // Trapinch
        {
            "Trapinch",
            new PKMSWSHShowdownSets
            {
                Name = "Trapinch",
                Gender = "M",
                Ability = "Arena Trap",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Sand Attack", "Astonish" }
            }
        },

        // Vibrava
        {
            "Vibrava",
            new PKMSWSHShowdownSets
            {
                Name = "Vibrava",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Sand Attack", "Astonish" }
            }
        },

        // Flygon
        {
            "Flygon",
            new PKMSWSHShowdownSets
            {
                Name = "Flygon",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Sand Attack", "Astonish" }
            }
        },

        // Swablu
        {
            "Swablu",
            new PKMSWSHShowdownSets
            {
                Name = "Swablu",
                Gender = "M",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Peck", "Growl" }
            }
        },

        // Altaria
        {
            "Altaria",
            new PKMSWSHShowdownSets
            {
                Name = "Altaria",
                Gender = "M",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Peck", "Growl" }
            }
        },

        // Lunatone
        {
            "Lunatone",
            new PKMSWSHShowdownSets
            {
                Name = "Lunatone",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Tackle", "Harden", "Confusion", "Rock Throw" }
            }
        },

        // Solrock
        {
            "Solrock",
            new PKMSWSHShowdownSets
            {
                Name = "Solrock",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Harden", "Confusion", "Rock Throw" }
            }
        },

        // Barboach
        {
            "Barboach",
            new PKMSWSHShowdownSets
            {
                Name = "Barboach",
                Gender = "M",
                Ability = "Anticipation",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Mud-Slap", "Water Gun" }
            }
        },

        // Whiscash
        {
            "Whiscash",
            new PKMSWSHShowdownSets
            {
                Name = "Whiscash",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Mud-Slap", "Water Gun" }
            }
        },

        // Corphish
        {
            "Corphish",
            new PKMSWSHShowdownSets
            {
                Name = "Corphish",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Water Gun", "Harden" }
            }
        },

        // Crawdaunt
        {
            "Crawdaunt",
            new PKMSWSHShowdownSets
            {
                Name = "Crawdaunt",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Water Gun", "Harden" }
            }
        },

        // Baltoy
        {
            "Baltoy",
            new PKMSWSHShowdownSets
            {
                Name = "Baltoy",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Harden", "Mud-Slap" }
            }
        },

        // Claydol
        {
            "Claydol",
            new PKMSWSHShowdownSets
            {
                Name = "Claydol",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Harden", "Mud-Slap" }
            }
        },

        // Lileep
        {
            "Lileep",
            new PKMSWSHShowdownSets
            {
                Name = "Lileep",
                Gender = "M",
                Ability = "Suction Cups",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Wrap", "Astonish" }
            }
        },

        // Cradily
        {
            "Cradily",
            new PKMSWSHShowdownSets
            {
                Name = "Cradily",
                Gender = "M",
                Ability = "Suction Cups",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Wrap", "Astonish" }
            }
        },

        // Anorith
        {
            "Anorith",
            new PKMSWSHShowdownSets
            {
                Name = "Anorith",
                Gender = "M",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Fury Cutter", "Harden" }
            }
        },

        // Armaldo
        {
            "Armaldo",
            new PKMSWSHShowdownSets
            {
                Name = "Armaldo",
                Gender = "M",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Fury Cutter", "Harden" }
            }
        },

        // Feebas
        {
            "Feebas",
            new PKMSWSHShowdownSets
            {
                Name = "Feebas",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Splash" }
            }
        },

        // Milotic
        {
            "Milotic",
            new PKMSWSHShowdownSets
            {
                Name = "Milotic",
                Gender = "M",
                Ability = "Competitive",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Splash" }
            }
        },

        // Duskull
        {
            "Duskull",
            new PKMSWSHShowdownSets
            {
                Name = "Duskull",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Astonish", "Leer" }
            }
        },

        // Dusclops
        {
            "Dusclops",
            new PKMSWSHShowdownSets
            {
                Name = "Dusclops",
                Gender = "M",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Astonish", "Leer" }
            }
        },

        // Absol
        {
            "Absol",
            new PKMSWSHShowdownSets
            {
                Name = "Absol",
                Gender = "M",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Wynaut
        {
            "Wynaut",
            new PKMSWSHShowdownSets
            {
                Name = "Wynaut",
                Gender = "M",
                Ability = "Shadow Tag",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Splash", "Charm", "Encore", "Amnesia" }
            }
        },

        // Snorunt
        {
            "Snorunt",
            new PKMSWSHShowdownSets
            {
                Name = "Snorunt",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Powder Snow", "Astonish" }
            }
        },

        // Glalie
        {
            "Glalie",
            new PKMSWSHShowdownSets
            {
                Name = "Glalie",
                Gender = "M",
                Ability = "Ice Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Powder Snow", "Astonish" }
            }
        },

        // Spheal
        {
            "Spheal",
            new PKMSWSHShowdownSets
            {
                Name = "Spheal",
                Gender = "M",
                Ability = "Thick Fat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Rollout", "Defense Curl" }
            }
        },

        // Sealeo
        {
            "Sealeo",
            new PKMSWSHShowdownSets
            {
                Name = "Sealeo",
                Gender = "M",
                Ability = "Ice Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Rollout", "Defense Curl" }
            }
        },

        // Walrein
        {
            "Walrein",
            new PKMSWSHShowdownSets
            {
                Name = "Walrein",
                Gender = "M",
                Ability = "Thick Fat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Rollout", "Defense Curl" }
            }
        },

        // Relicanth
        {
            "Relicanth",
            new PKMSWSHShowdownSets
            {
                Name = "Relicanth",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Bagon
        {
            "Bagon",
            new PKMSWSHShowdownSets
            {
                Name = "Bagon",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Ember", "Leer" }
            }
        },

        // Shelgon
        {
            "Shelgon",
            new PKMSWSHShowdownSets
            {
                Name = "Shelgon",
                Gender = "M",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Ember", "Leer" }
            }
        },

        // Salamence
        {
            "Salamence",
            new PKMSWSHShowdownSets
            {
                Name = "Salamence",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Ember", "Leer" }
            }
        },

        // Beldum
        {
            "Beldum",
            new PKMSWSHShowdownSets
            {
                Name = "Beldum",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Tackle" }
            }
        },

        // Metang
        {
            "Metang",
            new PKMSWSHShowdownSets
            {
                Name = "Metang",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Tackle" }
            }
        },

        // Metagross
        {
            "Metagross",
            new PKMSWSHShowdownSets
            {
                Name = "Metagross",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Tackle" }
            }
        },

        // Regirock
        {
            "Regirock",
            new PKMSWSHShowdownSets
            {
                Name = "Regirock",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Superpower", "Stone Edge", "Hammer Arm", "Curse" }
            }
        },

        // Regice
        {
            "Regice",
            new PKMSWSHShowdownSets
            {
                Name = "Regice",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Ice Beam", "Zap Cannon", "Amnesia", "Icy Wind" }
            }
        },

        // Registeel
        {
            "Registeel",
            new PKMSWSHShowdownSets
            {
                Name = "Registeel",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Heavy Slam", "Flash Cannon", "Iron Defense", "Charge Beam" }
            }
        },

        // Latias
        {
            "Latias",
            new PKMSWSHShowdownSets
            {
                Name = "Latias",
                Gender = "F",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Reflect Type", "Dragon Breath", "Zen Headbutt", "Surf" }
            }
        },

        // Latios
        {
            "Latios",
            new PKMSWSHShowdownSets
            {
                Name = "Latios",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Dragon Dance", "Dragon Pulse", "Zen Headbutt", "Aura Sphere" }
            }
        },

        // Kyogre
        {
            "Kyogre",
            new PKMSWSHShowdownSets
            {
                Name = "Kyogre",
                Ability = "Drizzle",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Surf", "Body Slam", "Aqua Ring", "Thunder" }
            }
        },

        // Groudon
        {
            "Groudon",
            new PKMSWSHShowdownSets
            {
                Name = "Groudon",
                Ability = "Drought",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Earthquake", "Scary Face", "Lava Plume", "Hammer Arm" }
            }
        },

        // Rayquaza
        {
            "Rayquaza",
            new PKMSWSHShowdownSets
            {
                Name = "Rayquaza",
                Ability = "Air Lock",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Dragon Ascent", "Brutal Swing", "Extreme Speed", "Twister" }
            }
        },

        // Jirachi
        {
            "Jirachi",
            new PKMSWSHShowdownSets
            {
                Name = "Jirachi",
                Ability = "Serene Grace",
                Level = "95",
                Shiny = "No",
                Nature = "Timid",
                Moves = new List<string> { "Meteor Mash", "Psychic", "Rest", "Wish" }
            }
        },

        // Shinx
        {
            "Shinx",
            new PKMSWSHShowdownSets
            {
                Name = "Shinx",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Luxio
        {
            "Luxio",
            new PKMSWSHShowdownSets
            {
                Name = "Luxio",
                Gender = "M",
                Ability = "Rivalry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Luxray
        {
            "Luxray",
            new PKMSWSHShowdownSets
            {
                Name = "Luxray",
                Gender = "M",
                Ability = "Rivalry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Budew
        {
            "Budew",
            new PKMSWSHShowdownSets
            {
                Name = "Budew",
                Gender = "M",
                Ability = "Poison Point",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Absorb", "Growth", "Stun Spore", "Worry Seed" }
            }
        },

        // Roserade
        {
            "Roserade",
            new PKMSWSHShowdownSets
            {
                Name = "Roserade",
                Gender = "M",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Absorb", "Growth", "Stun Spore", "Worry Seed" }
            }
        },

        // Combee
        {
            "Combee",
            new PKMSWSHShowdownSets
            {
                Name = "Combee",
                Gender = "M",
                Ability = "Honey Gather",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Sweet Scent", "Gust", "Struggle Bug", "Bug Bite" }
            }
        },

        // Vespiquen
        {
            "Vespiquen",
            new PKMSWSHShowdownSets
            {
                Name = "Vespiquen",
                Gender = "F",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Sweet Scent", "Gust", "Struggle Bug", "Bug Bite" }
            }
        },

        // Cherubi
        {
            "Cherubi",
            new PKMSWSHShowdownSets
            {
                Name = "Cherubi",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Morning Sun", "Tackle" }
            }
        },

        // Cherrim
        {
            "Cherrim",
            new PKMSWSHShowdownSets
            {
                Name = "Cherrim",
                Gender = "M",
                Ability = "Flower Gift",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Morning Sun", "Tackle" }
            }
        },

        // Shellos
        {
            "Shellos",
            new PKMSWSHShowdownSets
            {
                Name = "Shellos",
                Gender = "M",
                Ability = "Sticky Hold",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Water Gun", "Mud-Slap" }
            }
        },

        // Shellos-East (M)
        {
            "Shellos-East (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Shellos",
                Form = "East (M)",
                Ability = "Storm Drain",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Water Gun", "Mud-Slap" }
            }
        },

        // Gastrodon
        {
            "Gastrodon",
            new PKMSWSHShowdownSets
            {
                Name = "Gastrodon",
                Gender = "M",
                Ability = "Sticky Hold",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Water Gun", "Mud-Slap" }
            }
        },

        // Gastrodon-East (M)
        {
            "Gastrodon-East (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Gastrodon",
                Form = "East (M)",
                Ability = "Storm Drain",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Water Gun", "Mud-Slap" }
            }
        },

        // Drifloon
        {
            "Drifloon",
            new PKMSWSHShowdownSets
            {
                Name = "Drifloon",
                Gender = "M",
                Ability = "Aftermath",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Minimize", "Astonish" }
            }
        },

        // Drifblim
        {
            "Drifblim",
            new PKMSWSHShowdownSets
            {
                Name = "Drifblim",
                Gender = "M",
                Ability = "Aftermath",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Minimize", "Astonish" }
            }
        },

        // Buneary
        {
            "Buneary",
            new PKMSWSHShowdownSets
            {
                Name = "Buneary",
                Gender = "M",
                Ability = "Klutz",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Splash", "Pound" }
            }
        },

        // Lopunny
        {
            "Lopunny",
            new PKMSWSHShowdownSets
            {
                Name = "Lopunny",
                Gender = "M",
                Ability = "Cute Charm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Splash", "Pound" }
            }
        },

        // Stunky
        {
            "Stunky",
            new PKMSWSHShowdownSets
            {
                Name = "Stunky",
                Gender = "M",
                Ability = "Aftermath",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Poison Gas", "Scratch" }
            }
        },

        // Skuntank
        {
            "Skuntank",
            new PKMSWSHShowdownSets
            {
                Name = "Skuntank",
                Gender = "M",
                Ability = "Aftermath",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Poison Gas", "Scratch" }
            }
        },

        // Bronzor
        {
            "Bronzor",
            new PKMSWSHShowdownSets
            {
                Name = "Bronzor",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Confusion" }
            }
        },

        // Bronzong
        {
            "Bronzong",
            new PKMSWSHShowdownSets
            {
                Name = "Bronzong",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Tackle", "Confusion" }
            }
        },

        // Bonsly
        {
            "Bonsly",
            new PKMSWSHShowdownSets
            {
                Name = "Bonsly",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Fake Tears", "Copycat" }
            }
        },

        // Mime Jr.
        {
            "Mime Jr.",
            new PKMSWSHShowdownSets
            {
                Name = "Mime Jr.",
                Gender = "M",
                Ability = "Soundproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Pound", "Copycat" }
            }
        },

        // Happiny
        {
            "Happiny",
            new PKMSWSHShowdownSets
            {
                Name = "Happiny",
                Gender = "F",
                Ability = "Serene Grace",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Minimize", "Pound", "Copycat" }
            }
        },

        // Spiritomb
        {
            "Spiritomb",
            new PKMSWSHShowdownSets
            {
                Name = "Spiritomb",
                Gender = "M",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Night Shade", "Confuse Ray" }
            }
        },

        // Gible
        {
            "Gible",
            new PKMSWSHShowdownSets
            {
                Name = "Gible",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Sand Tomb", "Tackle" }
            }
        },

        // Gabite
        {
            "Gabite",
            new PKMSWSHShowdownSets
            {
                Name = "Gabite",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Sand Tomb", "Tackle" }
            }
        },

        // Garchomp
        {
            "Garchomp",
            new PKMSWSHShowdownSets
            {
                Name = "Garchomp",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Sand Tomb", "Tackle" }
            }
        },

        // Munchlax
        {
            "Munchlax",
            new PKMSWSHShowdownSets
            {
                Name = "Munchlax",
                Gender = "M",
                Ability = "Thick Fat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Lick", "Tackle" }
            }
        },

        // Riolu
        {
            "Riolu",
            new PKMSWSHShowdownSets
            {
                Name = "Riolu",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Quick Attack", "Endure" }
            }
        },

        // Lucario
        {
            "Lucario",
            new PKMSWSHShowdownSets
            {
                Name = "Lucario",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Quick Attack", "Endure" }
            }
        },

        // Hippopotas
        {
            "Hippopotas",
            new PKMSWSHShowdownSets
            {
                Name = "Hippopotas",
                Gender = "M",
                Ability = "Sand Stream",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Tackle", "Sand Attack" }
            }
        },

        // Hippowdon
        {
            "Hippowdon",
            new PKMSWSHShowdownSets
            {
                Name = "Hippowdon",
                Gender = "M",
                Ability = "Sand Stream",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Tackle", "Sand Attack" }
            }
        },

        // Skorupi
        {
            "Skorupi",
            new PKMSWSHShowdownSets
            {
                Name = "Skorupi",
                Gender = "M",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Poison Sting", "Leer" }
            }
        },

        // Drapion
        {
            "Drapion",
            new PKMSWSHShowdownSets
            {
                Name = "Drapion",
                Gender = "M",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Poison Sting", "Leer" }
            }
        },

        // Croagunk
        {
            "Croagunk",
            new PKMSWSHShowdownSets
            {
                Name = "Croagunk",
                Gender = "M",
                Ability = "Dry Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Poison Sting", "Mud-Slap" }
            }
        },

        // Toxicroak
        {
            "Toxicroak",
            new PKMSWSHShowdownSets
            {
                Name = "Toxicroak",
                Gender = "M",
                Ability = "Anticipation",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Poison Sting", "Mud-Slap" }
            }
        },

        // Mantyke
        {
            "Mantyke",
            new PKMSWSHShowdownSets
            {
                Name = "Mantyke",
                Gender = "M",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Water Gun" }
            }
        },

        // Snover
        {
            "Snover",
            new PKMSWSHShowdownSets
            {
                Name = "Snover",
                Gender = "M",
                Ability = "Snow Warning",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Powder Snow", "Leer" }
            }
        },

        // Abomasnow
        {
            "Abomasnow",
            new PKMSWSHShowdownSets
            {
                Name = "Abomasnow",
                Gender = "M",
                Ability = "Snow Warning",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Powder Snow", "Leer" }
            }
        },

        // Weavile
        {
            "Weavile",
            new PKMSWSHShowdownSets
            {
                Name = "Weavile",
                Gender = "M",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Magnezone
        {
            "Magnezone",
            new PKMSWSHShowdownSets
            {
                Name = "Magnezone",
                Ability = "Magnet Pull",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Thunder Shock", "Tackle" }
            }
        },

        // Lickilicky
        {
            "Lickilicky",
            new PKMSWSHShowdownSets
            {
                Name = "Lickilicky",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Lick", "Defense Curl" }
            }
        },

        // Rhyperior
        {
            "Rhyperior",
            new PKMSWSHShowdownSets
            {
                Name = "Rhyperior",
                Gender = "M",
                Ability = "Lightning Rod",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Tangrowth
        {
            "Tangrowth",
            new PKMSWSHShowdownSets
            {
                Name = "Tangrowth",
                Gender = "M",
                Ability = "Leaf Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Absorb", "Bind" }
            }
        },

        // Electivire
        {
            "Electivire",
            new PKMSWSHShowdownSets
            {
                Name = "Electivire",
                Gender = "M",
                Ability = "Motor Drive",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Magmortar
        {
            "Magmortar",
            new PKMSWSHShowdownSets
            {
                Name = "Magmortar",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Smog", "Leer" }
            }
        },

        // Togekiss
        {
            "Togekiss",
            new PKMSWSHShowdownSets
            {
                Name = "Togekiss",
                Gender = "M",
                Ability = "Hustle",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Growl", "Pound" }
            }
        },

        // Leafeon
        {
            "Leafeon",
            new PKMSWSHShowdownSets
            {
                Name = "Leafeon",
                Gender = "M",
                Ability = "Leaf Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Glaceon
        {
            "Glaceon",
            new PKMSWSHShowdownSets
            {
                Name = "Glaceon",
                Gender = "M",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Mamoswine
        {
            "Mamoswine",
            new PKMSWSHShowdownSets
            {
                Name = "Mamoswine",
                Gender = "M",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Mud-Slap" }
            }
        },

        // Porygon-Z
        {
            "Porygon-Z",
            new PKMSWSHShowdownSets
            {
                Name = "Porygon",
                Form = "Z",
                Ability = "Download",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Tackle", "Conversion" }
            }
        },

        // Gallade
        {
            "Gallade",
            new PKMSWSHShowdownSets
            {
                Name = "Gallade",
                Gender = "M",
                Ability = "Steadfast",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Growl", "Disarming Voice" }
            }
        },

        // Dusknoir
        {
            "Dusknoir",
            new PKMSWSHShowdownSets
            {
                Name = "Dusknoir",
                Gender = "M",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Astonish", "Leer" }
            }
        },

        // Froslass
        {
            "Froslass",
            new PKMSWSHShowdownSets
            {
                Name = "Froslass",
                Gender = "F",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Powder Snow", "Astonish" }
            }
        },

        // Rotom
        {
            "Rotom",
            new PKMSWSHShowdownSets
            {
                Name = "Rotom",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Astonish", "Double Team" }
            }
        },

        // Rotom-Heat
        {
            "Rotom-Heat",
            new PKMSWSHShowdownSets
            {
                Name = "Rotom",
                Form = "Heat",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Astonish", "Double Team" }
            }
        },

        // Rotom-Wash
        {
            "Rotom-Wash",
            new PKMSWSHShowdownSets
            {
                Name = "Rotom",
                Form = "Wash",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Astonish", "Double Team" }
            }
        },

        // Rotom-Frost
        {
            "Rotom-Frost",
            new PKMSWSHShowdownSets
            {
                Name = "Rotom",
                Form = "Frost",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Astonish", "Double Team" }
            }
        },

        // Rotom-Fan
        {
            "Rotom-Fan",
            new PKMSWSHShowdownSets
            {
                Name = "Rotom",
                Form = "Fan",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Astonish", "Double Team" }
            }
        },

        // Rotom-Mow
        {
            "Rotom-Mow",
            new PKMSWSHShowdownSets
            {
                Name = "Rotom",
                Form = "Mow",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Astonish", "Double Team" }
            }
        },

        // Uxie
        {
            "Uxie",
            new PKMSWSHShowdownSets
            {
                Name = "Uxie",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Psychic", "Future Sight", "Magic Room", "Shadow Ball" }
            }
        },

        // Mesprit
        {
            "Mesprit",
            new PKMSWSHShowdownSets
            {
                Name = "Mesprit",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Psychic", "Charm", "Draining Kiss", "Tri Attack" }
            }
        },

        // Azelf
        {
            "Azelf",
            new PKMSWSHShowdownSets
            {
                Name = "Azelf",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Psychic", "Dazzling Gleam", "Nasty Plot", "Facade" }
            }
        },

        // Dialga
        {
            "Dialga",
            new PKMSWSHShowdownSets
            {
                Name = "Dialga",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Slash", "Ancient Power", "Flash Cannon", "Dragon Claw" }
            }
        },

        // Palkia
        {
            "Palkia",
            new PKMSWSHShowdownSets
            {
                Name = "Palkia",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Slash", "Surf", "Ancient Power", "Dragon Claw" }
            }
        },

        // Heatran
        {
            "Heatran",
            new PKMSWSHShowdownSets
            {
                Name = "Heatran",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Metal Sound", "Lava Plume", "Crunch", "Iron Head" }
            }
        },

        // Regigigas
        {
            "Regigigas",
            new PKMSWSHShowdownSets
            {
                Name = "Regigigas",
                Ability = "Slow Start",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Giga Impact", "Zen Headbutt", "Hammer Arm", "Crush Grip" }
            }
        },

        // Giratina
        {
            "Giratina",
            new PKMSWSHShowdownSets
            {
                Name = "Giratina",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Dragon Claw", "Scary Face", "Shadow Ball", "Ancient Power" }
            }
        },

        // Giratina-Origin @ Griseous Orb
        {
            "Giratina-Origin @ Griseous Orb",
            new PKMSWSHShowdownSets
            {
                Name = "Giratina",
                Form = "Origin @ Griseous Orb",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Dragon Claw", "Scary Face", "Shadow Ball", "Ancient Power" }
            }
        },

        // Cresselia
        {
            "Cresselia",
            new PKMSWSHShowdownSets
            {
                Name = "Cresselia",
                Gender = "F",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Icy Wind", "Moonblast", "Psycho Cut", "Psyshock" }
            }
        },

        // Victini
        {
            "Victini",
            new PKMSWSHShowdownSets
            {
                Name = "Victini",
                Ability = "Victory Star",
                Level = "95",
                Shiny = "No",
                Nature = "Brave",
                Moves = new List<string> { "V-create", "Zen Headbutt", "Work Up", "Flame Charge" }
            }
        },

        // Lillipup
        {
            "Lillipup",
            new PKMSWSHShowdownSets
            {
                Name = "Lillipup",
                Gender = "M",
                Ability = "Vital Spirit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Herdier
        {
            "Herdier",
            new PKMSWSHShowdownSets
            {
                Name = "Herdier",
                Gender = "M",
                Ability = "Sand Rush",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Stoutland
        {
            "Stoutland",
            new PKMSWSHShowdownSets
            {
                Name = "Stoutland",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Purrloin
        {
            "Purrloin",
            new PKMSWSHShowdownSets
            {
                Name = "Purrloin",
                Gender = "M",
                Ability = "Unburden",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Liepard
        {
            "Liepard",
            new PKMSWSHShowdownSets
            {
                Name = "Liepard",
                Gender = "M",
                Ability = "Limber",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Munna
        {
            "Munna",
            new PKMSWSHShowdownSets
            {
                Name = "Munna",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Stored Power", "Defense Curl" }
            }
        },

        // Musharna
        {
            "Musharna",
            new PKMSWSHShowdownSets
            {
                Name = "Musharna",
                Gender = "M",
                Ability = "Forewarn",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Stored Power", "Defense Curl" }
            }
        },

        // Pidove
        {
            "Pidove",
            new PKMSWSHShowdownSets
            {
                Name = "Pidove",
                Gender = "M",
                Ability = "Super Luck",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Gust", "Growl" }
            }
        },

        // Tranquill
        {
            "Tranquill",
            new PKMSWSHShowdownSets
            {
                Name = "Tranquill",
                Gender = "M",
                Ability = "Super Luck",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Gust", "Growl" }
            }
        },

        // Unfezant
        {
            "Unfezant",
            new PKMSWSHShowdownSets
            {
                Name = "Unfezant",
                Gender = "M",
                Ability = "Super Luck",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Gust", "Growl" }
            }
        },

        // Roggenrola
        {
            "Roggenrola",
            new PKMSWSHShowdownSets
            {
                Name = "Roggenrola",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Sand Attack", "Tackle" }
            }
        },

        // Boldore
        {
            "Boldore",
            new PKMSWSHShowdownSets
            {
                Name = "Boldore",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Sand Attack", "Tackle" }
            }
        },

        // Gigalith
        {
            "Gigalith",
            new PKMSWSHShowdownSets
            {
                Name = "Gigalith",
                Gender = "M",
                Ability = "Sand Stream",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Sand Attack", "Tackle" }
            }
        },

        // Woobat
        {
            "Woobat",
            new PKMSWSHShowdownSets
            {
                Name = "Woobat",
                Gender = "M",
                Ability = "Unaware",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Gust", "Attract" }
            }
        },

        // Swoobat
        {
            "Swoobat",
            new PKMSWSHShowdownSets
            {
                Name = "Swoobat",
                Gender = "M",
                Ability = "Unaware",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Gust", "Attract" }
            }
        },

        // Drilbur
        {
            "Drilbur",
            new PKMSWSHShowdownSets
            {
                Name = "Drilbur",
                Gender = "M",
                Ability = "Sand Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Mud-Slap", "Rapid Spin" }
            }
        },

        // Excadrill
        {
            "Excadrill",
            new PKMSWSHShowdownSets
            {
                Name = "Excadrill",
                Gender = "M",
                Ability = "Sand Rush",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Mud-Slap", "Rapid Spin" }
            }
        },

        // Audino
        {
            "Audino",
            new PKMSWSHShowdownSets
            {
                Name = "Audino",
                Gender = "M",
                Ability = "Healer",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Pound", "Play Nice" }
            }
        },

        // Timburr
        {
            "Timburr",
            new PKMSWSHShowdownSets
            {
                Name = "Timburr",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Pound", "Leer" }
            }
        },

        // Gurdurr
        {
            "Gurdurr",
            new PKMSWSHShowdownSets
            {
                Name = "Gurdurr",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Pound", "Leer" }
            }
        },

        // Conkeldurr
        {
            "Conkeldurr",
            new PKMSWSHShowdownSets
            {
                Name = "Conkeldurr",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Pound", "Leer" }
            }
        },

        // Tympole
        {
            "Tympole",
            new PKMSWSHShowdownSets
            {
                Name = "Tympole",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Echoed Voice", "Growl" }
            }
        },

        // Palpitoad
        {
            "Palpitoad",
            new PKMSWSHShowdownSets
            {
                Name = "Palpitoad",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Echoed Voice", "Growl" }
            }
        },

        // Seismitoad
        {
            "Seismitoad",
            new PKMSWSHShowdownSets
            {
                Name = "Seismitoad",
                Gender = "M",
                Ability = "Poison Touch",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Echoed Voice", "Growl" }
            }
        },

        // Throh
        {
            "Throh",
            new PKMSWSHShowdownSets
            {
                Name = "Throh",
                Gender = "M",
                Ability = "Guts",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Bind", "Leer" }
            }
        },

        // Sawk
        {
            "Sawk",
            new PKMSWSHShowdownSets
            {
                Name = "Sawk",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Rock Smash", "Leer" }
            }
        },

        // Venipede
        {
            "Venipede",
            new PKMSWSHShowdownSets
            {
                Name = "Venipede",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Poison Sting", "Defense Curl" }
            }
        },

        // Whirlipede
        {
            "Whirlipede",
            new PKMSWSHShowdownSets
            {
                Name = "Whirlipede",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Poison Sting", "Defense Curl" }
            }
        },

        // Scolipede
        {
            "Scolipede",
            new PKMSWSHShowdownSets
            {
                Name = "Scolipede",
                Gender = "M",
                Ability = "Poison Point",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Poison Sting", "Defense Curl" }
            }
        },

        // Cottonee
        {
            "Cottonee",
            new PKMSWSHShowdownSets
            {
                Name = "Cottonee",
                Gender = "M",
                Ability = "Infiltrator",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Absorb", "Helping Hand" }
            }
        },

        // Whimsicott
        {
            "Whimsicott",
            new PKMSWSHShowdownSets
            {
                Name = "Whimsicott",
                Gender = "M",
                Ability = "Prankster",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Absorb", "Helping Hand" }
            }
        },

        // Petilil
        {
            "Petilil",
            new PKMSWSHShowdownSets
            {
                Name = "Petilil",
                Gender = "F",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Absorb", "Growth" }
            }
        },

        // Lilligant
        {
            "Lilligant",
            new PKMSWSHShowdownSets
            {
                Name = "Lilligant",
                Gender = "F",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Absorb", "Growth" }
            }
        },

        // Basculin
        {
            "Basculin",
            new PKMSWSHShowdownSets
            {
                Name = "Basculin",
                Gender = "M",
                Ability = "Reckless",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Water Gun", "Tail Whip" }
            }
        },

        // Basculin-Blue-Striped (M)
        {
            "Basculin-Blue-Striped (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Basculin",
                Form = "Blue-Striped (M)",
                Ability = "Rock Head",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Water Gun", "Tail Whip" }
            }
        },

        // Sandile
        {
            "Sandile",
            new PKMSWSHShowdownSets
            {
                Name = "Sandile",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Power Trip", "Leer" }
            }
        },

        // Krokorok
        {
            "Krokorok",
            new PKMSWSHShowdownSets
            {
                Name = "Krokorok",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Power Trip", "Leer" }
            }
        },

        // Krookodile
        {
            "Krookodile",
            new PKMSWSHShowdownSets
            {
                Name = "Krookodile",
                Gender = "M",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Power Trip", "Leer" }
            }
        },

        // Darumaka
        {
            "Darumaka",
            new PKMSWSHShowdownSets
            {
                Name = "Darumaka",
                Gender = "M",
                Ability = "Hustle",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Ember", "Tackle" }
            }
        },

        // Darumaka-Galar (M)
        {
            "Darumaka-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Darumaka",
                Form = "Galar (M)",
                Ability = "Hustle",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Powder Snow", "Tackle" }
            }
        },

        // Darmanitan
        {
            "Darmanitan",
            new PKMSWSHShowdownSets
            {
                Name = "Darmanitan",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Ember", "Tackle" }
            }
        },

        // Darmanitan-Galar (M)
        {
            "Darmanitan-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Darmanitan",
                Form = "Galar (M)",
                Ability = "Gorilla Tactics",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Powder Snow", "Tackle" }
            }
        },

        // Maractus
        {
            "Maractus",
            new PKMSWSHShowdownSets
            {
                Name = "Maractus",
                Gender = "M",
                Ability = "Chlorophyll",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Peck", "Absorb", "After You", "Ingrain" }
            }
        },

        // Dwebble
        {
            "Dwebble",
            new PKMSWSHShowdownSets
            {
                Name = "Dwebble",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Fury Cutter", "Sand Attack" }
            }
        },

        // Crustle
        {
            "Crustle",
            new PKMSWSHShowdownSets
            {
                Name = "Crustle",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Fury Cutter", "Sand Attack" }
            }
        },

        // Scraggy
        {
            "Scraggy",
            new PKMSWSHShowdownSets
            {
                Name = "Scraggy",
                Gender = "M",
                Ability = "Moxie",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Low Kick", "Leer" }
            }
        },

        // Scrafty
        {
            "Scrafty",
            new PKMSWSHShowdownSets
            {
                Name = "Scrafty",
                Gender = "M",
                Ability = "Shed Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Low Kick", "Leer" }
            }
        },

        // Sigilyph
        {
            "Sigilyph",
            new PKMSWSHShowdownSets
            {
                Name = "Sigilyph",
                Gender = "M",
                Ability = "Magic Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Gust", "Confusion" }
            }
        },

        // Yamask
        {
            "Yamask",
            new PKMSWSHShowdownSets
            {
                Name = "Yamask",
                Gender = "M",
                Ability = "Mummy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Astonish", "Protect" }
            }
        },

        // Yamask-Galar (M)
        {
            "Yamask-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Yamask",
                Form = "Galar (M)",
                Ability = "Wandering Spirit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Astonish", "Protect" }
            }
        },

        // Cofagrigus
        {
            "Cofagrigus",
            new PKMSWSHShowdownSets
            {
                Name = "Cofagrigus",
                Gender = "M",
                Ability = "Mummy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Astonish", "Protect" }
            }
        },

        // Tirtouga
        {
            "Tirtouga",
            new PKMSWSHShowdownSets
            {
                Name = "Tirtouga",
                Gender = "M",
                Ability = "Solid Rock",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Water Gun", "Withdraw" }
            }
        },

        // Carracosta
        {
            "Carracosta",
            new PKMSWSHShowdownSets
            {
                Name = "Carracosta",
                Gender = "M",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Water Gun", "Withdraw" }
            }
        },

        // Archen
        {
            "Archen",
            new PKMSWSHShowdownSets
            {
                Name = "Archen",
                Gender = "M",
                Ability = "Defeatist",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Archeops
        {
            "Archeops",
            new PKMSWSHShowdownSets
            {
                Name = "Archeops",
                Gender = "M",
                Ability = "Defeatist",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Quick Attack", "Leer" }
            }
        },

        // Trubbish
        {
            "Trubbish",
            new PKMSWSHShowdownSets
            {
                Name = "Trubbish",
                Gender = "M",
                Ability = "Stench",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Pound", "Poison Gas" }
            }
        },

        // Garbodor
        {
            "Garbodor",
            new PKMSWSHShowdownSets
            {
                Name = "Garbodor",
                Gender = "M",
                Ability = "Weak Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Pound", "Poison Gas" }
            }
        },

        // Zorua
        {
            "Zorua",
            new PKMSWSHShowdownSets
            {
                Name = "Zorua",
                Gender = "M",
                Ability = "Illusion",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Zoroark
        {
            "Zoroark",
            new PKMSWSHShowdownSets
            {
                Name = "Zoroark",
                Gender = "M",
                Ability = "Illusion",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Minccino
        {
            "Minccino",
            new PKMSWSHShowdownSets
            {
                Name = "Minccino",
                Gender = "M",
                Ability = "Technician",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Pound", "Baby-Doll Eyes" }
            }
        },

        // Cinccino
        {
            "Cinccino",
            new PKMSWSHShowdownSets
            {
                Name = "Cinccino",
                Gender = "M",
                Ability = "Cute Charm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Pound", "Baby-Doll Eyes" }
            }
        },

        // Gothita
        {
            "Gothita",
            new PKMSWSHShowdownSets
            {
                Name = "Gothita",
                Gender = "M",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Pound", "Confusion" }
            }
        },

        // Gothorita
        {
            "Gothorita",
            new PKMSWSHShowdownSets
            {
                Name = "Gothorita",
                Gender = "M",
                Ability = "Competitive",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Pound", "Confusion" }
            }
        },

        // Gothitelle
        {
            "Gothitelle",
            new PKMSWSHShowdownSets
            {
                Name = "Gothitelle",
                Gender = "M",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Pound", "Confusion" }
            }
        },

        // Solosis
        {
            "Solosis",
            new PKMSWSHShowdownSets
            {
                Name = "Solosis",
                Gender = "M",
                Ability = "Overcoat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Protect", "Confusion" }
            }
        },

        // Duosion
        {
            "Duosion",
            new PKMSWSHShowdownSets
            {
                Name = "Duosion",
                Gender = "M",
                Ability = "Magic Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Protect", "Confusion" }
            }
        },

        // Reuniclus
        {
            "Reuniclus",
            new PKMSWSHShowdownSets
            {
                Name = "Reuniclus",
                Gender = "M",
                Ability = "Overcoat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Protect", "Confusion" }
            }
        },

        // Vanillite
        {
            "Vanillite",
            new PKMSWSHShowdownSets
            {
                Name = "Vanillite",
                Gender = "M",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Astonish", "Harden" }
            }
        },

        // Vanillish
        {
            "Vanillish",
            new PKMSWSHShowdownSets
            {
                Name = "Vanillish",
                Gender = "M",
                Ability = "Snow Cloak",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Astonish", "Harden" }
            }
        },

        // Vanilluxe
        {
            "Vanilluxe",
            new PKMSWSHShowdownSets
            {
                Name = "Vanilluxe",
                Gender = "M",
                Ability = "Snow Warning",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Astonish", "Harden" }
            }
        },

        // Emolga
        {
            "Emolga",
            new PKMSWSHShowdownSets
            {
                Name = "Emolga",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Nuzzle", "Tail Whip" }
            }
        },

        // Karrablast
        {
            "Karrablast",
            new PKMSWSHShowdownSets
            {
                Name = "Karrablast",
                Gender = "M",
                Ability = "Shed Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Escavalier
        {
            "Escavalier",
            new PKMSWSHShowdownSets
            {
                Name = "Escavalier",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Foongus
        {
            "Foongus",
            new PKMSWSHShowdownSets
            {
                Name = "Foongus",
                Gender = "M",
                Ability = "Effect Spore",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Absorb", "Astonish" }
            }
        },

        // Amoonguss
        {
            "Amoonguss",
            new PKMSWSHShowdownSets
            {
                Name = "Amoonguss",
                Gender = "M",
                Ability = "Effect Spore",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Absorb", "Astonish" }
            }
        },

        // Frillish
        {
            "Frillish",
            new PKMSWSHShowdownSets
            {
                Name = "Frillish",
                Gender = "M",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Absorb", "Water Gun" }
            }
        },

        // Jellicent
        {
            "Jellicent",
            new PKMSWSHShowdownSets
            {
                Name = "Jellicent",
                Gender = "M",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Absorb", "Water Gun" }
            }
        },

        // Joltik
        {
            "Joltik",
            new PKMSWSHShowdownSets
            {
                Name = "Joltik",
                Gender = "M",
                Ability = "Compound Eyes",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Absorb", "Fury Cutter" }
            }
        },

        // Galvantula
        {
            "Galvantula",
            new PKMSWSHShowdownSets
            {
                Name = "Galvantula",
                Gender = "M",
                Ability = "Unnerve",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Absorb", "Fury Cutter" }
            }
        },

        // Ferroseed
        {
            "Ferroseed",
            new PKMSWSHShowdownSets
            {
                Name = "Ferroseed",
                Gender = "M",
                Ability = "Iron Barbs",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Ferrothorn
        {
            "Ferrothorn",
            new PKMSWSHShowdownSets
            {
                Name = "Ferrothorn",
                Gender = "M",
                Ability = "Iron Barbs",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Klink
        {
            "Klink",
            new PKMSWSHShowdownSets
            {
                Name = "Klink",
                Ability = "Minus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Thunder Shock", "Vise Grip" }
            }
        },

        // Klang
        {
            "Klang",
            new PKMSWSHShowdownSets
            {
                Name = "Klang",
                Ability = "Minus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Thunder Shock", "Vise Grip" }
            }
        },

        // Klinklang
        {
            "Klinklang",
            new PKMSWSHShowdownSets
            {
                Name = "Klinklang",
                Ability = "Plus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Thunder Shock", "Vise Grip" }
            }
        },

        // Elgyem
        {
            "Elgyem",
            new PKMSWSHShowdownSets
            {
                Name = "Elgyem",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Confusion", "Growl" }
            }
        },

        // Beheeyem
        {
            "Beheeyem",
            new PKMSWSHShowdownSets
            {
                Name = "Beheeyem",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Confusion", "Growl" }
            }
        },

        // Litwick
        {
            "Litwick",
            new PKMSWSHShowdownSets
            {
                Name = "Litwick",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Astonish", "Smog" }
            }
        },

        // Lampent
        {
            "Lampent",
            new PKMSWSHShowdownSets
            {
                Name = "Lampent",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Astonish", "Smog" }
            }
        },

        // Chandelure
        {
            "Chandelure",
            new PKMSWSHShowdownSets
            {
                Name = "Chandelure",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Astonish", "Smog" }
            }
        },

        // Axew
        {
            "Axew",
            new PKMSWSHShowdownSets
            {
                Name = "Axew",
                Gender = "M",
                Ability = "Mold Breaker",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Fraxure
        {
            "Fraxure",
            new PKMSWSHShowdownSets
            {
                Name = "Fraxure",
                Gender = "M",
                Ability = "Rivalry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Haxorus
        {
            "Haxorus",
            new PKMSWSHShowdownSets
            {
                Name = "Haxorus",
                Gender = "M",
                Ability = "Rivalry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Cubchoo
        {
            "Cubchoo",
            new PKMSWSHShowdownSets
            {
                Name = "Cubchoo",
                Gender = "M",
                Ability = "Slush Rush",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Powder Snow", "Growl" }
            }
        },

        // Beartic
        {
            "Beartic",
            new PKMSWSHShowdownSets
            {
                Name = "Beartic",
                Gender = "M",
                Ability = "Slush Rush",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Powder Snow", "Growl" }
            }
        },

        // Cryogonal
        {
            "Cryogonal",
            new PKMSWSHShowdownSets
            {
                Name = "Cryogonal",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Bind", "Ice Shard" }
            }
        },

        // Shelmet
        {
            "Shelmet",
            new PKMSWSHShowdownSets
            {
                Name = "Shelmet",
                Gender = "M",
                Ability = "Hydration",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Absorb", "Protect" }
            }
        },

        // Accelgor
        {
            "Accelgor",
            new PKMSWSHShowdownSets
            {
                Name = "Accelgor",
                Gender = "M",
                Ability = "Sticky Hold",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Absorb", "Protect" }
            }
        },

        // Stunfisk
        {
            "Stunfisk",
            new PKMSWSHShowdownSets
            {
                Name = "Stunfisk",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Mud-Slap", "Tackle", "Water Gun", "Thunder Shock" }
            }
        },

        // Stunfisk-Galar (M)
        {
            "Stunfisk-Galar (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Stunfisk",
                Form = "Galar (M)",
                Ability = "Mimicry",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Mud-Slap", "Tackle", "Water Gun", "Metal Claw" }
            }
        },

        // Mienfoo
        {
            "Mienfoo",
            new PKMSWSHShowdownSets
            {
                Name = "Mienfoo",
                Gender = "M",
                Ability = "Regenerator",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Pound", "Detect" }
            }
        },

        // Mienshao
        {
            "Mienshao",
            new PKMSWSHShowdownSets
            {
                Name = "Mienshao",
                Gender = "M",
                Ability = "Regenerator",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Pound", "Detect" }
            }
        },

        // Druddigon
        {
            "Druddigon",
            new PKMSWSHShowdownSets
            {
                Name = "Druddigon",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Golett
        {
            "Golett",
            new PKMSWSHShowdownSets
            {
                Name = "Golett",
                Ability = "Klutz",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Mud-Slap", "Astonish" }
            }
        },

        // Golurk
        {
            "Golurk",
            new PKMSWSHShowdownSets
            {
                Name = "Golurk",
                Ability = "Iron Fist",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Mud-Slap", "Astonish" }
            }
        },

        // Pawniard
        {
            "Pawniard",
            new PKMSWSHShowdownSets
            {
                Name = "Pawniard",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Bisharp
        {
            "Bisharp",
            new PKMSWSHShowdownSets
            {
                Name = "Bisharp",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Bouffalant
        {
            "Bouffalant",
            new PKMSWSHShowdownSets
            {
                Name = "Bouffalant",
                Gender = "M",
                Ability = "Reckless",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Rufflet
        {
            "Rufflet",
            new PKMSWSHShowdownSets
            {
                Name = "Rufflet",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Braviary
        {
            "Braviary",
            new PKMSWSHShowdownSets
            {
                Name = "Braviary",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Vullaby
        {
            "Vullaby",
            new PKMSWSHShowdownSets
            {
                Name = "Vullaby",
                Gender = "F",
                Ability = "Overcoat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Gust", "Leer" }
            }
        },

        // Mandibuzz
        {
            "Mandibuzz",
            new PKMSWSHShowdownSets
            {
                Name = "Mandibuzz",
                Gender = "F",
                Ability = "Overcoat",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Gust", "Leer" }
            }
        },

        // Heatmor
        {
            "Heatmor",
            new PKMSWSHShowdownSets
            {
                Name = "Heatmor",
                Gender = "M",
                Ability = "Gluttony",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Lick", "Tackle" }
            }
        },

        // Durant
        {
            "Durant",
            new PKMSWSHShowdownSets
            {
                Name = "Durant",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Fury Cutter", "Sand Attack" }
            }
        },

        // Deino
        {
            "Deino",
            new PKMSWSHShowdownSets
            {
                Name = "Deino",
                Gender = "M",
                Ability = "Hustle",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Tackle", "Focus Energy" }
            }
        },

        // Zweilous
        {
            "Zweilous",
            new PKMSWSHShowdownSets
            {
                Name = "Zweilous",
                Gender = "M",
                Ability = "Hustle",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Tackle", "Focus Energy" }
            }
        },

        // Hydreigon
        {
            "Hydreigon",
            new PKMSWSHShowdownSets
            {
                Name = "Hydreigon",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Tackle", "Focus Energy" }
            }
        },

        // Larvesta
        {
            "Larvesta",
            new PKMSWSHShowdownSets
            {
                Name = "Larvesta",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Ember", "String Shot" }
            }
        },

        // Volcarona
        {
            "Volcarona",
            new PKMSWSHShowdownSets
            {
                Name = "Volcarona",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Ember", "String Shot" }
            }
        },

        // Cobalion
        {
            "Cobalion",
            new PKMSWSHShowdownSets
            {
                Name = "Cobalion",
                Ability = "Justified",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Sacred Sword", "Swords Dance", "Iron Head", "Close Combat" }
            }
        },

        // Terrakion
        {
            "Terrakion",
            new PKMSWSHShowdownSets
            {
                Name = "Terrakion",
                Ability = "Justified",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Sacred Sword", "Swords Dance", "Stone Edge", "Close Combat" }
            }
        },

        // Virizion
        {
            "Virizion",
            new PKMSWSHShowdownSets
            {
                Name = "Virizion",
                Ability = "Justified",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Sacred Sword", "Swords Dance", "Leaf Blade", "Close Combat" }
            }
        },

        // Tornadus
        {
            "Tornadus",
            new PKMSWSHShowdownSets
            {
                Name = "Tornadus",
                Gender = "M",
                Ability = "Prankster",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Hurricane", "Agility", "Icy Wind", "Heat Wave" }
            }
        },

        // Tornadus-Therian (M)
        {
            "Tornadus-Therian (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Tornadus",
                Form = "Therian (M)",
                Ability = "Regenerator",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Hurricane", "Agility", "Icy Wind", "Heat Wave" }
            }
        },

        // Thundurus
        {
            "Thundurus",
            new PKMSWSHShowdownSets
            {
                Name = "Thundurus",
                Gender = "M",
                Ability = "Prankster",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Thunder", "Rain Dance", "Weather Ball", "Sludge Wave" }
            }
        },

        // Thundurus-Therian (M)
        {
            "Thundurus-Therian (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Thundurus",
                Form = "Therian (M)",
                Ability = "Volt Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Thunder", "Rain Dance", "Weather Ball", "Sludge Wave" }
            }
        },

        // Reshiram
        {
            "Reshiram",
            new PKMSWSHShowdownSets
            {
                Name = "Reshiram",
                Ability = "Turboblaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Noble Roar", "Extrasensory", "Fusion Flare", "Dragon Pulse" }
            }
        },

        // Zekrom
        {
            "Zekrom",
            new PKMSWSHShowdownSets
            {
                Name = "Zekrom",
                Ability = "Teravolt",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Noble Roar", "Slash", "Fusion Bolt", "Dragon Claw" }
            }
        },

        // Landorus
        {
            "Landorus",
            new PKMSWSHShowdownSets
            {
                Name = "Landorus",
                Gender = "M",
                Ability = "Sand Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Sand Tomb", "Rock Slide", "Bulldoze", "Focus Blast" }
            }
        },

        // Landorus-Therian (M)
        {
            "Landorus-Therian (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Landorus",
                Form = "Therian (M)",
                Ability = "Intimidate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Sand Tomb", "Rock Slide", "Bulldoze", "Focus Blast" }
            }
        },

        // Kyurem
        {
            "Kyurem",
            new PKMSWSHShowdownSets
            {
                Name = "Kyurem",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Ice Beam", "Hyper Voice", "Shadow Ball", "Scary Face" }
            }
        },

        // Keldeo
        {
            "Keldeo",
            new PKMSWSHShowdownSets
            {
                Name = "Keldeo",
                Ability = "Justified",
                Level = "95",
                Shiny = "No",
                Nature = "Calm",
                Moves = new List<string> { "Aqua Jet" }
            }
        },

        // Keldeo-Resolute
        {
            "Keldeo-Resolute",
            new PKMSWSHShowdownSets
            {
                Name = "Keldeo",
                Form = "Resolute",
                Ability = "Justified",
                Level = "95",
                Shiny = "No",
                Nature = "Gentle",
                Moves = new List<string> { "Secret Sword" }
            }
        },

        // Genesect
        {
            "Genesect",
            new PKMSWSHShowdownSets
            {
                Name = "Genesect",
                Ability = "Download",
                Level = "95",
                Shiny = "No",
                Nature = "Rash",
                Moves = new List<string> { "Techno Blast", "X-Scissor", "Metal Claw", "Fell Stinger" }
            }
        },

        // Genesect-Water @ Douse Drive
        {
            "Genesect-Water @ Douse Drive",
            new PKMSWSHShowdownSets
            {
                Name = "Genesect",
                Form = "Water @ Douse Drive",
                Ability = "Download",
                Level = "95",
                Shiny = "No",
                Nature = "Bold",
                Moves = new List<string> { "Techno Blast", "X-Scissor", "Metal Claw", "Fell Stinger" }
            }
        },

        // Genesect-Electric @ Shock Drive
        {
            "Genesect-Electric @ Shock Drive",
            new PKMSWSHShowdownSets
            {
                Name = "Genesect",
                Form = "Electric @ Shock Drive",
                Ability = "Download",
                Level = "95",
                Shiny = "No",
                Nature = "Relaxed",
                Moves = new List<string> { "Techno Blast", "X-Scissor", "Metal Claw", "Fell Stinger" }
            }
        },

        // Genesect-Fire @ Burn Drive
        {
            "Genesect-Fire @ Burn Drive",
            new PKMSWSHShowdownSets
            {
                Name = "Genesect",
                Form = "Fire @ Burn Drive",
                Ability = "Download",
                Level = "95",
                Shiny = "No",
                Nature = "Docile",
                Moves = new List<string> { "Techno Blast", "X-Scissor", "Metal Claw", "Fell Stinger" }
            }
        },

        // Genesect-Ice @ Chill Drive
        {
            "Genesect-Ice @ Chill Drive",
            new PKMSWSHShowdownSets
            {
                Name = "Genesect",
                Form = "Ice @ Chill Drive",
                Ability = "Download",
                Level = "95",
                Shiny = "No",
                Nature = "Quirky",
                Moves = new List<string> { "Techno Blast", "X-Scissor", "Metal Claw", "Fell Stinger" }
            }
        },

        // Bunnelby
        {
            "Bunnelby",
            new PKMSWSHShowdownSets
            {
                Name = "Bunnelby",
                Gender = "M",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Mud-Slap", "Leer" }
            }
        },

        // Diggersby
        {
            "Diggersby",
            new PKMSWSHShowdownSets
            {
                Name = "Diggersby",
                Gender = "M",
                Ability = "Cheek Pouch",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Mud-Slap", "Leer" }
            }
        },

        // Fletchling
        {
            "Fletchling",
            new PKMSWSHShowdownSets
            {
                Name = "Fletchling",
                Gender = "M",
                Ability = "Big Pecks",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Peck", "Growl" }
            }
        },

        // Fletchinder
        {
            "Fletchinder",
            new PKMSWSHShowdownSets
            {
                Name = "Fletchinder",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Peck", "Growl" }
            }
        },

        // Talonflame
        {
            "Talonflame",
            new PKMSWSHShowdownSets
            {
                Name = "Talonflame",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Peck", "Growl" }
            }
        },

        // Pancham
        {
            "Pancham",
            new PKMSWSHShowdownSets
            {
                Name = "Pancham",
                Gender = "M",
                Ability = "Mold Breaker",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Pangoro
        {
            "Pangoro",
            new PKMSWSHShowdownSets
            {
                Name = "Pangoro",
                Gender = "M",
                Ability = "Mold Breaker",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Espurr
        {
            "Espurr",
            new PKMSWSHShowdownSets
            {
                Name = "Espurr",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Meowstic
        {
            "Meowstic",
            new PKMSWSHShowdownSets
            {
                Name = "Meowstic",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Scratch", "Leer" }
            }
        },

        // Honedge
        {
            "Honedge",
            new PKMSWSHShowdownSets
            {
                Name = "Honedge",
                Gender = "M",
                Ability = "No Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Tackle", "Fury Cutter" }
            }
        },

        // Doublade
        {
            "Doublade",
            new PKMSWSHShowdownSets
            {
                Name = "Doublade",
                Gender = "M",
                Ability = "No Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Tackle", "Fury Cutter" }
            }
        },

        // Aegislash
        {
            "Aegislash",
            new PKMSWSHShowdownSets
            {
                Name = "Aegislash",
                Gender = "M",
                Ability = "Stance Change",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Fury Cutter" }
            }
        },

        // Spritzee
        {
            "Spritzee",
            new PKMSWSHShowdownSets
            {
                Name = "Spritzee",
                Gender = "M",
                Ability = "Healer",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Fairy Wind", "Sweet Scent" }
            }
        },

        // Aromatisse
        {
            "Aromatisse",
            new PKMSWSHShowdownSets
            {
                Name = "Aromatisse",
                Gender = "M",
                Ability = "Healer",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Fairy Wind", "Sweet Scent" }
            }
        },

        // Swirlix
        {
            "Swirlix",
            new PKMSWSHShowdownSets
            {
                Name = "Swirlix",
                Gender = "M",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Sweet Scent" }
            }
        },

        // Slurpuff
        {
            "Slurpuff",
            new PKMSWSHShowdownSets
            {
                Name = "Slurpuff",
                Gender = "M",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Sweet Scent" }
            }
        },

        // Inkay
        {
            "Inkay",
            new PKMSWSHShowdownSets
            {
                Name = "Inkay",
                Gender = "M",
                Ability = "Suction Cups",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Peck", "Tackle" }
            }
        },

        // Malamar
        {
            "Malamar",
            new PKMSWSHShowdownSets
            {
                Name = "Malamar",
                Gender = "M",
                Ability = "Contrary",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Peck", "Tackle" }
            }
        },

        // Binacle
        {
            "Binacle",
            new PKMSWSHShowdownSets
            {
                Name = "Binacle",
                Gender = "M",
                Ability = "Sniper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Mud-Slap", "Scratch" }
            }
        },

        // Barbaracle
        {
            "Barbaracle",
            new PKMSWSHShowdownSets
            {
                Name = "Barbaracle",
                Gender = "M",
                Ability = "Sniper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Mud-Slap", "Scratch" }
            }
        },

        // Skrelp
        {
            "Skrelp",
            new PKMSWSHShowdownSets
            {
                Name = "Skrelp",
                Gender = "M",
                Ability = "Poison Touch",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Smokescreen" }
            }
        },

        // Dragalge
        {
            "Dragalge",
            new PKMSWSHShowdownSets
            {
                Name = "Dragalge",
                Gender = "M",
                Ability = "Poison Point",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Tackle", "Smokescreen" }
            }
        },

        // Clauncher
        {
            "Clauncher",
            new PKMSWSHShowdownSets
            {
                Name = "Clauncher",
                Gender = "M",
                Ability = "Mega Launcher",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Splash", "Water Gun" }
            }
        },

        // Clawitzer
        {
            "Clawitzer",
            new PKMSWSHShowdownSets
            {
                Name = "Clawitzer",
                Gender = "M",
                Ability = "Mega Launcher",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Splash", "Water Gun" }
            }
        },

        // Helioptile
        {
            "Helioptile",
            new PKMSWSHShowdownSets
            {
                Name = "Helioptile",
                Gender = "M",
                Ability = "Dry Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Mud-Slap", "Tail Whip" }
            }
        },

        // Heliolisk
        {
            "Heliolisk",
            new PKMSWSHShowdownSets
            {
                Name = "Heliolisk",
                Gender = "M",
                Ability = "Sand Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Mud-Slap", "Tail Whip" }
            }
        },

        // Tyrunt
        {
            "Tyrunt",
            new PKMSWSHShowdownSets
            {
                Name = "Tyrunt",
                Gender = "M",
                Ability = "Strong Jaw",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Tyrantrum
        {
            "Tyrantrum",
            new PKMSWSHShowdownSets
            {
                Name = "Tyrantrum",
                Gender = "M",
                Ability = "Strong Jaw",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Amaura
        {
            "Amaura",
            new PKMSWSHShowdownSets
            {
                Name = "Amaura",
                Gender = "M",
                Ability = "Refrigerate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Powder Snow", "Growl" }
            }
        },

        // Aurorus
        {
            "Aurorus",
            new PKMSWSHShowdownSets
            {
                Name = "Aurorus",
                Gender = "M",
                Ability = "Refrigerate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Powder Snow", "Growl" }
            }
        },

        // Sylveon
        {
            "Sylveon",
            new PKMSWSHShowdownSets
            {
                Name = "Sylveon",
                Gender = "M",
                Ability = "Cute Charm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Helping Hand", "Tackle", "Growl", "Tail Whip" }
            }
        },

        // Hawlucha
        {
            "Hawlucha",
            new PKMSWSHShowdownSets
            {
                Name = "Hawlucha",
                Gender = "M",
                Ability = "Unburden",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Tackle", "Hone Claws" }
            }
        },

        // Dedenne
        {
            "Dedenne",
            new PKMSWSHShowdownSets
            {
                Name = "Dedenne",
                Gender = "M",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Nuzzle", "Tail Whip" }
            }
        },

        // Carbink
        {
            "Carbink",
            new PKMSWSHShowdownSets
            {
                Name = "Carbink",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Goomy
        {
            "Goomy",
            new PKMSWSHShowdownSets
            {
                Name = "Goomy",
                Gender = "M",
                Ability = "Sap Sipper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Absorb", "Tackle" }
            }
        },

        // Sliggoo
        {
            "Sliggoo",
            new PKMSWSHShowdownSets
            {
                Name = "Sliggoo",
                Gender = "M",
                Ability = "Sap Sipper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Absorb", "Tackle" }
            }
        },

        // Goodra
        {
            "Goodra",
            new PKMSWSHShowdownSets
            {
                Name = "Goodra",
                Gender = "M",
                Ability = "Sap Sipper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Absorb", "Tackle" }
            }
        },

        // Klefki
        {
            "Klefki",
            new PKMSWSHShowdownSets
            {
                Name = "Klefki",
                Gender = "M",
                Ability = "Prankster",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Fairy Lock", "Astonish" }
            }
        },

        // Phantump
        {
            "Phantump",
            new PKMSWSHShowdownSets
            {
                Name = "Phantump",
                Gender = "M",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Astonish", "Tackle" }
            }
        },

        // Trevenant
        {
            "Trevenant",
            new PKMSWSHShowdownSets
            {
                Name = "Trevenant",
                Gender = "M",
                Ability = "Natural Cure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Astonish", "Tackle" }
            }
        },

        // Pumpkaboo
        {
            "Pumpkaboo",
            new PKMSWSHShowdownSets
            {
                Name = "Pumpkaboo",
                Gender = "M",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Pumpkaboo-Small (M)
        {
            "Pumpkaboo-Small (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pumpkaboo",
                Form = "Small (M)",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Pumpkaboo-Large (M)
        {
            "Pumpkaboo-Large (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pumpkaboo",
                Form = "Large (M)",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Pumpkaboo-Super (M)
        {
            "Pumpkaboo-Super (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Pumpkaboo",
                Form = "Super (M)",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Gourgeist
        {
            "Gourgeist",
            new PKMSWSHShowdownSets
            {
                Name = "Gourgeist",
                Gender = "M",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Gourgeist-Small (M)
        {
            "Gourgeist-Small (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Gourgeist",
                Form = "Small (M)",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Gourgeist-Large (M)
        {
            "Gourgeist-Large (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Gourgeist",
                Form = "Large (M)",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Gourgeist-Super (M)
        {
            "Gourgeist-Super (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Gourgeist",
                Form = "Super (M)",
                Ability = "Pickup",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Trick-or-Treat", "Astonish" }
            }
        },

        // Bergmite
        {
            "Bergmite",
            new PKMSWSHShowdownSets
            {
                Name = "Bergmite",
                Gender = "M",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Rapid Spin", "Harden" }
            }
        },

        // Avalugg
        {
            "Avalugg",
            new PKMSWSHShowdownSets
            {
                Name = "Avalugg",
                Gender = "M",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Rapid Spin", "Harden" }
            }
        },

        // Noibat
        {
            "Noibat",
            new PKMSWSHShowdownSets
            {
                Name = "Noibat",
                Gender = "M",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Absorb", "Tackle" }
            }
        },

        // Noivern
        {
            "Noivern",
            new PKMSWSHShowdownSets
            {
                Name = "Noivern",
                Gender = "M",
                Ability = "Infiltrator",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Absorb", "Tackle" }
            }
        },

        // Xerneas
        {
            "Xerneas",
            new PKMSWSHShowdownSets
            {
                Name = "Xerneas",
                Ability = "Fairy Aura",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Ingrain", "Dazzling Gleam", "Moonblast", "Horn Leech" }
            }
        },

        // Yveltal
        {
            "Yveltal",
            new PKMSWSHShowdownSets
            {
                Name = "Yveltal",
                Ability = "Dark Aura",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Taunt", "Oblivion Wing", "Dragon Rush", "Sucker Punch" }
            }
        },

        // Zygarde
        {
            "Zygarde",
            new PKMSWSHShowdownSets
            {
                Name = "Zygarde",
                Ability = "Aura Break",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Thousand Arrows", "Land’s Wrath", "Dragon Pulse", "Bind" }
            }
        },

        // Zygarde-10%
        {
            "Zygarde-10%",
            new PKMSWSHShowdownSets
            {
                Name = "Zygarde",
                Form = "10%",
                Ability = "Aura Break",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Thousand Arrows", "Land’s Wrath", "Dragon Pulse", "Bind" }
            }
        },

        // Volcanion
        {
            "Volcanion",
            new PKMSWSHShowdownSets
            {
                Name = "Volcanion",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "No",
                Nature = "Sassy",
                Moves = new List<string> { "Steam Eruption", "Flare Blitz", "Incinerate", "Haze" }
            }
        },

        // Rowlet
        {
            "Rowlet",
            new PKMSWSHShowdownSets
            {
                Name = "Rowlet",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Dartrix
        {
            "Dartrix",
            new PKMSWSHShowdownSets
            {
                Name = "Dartrix",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Decidueye
        {
            "Decidueye",
            new PKMSWSHShowdownSets
            {
                Name = "Decidueye",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Litten
        {
            "Litten",
            new PKMSWSHShowdownSets
            {
                Name = "Litten",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Torracat
        {
            "Torracat",
            new PKMSWSHShowdownSets
            {
                Name = "Torracat",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Incineroar
        {
            "Incineroar",
            new PKMSWSHShowdownSets
            {
                Name = "Incineroar",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Popplio
        {
            "Popplio",
            new PKMSWSHShowdownSets
            {
                Name = "Popplio",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Pound", "Growl" }
            }
        },

        // Brionne
        {
            "Brionne",
            new PKMSWSHShowdownSets
            {
                Name = "Brionne",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Pound", "Growl" }
            }
        },

        // Primarina
        {
            "Primarina",
            new PKMSWSHShowdownSets
            {
                Name = "Primarina",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Pound", "Growl" }
            }
        },

        // Grubbin
        {
            "Grubbin",
            new PKMSWSHShowdownSets
            {
                Name = "Grubbin",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Vise Grip", "Mud-Slap" }
            }
        },

        // Charjabug
        {
            "Charjabug",
            new PKMSWSHShowdownSets
            {
                Name = "Charjabug",
                Gender = "M",
                Ability = "Battery",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Vise Grip", "Mud-Slap" }
            }
        },

        // Vikavolt
        {
            "Vikavolt",
            new PKMSWSHShowdownSets
            {
                Name = "Vikavolt",
                Gender = "M",
                Ability = "Levitate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Vise Grip", "Mud-Slap" }
            }
        },

        // Cutiefly
        {
            "Cutiefly",
            new PKMSWSHShowdownSets
            {
                Name = "Cutiefly",
                Gender = "M",
                Ability = "Honey Gather",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Absorb", "Fairy Wind" }
            }
        },

        // Ribombee
        {
            "Ribombee",
            new PKMSWSHShowdownSets
            {
                Name = "Ribombee",
                Gender = "M",
                Ability = "Honey Gather",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Absorb", "Fairy Wind" }
            }
        },

        // Rockruff
        {
            "Rockruff",
            new PKMSWSHShowdownSets
            {
                Name = "Rockruff",
                Gender = "M",
                Ability = "Keen Eye",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Lycanroc
        {
            "Lycanroc",
            new PKMSWSHShowdownSets
            {
                Name = "Lycanroc",
                Gender = "M",
                Ability = "Sand Rush",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Lycanroc-Midnight (M)
        {
            "Lycanroc-Midnight (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Lycanroc",
                Form = "Midnight (M)",
                Ability = "Vital Spirit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Lycanroc-Dusk (M)
        {
            "Lycanroc-Dusk (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Lycanroc",
                Form = "Dusk (M)",
                Ability = "Tough Claws",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Wishiwashi
        {
            "Wishiwashi",
            new PKMSWSHShowdownSets
            {
                Name = "Wishiwashi",
                Gender = "M",
                Ability = "Schooling",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Water Gun", "Growl" }
            }
        },

        // Mareanie
        {
            "Mareanie",
            new PKMSWSHShowdownSets
            {
                Name = "Mareanie",
                Gender = "M",
                Ability = "Limber",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Poison Sting", "Peck" }
            }
        },

        // Toxapex
        {
            "Toxapex",
            new PKMSWSHShowdownSets
            {
                Name = "Toxapex",
                Gender = "M",
                Ability = "Limber",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Poison Sting", "Peck" }
            }
        },

        // Mudbray
        {
            "Mudbray",
            new PKMSWSHShowdownSets
            {
                Name = "Mudbray",
                Gender = "M",
                Ability = "Own Tempo",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Mud-Slap", "Rock Smash" }
            }
        },

        // Mudsdale
        {
            "Mudsdale",
            new PKMSWSHShowdownSets
            {
                Name = "Mudsdale",
                Gender = "M",
                Ability = "Stamina",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Mud-Slap", "Rock Smash" }
            }
        },

        // Dewpider
        {
            "Dewpider",
            new PKMSWSHShowdownSets
            {
                Name = "Dewpider",
                Gender = "M",
                Ability = "Water Bubble",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Water Gun", "Infestation" }
            }
        },

        // Araquanid
        {
            "Araquanid",
            new PKMSWSHShowdownSets
            {
                Name = "Araquanid",
                Gender = "M",
                Ability = "Water Bubble",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Water Gun", "Infestation" }
            }
        },

        // Fomantis
        {
            "Fomantis",
            new PKMSWSHShowdownSets
            {
                Name = "Fomantis",
                Gender = "M",
                Ability = "Leaf Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Leafage", "Fury Cutter" }
            }
        },

        // Lurantis
        {
            "Lurantis",
            new PKMSWSHShowdownSets
            {
                Name = "Lurantis",
                Gender = "M",
                Ability = "Leaf Guard",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Leafage", "Fury Cutter" }
            }
        },

        // Morelull
        {
            "Morelull",
            new PKMSWSHShowdownSets
            {
                Name = "Morelull",
                Gender = "M",
                Ability = "Illuminate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Absorb", "Astonish" }
            }
        },

        // Shiinotic
        {
            "Shiinotic",
            new PKMSWSHShowdownSets
            {
                Name = "Shiinotic",
                Gender = "M",
                Ability = "Illuminate",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Absorb", "Astonish" }
            }
        },

        // Salandit
        {
            "Salandit",
            new PKMSWSHShowdownSets
            {
                Name = "Salandit",
                Gender = "M",
                Ability = "Corrosion",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Scratch", "Poison Gas" }
            }
        },

        // Salazzle
        {
            "Salazzle",
            new PKMSWSHShowdownSets
            {
                Name = "Salazzle",
                Gender = "F",
                Ability = "Corrosion",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Scratch", "Poison Gas" }
            }
        },

        // Stufful
        {
            "Stufful",
            new PKMSWSHShowdownSets
            {
                Name = "Stufful",
                Gender = "M",
                Ability = "Fluffy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Bewear
        {
            "Bewear",
            new PKMSWSHShowdownSets
            {
                Name = "Bewear",
                Gender = "M",
                Ability = "Fluffy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Bounsweet
        {
            "Bounsweet",
            new PKMSWSHShowdownSets
            {
                Name = "Bounsweet",
                Gender = "F",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Splash" }
            }
        },

        // Steenee
        {
            "Steenee",
            new PKMSWSHShowdownSets
            {
                Name = "Steenee",
                Gender = "F",
                Ability = "Oblivious",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Splash" }
            }
        },

        // Tsareena
        {
            "Tsareena",
            new PKMSWSHShowdownSets
            {
                Name = "Tsareena",
                Gender = "F",
                Ability = "Queenly Majesty",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Splash" }
            }
        },

        // Comfey
        {
            "Comfey",
            new PKMSWSHShowdownSets
            {
                Name = "Comfey",
                Gender = "M",
                Ability = "Flower Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Wrap", "Growth" }
            }
        },

        // Oranguru
        {
            "Oranguru",
            new PKMSWSHShowdownSets
            {
                Name = "Oranguru",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Confusion", "Taunt" }
            }
        },

        // Passimian
        {
            "Passimian",
            new PKMSWSHShowdownSets
            {
                Name = "Passimian",
                Gender = "M",
                Ability = "Receiver",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Wimpod
        {
            "Wimpod",
            new PKMSWSHShowdownSets
            {
                Name = "Wimpod",
                Gender = "M",
                Ability = "Wimp Out",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Struggle Bug", "Sand Attack", "Defense Curl" }
            }
        },

        // Golisopod
        {
            "Golisopod",
            new PKMSWSHShowdownSets
            {
                Name = "Golisopod",
                Gender = "M",
                Ability = "Emergency Exit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Struggle Bug", "Sand Attack", "Defense Curl" }
            }
        },

        // Sandygast
        {
            "Sandygast",
            new PKMSWSHShowdownSets
            {
                Name = "Sandygast",
                Gender = "M",
                Ability = "Water Compaction",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Absorb", "Harden" }
            }
        },

        // Palossand
        {
            "Palossand",
            new PKMSWSHShowdownSets
            {
                Name = "Palossand",
                Gender = "M",
                Ability = "Water Compaction",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Absorb", "Harden" }
            }
        },

        // Pyukumuku
        {
            "Pyukumuku",
            new PKMSWSHShowdownSets
            {
                Name = "Pyukumuku",
                Gender = "M",
                Ability = "Innards Out",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Baton Pass", "Harden" }
            }
        },

        // Type: Null
        {
            "Type: Null",
            new PKMSWSHShowdownSets
            {
                Name = "Type: Null",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "No",
                Nature = "Mild",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally
        {
            "Silvally",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Docile",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Fighting @ Fighting Memory
        {
            "Silvally-Fighting @ Fighting Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Fighting @ Fighting Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Lonely",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Flying @ Flying Memory
        {
            "Silvally-Flying @ Flying Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Flying @ Flying Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Lax",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Poison @ Poison Memory
        {
            "Silvally-Poison @ Poison Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Poison @ Poison Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Bold",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Ground @ Ground Memory
        {
            "Silvally-Ground @ Ground Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Ground @ Ground Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Bold",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Rock @ Rock Memory
        {
            "Silvally-Rock @ Rock Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Rock @ Rock Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Quirky",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Bug @ Bug Memory
        {
            "Silvally-Bug @ Bug Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Bug @ Bug Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Rash",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Ghost @ Ghost Memory
        {
            "Silvally-Ghost @ Ghost Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Ghost @ Ghost Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Hasty",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Steel @ Steel Memory
        {
            "Silvally-Steel @ Steel Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Steel @ Steel Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Quiet",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Fire @ Fire Memory
        {
            "Silvally-Fire @ Fire Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Fire @ Fire Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Mild",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Water @ Water Memory
        {
            "Silvally-Water @ Water Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Water @ Water Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Bashful",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Grass @ Grass Memory
        {
            "Silvally-Grass @ Grass Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Grass @ Grass Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Relaxed",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Electric @ Electric Memory
        {
            "Silvally-Electric @ Electric Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Electric @ Electric Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Timid",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Psychic @ Psychic Memory
        {
            "Silvally-Psychic @ Psychic Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Psychic @ Psychic Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Rash",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Ice @ Ice Memory
        {
            "Silvally-Ice @ Ice Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Ice @ Ice Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Modest",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Dragon @ Dragon Memory
        {
            "Silvally-Dragon @ Dragon Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Dragon @ Dragon Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Lonely",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Dark @ Dark Memory
        {
            "Silvally-Dark @ Dark Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Dark @ Dark Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Quirky",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Silvally-Fairy @ Fairy Memory
        {
            "Silvally-Fairy @ Fairy Memory",
            new PKMSWSHShowdownSets
            {
                Name = "Silvally",
                Form = "Fairy @ Fairy Memory",
                Ability = "RKS System",
                Level = "95",
                Shiny = "No",
                Nature = "Gentle",
                Moves = new List<string> { "Tri Attack", "X-Scissor", "Iron Head", "Take Down" }
            }
        },

        // Turtonator
        {
            "Turtonator",
            new PKMSWSHShowdownSets
            {
                Name = "Turtonator",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Smog", "Tackle" }
            }
        },

        // Togedemaru
        {
            "Togedemaru",
            new PKMSWSHShowdownSets
            {
                Name = "Togedemaru",
                Gender = "M",
                Ability = "Iron Barbs",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Nuzzle", "Tackle" }
            }
        },

        // Mimikyu
        {
            "Mimikyu",
            new PKMSWSHShowdownSets
            {
                Name = "Mimikyu",
                Gender = "M",
                Ability = "Disguise",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Splash", "Astonish", "Scratch", "Copycat" }
            }
        },

        // Drampa
        {
            "Drampa",
            new PKMSWSHShowdownSets
            {
                Name = "Drampa",
                Gender = "M",
                Ability = "Sap Sipper",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Play Nice", "Echoed Voice" }
            }
        },

        // Dhelmise
        {
            "Dhelmise",
            new PKMSWSHShowdownSets
            {
                Name = "Dhelmise",
                Ability = "Steelworker",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Absorb", "Rapid Spin" }
            }
        },

        // Jangmo-o (M)
        {
            "Jangmo-o (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Jangmo",
                Form = "o (M)",
                Ability = "Soundproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Hakamo-o (M)
        {
            "Hakamo-o (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Hakamo",
                Form = "o (M)",
                Ability = "Bulletproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Kommo-o (M)
        {
            "Kommo-o (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Kommo",
                Form = "o (M)",
                Ability = "Soundproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Tapu Koko
        {
            "Tapu Koko",
            new PKMSWSHShowdownSets
            {
                Name = "Tapu Koko",
                Ability = "Electric Surge",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Thunderbolt", "Quick Attack", "Brave Bird", "Taunt" }
            }
        },

        // Tapu Lele
        {
            "Tapu Lele",
            new PKMSWSHShowdownSets
            {
                Name = "Tapu Lele",
                Ability = "Psychic Surge",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Psychic", "Play Rough", "Magic Room", "Charm" }
            }
        },

        // Tapu Bulu
        {
            "Tapu Bulu",
            new PKMSWSHShowdownSets
            {
                Name = "Tapu Bulu",
                Ability = "Grassy Surge",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Superpower", "Megahorn", "Wood Hammer", "Scary Face" }
            }
        },

        // Tapu Fini
        {
            "Tapu Fini",
            new PKMSWSHShowdownSets
            {
                Name = "Tapu Fini",
                Ability = "Misty Surge",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Whirlpool", "Water Pulse", "Brine", "Moonblast" }
            }
        },

        // Cosmog
        {
            "Cosmog",
            new PKMSWSHShowdownSets
            {
                Name = "Cosmog",
                Ability = "Unaware",
                Level = "95",
                Shiny = "No",
                Nature = "Lonely",
                Moves = new List<string> { "Splash", "Teleport" }
            }
        },

        // Cosmoem
        {
            "Cosmoem",
            new PKMSWSHShowdownSets
            {
                Name = "Cosmoem",
                Ability = "Sturdy",
                Level = "95",
                Shiny = "No",
                Nature = "Naughty",
                Moves = new List<string> { "Splash", "Teleport" }
            }
        },

        // Solgaleo
        {
            "Solgaleo",
            new PKMSWSHShowdownSets
            {
                Name = "Solgaleo",
                Ability = "Full Metal Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Zen Headbutt", "Fire Spin", "Iron Tail", "Noble Roar" }
            }
        },

        // Lunala
        {
            "Lunala",
            new PKMSWSHShowdownSets
            {
                Name = "Lunala",
                Ability = "Shadow Shield",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Shadow Ball", "Moonblast", "Magic Coat", "Swift" }
            }
        },

        // Nihilego
        {
            "Nihilego",
            new PKMSWSHShowdownSets
            {
                Name = "Nihilego",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Wonder Room", "Sludge Wave", "Brutal Swing", "Acid Spray" }
            }
        },

        // Buzzwole
        {
            "Buzzwole",
            new PKMSWSHShowdownSets
            {
                Name = "Buzzwole",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Power-Up Punch", "Taunt", "Leech Life", "Dynamic Punch" }
            }
        },

        // Pheromosa
        {
            "Pheromosa",
            new PKMSWSHShowdownSets
            {
                Name = "Pheromosa",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "High Jump Kick", "Swift", "Throat Chop", "Lunge" }
            }
        },

        // Xurkitree
        {
            "Xurkitree",
            new PKMSWSHShowdownSets
            {
                Name = "Xurkitree",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Power Whip", "Discharge", "Eerie Impulse", "Brutal Swing" }
            }
        },

        // Celesteela
        {
            "Celesteela",
            new PKMSWSHShowdownSets
            {
                Name = "Celesteela",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Leech Seed", "Smack Down", "Gyro Ball", "Earthquake" }
            }
        },

        // Kartana
        {
            "Kartana",
            new PKMSWSHShowdownSets
            {
                Name = "Kartana",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Vacuum Wave", "Air Cutter", "Leaf Blade", "Swords Dance" }
            }
        },

        // Guzzlord
        {
            "Guzzlord",
            new PKMSWSHShowdownSets
            {
                Name = "Guzzlord",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Dragon Rush", "Stomping Tantrum", "Brutal Swing", "Mega Punch" }
            }
        },

        // Necrozma
        {
            "Necrozma",
            new PKMSWSHShowdownSets
            {
                Name = "Necrozma",
                Ability = "Prism Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Psycho Cut", "Charge Beam", "Power Gem", "Autotomize" }
            }
        },

        // Magearna-Original
        {
            "Magearna-Original",
            new PKMSWSHShowdownSets
            {
                Name = "Magearna",
                Form = "Original",
                Ability = "Soul-Heart",
                Level = "95",
                Shiny = "No",
                Nature = "Mild",
                Moves = new List<string> { "Fleur Cannon", "Flash Cannon", "Defense Curl", "Rest" }
            }
        },

        // Marshadow
        {
            "Marshadow",
            new PKMSWSHShowdownSets
            {
                Name = "Marshadow",
                Ability = "Technician",
                Level = "95",
                Shiny = "No",
                Nature = "Brave",
                Moves = new List<string> { "Spectral Thief", "Drain Punch", "Force Palm", "Shadow Sneak" }
            }
        },

        // Poipole
        {
            "Poipole",
            new PKMSWSHShowdownSets
            {
                Name = "Poipole",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "No",
                Nature = "Bold",
                Moves = new List<string> { "Helping Hand", "Acid", "Fury Attack", "Fell Stinger" }
            }
        },

        // Naganadel
        {
            "Naganadel",
            new PKMSWSHShowdownSets
            {
                Name = "Naganadel",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "No",
                Nature = "Modest",
                Moves = new List<string> { "Helping Hand", "Acid", "Fury Attack", "Fell Stinger" }
            }
        },

        // Stakataka
        {
            "Stakataka",
            new PKMSWSHShowdownSets
            {
                Name = "Stakataka",
                Ability = "16 HP / 17 Atk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Rock Slide", "Double-Edge", "Brutal Swing", "Autotomize" }
            }
        },

        // Blacephalon
        {
            "Blacephalon",
            new PKMSWSHShowdownSets
            {
                Name = "Blacephalon",
                Ability = "Beast Boost",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Shadow Claw", "Taunt", "Fire Blast", "Zen Headbutt" }
            }
        },

        // Zeraora
        {
            "Zeraora",
            new PKMSWSHShowdownSets
            {
                Name = "Zeraora",
                Ability = "Volt Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Plasma Fists", "Close Combat", "Blaze Kick", "Outrage" }
            }
        },

        // Grookey
        {
            "Grookey",
            new PKMSWSHShowdownSets
            {
                Name = "Grookey",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Thwackey
        {
            "Thwackey",
            new PKMSWSHShowdownSets
            {
                Name = "Thwackey",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Rillaboom
        {
            "Rillaboom",
            new PKMSWSHShowdownSets
            {
                Name = "Rillaboom",
                Gender = "M",
                Ability = "Overgrow",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Scratch", "Growl" }
            }
        },

        // Scorbunny
        {
            "Scorbunny",
            new PKMSWSHShowdownSets
            {
                Name = "Scorbunny",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Raboot
        {
            "Raboot",
            new PKMSWSHShowdownSets
            {
                Name = "Raboot",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Cinderace
        {
            "Cinderace",
            new PKMSWSHShowdownSets
            {
                Name = "Cinderace",
                Gender = "M",
                Ability = "Blaze",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Sobble
        {
            "Sobble",
            new PKMSWSHShowdownSets
            {
                Name = "Sobble",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Pound", "Growl" }
            }
        },

        // Drizzile
        {
            "Drizzile",
            new PKMSWSHShowdownSets
            {
                Name = "Drizzile",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Pound", "Growl" }
            }
        },

        // Inteleon
        {
            "Inteleon",
            new PKMSWSHShowdownSets
            {
                Name = "Inteleon",
                Gender = "M",
                Ability = "Torrent",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Pound", "Growl" }
            }
        },

        // Skwovet
        {
            "Skwovet",
            new PKMSWSHShowdownSets
            {
                Name = "Skwovet",
                Gender = "M",
                Ability = "Cheek Pouch",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Greedent
        {
            "Greedent",
            new PKMSWSHShowdownSets
            {
                Name = "Greedent",
                Gender = "M",
                Ability = "Cheek Pouch",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Rookidee
        {
            "Rookidee",
            new PKMSWSHShowdownSets
            {
                Name = "Rookidee",
                Gender = "M",
                Ability = "Unnerve",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quiet",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Corvisquire
        {
            "Corvisquire",
            new PKMSWSHShowdownSets
            {
                Name = "Corvisquire",
                Gender = "M",
                Ability = "Unnerve",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Corviknight
        {
            "Corviknight",
            new PKMSWSHShowdownSets
            {
                Name = "Corviknight",
                Gender = "M",
                Ability = "Pressure",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Peck", "Leer" }
            }
        },

        // Blipbug
        {
            "Blipbug",
            new PKMSWSHShowdownSets
            {
                Name = "Blipbug",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Struggle Bug" }
            }
        },

        // Dottler
        {
            "Dottler",
            new PKMSWSHShowdownSets
            {
                Name = "Dottler",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Struggle Bug" }
            }
        },

        // Orbeetle
        {
            "Orbeetle",
            new PKMSWSHShowdownSets
            {
                Name = "Orbeetle",
                Gender = "M",
                Ability = "Swarm",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Struggle Bug" }
            }
        },

        // Nickit
        {
            "Nickit",
            new PKMSWSHShowdownSets
            {
                Name = "Nickit",
                Gender = "M",
                Ability = "Unburden",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Quick Attack", "Tail Whip" }
            }
        },

        // Thievul
        {
            "Thievul",
            new PKMSWSHShowdownSets
            {
                Name = "Thievul",
                Gender = "M",
                Ability = "Run Away",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Quick Attack", "Tail Whip" }
            }
        },

        // Gossifleur
        {
            "Gossifleur",
            new PKMSWSHShowdownSets
            {
                Name = "Gossifleur",
                Gender = "M",
                Ability = "Cotton Down",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Leafage", "Sing" }
            }
        },

        // Eldegoss
        {
            "Eldegoss",
            new PKMSWSHShowdownSets
            {
                Name = "Eldegoss",
                Gender = "M",
                Ability = "Regenerator",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Leafage", "Sing" }
            }
        },

        // Wooloo
        {
            "Wooloo",
            new PKMSWSHShowdownSets
            {
                Name = "Wooloo",
                Gender = "M",
                Ability = "Fluffy",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Dubwool
        {
            "Dubwool",
            new PKMSWSHShowdownSets
            {
                Name = "Dubwool",
                Gender = "M",
                Ability = "Steadfast",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Chewtle
        {
            "Chewtle",
            new PKMSWSHShowdownSets
            {
                Name = "Chewtle",
                Gender = "M",
                Ability = "Shell Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Tackle", "Water Gun" }
            }
        },

        // Drednaw
        {
            "Drednaw",
            new PKMSWSHShowdownSets
            {
                Name = "Drednaw",
                Gender = "M",
                Ability = "Strong Jaw",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Tackle", "Water Gun" }
            }
        },

        // Yamper
        {
            "Yamper",
            new PKMSWSHShowdownSets
            {
                Name = "Yamper",
                Gender = "M",
                Ability = "Ball Fetch",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Boltund
        {
            "Boltund",
            new PKMSWSHShowdownSets
            {
                Name = "Boltund",
                Gender = "M",
                Ability = "Strong Jaw",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Tackle", "Tail Whip" }
            }
        },

        // Rolycoly
        {
            "Rolycoly",
            new PKMSWSHShowdownSets
            {
                Name = "Rolycoly",
                Gender = "M",
                Ability = "Heatproof",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Tackle", "Smokescreen" }
            }
        },

        // Carkol
        {
            "Carkol",
            new PKMSWSHShowdownSets
            {
                Name = "Carkol",
                Gender = "M",
                Ability = "Steam Engine",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Tackle", "Smokescreen" }
            }
        },

        // Coalossal
        {
            "Coalossal",
            new PKMSWSHShowdownSets
            {
                Name = "Coalossal",
                Gender = "M",
                Ability = "Flame Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Tackle", "Smokescreen" }
            }
        },

        // Applin
        {
            "Applin",
            new PKMSWSHShowdownSets
            {
                Name = "Applin",
                Gender = "M",
                Ability = "Ripen",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Withdraw", "Astonish" }
            }
        },

        // Flapple
        {
            "Flapple",
            new PKMSWSHShowdownSets
            {
                Name = "Flapple",
                Gender = "M",
                Ability = "Ripen",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Withdraw", "Astonish" }
            }
        },

        // Appletun
        {
            "Appletun",
            new PKMSWSHShowdownSets
            {
                Name = "Appletun",
                Gender = "M",
                Ability = "Ripen",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Withdraw", "Astonish" }
            }
        },

        // Silicobra
        {
            "Silicobra",
            new PKMSWSHShowdownSets
            {
                Name = "Silicobra",
                Gender = "M",
                Ability = "Shed Skin",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Wrap", "Sand Attack" }
            }
        },

        // Sandaconda
        {
            "Sandaconda",
            new PKMSWSHShowdownSets
            {
                Name = "Sandaconda",
                Gender = "M",
                Ability = "Sand Spit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Wrap", "Sand Attack" }
            }
        },

        // Cramorant
        {
            "Cramorant",
            new PKMSWSHShowdownSets
            {
                Name = "Cramorant",
                Gender = "M",
                Ability = "Gulp Missile",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Peck", "Stockpile", "Swallow", "Spit Up" }
            }
        },

        // Arrokuda
        {
            "Arrokuda",
            new PKMSWSHShowdownSets
            {
                Name = "Arrokuda",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hasty",
                Moves = new List<string> { "Peck", "Aqua Jet" }
            }
        },

        // Barraskewda
        {
            "Barraskewda",
            new PKMSWSHShowdownSets
            {
                Name = "Barraskewda",
                Gender = "M",
                Ability = "Swift Swim",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Peck", "Aqua Jet" }
            }
        },

        // Toxel
        {
            "Toxel",
            new PKMSWSHShowdownSets
            {
                Name = "Toxel",
                Gender = "M",
                Ability = "Static",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Nuzzle", "Growl", "Flail", "Acid" }
            }
        },

        // Toxtricity
        {
            "Toxtricity",
            new PKMSWSHShowdownSets
            {
                Name = "Toxtricity",
                Gender = "M",
                Ability = "Plus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Nuzzle", "Growl", "Flail", "Acid" }
            }
        },

        // Toxtricity-Low-Key (M)
        {
            "Toxtricity-Low-Key (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Toxtricity",
                Form = "Low-Key (M)",
                Ability = "Minus",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Nuzzle", "Growl", "Flail", "Acid" }
            }
        },

        // Sizzlipede
        {
            "Sizzlipede",
            new PKMSWSHShowdownSets
            {
                Name = "Sizzlipede",
                Gender = "M",
                Ability = "Flash Fire",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Ember", "Smokescreen" }
            }
        },

        // Centiskorch
        {
            "Centiskorch",
            new PKMSWSHShowdownSets
            {
                Name = "Centiskorch",
                Gender = "M",
                Ability = "White Smoke",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Ember", "Smokescreen" }
            }
        },

        // Clobbopus
        {
            "Clobbopus",
            new PKMSWSHShowdownSets
            {
                Name = "Clobbopus",
                Gender = "M",
                Ability = "Limber",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Rock Smash", "Leer" }
            }
        },

        // Grapploct
        {
            "Grapploct",
            new PKMSWSHShowdownSets
            {
                Name = "Grapploct",
                Gender = "M",
                Ability = "Limber",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Rock Smash", "Leer" }
            }
        },

        // Sinistea
        {
            "Sinistea",
            new PKMSWSHShowdownSets
            {
                Name = "Sinistea",
                Ability = "Weak Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Astonish", "Withdraw" }
            }
        },

        // Sinistea-Antique
        {
            "Sinistea-Antique",
            new PKMSWSHShowdownSets
            {
                Name = "Sinistea",
                Form = "Antique",
                Ability = "Cursed Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Mega Drain", "Protect", "Sucker Punch", "Aromatherapy" }
            }
        },

        // Polteageist
        {
            "Polteageist",
            new PKMSWSHShowdownSets
            {
                Name = "Polteageist",
                Ability = "Weak Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Astonish", "Withdraw" }
            }
        },

        // Polteageist-Antique
        {
            "Polteageist-Antique",
            new PKMSWSHShowdownSets
            {
                Name = "Polteageist",
                Form = "Antique",
                Ability = "Weak Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Mega Drain", "Protect", "Sucker Punch", "Aromatherapy" }
            }
        },

        // Hatenna
        {
            "Hatenna",
            new PKMSWSHShowdownSets
            {
                Name = "Hatenna",
                Gender = "F",
                Ability = "Healer",
                Level = "95",
                Shiny = "Yes",
                Nature = "Relaxed",
                Moves = new List<string> { "Confusion", "Play Nice" }
            }
        },

        // Hattrem
        {
            "Hattrem",
            new PKMSWSHShowdownSets
            {
                Name = "Hattrem",
                Gender = "F",
                Ability = "Healer",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Confusion", "Play Nice" }
            }
        },

        // Hatterene
        {
            "Hatterene",
            new PKMSWSHShowdownSets
            {
                Name = "Hatterene",
                Gender = "F",
                Ability = "Healer",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lonely",
                Moves = new List<string> { "Confusion", "Play Nice" }
            }
        },

        // Impidimp
        {
            "Impidimp",
            new PKMSWSHShowdownSets
            {
                Name = "Impidimp",
                Gender = "M",
                Ability = "Frisk",
                Level = "95",
                Shiny = "Yes",
                Nature = "Docile",
                Moves = new List<string> { "Fake Out", "Confide" }
            }
        },

        // Morgrem
        {
            "Morgrem",
            new PKMSWSHShowdownSets
            {
                Name = "Morgrem",
                Gender = "M",
                Ability = "Prankster",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Fake Out", "Confide" }
            }
        },

        // Grimmsnarl
        {
            "Grimmsnarl",
            new PKMSWSHShowdownSets
            {
                Name = "Grimmsnarl",
                Gender = "M",
                Ability = "Prankster",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Fake Out", "Confide" }
            }
        },

        // Obstagoon
        {
            "Obstagoon",
            new PKMSWSHShowdownSets
            {
                Name = "Obstagoon",
                Gender = "M",
                Ability = "Guts",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Leer" }
            }
        },

        // Perrserker
        {
            "Perrserker",
            new PKMSWSHShowdownSets
            {
                Name = "Perrserker",
                Gender = "M",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Fake Out", "Growl" }
            }
        },

        // Cursola
        {
            "Cursola",
            new PKMSWSHShowdownSets
            {
                Name = "Cursola",
                Gender = "M",
                Ability = "Weak Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Impish",
                Moves = new List<string> { "Tackle", "Harden" }
            }
        },

        // Sirfetch’d
        {
            "Sirfetch’d",
            new PKMSWSHShowdownSets
            {
                Name = "Sirfetch’d",
                Gender = "M",
                Ability = "Steadfast",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Peck", "Sand Attack" }
            }
        },

        // Mr. Rime
        {
            "Mr. Rime",
            new PKMSWSHShowdownSets
            {
                Name = "Mr. Rime",
                Gender = "M",
                Ability = "Tangled Feet",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Pound", "Copycat" }
            }
        },

        // Runerigus
        {
            "Runerigus",
            new PKMSWSHShowdownSets
            {
                Name = "Runerigus",
                Gender = "M",
                Ability = "Wandering Spirit",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Astonish", "Protect" }
            }
        },

        // Milcery
        {
            "Milcery",
            new PKMSWSHShowdownSets
            {
                Name = "Milcery",
                Gender = "F",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bashful",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie
        {
            "Alcremie",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Gender = "F",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Ruby-Cream (F)
        {
            "Alcremie-Ruby-Cream (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Ruby-Cream (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Matcha-Cream (F)
        {
            "Alcremie-Matcha-Cream (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Matcha-Cream (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Mint-Cream (F)
        {
            "Alcremie-Mint-Cream (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Mint-Cream (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Lemon-Cream (F)
        {
            "Alcremie-Lemon-Cream (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Lemon-Cream (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Salted-Cream (F)
        {
            "Alcremie-Salted-Cream (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Salted-Cream (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Lax",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Ruby-Swirl (F)
        {
            "Alcremie-Ruby-Swirl (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Ruby-Swirl (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Modest",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Caramel-Swirl (F)
        {
            "Alcremie-Caramel-Swirl (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Caramel-Swirl (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Alcremie-Rainbow-Swirl (F)
        {
            "Alcremie-Rainbow-Swirl (F)",
            new PKMSWSHShowdownSets
            {
                Name = "Alcremie",
                Form = "Rainbow-Swirl (F)",
                Ability = "Sweet Veil",
                Level = "95",
                Shiny = "Yes",
                Nature = "Sassy",
                Moves = new List<string> { "Tackle", "Aromatic Mist" }
            }
        },

        // Falinks
        {
            "Falinks",
            new PKMSWSHShowdownSets
            {
                Name = "Falinks",
                Ability = "Battle Armor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Protect" }
            }
        },

        // Pincurchin
        {
            "Pincurchin",
            new PKMSWSHShowdownSets
            {
                Name = "Pincurchin",
                Gender = "M",
                Ability = "Lightning Rod",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Peck", "Thunder Shock" }
            }
        },

        // Snom
        {
            "Snom",
            new PKMSWSHShowdownSets
            {
                Name = "Snom",
                Gender = "M",
                Ability = "Shield Dust",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naive",
                Moves = new List<string> { "Powder Snow", "Struggle Bug" }
            }
        },

        // Frosmoth
        {
            "Frosmoth",
            new PKMSWSHShowdownSets
            {
                Name = "Frosmoth",
                Gender = "M",
                Ability = "Shield Dust",
                Level = "95",
                Shiny = "Yes",
                Nature = "Mild",
                Moves = new List<string> { "Powder Snow", "Struggle Bug" }
            }
        },

        // Stonjourner
        {
            "Stonjourner",
            new PKMSWSHShowdownSets
            {
                Name = "Stonjourner",
                Gender = "M",
                Ability = "Power Spot",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Rock Throw", "Block" }
            }
        },

        // Eiscue
        {
            "Eiscue",
            new PKMSWSHShowdownSets
            {
                Name = "Eiscue",
                Gender = "M",
                Ability = "Ice Face",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Powder Snow", "Tackle" }
            }
        },

        // Indeedee
        {
            "Indeedee",
            new PKMSWSHShowdownSets
            {
                Name = "Indeedee",
                Gender = "M",
                Ability = "Synchronize",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Stored Power", "Play Nice" }
            }
        },

        // Morpeko
        {
            "Morpeko",
            new PKMSWSHShowdownSets
            {
                Name = "Morpeko",
                Gender = "M",
                Ability = "Hunger Switch",
                Level = "95",
                Shiny = "Yes",
                Nature = "Bold",
                Moves = new List<string> { "Thunder Shock", "Tail Whip" }
            }
        },

        // Cufant
        {
            "Cufant",
            new PKMSWSHShowdownSets
            {
                Name = "Cufant",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Brave",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Copperajah
        {
            "Copperajah",
            new PKMSWSHShowdownSets
            {
                Name = "Copperajah",
                Gender = "M",
                Ability = "Sheer Force",
                Level = "95",
                Shiny = "Yes",
                Nature = "Gentle",
                Moves = new List<string> { "Tackle", "Growl" }
            }
        },

        // Dracozolt
        {
            "Dracozolt",
            new PKMSWSHShowdownSets
            {
                Name = "Dracozolt",
                Ability = "Hustle",
                Level = "95",
                Shiny = "Yes",
                Nature = "Serious",
                Moves = new List<string> { "Tackle", "Thunder Shock", "Charge" }
            }
        },

        // Arctozolt
        {
            "Arctozolt",
            new PKMSWSHShowdownSets
            {
                Name = "Arctozolt",
                Ability = "Volt Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Powder Snow", "Thunder Shock", "Charge" }
            }
        },

        // Dracovish
        {
            "Dracovish",
            new PKMSWSHShowdownSets
            {
                Name = "Dracovish",
                Ability = "Water Absorb",
                Level = "95",
                Shiny = "Yes",
                Nature = "Careful",
                Moves = new List<string> { "Tackle", "Water Gun", "Protect" }
            }
        },

        // Arctovish
        {
            "Arctovish",
            new PKMSWSHShowdownSets
            {
                Name = "Arctovish",
                Ability = "Ice Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Jolly",
                Moves = new List<string> { "Powder Snow", "Water Gun", "Protect" }
            }
        },

        // Duraludon
        {
            "Duraludon",
            new PKMSWSHShowdownSets
            {
                Name = "Duraludon",
                Gender = "M",
                Ability = "Light Metal",
                Level = "95",
                Shiny = "Yes",
                Nature = "Hardy",
                Moves = new List<string> { "Metal Claw", "Leer" }
            }
        },

        // Dreepy
        {
            "Dreepy",
            new PKMSWSHShowdownSets
            {
                Name = "Dreepy",
                Gender = "M",
                Ability = "Infiltrator",
                Level = "95",
                Shiny = "Yes",
                Nature = "Timid",
                Moves = new List<string> { "Astonish", "Infestation", "Quick Attack", "Bite" }
            }
        },

        // Drakloak
        {
            "Drakloak",
            new PKMSWSHShowdownSets
            {
                Name = "Drakloak",
                Gender = "M",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Calm",
                Moves = new List<string> { "Astonish", "Infestation", "Quick Attack", "Bite" }
            }
        },

        // Dragapult
        {
            "Dragapult",
            new PKMSWSHShowdownSets
            {
                Name = "Dragapult",
                Gender = "M",
                Ability = "Clear Body",
                Level = "95",
                Shiny = "Yes",
                Nature = "Rash",
                Moves = new List<string> { "Astonish", "Infestation", "Quick Attack", "Bite" }
            }
        },

        // Zacian
        {
            "Zacian",
            new PKMSWSHShowdownSets
            {
                Name = "Zacian",
                Ability = "Intrepid Sword",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Iron Head", "Play Rough", "Swords Dance", "Sacred Sword" }
            }
        },

        // Zamazenta
        {
            "Zamazenta",
            new PKMSWSHShowdownSets
            {
                Name = "Zamazenta",
                Ability = "Dauntless Shield",
                Level = "95",
                Shiny = "Yes",
                Nature = "Adamant",
                Moves = new List<string> { "Iron Head", "Close Combat", "Iron Defense", "Wide Guard" }
            }
        },

        // Eternatus
        {
            "Eternatus",
            new PKMSWSHShowdownSets
            {
                Name = "Eternatus",
                Ability = "Pressure",
                Level = "95",
                Shiny = "No",
                Nature = "Adamant",
                Moves = new List<string> { "Cross Poison", "Dragon Pulse", "Flamethrower", "Dynamax Cannon" }
            }
        },

        // Kubfu
        {
            "Kubfu",
            new PKMSWSHShowdownSets
            {
                Name = "Kubfu",
                Gender = "M",
                Ability = "Inner Focus",
                Level = "95",
                Shiny = "No",
                Nature = "Serious",
                Moves = new List<string> { "Rock Smash", "Leer", "Endure", "Focus Energy" }
            }
        },

        // Urshifu
        {
            "Urshifu",
            new PKMSWSHShowdownSets
            {
                Name = "Urshifu",
                Gender = "M",
                Ability = "Unseen Fist",
                Level = "95",
                Shiny = "No",
                Nature = "Hasty",
                Moves = new List<string> { "Rock Smash", "Leer", "Endure", "Focus Energy" }
            }
        },

        // Urshifu-Rapid-Strike (M)
        {
            "Urshifu-Rapid-Strike (M)",
            new PKMSWSHShowdownSets
            {
                Name = "Urshifu",
                Form = "Rapid-Strike (M)",
                Ability = "Unseen Fist",
                Level = "95",
                Shiny = "No",
                Nature = "Serious",
                Moves = new List<string> { "Rock Smash", "Leer", "Endure", "Focus Energy" }
            }
        },

        // Zarude
        {
            "Zarude",
            new PKMSWSHShowdownSets
            {
                Name = "Zarude",
                Ability = "Leaf Guard",
                Level = "95",
                Shiny = "No",
                Nature = "Sassy",
                Moves = new List<string> { "Close Combat", "Power Whip", "Swagger", "Snarl" }
            }
        },

        // Zarude-Dada
        {
            "Zarude-Dada",
            new PKMSWSHShowdownSets
            {
                Name = "Zarude",
                Form = "Dada",
                Ability = "Leaf Guard",
                Level = "95",
                Shiny = "No",
                Nature = "Adamant",
                Moves = new List<string> { "Jungle Healing", "Hammer Arm", "Power Whip", "Energy Ball" }
            }
        },

        // Regieleki
        {
            "Regieleki",
            new PKMSWSHShowdownSets
            {
                Name = "Regieleki",
                Ability = "Transistor",
                Level = "95",
                Shiny = "Yes",
                Nature = "Naughty",
                Moves = new List<string> { "Thunder Cage", "Electroweb", "Extreme Speed", "Magnet Rise" }
            }
        },

        // Regidrago
        {
            "Regidrago",
            new PKMSWSHShowdownSets
            {
                Name = "Regidrago",
                Ability = "Dragon’s Maw",
                Level = "95",
                Shiny = "Yes",
                Nature = "Quirky",
                Moves = new List<string> { "Dragon Energy", "Dragon Claw", "Hammer Arm", "Laser Focus" }
            }
        },

        // Glastrier
        {
            "Glastrier",
            new PKMSWSHShowdownSets
            {
                Name = "Glastrier",
                Ability = "Chilling Neigh",
                Level = "95",
                Shiny = "No",
                Nature = "Lax",
                Moves = new List<string> { "Thrash", "Taunt", "Double-Edge", "Swords Dance" }
            }
        },

        // Spectrier
        {
            "Spectrier",
            new PKMSWSHShowdownSets
            {
                Name = "Spectrier",
                Ability = "Grim Neigh",
                Level = "95",
                Shiny = "No",
                Nature = "Rash",
                Moves = new List<string> { "Thrash", "Disable", "Double-Edge", "Nasty Plot" }
            }
        },

        // Calyrex
        {
            "Calyrex",
            new PKMSWSHShowdownSets
            {
                Name = "Calyrex",
                Ability = "Unnerve",
                Level = "95",
                Shiny = "No",
                Nature = "Bold",
                Moves = new List<string> { "Giga Drain", "Psychic", "Psyshock", "Heal Pulse" }
            }
        },
    };
};

