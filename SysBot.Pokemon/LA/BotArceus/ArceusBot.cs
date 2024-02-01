using PKHeX.Core;
using PermuteMMO.Lib;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;
using ResultsUtil = SysBot.Base.ResultsUtil;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.PokeDataOffsetsLA;
using SysBot.Base;

namespace SysBot.Pokemon;

public sealed class ArceusBot : PokeRoutineExecutor8LA, IEncounterBot, IArceusBot
{
    private readonly PokeTradeHub<PA8> Hub;
    private readonly IDumper DumpSetting;
    private readonly int[] DesiredMinIVs;
    private readonly int[] DesiredMaxIVs;
    private readonly ArceusBotSettings Settings;
    public ICountSettings Counts => Settings;

    public ArceusBot(PokeBotState cfg, PokeTradeHub<PA8> hub) : base(cfg)
    {
        Hub = hub;
        Settings = Hub.Config.ArceusLA;
        DumpSetting = Hub.Config.Folder;
        StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
    }

    private ulong MainNsoBase;
    private ulong OverworldOffset;
    private (string, string, string) coordinates;
    private List<PA8> boxlist = new();
    private bool HasCharm = false;

    private static readonly string[] ObsidianTitle =
    {
        "Rapidash","Snorlax","Luxio","Floatzel","Staravia","Parasect","Kricketune","Stantler","Bibarel","Scyther","Lopunny","Graveler","Blissey","Heracross","Magikarp","Infernape","Alakazam","Gyarados",
    };

    private static readonly string[] CrimsonTitle =
    {
        "Tangrowth","Hippowdon","Skuntank","Onix","Rhyhorn","Honchkrow","Roserade","Lickilicky","Pachirisu","Carnivine","Vespiquen","Yanmega","Ursaring","Toxicroak","Torterra","Sliggoo","Raichu","Ursaring","Whiscash",
    };

    private static readonly string[] CoronetTitle =
    {
        "Mothim","Bronzong","Carnivine","Gligar","Gabite","Luxray","Electivire","Goodra","Steelix","Clefable","Golem","Mismagius","Rhyperior","Probopass","Gliscor",
    };

    private static readonly string[] CobaltTitle =
    {
        "Walrein","Drapion","Purugly","Ambipom","Golduck","Dusknoir","Machoke","Octillery","Mantine","Tentacruel","Ninetales","Chansey","Lumineon","Gyarados","Gastrodon","Qwilfish","Empoleon","Mothim",
    };

    private static readonly string[] AlabasterTitle =
    {
        "Glalie","Abomasnow","Mamoswine","Gardevoir","Sneasel","Chimecho","Machamp","Swinub","Piloswine","Lucario","Electabuzz","Froslass","Garchomp",
    };

