using PKHeX.Core;
using SysBot.Base;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsets;

namespace SysBot.Pokemon
{
    public class RollingRaidBot : PokeRoutineExecutor8, ICountBot
    {
        private readonly PokeTradeHub<PK8> Hub;
        private readonly RollingRaidSettings Settings;
        public ICountSettings Counts => Settings;
        private readonly DenUtil.RaidData RaidInfo = new();

        public RollingRaidBot(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg)
        {
            Hub = hub;
            Settings = hub.Config.RollingRaid;
        }

        public static CancellationTokenSource RaidEmbedSource = new();
        public static bool RollingRaidEmbedsInitialized;
        public static (PK8?, string, string, byte[])? EmbedInfo;

        private int encounterCount;
        private bool deleteFriends;
        private bool addFriends;
        private readonly bool[] PlayerReady = new bool[4];
        private string raidBossString = string.Empty;
        private string IVString = string.Empty;
        private string embedString = string.Empty;
        private bool airplaneUsable = false;
        private bool softLock = false;
        private bool hardLock = false;
        private int airplaneLobbyExitCount;
        private int RaidLogCount;
        private uint denOfs = 0;
        private ulong playerNameOfs = 0;
        private PK8? raidPk;
        private LobbyPlayerInfo[] LobbyPlayers = new LobbyPlayerInfo[4];

        private class LobbyPlayerInfo
        {
            public string Name { get; set; } = string.Empty;
            public bool Ready { get; set; }
        }

        public override async Task MainLoop(CancellationToken token)
        {
            if (Settings.MinTimeToWait is < 0 or > 180)
            {
                Log("Time to wait must be between 0 and 180 seconds.");
                return;
            }
            else if (Settings.DaysToRoll < 0)
            {
                Log("Can't go back to the past.");
                return;
            }

            try
            {
                Log("Identifying trainer data of the host console.");
                RaidInfo.TrainerInfo = await IdentifyTrainer(token).ConfigureAwait(false);
                await InitializeHardware(Settings, token).ConfigureAwait(false);

                Log("Reading den data.");
                if (await ReadDenData(token).ConfigureAwait(false))
                {
                    Log("Starting main RollingRaidBot loop.");
                    await InnerLoop(token).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                var msg = $"Message: {e.Message}\nStack trace: {e.StackTrace}\nInner exception: {e.InnerException}";
                Log(msg);
            }

            Log($"Ending {nameof(RollingRaidBot)} loop.");
            await HardStop().ConfigureAwait(false);
        }

        private async Task InnerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.RollingRaid)
            {
                Config.IterateNextRoutine();
                addFriends = false;
                deleteFriends = false;

                if (await CheckIfDayRolled(token).ConfigureAwait(false))
                    return;

                // If they set this to 0, they want to add and remove friends before hosting any raids.
                if (Settings.InitialRaidsToHost == 0 && encounterCount == 0)
                {
                    if (Settings.NumberFriendsToAdd > 0)
                        addFriends = true;
                    if (Settings.NumberFriendsToDelete > 0)
                        deleteFriends = true;

                    if (addFriends || deleteFriends)
                    {
                        // Back out of the game.
                        await Click(B, 0_500, token).ConfigureAwait(false);
                        await Click(HOME, 4_000, token).ConfigureAwait(false);
                        await DeleteAddFriends(token).ConfigureAwait(false);
                        await Click(HOME, 1_000, token).ConfigureAwait(false);
                    }
                }

                encounterCount++;

                // Check if we're scheduled to delete or add friends after this raid is hosted.
                // If we're changing friends, we'll echo while waiting on the lobby to fill up.
                if (Settings.InitialRaidsToHost <= encounterCount)
                {
                    if (Settings.NumberFriendsToAdd > 0 && Settings.RaidsBetweenAddFriends > 0)
                        addFriends = (encounterCount - Settings.InitialRaidsToHost) % Settings.RaidsBetweenAddFriends == 0;
                    if (Settings.NumberFriendsToDelete > 0 && Settings.RaidsBetweenDeleteFriends > 0)
                        deleteFriends = (encounterCount - Settings.InitialRaidsToHost) % Settings.RaidsBetweenDeleteFriends == 0;
                }

                int code = Settings.GetRandomRaidCode();
                if (!await AutoRollDen(code, token).ConfigureAwait(false))
                    return;

                Log($"Raid host {encounterCount} finished.");
                Settings.AddCompletedRaids();

                if (airplaneUsable && (Settings.DaysToRoll == 0 || hardLock || softLock))
                    await ResetRaidAirplaneAsync(token).ConfigureAwait(false);
                else await ResetGameAsync(token).ConfigureAwait(false);
            }
        }

