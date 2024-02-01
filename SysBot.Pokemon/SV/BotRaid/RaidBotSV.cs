using PKHeX.Core;
using Discord;
using SysBot.Base;
using SysBot.Pokemon.SV;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RaidCrawler.Core.Structures;
using System.Net.Http;
using Newtonsoft.Json;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.OverworldSettingsSV;

namespace SysBot.Pokemon
{
    public class RaidBotSV : PokeRoutineExecutor9SV, ICountBot
    {
        private readonly PokeTradeHub<PK9> Hub;
        private readonly RaidSettingsSV Settings;
        public ICountSettings Counts => Settings;
        private RemoteControlAccessList RaiderBanList => Settings.RaiderBanList;

        public RaidBotSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
        {
            Hub = hub;
            Settings = hub.Config.RaidSV;
        }

        private int RaidCount;
        private int WinCount;
        private int LossCount;
        private int EmptyRaid;
        private int StoryProgress;
        private int EventProgress;
        private int StoredIndex;
        private byte FieldID = 0;
        private ulong OverworldOffset;
        private ulong ConnectedOffset;
        private ulong RaidBlockPointer;
        private int RaidBlockSize = 0;
        private TeraRaidMapParent RaidMap = TeraRaidMapParent.Paldea;
        private static ulong BaseBlockKeyPointer = 0;
        private readonly ulong[] TeraNIDOffsets = new ulong[3];
        private string TeraRaidCode { get; set; } = string.Empty;
        private string BaseDescription = string.Empty;
        private string[] PresetDescription = [];
        private string[] ModDescription = [];
        private List<BanList> GlobalBanList = [];
        private SAV9SV HostSAV = new();
        private DateTime StartTime = DateTime.Now;
        private RaidContainer? container;

