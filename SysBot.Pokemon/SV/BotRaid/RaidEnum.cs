namespace SysBot.Pokemon
{
    public enum DTFormat
    {
        MMDDYY,
        DDMMYY,
        YYMMDD,
    }
    public enum TeraCrystalType : int
    {
        Base = 0,
        Black = 1,
        Distribution = 2,
        Might = 3,
    }
    public enum LobbyMethodOptions
    {
        OpenLobby,
        SkipRaid,
        ContinueRaid,
    }

    public enum RaidAction
    {
        AFK,
        MashA,
        TurboA,
    }
}