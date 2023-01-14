using PKHeX.Core;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Text;
using static SysBot.Base.SwitchButton;

namespace SysBot.Pokemon
{
    public class RaidSV : PokeRoutineExecutor9SV, ICountBot
    {
        private readonly PokeTradeHub<PK9> Hub;
        private readonly RaidSettingsSV Settings;
        public ICountSettings Counts => Settings;

        public static CancellationTokenSource RaidSVEmbedSource = new();
        public static bool RaidSVEmbedsInitialized;
        public static ConcurrentQueue<(byte[]?, string, string, string)> EmbedQueue = new();

        public RaidSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
        {
            Hub = hub;
            Settings = hub.Config.RaidSV;
        }

        private string Assembly = "Version: 0.0.2";
        private string TID7 { get; set; } = string.Empty;
        private string TrainerName { get; set; } = string.Empty;
        private string HostName { get; set; } = string.Empty;
        private ulong TrainerNID;
        private string RaidCode { get; set; } = string.Empty;
        private const string Player2 = "[[[main+437ECE0]+48]+E0]";
        private const string Player3 = "[[[main+437ECE0]+48]+110]";
        private const string Player4 = "[[[main+437ECE0]+48]+140]";
        private const string ConnectionStatus = "[main+437E280]+30";
        private const string OverworldPointer = "[[[[main+43A7848]+348]+10]+D8]+28";
        private const string PlayerNIDs = "[[main+43A28F0]+F8]"; // length 32
        private const string RaidCodePointer = "[[[[[[main+437DEC0]+98]]+10]+30]+10]+1A9";
        private uint RaidLobby = 0x0403F4B0;
        private int RaidCount;
        private int ResetCount;
        private int RaidPenaltyCount;
        private int WinCount;
        private int LossCount;
        private const string RaidBlock = "[[main+4384B18]+180]+60";
        private int RaidsAtStart { get; set; }
        private RemoteControlAccessList RaiderBanList => Settings.RaiderBanList;
        private RemoteControlAccessList RaiderAllowList => Settings.RaiderAllowList;
        private bool BannedRaider(ulong uid) => RaiderBanList.Contains(uid);
        private bool AllowedRaider(ulong uid) => RaiderAllowList.Contains(uid);

        private Dictionary<ulong, int> RaidTracker = new();

        private class EmbedInfo
        {
            public string EmbedString { get; set; } = string.Empty;
            public string EmbedFooter { get; set; } = string.Empty;
            public string EmbedTitle { get; set; } = string.Empty;
        }

        public override async Task MainLoop(CancellationToken token)
        {
            RaidTracker = new(); // Create new dictionary each bot start incase we changed the raid to not keep old raider info.
            if (Settings.ConfigureRolloverCorrection)
            {
                await RolloverCorrectionSV(token).ConfigureAwait(false);
                return;
            }

            if (Settings.TimeToWaitPerSlot is < 0 or > 180)
            {
                Log("Time to wait must be between 0 and 180 seconds.");
                return;
            }
            try
            {
                Log("Identifying trainer data of the host console.");
                var sav = await IdentifyTrainer(token).ConfigureAwait(false);
                HostName = sav.OT;
                await InitializeHardware(Settings, token).ConfigureAwait(false);

                Log("Starting main RaidBot loop.");
                await InnerLoop(token).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log(e.Message);
            }

            Log($"Ending {nameof(RaidBot)} loop.");
            await HardStop().ConfigureAwait(false);
        }

        private async Task InnerLoop(CancellationToken token)
        {
            RaidCount = 0;
            ResetCount = 0;
            WinCount = 0;
            LossCount = 0;
            bool partyReady;
            List<ulong> LobbyNIDs = new();
            List<string> LobbyTrainers = new();

            await CountRaids(LobbyNIDs, LobbyTrainers, token).ConfigureAwait(false);
            while (!token.IsCancellationRequested)
            {
                await ClearPlayerHistory(token).ConfigureAwait(false);
                var nids = await InitialTrainerNIDRead(token).ConfigureAwait(false);
                await PrepareForRaid(token).ConfigureAwait(false);
                var lobbyReady = await GetLobbyReady(token).ConfigureAwait(false);
                if (!lobbyReady)
                    continue;
                (partyReady, LobbyNIDs, LobbyTrainers) = await ReadTrainers(nids, token).ConfigureAwait(false);
                if (!partyReady)
                {
                    await RegroupFromBannedUser(token).ConfigureAwait(false);
                    continue;
                }
                await CompleteRaid(LobbyNIDs, LobbyTrainers, token).ConfigureAwait(false);
            }
        }