        public override async Task MainLoop(CancellationToken token)
        {
            if (string.IsNullOrEmpty(Settings.RaidEmbedFilters.Seed))
            {
                Log("Please enter your seed in the Seed field before starting the bot.");
                return;
            }

            if (Settings.GenerateParametersFromFile)
            {
                GenerateSeedsFromFile();
                Log("Done.");
            }

            if (Settings.UsePresetFile)
            {
                LoadDefaultFile();
                Log("Using Preset file.");
            }

            if (Settings.RolloverFilters.ConfigureRolloverCorrection)
            {
                await RolloverCorrectionSV(token).ConfigureAwait(false);
                return;
            }

            if (Settings.TimeToWait is < 0 or > 180)
            {
                Log("Time to wait must be between 0 and 180 seconds.");
                return;
            }

            if (Settings.RaidsBetweenUpdate == 0 || Settings.RaidsBetweenUpdate < -1)
            {
                Log("Raids between updating the global ban list must be greater than 0, or -1 if you want it off.");
                return;
            }

            try
            {
                InitializeSessionJson();
                Log("Identifying trainer data of the host console.");
                HostSAV = await IdentifyTrainer(token).ConfigureAwait(false);
                await InitializeHardware(Settings, token).ConfigureAwait(false);
                Log("Starting main RaidBot loop.");
                await InnerLoop(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            Log($"Ending {nameof(RaidBotSV)} loop.");
            await HardStop().ConfigureAwait(false);
            return;
        }

        private void LoadDefaultFile()
        {
            var folder = "raidfilessv";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var prevpath = "preset.txt";
            var filepath = "raidfilessv\\preset.txt";
            if (File.Exists(prevpath))
                File.Move(prevpath, filepath);
            if (!File.Exists(filepath))
            {
                File.WriteAllText(filepath, "{shinySymbol} - {species} - {markTitle} - {genderSymbol} - {genderText}" + Environment.NewLine + "{stars} - {difficulty} - {tera}" + Environment.NewLine +
                    "{HP}/{ATK}/{DEF}/{SPA}/{SPD}/{SPE}\n{ability} | {nature}" + Environment.NewLine + "Scale: {scaleText} - {scaleNumber}" + Environment.NewLine + "{moveset}" + Environment.NewLine + "{extramoves}");
            }
            if (File.Exists(filepath))
            {
                PresetDescription = File.ReadAllLines(filepath);
                ModDescription = PresetDescription;
            }
            else
                PresetDescription = Array.Empty<string>();
        }

        private void GenerateSeedsFromFile()
        {
            var folder = "raidfilessv";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            BaseDescription = string.Empty;
            var prevpath = "bodyparam.txt";
            var filepath = "raidfilessv\\bodyparam.txt";
            if (File.Exists(prevpath))
                File.Move(prevpath, filepath);
            if (File.Exists(filepath))
                BaseDescription = File.ReadAllText(filepath);

            var data = string.Empty;
            var prevpk = "pkparam.txt";
            var pkpath = "raidfilessv\\pkparam.txt";
            if (File.Exists(prevpk))
                File.Move(prevpk, pkpath);
            if (File.Exists(pkpath))
                data = File.ReadAllText(pkpath);

            DirectorySearch(data);
        }

        private static void CreateJson(string path)
        {
            // Create new
            List<RaidSessionDetails> _data =
                [
                    new RaidSessionDetails()
                    {
                        Name = string.Empty,
                        ID = 0,
                        Comment = string.Empty,
                        PenaltyCount = 0,
                    }
                ];

            string json = JsonConvert.SerializeObject(_data.ToArray());
            File.WriteAllText(path, json);
        }

        private void DirectorySearch(string data)
        {
            RaidSettingsSV.RaidEmbedFiltersCategory param = new()
            {
                Seed = Settings.RaidEmbedFilters.Seed,
                PartyPK = [data],
            };
            Settings.RaidEmbedFilters = param;
            Log($"Parameters generated for 0x{Settings.RaidEmbedFilters.Seed}.");
        }

        private void InitializeSessionJson()
        {
            var path = "raidfilessv\\temp-session.json";

            if (File.Exists(path))
            {
                Log("Previous temp-session.json found, creating a new one for this session.");
                File.Delete(path);
                CreateJson(path);
            }

            if (!File.Exists(path))
                CreateJson(path);
        }

        public class RaidSessionDetails
        {
            public ulong ID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public int PenaltyCount { get; set; }
        }

        private async Task<bool> VerifyRaidSeed(int index)
        {
            List<long> ptr = [0];
            switch (FieldID)
            {
                case 0:
                    ptr = new(Offsets.RaidBlockPointerP)
                    {
                        [3] = 0x60 + index * 0x20
                    }; break;
                case 1:
                    ptr = new(Offsets.RaidBlockPointerK)
                    {
                        [3] = 0xCE8 + index * 0x20
                    }; break;
                case 2:
                    ptr = new(Offsets.RaidBlockPointerB)
                    {
                        [3] = 0x1968 + index * 0x20
                    }; break;
            }

            var seed = uint.Parse(Settings.RaidEmbedFilters.Seed, NumberStyles.AllowHexSpecifier);
            var currseed = await SwitchConnection.PointerPeek(4, ptr, CancellationToken.None).ConfigureAwait(false);
            if (BitConverter.ToUInt32(currseed) == seed)
            {
                Log($"Raid seed at stored index {StoredIndex} matches our current seed.");
                return true;
            }

            return false;
        }

        private async Task InnerLoop(CancellationToken token)
        {
            bool partyReady;
            List<(ulong, TradeMyStatus)> lobbyTrainers;
            StartTime = DateTime.Now;
            var dayRoll = 0;
            EmptyRaid = 0;
            RaidCount = 0;
            WinCount = 0;
            LossCount = 0;
            var raidsHosted = 0;
            Settings.RaidEmbedFilters.IsSet = false;
            while (!token.IsCancellationRequested)
            {
                // Initialize offsets at the start of the routine and cache them.
                await InitializeSessionOffsets(token).ConfigureAwait(false);
                if (!Settings.RaidEmbedFilters.IsSet)
                {
                    Log($"Preparing parameter for {Settings.RaidEmbedFilters.Species}");
                    bool valid = await ReadRaids(token).ConfigureAwait(false);
                    if (!valid)
                    {
                        Log($"{Settings.RaidEmbedFilters.Seed} was not found among your current raids in game. Please ensure your seed is entered correctly before starting the bot.");
                        return;
                    }
                }
                else
                    Log($"Parameter for {Settings.RaidEmbedFilters.Species} set previously, skipping raid reads.");

                bool raidValid = await VerifyRaidSeed(StoredIndex).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(Settings.GlobalBanListURL))
                    await GrabGlobalBanlist(token).ConfigureAwait(false);

                if (!raidValid)
                {
                    Log("Raid seed invalid, restarting the game.");
                    if (dayRoll != 0)
                    {
                        Log("Stopping routine for lost raid.");
                        return;
                    }
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await Task.Delay(2_500, token).ConfigureAwait(false);
                    await RolloverCorrectionSV(token).ConfigureAwait(false);
                    await Task.Delay(1_500, token).ConfigureAwait(false);
                    await StartGame(Hub.Config, token).ConfigureAwait(false);

                    dayRoll++;
                    continue;
                }

                if (Hub.Config.Stream.CreateAssets)
                    await GetRaidSprite(token).ConfigureAwait(false);

                // Clear NIDs.
                await SwitchConnection.WriteBytesAbsoluteAsync(new byte[32], TeraNIDOffsets[0], token).ConfigureAwait(false);

                // Connect online and enter den.
                if (!await PrepareForRaid(token).ConfigureAwait(false))
                {
                    Log("Failed to prepare the raid, rebooting the game.");
                    await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                    continue;
                }

                // Wait until we're in lobby.
                if (!await GetLobbyReady(token).ConfigureAwait(false))
                    continue;

                try
                {
                    // Read trainers until someone joins.
                    (partyReady, lobbyTrainers) = await ReadTrainers(token).ConfigureAwait(false);
                    if (!partyReady)
                    {
                        // Should add overworld recovery with a game restart fallback.
                        await RegroupFromBannedUser(token).ConfigureAwait(false);

                        if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                        {
                            Log("Something went wrong, attempting to recover.");
                            await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                            continue;
                        }

                        // Clear trainer OTs.
                        Log("Clearing stored OTs");
                        for (int i = 0; i < 3; i++)
                        {
                            List<long> ptr = new(Offsets.Trader2MyStatusPointer);
                            ptr[2] += i * 0x30;
                            await SwitchConnection.PointerPoke(new byte[16], ptr, token).ConfigureAwait(false);
                        }
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Log($"An error occurred while processing the lobby: {ex.Message}");
                    // Should add overworld recovery with a game restart fallback.
                    await RegroupFromBannedUser(token).ConfigureAwait(false);

                    if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    {
                        Log("Something went wrong, attempting to recover.");
                        await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                        continue;
                    }

                    // Clear trainer OTs.
                    Log("Clearing stored OTs");
                    for (int i = 0; i < 3; i++)
                    {
                        List<long> ptr = new(Offsets.Trader2MyStatusPointer);
                        ptr[2] += i * 0x30;
                        await SwitchConnection.PointerPoke(new byte[16], ptr, token).ConfigureAwait(false);
                    }
                    continue;

                }

                await CompleteRaid(lobbyTrainers, token).ConfigureAwait(false);
                raidsHosted++;
                if (raidsHosted == Settings.TotalRaidsToHost && Settings.TotalRaidsToHost != 0)
                    break;
            }
            if (Settings.TotalRaidsToHost != 0 && raidsHosted != 0)
                Log("Total raids to host has been met.");
        }

        public override async Task HardStop()
        {
            await CleanExit(CancellationToken.None).ConfigureAwait(false);
        }

        private async Task GrabGlobalBanlist(CancellationToken token)
        {
            using var httpClient = new HttpClient();
            var url = Settings.GlobalBanListURL;
            var data = await httpClient.GetStringAsync(url, token).ConfigureAwait(false);
            GlobalBanList = JsonConvert.DeserializeObject<List<BanList>>(data)!;
            if (GlobalBanList.Count is not 0)
                Log($"There are {GlobalBanList.Count} entries on the global ban list.");
            else
                Log("Failed to fetch the global ban list. Ensure you have the correct URL.");
        }

        private async Task CompleteRaid(List<(ulong, TradeMyStatus)> trainers, CancellationToken token)
        {
            List<(ulong, TradeMyStatus)> lobbyTrainersFinal = new();
            if (await IsConnectedToLobby(token).ConfigureAwait(false))
            {
                int b = 0;
                Log("Preparing for battle!");
                while (!await IsInRaid(token).ConfigureAwait(false))
                    await Click(A, 1_000, token).ConfigureAwait(false);

                if (await IsInRaid(token).ConfigureAwait(false))
                {
                    // Clear NIDs to refresh player check.
                    await SwitchConnection.WriteBytesAbsoluteAsync(new byte[32], TeraNIDOffsets[0], token).ConfigureAwait(false);
                    await Click(B, 5_000, token).ConfigureAwait(false);

                    // Loop through trainers again in case someone disconnected.
                    for (int i = 0; i < 3; i++)
                    {
                        var player = i + 2;
                        var nidOfs = TeraNIDOffsets[i];
                        var data = await SwitchConnection.ReadBytesAbsoluteAsync(nidOfs, 8, token).ConfigureAwait(false);
                        var nid = BitConverter.ToUInt64(data, 0);

                        if (nid == 0)
                            continue;

                        List<long> ptr = new(Offsets.Trader2MyStatusPointer);
                        ptr[2] += i * 0x30;
                        var trainer = await GetTradePartnerMyStatus(ptr, token).ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(trainer.OT) || HostSAV.OT == trainer.OT)
                            continue;

                        lobbyTrainersFinal.Add((nid, trainer));
                        var tr = trainers.FirstOrDefault(x => x.Item2.OT == trainer.OT);
                        if (tr != default)
                            Log($"Player {i + 2} matches lobby check for {trainer.OT}.");
                        else Log($"New Player {i + 2}: {trainer.OT} | TID: {trainer.DisplayTID} | NID: {nid}.");
                    }
                    var nidDupe = lobbyTrainersFinal.Select(x => x.Item1).ToList();
                    var dupe = lobbyTrainersFinal.Count > 1 && nidDupe.Distinct().Count() == 1;
                    if (dupe)
                    {
                        // We read bad data, reset game to end early and recover.
                        var msg = "Oops! Something went wrong, resetting to recover.";
                        await EnqueueEmbed(null, msg, false, false, false, false, token).ConfigureAwait(false);
                        await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                        return;
                    }

                    var names = lobbyTrainersFinal.Select(x => x.Item2.OT).ToList();
                    bool hatTrick = lobbyTrainersFinal.Count == 3 && names.Distinct().Count() == 1;

                    for (int i = 0; i < 3; i++)
                        await Click(B, 5_000, token).ConfigureAwait(false);
                    await EnqueueEmbed(names, "", hatTrick, false, false, true, token).ConfigureAwait(false);
                }

                while (await IsConnectedToLobby(token).ConfigureAwait(false))
                {
                    b++;
                    switch (Settings.Action)
                    {
                        case RaidAction.AFK: await Task.Delay(3_000, token).ConfigureAwait(false); break;
                        case RaidAction.MashA: await Click(A, 3_500, token).ConfigureAwait(false); break;
                    }

                    if (b % 10 == 0)
                        Log("Still in battle...");

                    if (b == 300 && Settings.RaidEmbedFilters.CrystalType is TeraCrystalType.Might || b == 200 && Settings.RaidEmbedFilters.CrystalType != TeraCrystalType.Might)
                    {
                        string time = b == 200 ? "10 minutes " : "15 minutes ";
                        Log($"We've been stuck in battle for {time}.. Raid frozen? Resetting game!");
                        await CloseGame(Hub.Config, token).ConfigureAwait(false);
                        await StartGame(Hub.Config, token).ConfigureAwait(false);
                        return;
                    }
                }

                Log("Raid lobby disbanded!");
                await Click(B, 0_500, token).ConfigureAwait(false);
                await Click(B, 0_500, token).ConfigureAwait(false);
                await Click(DDOWN, 0_500, token).ConfigureAwait(false);

                Log("Returning to overworld...");
                while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    await Click(A, 2_000, token).ConfigureAwait(false);

                Log("Back in the overworld.");
                bool status = await DenStatus(StoredIndex, token).ConfigureAwait(false);
                if (!status)
                {
                    Settings.AddCompletedRaids();
                    Log($"We defeated {Settings.RaidEmbedFilters.Species}!");
                    WinCount++;
                    if (trainers.Count > 0 && Settings.CatchLimit != 0)
                        ApplyPenalty(trainers);

                    await EnqueueEmbed(null, "", false, false, true, false, token).ConfigureAwait(false);
                }
                else
                {
                    Log("We lost the raid...");
                    LossCount++;
                }
            }

            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token).ConfigureAwait(false);
        }

