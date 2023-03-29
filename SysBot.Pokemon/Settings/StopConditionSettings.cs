using PKHeX.Core;
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

        [Category(StopConditions), Description("Desired spreads, search for nature and IVs. In the format HP/Atk/Def/SpA/SpD/Spe. Use \"x\" for unchecked IVs and \"/\" as a separator.")]
        public List<SearchCondition> SearchConditions { get; set; } = new();

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

        [Category(StopConditions), Description("Enter a Discord channel ID(s) to post shiny result embeds to. Feature has to be initialized via \"$sve\" for EggBotSV or \"$ae\" for ArceusBot after every client restart.")]
        public string ResultsEmbedChannels { get; set; } = string.Empty;

        [Category(StopConditions), Description("If not empty, the provided string will be prepended to the result found log message to Echo alerts for whomever you specify. For Discord, use <@userIDnumber> to mention.")]
        public string MatchFoundEchoMention { get; set; } = string.Empty;

        [Category(StopConditions)]
        public class SearchCondition
        {
            public override string ToString() => $"{(!IsEnabled ? $"{Nature}, condition is disabled" : $"{Nature}, {TargetMinIVs} - {TargetMaxIVs}")}";

            [Category(StopConditions), DisplayName("1. Enabled")]
            public bool IsEnabled { get; set; } = true;

            [Category(StopConditions), DisplayName("2. Nature")]
            public Nature Nature { get; set; }

            [Category(StopConditions), DisplayName("3. Minimum accepted IVs")]
            public string TargetMinIVs { get; set; } = "";

            [Category(StopConditions), DisplayName("4. Maximum accepted IVs")]
            public string TargetMaxIVs { get; set; } = "";
        }

        public static bool EncounterFound<T>(T pk, StopConditionSettings settings, IReadOnlyList<string>? markList) where T : PKM
        {
            // Match Species if they were specified.
            if (settings.StopOnSpecies != Species.None && settings.StopOnSpecies != (Species)pk.Species)
                return false;

            if (settings.StopOnForm.HasValue && settings.StopOnForm != pk.Form)
                return false;

            if (settings.TargetGender != GenderType.Any && (int)settings.TargetGender != pk.Gender)
                return false;

            // Return if it doesn't have a mark or it has an unwanted mark.
            var unmarked = pk is IRibbonIndex m && !HasMark(m);
            var unwanted = markList is not null && pk is IRibbonIndex m2 && settings.IsUnwantedMark(GetMarkName(m2), markList);
            if (settings.MarkOnly && (unmarked || unwanted))
                return false;

            if (settings.ShinyTarget != TargetShinyType.DisableOption)
            {
                var shinymatch = settings.ShinyTarget switch
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
            Span<int> pkIVs = stackalloc int[6];
            pk.GetIVs(pkIVs);
            (pkIVs[5], pkIVs[3], pkIVs[4]) = (pkIVs[3], pkIVs[4], pkIVs[5]);
            var pkIVsArr = pkIVs.ToArray();

            // No search conditions to match
            if (!settings.SearchConditions.Any(s => s.IsEnabled))
                return true;

            return settings.SearchConditions.Any(s =>
                MatchIVs(pkIVsArr, s.TargetMinIVs, s.TargetMaxIVs) &&
                (s.Nature == (Nature)pk.Nature || s.Nature == Nature.Random) &&
                s.IsEnabled);
        }

        private static bool MatchIVs(IReadOnlyList<int> pkIVs, string targetMinIVsStr, string targetMaxIVsStr)
        {
            var targetMinIVs = ReadTargetIVs(targetMinIVsStr, true);
            var targetMaxIVs = ReadTargetIVs(targetMaxIVsStr, false);

            for (var i = 0; i < 6; i++)
            {
                if (targetMinIVs[i] > pkIVs[i] || targetMaxIVs[i] < pkIVs[i])
                    return false;
            }

            return true;
        }

        private static int[] ReadTargetIVs(string splitIVsStr, bool min)
        {
            var targetIVs = new int[6];
            char[] split = { '/' };

            var splitIVs = splitIVsStr.Split(split, StringSplitOptions.RemoveEmptyEntries);

            // Only accept up to 6 values. Fill it in with default values if they don't provide 6.
            // Anything that isn't an integer will be a wild card.
            for (var i = 0; i < 6; i++)
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
            var set = $"{(pk.ShinyXor == 0 ? "■ - " : pk.ShinyXor <= 16 ? "★ - " : "")}{SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 9)}{TradeExtensions<PK9>.FormOutput(pk.Species, pk.Form, out _)}\nIVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}\nNature: {(Nature)pk.Nature} | Gender: {(Gender)pk.Gender}";
            if (pk is IRibbonIndex r)
            {
                var rstring = GetMarkName(r);
                if (!string.IsNullOrEmpty(rstring))
                    set += $"\nPokémon found to have **{GetMarkName(r)}**!";
            }
            return set;
        }

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
            var set = $"\n{alpha}{(pk.ShinyXor == 0 ? "■ - " : pk.ShinyXor <= 16 ? "★ - " : "")}{SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8)}{TradeExtensions<PK8>.FormOutput(pk.Species, pk.Form, out _)}\nNature: {(Nature)pk.Nature} | Gender: {(Gender)pk.Gender}\nEC: {pk.EncryptionConstant:X8} | PID: {pk.PID:X8}\nIVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}";
            return set;
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