        public override async Task HardStop()
        {
            RaidSVEmbedsInitialized = false;
            RaidSVEmbedSource.Cancel();
            await CleanExit(Settings, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task CompleteRaid(List<ulong> NIDs, List<string> trainers, CancellationToken token)
        {
            bool isReady = await IsConnectedToLobby(token).ConfigureAwait(false);

            if (isReady)
            {
                int b = 0;
                Log("Preparing for battle!");
                while (await IsConnectedToLobby(token).ConfigureAwait(false))
                {
                    b++;
                    await Click(A, 3_000, token).ConfigureAwait(false);
                    if (b == 10)
                    {
                        Log("Still in battle...");
                        b = 0;
                    }
                }
            }

            Log("Raid lobby disbanded!");

            await Click(B, 0_500, token).ConfigureAwait(false);
            await Click(B, 0_500, token).ConfigureAwait(false);
            await Click(DDOWN, 0_500, token).ConfigureAwait(false);

            Log("Returning to overworld...");

            while (!await IsOnOverworld(token).ConfigureAwait(false))
                await Click(A, 1_000, token).ConfigureAwait(false);

            Log("Back in the overworld, checking if we won or lost.");
            Settings.AddCompletedRaids();

            await CountRaids(NIDs, trainers, token).ConfigureAwait(false);

            ResetCount++;
            await CloseGame(Hub.Config, token).ConfigureAwait(false);

            if (ResetCount == Settings.RollbackTimeAfterThisManyRaids && Settings.RollbackTime)
            {
                Log("Applying rollover correction.");
                await RolloverCorrectionSV(token).ConfigureAwait(false);
                ResetCount = 0;
            }
            await StartGame(Hub.Config, token).ConfigureAwait(false);
        }

        private async Task CountRaids(List<ulong> LobbyNIDs, List<string> initialTrainers, CancellationToken token)
        {
            List<uint> seeds = new List<uint>();
            var ofs = await GetPointerAddress(RaidBlock, token).ConfigureAwait(false);
            var data = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 2304, token).ConfigureAwait(false);
            for (int i = 0; i < 69; i++)
            {
                var seed = BitConverter.ToUInt32(data.Slice(0 + (i * 32), 4));
                if (seed != 0)
                    seeds.Add(seed);
            }
            Log($"Total raids: {seeds.Count}");
            if (RaidCount == 0)
                RaidsAtStart = seeds.Count;

            if (RaidsAtStart > seeds.Count && RaidCount != 0)
            {
                Log("We defeated the raid boss!");
                WinCount++;
                for (int i = 0; i < LobbyNIDs.Count; i++)
                {
                    RaidPenaltyCount = 0;
                    if (RaidTracker.ContainsKey(LobbyNIDs[i]) && LobbyNIDs[i] != 0)
                    {
                        RaidPenaltyCount = RaidTracker[LobbyNIDs[i]] + RaidPenaltyCount + 1;
                        RaidTracker.Remove(LobbyNIDs[i]);
                        RaidTracker.Add(LobbyNIDs[i], RaidPenaltyCount);
                        Log($"Player: {initialTrainers[i]} completed the raid with Penalty Count: {RaidPenaltyCount}.");
                        if (RaidPenaltyCount > Settings.CatchLimit && !RaiderBanList.Contains(LobbyNIDs[i]) && Settings.CatchLimit != 0 && !RaiderAllowList.Contains(LobbyNIDs[i]))
                        {
                            Log($"Player: {initialTrainers[i]} has been added to the banlist for joining {RaidPenaltyCount}x this raid session on {DateTime.Now}.");
                            RaiderBanList.List.Add(new() { ID = LobbyNIDs[i], Name = initialTrainers[i], Comment = $"Exceeded max joins on {DateTime.Now}." });
                        }
                    }
                }
            }
            else if (RaidsAtStart == seeds.Count && RaidCount != 0)
            {
                Log("We lost the raid..");
                LossCount++;
            }
        }

        private async Task PrepareForRaid(CancellationToken token)
        {
            Log("Preparing lobby...");
            if (!await IsOnline(token).ConfigureAwait(false))
            {
                await Click(X, 2_500, token).ConfigureAwait(false);
                await Click(L, 5_000, token).ConfigureAwait(false);
                while (!await IsOnOverworld(token).ConfigureAwait(false))
                    await Click(B, 0_500, token).ConfigureAwait(false);
            }
            await Task.Delay(1_000, token).ConfigureAwait(false);
            if (await IsOnline(token).ConfigureAwait(false))
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                await Click(A, 3_000, token).ConfigureAwait(false);
                await Click(A, 3_000, token).ConfigureAwait(false);
            }

            if (!Settings.CodeTheRaid)
                await Click(DDOWN, 1_000, token).ConfigureAwait(false);

            await Click(A, 6_000, token).ConfigureAwait(false);
        }