        private async Task<bool> DenStatus(int index, CancellationToken token)
        {
            List<long> ptr = [0];
            switch (FieldID)
            {
                case 0:
                    ptr = new(Offsets.RaidBlockPointerP)
                    {
                        [3] = 0x60 + index * 0x20 - 0x10
                    }; break;
                case 1:
                    ptr = new(Offsets.RaidBlockPointerK)
                    {
                        [3] = 0xCE8 + index * 0x20 - 0x10
                    }; break;
                case 2:
                    ptr = new(Offsets.RaidBlockPointerB)
                    {
                        [3] = 0x1968 + index * 0x20 - 0x10
                    }; break;
            }
            var data = await SwitchConnection.PointerPeek(2, ptr, token).ConfigureAwait(false);
            var status = BitConverter.ToUInt16(data);
            var msg = status == 1 ? "active" : "inactive";
            Log($"Den is {msg}.");
            return status == 1;
        }

        private void ApplyPenalty(List<(ulong, TradeMyStatus)> trainers)
        {
            for (int i = 0; i < trainers.Count; i++)
            {
                var nid = trainers[i].Item1;
                var name = trainers[i].Item2.OT;

                var path = "raidfilessv\\temp-session.json";
                var json = File.ReadAllText(path);
                var jsonData = JsonConvert.DeserializeObject<List<RaidSessionDetails>>(json)!;
                foreach (var j in jsonData)
                {
                    if (j.ID == nid && nid != 0)
                    {
                        j.PenaltyCount++;
                        json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                        File.WriteAllText(path, json);
                        Log($"Player: {name} completed the raid with catch count: {j.PenaltyCount}.");

                        if (Settings.CatchLimit != 0 && j.PenaltyCount == Settings.CatchLimit)
                            Log($"Player: {name} has met the catch limit {j.PenaltyCount}/{Settings.CatchLimit}, adding to the block list for this session for {Settings.RaidEmbedFilters.Species}.");
                    }
                }
            }
        }

