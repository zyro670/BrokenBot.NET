using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using SysBot.Pokemon.Models;
using System.Data;
using System.Text;


namespace SysBot.Pokemon.Utils
{
    public static class BanService
    {
        public static string NormalizeAndClean(this string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormKD);
            var nonAlpha = "";
            for (int i = 0; i < normalized.Length; i++)
            {
                if (Char.IsLetterOrDigit(normalized, i))
                {
                    nonAlpha += normalized.Substring(i, 1);
                }
            }
            
            var lowered = nonAlpha.ToLower();
            Dictionary<string,string> subTable = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"Files\\substituionTable.json"));

            for (int i = 0; i < lowered.Length; i++)
            {
                char letter = (char)lowered[i];
                subTable.ToList().ForEach(item =>
                {
                    if (item.Value.Contains(letter))
                    {
                        lowered = lowered.Replace(letter, Convert.ToChar(item.Key));
                    }
                });

            }
            return lowered;
        }

        public static int CalculateLevenshteinDistance(string source1, string source2)
        {
            //Console.WriteLine("Raider: " + source1);
            //Console.WriteLine("Banned: " + source2);

            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // return result
            return matrix[source1Length, source2Length];
        }
    
        public static BanCheckResult CheckRaider(string raiderName, List<BannedRaider> banList, List<LanguageData> languages)
        {
            BanCheckResult result = new BanCheckResult()
            {
                RaiderName = raiderName
            };
            
            foreach (BannedRaider bannedUser in banList)
            {
                var levDistance = CalculateLevenshteinDistance(raiderName.NormalizeAndClean(), bannedUser.Name.NormalizeAndClean());
                
                if (levDistance == 0)
                {
                    result.IsBanned = true;
                    result.MatchType = ResultType.IS_EXACTMATCH;
                    result.LevenshteinDistance = levDistance;
                    result.BannedUserName = bannedUser.Name;
                    result.BanReason = bannedUser.Notes;
                    
                    return result;
                }
                int N = bannedUser.Name.NormalizeAndClean().Length;
                double K = N - levDistance;

                LanguageData lang = languages.FirstOrDefault(e => e.Language == bannedUser.Language);
                if (lang == null)
                {
                    throw new Exception($"No language in table matches with banned user. Banned User Language: {bannedUser.Language}.");
                }
                
                DataTable dt = new DataTable();
                var v = dt.Compute(lang.Weight, "");

                double weight = double.Parse(v.ToString());
                //double weight = 1 / 5d;
                var nc2 = Binomial.CDF(weight, N, K-1);
                var log10p = Math.Log10(1 - nc2);

                if (log10p <= Double.Parse(bannedUser.Log10p))
                {
                    result.IsBanned = true;
                    result.MatchType = ResultType.IS_SIMILAR_MATCH;
                    result.LevenshteinDistance = levDistance;
                    result.BannedUserName = bannedUser.Name;
                    result.BanReason = bannedUser.Notes;
                    result.Log10p = log10p;

                    return result;
                }
            }

            result.IsBanned = false;
            result.MatchType = ResultType.NO_MATCH;

            return result;
        }
    }
}