        public override async Task HardStop()
        {
            RollingRaidEmbedsInitialized = false;
            RaidEmbedSource.Cancel();
            await CleanExit(Settings, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<bool> AutoRollDen(int code, CancellationToken token)
        {
            if (!softLock && !hardLock)
            {
                for (int i = 0; i < Settings.DaysToRoll; i++)
                {
                    await DaySkip(token).ConfigureAwait(false);
                    await Task.Delay(0_500 + Settings.DateAdvanceDelay, token).ConfigureAwait(false);
                    Log($"Roll {i + 1}...");
                    if (i == Settings.DaysToRoll - 1)
                        await ResetTime(token).ConfigureAwait(false);
                }
            }

            bool rehost = await HostRaidAsync(code, token).ConfigureAwait(false);
            while (rehost)
            {
                if (await CheckIfDayRolled(token).ConfigureAwait(false))
                    return false;

                rehost = await HostRaidAsync(code, token).ConfigureAwait(false);
            }
            return true;
        }

        private async Task<bool> HostRaidAsync(int code, CancellationToken token)
        {
            bool unexpectedBattle = await CheckDen(token).ConfigureAwait(false);

            // Connect to Y-Comm
            await EnsureConnectedToYComm(Hub.Config, token).ConfigureAwait(false);

            // Because my internet is hot garbage and timings vary between 20 seconds and 3 minutes to connect. Yes, really.
            while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
            {
                await Click(B, 0_500, token).ConfigureAwait(false);
                if (await IsInBattle(token).ConfigureAwait(false))
                {
                    Log("We unexpectedly entered a battle! Trying to escape...");
                    await FleeToOverworld(token).ConfigureAwait(false);
                    return true;
                }
            }

            if (unexpectedBattle)
                await EnsureConnectedToYComm(Hub.Config, token).ConfigureAwait(false);

            // Press A and stall out a bit for the loading
            Log($"Initializing raid for {raidBossString}.");
            await Click(A, 5_000 + Hub.Config.Timings.ExtraTimeLoadRaid, token).ConfigureAwait(false);

            if (code >= 0)
            {
                Log($"Entering Link Code...");
                await Click(PLUS, 1_000, token).ConfigureAwait(false);
                await EnterLinkCode(code, Hub.Config, token).ConfigureAwait(false);
                await Click(PLUS, 2_000, token).ConfigureAwait(false);
                await Click(A, 2_000, token).ConfigureAwait(false);
            }

            var friendAdd = $"Send a friend request to Friend Code **{Settings.FriendCode}** to join in! Friends will be added after this raid.";
            if (addFriends && !string.IsNullOrEmpty(Settings.FriendCode))
            {
                embedString += $"\n\n{friendAdd}";
                if (!RollingRaidEmbedsInitialized)
                    EchoUtil.Echo(friendAdd);
            }

            var linkcodemsg = code < 0 ? "no Link Code" : $"code **{code:0000 0000}**";
            string raiddescmsg = string.IsNullOrEmpty(Settings.RaidDescription) ? raidBossString : "\"" + Settings.RaidDescription + "\"";
            var raidMsg = $"Raid lobby for {raiddescmsg} is open with {linkcodemsg}.";
            embedString += $"\n\n{raidMsg}";

            if (RollingRaidEmbedsInitialized)
            {
                var arr = Connection.Screengrab(token).Result.ToArray();
                EmbedInfo = new(raidPk, IVString + embedString, $"{(string.IsNullOrEmpty(Settings.RaidDescription) ? $"{RaidInfo.TrainerInfo.OT}'s Raid" : raiddescmsg)}", arr);
                await Task.Delay(0_100, token).ConfigureAwait(false);
            }

            // Invite others and wait
            await Click(A, 7_000 + Hub.Config.Timings.ExtraTimeOpenRaid, token).ConfigureAwait(false);

            RaidLog(linkcodemsg, raiddescmsg);
            if (!RollingRaidEmbedsInitialized)
                EchoUtil.Echo(raidMsg);

            var timetowait = Settings.MinTimeToWait * 1_000;
            var timetojoinraid = 175_000 - timetowait;

            Log("Waiting on raid party...");
            bool unexpected = !await GetRaidPartyReady(timetowait, token).ConfigureAwait(false);
            if (unexpected)
                Log("Unexpected error encountered! YComm issues?");

            bool ready = PlayerReady[1] || PlayerReady[2] || PlayerReady[3];
            if (ready && Config.Connection.Protocol == SwitchProtocol.USB && Settings.AirplaneQuitout) // Need at least one player to be ready
                airplaneUsable = true;

            embedString = string.Empty;
            LobbyPlayers = new LobbyPlayerInfo[4];
            for (int i = 0; i < 4; i++)
                PlayerReady[i] = false;

            await Task.Delay(1_000, token).ConfigureAwait(false);
            if (ready && !unexpected)
            {
                EchoUtil.Echo($"Raid is starting now with {linkcodemsg}.");
                await Click(DUP, 1_000, token).ConfigureAwait(false);
                await Click(A, 1_000, token).ConfigureAwait(false);
            }
            else if ((Settings.RehostEmptyLobby || softLock || Settings.DaysToRoll == 0) && !unexpected) // Don't waste time; re-host.
            {
                if (await AirplaneLobbyExit(token).ConfigureAwait(false))
                    return true;
                else return false;
            }
            else if (!unexpected && !Settings.RehostEmptyLobby)
            {
                EchoUtil.Echo("Nobody readied up in time, resetting the game because we want to keep rolling...");
                await ResetGameAsync(token).ConfigureAwait(false);
                return false;
            }

            /* Press A and check if we entered a raid.  If other users don't lock in,
               it will automatically start once the timer runs out. If we don't make it into
               a raid by the end, something has gone wrong and we should quit trying. */
            while (timetojoinraid > 0 && !await IsInBattle(token).ConfigureAwait(false))
            {
                if (await IsOnOverworld(Hub.Config, token).ConfigureAwait(false)) // If overworld, lobby disbanded.
                {
                    EchoUtil.Echo("Lobby disbanded! Recovering...");
                    await Click(B, 0_500, token).ConfigureAwait(false);
                    await Task.Delay(3_000, token).ConfigureAwait(false);
                    airplaneUsable = false;

                    if (await IsOnOverworld(Hub.Config, token).ConfigureAwait(false)) // If still on Overworld, we don't need to do anything special.
                    {
                        Log("Re-hosting the raid.");
                        return true;
                    }

                    if (softLock && await IsGameConnectedToYComm(token).ConfigureAwait(false)) // Otherwise we'll break Y-Comm.
                    {
                        await AirplaneLobbyRecover(token).ConfigureAwait(false);
                        return true;
                    }
                    else
                    {
                        Log("Got stuck somewhere, restarting the game...");
                        return false;
                    }
                }

                await Click(A, 1_000, token).ConfigureAwait(false);
                timetojoinraid -= 1_000;
            }

            Log("Finishing raid routine.");
            await Task.Delay(1_000 + Hub.Config.Timings.ExtraTimeEndRaid, token).ConfigureAwait(false);
            return false;
        }

        private async Task<bool> GetRaidPartyReady(int timetowait, CancellationToken token)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < timetowait)
            {
                for (uint i = 0; i < 4; i++)
                {
                    try
                    {
                        await ConfirmPlayerReady(i, token).ConfigureAwait(false);
                    }
                    catch
                    {
                        return false;
                    }
                }

                if (PlayerReady.All(x => x == true))
                    return true;
            }
            sw.Reset();

            if (Settings.EchoPartyReady && Settings.RaidSasser)
            {
                var notReady = LobbyPlayers.ToList().FindAll(x => x != default && !x.Ready && x.Name != string.Empty);
                if (notReady.Count == 0)
                    return true;

                string[] names = new string[notReady.Count];
                for (int i = 0; i < notReady.Count; i++)
                    names[i] = notReady[i].Name;

                var msg = $"{string.Join(", ", names)}, please ready up! You're making everyone wait!";
                EchoUtil.Echo(msg);
            }
            return true;
        }

        private async Task ConfirmPlayerReady(uint player, CancellationToken token)
        {
            if (PlayerReady[player])
                return;

            ulong pkOfs = 0;
            if (playerNameOfs == 0)
                playerNameOfs = await ParsePointer("[[[[main+28F4060]+190]+60]+140]+98", token).ConfigureAwait(false) - 0xD0; // Thank you for sharing the pointer, Anubis! <3
            if (Settings.RaidSasser)
                pkOfs = await ParsePointer("[[[main+28ED790]+E8]+70]+DF8", token).ConfigureAwait(false);

            var ofs = RaidP0PokemonOffset + (0x30 * player);
            var data = await Connection.ReadBytesAsync(ofs, 4, token).ConfigureAwait(false);
            bool joined = data.Any(x => x > 0);
            if (joined && LobbyPlayers[player] == default)
            {
                LobbyPlayers[player] = new();
                var nameData = await SwitchConnection.ReadBytesAbsoluteAsync(playerNameOfs + (player * 0xD0), 24, token).ConfigureAwait(false);
                if (nameData == null || nameData.Length == 0) // Offset failed, probably YComm or bad timings.
                    return;

                nameData = nameData.Reverse().SkipWhile((x, i) => x == 0 && nameData[nameData.Length - i - 2] == 0).Reverse().ToArray();
                LobbyPlayers[player].Name = Encoding.Unicode.GetString(nameData).Replace("�", "");

                PK8? pk = null;
                if (Settings.EchoPartyReady && Settings.RaidSasser)
                {
                    await Task.Delay(0_100, token).ConfigureAwait(false);
                    var pkData = await SwitchConnection.ReadBytesAbsoluteAsync(pkOfs + (player * 0xD90), 0x158, token).ConfigureAwait(false);
                    pk = (PK8?)PKMConverter.GetPKMfromBytes(pkData);
                    if (pk != null && pk.Species > 0 && pk.Species < 899)
                    {
                        var la = new LegalityAnalysis(pk);
                        var shinySymbol = pk.IsShiny && (pk.ShinyXor == 0 || pk.FatefulEncounter) ? "■" : pk.IsShiny ? "★" : "";
                        var form = TradeExtensions<PK8>.FormOutput(pk.Species, pk.Form, out _);
                        var speciesForm = SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8) + form;
                        var genderStr = $"{(pk.Gender == 0 ? " (M)" : pk.Gender == 1 ? " (F)" : "")}";
                        var nick = $"{(pk.IsNicknamed ? $" named {pk.Nickname}" : "")}";
                        string laMsg = !la.Valid ? $"\nHere's what's wrong with it!\n```{la.Report()}```" : "";

                        bool isAd = TradeExtensions<PK8>.HasAdName(pk, out string ad);
                        string adMsg = isAd ? $" Ew, it has an ad-name ({ad})!" : "";
                        EchoUtil.Echo($"{LobbyPlayers[player].Name} (Player {player + 1}) joined the lobby with{(!la.Valid ? " their madd hacc" : "")} {shinySymbol}{speciesForm}{genderStr}{nick}!{adMsg}{laMsg}");
                    }
                }

                if (Settings.EchoPartyReady && (!Settings.RaidSasser || pk == null))
                    EchoUtil.Echo($"{LobbyPlayers[player].Name} (Player {player + 1}) joined the lobby!");
            }

            // Check if the player has locked in.
            ofs = RaidP0PokemonOffset + (0x30 * player);
            data = await Connection.ReadBytesAsync(ofs + RaidLockedInIncr, 1, token).ConfigureAwait(false);
            if (data[0] == 0 && player != 0)
                return;

            PlayerReady[player] = true;
            LobbyPlayers[player].Ready = true;

            // If we get to here, they're locked in and should have a Pokémon selected.
            if (Settings.EchoPartyReady)
            {
                data = await Connection.ReadBytesAsync(ofs, 2, token).ConfigureAwait(false);
                var dexno = BitConverter.ToUInt16(data, 0);

                data = await Connection.ReadBytesAsync(ofs + RaidAltFormInc, 1, token).ConfigureAwait(false);
                var altformstr = data[0] == 0 ? "" : TradeExtensions<PK8>.FormOutput(dexno, data[0], out _);

                data = await Connection.ReadBytesAsync(ofs + RaidShinyIncr, 1, token).ConfigureAwait(false);
                var shiny = data[0] == 1 ? "★" : "";

                data = await Connection.ReadBytesAsync(ofs + RaidGenderIncr, 1, token).ConfigureAwait(false);
                var gender = data[0] == 0 ? " (M)" : (data[0] == 1 ? " (F)" : "");

                EchoUtil.Echo($"{LobbyPlayers[player].Name} (Player {player + 1}) is ready with {shiny}{(Species)dexno}{altformstr}{gender}!");
            }
        }

