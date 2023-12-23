using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon;

public class TIDResetBotSettingsSV : IBotStateSettings, ICountSettings
{
    private const string Feature = nameof(Feature);
    private const string Counts = nameof(Counts);
    public override string ToString() => "TIDResetSV Settings";

    [Category(Feature), Description("Use a comma to separate entries, do not include spaces or leading 0s. Ex: 123456,13,420,808080,13370")]
    public string DesiredTIDs { get; set; } = string.Empty;

    [Category(Feature), Description("Indicates how long to wait after pressing A to enter the game. [Default: 14s]")]
    public int TimeToLoadScreen { get; set; } = 14000;

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
}