        private async void InjectPartyPk(string battlepk)
        {
            var set = new ShowdownSet(battlepk);
            var template = AutoLegalityWrapper.GetTemplate(set);
            PK9 pk = (PK9)HostSAV.GetLegal(template, out _);
            pk.ResetPartyStats();
            var offset = await SwitchConnection.PointerAll(Offsets.BoxStartPokemonPointer, CancellationToken.None).ConfigureAwait(false);
            await SwitchConnection.WriteBytesAbsoluteAsync(pk.EncryptedBoxData, offset, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<bool> PrepareForRaid(CancellationToken token)
        {
            var len = string.Empty;
            foreach (var l in Settings.RaidEmbedFilters.PartyPK)
                len += l;
            if (len.Length > 1 && RaidCount == 0)
            {
                Log("Preparing PartyPK to inject..");
                await SetCurrentBox(0, token).ConfigureAwait(false);
                var res = string.Join("\n", Settings.RaidEmbedFilters.PartyPK);
                if (res.Length > 4096)
                    res = res[..4096];
                InjectPartyPk(res);

                await Click(X, 2_000, token).ConfigureAwait(false);
                await Click(DRIGHT, 0_500, token).ConfigureAwait(false);
                Log("Scrolling through menus...");
                await SetStick(SwitchStick.LEFT, 0, -32000, 1_000, token).ConfigureAwait(false);
                await SetStick(SwitchStick.LEFT, 0, 0, 0, token).ConfigureAwait(false);
                Log("Tap tap...");
                for (int i = 0; i < 2; i++)
                    await Click(DDOWN, 0_500, token).ConfigureAwait(false);
                await Click(A, 3_500, token).ConfigureAwait(false);
                await Click(Y, 0_500, token).ConfigureAwait(false);
                await Click(DLEFT, 0_800, token).ConfigureAwait(false);
                await Click(Y, 0_500, token).ConfigureAwait(false);
                for (int i = 0; i < 2; i++)
                    await Click(B, 1_500, token).ConfigureAwait(false);
                Log("Battle PK is ready!");
            }

            Log("Preparing lobby...");
            // Make sure we're connected.
            while (!await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
            {
                Log("Connecting...");
                await RecoverToOverworld(token).ConfigureAwait(false);
                if (!await ConnectToOnline(Hub.Config, token).ConfigureAwait(false))
                    return false;
            }

            for (int i = 0; i < 6; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            await Task.Delay(1_500, token).ConfigureAwait(false);

            // If not in the overworld, we've been attacked so quit earlier.
            if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                return false;

            await Click(A, 3_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);

            if (!Settings.RaidEmbedFilters.IsCoded || Settings.RaidEmbedFilters.IsCoded && EmptyRaid == Settings.EmptyRaidLimit && Settings.EmptyRaidLimit != 0)
            {
                if (Settings.RaidEmbedFilters.IsCoded && EmptyRaid == Settings.EmptyRaidLimit)
                    Log($"We had {Settings.EmptyRaidLimit} empty raids.. Opening this raid to all!");
                await Click(DDOWN, 1_000, token).ConfigureAwait(false);
            }

            await Click(A, 8_000, token).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> GetLobbyReady(CancellationToken token)
        {
            var x = 0;
            Log("Connecting to lobby...");
            while (!await IsConnectedToLobby(token).ConfigureAwait(false))
            {
                await Click(A, 1_000, token).ConfigureAwait(false);
                x++;
                if (await IsInBattle(Offsets.IsInBattle, token).ConfigureAwait(false) || x == 45)
                {
                    Log("Failed to connect to lobby, restarting game incase we were in battle/bad connection.");
                    await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                    Log("Attempting to restart routine!");
                    return false;
                }
            }
            return true;
        }

        private async Task<string> GetRaidCode(CancellationToken token)
        {
            var data = await SwitchConnection.PointerPeek(6, Offsets.TeraRaidCodePointer, token).ConfigureAwait(false);
            TeraRaidCode = Encoding.ASCII.GetString(data);
            Log($"Raid Code: {TeraRaidCode}");
            return $"\n{TeraRaidCode}\n";
        }

        private async Task<bool> CheckIfTrainerBanned(TradeMyStatus trainer, ulong nid, int player, bool updateBanList, CancellationToken token)
        {
            Log($"Player {player}: {trainer.OT} | TID: {trainer.DisplayTID} | NID: {nid}");

            var path = "raidfilessv\\temp-session.json";
            var json = File.ReadAllText(path);
            var jsonData = JsonConvert.DeserializeObject<List<RaidSessionDetails>>(json)!;

            bool isPresent = false;
            RaidSessionDetails current = new();
            foreach (var j in jsonData)
            {
                if (j.ID == nid && nid is not 0)
                {
                    isPresent = true;
                    current.ID = j.ID;
                    current.Name = j.Name;
                    current.PenaltyCount = j.PenaltyCount;
                    current.Comment = j.Comment;
                    break;
                }
            }

            if (!isPresent)
            {
                RaidSessionDetails raider = new()
                {
                    ID = nid,
                    Name = trainer.OT,
                    PenaltyCount = 0,
                    Comment = string.Empty,
                };
                jsonData.Add(raider);
                json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(path, json);
                jsonData = JsonConvert.DeserializeObject<List<RaidSessionDetails>>(json)!;
            }

            var msg = string.Empty;
            var banResultCC = Settings.RaidsBetweenUpdate == -1 ? (false, "") : await BanService.IsRaiderBanned(trainer.OT, Settings.BanListURL, Connection.Label, updateBanList).ConfigureAwait(false);
            var banResultCFW = RaiderBanList.List.FirstOrDefault(x => x.ID == nid);
            var banGlobalCFW = false;
            BanList user = new();
            for (int i = 0; i < GlobalBanList.Count; i++)
            {
                var gNID = GlobalBanList[i].NIDs;
                for (int g = 0; g < gNID.Length; g++)
                {
                    if (gNID[g] == nid)
                    {
                        Log($"NID: {nid} found on GlobalBanList.");
                        if (GlobalBanList[i].enabled)
                            banGlobalCFW = true;
                        user = GlobalBanList[i];
                        break;
                    }
                }
                if (banGlobalCFW is true)
                    break;
            }
            bool isBanned = banResultCFW != default || banGlobalCFW || banResultCC.Item1;
            if (banResultCC.Item1 is true)
            {
                using var httpClient = new HttpClient();
                var url = "https://raw.githubusercontent.com/zyro670/NIDGlobalBanList/main/override.json";
                var data = await httpClient.GetStringAsync(url, token).ConfigureAwait(false);
                var overridelist = JsonConvert.DeserializeObject<List<OverrideList>>(data)!;
                for (int i = 0; i < overridelist.Count; i++)
                {
                    var oNID = overridelist[i].NIDs;
                    for (int o = 0; o < oNID.Length; o++)
                    {
                        if (oNID[o] == nid)
                        {
                            Log($"NID: {nid} found on GlobalBanList. Initiating override, {overridelist[i].Names} is a good egg.");
                            isBanned = false;
                            break;
                        }
                    }
                }
            }

            bool blockResult = false;
            var blockCheck = isPresent;
            if (blockCheck)
            {
                if (current.PenaltyCount >= Settings.CatchLimit && Settings.CatchLimit != 0) // Soft pity - block user
                {
                    blockResult = true;
                    current.PenaltyCount++;
                    Log($"Player: {trainer.OT} current penalty count: {current.PenaltyCount - Settings.CatchLimit}.");
                    foreach (var j in jsonData)
                    {
                        if (j.ID == current.ID)
                        {
                            j.PenaltyCount = current.PenaltyCount;
                            json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                            File.WriteAllText(path, json);
                        }
                    }
                }
                if (current.PenaltyCount == Settings.CatchLimit + 2 && Settings.CatchLimit != 0) // Hard pity - ban user
                {
                    msg = $"{trainer.OT} is now banned for repeatedly attempting to go beyond the catch limit for {Settings.RaidEmbedFilters.Species} on {DateTime.Now}.";
                    Log(msg);
                    RaiderBanList.List.Add(new() { ID = nid, Name = trainer.OT, Comment = msg });
                    blockResult = false;
                    await EnqueueEmbed(null, $"Penalty #{current.PenaltyCount - Settings.CatchLimit}\n" + msg, false, true, false, false, token).ConfigureAwait(false);
                    return true;
                }
                if (blockResult && !isBanned)
                {
                    msg = $"Penalty #{current.PenaltyCount - Settings.CatchLimit}\n{trainer.OT} has already reached the catch limit.\nPlease do not join again.\nRepeated attempts to join like this will result in a ban from future raids.";
                    Log(msg);
                    await EnqueueEmbed(null, msg, false, true, false, false, token).ConfigureAwait(false);
                    return true;
                }
            }

            if (isBanned)
            {
                msg = banResultCC.Item1 ? banResultCC.Item2 : banGlobalCFW ? $"{trainer.OT} was found in the global ban list.\nReason: {user.Comment}" : $"Penalty #{current.PenaltyCount - Settings.CatchLimit}\n{banResultCFW!.Name} was found in the host's ban list.\n{banResultCFW.Comment}";
                Log(msg);
                await EnqueueEmbed(null, msg, false, true, false, false, token).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        private async Task<(bool, List<(ulong, TradeMyStatus)>)> ReadTrainers(CancellationToken token)
        {
            await EnqueueEmbed(null, "", false, false, false, false, token).ConfigureAwait(false);

            List<(ulong, TradeMyStatus)> lobbyTrainers = new();
            var wait = TimeSpan.FromSeconds(Settings.TimeToWait);
            var endTime = DateTime.Now + wait;
            bool full = false;
            bool updateBanList = Settings.RaidsBetweenUpdate != -1 && (RaidCount == 0 || RaidCount % Settings.RaidsBetweenUpdate == 0);

            while (!full && (DateTime.Now < endTime))
            {
                for (int i = 0; i < 3; i++)
                {
                    var player = i + 2;
                    Log($"Waiting for Player {player} to load...");

                    var nidOfs = TeraNIDOffsets[i];
                    var data = await SwitchConnection.ReadBytesAbsoluteAsync(nidOfs, 8, token).ConfigureAwait(false);
                    var nid = BitConverter.ToUInt64(data, 0);
                    while (nid == 0 && (DateTime.Now < endTime))
                    {
                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        data = await SwitchConnection.ReadBytesAbsoluteAsync(nidOfs, 8, token).ConfigureAwait(false);
                        nid = BitConverter.ToUInt64(data, 0);
                    }

                    List<long> ptr = new(Offsets.Trader2MyStatusPointer);
                    ptr[2] += i * 0x30;
                    var trainer = await GetTradePartnerMyStatus(ptr, token).ConfigureAwait(false);

                    while (trainer.OT.Length == 0 && (DateTime.Now < endTime))
                    {
                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        trainer = await GetTradePartnerMyStatus(ptr, token).ConfigureAwait(false);
                    }

                    if (nid != 0 && !string.IsNullOrWhiteSpace(trainer.OT))
                    {
                        if (await CheckIfTrainerBanned(trainer, nid, player, updateBanList, token).ConfigureAwait(false))
                            return (false, lobbyTrainers);

                        updateBanList = false;
                    }

                    if (lobbyTrainers.FirstOrDefault(x => x.Item1 == nid) != default && trainer.OT.Length > 0)
                        lobbyTrainers[i] = (nid, trainer);
                    else if (nid > 0 && trainer.OT.Length > 0)
                        lobbyTrainers.Add((nid, trainer));

                    full = lobbyTrainers.Count == 3;
                    if (full || (DateTime.Now >= endTime))
                        break;
                }
            }

            await Task.Delay(5_000, token).ConfigureAwait(false);

            RaidCount++;
            if (lobbyTrainers.Count == 0)
            {
                EmptyRaid++;
                Log($"Nobody joined the raid, recovering...\nEmpty Raid Count #{EmptyRaid}");
                return (false, lobbyTrainers);
            }
            Log($"Raid #{RaidCount} is starting!");
            if (EmptyRaid != 0)
                EmptyRaid = 0;
            return (true, lobbyTrainers);
        }

        private async Task<bool> IsConnectedToLobby(CancellationToken token)
        {
            var data = await SwitchConnection.ReadBytesMainAsync(Offsets.TeraLobbyIsConnected, 1, token).ConfigureAwait(false);
            return data[0] != 0x00; // 0 when in lobby but not connected
        }

        private async Task<bool> IsInRaid(CancellationToken token)
        {
            var data = await SwitchConnection.ReadBytesMainAsync(Offsets.LoadedIntoDesiredState, 1, token).ConfigureAwait(false);
            return data[0] == 0x02; // 2 when in raid, 1 when not
        }

        private async Task RolloverCorrectionSV(CancellationToken token)
        {
            if (Settings.RolloverFilters.RolloverPrevention == RolloverPrevention.TimeSkip)
            {
                for (int i = 0; i < 23; i++)
                {
                    await TimeSkipBwd(token).ConfigureAwait(false);
                }
                await Task.Delay(1_500, token).ConfigureAwait(false);
                return;
            }

            var scrollroll = Settings.RolloverFilters.DateTimeFormat switch
            {
                DTFormat.DDMMYY => 0,
                DTFormat.YYMMDD => 2,
                _ => 1,
            };

            for (int i = 0; i < 2; i++)
                await Click(B, 0_150, token).ConfigureAwait(false);

            for (int i = 0; i < 2; i++)
                await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(DRIGHT, 0_150, token).ConfigureAwait(false);
            await Click(A, 1_250, token).ConfigureAwait(false); // Enter settings

            await PressAndHold(DDOWN, 2_000, 0_250, token).ConfigureAwait(false); // Scroll to system settings
            await Click(A, 1_250, token).ConfigureAwait(false);

            if (Settings.RolloverFilters.RolloverPrevention == RolloverPrevention.Overshoot)
            {
                await PressAndHold(DDOWN, Settings.RolloverFilters.HoldTimeForRollover, 1_000, token).ConfigureAwait(false);
                await Click(DUP, 0_500, token).ConfigureAwait(false);
            }
            else if (Settings.RolloverFilters.RolloverPrevention == RolloverPrevention.DDOWN)
            {
                for (int i = 0; i < 39; i++)
                    await Click(DDOWN, 0_100, token).ConfigureAwait(false);
            }

            await Click(A, 1_250, token).ConfigureAwait(false);
            for (int i = 0; i < 2; i++)
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);
            for (int i = 0; i < scrollroll; i++) // 0 to roll day for DDMMYY, 1 to roll day for MMDDYY, 3 to roll hour
                await Click(DRIGHT, 0_200, token).ConfigureAwait(false);

            await Click(DDOWN, 0_200, token).ConfigureAwait(false);

            for (int i = 0; i < 8; i++) // Mash DRIGHT to confirm
                await Click(DRIGHT, 0_200, token).ConfigureAwait(false);

            await Click(A, 0_200, token).ConfigureAwait(false); // Confirm date/time change
            await Click(HOME, 1_000, token).ConfigureAwait(false); // Back to title screen
        }

        private async Task RegroupFromBannedUser(CancellationToken token)
        {
            Log("Attempting to remake lobby..");
            await Click(B, 2_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);
            await Click(B, 1_000, token).ConfigureAwait(false);
        }

        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("Caching session offsets...");
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
            ConnectedOffset = await SwitchConnection.PointerAll(Offsets.IsConnectedPointer, token).ConfigureAwait(false);
            BaseBlockKeyPointer = await SwitchConnection.PointerAll(Offsets.BlockKeyPointer, token).ConfigureAwait(false);
            FieldID = await ReadEncryptedBlockByte(BaseBlockKeyPointer, Blocks.KPlayerCurrentFieldID, token).ConfigureAwait(false);
            switch (FieldID)
            {
                case 0: RaidBlockPointer = await SwitchConnection.PointerAll(Offsets.RaidBlockPointerP, token).ConfigureAwait(false) + RaidBlock.HEADER_SIZE; RaidBlockSize = (int)(RaidBlock.SIZE_BASE - RaidBlock.HEADER_SIZE); break;
                case 1: RaidBlockPointer = await SwitchConnection.PointerAll(Offsets.RaidBlockPointerK, token).ConfigureAwait(false); RaidBlockSize = (int)RaidBlock.SIZE_KITAKAMI; RaidMap = TeraRaidMapParent.Kitakami; break;
                case 2: RaidBlockPointer = await SwitchConnection.PointerAll(Offsets.RaidBlockPointerB, token).ConfigureAwait(false); RaidBlockSize = (int)RaidBlock.SIZE_BLUEBERRY; RaidMap = TeraRaidMapParent.Blueberry; break;
            }
            var nidPointer = new long[] { Offsets.LinkTradePartnerNIDPointer[0], Offsets.LinkTradePartnerNIDPointer[1], Offsets.LinkTradePartnerNIDPointer[2] };
            for (int p = 0; p < TeraNIDOffsets.Length; p++)
            {
                nidPointer[2] = Offsets.LinkTradePartnerNIDPointer[2] + (p * 0x8);
                TeraNIDOffsets[p] = await SwitchConnection.PointerAll(nidPointer, token).ConfigureAwait(false);
            }
            Log("Caching offsets complete!");
        }

        private async Task EnqueueEmbed(List<string>? names, string message, bool hatTrick, bool disband, bool upnext, bool raidstart, CancellationToken token)
        {
            // Title can only be up to 256 characters.
            var title = hatTrick && names is not null ? $"**🪄🎩✨ {names[0]} with the Hat Trick! ✨🎩🪄**" : Settings.RaidEmbedFilters.Title.Length > 0 ? Settings.RaidEmbedFilters.Title : "Tera Raid Notification";
            if (title.Length > 256)
                title = title[..256];

            // Description can only be up to 4096 characters.
            var description = Settings.RaidEmbedFilters.Description.Length > 0 ? string.Join("\n", Settings.RaidEmbedFilters.Description) : "";
            if (description.Length > 4096)
                description = description[..4096];

            string code = string.Empty;
            if (names is null && !upnext)
            {
                code = $"**{(Settings.RaidEmbedFilters.IsCoded && EmptyRaid < Settings.EmptyRaidLimit ? await GetRaidCode(token).ConfigureAwait(false) : "Free For All")}**";
                Hub.Config.Stream.GetRaidCodeAsset(code.Replace("*", ""));
            }

            if (EmptyRaid == Settings.EmptyRaidLimit)
                EmptyRaid = 0;

            if (disband) // Wait for trainer to load before disband
                await Task.Delay(5_000, token).ConfigureAwait(false);

            byte[]? bytes = [];
            if (Settings.TakeScreenshot && !upnext)
                bytes = await SwitchConnection.PixelPeek(token).ConfigureAwait(false) ?? [];

            var embed = new EmbedBuilder()
            {
                Title = disband ? $"**Raid canceled: [{TeraRaidCode}]**" : upnext && Settings.TotalRaidsToHost != 0 ? $"Preparing Raid {RaidCount}/{Settings.TotalRaidsToHost}" : upnext && Settings.TotalRaidsToHost == 0 ? $"Preparing Raid" : title,
                Color = disband ? Color.Red : hatTrick ? Color.Purple : Color.Green,
                Description = disband ? message : upnext ? Settings.RaidEmbedFilters.Title : raidstart ? "" : description,
                ImageUrl = bytes.Length > 0 ? "attachment://zap.jpg" : default,
            }.WithFooter(new EmbedFooterBuilder()
            {
                Text = $"Host: {HostSAV.OT} | Uptime: {StartTime - DateTime.Now:d\\.hh\\:mm\\:ss}\n" +
                       $"Raids: {RaidCount} | Wins: {WinCount} | Losses: {LossCount}"
            });

            if (!disband && names is null && !upnext)
            {
                embed.AddField(Settings.IncludeCountdown ? $"**Raid Countdown: <t:{DateTimeOffset.Now.ToUnixTimeSeconds() + Settings.TimeToWait}:R>**" : $"**Waiting in lobby!**", $"Raid Code: {code}");
            }

            if (!disband && names is not null && !upnext && raidstart)
            {
                var players = string.Empty;
                if (names.Count == 0)
                    players = "Though our party did not make it :(";
                else
                {
                    int i = 2;
                    names.ForEach(x =>
                    {
                        players += $"Player {i} - **{x}**\n";
                        i++;
                    });
                }
                embed.AddField($"**Raid #{RaidCount} is starting!**", players);
            }

            var turl = string.Empty;
            var form = string.Empty;

            PK9 pk = new()
            {
                Species = (ushort)Settings.RaidEmbedFilters.Species,
                Form = (byte)Settings.RaidEmbedFilters.SpeciesForm
            };
            if (pk.Form != 0)
                form = $"-{pk.Form}";
            if (pk.Species is (ushort)Species.Basculegion or (ushort)Species.Indeedee or (ushort)Species.Oinkologne && pk.Form == 1)
                pk.Gender = 1;

            if (Settings.RaidEmbedFilters.IsShiny == true)
                CommonEdits.SetIsShiny(pk, true);
            else
                CommonEdits.SetIsShiny(pk, false);

            if (Settings.SpriteAlternateArt && Settings.RaidEmbedFilters.IsShiny)
            {
                turl = AltPokeImg(pk);
                bool valid = await VerifySprite(turl).ConfigureAwait(false);
                if (!valid)
                    turl = TradeExtensions<PK9>.PokeImg(pk, false, false);
            }
            else
                turl = TradeExtensions<PK9>.PokeImg(pk, false, false);

            if (Settings.RaidEmbedFilters.Species is 0)
                turl = "https://i.imgur.com/uHSaGGJ.png";

            var fileName = $"raidecho{0}.jpg";
            embed.ThumbnailUrl = turl;
            embed.WithImageUrl($"attachment://{fileName}");
            EchoUtil.RaidEmbed(bytes, fileName, embed);
        }

        // From PokeTradeBotSV, modified.
        private async Task<bool> ConnectToOnline(PokeTradeHubConfig config, CancellationToken token)
        {
            if (await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
                return true;

            await Click(X, 3_000, token).ConfigureAwait(false);
            await Click(L, 5_000 + config.Timings.ExtraTimeConnectOnline, token).ConfigureAwait(false);

            // Try one more time.
            if (!await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
            {
                Log("Failed to connect the first time, trying again...");
                await RecoverToOverworld(token).ConfigureAwait(false);
                await Click(X, 3_000, token).ConfigureAwait(false);
                await Click(L, 5_000 + config.Timings.ExtraTimeConnectOnline, token).ConfigureAwait(false);
            }

            var wait = 0;
            while (!await IsConnectedOnline(ConnectedOffset, token).ConfigureAwait(false))
            {
                await Task.Delay(0_500, token).ConfigureAwait(false);
                if (++wait > 30) // More than 15 seconds without a connection.
                    return false;
            }

            // There are several seconds after connection is established before we can dismiss the menu.
            await Task.Delay(3_000 + config.Timings.ExtraTimeConnectOnline, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);
            return true;
        }

        // From PokeTradeBotSV.
        private async Task<bool> RecoverToOverworld(CancellationToken token)
        {
            if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                return true;

            Log("Attempting to recover to overworld.");
            var attempts = 0;
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                attempts++;
                if (attempts >= 30)
                    break;

                await Click(B, 1_300, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;

                await Click(B, 2_000, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;

                await Click(A, 1_300, token).ConfigureAwait(false);
                if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    break;
            }

            // We didn't make it for some reason.
            if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                Log("Failed to recover to overworld, rebooting the game.");
                await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
            }
            await Task.Delay(1_000, token).ConfigureAwait(false);
            return true;
        }

        private static string AltPokeImg(PKM pkm)
        {
            string pkmform = string.Empty;
            if (pkm.Form != 0)
                pkmform = $"-{pkm.Form}";

            return _ = $"https://raw.githubusercontent.com/zyro670/PokeTextures/main/Placeholder_Sprites/scaled_up_sprites/Shiny/AlternateArt/" + $"{pkm.Species}{pkmform}" + ".png";
        }

        private async Task GetRaidSprite(CancellationToken token)
        {
            PK9 pk = new()
            {
                Species = (ushort)Settings.RaidEmbedFilters.Species
            };
            if (Settings.RaidEmbedFilters.IsShiny)
                CommonEdits.SetIsShiny(pk, true);
            else
                CommonEdits.SetIsShiny(pk, false);
            PK9 pknext = new();

            await Hub.Config.Stream.StartRaid(this, pk, pknext, 0, Hub, 0, token).ConfigureAwait(false);
        }

        #region RaidCrawler
        // via RaidCrawler modified for this proj
        private async Task<bool> ReadRaids(CancellationToken token)
        {
            string id = await SwitchConnection.GetTitleID(token).ConfigureAwait(false);
            var game = id switch
            {
                RaidCrawler.Core.Structures.Offsets.ScarletID => "Scarlet",
                RaidCrawler.Core.Structures.Offsets.VioletID => "Violet",
                _ => "",
            };

            container = new(game);
            container.SetGame(game);

            StoryProgress = await GetStoryProgress(BaseBlockKeyPointer, token).ConfigureAwait(false);
            EventProgress = Math.Min(StoryProgress, 3);

            await ReadEventRaids(BaseBlockKeyPointer, container, token).ConfigureAwait(false);

            var data = await SwitchConnection.ReadBytesAbsoluteAsync(RaidBlockPointer, RaidBlockSize, token).ConfigureAwait(false);

            (int delivery, int enc) = container.ReadAllRaids(data, StoryProgress, EventProgress, 0, RaidMap);
            if (enc > 0)
                Log($"Failed to find encounters for {enc} raid(s).");

            if (delivery > 0)
                Log($"Invalid delivery group ID for {delivery} raid(s). Try deleting the \"cache\" folder.");

            for (int i = 0; i < container.Raids.Count; i++)
            {
                var (pk, seed) = IsSeedReturned(container.Encounters[i], container.Raids[i]);
                var set = uint.Parse(Settings.RaidEmbedFilters.Seed, NumberStyles.AllowHexSpecifier);
                if (seed == set)
                {
                    var res = GetSpecialRewards(container.Rewards[i]);
                    if (string.IsNullOrEmpty(res))
                        res = string.Empty;
                    else
                        res = "**Special Rewards:**\n" + res;
                    Log($"Seed {seed:X8} found for {(Species)container.Encounters[i].Species}");
                    Settings.RaidEmbedFilters.Seed = $"{seed:X8}";
                    var stars = container.Raids[i].IsEvent ? container.Encounters[i].Stars : RaidExtensions.GetStarCount(container.Raids[i], container.Raids[i].Difficulty, StoryProgress, container.Raids[i].IsBlack);
                    string starcount = string.Empty;
                    switch (stars)
                    {
                        case 1: starcount = "1 ☆"; break;
                        case 2: starcount = "2 ☆"; break;
                        case 3: starcount = "3 ☆"; break;
                        case 4: starcount = "4 ☆"; break;
                        case 5: starcount = "5 ☆"; break;
                        case 6: starcount = "6 ☆"; break;
                        case 7: starcount = "7 ☆"; break;
                    }
                    Settings.RaidEmbedFilters.IsShiny = container.Raids[i].IsShiny;
                    Settings.RaidEmbedFilters.CrystalType = container.Raids[i].IsBlack ? TeraCrystalType.Black : container.Raids[i].IsEvent && stars == 7 ? TeraCrystalType.Might : container.Raids[i].IsEvent ? TeraCrystalType.Distribution : TeraCrystalType.Base;
                    Settings.RaidEmbedFilters.Species = (Species)container.Encounters[i].Species;
                    Settings.RaidEmbedFilters.SpeciesForm = container.Encounters[i].Form;
                    var catchlimit = Settings.CatchLimit;
                    string cl = catchlimit is 0 ? "\n**No catch limit!**" : $"\n**Catch Limit: {catchlimit}**";
                    var pkinfo = Hub.Config.StopConditions.GetRaidPrintName(pk);
                    pkinfo += $"\nTera Type: {(MoveType)container.Raids[i].TeraType}";
                    var strings = GameInfo.GetStrings(1);
                    var moves = new ushort[4] { container.Encounters[i].Move1, container.Encounters[i].Move2, container.Encounters[i].Move3, container.Encounters[i].Move4 };
                    var movestr = string.Concat(moves.Where(z => z != 0).Select(z => $"{strings.Move[z]}ㅤ{Environment.NewLine}")).TrimEnd(Environment.NewLine.ToCharArray());
                    var extramoves = string.Empty;
                    if (container.Encounters[i].ExtraMoves.Length != 0)
                    {
                        var extraMovesList = container.Encounters[i].ExtraMoves.Where(z => z != 0).Select(z => $"{strings.Move[z]}ㅤ{Environment.NewLine}");
                        extramoves = string.Concat(extraMovesList.Take(extraMovesList.Count() - 1)).TrimEnd(Environment.NewLine.ToCharArray());
                        extramoves += extraMovesList.LastOrDefault()?.TrimEnd(Environment.NewLine.ToCharArray());
                    }

                    if (Settings.UsePresetFile)
                    {
                        string tera = $"{(MoveType)container.Raids[i].TeraType}";
                        if (!string.IsNullOrEmpty(Settings.RaidEmbedFilters.Title) && !Settings.PresetFilters.ForceTitle)
                            ModDescription[0] = Settings.RaidEmbedFilters.Title;

                        if (Settings.RaidEmbedFilters.Description.Length > 0 && !Settings.PresetFilters.ForceDescription)
                        {
                            string[] presetOverwrite = new string[Settings.RaidEmbedFilters.Description.Length + 1];
                            presetOverwrite[0] = ModDescription[0];
                            for (int l = 0; l < Settings.RaidEmbedFilters.Description.Length; l++)
                                presetOverwrite[l + 1] = Settings.RaidEmbedFilters.Description[l];

                            ModDescription = presetOverwrite;
                        }

                        var raidDescription = ProcessRaidPlaceholders(ModDescription, pk);

                        for (int j = 0; j < raidDescription.Length; j++)
                        {
                            raidDescription[j] = raidDescription[j].Replace("{tera}", tera).Replace("{difficulty}", $"{stars}").Replace("{stars}", starcount).Trim();
                            raidDescription[j] = Regex.Replace(raidDescription[j], @"\s+", " ");
                        }
                        if (Settings.PresetFilters.IncludeMoves)
                            raidDescription = raidDescription.Concat(new string[] { Environment.NewLine, movestr, extramoves }).ToArray();

                        if (Settings.PresetFilters.IncludeRewards)
                            raidDescription = raidDescription.Concat(new string[] { res.Replace("\n", Environment.NewLine) }).ToArray();

                        if (Settings.PresetFilters.TitleFromPreset)
                        {
                            if (string.IsNullOrEmpty(Settings.RaidEmbedFilters.Title) || Settings.PresetFilters.ForceTitle)
                                Settings.RaidEmbedFilters.Title = raidDescription[0];

                            if (Settings.RaidEmbedFilters.Description == null || Settings.RaidEmbedFilters.Description.Length == 0 || Settings.RaidEmbedFilters.Description.All(string.IsNullOrEmpty) || Settings.PresetFilters.ForceDescription)
                                Settings.RaidEmbedFilters.Description = raidDescription.Skip(1).ToArray();
                        }
                        else if (!Settings.PresetFilters.TitleFromPreset)
                        {
                            if (Settings.RaidEmbedFilters.Description == null || Settings.RaidEmbedFilters.Description.Length == 0 || Settings.RaidEmbedFilters.Description.All(string.IsNullOrEmpty) || Settings.PresetFilters.ForceDescription)
                                Settings.RaidEmbedFilters.Description = raidDescription.ToArray();
                        }
                    }

                    else if (!Settings.UsePresetFile)
                    {
                        Settings.RaidEmbedFilters.Description = ["\n**Raid Info:**", pkinfo, "\n**Moveset:**", movestr, extramoves, BaseDescription, res];
                        Settings.RaidEmbedFilters.Title = $"{(Species)pk.Species} {starcount} - {(MoveType)container.Raids[i].TeraType}";
                    }

                    Settings.RaidEmbedFilters.IsSet = true;
                    StoredIndex = i;
                    break;
                }
            }
            if (!Settings.RaidEmbedFilters.IsSet)
                return false;
            else
                return true;
        }
        #endregion
    }
}
