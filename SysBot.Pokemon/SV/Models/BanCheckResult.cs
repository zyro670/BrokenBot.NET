namespace SysBot.Pokemon.Models
{
    public class BanCheckResult
    {
        public string RaiderName { get; set; }
        public string BannedUserName { get; set; }
        public string BanReason { get; set; }
        public int LevenshteinDistance { get; set; }
        public double Log10p { get; set; }
        public ResultType MatchType { get; set; }
        public bool IsBanned { get; set; }
    }
}

public enum ResultType
{
    IS_EXACTMATCH = 0,
    IS_SIMILAR_MATCH = 1,
    NO_MATCH = 2,
}