        private async Task<bool> GetLobbyReady(CancellationToken token)
        {
            bool isReady = await IsConnectedToLobby(token).ConfigureAwait(false);
            var x = 0;
            if (!isReady)
            {
                Log("Connecting to lobby...");
                while (!await IsConnectedToLobby(token).ConfigureAwait(false))
                {
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    x++;
                    if (x == 30)
                    {
                        Log("Failed to connect to lobby, restarting game incase we were in battle/bad connection.");
                        await Click(A, 1_000, token).ConfigureAwait(false);
                        await CloseGame(Hub.Config, token).ConfigureAwait(false);
                        await StartGame(Hub.Config, token).ConfigureAwait(false);
                        Log("Attempting to restart routine!");
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                    }
                }
            }
            return isReady;
        }
        private async Task<string> GetRaidCode(CancellationToken token)
        {
            var ofs = await GetPointerAddress(RaidCodePointer, token).ConfigureAwait(false);
            var data = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 6, token).ConfigureAwait(false);
            RaidCode = Encoding.ASCII.GetString(data);
            Log("Raid Code: " + RaidCode);
            return $"\nRaid Code: {RaidCode}\n";
        }

        private async Task<ulong[]> InitialTrainerNIDRead(CancellationToken token)
        {
            var nids = new ulong[3];
            var ofs = await GetPointerAddress($"{PlayerNIDs}+8", token).ConfigureAwait(false);
            var data = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 24, token).ConfigureAwait(false);
            for (int i = 0; i < 3; i++)
                nids[i] = BitConverter.ToUInt64(data.Slice(0 + (i * 8), 8), 0);