    public override async Task MainLoop(CancellationToken token)
    {
        Log("Identifying trainer data of the host console.");
        var trainersav = await IdentifyTrainer(token).ConfigureAwait(false);
        await InitializeHardware(Settings, token).ConfigureAwait(false);
        try
        {
            Log($"Starting main {GetType().Name} loop.");
            Config.IterateNextRoutine();
            HasCharm = await CheckForCharm(token).ConfigureAwait(false);
            var dex = await ReadPokedex(token).ConfigureAwait(false);
            await ResetStick(token).ConfigureAwait(false);
            MainNsoBase = await SwitchConnection.GetMainNsoBaseAsync(token).ConfigureAwait(false);
            var task = Hub.Config.ArceusLA.BotType switch
            {
                ArceusMode.PlayerCoordScan => PlayerCoordScan(token),
                ArceusMode.SeedAdvancer => SeedAdvancer(token),
                ArceusMode.TimeSeedAdvancer => TimeSeedAdvancer(token),
                ArceusMode.StaticAlphaScan => ScanForSpawners(token),
                ArceusMode.DistortionSpammer => DistortionSpammer(token),
                ArceusMode.DistortionReader => DistortionReader(dex, token),
                ArceusMode.MassiveOutbreakHunter => MassiveOutbreakHunter(dex, token),
                ArceusMode.MultiSpawnPathSearch => PerformMultiSpawnerScan(dex, token),
                ArceusMode.GenieScanner => ScanForSpawners(token),
                ArceusMode.ManaphyReset => ManaphyReset(token),
                ArceusMode.OtherLegendReset => ScanForSpawners(token),
                ArceusMode.AdvanceTimeDelay => AdvanceTimeDelay(token),
                _ => PlayerCoordScan(token),
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
    private bool IsWaitingConfirmation;
    public void Acknowledge() => IsWaiting = false;
    public void AcknowledgeConfirmation() => IsWaitingConfirmation = false;

    private async Task TimeTest(CancellationToken token)
    {
        var timeofs = await NewParsePointer(TimePtrLA, token).ConfigureAwait(false);
        var timeVal = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(timeofs, 2, token).ConfigureAwait(false), 0);
        if (timeVal <= 7)
            timeVal = 15;
        else timeVal = 00;

        byte[] timeByte = BitConverter.GetBytes(timeVal);
        await SwitchConnection.WriteBytesAbsoluteAsync(timeByte, timeofs, token).ConfigureAwait(false);
    }

    private async Task AdvanceTimeDelay(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Log("Changing time to advance frames...");
            await TimeTest(token).ConfigureAwait(false);
            Log("Waiting for confirmation before advancing again...");
            IsWaitingConfirmation = true;
            while (IsWaitingConfirmation)
                await Task.Delay(0_500, token).ConfigureAwait(false);
        }
    }

    private async Task PlayerCoordScan(CancellationToken token)
    {
        var ofs = await NewParsePointer(PlayerCoordPtrLA, token).ConfigureAwait(false);
        var coord = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs, 4, token).ConfigureAwait(false), 0);
        var coord2 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x4, 4, token).ConfigureAwait(false), 0);
        var coord3 = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(ofs + 0x8, 4, token).ConfigureAwait(false), 0);

        Log($"Current Player Coords - X: {coord:X8} Y: {coord2:X8} Z: {coord3:X8}");

        if (Settings.AlphaScanConditions.AutoFillCoords == ArceusAutoFill.CampZone)
        {
            Log($"Autofilling CampZone XYZ");
            Settings.SpecialConditions.CampZoneX = $"{coord:X8}";
            Settings.SpecialConditions.CampZoneY = $"{coord2:X8}";
            Settings.SpecialConditions.CampZoneZ = $"{coord3:X8}";
        }

        if (Settings.AlphaScanConditions.AutoFillCoords == ArceusAutoFill.SpawnZone)
        {
            Log($"Autofilling SpawnZone XYZ");
            Settings.SpecialConditions.SpawnZoneX = $"{coord:X8}";
            Settings.SpecialConditions.SpawnZoneY = $"{coord2:X8}";
            Settings.SpecialConditions.SpawnZoneZ = $"{coord3:X8}";
        }
    }

    private async Task Reposition(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Log("Not in camp, repositioning and trying again.");
            await TeleportToCampZone(token);
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);

            var menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
            Log($"test: {menucheck}");
            if (menucheck == 66)
                return;

            Log("Attempting face up");
            await TeleportToCampZone(token);
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 5_000, 0_500, token).ConfigureAwait(false); // reset face forward
            await ResetStick(token).ConfigureAwait(false); // reset
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);

            menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
            Log($"test: {menucheck}");
            if (menucheck == 66)
                return;

            Log("Attempting face down");
            await TeleportToCampZone(token);
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -5_000, 0_500, token).ConfigureAwait(false); // reset face forward
            await ResetStick(token).ConfigureAwait(false); // reset                
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);

            menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
            Log($"test: {menucheck}");
            if (menucheck == 66)
                return;
            Log("Attempting face right");
            await TeleportToCampZone(token);
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 5_000, 0, 0_500, token).ConfigureAwait(false); // reset face forward
            await ResetStick(token).ConfigureAwait(false); // reset
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);

            menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
            Log($"test: {menucheck}");
            if (menucheck == 66)
                return;
            Log("Attempting face left");
            await TeleportToCampZone(token);
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);
            await SetStick(LEFT, -5_000, 0, 0_500, token).ConfigureAwait(false); // reset face forward
            await ResetStick(token).ConfigureAwait(false); // reset
            for (int i = 0; i < 2; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);

            menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
            Log($"test: {menucheck}");
            if (menucheck == 66)
                return;
        }
    }

    private async Task DistortionReader(PokedexSaveData dex, CancellationToken token)
    {
        List<PA8> matchlist = new();
        List<string> loglist = new();
        Log($"Starting Distortion Scanner for {Settings.DistortionConditions.DistortionLocation}...");
        int tries = 1;
        int count = Settings.DistortionConditions.DistortionLocation switch
        {
            ArceusMap.ObsidianFieldlands => 16,
            ArceusMap.CrimsonMirelands => 25,
            ArceusMap.CobaltCoastlands or ArceusMap.CoronetHighlands => 20,
            ArceusMap.AlabasterIcelands => 24,
            _ => 0,
        };

        while (!token.IsCancellationRequested)
        {
            Log($"Scan #{tries}...");
            for (int i = 0; i < count; i++)
            {
                int encounter_slot_sum;
                int common_sum;
                long[] disofs;
                switch (Settings.DistortionConditions.DistortionLocation)
                {
                    case ArceusMap.ObsidianFieldlands: disofs = new long[] { 0x42CC4D8, 0xC0, 0x1C0, 0x990 + i * 0x8, 0x18, 0x430, 0xC0 }; encounter_slot_sum = 112; common_sum = 546; break;
                    case ArceusMap.CrimsonMirelands: disofs = new long[] { 0x42CC4D8, 0xC0, 0x1C0, 0xC78 + i * 0x8, 0x18, 0x430, 0xC0 }; encounter_slot_sum = 276; common_sum = 480; break;
                    case ArceusMap.CobaltCoastlands: disofs = new long[] { 0x42CC4D8, 0xC0, 0x1C0, 0xCC0 + i * 0x8, 0x18, 0x430, 0xC0 }; encounter_slot_sum = 163; common_sum = 529; break;
                    case ArceusMap.CoronetHighlands: disofs = new long[] { 0x42CC4D8, 0xC0, 0x1C0, 0x818 + i * 0x8, 0x18, 0x430, 0xC0 }; encounter_slot_sum = 382; common_sum = 382; break;
                    case ArceusMap.AlabasterIcelands: disofs = new long[] { 0x42CC4D8, 0xC0, 0x1C0, 0x948 + i * 0x8, 0x18, 0x430, 0xC0 }; encounter_slot_sum = 259; common_sum = 675; break;
                    default: throw new NotImplementedException("Invalid distortion location.");
                }

                var SpawnerOff = await SwitchConnection.PointerAll(disofs, token).ConfigureAwait(false);
                var GeneratorSeed = await SwitchConnection.ReadBytesAbsoluteAsync(SpawnerOff, 8, token).ConfigureAwait(false);
                var group_seed = (BitConverter.ToUInt64(GeneratorSeed, 0) - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
                if (group_seed != 0)
                {
                    if (i >= 13 && i <= 15 && Settings.DistortionConditions.DistortionLocation == ArceusMap.CrimsonMirelands)
                        encounter_slot_sum = 118;

                    var (match, shiny, logs) = ReadDistortionSeed(dex, i, group_seed, encounter_slot_sum, common_sum);
                    loglist.Add(logs);
                    string[] monlist = Settings.SpeciesToHunt.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (shiny || Settings.DistortionConditions.AnyAlpha && match.IsAlpha)
                    {
                        if (monlist.Length != 0)
                        {
                            bool huntedspecies = monlist.Contains($"{(Species)match.Species}");
                            if (!huntedspecies)
                            {
                                string url = TradeExtensions<PK9>.PokeImg(match, false, false);
                                var print = Hub.Config.StopConditions.GetAlphaPrintName(match);
                                EchoUtil.EchoEmbed("", print, url, "", false);
                                Log(logs);
                                break;
                            }
                        }
                        matchlist.Add(match);

                        if (Settings.DistortionConditions.AnyAlpha && match.IsAlpha)
                        {
                            // Activates invcincible trainer cheat so we don't faint from teleporting or a Pokemon attacking and infinite PP
                            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer1, token).ConfigureAwait(false);
                            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer2, token).ConfigureAwait(false);

                            Log($"Found an Alpha {(Species)match.Species}. Storing its coordinates...\nPress continue if a desired encounter to teleport to, otherwise toss to toss.\nReference the image guide if needed: https://imgur.com/a/OyBIIbR");
                            string url = TradeExtensions<PK9>.PokeImg(match, false, false);
                            var print = Hub.Config.StopConditions.GetAlphaPrintName(match);
                            EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                            FillDistortionCoords(i);
                            IsWaiting = true;
                            IsWaitingConfirmation = true;
                            await Click(HOME, 1_000, token).ConfigureAwait(false);

                            while (IsWaiting && IsWaitingConfirmation)
                            {
                                if (IsWaiting && !IsWaitingConfirmation || !IsWaiting && IsWaitingConfirmation)
                                    break;
                                await Task.Delay(1_000, token).ConfigureAwait(false);
                            }
                            await Click(HOME, 1_000, token).ConfigureAwait(false);

                            if (Settings.DistortionConditions.TeleportToDistortionLocation && !IsWaitingConfirmation)
                                await TeleportToDistortionZone(token).ConfigureAwait(false);

                        }
                    }
                }
                foreach (PA8 match in matchlist)
                {
                    if (match.IsShiny)
                    {
                        string url = string.Empty;
                        string print = string.Empty;
                        if (Settings.DistortionConditions.ShinyAlphaOnly)
                        {
                            if (!match.IsAlpha)
                            {
                                url = TradeExtensions<PK9>.PokeImg(match, false, false);
                                print = Hub.Config.StopConditions.GetAlphaPrintName(match);
                                EchoUtil.EchoEmbed("", print, url, "", false);
                                break;
                            }
                        }
                        Log(loglist.Last());

                        url = TradeExtensions<PK9>.PokeImg(match, false, false);
                        print = Hub.Config.StopConditions.GetAlphaPrintName(match);
                        EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                        Log($"\nReference the image guide if needed: https://imgur.com/a/OyBIIbR");
                        Settings.AddCompletedShinyAlphaFound();

                        if (Settings.DistortionConditions.TeleportToDistortionLocation)
                            await TeleportToDistortionZone(token).ConfigureAwait(false);

                        await Click(HOME, 1_000, token).ConfigureAwait(false);
                        IsWaiting = true;
                        while (IsWaiting)
                            await Task.Delay(1_000, token).ConfigureAwait(false);

                        if (!IsWaiting)
                            await Click(HOME, 1_000, token).ConfigureAwait(false);
                    }
                }
                matchlist.Clear();
            }
            string report = string.Join("\n", loglist);
            Log(report);
            loglist.Clear();
            tries++;

            if (Settings.OutbreakConditions.CheckDistortionFirst && Settings.BotType == ArceusMode.MassiveOutbreakHunter)
                return;

            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token).ConfigureAwait(false);
        }
    }

    private (string, string, string) FillDistortionCoords(int id) => Settings.DistortionConditions.DistortionLocation switch
    {
        ArceusMap.ObsidianFieldlands when id is <= 4 => coordinates = ("4402d333", "4206c0a0", "43296666"),
        ArceusMap.ObsidianFieldlands when id is > 4 and <= 8 => coordinates = ("43de0ccd", "41e94aea", "43f97333"),
        ArceusMap.ObsidianFieldlands when id is > 8 and <= 12 => coordinates = ("44206666", "4207a9b0", "4430b333"),
        ArceusMap.ObsidianFieldlands when id is > 12 and <= 16 => coordinates = ("43154ccd", "42094701", "443e3333"),

        ArceusMap.CrimsonMirelands when id is <= 4 => coordinates = ("44553333", "42167c7d", "4433e000"),
        ArceusMap.CrimsonMirelands when id is > 4 and <= 8 => coordinates = ("43d9d99a", "4215872e", "445de666"),
        ArceusMap.CrimsonMirelands when id is > 8 and <= 12 => coordinates = ("44374666", "4214dc82", "4449eccd"),
        ArceusMap.CrimsonMirelands when id is > 12 and <= 16 => coordinates = ("444b8000", "42876336", "43e72666"),
        ArceusMap.CrimsonMirelands when id is > 16 and <= 20 => coordinates = ("444b8000", "42876336", "43e72666"),
        ArceusMap.CrimsonMirelands when id is > 20 and <= 24 => coordinates = ("438b3333", "4226454b", "44200666"),

        ArceusMap.CobaltCoastlands when id is <= 4 => coordinates = ("4367b333", "41da6666", "44014ccd"),
        ArceusMap.CobaltCoastlands when id is > 4 and <= 8 => coordinates = ("4373cccd", "41df9edd", "4439999a"),
        ArceusMap.CobaltCoastlands when id is > 8 and <= 12 => coordinates = ("441866e1", "41eba8a7", "445d8480"),
        ArceusMap.CobaltCoastlands when id is > 12 and <= 16 => coordinates = ("438ac000", "421cc140", "432e4ccd"),
        ArceusMap.CobaltCoastlands when id is > 16 and <= 20 => coordinates = ("43a12666", "4291b53c", "43b02666"),

        ArceusMap.CoronetHighlands when id is <= 4 => coordinates = ("44166000", "428c2f78", "442cd333"),
        ArceusMap.CoronetHighlands when id is > 4 and <= 8 => coordinates = ("44048002", "42ac8fe6", "444cb99a"),
        ArceusMap.CoronetHighlands when id is > 8 and <= 12 => coordinates = ("43e7aa3d", "430248a7", "43f90666"),
        ArceusMap.CoronetHighlands when id is > 12 and <= 16 => coordinates = ("439a41ec", "430803fe", "44159ba6"),
        ArceusMap.CoronetHighlands when id is > 16 and <= 20 => coordinates = ("43426666", "42eb01e5", "44211333"),

        ArceusMap.AlabasterIcelands when id is <= 4 => coordinates = ("4427f333", "4210645d", "4405c000"),
        ArceusMap.AlabasterIcelands when id is > 4 and <= 8 => coordinates = ("43e8599a", "420ca920", "43f7c000"),
        ArceusMap.AlabasterIcelands when id is > 8 and <= 12 => coordinates = ("440ea000", "41fe8750", "4435c000"),
        ArceusMap.AlabasterIcelands when id is > 12 and <= 16 => coordinates = ("43b6b333", "42073571", "4422999a"),
        ArceusMap.AlabasterIcelands when id is > 16 and <= 20 => coordinates = ("444f999a", "41f13987", "43bf6666"),
        ArceusMap.AlabasterIcelands when id is > 20 and <= 24 => coordinates = ("433ecccd", "4206563e", "4428199a"),
        _ => throw new NotImplementedException("Invalid location coordinates."),

    };
    private string GetDistortionSpeciesLocation(int id) => Settings.DistortionConditions.DistortionLocation switch
    {
        ArceusMap.ObsidianFieldlands when id is <= 4 => "Horseshoe Plains",
        ArceusMap.ObsidianFieldlands when id is > 4 and <= 8 => "Windswept Run",
        ArceusMap.ObsidianFieldlands when id is > 8 and <= 12 => "Nature's Pantry",
        ArceusMap.ObsidianFieldlands when id is > 12 and <= 16 => "Sandgem Flats",

        ArceusMap.CrimsonMirelands when id is <= 4 => "Droning Meadow",
        ArceusMap.CrimsonMirelands when id is > 4 and <= 8 => "Holm of Trials",
        ArceusMap.CrimsonMirelands when id is > 8 and <= 12 => "Location Unknown",
        ArceusMap.CrimsonMirelands when id is > 12 and <= 16 => "North of Ursa's Ring",
        ArceusMap.CrimsonMirelands when id is > 16 and <= 20 => "To the right of Bolderoll Slope",
        ArceusMap.CrimsonMirelands when id is > 20 and <= 24 => "Gapejaw Bog",

        ArceusMap.CobaltCoastlands when id is <= 4 => "Below Windbreak Stand",
        ArceusMap.CobaltCoastlands when id is > 4 and <= 8 => "Right of Crossing Slope",
        ArceusMap.CobaltCoastlands when id is > 8 and <= 12 => "Right of Bather's Lagoon",
        ArceusMap.CobaltCoastlands when id is > 12 and <= 16 => "Left of Islespy Shore",
        ArceusMap.CobaltCoastlands when id is > 16 and <= 20 => "Right of Windbreak Stand",

        ArceusMap.CoronetHighlands when id is <= 4 => "Between Sonorous Path and Celestica Trail",
        ArceusMap.CoronetHighlands when id is > 4 and <= 8 => "Ancient Quarry",
        ArceusMap.CoronetHighlands when id is > 8 and <= 12 => "North of Primeval Grotto",
        ArceusMap.CoronetHighlands when id is > 12 and <= 16 => "South of Sacred Plaza",
        ArceusMap.CoronetHighlands when id is > 16 and <= 20 => "Between Boulderoll Ravine and Stonetooth Rows",

        ArceusMap.AlabasterIcelands when id is <= 4 => "Between and to the right of Bonechill Wastes and Avalugg's Legacy",
        ArceusMap.AlabasterIcelands when id is > 4 and <= 8 => "Little left of Avalugg's Legacy",
        ArceusMap.AlabasterIcelands when id is > 8 and <= 12 => "Between Bonechill Wastes and Whiteout Valley",
        ArceusMap.AlabasterIcelands when id is > 12 and <= 16 => "To the right of Arena's Approach",
        ArceusMap.AlabasterIcelands when id is > 16 and <= 20 => "Heart's Crag",
        ArceusMap.AlabasterIcelands when id is > 20 and <= 24 => "North of Avalanche Slopes",
        _ => throw new NotImplementedException("Invalid location ID."),
    };

    private PA8 GetCommonDistortionSpecies(double encslot) => Settings.DistortionConditions.DistortionLocation switch
    {
        ArceusMap.ObsidianFieldlands when encslot is < 75 => new() { Species = (int)Species.Onix },
        ArceusMap.ObsidianFieldlands when encslot is > 75 and < 76 => new() { Species = (int)Species.Onix, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 76 and < 86 => new() { Species = (int)Species.Steelix },
        ArceusMap.ObsidianFieldlands when encslot is > 86 and < 87 => new() { Species = (int)Species.Steelix, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 87 and < 187 => new() { Species = (int)Species.Haunter },
        ArceusMap.ObsidianFieldlands when encslot is > 187 and < 188 => new() { Species = (int)Species.Haunter, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 188 and < 198 => new() { Species = (int)Species.Gengar },
        ArceusMap.ObsidianFieldlands when encslot is > 198 and < 199 => new() { Species = (int)Species.Gengar, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 199 and < 299 => new() { Species = (int)Species.Lickitung },
        ArceusMap.ObsidianFieldlands when encslot is > 299 and < 300 => new() { Species = (int)Species.Lickitung, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 300 and < 350 => new() { Species = (int)Species.Lickilicky },
        ArceusMap.ObsidianFieldlands when encslot is > 350 and < 351 => new() { Species = (int)Species.Lickilicky, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 351 and < 451 => new() { Species = (int)Species.Ursaring },
        ArceusMap.ObsidianFieldlands when encslot is > 451 and < 452 => new() { Species = (int)Species.Ursaring, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 452 and < 472 => new() { Species = (int)Species.Toxicroak },
        ArceusMap.ObsidianFieldlands when encslot is > 472 and < 473 => new() { Species = (int)Species.Toxicroak, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 473 and < 523 => new() { Species = (int)Species.Eevee },
        ArceusMap.ObsidianFieldlands when encslot is > 523 and < 524 => new() { Species = (int)Species.Eevee, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 524 and < 534 => new() { Species = (int)Species.Leafeon },
        ArceusMap.ObsidianFieldlands when encslot is > 534 and < 535 => new() { Species = (int)Species.Leafeon, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 535 and < 545 => new() { Species = (int)Species.Sylveon },
        ArceusMap.ObsidianFieldlands when encslot is >= 545 => new() { Species = (int)Species.Sylveon, IsAlpha = true },

        ArceusMap.CrimsonMirelands when encslot is < 100 => new() { Species = (int)Species.Floatzel },
        ArceusMap.CrimsonMirelands when encslot is > 100 and < 101 => new() { Species = (int)Species.Floatzel, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 101 and < 111 => new() { Species = (int)Species.Snorlax },
        ArceusMap.CrimsonMirelands when encslot is > 111 and < 112 => new() { Species = (int)Species.Snorlax, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 112 and < 212 => new() { Species = (int)Species.Drifblim },
        ArceusMap.CrimsonMirelands when encslot is > 212 and < 213 => new() { Species = (int)Species.Drifblim, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 213 and < 233 => new() { Species = (int)Species.Lopunny },
        ArceusMap.CrimsonMirelands when encslot is > 233 and < 234 => new() { Species = (int)Species.Lopunny, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 234 and < 334 => new() { Species = (int)Species.Luxio },
        ArceusMap.CrimsonMirelands when encslot is > 334 and < 335 => new() { Species = (int)Species.Luxio, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 335 and < 375 => new() { Species = (int)Species.Luxray },
        ArceusMap.CrimsonMirelands when encslot is > 375 and < 376 => new() { Species = (int)Species.Luxray, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 376 and < 406 => new() { Species = (int)Species.Heracross },
        ArceusMap.CrimsonMirelands when encslot is > 406 and < 407 => new() { Species = (int)Species.Heracross, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 407 and < 457 => new() { Species = (int)Species.Eevee },
        ArceusMap.CrimsonMirelands when encslot is > 457 and < 458 => new() { Species = (int)Species.Eevee, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 458 and < 468 => new() { Species = (int)Species.Umbreon },
        ArceusMap.CrimsonMirelands when encslot is > 468 and < 469 => new() { Species = (int)Species.Umbreon, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 469 and < 479 => new() { Species = (int)Species.Flareon },
        ArceusMap.CrimsonMirelands when encslot is >= 479 => new() { Species = (int)Species.Flareon, IsAlpha = true },

        ArceusMap.CobaltCoastlands when encslot is < 100 => new() { Species = (int)Species.Kadabra },
        ArceusMap.CobaltCoastlands when encslot is > 100 and < 101 => new() { Species = (int)Species.Kadabra, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 101 and < 111 => new() { Species = (int)Species.Alakazam },
        ArceusMap.CobaltCoastlands when encslot is > 111 and < 112 => new() { Species = (int)Species.Alakazam, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 112 and < 212 => new() { Species = (int)Species.Rhydon },
        ArceusMap.CobaltCoastlands when encslot is > 212 and < 213 => new() { Species = (int)Species.Rhydon, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 213 and < 223 => new() { Species = (int)Species.Rhyperior },
        ArceusMap.CobaltCoastlands when encslot is > 223 and < 224 => new() { Species = (int)Species.Rhyperior, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 224 and < 324 => new() { Species = (int)Species.Skuntank },
        ArceusMap.CobaltCoastlands when encslot is > 324 and < 325 => new() { Species = (int)Species.Skuntank, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 325 and < 425 => new() { Species = (int)Species.Carnivine },
        ArceusMap.CobaltCoastlands when encslot is > 425 and < 426 => new() { Species = (int)Species.Carnivine, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 426 and < 526 => new() { Species = (int)Species.MrMime },
        ArceusMap.CobaltCoastlands when encslot is > 526 and < 527 => new() { Species = (int)Species.MrMime, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 527 and < 557 => new() { Species = (int)Species.Eevee },
        ArceusMap.CobaltCoastlands when encslot is > 557 and < 558 => new() { Species = (int)Species.Eevee, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 558 and < 568 => new() { Species = (int)Species.Vaporeon },
        ArceusMap.CobaltCoastlands when encslot is > 568 and < 569 => new() { Species = (int)Species.Vaporeon, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 569 and < 579 => new() { Species = (int)Species.Flareon },
        ArceusMap.CobaltCoastlands when encslot is >= 579 => new() { Species = (int)Species.Flareon, IsAlpha = true },

        ArceusMap.CoronetHighlands when encslot is < 100 => new() { Species = (int)Species.Magmar },
        ArceusMap.CoronetHighlands when encslot is > 100 and < 101 => new() { Species = (int)Species.Magmar, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 101 and < 201 => new() { Species = (int)Species.Dusclops },
        ArceusMap.CoronetHighlands when encslot is > 201 and < 202 => new() { Species = (int)Species.Dusclops, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 202 and < 212 => new() { Species = (int)Species.Dusknoir },
        ArceusMap.CoronetHighlands when encslot is > 212 and < 213 => new() { Species = (int)Species.Dusknoir, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 213 and < 313 => new() { Species = (int)Species.Octillery },
        ArceusMap.CoronetHighlands when encslot is > 313 and < 314 => new() { Species = (int)Species.Octillery, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 314 and < 414 => new() { Species = (int)Species.Drapion },
        ArceusMap.CoronetHighlands when encslot is > 414 and < 415 => new() { Species = (int)Species.Drapion, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 415 and < 455 => new() { Species = (int)Species.Ambipom },
        ArceusMap.CoronetHighlands when encslot is > 455 and < 456 => new() { Species = (int)Species.Ambipom, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 456 and < 506 => new() { Species = (int)Species.Eevee },
        ArceusMap.CoronetHighlands when encslot is > 506 and < 507 => new() { Species = (int)Species.Eevee, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 507 and < 517 => new() { Species = (int)Species.Jolteon },
        ArceusMap.CoronetHighlands when encslot is > 517 and < 518 => new() { Species = (int)Species.Jolteon, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 518 and < 528 => new() { Species = (int)Species.Sylveon },
        ArceusMap.CoronetHighlands when encslot is >= 528 => new() { Species = (int)Species.Sylveon, IsAlpha = true },

        ArceusMap.AlabasterIcelands when encslot is < 100 => new() { Species = (int)Species.Electabuzz },
        ArceusMap.AlabasterIcelands when encslot is > 100 and < 102 => new() { Species = (int)Species.Electabuzz, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 102 and < 112 => new() { Species = (int)Species.Electivire },
        ArceusMap.AlabasterIcelands when encslot is > 112 and < 113 => new() { Species = (int)Species.Electivire, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 113 and < 173 => new() { Species = (int)Species.Pikachu },
        ArceusMap.AlabasterIcelands when encslot is > 173 and < 175 => new() { Species = (int)Species.Pikachu, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 175 and < 195 => new() { Species = (int)Species.Raichu },
        ArceusMap.AlabasterIcelands when encslot is > 195 and < 196 => new() { Species = (int)Species.Raichu, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 196 and < 296 => new() { Species = (int)Species.Sealeo },
        ArceusMap.AlabasterIcelands when encslot is > 296 and < 298 => new() { Species = (int)Species.Sealeo, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 298 and < 378 => new() { Species = (int)Species.Walrein },
        ArceusMap.AlabasterIcelands when encslot is > 378 and < 379 => new() { Species = (int)Species.Walrein, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 379 and < 459 => new() { Species = (int)Species.Rapidash },
        ArceusMap.AlabasterIcelands when encslot is > 459 and < 460 => new() { Species = (int)Species.Rapidash, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 460 and < 500 => new() { Species = (int)Species.Tangrowth },
        ArceusMap.AlabasterIcelands when encslot is > 500 and < 501 => new() { Species = (int)Species.Tangrowth, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 501 and < 601 => new() { Species = (int)Species.Scyther },
        ArceusMap.AlabasterIcelands when encslot is > 601 and < 602 => new() { Species = (int)Species.Scyther, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 602 and < 652 => new() { Species = (int)Species.Eevee },
        ArceusMap.AlabasterIcelands when encslot is > 652 and < 653 => new() { Species = (int)Species.Eevee, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 653 and < 663 => new() { Species = (int)Species.Glaceon },
        ArceusMap.AlabasterIcelands when encslot is > 663 and < 664 => new() { Species = (int)Species.Glaceon, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 664 and < 674 => new() { Species = (int)Species.Espeon },
        ArceusMap.AlabasterIcelands when encslot is >= 674 => new() { Species = (int)Species.Espeon, IsAlpha = true },
        _ => throw new NotImplementedException("Not a valid encounter slot."),
    };

    private PA8 GetDistortionSpecies(double encslot) => Settings.DistortionConditions.DistortionLocation switch
    {
        ArceusMap.ObsidianFieldlands when encslot is <= 100 => new() { Species = (int)Species.Sneasel },
        ArceusMap.ObsidianFieldlands when encslot is > 100 and < 101 => new() { Species = (int)Species.Sneasel, IsAlpha = true },
        ArceusMap.ObsidianFieldlands when encslot is > 101 and < 111 => new() { Species = (int)Species.Weavile },
        ArceusMap.ObsidianFieldlands when encslot is > 111 and < 112 => new() { Species = (int)Species.Weavile, IsAlpha = true },

        ArceusMap.CrimsonMirelands when encslot is <= 100 => new() { Species = (int)Species.Porygon },
        ArceusMap.CrimsonMirelands when encslot is > 100 and < 101 => new() { Species = (int)Species.Porygon, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 101 and < 111 => new() { Species = (int)Species.Porygon2 },
        ArceusMap.CrimsonMirelands when encslot is > 111 and < 112 => new() { Species = (int)Species.Porygon2, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 112 and < 117 => new() { Species = (int)Species.PorygonZ },
        ArceusMap.CrimsonMirelands when encslot is > 117 and < 118 => new() { Species = (int)Species.PorygonZ, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 118 and < 218 => new() { Species = (int)Species.Cyndaquil },
        ArceusMap.CrimsonMirelands when encslot is > 218 and < 219 => new() { Species = (int)Species.Cyndaquil, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 219 and < 269 => new() { Species = (int)Species.Quilava },
        ArceusMap.CrimsonMirelands when encslot is > 269 and < 270 => new() { Species = (int)Species.Quilava, IsAlpha = true },
        ArceusMap.CrimsonMirelands when encslot is > 270 and < 275 => new() { Species = (int)Species.Typhlosion },
        ArceusMap.CrimsonMirelands when encslot is >= 275 => new() { Species = (int)Species.Typhlosion, IsAlpha = true },

        ArceusMap.CobaltCoastlands when encslot is <= 100 => new() { Species = (int)Species.Magnemite },
        ArceusMap.CobaltCoastlands when encslot is > 100 and < 101 => new() { Species = (int)Species.Magnemite, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 101 and < 151 => new() { Species = (int)Species.Magneton },
        ArceusMap.CobaltCoastlands when encslot is > 151 and < 152 => new() { Species = (int)Species.Magneton, IsAlpha = true },
        ArceusMap.CobaltCoastlands when encslot is > 152 and < 162 => new() { Species = (int)Species.Magnezone },
        ArceusMap.CobaltCoastlands when encslot is >= 162 => new() { Species = (int)Species.Magnezone, IsAlpha = true },

        ArceusMap.CoronetHighlands when encslot is <= 100 => new() { Species = (int)Species.Cranidos },
        ArceusMap.CoronetHighlands when encslot is > 100 and < 101 => new() { Species = (int)Species.Cranidos, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 101 and < 111 => new() { Species = (int)Species.Rampardos },
        ArceusMap.CoronetHighlands when encslot is > 111 and < 112 => new() { Species = (int)Species.Rampardos, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 112 and < 212 => new() { Species = (int)Species.Shieldon },
        ArceusMap.CoronetHighlands when encslot is > 212 and < 213 => new() { Species = (int)Species.Shieldon, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 213 and < 223 => new() { Species = (int)Species.Bastiodon },
        ArceusMap.CoronetHighlands when encslot is > 223 and < 224 => new() { Species = (int)Species.Bastiodon, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 224 and < 324 => new() { Species = (int)Species.Rowlet },
        ArceusMap.CoronetHighlands when encslot is > 324 and < 325 => new() { Species = (int)Species.Rowlet, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 325 and < 375 => new() { Species = (int)Species.Dartrix },
        ArceusMap.CoronetHighlands when encslot is > 375 and < 376 => new() { Species = (int)Species.Dartrix, IsAlpha = true },
        ArceusMap.CoronetHighlands when encslot is > 376 and < 381 => new() { Species = (int)Species.Decidueye },
        ArceusMap.CoronetHighlands when encslot is > 381 and < 382 => new() { Species = (int)Species.Decidueye, IsAlpha = true },

        ArceusMap.AlabasterIcelands when encslot is <= 100 => new() { Species = (int)Species.Scizor },
        ArceusMap.AlabasterIcelands when encslot is > 100 and < 101 => new() { Species = (int)Species.Scizor, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 101 and < 201 => new() { Species = (int)Species.Oshawott },
        ArceusMap.AlabasterIcelands when encslot is > 201 and < 202 => new() { Species = (int)Species.Oshawott, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 202 and < 252 => new() { Species = (int)Species.Dewott },
        ArceusMap.AlabasterIcelands when encslot is > 252 and < 253 => new() { Species = (int)Species.Dewott, IsAlpha = true },
        ArceusMap.AlabasterIcelands when encslot is > 253 and < 258 => new() { Species = (int)Species.Samurott },
        ArceusMap.AlabasterIcelands when encslot is >= 258 => new() { Species = (int)Species.Samurott, IsAlpha = true },
        _ => throw new NotImplementedException("Not a valid encounter slot."),
    };

    private async Task DistortionSpammer(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Log("Whipping up a distortion...");
            // Activates distortions
            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x5280010A), MainNsoBase + ActivateDistortion, token).ConfigureAwait(false);
            await Task.Delay(0_500, token).ConfigureAwait(false);
            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7100052A), MainNsoBase + ActivateDistortion, token).ConfigureAwait(false);
            var delta = DateTime.Now;
            var cd = TimeSpan.FromMinutes(Settings.DistortionConditions.WaitTimeDistortion);
            Log($"Waiting {Settings.DistortionConditions.WaitTimeDistortion} minutes then starting the next one...");
            while (DateTime.Now - delta < cd && !token.IsCancellationRequested)
            {
                await Task.Delay(5_000, token).ConfigureAwait(false);
                Log($"Time Remaining: {cd - (DateTime.Now - delta)}");
            }

            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x5280010A), MainNsoBase + ActivateDistortion, token).ConfigureAwait(false);
            await Task.Delay(0_500, token).ConfigureAwait(false);
            await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0x7100052A), MainNsoBase + ActivateDistortion, token).ConfigureAwait(false);
            await Task.Delay(5_000, token).ConfigureAwait(false);
        }
    }

    private async Task TimeSeedAdvancer(CancellationToken token)
    {
        int success = 0;
        int heal = 0;
        string[] coords = { Settings.SpecialConditions.SpawnZoneX, Settings.SpecialConditions.SpawnZoneY, Settings.SpecialConditions.SpawnZoneZ };
        for (int a = 0; a < coords.Length; a++)
        {
            if (string.IsNullOrEmpty(coords[a]))
            {
                Log($"One of your coordinates is empty, please fill it accordingly!");
                return;
            }
        }
        OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
        // Activates invcincible trainer cheat so we don't faint from teleporting or a Pokemon attacking and infinite PP
        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer1, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer2, token).ConfigureAwait(false);

        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD28008AA), MainNsoBase + InfPP1, token).ConfigureAwait(false);
        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD28008A9), MainNsoBase + InfPP2, token).ConfigureAwait(false);

        GetDefaultCampCoords();
        while (!token.IsCancellationRequested)
        {
            string alpha = string.Empty;
            for (int adv = 0; adv < Settings.AlphaScanConditions.Advances; adv++)
            {
                Log($"Advancing {Settings.AlphaScanConditions.Advances - adv} times...");
                await Click(B, 1_000, token).ConfigureAwait(false);// Random B incase of button miss
                await Click(A, 1_000, token).ConfigureAwait(false);
                await TimeTest(token).ConfigureAwait(false);
                await Task.Delay(2_000, token).ConfigureAwait(false);
                if (Settings.AlphaScanConditions.SpawnIsStaticAlpha)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        await TimeTest(token).ConfigureAwait(false);
                        await Task.Delay(2_000, token).ConfigureAwait(false);
                    }
                }
                await TeleportToSpawnZone(token).ConfigureAwait(false);
                Log("Trying to enter battle!");

                await PressAndHold(ZL, 0_800, 0, token).ConfigureAwait(false);
                await Click(ZR, 1_000, token).ConfigureAwait(false);
                await ResetStick(token).ConfigureAwait(false);
                var overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                if (overworldcheck == 1)
                {
                    Log("Not in battle, trying again!");
                    int tries = 0;
                    while (overworldcheck == 1)
                    {
                        if (tries == 3)
                        {
                            Log("Tried 3 times, is the encounter present? Changing time to try again.");
                            await TimeTest(token).ConfigureAwait(false);
                            await TeleportToSpawnZone(token).ConfigureAwait(false);
                            adv--;
                            break;
                        }
                        await PressAndHold(ZL, 0_800, 0, token).ConfigureAwait(false);
                        await Click(ZR, 1_000, token).ConfigureAwait(false);
                        await ResetStick(token).ConfigureAwait(false); // reset
                        overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                        if (overworldcheck != 1)
                            break;

                        await SetStick(LEFT, 0, -5_000, 0_500, token).ConfigureAwait(false); // turn around check encounter
                        await ResetStick(token).ConfigureAwait(false); // reset
                        await Click(ZL, 1_000, token).ConfigureAwait(false);
                        overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                        if (overworldcheck != 1)
                            break;

                        await PressAndHold(ZL, 0_800, 0, token).ConfigureAwait(false);
                        await Click(ZR, 1_000, token).ConfigureAwait(false);
                        await ResetStick(token).ConfigureAwait(false); // reset
                        overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                        if (overworldcheck != 1)
                            break;

                        await SetStick(LEFT, 0, 5_000, 0_500, token).ConfigureAwait(false); // turn around check encounter
                        await ResetStick(token).ConfigureAwait(false); // reset
                        await Click(ZL, 1_000, token).ConfigureAwait(false);
                        tries++;
                    }
                }
                if (overworldcheck == 0)
                {
                    ulong ofs = await NewParsePointer(WildPokemonPtrLA, token).ConfigureAwait(false);
                    var pk = await ReadPokemon(ofs, 0x168, token).ConfigureAwait(false);
                    if (pk.Species > 0 && pk.Species < (int)Species.MAX_COUNT)
                    {
                        success++;
                        heal++;
                        var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                        if (pk.IsAlpha)
                            alpha = "Alpha - ";
                        if (pk.IsShiny)
                        {
                            Log($"In battle with {print}!");
                            string url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                            EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                            Settings.AddCompletedShinyAlphaFound();

                            if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
                                DumpPokemon(DumpSetting.DumpFolder, "advances", pk);

                            return;
                        }
                        Log($"Mashing A to knockout {alpha}{(Species)pk.Species}!");
                        while (overworldcheck != 1)
                        {
                            overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                            pk = await ReadPokemon(ofs, 0x168, token).ConfigureAwait(false);
                            for (int i = 0; i < 3; i++)
                                await Click(A, 0_500, token).ConfigureAwait(false);

                            if (overworldcheck == 1 || pk.Species <= 0 || pk.Species >= (int)Species.MAX_COUNT)
                                break;
                        }
                        Log($"Defeated {alpha}{(Species)pk.Species}! Returning to spawn point.");
                        alpha = string.Empty;
                    }
                }
                if (adv == Settings.AlphaScanConditions.Advances)
                    return;
                if (heal == 3 && Settings.AlphaScanConditions.HealInCamp)
                {
                    Log("Returning to camp to heal our party!");
                    await TeleportToCampZone(token).ConfigureAwait(false);
                    await Click(A, 0_800, token).ConfigureAwait(false);
                    var menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
                    //Log($"Menu check: {menucheck}");
                    if (menucheck == 42)
                        await Click(A, 0_800, token).ConfigureAwait(false);
                    while (menucheck == 4000 || menucheck == 0000)
                    {
                        Log("Wrong menu opened? Backing out now and trying to reposition.");
                        await Click(B, 1_500, token).ConfigureAwait(false);
                        await Reposition(token).ConfigureAwait(false);
                        await Click(B, 1_500, token).ConfigureAwait(false);
                        menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
                        if (menucheck == 4200)
                            break;
                    }
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    Log("Resting for a little while!");
                    await Click(A, 12_000, token).ConfigureAwait(false);
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    await TeleportToSpawnZone(token).ConfigureAwait(false);

                }
                if (success == 50)
                {
                    Log("Saving in case of a game crash!");
                    await ArceusSaveGame(token).ConfigureAwait(false);
                    success = 0;
                }
            }
        }
        Log("Advances reached! Shiny should be on the next respawn! Refer to the live map if unsure of frame!");
        await Click(HOME, 1_000, token).ConfigureAwait(false);
    }
    private async Task SeedAdvancer(CancellationToken token)
    {
        string[] coords = { Settings.SpecialConditions.CampZoneX, Settings.SpecialConditions.CampZoneY, Settings.SpecialConditions.CampZoneZ, Settings.SpecialConditions.SpawnZoneX, Settings.SpecialConditions.SpawnZoneY, Settings.SpecialConditions.SpawnZoneZ };
        for (int a = 0; a < coords.Length; a++)
        {
            if (string.IsNullOrEmpty(coords[a]))
            {
                Log($"One of your coordinates is empty, please fill it accordingly!");
                return;
            }
        }
        OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
        // Activates invcincible trainer cheat so we don't faint from teleporting or a Pokemon attacking
        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer1, token).ConfigureAwait(false);//invi
        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer2, token).ConfigureAwait(false);//invi

        while (!token.IsCancellationRequested)
        {
            GetDefaultCampCoords();
            string alpha = string.Empty;
            for (int adv = 0; adv < Settings.AlphaScanConditions.Advances; adv++)
            {
                Log($"Advancing {Settings.AlphaScanConditions.Advances - adv} times...");
                await Click(B, 1_000, token).ConfigureAwait(false);// Random B incase of button miss
                await Click(A, 1_000, token).ConfigureAwait(false);
                var menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
                Log($"test: {menucheck}");
                if (menucheck == 66)
                    await Click(A, 0_800, token).ConfigureAwait(false);
                while (menucheck == 64 || menucheck == 0)
                {
                    Log("Wrong menu opened? Backing out now and trying to reposition.");
                    await Click(B, 1_500, token).ConfigureAwait(false);
                    await Reposition(token).ConfigureAwait(false);
                    await Click(B, 1_500, token).ConfigureAwait(false);
                    menucheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesMainAsync(MenuOffset, 2, token).ConfigureAwait(false), 0);
                    if (menucheck == 66)
                        break;
                }
                if (adv % 2 == 0)
                    await Click(DDOWN, 0_500, token).ConfigureAwait(false);
                if (adv % 2 != 0)
                {
                    await Click(DUP, 0_500, token).ConfigureAwait(false);
                    await Click(DUP, 0_500, token).ConfigureAwait(false);
                }
                Log("Resting...");
                await Click(A, 12_000, token).ConfigureAwait(false);
                Log("Rested once!");
                await Click(A, 1_000, token).ConfigureAwait(false);
                if (Settings.AlphaScanConditions.SpawnIsStaticAlpha)
                {
                    // Rest a second time for Alphas
                    await SetStick(LEFT, 0, 10_000, 0_500, token).ConfigureAwait(false); // reset face forward
                    for (int i = 0; i < 2; i++)
                        await Click(A, 1_000, token).ConfigureAwait(false);
                    if (adv % 2 != 0)
                        await Click(DDOWN, 0_500, token).ConfigureAwait(false);
                    if (adv % 2 == 0)
                    {
                        for (int i = 0; i < 2; i++)
                            await Click(DUP, 0_500, token).ConfigureAwait(false);
                    }
                    Log("Resting...");
                    await Click(A, 12_000, token).ConfigureAwait(false);
                    Log("Rested twice!");
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    // Rest a third time for Alphas
                    await SetStick(LEFT, 0, 10_000, 0_500, token).ConfigureAwait(false); // reset face forward
                    await ResetStick(token).ConfigureAwait(false);
                    for (int i = 0; i < 2; i++)
                        await Click(A, 1_000, token).ConfigureAwait(false);
                    if (adv % 2 == 0)
                        await Click(DDOWN, 0_500, token).ConfigureAwait(false);
                    if (adv % 2 != 0)
                    {
                        await Click(DUP, 0_500, token).ConfigureAwait(false);
                        await Click(DUP, 0_500, token).ConfigureAwait(false);
                    }
                    Log("Resting...");
                    await Click(A, 12_000, token).ConfigureAwait(false);
                    Log("Rested thrice!");
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    //Night spawn only
                    if (Settings.AlphaScanConditions.NightSpawn)
                    {
                        await SetStick(LEFT, 0, 10_000, 0_500, token).ConfigureAwait(false); // reset face forward
                        await ResetStick(token).ConfigureAwait(false);
                        for (int i = 0; i < 2; i++)
                            await Click(A, 1_000, token).ConfigureAwait(false);
                        if (adv % 2 != 0)
                            await Click(DDOWN, 0_500, token).ConfigureAwait(false);
                        if (adv % 2 == 0)
                        {
                            await Click(DUP, 0_500, token).ConfigureAwait(false);
                            await Click(DUP, 0_500, token).ConfigureAwait(false);
                        }
                        Log("Resting...");
                        await Click(A, 12_000, token).ConfigureAwait(false);
                        Log("Rested for the night spawn!");
                        await Click(A, 1_000, token).ConfigureAwait(false);
                    }

                }
                await Click(DUP, 1_000, token).ConfigureAwait(false);
                await SetStick(LEFT, 0, 10_000, 0_500, token).ConfigureAwait(false); // reset face forward
                await ResetStick(token).ConfigureAwait(false);
                await TeleportToSpawnZone(token).ConfigureAwait(false);
                for (int i = 0; i < 2; i++)
                    await Click(B, 1_000, token).ConfigureAwait(false);

                if (Settings.AlphaScanConditions.IsSpawnInWater)
                    await Click(PLUS, 1_000, token).ConfigureAwait(false);

                Log("Trying to enter battle!");
                await PressAndHold(ZL, 1_000, 0, token).ConfigureAwait(false);
                await Click(ZR, 2_000, token).ConfigureAwait(false);
                await ResetStick(token).ConfigureAwait(false);
                var overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                if (overworldcheck == 1)
                {
                    Log("Not in battle, trying again!");
                    int tries = 0;
                    while (overworldcheck == 1)
                    {
                        if (tries == 3)
                        {
                            Log("Tried 3 times, is the encounter present? Going back to camp to try again.");
                            await TeleportToCampZone(token).ConfigureAwait(false);
                            break;
                        }
                        await PressAndHold(ZL, 0_800, 0, token).ConfigureAwait(false);
                        await Click(ZR, 1_000, token).ConfigureAwait(false);
                        await ResetStick(token).ConfigureAwait(false); // reset
                        overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                        if (overworldcheck != 1)
                            break;
                        await SetStick(LEFT, 0, -5_000, 0_500, token).ConfigureAwait(false); // turn around check encounter
                        await ResetStick(token).ConfigureAwait(false); // reset
                        await Click(ZL, 1_000, token).ConfigureAwait(false);
                        overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                        if (overworldcheck != 1)
                            break;
                        await PressAndHold(ZL, 0_800, 0, token).ConfigureAwait(false);
                        await Click(ZR, 1_000, token).ConfigureAwait(false);
                        await ResetStick(token).ConfigureAwait(false); // reset
                        overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                        if (overworldcheck != 1)
                            break;
                        await SetStick(LEFT, 0, 5_000, 0_500, token).ConfigureAwait(false); // turn around check encounter
                        await ResetStick(token).ConfigureAwait(false); // reset
                        await Click(ZL, 1_000, token).ConfigureAwait(false);
                        tries++;
                    }
                }
                if (overworldcheck == 0)
                {
                    ulong ofs = await NewParsePointer(WildPokemonPtrLA, token).ConfigureAwait(false);
                    var pk = await ReadPokemon(ofs, 0x168, token).ConfigureAwait(false);
                    if (pk != null)
                    {
                        var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                        if (pk.IsAlpha)
                            alpha = "Alpha - ";
                        if (pk.IsShiny)
                        {
                            Log($"In battle with {print}!");
                            string url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                            EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                            Settings.AddCompletedShinyAlphaFound();

                            if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
                                DumpPokemon(DumpSetting.DumpFolder, "advances", pk);

                            return;
                        }
                        Log($"Mashing A to knockout {alpha}{(Species)pk.Species}!");
                        while (overworldcheck != 1)
                        {
                            overworldcheck = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(OverworldOffset, 2, token).ConfigureAwait(false), 0);
                            await Click(A, 1_000, token).ConfigureAwait(false);
                        }
                        Log($"Defeated {alpha}{(Species)pk.Species}! Returning to spawn point.");
                        alpha = string.Empty;
                    }
                }
                Log($"Returning to camp...");
                if (Settings.AlphaScanConditions.IsSpawnInWater)
                    await Click(PLUS, 1_000, token).ConfigureAwait(false);

                await TeleportToCampZone(token);
                await SetStick(LEFT, 0, 5_000, 0_500, token).ConfigureAwait(false); // reset face forward
                await ResetStick(token).ConfigureAwait(false); // reset
                await ArceusSaveGame(token).ConfigureAwait(false);
            }
        }
        Log("Advances reached! Shiny should be on the next respawn! Refer to the live map if unsure of frame!");
        await Click(HOME, 1_000, token).ConfigureAwait(false);
    }

    private void GetDefaultCoords()
    {
        var mode = Settings.SpecialConditions.ScanLocation;
        switch (mode)
        {
            case ArceusMap.ObsidianFieldlands: coordinates = ("43AC18AC", "4242DEC9", "4305C37C"); break;
            case ArceusMap.CrimsonMirelands: coordinates = ("436208ED", "425FD612", "43DBCD87"); break;
            case ArceusMap.CoronetHighlands: coordinates = ("44636F1B", "420FDC78", "446A1DE5"); break;
            case ArceusMap.CobaltCoastlands: coordinates = ("425E5D04", "4237E63D", "44207689"); break;
            case ArceusMap.AlabasterIcelands: coordinates = ("440B8982", "41F37461", "4467EAF7"); break;
        }

        Settings.SpecialConditions.SpawnZoneX = "4408C60D";
        Settings.SpecialConditions.SpawnZoneY = "4270D7B3";
        Settings.SpecialConditions.SpawnZoneZ = "43E52E0D";

        Settings.SpecialConditions.CampZoneX = coordinates.Item1;
        Settings.SpecialConditions.CampZoneY = coordinates.Item2;
        Settings.SpecialConditions.CampZoneZ = coordinates.Item3;
    }

    private void GetDefaultCampCoords()
    {
        var mode = Settings.SpecialConditions.ScanLocation;
        switch (mode)
        {
            case ArceusMap.ObsidianFieldlands: coordinates = ("43B7F23A", "424FF99B", "4308306A"); break;
            case ArceusMap.CrimsonMirelands: coordinates = ("43751CD5", "425E7B13", "43D92BA9"); break;
            case ArceusMap.CoronetHighlands: coordinates = ("445E8412", "4211C885", "4466973A"); break;
            case ArceusMap.CobaltCoastlands: coordinates = ("4291B987", "4234AEB4", "441BF96B"); break;
            case ArceusMap.AlabasterIcelands: coordinates = ("4404BA24", "41F91B54", "446417D3"); break;
        }

        Settings.SpecialConditions.CampZoneX = coordinates.Item1;
        Settings.SpecialConditions.CampZoneY = coordinates.Item2;
        Settings.SpecialConditions.CampZoneZ = coordinates.Item3;
    }

    private async Task ScanForSpawners(CancellationToken token)
    {
        Settings.AlphaScanConditions.StopOnMatch = false;
        int attempts = 1;
        for (int i = 0; i < 2; i++)
            await Click(B, 0_500, token).ConfigureAwait(false);
        Log("Searching for a match!");
        GetDefaultCoords();
        if (Settings.SpecialConditions.RunToProfessor == false && Settings.SpecialConditions.TypeOfLegend is not LegendsOnMap.Heatran or LegendsOnMap.Azelf or LegendsOnMap.Mesprit or LegendsOnMap.Uxie)
            await TeleportToCampZone(token);
        while (!token.IsCancellationRequested)
        {
            Log($"Search #{attempts}");
            if (Settings.BotType == ArceusMode.StaticAlphaScan)
                await SpawnerScan(token).ConfigureAwait(false);
            if (Settings.BotType == ArceusMode.GenieScanner)
                await GenieScan(token).ConfigureAwait(false);
            if (Settings.BotType == ArceusMode.OtherLegendReset)
            {
                if (Settings.SpecialConditions.TypeOfLegend is LegendsOnMap.Heatran or LegendsOnMap.Azelf or LegendsOnMap.Mesprit or LegendsOnMap.Uxie)
                {
                    await Click(A, 0_800, token).ConfigureAwait(false);

                    while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                        await Click(A, 0_800, token).ConfigureAwait(false);

                    await Task.Delay(2_000, token).ConfigureAwait(false);
                }
                await OtherLegendsReset(token).ConfigureAwait(false);
            }
            if (Settings.AlphaScanConditions.StopOnMatch)
            {
                Log($"{Hub.Config.StopConditions.MatchFoundEchoMention} a match has been found!");
                await Click(HOME, 0_700, token).ConfigureAwait(false);
                if (Settings.ContinueAfterMatch == ContinueAfterMatch.PauseWaitAcknowledge)
                {
                    await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer1, token).ConfigureAwait(false);
                    await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer2, token).ConfigureAwait(false);

                    IsWaiting = true;
                    while (IsWaiting)
                        await Task.Delay(1_000, token).ConfigureAwait(false);

                    Settings.AlphaScanConditions.StopOnMatch = false;
                    await Click(HOME, 1_000, token).ConfigureAwait(false);
                }
                else
                    return;
            }
            if (Settings.BotType == ArceusMode.OtherLegendReset && Settings.SpecialConditions.TypeOfLegend is LegendsOnMap.Azelf or LegendsOnMap.Heatran or LegendsOnMap.Mesprit or LegendsOnMap.Uxie)
            {
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                await StartGame(Hub.Config, token).ConfigureAwait(false);
                continue;
            }
            if (Settings.SpecialConditions.RunToProfessor == false)
            {
                await TeleportToCampZone(token);
                await SetStick(LEFT, -30_000, 0, 1_000, token).ConfigureAwait(false); // reset face forward
                await ResetStick(token).ConfigureAwait(false); // reset
                await Click(A, 1_000, token).ConfigureAwait(false);
                await Click(A, 1_000, token).ConfigureAwait(false);

                while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                    await Click(A, 1_000, token).ConfigureAwait(false);

                await TeleportToSpawnZone(token);
                await SetStick(LEFT, 0, -30_000, 1_000, token).ConfigureAwait(false); // reset face downward
                await ResetStick(token).ConfigureAwait(false); // reset
                for (int i = 0; i < 2; i++)
                    await Click(A, 1_000, token).ConfigureAwait(false);

                while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                    await Click(A, 1_000, token).ConfigureAwait(false);
            }
            if (Settings.SpecialConditions.RunToProfessor == true)
            {
                var mode = Settings.SpecialConditions.ScanLocation;
                switch (mode)
                {
                    case ArceusMap.ObsidianFieldlands: await SetStick(LEFT, 30_000, 32767, 1_000, token).ConfigureAwait(false); break;
                    case ArceusMap.CrimsonMirelands:
                        await SetStick(RIGHT, 19_500, 5000, 1_000, token).ConfigureAwait(false);
                        await SetStick(RIGHT, 0, 0, 0_500, token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, 4000, 0, token).ConfigureAwait(false);
                        await ResetStick(token).ConfigureAwait(false); // reset
                        await Click(Y, 1_500, token).ConfigureAwait(false); break;
                    case ArceusMap.CobaltCoastlands:
                        await SetStick(RIGHT, 20_000, 5000, 1_000, token).ConfigureAwait(false);
                        await SetStick(RIGHT, 0, 0, 0_500, token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, 32767, 1_000, token).ConfigureAwait(false); break;
                    case ArceusMap.CoronetHighlands:
                        await SetStick(RIGHT, 19_000, 5000, 1_000, token).ConfigureAwait(false);
                        await SetStick(RIGHT, 0, 0, 0_500, token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, 32767, 1_200, token).ConfigureAwait(false); break;
                    case ArceusMap.AlabasterIcelands:
                        await SetStick(RIGHT, 19_000, 5000, 1_000, token).ConfigureAwait(false);
                        await SetStick(RIGHT, 0, 0, 0_500, token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, 32767, 0_800, token).ConfigureAwait(false); break;
                }
                await ResetStick(token).ConfigureAwait(false); // reset

                for (int i = 0; i < 3; i++)
                    await Click(A, 1_000, token).ConfigureAwait(false);
                await Click(DDOWN, 1_000, token).ConfigureAwait(false);

                while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                    await Click(A, 1_000, token).ConfigureAwait(false);

                await SetStick(LEFT, 0, -30_000, 1_000, token).ConfigureAwait(false); // reset face downward
                await ResetStick(token).ConfigureAwait(false); // reset

                await Click(Y, 1_800, token).ConfigureAwait(false);
                while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                    await Click(A, 1_000, token).ConfigureAwait(false);
            }
            attempts++;
        }
    }

    private async Task TeleportToCampZone(CancellationToken token)
    {
        var ofs = await NewParsePointer(PlayerCoordPtrLA, token).ConfigureAwait(false);
        uint coordX1 = uint.Parse(Settings.SpecialConditions.CampZoneX, NumberStyles.AllowHexSpecifier);
        byte[] X1 = BitConverter.GetBytes(coordX1);
        uint coordY1 = uint.Parse(Settings.SpecialConditions.CampZoneY, NumberStyles.AllowHexSpecifier);
        byte[] Y1 = BitConverter.GetBytes(coordY1);
        uint coordZ1 = uint.Parse(Settings.SpecialConditions.CampZoneZ, NumberStyles.AllowHexSpecifier);
        byte[] Z1 = BitConverter.GetBytes(coordZ1);

        X1 = X1.Concat(Y1).Concat(Z1).ToArray();
        await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);

        await Task.Delay(Settings.SpecialConditions.WaitMsBetweenTeleports, token).ConfigureAwait(false);
    }

    private async Task TeleportToSpawnZone(CancellationToken token)
    {
        var ofs = await NewParsePointer(PlayerCoordPtrLA, token).ConfigureAwait(false);
        uint coordX1 = uint.Parse(Settings.SpecialConditions.SpawnZoneX, NumberStyles.AllowHexSpecifier);
        byte[] X1 = BitConverter.GetBytes(coordX1);
        uint coordY1 = uint.Parse(Settings.SpecialConditions.SpawnZoneY, NumberStyles.AllowHexSpecifier);
        byte[] Y1 = BitConverter.GetBytes(coordY1);
        uint coordZ1 = uint.Parse(Settings.SpecialConditions.SpawnZoneZ, NumberStyles.AllowHexSpecifier);
        byte[] Z1 = BitConverter.GetBytes(coordZ1);

        X1 = X1.Concat(Y1).Concat(Z1).ToArray();
        await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);

        await Task.Delay(Settings.SpecialConditions.WaitMsBetweenTeleports, token).ConfigureAwait(false);
    }

    private async Task TeleportToMMOGroupZone(CancellationToken token)
    {
        var ofs = await NewParsePointer(PlayerCoordPtrLA, token).ConfigureAwait(false);
        uint coordX1 = uint.Parse(coordinates.Item1, NumberStyles.AllowHexSpecifier);
        byte[] X1 = BitConverter.GetBytes(coordX1);
        uint coordY1 = uint.Parse(coordinates.Item2, NumberStyles.AllowHexSpecifier);
        byte[] Y1 = BitConverter.GetBytes(coordY1);
        uint coordZ1 = uint.Parse(coordinates.Item3, NumberStyles.AllowHexSpecifier);
        byte[] Z1 = BitConverter.GetBytes(coordZ1);

        X1 = X1.Concat(Y1).Concat(Z1).ToArray();
        await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);

        await Task.Delay(Settings.SpecialConditions.WaitMsBetweenTeleports, token).ConfigureAwait(false);
    }

    private async Task TeleportToDistortionZone(CancellationToken token)
    {
        var ofs = await NewParsePointer(PlayerCoordPtrLA, token).ConfigureAwait(false);
        uint coordX1 = uint.Parse(coordinates.Item1, NumberStyles.AllowHexSpecifier);
        byte[] X1 = BitConverter.GetBytes(coordX1);
        uint coordY1 = uint.Parse(coordinates.Item2, NumberStyles.AllowHexSpecifier);
        byte[] Y1 = BitConverter.GetBytes(coordY1);
        uint coordZ1 = uint.Parse(coordinates.Item3, NumberStyles.AllowHexSpecifier);
        byte[] Z1 = BitConverter.GetBytes(coordZ1);

        X1 = X1.Concat(Y1).Concat(Z1).ToArray();
        await SwitchConnection.WriteBytesAbsoluteAsync(X1, ofs, token).ConfigureAwait(false);

        await Task.Delay(Settings.SpecialConditions.WaitMsBetweenTeleports, token).ConfigureAwait(false);
    }

    private async Task GenieScan(CancellationToken token)
    {
        await Task.Delay(0_500, token).ConfigureAwait(false);
        int groupID = Settings.SpecialConditions.ScanLocation switch
        {
            ArceusMap.ObsidianFieldlands => 310, //305 lando
            ArceusMap.CrimsonMirelands => 402, //398 enamo
            ArceusMap.CobaltCoastlands => 412, //408 thundo
            ArceusMap.AlabasterIcelands => 302, //297 torn
            _ => throw new NotImplementedException("Invalid scan location."),
        };
        string spawnerID = Settings.SpecialConditions.ScanLocation switch
        {
            ArceusMap.ObsidianFieldlands => "88AF9BCFDD5FCD8F",
            ArceusMap.CrimsonMirelands => "A468ADF5964CCE65",
            ArceusMap.AlabasterIcelands => "B8B7D33AB95F7A11",
            ArceusMap.CobaltCoastlands => "08398514506FBE25",
            _ => throw new NotImplementedException("Invalid scan location."),
        };

        bool isSpawner = false;
        while (!isSpawner)
        {
            var test = new long[] { 0x42a6ee0, 0x330, 0x70 + groupID * 0x440 + 0x20 + 0x3F0 };
            var stest = await SwitchConnection.PointerAll(test, token).ConfigureAwait(false);
            var bytes = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(stest, 8, token).ConfigureAwait(false), 0);
            isSpawner = $"{bytes:X16}".Equals(spawnerID);
            if (!isSpawner)
            {
                groupID--;
                Log($"Bad SpawnerID {bytes:X16}\nTrying again with SpawnerID: {groupID}.");
                continue;
            }
            Log($"Checking SpawnerID: {groupID}...");
            var SpawnerOffpoint = new long[] { 0x42a6ee0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var SpawnerOff = await SwitchConnection.PointerAll(SpawnerOffpoint, token).ConfigureAwait(false);
            var GeneratorSeed = await SwitchConnection.ReadBytesAbsoluteAsync(SpawnerOff, 8, token).ConfigureAwait(false);

            Log($"SpawnerID {bytes:X16} - Generator Seed: {BitConverter.ToString(GeneratorSeed).Replace("-", "")}");
            var group_seed = (BitConverter.ToUInt64(GeneratorSeed, 0) - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            GenerateNextShiny(0, group_seed);
        }
    }

    private async Task OtherLegendsReset(CancellationToken token)
    {
        await Task.Delay(0_500, token).ConfigureAwait(false);
        int groupID = Settings.SpecialConditions.ScanLocation switch
        {
            ArceusMap.ObsidianFieldlands => 310,
            ArceusMap.CrimsonMirelands => 402,
            ArceusMap.CobaltCoastlands => 412,
            ArceusMap.AlabasterIcelands => 302,
            ArceusMap.CoronetHighlands => 265,
            _ => throw new NotImplementedException("Invalid scan location."),
        };
        string spawnerID = Settings.SpecialConditions.TypeOfLegend switch
        {
            LegendsOnMap.Shaymin => "CDE0EAB0B0192256",
            LegendsOnMap.Darkrai => "14044B0D5D36E4B6",
            LegendsOnMap.Cresselia => "80E30B44446F80BE",
            LegendsOnMap.Heatran => "DA1BB574FA53D58C",
            LegendsOnMap.Mesprit => "375CE0719E93B5CF",
            LegendsOnMap.Uxie => "5A67A8230FAF4B95",
            LegendsOnMap.Azelf => "33DB175E98E892F7",
            _ => throw new NotImplementedException("Invalid scan location."),
        };

        bool isSpawner = false;
        while (!isSpawner)
        {
            var test = new long[] { 0x42a6ee0, 0x330, 0x70 + groupID * 0x440 + 0x20 + 0x3F0 };
            var stest = await SwitchConnection.PointerAll(test, token).ConfigureAwait(false);
            var bytes = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(stest, 8, token).ConfigureAwait(false), 0);
            isSpawner = $"{bytes:X16}".Equals(spawnerID);
            if (!isSpawner)
            {
                groupID--;
                Log($"Bad SpawnerID {bytes:X16}\nTrying again with SpawnerID: {groupID}.");
                continue;
            }
            Log($"Checking SpawnerID: {groupID}...");
            var SpawnerOffpoint = new long[] { 0x42a6ee0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var SpawnerOff = await SwitchConnection.PointerAll(SpawnerOffpoint, token).ConfigureAwait(false);
            var GeneratorSeed = await SwitchConnection.ReadBytesAbsoluteAsync(SpawnerOff, 8, token).ConfigureAwait(false);

            Log($"SpawnerID {bytes:X16} - Generator Seed: {BitConverter.ToString(GeneratorSeed).Replace("-", "")}");
            var group_seed = (BitConverter.ToUInt64(GeneratorSeed, 0) - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            GenerateNextShiny(0, group_seed);
        }
    }

    private async Task ManaphyReset(CancellationToken token)
    {
        int count = 1;
        Log($"Starting Manaphy Scanner...");
        while (!token.IsCancellationRequested)
        {
            Log($"Search #{count}");
            await Click(A, 0_800, token).ConfigureAwait(false);

            while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                await Click(A, 0_800, token).ConfigureAwait(false);

            await Task.Delay(2_000, token).ConfigureAwait(false);

            for (int i = 409; i < 413; i++)
            {
                string spawnerID = i switch
                {
                    409 => "97D85B3BB18FD0AB",
                    410 => "7C9E6D810B343908",
                    411 => "162F766DB48FC6D8",
                    412 => "E1CBACBBA2D63665",
                    _ => throw new NotImplementedException("Invalid spawner."),
                };
                var test = new long[] { 0x42a6ee0, 0x330, 0x70 + i * 0x440 + 0x20 + 0x3F0 };
                var stest = await SwitchConnection.PointerAll(test, token).ConfigureAwait(false);
                var bytes = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(stest, 8, token).ConfigureAwait(false), 0);
                bool isSpawner = $"{bytes:X16}".Equals(spawnerID);
                if (!isSpawner)
                {
                    i--;
                    Log($"Bad SpawnerID {bytes:X16}\nTrying again with SpawnerID: {i}.");
                    continue;
                }
                Log($"Checking SpawnerID: {i}...");
                var SpawnerOffpoint = new long[] { 0x42a6ee0, 0x330, 0x70 + i * 0x440 + 0x20 };
                var SpawnerOff = await SwitchConnection.PointerAll(SpawnerOffpoint, token).ConfigureAwait(false);
                var GeneratorSeed = await SwitchConnection.ReadBytesAbsoluteAsync(SpawnerOff, 8, token).ConfigureAwait(false);

                Log($"SpawnerID {bytes:X16} - Generator Seed: {BitConverter.ToString(GeneratorSeed).Replace("-", "")}");
                var group_seed = (BitConverter.ToUInt64(GeneratorSeed, 0) - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
                if (i == 409)
                    GenerateCaveFamily(true, group_seed);
                else
                    GenerateCaveFamily(false, group_seed);

                if (Settings.AlphaScanConditions.StopOnMatch)
                {
                    Log($"{Hub.Config.StopConditions.MatchFoundEchoMention} a match has been found!");
                    await Click(HOME, 0_700, token).ConfigureAwait(false);
                    if (Settings.ContinueAfterMatch == ContinueAfterMatch.PauseWaitAcknowledge)
                    {
                        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer1, token).ConfigureAwait(false);
                        await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer2, token).ConfigureAwait(false);

                        IsWaiting = true;
                        while (IsWaiting)
                            await Task.Delay(1_000, token).ConfigureAwait(false);

                        Settings.AlphaScanConditions.StopOnMatch = false;
                        await Click(HOME, 1_000, token).ConfigureAwait(false);
                    }
                    else
                        return;
                }
            }
            count++;

            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token).ConfigureAwait(false);
        }
    }

    private async Task SpawnerScan(CancellationToken token)
    {
        await Task.Delay(0_500, token).ConfigureAwait(false);
        Log($"Starting Alpha Scanner for {Settings.SpecialConditions.ScanLocation}...");
        int alphacount = Settings.SpecialConditions.ScanLocation switch
        {
            ArceusMap.ObsidianFieldlands or ArceusMap.CobaltCoastlands => 18,
            ArceusMap.CrimsonMirelands => 19,
            ArceusMap.CoronetHighlands => 15,
            ArceusMap.AlabasterIcelands => 13,
            _ => throw new NotImplementedException("Invalid scan location."),
        };

        for (int i = 0; i < alphacount; i++)
        {
            var SpawnerOffpoint = new long[] { 0x42a6ee0, 0x330, 0x70 + i * 0x440 + 0x20 };
            var SpawnerOff = await SwitchConnection.PointerAll(SpawnerOffpoint, token).ConfigureAwait(false);
            var GeneratorSeed = await SwitchConnection.ReadBytesAbsoluteAsync(SpawnerOff, 8, token).ConfigureAwait(false);
            var group_seed = (BitConverter.ToUInt64(GeneratorSeed, 0) - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            ResultsUtil.Log($"Generator Seed: {BitConverter.ToString(GeneratorSeed).Replace("-", "")}\nGroup Seed: {string.Format("0x{0:X}", group_seed)}", "");
            GenerateNextShiny(i, group_seed);
        }
    }

    private (List<PA8>, List<PA8>) ReadMMOSeed(PokedexSaveData dex, int totalspawn, ulong group_seed, int bonuscount, ulong encslot, ulong bonusencslot)
    {
        List<PA8> monlist = new();
        List<PA8> bonuslist = new();
        var groupseed = group_seed;
        var mainrng = new Xoroshiro128Plus(groupseed);
        var sum = GrabEncounterSum(encslot, bonusencslot);

        for (int i = 0; i < 4; i++)
        {
            var spawner_seed = mainrng.Next();
            var spawner_rng = new Xoroshiro128Plus(spawner_seed);
            var encounter_slot = spawner_rng.Next() / Math.Pow(2, 64) * sum.Item1;
            var fixedseed = spawner_rng.Next();
            mainrng.Next();
            var poke = GrabMMOSpecies(encounter_slot, encslot);
            var gt = PersonalTable.LA.GetFormEntry(poke.Item1.Species, poke.Item1.Form).Gender;

            var (perfect, complete) = CheckForPerfectComplete(HasCharm, dex, poke.Item1.Species);
            var rolls = 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(SpawnType.MMO - 7);

            var gen = GenerateFromSeed(fixedseed, rolls, poke.Item2, gt);
            poke.Item1.EncryptionConstant = gen.EC;
            poke.Item1.PID = gen.PID;
            int[] pkIVList = gen.IVs;
            poke.Item1.SetIVs(pkIVList);
            (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);
            poke.Item1.IVs = pkIVList;
            poke.Item1.Nature = (int)gen.Item8;
            poke.Item1.Gender = gen.gender;

            if (gen.shiny)
                CommonEdits.SetShiny(poke.Item1, Shiny.Always);

            monlist.Add(poke.Item1);
        }

        groupseed = mainrng.Next();
        mainrng = new Xoroshiro128Plus(groupseed);
        var respawnrng = new Xoroshiro128Plus(groupseed);
        for (int r = 0; r < totalspawn - 4; r++)
        {
            var spawner_seed = respawnrng.Next();
            respawnrng.Next();
            respawnrng = new Xoroshiro128Plus(respawnrng.Next());
            var fixed_rng = new Xoroshiro128Plus(spawner_seed);
            var encounter_slot = fixed_rng.Next() / Math.Pow(2, 64) * sum.Item1;
            var fixed_seed = fixed_rng.Next();

            var poke = GrabMMOSpecies(encounter_slot, encslot);
            var gt = PersonalTable.LA.GetFormEntry(poke.Item1.Species, poke.Item1.Form).Gender;

            var (perfect, complete) = CheckForPerfectComplete(HasCharm, dex, poke.Item1.Species);
            var rolls = 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(SpawnType.MMO - 7);

            var gen = GenerateFromSeed(fixed_seed, rolls, poke.Item2, gt);
            int[] pkIVList = gen.IVs;
            poke.Item1.SetIVs(pkIVList);
            (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);
            poke.Item1.IVs = pkIVList;
            poke.Item1.EncryptionConstant = gen.EC;
            poke.Item1.PID = gen.PID;
            poke.Item1.Nature = (int)gen.Item8;
            poke.Item1.Gender = gen.gender;

            if (gen.shiny)
                CommonEdits.SetShiny(poke.Item1, Shiny.Always);

            monlist.Add(poke.Item1);
        }

        //Bonus round
        if (sum.Item2 != 0)
        {
            var bonus_seed = respawnrng.Next() - 0x82A2B175229D6A5B & 0xFFFFFFFFFFFFFFFF;
            mainrng = new Xoroshiro128Plus(bonus_seed);
            for (int i = 0; i < 4; i++)
            {
                var spawner_seed = mainrng.Next();
                var spawner_rng = new Xoroshiro128Plus(spawner_seed);
                var encounter_slot = spawner_rng.Next() / Math.Pow(2, 64) * sum.Item2;
                var fixedseed = spawner_rng.Next();
                mainrng.Next();

                var poke = GrabMMOSpecies(encounter_slot, bonusencslot);
                var gt = PersonalTable.LA.GetFormEntry(poke.Item1.Species, poke.Item1.Form).Gender;

                var (perfect, complete) = CheckForPerfectComplete(HasCharm, dex, poke.Item1.Species);
                var rolls = 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(SpawnType.MMO - 7);

                var gen = GenerateFromSeed(fixedseed, rolls, poke.Item2, gt);
                int[] pkIVList = gen.IVs;
                poke.Item1.SetIVs(pkIVList);
                (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);
                poke.Item1.IVs = pkIVList;
                poke.Item1.EncryptionConstant = gen.EC;
                poke.Item1.PID = gen.PID;
                poke.Item1.Nature = (int)gen.Item8;
                poke.Item1.Gender = gen.gender;

                if (gen.shiny)
                    CommonEdits.SetShiny(poke.Item1, Shiny.Always);

                bonuslist.Add(poke.Item1);
            }

            bonus_seed = mainrng.Next();
            mainrng = new Xoroshiro128Plus(bonus_seed);
            var bonusrng = new Xoroshiro128Plus(bonus_seed);
            for (int r = 0; r < bonuscount - 4; r++)
            {
                var bonusspawner_seed = bonusrng.Next();
                bonusrng.Next();
                bonusrng = new Xoroshiro128Plus(bonusrng.Next());
                var fixed_rng = new Xoroshiro128Plus(bonusspawner_seed);
                var encounter_slot = fixed_rng.Next() / Math.Pow(2, 64) * sum.Item2;
                var fixed_seed = fixed_rng.Next();

                var poke = GrabMMOSpecies(encounter_slot, bonusencslot);
                var gt = PersonalTable.LA.GetFormEntry(poke.Item1.Species, poke.Item1.Form).Gender;

                var (perfect, complete) = CheckForPerfectComplete(HasCharm, dex, poke.Item1.Species);
                var rolls = 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(SpawnType.MMO - 7);

                var gen = GenerateFromSeed(fixed_seed, rolls, poke.Item2, gt);
                poke.Item1.EncryptionConstant = gen.EC;
                poke.Item1.PID = gen.PID;
                int[] pkIVList = gen.IVs;
                poke.Item1.SetIVs(pkIVList);
                (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);
                poke.Item1.IVs = pkIVList;
                poke.Item1.Nature = (int)gen.Item8;
                poke.Item1.Gender = gen.gender;

                if (gen.shiny)
                    CommonEdits.SetShiny(poke.Item1, Shiny.Always);

                bonuslist.Add(poke.Item1);
            }
        }
        return (monlist, bonuslist);
    }

    private List<PA8> ReadOutbreakSeed(PokedexSaveData dex, Species species, int totalspawn, ulong groupseed)
    {
        List<PA8> monlist = new();
        PA8 pk = new() { Species = (ushort)species };
        int givs = 0;
        var mainrng = new Xoroshiro128Plus(groupseed);
        for (int i = 0; i < 4; i++)
        {
            var spawner_seed = mainrng.Next();
            mainrng.Next();
            var spawner_rng = new Xoroshiro128Plus(spawner_seed);
            var slot = spawner_rng.Next() / Math.Pow(2, 64) * 101;
            var alpha = slot >= 100;
            var fixedseed = spawner_rng.Next();

            if (alpha)
                givs = 3;
            var gt = PersonalTable.LA.GetFormEntry(pk.Species, pk.Form).Gender;

            var (perfect, complete) = CheckForPerfectComplete(HasCharm, dex, pk.Species);
            var rolls = 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(SpawnType.Outbreak - 7);

            var gen = GenerateFromSeed(fixedseed, rolls, givs, gt);
            pk.Species = (ushort)species;
            pk.EncryptionConstant = gen.EC;
            pk.PID = gen.PID;
            int[] pkIVList = gen.IVs;
            pk.SetIVs(pkIVList);
            (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);
            pk.IVs = pkIVList;
            pk.Nature = gen.Item8;
            pk.Gender = gen.gender;
            pk.IsAlpha = alpha;

            if (gen.shiny)
                CheckEmbed(pk, "", "", CancellationToken.None).ConfigureAwait(false);

            monlist.Add(pk);
            givs = 0;
            pk = new PA8();
        }

        groupseed = mainrng.Next();
        mainrng = new Xoroshiro128Plus(groupseed);
        var respawnrng = new Xoroshiro128Plus(groupseed);
        for (int r = 0; r < totalspawn - 4; r++)
        {
            var spawner_seed = respawnrng.Next();
            respawnrng.Next();
            respawnrng = new Xoroshiro128Plus(respawnrng.Next());
            var fixed_rng = new Xoroshiro128Plus(spawner_seed);
            var slot = fixed_rng.Next() / Math.Pow(2, 64) * 101;
            var alpha = slot >= 100;
            var fixed_seed = fixed_rng.Next();

            if (alpha)
                givs = 3;
            var gt = PersonalTable.LA.GetFormEntry(pk.Species, pk.Form).Gender;

            var (perfect, complete) = CheckForPerfectComplete(HasCharm, dex, pk.Species);
            var rolls = 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(SpawnType.Outbreak - 7);

            var gen = GenerateFromSeed(fixed_seed, rolls, givs, gt);
            pk.Species = (ushort)species;
            pk.EncryptionConstant = gen.EC;
            pk.PID = gen.PID;
            int[] pkIVList = gen.IVs;
            pk.SetIVs(pkIVList);
            (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);
            pk.IVs = pkIVList;
            pk.Nature = (int)gen.Item8;
            pk.IsAlpha = alpha;
            pk.Gender = gen.gender;

            if (gen.shiny)
                CheckEmbed(pk, "", "", CancellationToken.None).ConfigureAwait(false);

            monlist.Add(pk);
            givs = 0;
            pk = new PA8();
        }

        return monlist;
    }

    private (PA8 match, bool shiny, string log) ReadDistortionSeed(PokedexSaveData dex, int id, ulong group_seed, int encslotsum, int common_sum)
    {
        string logs = string.Empty;
        PA8 pk = new();
        var sum_to_use = 0;
        if (id == 0 || id == 4 || id == 8 || id == 12 || id == 16 || id == 20)
            sum_to_use = common_sum;
        else if (id != 0 && id != 4 && id != 8 && id != 12 && id != 16 && id != 20)
            sum_to_use = encslotsum;

        var groupseed = group_seed;
        var mainrng = new Xoroshiro128Plus(groupseed);
        var generator_seed = mainrng.Next();
        var rng = new Xoroshiro128Plus(generator_seed);
        var encounter_slot = rng.Next() / Math.Pow(2, 64) * sum_to_use;
        var fixedseed = rng.Next();
        var givs = 0;
        if (id == 0 || id == 4 || id == 8 || id == 12 || id == 16 || id == 20 || id == 24)
            pk = GetCommonDistortionSpecies(encounter_slot);
        else if (id != 0 && id != 4 && id != 8 && id != 12 && id != 16 && id != 20 && id != 24)
            pk = GetDistortionSpecies(encounter_slot);

        if (pk.IsAlpha)
        {
            givs = 3;
            if (Settings.DistortionConditions.AnyAlpha)
                FillDistortionCoords(id);
        }
        var gt = PersonalTable.LA.GetFormEntry(pk.Species, pk.Form).Gender;
        var (perfect, complete) = CheckForPerfectComplete(HasCharm, dex, pk.Species);
        var rolls = 1 + (complete ? 1 : 0) + (perfect ? 2 : 0) + (HasCharm ? 3 : 0) + (int)(SpawnType.Regular - 7);

        var gen = GenerateFromSeed(fixedseed, rolls, givs, gt);

        string location = GetDistortionSpeciesLocation(id);
        if (id >= 9 && id <= 12 && Settings.DistortionConditions.DistortionLocation == ArceusMap.CrimsonMirelands)
        {
            logs += $"Ignoring Spawner from GroupID: {id} as location currently unknown.";
            return (pk, false, logs);
        }

        pk.EncryptionConstant = gen.EC;
        pk.PID = gen.PID;
        int[] pkIVList = gen.IVs;
        pk.SetIVs(pkIVList);
        (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);
        pk.IVs = pkIVList;
        pk.Nature = (int)gen.Item8;
        pk.Gender = gen.gender;

        if (gen.shiny)
        {
            CommonEdits.SetShiny(pk, Shiny.Always);
            FillDistortionCoords(id);
        }

        if (!pk.IsShiny && pk.IsAlpha)
            Log($"Alpha {(Species)pk.Species} GroupID: {id} found at Location: {location}");

        var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
        logs += $"\nGenerator Seed: {(group_seed + 0x82A2B175229D6A5B & 0xFFFFFFFFFFFFFFFF):X16}\nGroup: {id}{print}\nEncounter Slot: {encounter_slot}\nLocation: {location}\n";
        mainrng.Next();
        mainrng.Next();
        _ = new Xoroshiro128Plus(mainrng.Next());
        return (pk, gen.shiny, logs);
    }

    private void GenerateCaveFamily(bool mana, ulong seed)
    {
        PA8 pk = new();
        var mainrng = new Xoroshiro128Plus(seed);
        for (int i = 0; i < Settings.SpecialConditions.MaxAdvancesToSearch; i++)
        {
            var generator_seed = mainrng.Next();
            mainrng.Next();
            var rng = new Xoroshiro128Plus(generator_seed);
            rng.Next();
            var givs = mana ? 3 : 0;
            var gen = GenerateFromSeed(rng.Next(), 1, givs, 0);
            if (mana)
                pk.Species = (ushort)Species.Manaphy;
            if (!mana)
                pk.Species = (ushort)Species.Phione;
            pk.EncryptionConstant = gen.EC;
            pk.PID = gen.PID;
            pk.IV_HP = gen.IVs[0];
            pk.IV_ATK = gen.IVs[1];
            pk.IV_DEF = gen.IVs[2];
            pk.IV_SPA = gen.IVs[3];
            pk.IV_SPD = gen.IVs[4];
            pk.IV_SPE = gen.IVs[5];
            pk.Nature = (int)gen.nature;
            if (StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
            {
                Settings.AlphaScanConditions.StopOnMatch = true;
                string url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
            }
            Log($"\n{(Species)pk.Species}\nEC: {pk.EncryptionConstant:X8}\nPID: {pk.PID:X8}\nIVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}\nNature: {(Nature)pk.Nature}\nGenerator Seed: {generator_seed:X16}");
        }
    }

    private (ulong, PA8) GenerateNextShiny(int spawnerid, ulong seed)
    {
        PA8 pk = new();
        int hits = 0;
        ulong newseed = 0;
        var mainrng = new Xoroshiro128Plus(seed);
        int givs = 0;
        if (Settings.AlphaScanConditions.SpawnIsStaticAlpha)
            givs = 3;

        if (!Settings.AlphaScanConditions.InItSpawn)
        {
            mainrng.Next();
            mainrng.Next();
            mainrng = new Xoroshiro128Plus(mainrng.Next());
        }

        if (Settings.BotType == ArceusMode.GenieScanner || Settings.BotType == ArceusMode.OtherLegendReset)
        {
            Species species = Species.None;
            for (int i = 0; i < Settings.SpecialConditions.MaxAdvancesToSearch; i++)
            {
                if (Settings.BotType == ArceusMode.GenieScanner)
                {
                    species = Settings.SpecialConditions.ScanLocation switch
                    {
                        ArceusMap.ObsidianFieldlands => Species.Landorus,
                        ArceusMap.CrimsonMirelands => Species.Enamorus,
                        ArceusMap.CobaltCoastlands => Species.Thundurus,
                        ArceusMap.AlabasterIcelands => Species.Tornadus,
                        _ => throw new NotImplementedException("Invalid location."),
                    };
                }

                if (Settings.BotType == ArceusMode.OtherLegendReset)
                {
                    species = Settings.SpecialConditions.TypeOfLegend switch
                    {
                        LegendsOnMap.Heatran => Species.Heatran,
                        LegendsOnMap.Azelf => Species.Azelf,
                        LegendsOnMap.Mesprit => Species.Mesprit,
                        LegendsOnMap.Uxie => Species.Uxie,
                        LegendsOnMap.Cresselia => Species.Cresselia,
                        LegendsOnMap.Shaymin => Species.Shaymin,
                        LegendsOnMap.Darkrai => Species.Darkrai,
                        _ => throw new NotImplementedException("Invalid location."),
                    };
                }

                var generator_seed = mainrng.Next();
                mainrng.Next();
                var rng = new Xoroshiro128Plus(generator_seed);
                rng.Next();

                var genderratio = Settings.SpecialConditions.TypeOfLegend is LegendsOnMap.Heatran ? 127 : -1;
                var gen = GenerateFromSeed(rng.Next(), 1, 3, genderratio);
                pk.Species = (ushort)species;
                pk.EncryptionConstant = gen.EC;
                pk.PID = gen.PID;
                pk.IV_HP = gen.IVs[0];
                pk.IV_ATK = gen.IVs[1];
                pk.IV_DEF = gen.IVs[2];
                pk.IV_SPA = gen.IVs[3];
                pk.IV_SPD = gen.IVs[4];
                pk.IV_SPE = gen.IVs[5];
                pk.Nature = gen.nature;
                pk.Gender = gen.gender;

                if (StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
                {
                    Log($"\nAdvance: {i} - {species}\nEC: {pk.EncryptionConstant:X8}\nPID: {pk.PID:X8}\nIVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}\nNature: {(Nature)pk.Nature}\nGenerator Seed: {generator_seed:X16}");
                    Settings.AlphaScanConditions.StopOnMatch = true;
                    string url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                    var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                    EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                    return (newseed, pk);
                }
                Log($"\nAdvance: {i} - {species}\nEC: {pk.EncryptionConstant:X8}\nPID: {pk.PID:X8}\nIVs: {pk.IV_HP}/{pk.IV_ATK}/{pk.IV_DEF}/{pk.IV_SPA}/{pk.IV_SPD}/{pk.IV_SPE}\nNature: {(Nature)pk.Nature}\nGenerator Seed: {generator_seed:X16}");

                mainrng = new Xoroshiro128Plus(mainrng.Next());
            }

        }
        else if (Settings.BotType != ArceusMode.GenieScanner && Settings.BotType != ArceusMode.OtherLegendReset)
        {
            string species = Settings.SpecialConditions.ScanLocation switch
            {
                ArceusMap.ObsidianFieldlands => ObsidianTitle[spawnerid],
                ArceusMap.CrimsonMirelands => CrimsonTitle[spawnerid],
                ArceusMap.CoronetHighlands => CoronetTitle[spawnerid],
                ArceusMap.CobaltCoastlands => CobaltTitle[spawnerid],
                ArceusMap.AlabasterIcelands => AlabasterTitle[spawnerid],
                _ => throw new NotImplementedException("Invalid spawner ID or location."),
            };

            for (int i = 0; i < Settings.SpecialConditions.MaxAdvancesToSearch; i++)
            {
                var generator_seed = mainrng.Next();
                mainrng.Next();
                var rng = new Xoroshiro128Plus(generator_seed);
                rng.Next();

                var gen = GenerateFromSeed(rng.Next(), (int)Settings.AlphaScanConditions.StaticAlphaShinyRolls, givs, 0);
                Log($"\nAdvances: {i}\nAlpha: {species} - {gen.shinyXor} | SpawnerID: {spawnerid}\nEC: {gen.EC:X8}\nPID: {gen.PID:X8}\nIVs: {string.Join("/", gen.IVs)}\nNature: {gen.Item8}\nSeed: {gen.Item9:X16}");
                if (gen.shiny)
                {
                    if (Settings.SpeciesToHunt.Length != 0 && !Settings.SpeciesToHunt.Contains(species))
                        break;

                    if (StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
                    {
                        Log($"\nAdvances: {i}\nAlpha: {species} - {gen.shinyXor} | SpawnerID: {spawnerid}\nEC: {gen.EC:X8}\nPID: {gen.PID:X8}\nIVs: {string.Join("/", gen.IVs)}\nNature: {gen.Item8}\nSeed: {gen.Item9:X16}");
                        newseed = generator_seed;
                        Settings.AlphaScanConditions.StopOnMatch = true;
                        hits++;

                        if (hits == 3)
                        {
                            Log($"First three shiny results for {species} found.");
                            break;
                        }
                    }
                }
                mainrng = new Xoroshiro128Plus(mainrng.Next());
                if (i == Settings.SpecialConditions.MaxAdvancesToSearch - 1 && !gen.shiny)
                    Log($"No results within {Settings.SpecialConditions.MaxAdvancesToSearch} advances for {species} | SpawnerID: {spawnerid}.");
            }
        }

        return (newseed, pk);
    }

    private async Task PerformOutbreakScan(PokedexSaveData dex, CancellationToken token)
    {
        List<string> speclist = new();
        List<string> result = new();
        string[] list = Settings.SpeciesToHunt.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var ofs = new long[] { 0x42BA6B0, 0x2B0, 0x58, 0x18, 0x20 };
        var outbreakptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
        var info = await SwitchConnection.ReadBytesAbsoluteAsync(outbreakptr, 0x190, token).ConfigureAwait(false);
        for (int i = 0; i < 4; i++)
        {
            int count = 0;
            Species outbreakspecies = (Species)BitConverter.ToUInt16(info.AsSpan(0 + (i * 80), 2));
            if (outbreakspecies != Species.None)
            {
                var outbreakseed = BitConverter.ToUInt64(info.AsSpan(56 + (i * 80), 8));
                var spawncount = BitConverter.ToUInt16(info.AsSpan(64 + (i * 80), 2));
                result.Add($"\nOutbreak found for: {outbreakspecies} | Total Spawn Count: {spawncount}.");
                var monlist = ReadOutbreakSeed(dex, outbreakspecies, spawncount, outbreakseed);
                foreach (PA8 pk in monlist)
                {
                    count++;
                    var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                    var msg = $"\nOutbreak Spawn: #{count}" + print;
                    speclist.Add(msg);

                    if (pk.IsShiny && Settings.OutbreakConditions.AlphaShinyOnly && pk.IsAlpha || pk.IsShiny && !Settings.OutbreakConditions.AlphaShinyOnly)
                    {
                        await CheckEmbed(pk, "", msg, token).ConfigureAwait(false);
                    }
                }
            }
            if (list.Contains($"{outbreakspecies}") && Settings.OutbreakConditions.StopIfSpeciesFound)
            {
                Log($"{Hub.Config.StopConditions.MatchFoundEchoMention} Desired species {outbreakspecies} was found!");
                IsWaiting = true;
                while (IsWaiting)
                    await Task.Delay(1_000, token).ConfigureAwait(false);
            }
        }
        var rez = string.Join("", result);
        Log(rez);
        var res = string.Join("", speclist);
        ResultsUtil.Log(res, "[OutbreakScan]");

        if (Settings.OutbreakConditions.Permute)
        {
            Log("Beginning Outbreak permutations...");
            var (specieslist, results, moreresults) = ConsolePermuter.PermuteBlockMassOutbreak(info);
            Log("Done with permutations, check the results tab! If no results, no permutations/outbreaks are present!");
            var report = string.Join("\n", results);
            ResultsUtil.Log(report, "");
            string report2 = string.Join("\n", moreresults);
            ResultsUtil.Log(report2, "");
            bool afk = false;
            foreach (Species s in specieslist)
            {
                if (list.Contains(s.ToString()))
                {
                    Log($"Desired species has a permutation!\n{report2}");
                    afk = true;
                }
            }
            if (afk)
            {
                Settings.AddCompletedShinyAlphaFound();
                IsWaiting = true;
                while (IsWaiting)
                    await Task.Delay(1_000, token).ConfigureAwait(false);
            }
            IsWaiting = false;
        }
    }

    private static (int, int) GrabEncounterSum(ulong encslot, ulong bonusslot)
    {
        int encmax = 0;
        int bonusmax = 0;
        foreach (var keyValuePair in SpawnGenerator.EncounterTables)
        {
            if (keyValuePair.Key == encslot)
            {
                foreach (var keyValue in keyValuePair.Value)
                    encmax += keyValue.Rate;
            }

            if (keyValuePair.Key == bonusslot)
            {
                foreach (var keyValue in keyValuePair.Value)
                    bonusmax += keyValue.Rate;
            }
        }
        return (encmax, bonusmax);
    }

    private static (PA8, int) GrabMMOSpecies(double encounter_slot, ulong encslot)
    {
        var encmax = 0;
        foreach (var keyValuePair in SpawnGenerator.EncounterTables)
        {
            if (keyValuePair.Key == encslot)
            {
                foreach (var keyValue in keyValuePair.Value)
                {
                    encmax += keyValue.Rate;
                    if (encounter_slot < encmax)
                        return (new PA8 { Species = keyValue.Species, Form = (byte)keyValue.Form, IsAlpha = keyValue.IsAlpha }, keyValue.FlawlessIVs);
                }
            }
        }
        return (new(), 0);
    }

    private async Task CheckEmbed(PA8 pk, string map, string spawn, CancellationToken token)
    {
        string url = string.Empty;
        var print = $"{Hub.Config.StopConditions.GetSpecialPrintName(pk)}";
        string[] list = Settings.SpeciesToHunt.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        bool huntedspecies = list.Contains($"{(Species)pk.Species}");
        if (string.IsNullOrEmpty(map) && Hub.Config.ArceusLA.OutbreakConditions.TypeOfScan == OutbreakScanType.OutbreakOnly)
        {
            ResultsUtil.Log($"Outbreak for {(Species)pk.Species} has been found! Stopping routine execution!", "");
            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }

        bool match = false;
        if (list.Length != 0)
        {
            if (!huntedspecies || huntedspecies && Settings.OutbreakConditions.AlphaShinyOnly && !pk.IsAlpha)
            {
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                EchoUtil.EchoEmbed("", print, url, "", false);
            }

            if (huntedspecies && Settings.OutbreakConditions.AlphaShinyOnly && pk.IsAlpha || huntedspecies && !Settings.OutbreakConditions.AlphaShinyOnly && pk.IsShiny)
            {
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                match = true;
            }
        }

        if (list.Length == 0 && !Settings.OutbreakConditions.CheckBoxes)
        {
            if (Settings.OutbreakConditions.AlphaShinyOnly && !pk.IsAlpha)
            {
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                EchoUtil.EchoEmbed("", print, url, "", false);
            }
            else
            {
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                match = true;
            }
        }

        if (Settings.OutbreakConditions.CheckBoxes && match == false)
        {
            int f = 0;
            int n = 0;
            foreach (var mon in boxlist.ToList())
            {
                bool alpha = !mon.IsAlpha && pk.IsAlpha && mon.Species == pk.Species;
                bool newform = mon.Form != pk.Form && alpha;
                if (alpha || newform || !mon.IsAlpha && pk.IsAlpha && (Species)pk.Species == Species.Eevee || !mon.IsAlpha && pk.IsAlpha && (Species)pk.Species == Species.Unown)
                {
                    if (f == 0)
                    {
                        Log($"Found a {(Species)pk.Species}! It's something we don't have!\nAdding it to our boxlist!");
                        url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                        EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                        match = true;
                        boxlist.Add(pk);
                    }
                    f++;
                }
                else
                {
                    if (n == 0)
                    {
                        url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                        EchoUtil.EchoEmbed("", print, url, "", false);
                        n++;
                    }
                }
            }
        }

        ResultsUtil.Log(spawn, "");
        if (match)
        {
            IsWaiting = true;
            IsWaitingConfirmation = true;
            Settings.AddCompletedShinyAlphaFound();
            ResultsUtil.Log($"Match found! Enter{map}, type $continue or click the Continue button and I'll teleport you to the location of {(Species)pk.Species} in a Massive Mass Outbreak!", "");
            Log($"Match found! Enter{map}, type $continue or click the Continue button and I'll teleport you to the location of {(Species)pk.Species} in a Massive Mass Outbreak!");
            while (IsWaiting)
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                if (!IsWaitingConfirmation)
                {
                    await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer1, token).ConfigureAwait(false);
                    await SwitchConnection.WriteBytesAbsoluteAsync(BitConverter.GetBytes(0xD65F03C0), MainNsoBase + InvincibleTrainer2, token).ConfigureAwait(false);

                    await TeleportToMMOGroupZone(token).ConfigureAwait(false);
                    await Click(HOME, 1_000, token).ConfigureAwait(false);
                    ResultsUtil.Log($"Teleported to the location of {(Species)pk.Species}! Pressing HOME incase you weren't ready in game.", "");
                    while (IsWaiting)
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                }
            }
        }
    }

    private static void SetFakeTable(SlotDetail[] slots, ulong key)
    {
        foreach (var s in slots)
            s.SetSpecies();

        SpawnGenerator.EncounterTables.Add(key, slots);
    }

    private async Task PerformMultiSpawnerScan(PokedexSaveData dex, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var task = Hub.Config.ArceusLA.MultiScanConditions.MultiSpecies switch
            {
                MultiSpawners.Eevee => PerformMultiEeveeScan(token),
                MultiSpawners.CombeeLeft or MultiSpawners.CombeeRight => PerformMultiCombeeScan(token),
                //MultiSpawners.Unown => PerformMultiUnownScan(token),
                MultiSpawners.BasculinLeft or MultiSpawners.BasculinMid or MultiSpawners.BasculinRight => PerformMultiBasculinScan(token),
                MultiSpawners.HipposRight or MultiSpawners.HipposLeft => PerformMultiHippoScan(token),
                MultiSpawners.Magikarp => PerformMultiMagikarpScan(token),
                MultiSpawners.Buneary => PerformMultiBunnyScan(token),
                _ => PerformMultiEeveeScan(token),
            };
            await task.ConfigureAwait(false);
        }
    }
    private async Task PerformMultiEeveeScan(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int count = 2;
            ulong key = (ulong)(0x1337BABE12345678 + Util.Rand.Next(1, 999999999));
            var slots = new SlotDetail[]
            {
                new(100, "Bidoof", false, new [] {3, 6}, 0),
                new(2, "Bidoof", true , new [] {17, 19}, 3),
                new(20, "Eevee", false, new [] {3, 6}, 0),
                new(1, "Eevee", true , new [] {17, 19}, 3),
            };
            SetFakeTable(slots, key);

            string log = string.Empty;
            var groupID = (int)Settings.MultiScanConditions.MultiSpecies;
            var ofs = new long[] { 0x42A6EE0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var multiptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
            var GeneratorSeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(multiptr, 8, token).ConfigureAwait(false), 0);
            var group_seed = (GeneratorSeed - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            //Log($"Seed: {group_seed:X16}");

            var details = new SpawnCount(count, count);
            var set = new SpawnSet(key, count);
            var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

            var results = Permuter.Permute(spawner, group_seed, Settings.MultiScanConditions.Advances);
            if (!results.HasResults)
                log += $"\nNo results found within {Settings.MultiScanConditions.Advances} advances :(";
            else
            {
                var lines = results.GetLines();
                foreach (var line in lines)
                    log += "\n" + line;
            }
            Log($"{log}");
            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }
    private async Task PerformMultiCombeeScan(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int count = 2;
            ulong key = (ulong)(0x1337BABECAFEDEAD + Util.Rand.Next(1, 999999999));
            var slots = new SlotDetail[]
            {
                new(100, "Combee", false, new [] {17, 20}, 0),
                new(2, "Combee", true , new [] {32, 35}, 3),
            };
            SetFakeTable(slots, key);

            string log = string.Empty;
            var groupID = (int)Settings.MultiScanConditions.MultiSpecies;
            var ofs = new long[] { 0x42A6EE0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var multiptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
            var GeneratorSeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(multiptr, 8, token).ConfigureAwait(false), 0);
            var group_seed = (GeneratorSeed - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            //Log($"Seed: {group_seed:X16}");

            var details = new SpawnCount(count, count);
            var set = new SpawnSet(key, count);
            var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

            var results = Permuter.Permute(spawner, group_seed, Settings.MultiScanConditions.Advances);
            if (!results.HasResults)
                log += $"\nNo results found within {Settings.MultiScanConditions.Advances} advances :(";
            else
            {
                var lines = results.GetLines();
                foreach (var line in lines)
                    log += "\n" + line;
            }
            Log($"{log}");
            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }

    private async Task PerformMultiBunnyScan(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int count = 2;
            ulong key = (ulong)(0x1331B1B112345678 + Util.Rand.Next(1, 999999999));
            var slots = new SlotDetail[]
            {
                new(25, "Psyduck", false, new [] {13, 16}, 0),
                new(2, "Psyduck", true , new [] {28, 31}, 3),
                new(25, "Buneary", false, new [] {13, 16}, 0),
                new(2, "Buneary", true , new [] {28, 31}, 3),
            };
            SetFakeTable(slots, key);

            string log = string.Empty;
            var groupID = (int)Settings.MultiScanConditions.MultiSpecies;
            var ofs = new long[] { 0x42A6EE0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var multiptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
            var GeneratorSeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(multiptr, 8, token).ConfigureAwait(false), 0);
            var group_seed = (GeneratorSeed - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            //Log($"Seed: {group_seed:X16}");

            var details = new SpawnCount(count, count);
            var set = new SpawnSet(key, count);
            var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

            var results = Permuter.Permute(spawner, group_seed, Settings.MultiScanConditions.Advances);
            if (!results.HasResults)
                log += $"\nNo results found within {Settings.MultiScanConditions.Advances} advances :(";
            else
            {
                var lines = results.GetLines();
                foreach (var line in lines)
                    log += "\n" + line;
            }
            Log($"{log}");
            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }

    private async Task PerformMultiBasculinScan(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int count = 2;
            ulong key = (ulong)(0x1337B0BACAFEB00B + Util.Rand.Next(1, 999999999));
            var slots = new SlotDetail[]
            {
                new(100, "Basculin-2", false, new [] {41, 44}, 0),
                new(2, "Basculin-2", true , new [] {56, 59}, 3),
            };
            SetFakeTable(slots, key);

            string log = string.Empty;
            var groupID = (int)Settings.MultiScanConditions.MultiSpecies;
            var ofs = new long[] { 0x42A6EE0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var multiptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
            var GeneratorSeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(multiptr, 8, token).ConfigureAwait(false), 0);
            var group_seed = (GeneratorSeed - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            //Log($"Seed: {group_seed:X16}");

            var details = new SpawnCount(count, count);
            var set = new SpawnSet(key, count);
            var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

            var results = Permuter.Permute(spawner, group_seed, Settings.MultiScanConditions.Advances);
            if (!results.HasResults)
                log += $"\nNo results found within {Settings.MultiScanConditions.Advances} advances :(";
            else
            {
                var lines = results.GetLines();
                foreach (var line in lines)
                    log += "\n" + line;
            }
            Log($"{log}");
            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }

    private async Task PerformMultiHippoScan(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int count = 3;
            ulong key = (ulong)(0x1221B3A312345678 + Util.Rand.Next(1, 999999999));
            var slots = new SlotDetail[]
            {
                new(30, "Hippopotas", false, new [] {30, 33}, 0),
                new(2, "Hippopotas", true , new [] {45, 48}, 3),
                new(100, "Hippowdon", false, new [] {43, 46}, 0),
                new(1, "Hippowdon", true , new [] {58, 61}, 3),
            };
            SetFakeTable(slots, key);

            string log = string.Empty;
            var groupID = (int)Settings.MultiScanConditions.MultiSpecies;
            var ofs = new long[] { 0x42A6EE0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var multiptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
            var GeneratorSeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(multiptr, 8, token).ConfigureAwait(false), 0);
            var group_seed = (GeneratorSeed - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            //Log($"Seed: {group_seed:X16}");

            var details = new SpawnCount(count, count);
            var set = new SpawnSet(key, count);
            var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

            var results = Permuter.Permute(spawner, group_seed, Settings.MultiScanConditions.Advances);
            if (!results.HasResults)
                log += $"\nNo results found within {Settings.MultiScanConditions.Advances} advances :(";
            else
            {
                var lines = results.GetLines();
                foreach (var line in lines)
                    log += "\n" + line;
            }
            Log($"{log}");
            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }

    private async Task PerformMultiMagikarpScan(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int count = 2;
            ulong key = (ulong)(0x1221B1B112345678 + Util.Rand.Next(1, 999999999));
            var slots = new SlotDetail[]
            {
                new(100, "Magikarp", false, new [] {16, 19}, 0),
                new(2, "Magikarp", true , new [] {31, 34}, 3),
                new(30, "Gyarados", false, new [] {53, 56}, 0),
                new(1, "Gyarados", true , new [] {68, 71}, 3),
            };
            SetFakeTable(slots, key);

            string log = string.Empty;
            var groupID = (int)Settings.MultiScanConditions.MultiSpecies;
            var ofs = new long[] { 0x42A6EE0, 0x330, 0x70 + groupID * 0x440 + 0x20 };
            var multiptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
            var GeneratorSeed = BitConverter.ToUInt64(await SwitchConnection.ReadBytesAbsoluteAsync(multiptr, 8, token).ConfigureAwait(false), 0);
            var group_seed = (GeneratorSeed - 0x82A2B175229D6A5B) & 0xFFFFFFFFFFFFFFFF;
            //Log($"Seed: {group_seed:X16}");

            var details = new SpawnCount(count, count);
            var set = new SpawnSet(key, count);
            var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);

            var results = Permuter.Permute(spawner, group_seed, Settings.MultiScanConditions.Advances);
            if (!results.HasResults)
                log += $"\nNo results found within {Settings.MultiScanConditions.Advances} advances :(";
            else
            {
                var lines = results.GetLines();
                foreach (var line in lines)
                    log += "\n" + line;
            }
            Log($"{log}");
            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }

    private async Task PerformMMOScan(PokedexSaveData dex, CancellationToken token)
    {
        List<string> logs = new();
        List<string> mmoactive = new();
        var ofs = new long[] { 0x42BA6B0, 0x2B0, 0x58, 0x18, 0x1B0 };
        var outbreakptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
        var info = await SwitchConnection.ReadBytesAbsoluteAsync(outbreakptr, 14720, token).ConfigureAwait(false);

        for (int mapcount = 0; mapcount < 5; mapcount++)
        {
            ResultsUtil.Log($"Checking map #{mapcount + 1}...", "");
            ofs = new long[] { 0x42BA6B0, 0x2B0, 0x58, 0x18, 0x1B0 + (mapcount * 0xB80) };
            outbreakptr = await SwitchConnection.PointerAll(ofs, token).ConfigureAwait(false);
            var active = BitConverter.ToUInt16(await SwitchConnection.ReadBytesAbsoluteAsync(outbreakptr, 2, token).ConfigureAwait(false), 0);
            bool areavalid = $"{active:X4}" is not ("0000" or "2645");
            if (!areavalid)
                continue;

            var mapinfo = info.AsSpan(mapcount * 2944, 2944).ToArray();

            string map = $"{active:X4}" switch
            {
                "56B7" => " in the Cobalt Coastlands",
                "5504" => " in the Crimson Mirelands",
                "5351" => " in the Alabaster Icelands",
                "519E" => " in the Coronet Highlands",
                "5A1D" => " in the Obsidian Fieldlands",
                _ => throw new NotImplementedException("Unknown map."),
            };

            var groupcount = 0;
            var species = (Species)BitConverter.ToUInt16(mapinfo.AsSpan(36 + (groupcount * 144), 2).ToArray(), 0);
            while (species != Species.None && species < Species.MAX_COUNT)
            {
                var spawncount = BitConverter.ToUInt16(mapinfo.AsSpan(112 + (groupcount * 144), 2).ToArray(), 0);
                var encslot = BitConverter.ToUInt64(mapinfo.AsSpan(72 + (groupcount * 144), 8).ToArray(), 0);
                var bonusencslot = BitConverter.ToUInt64(mapinfo.AsSpan(80 + (groupcount * 144), 8).ToArray(), 0);
                var bonuscount = BitConverter.ToUInt16(mapinfo.AsSpan(132 + (groupcount * 144), 2).ToArray(), 0);
                var group_seed = BitConverter.ToUInt64(mapinfo.AsSpan(104 + (groupcount * 144), 8).ToArray(), 0);
                var xyz = mapinfo.AsSpan(16 + (groupcount * 144), 12).ToArray();
                var spawncoordx = BitConverter.ToUInt32(xyz, 0);
                var spawncoordy = BitConverter.ToUInt32(xyz, 4);
                var spawncoordz = BitConverter.ToUInt32(xyz, 8);
                mmoactive.Add($"\nGroup Seed: {string.Format("0x{0:X}", group_seed)}\nMassive Mass Outbreak found for: {species}{map} | Total Spawn Count: {spawncount} | Group ID: {groupcount}");
                List<PA8> pokelist;
                List<PA8> bonuslist;
                (pokelist, bonuslist) = ReadMMOSeed(dex, spawncount, group_seed, bonuscount, encslot, bonusencslot);
                for (int p = 0; p < pokelist.Count; p++)
                {
                    var pk = pokelist[p];
                    var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                    var msg = $"\nRegular Spawn: #{p + 1}" + print;
                    logs.Add(msg);
                    if (pk.IsShiny)
                    {
                        Log($"{msg}\n\nAutofilling coords for {(Species)pk.Species}. Check the Results tab for more information.");
                        coordinates = ($"{spawncoordx:X8}", $"{spawncoordy:X8}", $"{spawncoordz:X8}");
                        await CheckEmbed(pk, map, msg, token).ConfigureAwait(false);
                    }
                }

                for (int b = 0; b < bonuslist.Count; b++)
                {
                    var pk = bonuslist[b];
                    var print = Hub.Config.StopConditions.GetAlphaPrintName(pk);
                    var msg = $"\nBonus Spawn: #{b + 1}" + print;
                    logs.Add(msg);
                    if (pk.IsShiny)
                    {
                        Log($"{msg}\n\nAutofilling coords for {(Species)pk.Species}. Check the Results tab for more information.");
                        coordinates = ($"{spawncoordx:X8}", $"{spawncoordy:X8}", $"{spawncoordz:X8}");
                        await CheckEmbed(pk, map, msg, token).ConfigureAwait(false);
                    }
                }
                var report = string.Join("\n", logs);
                ResultsUtil.Log(report, "[MMOScan]");
                groupcount++;
                species = (Species)BitConverter.ToUInt16(mapinfo.AsSpan(36 + (groupcount * 144), 2).ToArray(), 0);
            }
            var actives = string.Join("", mmoactive);
            Log(actives);
        }

        if (Settings.OutbreakConditions.Permute)
        {
            string res = string.Empty;
            Log("Beginning MMO permutations...");
            var (specieslist, results, moreresults) = ConsolePermuter.PermuteMassiveMassOutbreak(info);
            Log("Done with permutations, check the results tab! If no results, no permutations/outbreaks are present!");
            var report = string.Join("\n", results);
            string report2 = string.Join("\n", moreresults);
            ResultsUtil.Log(report2, "");
            ResultsUtil.Log(report, "");
            ResultsUtil.Log(res, "");
            string[] list = Settings.SpeciesToHunt.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool afk = false;
            if (list.Length != 0)
            {
                foreach (Species s in specieslist)
                {
                    if (list.Contains(s.ToString()))
                    {
                        afk = true;
                        PA8 s1 = new()
                        {
                            Species = (ushort)s
                        };
                        string url = TradeExtensions<PK9>.PokeImg(s1, false, false);
                        var print = Hub.Config.StopConditions.GetAlphaPrintName(s1);
                        EchoUtil.EchoEmbed(Hub.Config.StopConditions.MatchFoundEchoMention, print, url, "", true);
                        Settings.AddCompletedShinyAlphaFound();
                    }
                }
                if (afk)
                {
                    Log($"Desired species has a permutation!\n{report}");

                    IsWaiting = true;
                    while (IsWaiting)
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                }
            }
            if (list.Length == 0 && specieslist.Count > 0)
            {
                IsWaiting = true;
                while (IsWaiting)
                    await Task.Delay(1_000, token).ConfigureAwait(false);
            }
            IsWaiting = false;
        }
    }

    private async Task MassiveOutbreakHunter(PokedexSaveData dex, CancellationToken token)
    {
        if (Settings.OutbreakConditions.CheckBoxes)
        {
            Log("Initiating box reader...");
            boxlist = await ReadPokemonBoxes(token).ConfigureAwait(false);
        }
        int attempts = 0;
        while (!token.IsCancellationRequested)
        {
            if (Settings.OutbreakConditions.CheckDistortionFirst)
            {
                Log("Checking our distortions before scanning outbreaks!");
                await DistortionReader(dex, token).ConfigureAwait(false);
            }
            Log($"Search #{attempts + 1}: Reading map for active outbreaks...");
            await Click(Y, 1_000, token).ConfigureAwait(false);
            while (!await IsOnOverworldTitle(token).ConfigureAwait(false))
                await Click(A, 0_800, token).ConfigureAwait(false);

            var type = Settings.OutbreakConditions.TypeOfScan;
            switch (type)
            {
                case OutbreakScanType.Both: await PerformOutbreakScan(dex, token).ConfigureAwait(false); await PerformMMOScan(dex, token).ConfigureAwait(false); break;
                case OutbreakScanType.OutbreakOnly: await PerformOutbreakScan(dex, token).ConfigureAwait(false); break;
                case OutbreakScanType.MMOOnly: await PerformMMOScan(dex, token).ConfigureAwait(false); break;
            }

            Log("No match found, resetting.");
            await Click(B, 1_000, token).ConfigureAwait(false);
            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token).ConfigureAwait(false);
            attempts++;
        }
    }

    public async Task<List<PA8>> ReadPokemonBoxes(CancellationToken token)
    {
        var b1s1 = new long[] { 0x42BA6B0, 0x1F0, 0x68 };
        var boxStart = await SwitchConnection.PointerAll(b1s1, token).ConfigureAwait(false);
        var data = await SwitchConnection.ReadBytesAbsoluteAsync(boxStart, BoxFormatSlotSize * 960, token).ConfigureAwait(false);

        for (int slot = 0; slot < 960; slot++)
        {
            int slotAdjustment = slot * BoxFormatSlotSize;
            byte[] dataSlice = data.AsSpan(slotAdjustment, BoxFormatSlotSize).ToArray();
            boxlist.Add(new PA8(dataSlice));
        }
        return boxlist;
    }
}