﻿using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon
{
    public class StopConditionSettings
    {
        private const string StopConditions = nameof(StopConditions);
        public override string ToString() => "Stop Condition Settings";

        [Category(StopConditions), Description("Stops only on Pokémon of this species. No restrictions if set to \"None\".")]
        public Species StopOnSpecies { get; set; }

        [Category(StopConditions), Description("Stops only on Pokémon with this FormID. No restrictions if left blank.")]
        public int? StopOnForm { get; set; }

        [Category(StopConditions), Description("Stop only on Pokémon of the specified gender.")]
        public GenderType TargetGender { get; set; } = GenderType.Any;

        [Category(StopConditions), Description("Stop only on Pokémon of the specified nature.")]
        public Nature TargetNature { get; set; } = Nature.Random;

        [Category(StopConditions), Description("Minimum accepted IVs in the format HP/Atk/Def/SpA/SpD/Spe. Use \"x\" for unchecked IVs and \"/\" as a separator.")]
        public string TargetMinIVs { get; set; } = "";

        [Category(StopConditions), Description("Maximum accepted IVs in the format HP/Atk/Def/SpA/SpD/Spe. Use \"x\" for unchecked IVs and \"/\" as a separator.")]
        public string TargetMaxIVs { get; set; } = "";

        [Category(StopConditions), Description("Selects the shiny type to stop on.")]
        public TargetShinyType ShinyTarget { get; set; } = TargetShinyType.DisableOption;

        [Category(StopConditions), Description("Stop only on Pokémon that have a mark.")]
        public bool MarkOnly { get; set; }

        [Category(StopConditions), Description("List of marks to ignore separated by commas. Use the full name, e.g. \"Uncommon Mark, Dawn Mark, Prideful Mark\".")]
        public string UnwantedMarks { get; set; } = "";

        [Category(StopConditions), Description("Holds Capture button to record a 30 second clip when a matching Pokémon is found by EncounterBot or Fossilbot.")]
        public bool CaptureVideoClip { get; set; }

        [Category(StopConditions), Description("Extra time in milliseconds to wait after an encounter is matched before pressing Capture for EncounterBot or Fossilbot.")]
        public int ExtraTimeWaitCaptureVideo { get; set; } = 10000;

        [Category(StopConditions), Description("If set to TRUE, matches both ShinyTarget and TargetIVs settings. Otherwise, looks for either ShinyTarget or TargetIVs match.")]
        public bool MatchShinyAndIV { get; set; } = true;

        [Category(StopConditions), Description("If not empty, the provided string will be prepended to the result found log message to Echo alerts for whomever you specify. For Discord, use <@userIDnumber> to mention.")]
        public string MatchFoundEchoMention { get; set; } = string.Empty;

        public static bool EncounterFound<T>(T pk, int[] targetminIVs, int[] targetmaxIVs, StopConditionSettings settings, IReadOnlyList<string>? marklist) where T : PKM
        {
            // Match Nature and Species if they were specified.
            if (settings.StopOnSpecies != Species.None && settings.StopOnSpecies != (Species)pk.Species)
                return false;

            if (settings.StopOnForm.HasValue && settings.StopOnForm != pk.Form)
                return false;

            if (settings.TargetGender != GenderType.Any && (int)settings.TargetGender != pk.Gender)
                return false;

            if (settings.TargetNature != Nature.Random && settings.TargetNature != (Nature)pk.Nature)
                return false;

            // Return if it doesn't have a mark or it has an unwanted mark.
            var unmarked = pk is IRibbonIndex m && !HasMark(m);
            var unwanted = marklist is not null && pk is IRibbonIndex m2 && settings.IsUnwantedMark(GetMarkName(m2), marklist);
            if (settings.MarkOnly && (unmarked || unwanted))
                return false;

            if (settings.ShinyTarget != TargetShinyType.DisableOption)
            {
                bool shinymatch = settings.ShinyTarget switch
                {
                    TargetShinyType.AnyShiny => pk.IsShiny,
                    TargetShinyType.NonShiny => !pk.IsShiny,
                    TargetShinyType.StarOnly => pk.IsShiny && pk.ShinyXor != 0,
                    TargetShinyType.SquareOnly => pk.ShinyXor == 0,
                    TargetShinyType.DisableOption => true,
                    _ => throw new ArgumentException(nameof(TargetShinyType)),
                };

                // If we only needed to match one of the criteria and it shinymatch'd, return true.
                // If we needed to match both criteria and it didn't shinymatch, return false.
                if (!settings.MatchShinyAndIV && shinymatch)
                    return true;
                if (settings.MatchShinyAndIV && !shinymatch)
                    return false;
            }

            // Reorder the speed to be last.
            Span<int> pkIVList = stackalloc int[6];
            pk.GetIVs(pkIVList);
            (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);

            for (int i = 0; i < 6; i++)
            {
                if (targetminIVs[i] > pkIVList[i] || targetmaxIVs[i] < pkIVList[i])
                    return false;
            }
            return true;
        }

        public static void InitializeTargetIVs(PokeTradeHubConfig config, out int[] min, out int[] max)
        {
            min = ReadTargetIVs(config.StopConditions, true);
            max = ReadTargetIVs(config.StopConditions, false);
        }

        private static int[] ReadTargetIVs(StopConditionSettings settings, bool min)
        {
            int[] targetIVs = new int[6];
            char[] split = { '/' };

            string[] splitIVs = min
                ? settings.TargetMinIVs.Split(split, StringSplitOptions.RemoveEmptyEntries)
                : settings.TargetMaxIVs.Split(split, StringSplitOptions.RemoveEmptyEntries);

            // Only accept up to 6 values.  Fill it in with default values if they don't provide 6.
            // Anything that isn't an integer will be a wild card.
            for (int i = 0; i < 6; i++)
            {
                if (i < splitIVs.Length)
                {
                    var str = splitIVs[i];
                    if (int.TryParse(str, out var val))
                    {
                        targetIVs[i] = val;
                        continue;
                    }
                }
                targetIVs[i] = min ? 0 : 31;
            }
            return targetIVs;
        }

        private static bool HasMark(IRibbonIndex pk)
        {
            for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
            {
                if (pk.GetRibbon((int)mark))
                    return true;
            }
            return false;
        }

        public static bool HasMark(IRibbonIndex pk, out RibbonIndex result)
        {
            result = default;
            for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
            {
                if (pk.GetRibbon((int)mark))
                {
                    result = mark;
                    return true;
                }
            }
            return false;
        }

        public string GetPrintName(PKM pk)
        {
            var set = ShowdownParsing.GetShowdownText(pk);
            if (pk is IRibbonIndex r)
            {
                var rstring = GetMarkName(r);
                if (!string.IsNullOrEmpty(rstring))
                    set += $"\nPokémon found to have **{GetMarkName(r)}**!";
            }
            return set;
        }

        public string GetSpecialPrintName(PKM pk)
        {
            string markEntryText = "";
            HasMark((IRibbonIndex)pk, out RibbonIndex mark);
            var index = (int)mark - (int)RibbonIndex.MarkLunchtime;
            if (index >= 0)
                markEntryText = MarkTitle[index];
            var set = $"{(pk.ShinyXor == 0 ? "■ - " : pk.ShinyXor <= 16 ? "★ - " : "")}{SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 9)}{TradeExtensions<PK9>.FormOutput(pk.Species, pk.Form, out _)}{markEntryText}\nIVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}\nNature: {(Nature)pk.Nature} | Gender: {(Gender)pk.Gender}";
            if (pk is IRibbonIndex r)
            {
                var rstring = GetMarkName(r);
                if (!string.IsNullOrEmpty(rstring))
                    set += $"\nPokémon has the **{GetMarkName(r)}**!";
            }
            if (pk is PK9 pk9)
            {
                set += $"\nScale: {PokeSizeDetailedUtil.GetSizeRating(pk9.Scale)}";
            }
            return set;
        }

        public readonly string[] MarkTitle =
        {
            " the Peckish"," the Sleepy"," the Dozy"," the Early Riser"," the Cloud Watcher"," the Sodden"," the Thunderstruck"," the Snow Frolicker"," the Shivering"," the Parched"," the Sandswept"," the Mist Drifter",
            " the Chosen One"," the Catch of the Day"," the Curry Connoisseur"," the Sociable"," the Recluse"," the Rowdy"," the Spacey"," the Anxious"," the Giddy"," the Radiant"," the Serene"," the Feisty"," the Daydreamer",
            " the Joyful"," the Furious"," the Beaming"," the Teary-Eyed"," the Chipper"," the Grumpy"," the Scholar"," the Rampaging"," the Opportunist"," the Stern"," the Kindhearted"," the Easily Flustered"," the Driven",
            " the Apathetic"," the Arrogant"," the Reluctant"," the Humble"," the Pompous"," the Lively"," the Worn-Out",
        };

        public static void ReadUnwantedMarks(StopConditionSettings settings, out IReadOnlyList<string> marks) =>
            marks = settings.UnwantedMarks.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

        public virtual bool IsUnwantedMark(string mark, IReadOnlyList<string> marklist) => marklist.Contains(mark);

        public static string GetMarkName(IRibbonIndex pk)
        {
            for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
            {
                if (pk.GetRibbon((int)mark))
                    return RibbonStrings.GetName($"Ribbon{mark}");
            }
            return "";
        }

        public string GetAlphaPrintName(PA8 pk)
        {
            string alpha = string.Empty;
            if (pk.IsAlpha) alpha = $"Alpha - ";
            var set = $"\n{alpha}{(pk.ShinyXor == 0 ? "■ - " : pk.ShinyXor <= 16 ? "★ - " : "") }{SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8)}{TradeExtensions<PK8>.FormOutput(pk.Species, pk.Form, out _)}\nNature: {(Nature)pk.Nature} | Gender: {(Gender)pk.Gender}\nEC: {pk.EncryptionConstant:X8} | PID: {pk.PID:X8}\nIVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}";
            return set;
        }

        public (string[] raidDescription, string raidTitle) GetRaidPrintName(string[] description, PKM pk)
        {
            string raidTitle = "";
            string[] raidDescription = new string[] { };

            if (description.Length > 0)
            {
                raidTitle = description[0];
                raidDescription = description.Skip(1).ToArray(); // Skip the first element and create a new array with the rest of the lines
            }

            string markEntryText = "";
            string markTitle = "";
            string scaleText = "";
            string scaleNumber = "";
            string shinySymbol = pk.ShinyXor == 0 ? "■" : pk.ShinyXor <= 16 ? "★" : "";
            string shinySymbolText = pk.ShinyXor == 0 ? "Square Shiny" : pk.ShinyXor <= 16 ? "Star Shiny" : "";
            string shiny = pk.ShinyXor <= 16 ? "Shiny" : "";
            string species = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 9);
            string IVList = $"{pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}";
            string HP = pk.IV_HP.ToString();
            string ATK = pk.IV_ATK.ToString();
            string DEF = pk.IV_DEF.ToString();
            string SPA = pk.IV_SPA.ToString();
            string SPD = pk.IV_SPD.ToString();
            string SPE = pk.IV_SPE.ToString();
            string nature = (Nature)pk.Nature.ToString();
            string genderSymbol = pk.Gender == 0 ? "♀" : pk.Gender <= 16 ? "♂" : "⚥";
            string genderText = $"{(Gender)pk.Gender}";

            HasMark((IRibbonIndex)pk, out RibbonIndex mark);
            if (mark == RibbonIndex.MarkMightiest)
                markEntryText = "the Unrivaled";
            if (pk is PK9 pkl)
            {
                scaleText = $"{PokeSizeDetailedUtil.GetSizeRating(pkl.Scale)}";
                scaleNumber = pkl.Scale.ToString();
                if (pkl.Scale == 0)
                {
                    markEntryText = "The Teeny";
                    markTitle = "Teeny";
                }
                if (pkl.Scale == 255)
                {
                    markEntryText = "The Great";
                    markTitle = "Jumbo";
                }
            }
            
            for (int i = 0; i < raidDescription.Length; i++)
                {
                    raidDescription[i] = raidDescription[i]
                    .Replace("{markEntryText}", markEntryText)
                    .Replace("{markTitle}", markTitle)
                    .Replace("{scaleText}", scaleText)
                    .Replace("{scaleNumber}", scaleNumber)
                    .Replace("{markEntryText}", markEntryText)
                    .Replace("{shinySymbol}", shinySymbol)
                    .Replace("{shinySymbolText}", shinySymbolText)
                    .Replace("{shinyText}", shiny)
                    .Replace("{species}", species)
                    .Replace("{IVList}", IVList)
                    .Replace("{HP}", HP)
                    .Replace("{ATK}", ATK)
                    .Replace("{DEF}", DEF)
                    .Replace("{SPA}", SPA)
                    .Replace("{SPD}", SPD)
                    .Replace("{SPE}", SPE)
                    .Replace("{nature}", nature)
                    .Replace("{genderSymbol}", genderSymbol)
                    .Replace("{genderText}", genderText); // Replace placeholder with Variable
                }

            raidTitle = raidTitle
                    .Replace("{markEntryText}", markEntryText)
                    .Replace("{markTitle}", markTitle)
                    .Replace("{scaleText}", scaleText)
                    .Replace("{scaleNumber}", scaleNumber)
                    .Replace("{markEntryText}", markEntryText)
                    .Replace("{shinySymbol}", shinySymbol)
                    .Replace("{shinySymbolText}", shinySymbolText)
                    .Replace("{shinyText}", shiny)
                    .Replace("{species}", species)
                    .Replace("{IVList}", IVList)
                    .Replace("{HP}", HP)
                    .Replace("{ATK}", ATK)
                    .Replace("{DEF}", DEF)
                    .Replace("{SPA}", SPA)
                    .Replace("{SPD}", SPD)
                    .Replace("{SPE}", SPE)
                    .Replace("{nature}", nature)
                    .Replace("{genderSymbol}", genderSymbol)
                    .Replace("{genderText}", genderText);

            return (raidDescription, raidTitle);
        }
    }

    public enum TargetShinyType
    {
        DisableOption,  // Doesn't care
        NonShiny,       // Match nonshiny only
        AnyShiny,       // Match any shiny regardless of type
        StarOnly,       // Match star shiny only
        SquareOnly,     // Match square shiny only
    }
}
