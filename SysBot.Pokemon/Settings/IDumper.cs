namespace SysBot.Pokemon
{
    public interface IDumper
    {
        bool Dump { get; set; }
        public bool DumpRaw { get; set; }
        bool DumpShinyOnly { get; set; }
        string DumpFolder { get; set; }
    }
}