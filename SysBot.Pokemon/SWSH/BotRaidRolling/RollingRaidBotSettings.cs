using System.Collections.Generic;
using PKHeX.Core;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class RollingRaidSettings : IBotStateSettings, ICountSettings
    {
        private const string Hosting = nameof(Hosting);
        private const string Counts = nameof(Counts);
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "Rolling Raid Bot Settings";

        [Category(Hosting), Description("Den ID (1 - 100 if Vanilla, 1 - 90 if IoA, 1 - 86 if CT).")]
        public uint DenID { get; set; } = 1;

        [Category(Hosting), Description("Select Den Type.")]
        public DenType DenType { get; set; } = DenType.Vanilla;

        [Category(Hosting), Description("Specify Pokémon species to stop rolling on and to soft-lock host via airplane mode.")]
        public Species SoftLockSpecies { get; set; } = Species.None;

        [Category(Hosting), Description("Will hard-lock on specified species. This will save your game.")]
        public Species HardLockSpecies { get; set; } = Species.None;

        [Category(FeatureToggle), Description("If SoftLockSpecies or HardLockSpecies is enabled, specify whether to lock on a Gmax version of that species.")]
        public bool GmaxLock { get; set; } = false;

        [Category(FeatureToggle), Description("If SoftLockSpecies or HardLockSpecies is enabled, enter a valid form to lock on for those species (or blank if doesn't matter).")]
        public string FormLock { get; set; } = string.Empty;

        [Category(FeatureToggle), Description("Enter the guaranteed flawless IV count to fine-tune what you want to lock on. \"-1\" means anything is fine.")]
        public int GuaranteedIVLock { get; set; } = -1;

        [Category(FeatureToggle), Description("If using USB-Botbase, quit out the raid by toggling airplane mode.")]
        public bool AirplaneQuitout { get; set; } = false;

        [Category(Hosting), Description("Days to skip before hosting. Default is 3.")]
        public int DaysToRoll { get; set; } = 3;

        [Category(Hosting), Description("Additional delay between rolls in milliseconds. Base delay is 500 ms.")]
        public int DateAdvanceDelay { get; set; } = 500;

        [Category(Hosting), Description("If enabled, it will check if your den unexpectedly has watts appear. If watts appear, it will attempt to fix it.")]
        public bool RolloverPrevention { get; set; } = false;

        [Category(Hosting), Description("If enabled, it will re-host a rolled lobby if no one readies up in time instead of restarting the game to save time.")]
        public bool RehostEmptyLobby { get; set; } = false;

        [Category(Hosting), Description("Minimum amount of seconds to wait before starting a raid. Ranges from 0 to 180 seconds.")]
        public int MinTimeToWait { get; set; } = 90;

        [Category(Hosting), Description("Minimum Link Code to host the raid with. Set this to -1 to host with no code.")]
        public int MinRaidCode { get; set; } = 8180;

        [Category(Hosting), Description("Maximum Link Code to host the raid with. Set this to -1 to host with no code.")]
        public int MaxRaidCode { get; set; } = 8199;

        [Category(FeatureToggle), Description("Optional description of the raid the bot is hosting. Uses automatic Pokémon detection if left blank.")]
        public string RaidDescription { get; set; } = string.Empty;

        [Category(FeatureToggle), Description("Echoes each party member as they lock into a Pokémon.")]
        public bool EchoPartyReady { get; set; } = false;

        [Category(FeatureToggle), Description("If enabled, it will yell at players for taking too long to ready up, among other things.")]
        public bool RaidSasser { get; set; } = false;

        [Category(FeatureToggle), Description("Allows the bot to echo your Friend Code if set.")]
        public string FriendCode { get; set; } = string.Empty;

        [Category(Hosting), Description("Number of friend requests to accept each time.")]
        public int NumberFriendsToAdd { get; set; } = 0;

        [Category(Hosting), Description("Number of friends to delete each time.")]
        public int NumberFriendsToDelete { get; set; } = 0;

        [Category(Hosting), Description("Number of raids to host before trying to add/remove friends. Setting a value of 1 will tell the bot to host one raid, then start adding/removing friends.")]
        public int InitialRaidsToHost { get; set; } = 0;

        [Category(Hosting), Description("Number of raids to host between trying to add friends.")]
        public int RaidsBetweenAddFriends { get; set; } = 0;

        [Category(Hosting), Description("Number of raids to host between trying to delete friends.")]
        public int RaidsBetweenDeleteFriends { get; set; } = 0;

        [Category(Hosting), Description("Number of row to start trying to add friends.")]
        public int RowStartAddingFriends { get; set; } = 1;

        [Category(Hosting), Description("Number of row to start trying to delete friends.")]
        public int RowStartDeletingFriends { get; set; } = 1;

        [Category(Hosting), Description("The Nintendo Switch profile you are using to manage friends. For example, set this to 2 if you are using the second profile.")]
        public int ProfileNumber { get; set; } = 1;

        [Category(FeatureToggle), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; } = false;

        [Category(FeatureToggle), Description("When set, the bot will create a text file with current Raid Code for OBS.")]
        public bool RaidLog { get; set; } = false;

        [Category(Hosting), Description("Enter Discord channel ID(s) to post raid embeds to. Feature has to be initialized via \"$raidEmbed\" after every client restart.")]
        public string RollingRaidEmbedChannels { get; set; } = string.Empty;

        /// <summary>
        /// Gets a random trade code based on the range settings.
        /// </summary>
        public int GetRandomRaidCode() => Util.Rand.Next(MinRaidCode, MaxRaidCode + 1);

        private int _completedRaids;

        [Category(Counts), Description("Raids Started")]
        public int CompletedRaids
        {
            get => _completedRaids;
            set => _completedRaids = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedRaids() => Interlocked.Increment(ref _completedRaids);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedRaids != 0)
                yield return $"Started Raids: {CompletedRaids}";
        }
    }
}
