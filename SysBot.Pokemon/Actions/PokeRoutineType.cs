namespace SysBot.Pokemon
{
    /// <summary>
    /// Type of routine the Bot carries out.
    /// </summary>
    public enum PokeRoutineType
    {
        /// <summary> Sits idle waiting to be re-tasked. </summary>
        Idle = 0,

        /// <summary> Performs random trades using a predetermined pool of data. </summary>
        SurpriseTrade = 1,

        /// <summary> Performs the behavior of all trade bots. </summary>
        FlexTrade = 2,
        /// <summary> Performs only P2P Link Trades of specific data. </summary>
        LinkTrade = 3,
        /// <summary> Performs a seed check without transferring data from the bot. </summary>
        SeedCheck = 4,
        /// <summary> Performs a clone operation on the partner's data, sending them a copy of what they show. </summary>
        Clone = 5,
        /// <summary> Exports files for all data shown to the bot. </summary>
        Dump = 6,

        /// <summary> Retrieves eggs from the Day Care. </summary>
        EggFetch = 7,

        /// <summary> Revives fossils until the criteria is satisfied. </summary>
        FossilBot = 8,

        /// <summary> Performs group battles as a host. </summary>
        RaidBot = 9,

        /// <summary> Triggers encounters until the criteria is satisfied. </summary>
        EncounterLine = 1_000,

        /// <summary> Triggers encounters with Eternatus until the criteria is satisfied. </summary>
        Reset = 1_001,

        /// <summary> Triggers encounters with Sword &amp; Shield box legend until the criteria is satisfied. </summary>
        Dogbot = 1_002,

        /// <summary> Similar to idle, but identifies the bot as available for Remote input (Twitch Plays, etc). </summary>
        RemoteControl = 6_000,

        // Add your own custom bots here so they don't clash for future main-branch bot releases.

        /// <summary> Searches, injects, or skips to den seeds. </summary>
        DenBot = 6001,

        /// <summary> Attempts to fix advert names and minor legality issues of what a trade partner shows. </summary>
        FixOT = 6002,

        /// <summary> Discord mini-game that generates random Pokémon. </summary>
        TradeCord = 6003,

        /// <summary> Automates Dynamax Adventures. </summary>
        LairBot = 6004,

        /// <summary> Easily and quickly resets various in-game flags. </summary>
        BoolBot = 6005,

        /// <summary> Easily and quickly resets various in-game flags. </summary>
        CurryBot = 6006,

        /// <summary> Resets Swords Of Justice via the camp method. </summary>
        SoJCamp = 6007,

        /// <summary> Resets Swords Of Justice via the camp method. </summary>
        RollingRaid = 6008,

        /// <summary> Resets Swords Of Justice via the camp method. </summary>
        EtumrepDump = 6009,
    }

    public static class PokeRoutineTypeExtensions
    {
        public static bool IsTradeBot(this PokeRoutineType type) => type is (>=PokeRoutineType.FlexTrade and <= PokeRoutineType.Dump) || type is PokeRoutineType.FixOT or PokeRoutineType.TradeCord or PokeRoutineType.EtumrepDump;
    }
}
