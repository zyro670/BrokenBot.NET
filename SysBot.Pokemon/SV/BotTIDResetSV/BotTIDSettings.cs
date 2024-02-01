using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon;

public class TIDResetBotSettings : IBotStateSettings, ICountSettings
{
    private const string Feature = nameof(Feature);
    private const string Counts = nameof(Counts);
    public override string ToString() => "TIDReset Settings";

    [Category(Feature), Description("Use a comma to separate entries, do not include spaces or leading 0s. Ex: 123456,13,420,808080,13370")]
    public string DesiredTIDs { get; set; } = string.Empty;

    [Category(Feature), Description("Indicates how long to wait after pressing A to enter the game. [Default: 14s]")]
    public int TimeToLoadScreen { get; set; } = 14000;

    [Category(Feature), Description("Select what language to choose for SWSH save file. [Default: English]")]
    public SWSHLanguage SWSH_LangSelect { get; set; } = SWSHLanguage.English;

    [Category(Feature), Description("Select what avatar to choose for SWSH save file. [Default: English]")]
    public SWSHAvatar SWSH_AvatarSelect { get; set; } = SWSHAvatar.BoyDefault;

    [Category(Feature), Description("Enter the Trainer OT to input for SWSH.")]
    public string SWSH_OT { get; set; } = string.Empty;

    [Category(Feature), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
    public bool ScreenOff { get; set; }

    private int _completedScans;

    [Category(Counts), Description("TIDs Scanned")]
    public int CompletedScans
    {
        get => _completedScans;
        set => _completedScans = value;
    }

    [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public int AddCompletedScans() => Interlocked.Increment(ref _completedScans);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedScans != 0)
            yield return $"TIDs Scanned: {CompletedScans}";
    }

    public enum SWSHLanguage
    {
        English = 0,
        Spanish = 1,
        French = 2,
        Dutch = 3,
        Italian = 4,
        Japanese1 = 5,
        Japanese2 = 6,
        Korean = 7,
        Chinese1 = 8,
        Chinese2 = 9,
    }

    public enum SWSHAvatar
    {
        BoyDefault = 0,
        BoyBlonde = 1,
        BoyDark = 2,
        BoyTan = 3,
        GirlDefault = 4,
        GirlBlonde = 5,
        GirlDark = 6,
        GirlTan = 7,
    }
}
