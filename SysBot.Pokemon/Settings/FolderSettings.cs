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

            var RaidFilesSV = Path.Combine(path, "RaidFilesSV");
            Directory.CreateDirectory(RaidFilesSV); 

            var raidsvFilePath = Path.Combine(path, "raidsv.txt");
            var pkparamFilePath = Path.Combine(path, "pkparam.txt");
            var bodyparamFilePath = Path.Combine(path, "bodyparam.txt"); 

            if (File.Exists(raidsvFilePath))
            {
                File.Move(raidsvFilePath, Path.Combine(RaidFilesSV, "raidsv.txt"));
            }
            else
            {
                File.Create(Path.Combine(RaidFilesSV, "raidsv.txt")).Dispose();
            } 

            if (File.Exists(pkparamFilePath))
            {
                File.Move(pkparamFilePath, Path.Combine(RaidFilesSV, "pkparam.txt"));
            }
            else
            {
                File.Create(Path.Combine(RaidFilesSV, "pkparam.txt")).Dispose();
            } 

            if (File.Exists(bodyparamFilePath))
            {
                File.Move(bodyparamFilePath, Path.Combine(RaidFilesSV, "bodyparam.txt"));
            }
            else
            {
                File.Create(Path.Combine(RaidFilesSV, "bodyparam.txt")).Dispose();
            }
        }
    }
}