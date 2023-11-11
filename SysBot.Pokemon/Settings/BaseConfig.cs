using System.ComponentModel;

namespace SysBot.Pokemon
{
    /// <summary>
    /// Console agnostic settings
    /// </summary>
    public abstract class BaseConfig
    {
        protected const string Operation = nameof(Operation);
        

        [Category(Operation), Description("When enabled, the bot will press the B button occasionally when it is not processing anything (to avoid sleep).")]
        public bool AntiIdle { get; set; }

        [Category(Operation), Description("Skips creating bots when the program is started; helpful for testing integrations.")]
        public bool SkipConsoleBotCreation { get; set; }

        [Category(Operation)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public LegalitySettings Legality { get; set; } = new();

        [Category(Operation)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public FolderSettings Folder { get; set; } = new();

        public abstract bool Shuffled { get; }
    }
}