        private async Task ResetGameAsync(CancellationToken token)
        {
            playerNameOfs = 0;
            softLock = false;
            airplaneUsable = false;
            Log("Resetting raid by restarting the game");
            await CloseGame(Hub.Config, token).ConfigureAwait(false);

            if (addFriends || deleteFriends)
                await DeleteAddFriends(token).ConfigureAwait(false);

            await StartGame(Hub.Config, token).ConfigureAwait(false);
        }

        private async Task DeleteAddFriends(CancellationToken token)
        {
            await NavigateToProfile(token).ConfigureAwait(false);

            // Delete before adding to avoid deleting new friends.
            if (deleteFriends)
            {
                Log("Deleting friends.");
                await NavigateFriendsMenu(true, token).ConfigureAwait(false);
                for (int i = 0; i < Settings.NumberFriendsToDelete; i++)
                    await DeleteFriend(token).ConfigureAwait(false);
            }

            // If we're deleting friends and need to add friends, it's cleaner to back out 
            // to Home and re-open the profile in case we ran out of friends to delete.
            if (deleteFriends && addFriends)
            {
                Log("Navigating back to add friends.");
                await Click(HOME, 2_000, token).ConfigureAwait(false);
                await NavigateToProfile(token).ConfigureAwait(false);
            }

            if (addFriends)
            {
                Log("Adding friends.");
                await NavigateFriendsMenu(false, token).ConfigureAwait(false);
                for (int i = 0; i < Settings.NumberFriendsToAdd; i++)
                    await AddFriend(token).ConfigureAwait(false);
            }

            addFriends = false;
            deleteFriends = false;
            airplaneLobbyExitCount = 0;
            await Click(HOME, 2_000, token).ConfigureAwait(false);
        }

