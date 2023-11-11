using System.ComponentModel;
using System.IO;

namespace SysBot.Pokemon
{
    public class FolderSettings : IDumper
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Files = nameof(Files);
        public override string ToString() => "Folder / Dumping Settings";

        [Category(FeatureToggle), Description("When enabled, dumps any received PKM files (trade results) to the DumpFolder.")]
        public bool Dump { get; set; }

        [Category(Files), Description("Source folder: where PKM files to distribute are selected from.")]
        public string DistributeFolder { get; set; } = string.Empty;

        [Category(Files), Description("Source folder: where PKM files to giveaway are selected from.")]
        public string GiveawayFolder { get; set; } = string.Empty;

        [Category(Files), Description("Source folder: where PKM files for the EggTradePool are selected from.")]

        public string EggTradeFolder { get; set; } = string.Empty;

        [Category(Files), Description("Destination folder: where all received PKM files are dumped to.")]
        public string DumpFolder { get; set; } = string.Empty;

        public void CreateDefaults(string path)
        {
            var dump = Path.Combine(path, "dump");
            Directory.CreateDirectory(dump);
            DumpFolder = dump;
            Dump = true;

            var distribute = Path.Combine(path, "distribute");
            Directory.CreateDirectory(distribute);
            DistributeFolder = distribute;

            var giveaway = Path.Combine(path, "giveaway");
            Directory.CreateDirectory(giveaway);
            GiveawayFolder = giveaway;

            var eggTrade = Path.Combine(path, "eggtrade");
            Directory.CreateDirectory(eggTrade);
            EggTradeFolder = eggTrade;

        }
    }
}