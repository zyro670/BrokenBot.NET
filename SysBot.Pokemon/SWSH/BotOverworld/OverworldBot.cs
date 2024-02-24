using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;
using System.Linq;

namespace SysBot.Pokemon;

public sealed class OverworldBotSWSH : PokeRoutineExecutor8SWSH, IEncounterBot
{
    private readonly PokeTradeHub<PK8> Hub;
    private readonly IDumper DumpSetting;
    private readonly int[] DesiredMinIVs;
    private readonly int[] DesiredMaxIVs;
    private readonly OverworldSettings Settings;
    public ICountSettings Counts => Settings;
    public readonly IReadOnlyList<string> UnwantedMarks;
    private readonly byte[] BattleMenuReady = { 0, 0, 0, 255 };

    public OverworldBotSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg)
    {
        Hub = hub;
        Settings = hub.Config.OverworldSWSH;
        DumpSetting = Hub.Config.Folder;
        StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
        StopConditionSettings.ReadUnwantedMarks(Hub.Config.StopConditions, out UnwantedMarks);
    }

    private int overworldCount;
    private ulong ofs;
    private byte[] pouchData = { 0 };
    private ulong MainNsoBase;
    private ulong CaughtFlag;
    private ulong BattleCheck;
    private int kocount = 0;
    private uint StartingOffset = 0x4505B880;
    private uint KCoordIncrement = 192;
    private ulong OverworldOffset;

    public string ConditionalMarks { get; set; } = $"Rare,Rowdy,AbsentMinded,Jittery,Excited,Charismatic,Calmness,Intense,ZonedOut,Joyful,Angry,Smiley,Teary,Upbeat,Peeved,Intellectual,Ferocious,Crafty,Scowling,Kindly,Flustered," +
                 $"PumpedUp,ZeroEnergy,Prideful,Unsure,Humble,Thorny,Vigor,Slump";

    public override async Task MainLoop(CancellationToken token)
    {
        Log("Identifying trainer data of the host console.");
        await IdentifyTrainer(token).ConfigureAwait(false);
        await InitializeHardware(Settings, token).ConfigureAwait(false);
        OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
        try
        {
            Log($"Starting main {GetType().Name} loop.");
            Config.IterateNextRoutine();

            // Clear out any residual stick weirdness.
            await ResetStick(token).ConfigureAwait(false);

            var task = Hub.Config.OverworldSWSH.ScanType switch
            {
                OverworldMode.OverworldScan => DoOverworldScan(token),
                OverworldMode.OffsetScan => OffsetScan(token),
                OverworldMode.PlayerCoordScan => PlayerCoordScan(token),
                OverworldMode.BallTosser => BallTosser(token),
                _ => OffsetScan(token),
            };
            await task.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log(e.Message);
        }

        Log($"Ending {GetType().Name} loop.");
        await HardStop().ConfigureAwait(false);
    }        

    public override async Task HardStop()
    {
        await ResetStick(CancellationToken.None).ConfigureAwait(false);
        await CleanExit(CancellationToken.None).ConfigureAwait(false);
    }

    private bool IsWaiting;
    public void Acknowledge() => IsWaiting = false;
    
    private async Task OffsetScan(CancellationToken token)
    {
        if (Settings.NavigationType == NavigationType.Teleportation)
        {
            Log($"Grabbing player coords...");
            var ofs = await ParsePointer("[[[[[[main+26365B8]+88]+1F8]+E0]+10]+E0]+60", token).ConfigureAwait(false);
            var coord = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
            var coord2 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x4, 4, token).ConfigureAwait(false), 0);
            var coord3 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x8, 4, token).ConfigureAwait(false), 0);

            Log($"Current Player Coords - X: {coord:X8} Y: {coord2:X8} Z: {coord3:X8}");

            if (Settings.AutoFillCoords == CoordsAutoFill.ScanZone)
            {
                Log($"Autofilling ScanZone XYZ");
                Settings.TeleportConditions.ScanZoneX = $"{coord:X8}";
                Settings.TeleportConditions.ScanZoneY = $"{coord2:X8}";
                Settings.TeleportConditions.ScanZoneZ = $"{coord3:X8}";
            }

            if (Settings.AutoFillCoords == CoordsAutoFill.DespawnZone)
            {
                Log($"Autofilling DespawnZone XYZ");
                Settings.TeleportConditions.DespawnZoneX = $"{coord:X8}";
                Settings.TeleportConditions.DespawnZoneY = $"{coord2:X8}";
                Settings.TeleportConditions.DespawnZoneZ = $"{coord3:X8}";
            }

            bool trash = coord.Equals(coord2) && coord2.Equals(coord3) && coord3.Equals(coord);
            if (coord == 0 || coord2 == 0 || coord3 == 0 || trash)
            {
                Log($"Pointer data is trashy! Restarting game to restart routine!");
                await ResetGameAsync(token).ConfigureAwait(false);
                await GetOverworldOffsets(token).ConfigureAwait(false);
            }
        }

        _ = await GetOverworldOffsets(token).ConfigureAwait(false);
        Log($"Offset Scan Complete!");
    }
    private async Task<List<uint>> GetOverworldOffsets(CancellationToken token)
    {
        List<uint> offset = new();
        string log = string.Empty;
        int n = 0;
        Log($"Looking for a starting offset...");
        if (Settings.NavigationType == NavigationType.Fishing)
        {
            for (uint i = 0; i < 10; i++)
            {
                uint fishing = 0x4505B640 + i * KCoordIncrement;
                byte[] check = await Connection.ReadBytesAsync(fishing, 2, token).ConfigureAwait(false);
                Species species = (Species)BitConverter.ToUInt16(check.AsSpan(0, 2).ToArray(), 0);
                if (species == 0 || species > Species.MAX_COUNT || !(PersonalTable.SWSH[(int)species]).IsPresentInGame)
                    continue;

                var data = await Connection.ReadBytesAsync(fishing + 0x39, 1, token).ConfigureAwait(false);
                if (data[0] != 255)
                    continue;

                string shiny = await ScanIsShiny(fishing, token).ConfigureAwait(false);
                log += $"\nPokemon: {species} | {shiny} | Offset: {fishing:X8}";
                offset.Add(fishing);                    
            }
            if (offset.FirstOrDefault() == 0)
            {
                Log($"No Pokemon are present at the fishing offset, checking second starting offset!");
                for (uint i = 0; i < 100; i++)
                {
                    if (offset.Count == Settings.Conditions.IncrementFromOffset)
                    {
                        Log($"Offset count = IncrementFromOffset, ending task early. Increase increment to check for more offsets.");
                        break;
                    }

                    uint startingoffset = StartingOffset + i * KCoordIncrement;
                    byte[] check = await Connection.ReadBytesAsync(startingoffset, 2, token).ConfigureAwait(false);
                    Species species = (Species)BitConverter.ToUInt16(check.AsSpan(0, 2).ToArray(), 0);
                    if (species == 0 || species > Species.MAX_COUNT || !(PersonalTable.SWSH[(int)species]).IsPresentInGame)
                        continue;

                    var data = await Connection.ReadBytesAsync(startingoffset + 0x39, 1, token).ConfigureAwait(false);
                    if (data[0] != 255)
                        continue;

                    string shiny = await ScanIsShiny(startingoffset, token).ConfigureAwait(false);
                    int isshiny = BitConverter.ToUInt16(await Connection.ReadBytesAsync(startingoffset + 0x6, 2, token).ConfigureAwait(false), 0);
                    n++;
                    log += $"\nPokemon #{n}: {species} | {shiny} | Offset: {startingoffset:X8}";
                    offset.Add(startingoffset);
                    if (isshiny == 1)
                    {
                        SAV8SWSH sav = await GetFakeTrainerSAV(token).ConfigureAwait(false);
                        var pk = await ShinyScan(startingoffset, sav, token).ConfigureAwait(false);
                        if (pk != null)
                        {
                            if (await HandleOverworldField(pk, token).ConfigureAwait(false))
                                return offset;
                        }
                    }
                }
                Log(log + $"\nAutofilling offset to {offset.First():X8} and setting increment to 1.");
                Settings.Offset = $"{offset.FirstOrDefault():X8}";
                Settings.Conditions.IncrementFromOffset = 1;
                return offset;
            }
            if (Settings.ScanType == OverworldMode.OffsetScan && offset.FirstOrDefault() != 0)
            {
                Settings.Offset = $"{offset.FirstOrDefault():X8}";
                Log($"Autofilling Offset to: {offset.FirstOrDefault():X8}");
            }
            Log(log + $"\nAutofilling offset to {offset.First():X8} and setting increment to 1.");
            Settings.Offset = $"{offset.FirstOrDefault():X8}";
            Settings.Conditions.IncrementFromOffset = 1;
            return offset;
        }
        if (Settings.ScanType == OverworldMode.OffsetScan)
        {
            await OverworldSaveGame(token).ConfigureAwait(false);
            Log($"Game saved!");
        }
        for (uint i = 0; i < 100; i++)
        {
            if (offset.Count == Settings.Conditions.IncrementFromOffset)
            {
                Log($"Offset count = IncrementFromOffset, ending task early. Increase increment to check more offsets.");
                break;
            }
            
            uint startingoffset = StartingOffset + i * KCoordIncrement;
            byte[] check = await Connection.ReadBytesAsync(startingoffset, 2, token).ConfigureAwait(false);
            Species species = (Species)BitConverter.ToUInt16(check.AsSpan(0, 2).ToArray(), 0);
            if (species == 0 || species > Species.MAX_COUNT || !(PersonalTable.SWSH[(int)species]).IsPresentInGame)
                continue;

            var data = await Connection.ReadBytesAsync(startingoffset + 0x39, 1, token).ConfigureAwait(false);
            if (data[0] != 255)
                continue;

            string shiny = await ScanIsShiny(startingoffset, token).ConfigureAwait(false);
            int isshiny = BitConverter.ToUInt16(await Connection.ReadBytesAsync(startingoffset + 0x6, 2, token).ConfigureAwait(false), 0);
            n++;
            log += $"\nPokemon #{n}: {species} | {shiny} | Offset: {startingoffset:X8}";
            offset.Add(startingoffset);
            if (isshiny == 1)
            {
                SAV8SWSH sav = await GetFakeTrainerSAV(token).ConfigureAwait(false);
                var pk = await ShinyScan(startingoffset, sav, token).ConfigureAwait(false);
                if (pk != null)
                {
                    if (await HandleOverworldField(pk, token).ConfigureAwait(false))
                        return offset;
                }
            }
        }
        Log(log);
        if (offset.FirstOrDefault() == 0)
        {
            Log($"No Pokemon are present, change your position!");
            return offset;
        }
        if (Settings.ScanType == OverworldMode.OffsetScan && offset.FirstOrDefault() != 0)
        {
            Settings.Offset = $"{offset.FirstOrDefault():X8}";
            Log($"Autofilling Offset to: {offset.FirstOrDefault():X8}");
        }
        return offset;
    }

    private async Task PlayerCoordScan(CancellationToken token)
    {
        var ofs = await ParsePointer("[[[[[[main+26365B8]+88]+1F8]+E0]+10]+E0]+60", token).ConfigureAwait(false);
        var coord = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
        var coord2 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x4, 4, token).ConfigureAwait(false), 0);
        var coord3 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x8, 4, token).ConfigureAwait(false), 0);

        Log($"Current Player Coords - X: {coord:X8} Y: {coord2:X8} Z: {coord3:X8}");

        if (Settings.AutoFillCoords == CoordsAutoFill.ScanZone)
        {
            Log($"Autofilling ScanZone XYZ");
            Settings.TeleportConditions.ScanZoneX = $"{coord:X8}";
            Settings.TeleportConditions.ScanZoneY = $"{coord2:X8}";
            Settings.TeleportConditions.ScanZoneZ = $"{coord3:X8}";
        }

        if (Settings.AutoFillCoords == CoordsAutoFill.DespawnZone)
        {
            Log($"Autofilling DespawnZone XYZ");
            Settings.TeleportConditions.DespawnZoneX = $"{coord:X8}";
            Settings.TeleportConditions.DespawnZoneY = $"{coord2:X8}";
            Settings.TeleportConditions.DespawnZoneZ = $"{coord3:X8}";
        }

        bool trash = coord.Equals(coord2) && coord2.Equals(coord3) && coord3.Equals(coord);
        if (coord == 0 || coord2 == 0 || coord3 == 0 || trash)
        {
            Log($"Pointer data is trashy! Restarting game to restart routine!");
            await ResetGameAsync(token).ConfigureAwait(false);
            await PlayerCoordScan(token).ConfigureAwait(false);
        }
    }
            
    private async Task DoOverworldScan(CancellationToken token)
    {
        SAV8SWSH sav = await GetFakeTrainerSAV(token).ConfigureAwait(false);
        uint customoffset = uint.Parse(Settings.Offset, System.Globalization.NumberStyles.AllowHexSpecifier);
        uint offset = 0x00 + customoffset;
        
        if (Settings.NavigationType == NavigationType.Teleportation)
        {
            string[] coords = { Settings.TeleportConditions.ScanZoneX, Settings.TeleportConditions.ScanZoneY, Settings.TeleportConditions.ScanZoneZ, Settings.TeleportConditions.DespawnZoneX, Settings.TeleportConditions.DespawnZoneY, Settings.TeleportConditions.DespawnZoneZ };
            for (int i = 0; i < coords.Length; i++)
            {
                if (string.IsNullOrEmpty(coords[i]))
                {
                    Log($"One of your coordinates is empty, please fill it accordingly!");
                    return;
                }
            }
        }

        if (Settings.NavigationType == NavigationType.RunDownTpUp)
        {
            Log($"Grabbing origin coords...");
            var ofs = await ParsePointer("[[[[[[main+26365B8]+88]+1F8]+E0]+10]+E0]+60", token).ConfigureAwait(false);
            var coord = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
            var coord2 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x4, 4, token).ConfigureAwait(false), 0);
            var coord3 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x8, 4, token).ConfigureAwait(false), 0);

            //Log($"Current Player Coords - X: {coord:X8} Y: {coord2:X8} Z: {coord3:X8}");

            Log($"Autofilling ScanZone XYZ for RunDownTpUp!");
            Settings.TeleportConditions.ScanZoneX = $"{coord:X8}";
            Settings.TeleportConditions.ScanZoneY = $"{coord2:X8}";
            Settings.TeleportConditions.ScanZoneZ = $"{coord3:X8}";
        }

        if (Settings.SpeciesToHunt.Length == 0)
            Hub.Config.StopConditions.StopOnSpecies = Species.None;

        if (Settings.ScanType == OverworldMode.OverworldScan)
            Settings.AutoFillCoords = CoordsAutoFill.NoAutoFill;

        while (!token.IsCancellationRequested)
        {
            Log($"Logging time for rollover correction!");
            var time = TimeSpan.FromDays(Settings.RollBackTime);
            var start = DateTime.Today;
            Log($"Initial Start Date: {start} - Current Date: {DateTime.Now}.");
            while (DateTime.Now - start < time)
            {
                Log($"Scanning overworld..");
                if (!await IsInBattle(token).ConfigureAwait(false))
                    await OverworldSaveGame(token).ConfigureAwait(false);

                if (await IsInBattle(token).ConfigureAwait(false))
                {
                    var check = await ReadPokemon(WildPokemonOffset, token).ConfigureAwait(false);
                    if (check.IsShiny)
                        await BallTosser(token).ConfigureAwait(false);

                    if (!check.IsShiny)
                    {
                        Log($"Unwanted encounter {(Species)check.Species}! Initializing {Settings.Conditions.UnwantedEncounterCorrection}!");
                        var task = Settings.Conditions.UnwantedEncounterCorrection switch
                        {
                            UnwantedCorrection.FleeBattle => FleeToOverworld(token),
                            UnwantedCorrection.KnockOut => KnockOut(token),
                            _ => ResetGameAsync(token),
                        };
                        await task.ConfigureAwait(false);
                    }
                }
                if (Hub.Config.StopConditions.ShinyTarget == TargetShinyType.NonShiny || Hub.Config.StopConditions.ShinyTarget == TargetShinyType.DisableOption)
                {
                    var pk = await NonShinyScan(offset, token).ConfigureAwait(false);
                    if (pk != null)
                    {
                        if (await HandleNonShiny(pk, token).ConfigureAwait(false))
                            return;
                    }
                }
                if (Hub.Config.StopConditions.ShinyTarget == TargetShinyType.AnyShiny)
                {
                    if (Settings.NavigationType == NavigationType.Fishing)
                    {
                        var pk = await FishingScan(offset, token).ConfigureAwait(false);
                        if (pk != null)
                        {
                            if (await HandleOverworldField(pk, token).ConfigureAwait(false))
                                return;
                        }
                    }
                    if (Settings.NavigationType != NavigationType.Fishing)
                    {
                        var pk = await ShinyScan(offset, sav, token).ConfigureAwait(false);
                        if (pk != null)
                        {
                            if (await HandleOverworldField(pk, token).ConfigureAwait(false))
                                return;
                        }
                    }
                }
                Log("Match not found, searching again...");
                if (!await IsInBattle(token).ConfigureAwait(false))
                {
                    var task = Settings.NavigationType switch
                    {
                        NavigationType.RunDownTpUp => DownTpUp(token),
                        NavigationType.Teleportation => TeleportDespawnToRespawn(token),
                        NavigationType.Run => Running(token),
                        NavigationType.Fishing => Fishing(token),
                        NavigationType.FlyInPlace => FlyInPlace(token),
                        _ => Running(token),
                    };
                    await task.ConfigureAwait(false);
                    await Task.Delay(0_500, token).ConfigureAwait(false);                        
                }
                if (await IsInBattle(token).ConfigureAwait(false))
                {
                    var check = await ReadPokemon(WildPokemonOffset, token).ConfigureAwait(false);
                    if (check.IsShiny)
                        await BallTosser(token).ConfigureAwait(false);
                    if (!check.IsShiny)
                    {
                        Log($"Unwanted encounter {(Species)check.Species}! Initializing {Settings.Conditions.UnwantedEncounterCorrection}!");
                        var task = Settings.Conditions.UnwantedEncounterCorrection switch
                        {
                            UnwantedCorrection.ResetGame => ResetGameAsync(token),
                            UnwantedCorrection.FleeBattle => FleeToOverworld(token),
                            UnwantedCorrection.KnockOut => KnockOut(token),
                            _ => ResetGameAsync(token),
                        };
                        await task.ConfigureAwait(false);
                    }
                }
            }
            Log("Applying rollover correction!");
            await RolloverCorrection(token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
        }
    }
    private async Task<bool> HandleOverworldField(PK8 pk, CancellationToken token)
    {
        TradeExtensions<PK8>.EncounterLogs(pk, "OverworldLogPretty.txt");
        var print = Hub.Config.StopConditions.GetSpecialPrintName(pk);
        Settings.AddCompletedScans();

        TradeExtensions<PK8>.EncounterLogs(pk, "OverworldLogPretty.txt");
        var url = TradeExtensions<PK8>.PokeImg(pk, false, false);
        var ping = string.Empty;
        if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
            ping = Hub.Config.StopConditions.MatchFoundEchoMention;
        bool hasMark = StopConditionSettings.HasMark(pk, out RibbonIndex mark);
        string markmsg = hasMark ? $"{mark.ToString().Replace("Mark", "")}mark" : "";
        string markurl = string.Empty;
        if (hasMark)
        {
            markurl = $"https://www.serebii.net/swordshield/ribbons/" + $"{markmsg.ToLower()}" + ".png";
            if (mark == RibbonIndex.MarkPumpedUp)
                markurl = $"https://www.serebii.net/swordshield/ribbons/pumped-upmark.png";
            if (mark == RibbonIndex.MarkAbsentMinded)
                markurl = $"https://www.serebii.net/swordshield/ribbons/absent-mindedmark.png";
            if (mark == RibbonIndex.MarkSleepyTime)
                markurl = $"https://www.serebii.net/swordshield/ribbons/sleepy-timemark.png";
            if (mark == RibbonIndex.MarkZonedOut)
                markurl = $"https://www.serebii.net/swordshield/ribbons/zoned-outmark.png";
        }

        if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
            DumpPokemon(DumpSetting.DumpFolder, "overworld", pk);

        if (pk.ShinyXor != 0)
        {
            EchoUtil.EchoEmbed(ping, $"Wait, STAR SHINY?!\n" + print, url, markurl, true);
            Log($"Pressing X to freeze Overworld spawns!");
            await Click(X, 0_600, token).ConfigureAwait(false);

            if (Settings.TeleportConditions.TeleportToMatch)
                await TeleportToMatch(token).ConfigureAwait(false);

            if (Settings.Sleep)
                await Sleep(token).ConfigureAwait(false);

            return true;

        }

        if (pk.Species == (int)Species.Sinistea)
        {
            bool authentic = pk.Form != 0;
            bool phony = !authentic;
            StopConditionSettings.HasMark(pk, out RibbonIndex specmark);
            string[] raremarks = ConditionalMarks.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string checkmark = $"{specmark.ToString().Replace("Mark", "")}";
            bool settle = raremarks.Contains(checkmark);
            if (authentic || settle)
            {
                EchoUtil.EchoEmbed(ping, $"One shiny antique teacup found... FOUND?!\n" + print, url, markurl, true);
                Log($"Pressing X to freeze Overworld spawns!");
                await Click(X, 0_600, token).ConfigureAwait(false);

                if (Settings.TeleportConditions.TeleportToMatch)
                    await TeleportToMatch(token).ConfigureAwait(false);

                return true;
            }
        }

        if (Settings.Conditions.StopOnBrilliantOrZeroStat)
        {
            bool brilliant = pk.FlawlessIVCount >= 1;
            bool zerostat = pk.IV_ATK == 0 || pk.IV_SPE == 0 || pk.IV_SPA == 0;
            if (brilliant || zerostat)
            {
                bool hasAMark = StopConditionSettings.HasMark(pk, out _);
                string bmsg = string.Empty;
                string zmsg = string.Empty;
                if (hasAMark)
                {
                    if (brilliant) bmsg = $"Brilliant Aura";
                    if (zerostat)
                    { if (pk.IV_SPE == 0) zmsg = $" 0 SPE"; if (pk.IV_SPA == 0) zmsg = $" 0 SPA"; if (pk.IV_ATK == 0) zmsg = $" 0 ATK"; }

                    EchoUtil.EchoEmbed(ping, $"{bmsg}{zmsg} match found on Scan: {overworldCount}!\n" + print, url, markurl, true);

                    Log($"Pressing X to freeze Overworld spawns!");
                    await Click(X, 0_600, token).ConfigureAwait(false);

                    if (Settings.TeleportConditions.TeleportToMatch)
                        await TeleportToMatch(token).ConfigureAwait(false);

                    if (Settings.Sleep)
                        await Sleep(token).ConfigureAwait(false);

                    return true;
                }
            }
        }

        StopConditionSettings.HasMark(pk, out RibbonIndex amark);
        string[] omark = Hub.Config.OverworldSWSH.Conditions.UnwantedMarks.Replace("Mark", "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string wmark = $"{amark.ToString().Replace("Mark", "")}";
        bool result = !omark.Contains(wmark);
        if (!result)
            EchoUtil.EchoEmbed("", print, url, markurl, false);
        string[] monlist = Settings.SpeciesToHunt.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (monlist.Length != 0)
        {
            bool huntedspecies = monlist.Contains($"{(Species)pk.Species}");
            if (huntedspecies)
            {
                bool yescondition = StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, UnwantedMarks);
                bool nocondition = !yescondition;
                if (yescondition)
                {
                    EchoUtil.EchoEmbed(ping, $"Match found on Scan: {overworldCount}!\n" + print, url, markurl, true);
                    Log($"Pressing X to freeze Overworld spawns!");
                    await Click(X, 0_600, token).ConfigureAwait(false);

                    await TeleportToMatch(token).ConfigureAwait(false);

                    if (Settings.Sleep)
                        await Sleep(token).ConfigureAwait(false);

                    return true;
                }
            }
            if (!huntedspecies)
            {
                StopConditionSettings.HasMark(pk, out RibbonIndex specmark);
                string[] raremarks = ConditionalMarks.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string checkmark = $"{specmark.ToString().Replace("Mark", "")}";
                bool settle = raremarks.Contains(checkmark);
                if (settle)
                {
                    EchoUtil.EchoEmbed(ping, $"Match found on Scan: {overworldCount}!\n" + print, url, markurl, true);
                    Log($"Pressing X to freeze Overworld spawns!");
                    await Click(X, 0_600, token).ConfigureAwait(false);

                    if (Settings.TeleportConditions.TeleportToMatch)
                        await TeleportToMatch(token).ConfigureAwait(false);

                    if (Settings.Sleep)
                        await Sleep(token).ConfigureAwait(false);

                    return true;
                }
            }
        }

        if (monlist.Length == 0)
        {
            bool matchfound = StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, UnwantedMarks) && result;
            bool nomatch = !matchfound;
            if (matchfound)
            {
                EchoUtil.EchoEmbed(ping, $"Match found on Scan: {overworldCount}!\n" + print, url, markurl, true);
                Log($"Pressing X to freeze Overworld spawns!");
                await Click(X, 0_600, token).ConfigureAwait(false);

                if (Settings.TeleportConditions.TeleportToMatch)
                    await TeleportToMatch(token).ConfigureAwait(false);

                if (Settings.Sleep)
                    await Sleep(token).ConfigureAwait(false);

                return true;
            }
        }
        EchoUtil.EchoEmbed("", print, url, markurl, false);
        return false;
    }

    private async Task<bool> HandleNonShiny(PK8 pk, CancellationToken token)
    {
        var print = Hub.Config.StopConditions.GetSpecialPrintName(pk);
        Log($"Scan #{overworldCount}{Environment.NewLine}{Environment.NewLine}{print}");
        TradeExtensions<PK8>.EncounterLogs(pk, "OverworldLogPretty.txt");
        var url = TradeExtensions<PK8>.PokeImg(pk, false, false);
        var ping = string.Empty;
        if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
            ping = Hub.Config.StopConditions.MatchFoundEchoMention;
        bool hasMark = StopConditionSettings.HasMark(pk, out RibbonIndex mark);
        string markmsg = hasMark ? $"{mark.ToString().Replace("Mark", "")}mark" : "";
        string markurl = string.Empty;
        if (hasMark)
        {
            markurl = $"https://www.serebii.net/swordshield/ribbons/" + $"{markmsg.ToLower()}" + ".png";
            if (mark == RibbonIndex.MarkPumpedUp)
                markurl = $"https://www.serebii.net/swordshield/ribbons/pumped-upmark.png";
            if (mark == RibbonIndex.MarkAbsentMinded)
                markurl = $"https://www.serebii.net/swordshield/ribbons/absent-mindedmark.png";
            if (mark == RibbonIndex.MarkSleepyTime)
                markurl = $"https://www.serebii.net/swordshield/ribbons/sleepy-timemark.png";
            if (mark == RibbonIndex.MarkZonedOut)
                markurl = $"https://www.serebii.net/swordshield/ribbons/zoned-outmark.png";
        }
        Settings.AddCompletedScans();

        if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
            DumpPokemon(DumpSetting.DumpFolder, "overworld", pk);

        bool matchfound = StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, UnwantedMarks);
        bool nomatch = StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, UnwantedMarks);

        if (matchfound)
        {
            EchoUtil.EchoEmbed(ping, $"Match found on Scan: {overworldCount}!\n" + print, url, markurl, true);
            Log($"Pressing X to freeze Overworld spawns!");
            await Click(X, 0_600, token).ConfigureAwait(false);

            if (Settings.TeleportConditions.TeleportToMatch)
                await TeleportToMatch(token).ConfigureAwait(false);

            if (Settings.Sleep)
                await Sleep(token).ConfigureAwait(false);

            return true;
        }
        EchoUtil.EchoEmbed("", print, url, markurl, false);
        return false;
    }
    
    public async Task ResetStick(CancellationToken token)
    {
        // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
        await SetStick(LEFT, 0, 0, 0_500, token).ConfigureAwait(false); // reset
    }

    private async Task ResetGameAsync(CancellationToken token)
    {
        Log("Resetting game to recover encounter!");
        await CloseGame(Hub.Config, token).ConfigureAwait(false);
        // Open game.
        await Click(A, 1_000 + Hub.Config.Timings.ExtraTimeLoadProfile, token).ConfigureAwait(false);

        // Menus here can go in the order: Update Prompt -> Profile -> DLC check -> Unable to use DLC.
        //  The user can optionally turn on the setting if they know of a breaking system update incoming.
        if (Hub.Config.Timings.AvoidSystemUpdate)
        {
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 1_000 + Hub.Config.Timings.ExtraTimeLoadProfile, token).ConfigureAwait(false);
        }

        await Click(A, 1_000 + Hub.Config.Timings.ExtraTimeCheckDLC, token).ConfigureAwait(false);
        // If they have DLC on the system and can't use it, requires an UP + A to start the game.
        // Should be harmless otherwise since they'll be in loading screen.
        await Click(DUP, 0_600, token).ConfigureAwait(false);
        await Click(A, 0_600, token).ConfigureAwait(false);

        Log("Restarting the game!");
        while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            await Click(A, 0_500, token).ConfigureAwait(false);

        OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
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
        if (Config.Connection.Protocol == SwitchProtocol.WiFi) // Scroll to system settings
            await PressAndHold(DDOWN, 2_000, 0, token).ConfigureAwait(false);
        await Click(A, 1_250, token).ConfigureAwait(false);

        if (Config.Connection.Protocol == SwitchProtocol.WiFi) // Scroll to date/time settings
            await PressAndHold(DDOWN, 0_750, 0, token).ConfigureAwait(false);
        await Click(DDOWN, 0_150, token).ConfigureAwait(false);

        await Click(A, 1_250, token).ConfigureAwait(false);
        await Click(DDOWN, Config.Connection.Protocol == SwitchProtocol.WiFi ? 1_000 : 0, token).ConfigureAwait(false);
        await Click(DDOWN, Config.Connection.Protocol == SwitchProtocol.WiFi ? 1_000 : 0, token).ConfigureAwait(false);
        await Click(A, 1_250, token).ConfigureAwait(false); // Scroll to date/time settings
        //await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        //await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        //Ticks for Hourly rollback
        for (int x = 0; x < Settings.RollBackTime; x++)
            await Click(DDOWN, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        await Click(DRIGHT, Config.Connection.Protocol == SwitchProtocol.WiFi ? 0_350 : 0, token).ConfigureAwait(false);
        await Click(A, 0_500, token).ConfigureAwait(false); // Turn sync off/on
        if (gameClosed)
        {
            for (int i = 0; i < 2; i++)
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(A, 1_250, token).ConfigureAwait(false);
            await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            for (int i = 0; i < 6; i++)
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(A, 0_750, token).ConfigureAwait(false);
        }

        await Click(HOME, 1_000, token).ConfigureAwait(false); // Back to title screen
        if (!gameClosed)
            await Click(HOME, 2_000, token).ConfigureAwait(false); // Back to game
        if (Hub.Config.Timings.AvoidSystemUpdate)
        {
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 1_000 + Hub.Config.Timings.ExtraTimeLoadProfile, token).ConfigureAwait(false);
        }
    }
    private async Task FleeToOverworld(CancellationToken token)
    {
        // Offsets are flickery so make sure we see it 3 times.
        for (int i = 0; i < 3; i++)
            await ReadUntilChanged(BattleMenuOffset, BattleMenuReady, 5_000, 0_100, true, token).ConfigureAwait(false);

        Log($"Running away from battle!");
        // This routine will always escape a battle.
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
    private async Task KnockOut(CancellationToken token)
    {
        var ourcheck = await ReadPartyMon(token);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var ourmove1 = (Move)ourcheck.Move1;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        int ourmove1pp = ourcheck.Move1_PP;
        Log($"{ourmove1} - {ourmove1pp} PP");
        if (ourmove1pp == 1)
        {
            Log($"Knocking out encounter!");
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Click(A, 0_500, token).ConfigureAwait(false);
            Log($"Insufficient PP for {ourmove1}! Refreshing our fighter!");
            await Click(X, 1_000, token).ConfigureAwait(false);
            await InjectBattleFighter(token).ConfigureAwait(false);
        }
        if (ourmove1pp != 1)
        {
            Log($"Knocking out encounter!");
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Click(A, 0_500, token).ConfigureAwait(false);
        }
        kocount++;
        Log($"Current KnockOut Count: {kocount}");
    }
    private async Task Fishing(CancellationToken token)
    {
        Log($"Going fishing!");
        await Click(PLUS, 1_000, token).ConfigureAwait(false);
        await Click(PLUS, 5_000, token).ConfigureAwait(false);
    }
    private async Task FlyInPlace(CancellationToken token)
    {
        Log("Flying in place!");
        await Click(X, 1_000, token).ConfigureAwait(false);
        await Click(A, 2_000, token).ConfigureAwait(false);
        while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            await Click(A, 0_500, token).ConfigureAwait(false);
    }
    public async Task TeleportDespawnToRespawn(CancellationToken token)
    {
        ofs = await ParsePointer("[[[[[[main+26365B8]+88]+1F8]+E0]+10]+E0]+60", token).ConfigureAwait(false);

        uint coordX1 = uint.Parse(Settings.TeleportConditions.ScanZoneX, System.Globalization.NumberStyles.AllowHexSpecifier);
        byte[] X1 = BitConverter.GetBytes(coordX1);
        uint coordY1 = uint.Parse(Settings.TeleportConditions.ScanZoneY, System.Globalization.NumberStyles.AllowHexSpecifier);
        byte[] Y1 = BitConverter.GetBytes(coordY1);
        uint coordZ1 = uint.Parse(Settings.TeleportConditions.ScanZoneZ, System.Globalization.NumberStyles.AllowHexSpecifier);
        byte[] Z1 = BitConverter.GetBytes(coordZ1);

        uint coordX2 = uint.Parse(Settings.TeleportConditions.DespawnZoneX, System.Globalization.NumberStyles.AllowHexSpecifier);
        byte[] X2 = BitConverter.GetBytes(coordX2);
        uint coordY2 = uint.Parse(Settings.TeleportConditions.DespawnZoneY, System.Globalization.NumberStyles.AllowHexSpecifier);
        byte[] Y2 = BitConverter.GetBytes(coordY2);
        uint coordZ2 = uint.Parse(Settings.TeleportConditions.DespawnZoneZ, System.Globalization.NumberStyles.AllowHexSpecifier);
        byte[] Z2 = BitConverter.GetBytes(coordZ2);

        var coord = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
        var coord2 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x4, 4, token).ConfigureAwait(false), 0);
        var coord3 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x8, 4, token).ConfigureAwait(false), 0);
        bool trash = coord.Equals(coord2) && coord2.Equals(coord3) && coord3.Equals(coord);
        if (coord == 0 || coord2 == 0 || coord3 == 0 || trash)
        {
            Log($"Pointer data is trashy! Restarting game to restart routine!");
            await ResetGameAsync(token).ConfigureAwait(false);
            await DoOverworldScan(token).ConfigureAwait(false);
        }
        await SwitchConnection.WriteBytesAbsoluteAsync(X2, ofs, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(Y2, ofs + 0x4, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(Z2, ofs + 0x8, token).ConfigureAwait(false);
        //↓ Location 2
        await ResetStick(token).ConfigureAwait(false);
        await Task.Delay(0_250, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(Y1, ofs + 0x4, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(Z1, ofs + 0x8, token).ConfigureAwait(false);
        // ↑ Location 1
        await ResetStick(token).ConfigureAwait(false);
        await Task.Delay(Settings.TeleportConditions.WaitMsBetweenTeleports, token).ConfigureAwait(false);
    }
    public async Task DownTpUp(CancellationToken token)
    {
        ofs = await ParsePointer("[[[[[[main+26365B8]+88]+1F8]+E0]+10]+E0]+60", token).ConfigureAwait(false);
        var ydown = (ushort)Hub.Config.OverworldSWSH.MovementConditions.MoveDownMs;
        var coord = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
        byte[] X1 = BitConverter.GetBytes(coord);
        var coord2 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x4, 4, token).ConfigureAwait(false), 0);
        byte[] Y1 = BitConverter.GetBytes(coord2);
        var coord3 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x8, 4, token).ConfigureAwait(false), 0);
        byte[] Z1 = BitConverter.GetBytes(coord3);

        if (await IsInBattle(token).ConfigureAwait(false))
        {
            Log($"Unwanted encounter! Initializing {Settings.Conditions.UnwantedEncounterCorrection}!");
            var task = Settings.Conditions.UnwantedEncounterCorrection switch
            {
                UnwantedCorrection.ResetGame => ResetGameAsync(token),
                UnwantedCorrection.FleeBattle => FleeToOverworld(token),
                UnwantedCorrection.KnockOut => KnockOut(token),
                _ => ResetGameAsync(token),
            };
            await task.ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
        }
        if (!await IsInBattle(token).ConfigureAwait(false))
        {
            Log($"Running to despawn!");
            await SetStick(LEFT, 0, -30000, ydown, token).ConfigureAwait(false); //↓
            await ResetStick(token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
            Log($"Teleporting to respawn!");
            //Location 1
            await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);
            await SwitchConnection.WriteBytesAbsoluteAsync(Y1, ofs + 0x4, token).ConfigureAwait(false);
            await SwitchConnection.WriteBytesAbsoluteAsync(Z1, ofs + 0x8, token).ConfigureAwait(false);
            await ResetStick(token).ConfigureAwait(false);
            //await Task.Delay(0_500, token).ConfigureAwait(false);
            await Task.Delay(Settings.TeleportConditions.WaitMsBetweenTeleports, token).ConfigureAwait(false);                
        }
    }
    public async Task DownUp(CancellationToken token)
    {
        var ydown = (ushort)Hub.Config.OverworldSWSH.MovementConditions.MoveDownMs;
        var yup = (ushort)Hub.Config.OverworldSWSH.MovementConditions.MoveUpMs;
        if (await IsInBattle(token).ConfigureAwait(false))
        {
            Log($"Unwanted encounter! Initializing {Settings.Conditions.UnwantedEncounterCorrection}!");
            var task = Settings.Conditions.UnwantedEncounterCorrection switch
            {
                UnwantedCorrection.FleeBattle => FleeToOverworld(token),
                UnwantedCorrection.KnockOut => KnockOut(token),
                _ => ResetGameAsync(token),
            };
            await task.ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
        }
        if (!await IsInBattle(token).ConfigureAwait(false))
        {
            await SetStick(LEFT, 0, -30000, ydown, token).ConfigureAwait(false); //↓
            await ResetStick(token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 30000, yup, token).ConfigureAwait(false); // ↑
            await ResetStick(token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
        }
    }
    public async Task DownDown(CancellationToken token)
    {
        var ydown = (ushort)Hub.Config.OverworldSWSH.MovementConditions.MoveDownMs;
        if (await IsInBattle(token).ConfigureAwait(false))
        {
            Log($"Unwanted encounter! Initializing {Settings.Conditions.UnwantedEncounterCorrection}!");
            var task = Settings.Conditions.UnwantedEncounterCorrection switch
            {
                UnwantedCorrection.FleeBattle => FleeToOverworld(token),
                UnwantedCorrection.KnockOut => KnockOut(token),
                _ => ResetGameAsync(token),
            };
            await task.ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
        }
        if (!await IsInBattle(token).ConfigureAwait(false))
        {
            await SetStick(LEFT, 0, -30000, ydown, token).ConfigureAwait(false); //↓
            await Task.Delay(1_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -30000, ydown, token).ConfigureAwait(false); //↓
            await ResetStick(token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
        }
    }
    public async Task RightLeft(CancellationToken token)
    {
        var yright = (ushort)Hub.Config.OverworldSWSH.MovementConditions.MoveRightMs;
        var yleft = (ushort)Hub.Config.OverworldSWSH.MovementConditions.MoveLeftMs;
        if (await IsInBattle(token).ConfigureAwait(false))
        {
            Log($"Unwanted encounter! Initializing {Settings.Conditions.UnwantedEncounterCorrection}!");
            var task = Settings.Conditions.UnwantedEncounterCorrection switch
            {
                UnwantedCorrection.FleeBattle => FleeToOverworld(token),
                UnwantedCorrection.KnockOut => KnockOut(token),
                _ => ResetGameAsync(token),
            };
            await task.ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
        }
        if (!await IsInBattle(token).ConfigureAwait(false))
        {
            await SetStick(LEFT, 30_000, 0, yright, token).ConfigureAwait(false); // →
            await ResetStick(token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
            await SetStick(LEFT, -30_000, 0, yleft, token).ConfigureAwait(false); // ↑
            await ResetStick(token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
        }
    }
    public async Task Running(CancellationToken token)
    {
        var task = Hub.Config.OverworldSWSH.NavigationOrder switch
        {
            NavigationOrder.DownUp => DownUp(token),
            NavigationOrder.DownDown => DownDown(token),
            NavigationOrder.RightLeft => RightLeft(token),
            _ => DownUp(token),
        };
        await task.ConfigureAwait(false);
    }

    private async Task SelectBall(CancellationToken token)
    {
        Log($"Selecting {Settings.BallTosser.DesiredBall} Ball...");
        var encounterBall = (Ball)Settings.BallTosser.DesiredBall;
        var index = new BallPouchUtil().BallIndex((int)Settings.BallTosser.DesiredBall);
        var ofs = await ParsePointer("[[[[[[main+2951270]+1D8]+818]+2B0]+2E0]+200]", token).ConfigureAwait(false);
        while (true)
        {
            int ball = BitConverter.ToInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
            if (ball == index)
                break;
            if (encounterBall.IsApricornBall())
                await Click(DLEFT, 0_050, token).ConfigureAwait(false);
            else await Click(DRIGHT, 0_050, token).ConfigureAwait(false);
        }
    }
    public static bool Immunity(int encounterAbility, int[] encounterTypes)
    {
        if (encounterTypes[0] == 4 || encounterTypes[1] == 4 || encounterTypes[0] == 7 || encounterTypes[1] == 7 || encounterTypes[0] == 5 || encounterTypes[1] == 5 ||
            encounterTypes[0] == 8 || encounterTypes[1] == 8 || encounterTypes[0] == 14 || encounterTypes[1] == 14)
            return true;

        return encounterAbility switch
        {
            (int)Ability.VoltAbsorb or (int)Ability.LightningRod or (int)Ability.MotorDrive => true,
            _ => false,
        };
    }

    private async Task BallTosser(CancellationToken token)
    {
        uint sleepvalue = 0x00000322;
        byte[] sleephax = BitConverter.GetBytes(sleepvalue);
        uint hpvalue = 0x00000001;
        byte[] hphax = BitConverter.GetBytes(hpvalue);
        int fainted = 0;
        int turn = 0;
        int isparalyed = 0;
        string status = string.Empty;
        MainNsoBase = await SwitchConnection.GetMainNsoBaseAsync(token).ConfigureAwait(false);
        BattleCheck = MainNsoBase + LairMiscScreenOffset;
        CaughtFlag = MainNsoBase + 0x29507E0;

        // Offsets are flickery so make sure we see it 3 times.
        for (int i = 0; i < 3; i++)
            await ReadUntilChanged(BattleMenuOffset, BattleMenuReady, 5_000, 0_100, true, token).ConfigureAwait(false);

        var check = await ReadPokemon(WildPokemonOffset, token).ConfigureAwait(false);
        if (check != null)
            DumpPokemon(DumpSetting.DumpFolder, "overworldtosser", check);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        int[] types = { check.PersonalInfo.Type1, check.PersonalInfo.Type2 };
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        bool immune = Immunity(check.Ability, types);
        bool groundedghost = (Species)check.Species == Species.Yamask || (Species)check.Species == Species.Golett || (Species)check.Species == Species.Golurk || (Species)check.Species == Species.Sandygast || (Species)check.Species == Species.Palossand || (Species)check.Species == Species.Runerigus;
        bool ghosttype = check.PersonalInfo.Type1 == 7 || check.PersonalInfo.Type2 == 7;
        bool sandstorm = await LairStatusCheck(0x3330306C, 0x4FCC20A0, token).ConfigureAwait(false);
        bool hailstorm = await LairStatusCheck(0x3230306C, 0x4FCC20A0, token).ConfigureAwait(false);
        bool thunderstorm = await LairStatusCheck(0x3130306C, 0x4FCC20A0, token).ConfigureAwait(false);
        bool statusheal = (Ability)check.Ability == Ability.Hydration && thunderstorm;
        if (sandstorm) Log("We're caught up in a sandstorm!"); if (hailstorm) Log("We're caught up in a hailstorm!"); if (thunderstorm) Log("We're caught up in a thunderstorm!");
        if (statusheal) Log($"{(Species)check.Species} has the Ability: Hydration. The rain makes it immune to status conditions for this battle!");
        bool badweather = sandstorm || hailstorm; bool hailimmune = hailstorm && immune; bool sandimmune = sandstorm && immune;

        while (!await LairStatusCheckMain(0x0000000C, CaughtFlag, token).ConfigureAwait(false))
        {
            check = await ReadPokemon(WildPokemonOffset, token).ConfigureAwait(false);
            if (check == null)
                break;

            var ourcheck = await ReadPartyMon(token);
            var monhp = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAsync(0x83F943B8, 4, token).ConfigureAwait(false), 0); // Our mon hp 
            var encounterhp = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAsync(0x83F94528, 4, token).ConfigureAwait(false), 0); // Encounter's hp

            if (await LairStatusCheckMain(0x0000032E, BattleCheck, token).ConfigureAwait(false))
            {
                bool paralyzed = await LairStatusCheck(0x00000001, 0x8FEA3188, token).ConfigureAwait(false) || await LairStatusCheck(0x00000009, 0x8FEA3188, token).ConfigureAwait(false);
                bool asleep = await LairStatusCheck(0x00000322, 0x8FEA3190, token).ConfigureAwait(false);
                turn++;
                Log($"Battle menu present! Resuming sequence on Turn: {turn}!");
                await Task.Delay(1_000, token).ConfigureAwait(false);

                if (asleep) status = $" - Asleep"; if (paralyzed) status = $" - Paralyzed";

                Log($"\n{(Species)check.Species} - {encounterhp}/{check.Stats[0]} HP{status}\nMoveset:\n{(Move)check.Move1} - {check.Move1_PP} PP\n{(Move)check.Move2} - {check.Move2_PP} PP\n{(Move)check.Move3} - {check.Move3_PP} PP\n{(Move)check.Move4} - {check.Move4_PP} PP\n" +
                    $"------------------------------" +
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    $"\n{(Species)ourcheck.Species} - {monhp}/{ourcheck.Stats[0]} HP\nMoveset:\n{(Move)ourcheck.Move1} - {ourcheck.Move1_PP} PP\n{(Move)ourcheck.Move2} - {ourcheck.Move2_PP} PP\n{(Move)ourcheck.Move3} - {ourcheck.Move3_PP} PP\n{(Move)ourcheck.Move4} - {ourcheck.Move4_PP} PP");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                await Click(B, 1_000, token).ConfigureAwait(false);

                if (Settings.BallTosser.BattleHax || paralyzed && encounterhp == 1 || paralyzed || groundedghost || paralyzed && encounterhp != 1 && immune || !paralyzed && encounterhp == 1 && immune || immune || !ourcheck.HasMove(86) || !ourcheck.HasMove(206))
                {
                    if (!asleep && Settings.BallTosser.BattleHax)
                    {
                        Log($"Applying sleep status!");
                        await Connection.WriteBytesAsync(sleephax, 0x8FEA3190, token).ConfigureAwait(false);

                        if (!badweather || sandimmune || hailimmune)
                        {
                            //Log($"Dropping Hp to 1!");
                            await Connection.WriteBytesAsync(hphax, 0x8FEA3164, token).ConfigureAwait(false);
                        }
                    }

                    await Click(X, 1_000, token).ConfigureAwait(false);
                    await SelectBall(token).ConfigureAwait(false); // Select ball to catch with.  
                    for (int i = 0; i < 6; i++)
                        await Click(A, 1_000, token).ConfigureAwait(false); // Throw ball  

                    while (!await LairStatusCheckMain(0x00000C3A, BattleCheck, token).ConfigureAwait(false) && !await LairStatusCheckMain(0x0000032E, BattleCheck, token).ConfigureAwait(false))
                    {
                        if (await LairStatusCheckMain(0x0000032E, BattleCheck, token).ConfigureAwait(false))
                            continue;
                        if (await LairStatusCheckMain(0x000006BA, BattleCheck, token).ConfigureAwait(false))
                            break;
                        if (await LairStatusCheckMain(0x0000000C, CaughtFlag, token).ConfigureAwait(false))
                        {
                            EchoUtil.Echo($"Gotcha! {(check.ShinyXor == 0 ? "■ - " : check.ShinyXor <= 16 ? "★ - " : "")}{(Species)check.Species} was caught!");
                            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                                await Click(A, 1_000, token).ConfigureAwait(false);
                            return;
                        }

                        await Click(A, 1_000, token).ConfigureAwait(false);// Mash A for good luck RNG
                    }
                }

                if (!paralyzed && Settings.BallTosser.ApplyParalysis && ourcheck.HasMove(86) && !Settings.BallTosser.BattleHax)
                {
                    if (!immune && !statusheal && !thunderstorm)
                    {
                        if (turn == 1 && !paralyzed)
                        {
                            await Click(A, 0_800, token).ConfigureAwait(false);

                            //Thunder Wave should be in slot 2 if we injected a battle fighter
                            Log($"Using {(Move)ourcheck.Move2}!");
                            await Click(DDOWN, 0_800, token).ConfigureAwait(false);

                            await Click(A, 0_800, token).ConfigureAwait(false);
                        }
                        if (turn >= 2 && !paralyzed)
                        {
                            await Click(A, 0_800, token).ConfigureAwait(false);
                            Log($"Using {(Move)ourcheck.Move2}!");
                            await Click(A, 0_800, token).ConfigureAwait(false);
                        }
                    }
                    else if (immune || statusheal || thunderstorm) Log($"Conditions are not good to apply paralysis!");
                }
                if (Settings.BallTosser.WhackTil1Hp && ourcheck.HasMove(206) /*&& !harakiri*/ && encounterhp != 1 && !Settings.BallTosser.BattleHax)// && encounterhp != 1 && !harakiri && !immune)
                {
                    if (badweather)
                    {
                        Log($"Looks like we're in some bad weather! Checking for weather immunity.");
                        if (sandimmune || hailimmune)
                        {
                            if (encounterhp != 1)
                            {
                                isparalyed++;
                                await Click(A, 1_000, token).ConfigureAwait(false);

                                if (paralyzed && Settings.BallTosser.ApplyParalysis && isparalyed <= 2)
                                    await Click(DUP, 1_000, token).ConfigureAwait(false);

                                Log($"Using {(Move)ourcheck.Move1}!");
                                await Click(A, 1_000, token).ConfigureAwait(false);
                            }
                        }
                    }
                    if (!badweather && !ghosttype)
                    {
                        if (encounterhp != 1)
                        {
                            isparalyed++;
                            await Click(A, 1_000, token).ConfigureAwait(false);

                            if (paralyzed && Settings.BallTosser.ApplyParalysis && isparalyed <= 2)
                                await Click(DUP, 1_000, token).ConfigureAwait(false);

                            Log($"Using {(Move)ourcheck.Move1}!");
                            await Click(A, 1_000, token).ConfigureAwait(false);
                        }
                    }
                }
            }

            await Task.Delay(15_000, token).ConfigureAwait(false);

            if (await LairStatusCheckMain(0x0000032E, BattleCheck, token).ConfigureAwait(false))// || await LairStatusCheckMain(0x00000C3A, BattleCheck, token).ConfigureAwait(false))
            {
                await Click(B, 1_000, token).ConfigureAwait(false);
                await Click(B, 1_000, token).ConfigureAwait(false);
            }

            if (await LairStatusCheckMain(0x000006BA, BattleCheck, token).ConfigureAwait(false))// Mon fainted animation
            {
                fainted++;
                await Task.Delay(5_000, token).ConfigureAwait(false);
                if (await LairStatusCheckMain(0x00000583, BattleCheck, token).ConfigureAwait(false))// Out of usable Pokemon
                {
                    await Click(A, 5_000, token).ConfigureAwait(false);
                    if (await LairStatusCheckMain(0x000002FA, BattleCheck, token).ConfigureAwait(false))
                    {
                        Log($"No more Pokémon can fight! Restarting the game to reattempt catch!");
                        await ResetGameAsync(token).ConfigureAwait(false);
                        //await OverworldSaveGame(token).ConfigureAwait(false);
                        await TeleportToMatch(token).ConfigureAwait(false);
                    }
                }
                if (await LairStatusCheckMain(0x00000B28, BattleCheck, token).ConfigureAwait(false))
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    Log($"Our {(Species)ourcheck.Species} fainted! Using next Pokémon!");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    if (await LairStatusCheckMain(0x000004DE, BattleCheck, token).ConfigureAwait(false)) // Party screen
                    {
                        Log($"Selecting next party Pokémon!");
                        for (int i = 0; i < fainted; i++)
                            await Click(DDOWN, 0_800, token).ConfigureAwait(false);
                        await Click(A, 0_800, token).ConfigureAwait(false);
                        await Click(A, 1_000, token).ConfigureAwait(false);
                        await Task.Delay(5_000, token).ConfigureAwait(false);
                        continue;
                    }
                }
            }
            if (encounterhp == 0)
            {
                Log($"{(Species)check.Species} fainted! Closing the game to restart catch attempt!");
                await ResetGameAsync(token).ConfigureAwait(false);
                await TeleportToMatch(token).ConfigureAwait(false);
            }
        }
        if (await LairStatusCheckMain(0x0000000C, CaughtFlag, token).ConfigureAwait(false)) // Sent to boxes
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            EchoUtil.Echo($"Gotcha! {(check.ShinyXor == 0 ? "■ - " : check.ShinyXor <= 16 ? "★ - " : "")}{(Species)check.Species} was caught!");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Click(A, 1_000, token).ConfigureAwait(false);
            return;
        }
    }
    public async Task InjectBattleFighter(CancellationToken token)
    {
        await SetCurrentBox(0, token).ConfigureAwait(false);
        string battlepk = string.Empty;
        var existing = await ReadBoxPokemon(0, 0, token).ConfigureAwait(false);
        if (existing.Species != 0 && existing.ChecksumValid)
        {
            Log("Destination slot is occupied! Dumping the Pokémon found there...");
            DumpPokemon(DumpSetting.DumpFolder, "saved", existing);
        }
        Log("Clearing destination slot to start the bot.");

        if (Settings.BallTosser.DesiredFighter == BattleInjector.Gallade)
            battlepk = $"Gallade (M) @ Leftovers\nEVs: 6 HP / 252 Atk / 252 Def\nAbility: Steadfast\nAdamant Nature\n- False Swipe\n- Thunder Wave\n- Taunt\n- Hypnosis";
        if (Settings.BallTosser.DesiredFighter == BattleInjector.Gardevoir && Settings.Conditions.UnwantedEncounterCorrection != UnwantedCorrection.KnockOut)
            battlepk = $"Gardevoir (F) @ Leftovers\nEVs: 252 HP / 6 Atk / 252 Def\nAbility: Synchronize\n{Settings.BallTosser.DesiredNature} Nature\n- Hypnosis\n- Thunder Wave\n- Dazzling Gleam\n- Shadow Ball";
        if (Settings.BallTosser.DesiredFighter == BattleInjector.Gardevoir && Settings.Conditions.UnwantedEncounterCorrection == UnwantedCorrection.KnockOut)
            battlepk = $"Gardevoir (F) @ Leftovers\nEVs: 6 HP / 252 Spa / 252 Spe\nAbility: Synchronize\n{Settings.BallTosser.DesiredNature} Nature\n- Dazzling Gleam\n- Thunder Wave\n- Shadow Ball\n- Hypnosis";

        //var pkm = AutoLegalityWrapper.GetTrainerInfo(8).GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet(battlepk)), out _);
        var set = new ShowdownSet(battlepk);
        var template = AutoLegalityWrapper.GetTemplate(set);
        var sav = await IdentifyTrainer(token).ConfigureAwait(false);
        PK8 pk = (PK8)sav.GetLegal(template, out _);
        await SetBoxPokemon(pk, 0, 0, token, sav).ConfigureAwait(false);
        // Begin swapping mon over for encounter
        Log($"Swapping {Settings.BallTosser.ReplacePartySlot} with a battle ready {Settings.BallTosser.DesiredFighter}!");
        await Click(DRIGHT, 1_000, token).ConfigureAwait(false);
        await Click(A, 4_000, token).ConfigureAwait(false);
        await Click(R, 3_000, token).ConfigureAwait(false);
        await Click(Y, 1_000, token).ConfigureAwait(false);
        await Click(A, 1_000, token).ConfigureAwait(false);
        await Click(DLEFT, 1_000, token).ConfigureAwait(false);
        for (int i = 0; i < (int)Settings.BallTosser.ReplacePartySlot; i++)
            await Click(DDOWN, 1_000, token).ConfigureAwait(false);
        await Click(A, 1_000, token).ConfigureAwait(false);
        await Click(B, 3_000, token).ConfigureAwait(false);
        await Click(B, 3_500, token).ConfigureAwait(false);
        await Click(DLEFT, 1_000, token).ConfigureAwait(false);
    }
    private async Task TeleportToMatch(CancellationToken token)
    {
        ofs = await ParsePointer("[[[[[[main+26365B8]+88]+1F8]+E0]+10]+E0]+60", token).ConfigureAwait(false);
        uint offset = uint.Parse(Settings.TeleportConditions.MatchFoundOffset, System.Globalization.NumberStyles.HexNumber);
        uint newoffset = offset - 0x30;

        uint coordx = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAsync(newoffset, 4, token).ConfigureAwait(false), 0);
        byte[] XX = BitConverter.GetBytes(coordx);
        uint coordy = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAsync(newoffset + 0x4, 4, token).ConfigureAwait(false), 0);
        byte[] YY = BitConverter.GetBytes(coordy);
        uint coordz = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAsync(newoffset + 0x8, 4, token).ConfigureAwait(false), 0);
        byte[] ZZ = BitConverter.GetBytes(coordz);

        Log($"Teleporting to our match!");
        await SwitchConnection.WriteBytesAbsoluteAsync(XX, ofs, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(YY, ofs + 0x4, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(ZZ, ofs + 0x8, token).ConfigureAwait(false);

        if (Settings.BallTosser.RefreshBattleFighter)
        {
            await InjectBattleFighter(token).ConfigureAwait(false);
        }
        if (Settings.Conditions.BallTosser)
        {
            Log($"Ball tosser enabled!");
            await Click(B, 1_000, token).ConfigureAwait(false);
            await Click(B, 5_000, token).ConfigureAwait(false);
            if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Click(A, 1_000, token).ConfigureAwait(false);
            pouchData = await Connection.ReadBytesAsync(PokeBallOffset, 116, token).ConfigureAwait(false);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            do
            {
                var pk = await ReadPokemon(WildPokemonOffset, token).ConfigureAwait(false);
                if (await IsInBattle(token).ConfigureAwait(false) && pk.IsShiny)
                {
                    Log($"In battle with {(pk.ShinyXor == 0 ? "■" : pk.ShinyXor <= 16 ? "★" : "")} {(Species)pk.Species}. Initiating catch sequence!");
                    await Click(B, 0_800, token).ConfigureAwait(false);
                    await Click(B, 1_000, token).ConfigureAwait(false);
                    if (Hub.Config.StopConditions.CaptureVideoClip)
                    {
                        await Task.Delay(Hub.Config.StopConditions.ExtraTimeWaitCaptureVideo, token).ConfigureAwait(false);
                        await PressAndHold(CAPTURE, 2_000, 0, token).ConfigureAwait(false);
                    }
                    sw.Stop();
                    await BallTosser(token).ConfigureAwait(false);

                    var mode = Settings.ContinueAfterMatch;
                    var msg = mode switch
                    {
                        ContinueAfterMatch.Continue => "Continuing...",
                        ContinueAfterMatch.PauseWaitAcknowledge => "Waiting for instructions to continue.",
                        ContinueAfterMatch.StopExit => "Stopping routine execution; restart the bot to search again.",
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                        msg = $"{msg}";
                    EchoUtil.Echo(msg);
                    Log(msg);

                    if (Settings.ContinueAfterMatch == ContinueAfterMatch.Continue)                        
                    {
                        while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                            await Click(B, 0_500, token).ConfigureAwait(false);

                        if (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                        {
                            Log($"Captured our shiny! Continuing the hunt...");
                            await Connection.WriteBytesAsync(pouchData, PokeBallOffset, token).ConfigureAwait(false);
                            await OverworldSaveGame(token).ConfigureAwait(false);
                            await Task.Delay(0_500, token).ConfigureAwait(false);
                        }
                        uint coordX1 = uint.Parse(Settings.TeleportConditions.ScanZoneX, System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] X1 = BitConverter.GetBytes(coordX1);
                        uint coordY1 = uint.Parse(Settings.TeleportConditions.ScanZoneY, System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] Y1 = BitConverter.GetBytes(coordY1);
                        uint coordZ1 = uint.Parse(Settings.TeleportConditions.ScanZoneZ, System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] Z1 = BitConverter.GetBytes(coordZ1);

                        uint coordX2 = uint.Parse(Settings.TeleportConditions.DespawnZoneX, System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] X2 = BitConverter.GetBytes(coordX2);
                        uint coordY2 = uint.Parse(Settings.TeleportConditions.DespawnZoneY, System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] Y2 = BitConverter.GetBytes(coordY2);
                        uint coordZ2 = uint.Parse(Settings.TeleportConditions.DespawnZoneZ, System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] Z2 = BitConverter.GetBytes(coordZ2);

                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        ofs = await ParsePointer("[[[[[[main+26365B8]+88]+1F8]+E0]+10]+E0]+60", token).ConfigureAwait(false);
                        if (Settings.NavigationType == NavigationType.Teleportation)
                        {
                            Log($"Teleporting to our Safe Zone!");
                            await SwitchConnection.WriteBytesAbsoluteAsync(X2, ofs, token).ConfigureAwait(false);
                            await SwitchConnection.WriteBytesAbsoluteAsync(Y2, ofs + 0x4, token).ConfigureAwait(false);
                            await SwitchConnection.WriteBytesAbsoluteAsync(Z2, ofs + 0x8, token).ConfigureAwait(false);

                            await Task.Delay(0_500, token).ConfigureAwait(false);

                            Log($"Teleporting to the Scan Zone!");
                            //Location 1
                            await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);
                            await SwitchConnection.WriteBytesAbsoluteAsync(Y1, ofs + 0x4, token).ConfigureAwait(false);
                            await SwitchConnection.WriteBytesAbsoluteAsync(Z1, ofs + 0x8, token).ConfigureAwait(false);
                        }
                        if (Settings.NavigationType == NavigationType.RunDownTpUp)
                        {
                            Log($"Teleporting to the Scan Zone!");
                            //Location 1
                            await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);
                            await SwitchConnection.WriteBytesAbsoluteAsync(Y1, ofs + 0x4, token).ConfigureAwait(false);
                            await SwitchConnection.WriteBytesAbsoluteAsync(Z1, ofs + 0x8, token).ConfigureAwait(false);

                            // await Task.Delay(0_500, token).ConfigureAwait(false);
                        }
                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        await DoOverworldScan(token).ConfigureAwait(false);
                    }
                    if (Settings.ContinueAfterMatch == ContinueAfterMatch.PauseWaitAcknowledge)
                    {
                        if (await Acknowledge(token))
                            return;

                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        await DoOverworldScan(token).ConfigureAwait(false);
                    }

                    if (Settings.ContinueAfterMatch == ContinueAfterMatch.StopExit)
                        return;
                }

                if (await IsInBattle(token).ConfigureAwait(false) && !pk.IsShiny)
                {
                    Log($"In battle with {(pk.ShinyXor == 0 ? "■" : pk.ShinyXor <= 16 ? "★" : "")} {(Species)pk.Species}. Initiating {Settings.Conditions.UnwantedEncounterCorrection}!");
                    var task = Settings.Conditions.UnwantedEncounterCorrection switch
                    {
                        UnwantedCorrection.ResetGame => ResetGameAsync(token),
                        UnwantedCorrection.FleeBattle => FleeToOverworld(token),
                        UnwantedCorrection.KnockOut => KnockOut(token),
                        _ => ResetGameAsync(token),
                    };
                    await task.ConfigureAwait(false);
                    Log($"{(!Hub.Config.StopConditions.MatchFoundEchoMention.Equals(string.Empty) ? $"<@{Hub.Config.StopConditions.MatchFoundEchoMention}>" : "")} not in battle, shiny must have moved while scanning! Pressing X to freeze Overworld spawns!");
                    await Click(X, 1_000, token).ConfigureAwait(false);
                    return;
                }
            } while (sw.ElapsedMilliseconds < 20_000);

            if (sw.ElapsedMilliseconds >= 20_000 && await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                Log($"{(!Hub.Config.StopConditions.MatchFoundEchoMention.Equals(string.Empty) ? $"<@{Hub.Config.StopConditions.MatchFoundEchoMention}>" : "")} not in battle, shiny must have moved while scanning! Pressing X to freeze Overworld spawns!");
                await Click(X, 1_000, token).ConfigureAwait(false);
                return;
            }
        }
    }
    public async Task<PK8?> NonShinyScan(uint offset, CancellationToken token)
    {
        byte[] data;
        Species species;

        if (Hub.Config.OverworldSWSH.NavigationType == NavigationType.Teleportation || Hub.Config.OverworldSWSH.NavigationType == NavigationType.RunDownTpUp)
            await Click(X, 1_000, token).ConfigureAwait(false);

        data = await Connection.ReadBytesAsync(offset, 56, token).ConfigureAwait(false);
        species = (Species)BitConverter.ToUInt16(data.AsSpan(0, 2).ToArray(), 0);
        overworldCount++;
        Settings.AddCompletedScans();

        if (Hub.Config.OverworldSWSH.NavigationType == NavigationType.Teleportation || Hub.Config.OverworldSWSH.NavigationType == NavigationType.RunDownTpUp)
            await Click(B, 0_800, token).ConfigureAwait(false);

        if (data != null)
        {
            PK8 pk = new PK8();
            pk.Species = (ushort)(int)species;
            pk.Form = data[2];
            if (data[22] != 255)
                pk.SetRibbonIndex((RibbonIndex)data[22]);
            int ivs = data[18];
            uint seed = BitConverter.ToUInt32(data.AsSpan(24, 4).ToArray(), 0);

            Settings.TeleportConditions.MatchFoundOffset = $"{offset:X8}";

            pk = ScanOverworld.CalculateIVs(pk, ivs, seed);
            return pk;
        }
        else
            return null;
    }
    public async Task<PK8?> FishingScan(uint fishingoffset, CancellationToken token)
    {
        byte[] data;
        uint offset = fishingoffset;
        int species = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset, 2, token).ConfigureAwait(false), 0);
        string mark = await HasOverworldMark(offset, token).ConfigureAwait(false);
        string isshiny = await ScanIsShiny(offset, token).ConfigureAwait(false);
        int shiny = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset + 0x6, 2, token).ConfigureAwait(false), 0);
        uint seed = BitConverter.ToUInt32(await Connection.ReadBytesAsync(offset + 0x18, 4, token).ConfigureAwait(false), 0);
        overworldCount++;
        Settings.AddCompletedScans();
        Log($"\nScan #{overworldCount}: {(Species)species}. Overworld Seed: {String.Format("{0:X}", seed)}. {isshiny}. {mark}");

        if (shiny == 1)
        {
            data = await Connection.ReadBytesAsync(offset, 56, token).ConfigureAwait(false);
            PK8 pk = new PK8
            {
                Species = (ushort)species,
                Gender = (byte)((data[10] == 1) ? 0 : 1),
            };
            if (data[22] != 255)
                pk.SetRibbonIndex((RibbonIndex)data[22]);
            if (!pk.IsGenderValid())
                pk.Gender = 2;

            Move Move1 = (Move)BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset + 0x30, 2, token).ConfigureAwait(false), 0);
            if (Move1 != 0)
                pk.Move1 = (ushort)(int)Move1;

            Shiny shinyness = (Shiny)(data[6] + 1);
            int ivs = data[18];
            pk = ScanOverworld.CalculateSeed(pk, shinyness, ivs, seed);
            return pk;

        }
        else
            return null;
    }
    
    public async Task<PK8?> ShinyScan(uint startoffset, SAV8SWSH TrainerData, CancellationToken token)
    {
        byte[] data;
        int shinytype;
        int form;
        int aura;
        //int givs;
        string formtype = string.Empty;
        string auratype = string.Empty;
        uint overworldseed;
        Species species;
        uint offset = startoffset;
        int i = 0;
        string log = string.Empty;
        string moveset = string.Empty;

        if (Hub.Config.OverworldSWSH.NavigationType == NavigationType.Teleportation || Hub.Config.OverworldSWSH.NavigationType == NavigationType.RunDownTpUp)
            await Click(X, 1_000, token).ConfigureAwait(false);

        do
        {
            data = await Connection.ReadBytesAsync(offset, 56, token).ConfigureAwait(false);
            species = (Species)BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset, 2, token).ConfigureAwait(false), 0);
            form = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset + 0x2, 2, token).ConfigureAwait(false), 0);
            //givs = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset + 0x12, 2, token).ConfigureAwait(false), 0);
            aura = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset + 0x20, 2, token).ConfigureAwait(false), 0);
            string mark = await HasOverworldMark(offset, token).ConfigureAwait(false);
            string isshiny = await ScanIsShiny(offset, token).ConfigureAwait(false);
            overworldseed = BitConverter.ToUInt32(await Connection.ReadBytesAsync(offset + 0x18, 4, token).ConfigureAwait(false), 0);
            if (form is 1 or 2) formtype = $"-Galar"; if (species == Species.Sinistea) { if (form == 1) formtype = $"-Antique"; if (form == 0) formtype = $"-Phony"; }
            if (species != Species.Sinistea && form == 0) formtype = $"";
            if (species is Species.Pumpkaboo or Species.Gourgeist)
            { if (form == 0) formtype = $""; if (form == 1) formtype = $" (S)"; if (form == 2) formtype = $" (L)"; if (form == 3) formtype = $" (XL)"; }

            if (aura != 0) auratype = $"\n- Has a Brilliant Aura."; if (aura == 0) auratype = $"";
            Move Move1 = (Move)BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset + 0x30, 2, token).ConfigureAwait(false), 0);
            if (Move1 != 0) moveset = $"\n- Has the Move: {Move1}."; if (Move1 == 0) moveset = $"";

            shinytype = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset + 0x6, 2, token).ConfigureAwait(false), 0);

            if (species != Species.None && species < Species.MAX_COUNT && (PersonalTable.SWSH[(int)species]).IsPresentInGame && overworldseed != 0 && overworldseed > 7999999)
            {
                overworldCount++;
                Settings.AddCompletedScans();
                log += $"\nScan #{overworldCount}: {species}{formtype}. Overworld Seed: {overworldseed:X8}. {isshiny}. {mark}{auratype}{moveset}";
            }
            offset += 192;
            i++;
            if (i == Settings.Conditions.IncrementFromOffset || shinytype == 1)
                Log(log);
            if (string.IsNullOrEmpty(log))
            {
                Log("Overworld data shifted, trying again...");
                _ = await GetOverworldOffsets(token).ConfigureAwait(false);
                break;
            }

        } while (shinytype != 1 && i < Settings.Conditions.IncrementFromOffset);

        if (Hub.Config.OverworldSWSH.NavigationType == NavigationType.Teleportation || Hub.Config.OverworldSWSH.NavigationType == NavigationType.RunDownTpUp)
            await Click(B, 1_000, token).ConfigureAwait(false);

        if (shinytype == 1)
        {
            PK8 pk = new()
            {
                Species = (ushort)(int)species,
                Form = data[2],
                CurrentLevel = data[4],
                MetLevel = data[4],
                Gender = (byte)((data[10] == 1) ? 0 : 1),
                Language = 2,
                OriginalTrainerName = TrainerData.OT,
                TID16 = TrainerData.TID16,
                SID16 = TrainerData.SID16,
                TrainerTID7 = TrainerData.TrainerTID7,
                TrainerSID7 = TrainerData.TrainerSID7,
                OriginalTrainerGender = TrainerData.Gender,
                HandlingTrainerName = TrainerData.OT,
                HandlingTrainerGender = TrainerData.Gender,
            };
            pk.SetNature((Nature)data[8]);
            pk.SetAbility(data[12] - 1);
            if (data[22] != 255)
                pk.SetRibbonIndex((RibbonIndex)data[22]);
            if (!pk.IsGenderValid())
                pk.Gender = 2;

            pk.Nickname = $"{(int)species}";
            pk.ClearNickname();
            pk.IsNicknamed = false;

            if (Version == GameVersion.SW)
                pk.Version = (GameVersion)44;
            if (Version == GameVersion.SH)
                pk.Version = (GameVersion)45;

            Move Move1 = (Move)BitConverter.ToUInt16(data.AsSpan(48, 2).ToArray(), 0);
            if (Move1 != 0)
                pk.Move1 = (ushort)(int)Move1;

            Shiny shinyness = (Shiny)(data[6] + 1);
            int ivs = data[18];
            uint seed = BitConverter.ToUInt32(data.AsSpan(24, 4).ToArray(), 0);
            string nature = $"{(Nature)data[8]}";
            pk = ScanOverworld.CalculateSeed(pk, shinyness, ivs, seed);
            string brilliant = string.Empty;
            if (pk.FlawlessIVCount >= 1)
                brilliant = $"\n- Has a Brilliant Aura!";

            Log($"({pk.Species}) - {(Species)pk.Species} - Ability: {(Ability)pk.Ability} - {nature} Nature.{brilliant}");
            uint newoffset = offset - 192;
            Settings.TeleportConditions.MatchFoundOffset = $"{newoffset:X8}";
            return pk;

        }
        else
            return null;
    }
    public async Task<bool> Acknowledge(CancellationToken token)
    {            
        IsWaiting = true;
        while (IsWaiting)
            await Task.Delay(1_000, token).ConfigureAwait(false);
        return false;
    }

    public async Task OverworldSaveGame(CancellationToken token)
    {
        await Click(X, 1_000, token).ConfigureAwait(false);
        await Click(R, 2_000, token).ConfigureAwait(false);
        await Click(A, 3_500, token).ConfigureAwait(false);
    }
    
}