        // Goes from Home screen hovering over the game to the correct profile
        private async Task NavigateToProfile(CancellationToken token)
        {
            int delay = Hub.Config.Timings.KeypressTime;

            await Click(DUP, delay, token).ConfigureAwait(false);
            for (int i = 1; i < Settings.ProfileNumber; i++)
                await Click(DRIGHT, delay, token).ConfigureAwait(false);
            await Click(A, 2_000, token).ConfigureAwait(false);
        }

        // Gets us on the friend card if it exists after HOME button has been pressed.
        // Should already be on either "Friend List" or "Add Friend"
        private async Task NavigateFriendsMenu(bool delete, CancellationToken token)
        {
            int delay = Hub.Config.Timings.KeypressTime;

            // Go all the way up, then down 1. Reverse for adding friends.
            if (delete)
            {
                for (int i = 0; i < 5; i++)
                    await Click(DUP, delay, token).ConfigureAwait(false);
                await Click(DDOWN, 1_000, token).ConfigureAwait(false);
                await Click(A, 1_000, token).ConfigureAwait(false);

                await NavigateFriends(Settings.RowStartDeletingFriends, 4, token).ConfigureAwait(false);
            }
            else
            {
                for (int i = 0; i < 5; i++)
                    await Click(DDOWN, delay, token).ConfigureAwait(false);
                await Click(DUP, 1_000, token).ConfigureAwait(false);

                // Click into the menu.
                await Click(A, 1_000, token).ConfigureAwait(false);
                await Click(A, 2_500, token).ConfigureAwait(false);

                await NavigateFriends(Settings.RowStartAddingFriends, 5, token).ConfigureAwait(false);
            }
        }