            RaidCount++;
            return nids;
        }

        private async Task<(bool, List<ulong>, List<string>)> ReadTrainers(ulong[] nids, CancellationToken token)
        {
            var info = new EmbedInfo()
            {
                EmbedString = string.Empty,
            };
            info.EmbedString += Settings.RaidDescription;
            info.EmbedFooter += $"Wins: {WinCount} | Losses: {LossCount} - {Assembly}";
            info.EmbedTitle = Settings.RaidTitleDescription;

            info.EmbedString += await GetRaidCode(token).ConfigureAwait(false);
            info.EmbedString += $"\nHost - {HostName}";
            await Task.Delay(1_000, token).ConfigureAwait(false);
            if (RaidSVEmbedsInitialized)
            {
                var bytes = new byte[0];
                if (Settings.TakeScreenshot)
                    bytes = await SwitchConnection.Screengrab(token).ConfigureAwait(false);
                EmbedQueue.Enqueue((bytes, info.EmbedString, info.EmbedFooter, info.EmbedTitle));
            }
            await Task.Delay(2_000, token).ConfigureAwait(false);
            string value = string.Empty;
            string NID = $"{PlayerNIDs}+8";
            List<string> initialTID = new();
            List<string> initialSID = new();
            List<string> initialTrainers = new();
            List<ulong> LobbyNIDs = new();

            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 0: value = Player2; break;
                    case 1: value = Player3; break;
                    case 2: value = Player4; break;
                }

                var nidofs = await GetPointerAddress(NID, token).ConfigureAwait(false);
                var nidData = await SwitchConnection.ReadBytesAbsoluteAsync(nidofs, 24, token).ConfigureAwait(false);
                TrainerNID = BitConverter.ToUInt64(nidData.Slice(0 + (i * 8), 8), 0);

                var tries = 0;

                while (nids[i] == TrainerNID && TrainerNID == 0)
                {
                    await Task.Delay(1_000, token).ConfigureAwait(false);
                    nidData = await SwitchConnection.ReadBytesAbsoluteAsync(nidofs, 32, token).ConfigureAwait(false);
                    TrainerNID = BitConverter.ToUInt64(nidData.Slice(0 + (i * 8), 8), 0);
                    tries++;

                    if (tries == Settings.TimeToWaitPerSlot)
                        break;
                }

                await Task.Delay(4_000, token).ConfigureAwait(false); // Allow trainers to load into lobby

                var ofs = await GetPointerAddress(value, token).ConfigureAwait(false);
                var Data = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 32, token).ConfigureAwait(false);
                var DisplaySID = BinaryPrimitives.ReadUInt32LittleEndian(Data.AsSpan(0)) / 1_000_000;
                var DisplayTID = BinaryPrimitives.ReadUInt32LittleEndian(Data.AsSpan(0)) % 1_000_000;
                TID7 = DisplayTID.ToString("D6");
                TrainerName = StringConverter8.GetString(Data.AsSpan(8, 24));
                initialTrainers.Add(TrainerName);
                initialTID.Add(TID7);

                if (TrainerNID != 0)
                {
                    LobbyNIDs.Add(TrainerNID);

                    Log($"Player {i + 2} - " + TrainerName + " | TID: " + TID7 + $" | NID: {TrainerNID}");
                    info.EmbedString += $"\nPlayer {i + 2} - " + TrainerName;

                    RaidPenaltyCount = 0;

                    if (BannedRaider(TrainerNID))
                    {
                        var titlemsg = "Raid Canceled Due to Banned User";
                        var msg = $"Banned User: {initialTrainers[i]} was found in the lobby.\nRecreating raid team.";
                        Log(msg);
                        if (RaidSVEmbedsInitialized)
                        {
                            var bytes = new byte[0];
                            if (Settings.TakeScreenshot)
                                bytes = await SwitchConnection.Screengrab(token).ConfigureAwait(false);
                            EmbedQueue.Enqueue((bytes, msg, Assembly, titlemsg));
                        }
                        await Task.Delay(2_000, token).ConfigureAwait(false);
                        return (false, LobbyNIDs, initialTrainers);
                    }

                    if (!RaidTracker.ContainsKey(TrainerNID))
                        RaidTracker.Add(TrainerNID, RaidPenaltyCount);

                }
            }

            Log($"Raid #{RaidCount} is starting!");
            if (!string.IsNullOrEmpty(initialTrainers[0]) && !string.IsNullOrEmpty(initialTrainers[1]) && !string.IsNullOrEmpty(initialTrainers[2]))
            {
                if (string.Equals(initialTrainers[0], initialTrainers[1]) && string.Equals(initialTrainers[1], initialTrainers[2]))
                {
                    info.EmbedTitle = $" 🌟🎩🌟 {initialTrainers[1]} Hat Trick 🌟🎩🌟\n\n" + Settings.RaidTitleDescription;
                }
            }
            await Task.Delay(2_000, token).ConfigureAwait(false);
            if (RaidSVEmbedsInitialized)
            {
                var bytes = new byte[0];
                if (Settings.TakeScreenshot)
                    bytes = await SwitchConnection.Screengrab(token).ConfigureAwait(false);
                EmbedQueue.Enqueue((bytes, info.EmbedString, info.EmbedFooter, info.EmbedTitle));
            }
            await Task.Delay(2_000, token).ConfigureAwait(false);
            return (true, LobbyNIDs, initialTrainers);
        }

        private async Task ClearPlayerHistory(CancellationToken token)
        {
            var trainerdata = new byte[32];
            var ofs = await GetPointerAddress(PlayerNIDs, token).ConfigureAwait(false);
            await SwitchConnection.WriteBytesAbsoluteAsync(trainerdata, ofs, token).ConfigureAwait(false);

        }

        private async Task<bool> IsOnline(CancellationToken token)
        {
            var ofs = await GetPointerAddress(ConnectionStatus, token).ConfigureAwait(false);
            var Data = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 1, token).ConfigureAwait(false);
            return Data[0] == 1;
        }

        private async Task<bool> IsConnectedToLobby(CancellationToken token)
        {
            var Data = await SwitchConnection.ReadBytesMainAsync(RaidLobby, 1, token).ConfigureAwait(false);
            return Data[0] != 0x00; // 0 when in lobby but not connected
        }

        private async Task<bool> IsOnOverworld(CancellationToken token)
        {
            var ofs = await GetPointerAddress(OverworldPointer, token).ConfigureAwait(false);
            var Data = await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 1, token).ConfigureAwait(false);
            return Data[0] == 0x11;
        }

        private async Task RolloverCorrectionSV(CancellationToken token)
        {
            // May try to just roll day back if raid is missing instead of rolling time back every X raids

            for (int i = 0; i < 2; i++)
                await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(A, 1_250, token).ConfigureAwait(false); // Enter settings

            await PressAndHold(DDOWN, 2_000, 0_250, token).ConfigureAwait(false); // Scroll to system settings
            await Click(A, 1_250, token).ConfigureAwait(false);

            await PressAndHold(DDOWN, Settings.TimeToScrollDownForRollover, 0, token).ConfigureAwait(false);
            await Click(DUP, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_250, token).ConfigureAwait(false);
            for (int i = 0; i < 2; i++)
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);
            for (int i = 0; i < 3; i++)// try 1 for Day change instead of 3 for Hour
                await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            for (int i = 0; i < 4; i++)// try 6 instead of 4 if doing Day for Hour
                await Click(A, 0_200, token).ConfigureAwait(false);

            await Click(HOME, 1_000, token).ConfigureAwait(false); // Back to title screen
        }

        private async Task RegroupFromBannedUser(CancellationToken token)
        {
            await Click(B, 1_250, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);
            await Click(A, 1_500, token).ConfigureAwait(false);
            await Click(B, 1_000, token).ConfigureAwait(false);
        }

    }
}