        // Navigates to the specified row and column.
        private async Task NavigateFriends(int row, int column, CancellationToken token)
        {
            int delay = Hub.Config.Timings.KeypressTime;

            if (row == 1)
                return;

            for (int i = 1; i < row; i++)
                await Click(DDOWN, delay, token).ConfigureAwait(false);

            for (int i = 1; i < column; i++)
                await Click(DRIGHT, delay, token).ConfigureAwait(false);
        }

        // Deletes one friend. Should already be hovering over the friend card.
        private async Task DeleteFriend(CancellationToken token)
        {
            await Click(A, 1_500, token).ConfigureAwait(false);
            // Opens Options
            await Click(DDOWN, 0_600, token).ConfigureAwait(false);
            await Click(A, 0_600, token).ConfigureAwait(false);
            // Click "Remove Friend", confirm "Delete", return to next card.
            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 5_000 + Hub.Config.Timings.ExtraTimeDeleteFriend, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);
        }

        // Adds one friend. Timing may need to be adjusted since delays vary with connection.
        private async Task AddFriend(CancellationToken token)
        {
            await Click(A, 3_500 + Hub.Config.Timings.ExtraTimeAddFriend, token).ConfigureAwait(false);
            await Click(A, 3_000 + Hub.Config.Timings.ExtraTimeAddFriend, token).ConfigureAwait(false);
        }

        private async Task ResetRaidAirplaneAsync(CancellationToken token)
        {
            airplaneUsable = false;
            var timer = 60_000;
            Log("Resetting raid by toggling airplane mode.");
            await ToggleAirplane(Hub.Config.Timings.ExtraTimeAirplane, token).ConfigureAwait(false);
            Log("Airplaned out!");

            while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false) && timer > 45)
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                timer -= 1_000;
            }

            while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false) && timer > 0) // If airplaned too late, we might be stuck in raid (move selection)
            {
                await Click(A, 1_000, token).ConfigureAwait(false);
                timer -= 1_000;
            }

            if (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false) && timer <= 0) // Something's gone wrong
            {
                Log("Something went wrong. Restarting by closing the game.");
                await ResetGameAsync(token).ConfigureAwait(false);
                return;
            }

            await Task.Delay(4_000 + Hub.Config.Timings.AirplaneConnectionFreezeDelay, token).ConfigureAwait(false);
            if (addFriends || deleteFriends)
            {
                await Click(HOME, 4_000, token).ConfigureAwait(false);
                await DeleteAddFriends(token).ConfigureAwait(false);
                await Click(HOME, 2_000, token).ConfigureAwait(false);
            }
            Log("Back in the overworld!");
        }

        private async Task<bool> AirplaneLobbyExit(CancellationToken token)
        {
            EchoUtil.Echo("No players readied up in time; exiting lobby...");
            airplaneUsable = false;
            airplaneLobbyExitCount++;

            await Click(B, 0_150, token).ConfigureAwait(false);
            await Click(B, 2_500, token).ConfigureAwait(false);
            await Click(A, 0_150, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);

            var timer = 10_000;
            while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                timer -= 1_000;
                if (timer == 0)
                {
                    Log("Failed to exit the lobby, recovering by restarting the game...");
                    await ResetGameAsync(token).ConfigureAwait(false);
                    return false;
                }
            }

            if (Settings.NumberFriendsToAdd > 0 && Settings.RaidsBetweenAddFriends > 0)
                addFriends = (encounterCount + airplaneLobbyExitCount - Settings.InitialRaidsToHost) % Settings.RaidsBetweenAddFriends == 0;
            if (Settings.NumberFriendsToDelete > 0 && Settings.RaidsBetweenDeleteFriends > 0)
                deleteFriends = (encounterCount + airplaneLobbyExitCount - Settings.InitialRaidsToHost) % Settings.RaidsBetweenDeleteFriends == 0;

            if (addFriends || deleteFriends)
            {
                await Click(HOME, 2_000, token).ConfigureAwait(false);
                await DeleteAddFriends(token).ConfigureAwait(false);
                await Click(HOME, 3_000, token).ConfigureAwait(false);
                await Click(B, 0_500, token).ConfigureAwait(false);
                await Click(B, 0_500, token).ConfigureAwait(false);
            }
            Log("Back in the overworld! Re-hosting the raid.");
            return true;
        }

        private async Task AirplaneLobbyRecover(CancellationToken token)
        {
            await ToggleAirplane(0, token).ConfigureAwait(false); // We could be in lobby, or have invited others, or in a box. Conflicts with ldn_mitm, but we don't need it anyways.
            while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
                await Click(B, 0_500, token).ConfigureAwait(false); // If we airplaned, need to clear errors and leave a box if we were stuck.
            Log("Back in the overworld! Re-hosting the raid.");
        }

        private void RaidLog(string linkcodemsg, string raiddescmsg)
        {
            if (Settings.RaidLog)
            {
                RaidLogCount++;
                File.WriteAllText("RaidCode.txt", $"{raiddescmsg} raid #{RaidLogCount}\n{Settings.FriendCode}\nHosting raid as: {Connection.Label.Split('-')[0]}\nRaid is open with {linkcodemsg}\n------------------------");
            }
        }

        private async Task<bool> CheckIfDayRolled(CancellationToken token)
        {
            if (!Settings.RolloverPrevention || encounterCount == 0)
                return false;

            await Task.Delay(2_000, token).ConfigureAwait(false);
            var denData = await Connection.ReadBytesAsync(denOfs, 0x18, token).ConfigureAwait(false);
            RaidInfo.Den = new RaidSpawnDetail(denData, 0);
            if (!RaidInfo.Den.WattsHarvested)
            {
                playerNameOfs = 0;
                softLock = false;
                Log("Watts appeared in den. Correcting for rollover...");
                await Click(B, 0_250, token).ConfigureAwait(false);
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                Log("Navigating to time settings...");
                await RolloverCorrection(token, true).ConfigureAwait(false);
                Log("Time sync turned off.");
                await StartGame(Hub.Config, token).ConfigureAwait(false);

                denData = await Connection.ReadBytesAsync(denOfs, 0x18, token).ConfigureAwait(false);
                RaidInfo.Den = new RaidSpawnDetail(denData, 0);
                if (RaidInfo.Den.WattsHarvested)
                {
                    await SaveGame(Hub.Config, token).ConfigureAwait(false);
                    Log("Turning time sync back on...");
                    await RolloverCorrection(token).ConfigureAwait(false);
                    Log("Rollover correction complete, resuming hosting routine.");
                    await Task.Delay(2_000, token).ConfigureAwait(false);
                    return false;
                }
                Log("Failed to clear Watts, stopping execution...");
                return true;
            }
            return false;
        }

        private async Task RolloverCorrection(CancellationToken token, bool gameClosed = false)
        {
            if (!gameClosed)
                await Click(HOME, 2_000, token).ConfigureAwait(false);

            for (int i = 0; i < 2; i++)
                await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(A, 1_250, token).ConfigureAwait(false); // Enter settings

            await PressAndHold(DDOWN, 2_000, 0_250, token).ConfigureAwait(false); // Scroll to system settings
            await Click(A, 1_250, token).ConfigureAwait(false);

            await PressAndHold(DDOWN, 0_750, 0_250, token).ConfigureAwait(false); // Scroll to date/time settings
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);

            await Click(A, 1_250, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false); // Turn sync off/on
            if (gameClosed)
            {
                for (int i = 0; i < 2; i++)
                    await Click(DDOWN, 0_150, token).ConfigureAwait(false);
                await Click(A, 1_250, token).ConfigureAwait(false);
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);
                for (int i = 0; i < 6; i++)
                    await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
                await Click(A, 0_750, token).ConfigureAwait(false);
            }

            await Click(HOME, 1_000, token).ConfigureAwait(false); // Back to title screen
            if (!gameClosed)
                await Click(HOME, 2_000, token).ConfigureAwait(false); // Back to game
        }

        private async Task<bool> CheckDen(CancellationToken token)
        {
            var denData = await Connection.ReadBytesAsync(denOfs, 0x18, token).ConfigureAwait(false);
            RaidInfo.Den = new RaidSpawnDetail(denData, 0);

            if (!RaidInfo.Den.WattsHarvested)
                await ClearWatts(token).ConfigureAwait(false);

            bool unexpectedBattle = false;
            while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
            {
                await Click(B, 0_300, token).ConfigureAwait(false);
                if (await IsInBattle(token).ConfigureAwait(false))
                {
                    unexpectedBattle = true;
                    Log("We unexpectedly entered a battle! Trying to escape...");
                    await FleeToOverworld(token).ConfigureAwait(false);
                }
            }

            if (softLock || hardLock)
                return unexpectedBattle;

            bool isEvent = RaidInfo.Den.IsEvent;
            if (isEvent)
                RaidInfo.RaidDistributionEncounter = DenUtil.GetSpawnEventShort(RaidInfo);
            else RaidInfo.RaidEncounter = DenUtil.GetSpawnShort(RaidInfo);

            var species = (Species)(isEvent ? RaidInfo.RaidDistributionEncounter.Species : RaidInfo.RaidEncounter.Species);
            var speciesStr = SpeciesName.GetSpeciesNameGeneration((int)species, 2, 8);

            var form = isEvent ? RaidInfo.RaidDistributionEncounter.AltForm : RaidInfo.RaidEncounter.AltForm;
            var formStr = TradeExtensions<PK8>.FormOutput((int)species, (int)form, out _);
            bool gmax = Settings.GmaxLock && (isEvent ? RaidInfo.RaidDistributionEncounter.IsGigantamax : RaidInfo.RaidEncounter.IsGigantamax);

            var flawless = (uint)(isEvent ? RaidInfo.RaidDistributionEncounter.FlawlessIVs : RaidInfo.RaidEncounter.FlawlessIVs);
            bool flawlessLock = Settings.GuaranteedIVLock <= 0 || Settings.GuaranteedIVLock == flawless;
            IVString = SeedSearchUtil.GetCurrentFrameInfo(RaidInfo, flawless, RaidInfo.Den.Seed, out uint shinyType);

            var shiny = shinyType == 1 ? "\nShiny: Star" : shinyType == 2 ? "\nShiny: Square" : "";
            raidPk = (PK8)AutoLegalityWrapper.GetTrainerInfo<PK8>().GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet($"{speciesStr}{formStr}{(gmax ? "-Gmax" : "")}{shiny}")), out _);

            bool formLock = Settings.FormLock == string.Empty || formStr.ToLower() == Settings.FormLock.ToLower();
            raidBossString = $"{speciesStr}{formStr}{(gmax ? "-Gmax" : "")}";

            bool shared = gmax && formLock && flawlessLock;
            softLock = shared && species == Settings.SoftLockSpecies && Config.Connection.Protocol == SwitchProtocol.USB && Settings.AirplaneQuitout && Settings.HardLockSpecies == Species.None;
            hardLock = shared && species == Settings.HardLockSpecies && Settings.SoftLockSpecies == Species.None;

            if (!unexpectedBattle && (softLock || hardLock))
                EchoUtil.Echo($"{(softLock ? "Soft" : "Hard")} locking on {raidBossString}.");
            else EchoUtil.Echo($"Rolling complete. Raid for {raidBossString} will be going up shortly!");

            if (hardLock && !unexpectedBattle)
                await SaveGame(Hub.Config, token).ConfigureAwait(false);
            return unexpectedBattle;
        }

        private async Task<bool> ReadDenData(CancellationToken token)
        {
            denOfs = DenUtil.GetDenOffset(Settings.DenID, Settings.DenType, out uint denID);
            RaidInfo.DenID = denID;

            var denData = await Connection.ReadBytesAsync(denOfs, 0x18, token).ConfigureAwait(false);
            RaidInfo.Den = new RaidSpawnDetail(denData, 0);
            if (!RaidInfo.Den.WattsHarvested)
            {
                Log("For correct operation, start the bot with Watts cleared. If Watts are cleared and you see this message, make sure you've entered the correct den ID. Stopping routine...");
                return false;
            }

            try
            {
                if (RaidInfo.Den.IsEvent)
                {
                    var eventOfs = DenUtil.GetEventDenOffset((int)Hub.Config.ConsoleLanguage, RaidInfo.DenID, Settings.DenType, out _);
                    var eventData = await Connection.ReadBytesAsync(eventOfs, 0x23D4, token).ConfigureAwait(false);

                    RaidInfo.RaidDistributionEncounter = DenUtil.GetSpawnEvent(RaidInfo, eventData, out FlatbuffersResource.NestHoleDistributionEncounter8Table table);
                    RaidInfo.RaidDistributionEncounterTable = table;
                }
                else
                {
                    RaidInfo.RaidEncounter = DenUtil.GetSpawn(RaidInfo, out FlatbuffersResource.EncounterNest8Table table);
                    RaidInfo.RaidEncounterTable = table;
                }
                return true;
            }
            catch (Exception ex)
            {
                var msg = $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}";
                Log($"Error ocurred while reading raid encounter tables for {(RaidInfo.Den.IsEvent ? "event den" : "den")}:\nID/Type: {Settings.DenID}/{Settings.DenType}\n\n{msg}");
                return false;
            }
        }

        private async Task FleeToOverworld(CancellationToken token)
        {
            await Click(DUP, 0_200, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);

            while (await IsInBattle(token).ConfigureAwait(false))
            {
                await Click(B, 0_500, token).ConfigureAwait(false);
                await Click(B, 1_000, token).ConfigureAwait(false);
                await Click(DUP, 0_200, token).ConfigureAwait(false);
                await Click(A, 1_000, token).ConfigureAwait(false);
            }
        }

        private async Task ClearWatts(CancellationToken token)
        {
            Log("Collecting watts...");
            for (int i = 0; i < 2; i++)
                await Click(A, 1_000 + Hub.Config.Timings.ExtraTimeAButtonClickAR, token).ConfigureAwait(false);
            await Click(A, 2_000 + Hub.Config.Timings.ExtraTimeLoadLobbyAR, token).ConfigureAwait(false);
        }
    }
}