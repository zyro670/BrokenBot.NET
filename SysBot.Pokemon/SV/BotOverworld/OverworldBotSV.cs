﻿using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.OverworldSettingsSV;
using static System.Buffers.Binary.BinaryPrimitives;

namespace SysBot.Pokemon
{
    public class OverworldBotSV : PokeRoutineExecutor9SV, IEncounterBot
    {
        private readonly PokeTradeHub<PK9> Hub;
        private readonly IDumper DumpSetting;
        private readonly int[] DesiredMinIVs;
        private readonly int[] DesiredMaxIVs;
        private readonly OverworldSettingsSV Settings;
        public ICountSettings Counts => Settings;
        public readonly IReadOnlyList<string> UnwantedMarks;

        private uint TodaySeed;
        private ulong OverworldOffset;
        private ulong TeraRaidBlockOffset;
        private int scanCount;
        private int PicnicVal = 0;
        private static ulong BaseBlockKeyPointer = 0;
        private ulong PlayerCanMoveOffset;
        private ulong PlayerOnMountOffset;
        private bool GameWasReset = false;
        private int SandwichCounter;
        private SAV9SV TrainerSav = new();

        public OverworldBotSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
        {
            Hub = hub;
            Settings = Hub.Config.OverworldSV;
            DumpSetting = Hub.Config.Folder;
            StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
            StopConditionSettings.ReadUnwantedMarks(Hub.Config.StopConditions, out UnwantedMarks);
        }

        public override async Task MainLoop(CancellationToken token)
        {
            await InitializeHardware(Hub.Config.OverworldSV, token).ConfigureAwait(false);
            Log("Identifying trainer data of the host console.");
            TrainerSav = await IdentifyTrainer(token).ConfigureAwait(false);
            Log("Starting main OverworldBotSV loop.");
            Config.IterateNextRoutine();

            try
            {
                await InitializeSessionOffsets(token).ConfigureAwait(false);
                if (Settings.RolloverFilters.ConfigureRolloverCorrection)
                {
                    await RolloverCorrectionSV(token, false).ConfigureAwait(false);
                    return;
                }
                await ScanOverworld(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            Log($"Ending {nameof(OverworldBotSV)} loop.");
            await HardStop().ConfigureAwait(false);
        }

        private bool IsWaiting;
        public void Acknowledge() => IsWaiting = false;

        public override async Task HardStop()
        {
            // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
            await SetStick(LEFT, 0, 0, 0, CancellationToken.None).ConfigureAwait(false); // reset
            await CleanExit(CancellationToken.None).ConfigureAwait(false);
        }

        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            BaseBlockKeyPointer = await SwitchConnection.PointerAll(Offsets.BlockKeyPointer, token).ConfigureAwait(false);
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
            PlayerCanMoveOffset = await SwitchConnection.PointerAll(Offsets.MobilityPointer, token).ConfigureAwait(false);
            PlayerOnMountOffset = await SwitchConnection.PointerAll(Offsets.PlayerOnMountPointer, token).ConfigureAwait(false);
            TeraRaidBlockOffset = await SwitchConnection.PointerAll(Offsets.TeraRaidBlockPointer, token).ConfigureAwait(false);
            Log("Caching offsets complete!");
        }       

        private async Task ResetOverworld(CancellationToken token, bool time)
        {
            await Click(B, 0_050, token).ConfigureAwait(false);
            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await RolloverCorrectionSV(token, time).ConfigureAwait(false);
            await StartGame(Hub.Config, token).ConfigureAwait(false);
            await InitializeSessionOffsets(token).ConfigureAwait(false);
        }
        
        private async Task Preparize(CancellationToken token)
        {
            Log("Navigating to picnic..");
            await Click(X, 3_000, token).ConfigureAwait(false);
            await Click(DRIGHT, 0_800, token).ConfigureAwait(false);
            while (await PlayerCannotMove(token).ConfigureAwait(false))
            {
                Log("Scrolling through menus...");
                await SetStick(LEFT, 0, -32000, 1_000, token).ConfigureAwait(false);
                await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
                await Task.Delay(0_100, token).ConfigureAwait(false);
                Log("Tap tap tap...");
                for (int i = 0; i < 3; i++)
                    await Click(DDOWN, 0_800, token).ConfigureAwait(false);
                Log("Attempting to enter picnic!");
                await Click(A, 9_500, token).ConfigureAwait(false);

                if (!await PlayerCannotMove(token).ConfigureAwait(false))
                    break;

                Log("Not in picnic! Wrong menu? Attempting recovery.");
                await Click(B, 4_500, token).ConfigureAwait(false); // Not in picnic, press B to reset

            }
            Log("Time for a bonus!");
            await MakeSandwich(token).ConfigureAwait(false);
            SandwichCounter++;
            Log("Continuing the hunt..");
        }

        private async Task ScanOverworld(CancellationToken token)
        {
            bool atStation = false;
            List<PK9> encounters = new();
            List<string> prints = new();
            var dayRoll = 0;
            PicnicVal = await PicnicState(token).ConfigureAwait(false);
            TodaySeed = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 4, token).ConfigureAwait(false), 0);
            int status = 0;
            int start = 0;
            while (!token.IsCancellationRequested)
            {
                start++;
                if (start == 3 && Settings.RolloverFilters.CheckForRollover)
                {
                    await ResetOverworld(token, true).ConfigureAwait(false);
                    start = 0;
                }
                uint currentSeed = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 4, token).ConfigureAwait(false), 0);
                if (TodaySeed != currentSeed && Settings.RolloverFilters.CheckForRollover)
                {
                    var msg = $"Current Today Seed {currentSeed:X8} does not match Starting Today Seed: {TodaySeed:X8} attempting rollover correction. ";
                    if (dayRoll != 0)
                    {
                        Log(msg + "Stopping routine for the day changing.");
                        return;
                    }
                    Log(msg);     
                    await ResetOverworld(token, false).ConfigureAwait(false);
                    PicnicVal = await PicnicState(token).ConfigureAwait(false);
                    TodaySeed = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 4, token).ConfigureAwait(false), 0);
                    if (Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder)
                    {
                        await Click(Y, 1_700, token).ConfigureAwait(false);
                        await Click(RSTICK, 1_000, token).ConfigureAwait(false);
                        await Click(B, 2_500, token).ConfigureAwait(false);
                    }
                    status = 0;

                    dayRoll++;
                    continue;
                }

                if (Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder && atStation is false)
                {
                    Log("Preparing for Area Zero...");
                    await NavigateToAreaZeroEntrance(token).ConfigureAwait(false);
                    await NavigateToAreaZeroPicnic(token).ConfigureAwait(false);
                }

                if (Settings.MakeASandwich)
                    await Preparize(token).ConfigureAwait(false);

                var wait = TimeSpan.FromMinutes(30);
                var endTime = DateTime.Now + wait;
                while (DateTime.Now < endTime)
                {
                    if (Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder && atStation is false)
                    {
                        await RepositionToGate(token).ConfigureAwait(false);
                        await NavigateToResearchStation(token).ConfigureAwait(false);
                        if (Settings.LocationSelection is Location.SecretCave)
                        {
                            await Click(PLUS, 1_500, token).ConfigureAwait(false);
                            await CollideToCave(token).ConfigureAwait(false);
                        }

                        atStation = true;
                    }

                    if (Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder && atStation is false)
                    {
                        await Task.Delay(1_500, token).ConfigureAwait(false);
                        if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                        {
                            Log("Not in the overworld, are we in an unwanted battle? Attempting recovery");

                            for (int i = 0; i < 6; i++)
                                await Click(B, 0_500, token).ConfigureAwait(false);
                            await Click(DUP, 1_500, token).ConfigureAwait(false);
                            await Click(A, 2_500, token).ConfigureAwait(false);

                            for (int i = 0; i < 2; i++)
                                await Click(R, 0_500, token).ConfigureAwait(false); // Trigger twice to send out Let's Go Pokemon with aggression to knockout a close by encounter
                        }
                    }

                    currentSeed = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 4, token).ConfigureAwait(false), 0);
                    if (TodaySeed != currentSeed && Settings.RolloverFilters.CheckForRollover)
                    {
                        var msg = $"Current Today Seed {currentSeed:X8} does not match Starting Today Seed: {TodaySeed:X8} after rolling back 1 day. ";
                        if (dayRoll != 0)
                        {
                            Log(msg + "Stopping routine for the day changing.");
                            return;
                        }
                        Log(msg);
                        await ResetOverworld(token, false).ConfigureAwait(false);
                        PicnicVal = await PicnicState(token).ConfigureAwait(false);
                        TodaySeed = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 4, token).ConfigureAwait(false), 0);
                        if (Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder)
                        {
                            await Click(Y, 1_700, token).ConfigureAwait(false);
                            await Click(RSTICK, 1_000, token).ConfigureAwait(false);
                            await Click(B, 2_500, token).ConfigureAwait(false);
                        }
                        if (Settings.MakeASandwich)
                            await Preparize(token).ConfigureAwait(false);

                        dayRoll++;

                        wait = TimeSpan.FromMinutes(30);
                        endTime = DateTime.Now + wait;
                        status = 0;
                        continue;
                    }

                    if (await PlayerCannotMove(token).ConfigureAwait(false) && Settings.LocationSelection != Location.SecretCave || await PlayerCannotMove(token).ConfigureAwait(false) && await PlayerNotOnMount(token).ConfigureAwait(false) && Settings.LocationSelection == Location.SecretCave)
                    {
                        Log("We can't move! Are we in battle? Resetting game to attempt recovery and positioning.");
                        await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                        PicnicVal = await PicnicState(token).ConfigureAwait(false);
                        TodaySeed = BitConverter.ToUInt32(await SwitchConnection.ReadBytesAbsoluteAsync(TeraRaidBlockOffset, 4, token).ConfigureAwait(false), 0);
                        if (Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder)
                        {
                            await Click(Y, 1_700, token).ConfigureAwait(false);
                            await Click(RSTICK, 1_000, token).ConfigureAwait(false);
                            await Click(B, 2_500, token).ConfigureAwait(false);
                        }
                        status = 0;

                        if (Settings.LocationSelection == Location.SecretCave)
                        {
                            Log("Attempting to reposition map from reset...");
                            await Click(Y, 2_000, token).ConfigureAwait(false);
                            await Click(RSTICK, 1_000, token).ConfigureAwait(false);
                            await Click(B, 2_500, token).ConfigureAwait(false);
                        }

                        GameWasReset = true;
                    }

                    if (GameWasReset)
                        break;

                    await Task.Delay(0 + Settings.WaitTimeBeforeSaving, token).ConfigureAwait(false);
                    await SVSaveGameOverworld(token).ConfigureAwait(false);
                    var block = await ReadBlock(BaseBlockKeyPointer, Blocks.Overworld, status is 0, token).ConfigureAwait(false);
                    if (status is 0)
                        status++;
                    for (int i = 0; i < 20; i++)
                    {
                        var data = block.Slice(0 + (i * 0x1D4), 0x157);
                        var pk = new PK9(data);
                        if ((Species)pk.Species == Species.None)
                            break;
                        scanCount++;
                        var result = $"\nEncounter: {scanCount}{Environment.NewLine}{Hub.Config.StopConditions.GetSpecialPrintName(pk)}";
                        TradeExtensions<PK9>.EncounterLogs(pk, "EncounterLogPretty_OverworldSV.txt");
                        TradeExtensions<PK9>.EncounterScaleLogs(pk, "EncounterLogScalePretty.txt");
                        encounters.Add(pk);
                        prints.Add(result);
                    }                    

                    if (encounters.Count < 1 && !await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false) && Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder)
                    {
                        Log("No encounters present, are we in a lab? Attempting recovery");
                        await Click(B, 1_500, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, -32323, 0_800, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        await SetStick(LEFT, -32323, 0, 0_500, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
                        await Task.Delay(0_500, token).ConfigureAwait(false);
                        await SetStick(LEFT, -32323, 0, 2_000, token).ConfigureAwait(false);
                        await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
                        await Task.Delay(6_500, token).ConfigureAwait(false);
                        if (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                        {
                            Log($"{Hub.Config.StopConditions.MatchFoundEchoMention} failed to return to overworld. Stopping routine.");
                            return;
                        }
                        else
                            continue;
                    }

                    string res = string.Join(Environment.NewLine, prints);
                    Log(res);

                    for (int i = 0; i < encounters.Count; i++)
                    {
                        bool match = await CheckEncounter(prints[i], encounters[i]).ConfigureAwait(false);
                        if (!match && Settings.ContinueAfterMatch is ContinueAfterMatch.StopExit)
                            return;
                    }

                    encounters = new();
                    prints = new();

                    var task = Hub.Config.OverworldSV.LocationSelection switch
                    {
                        Location.SecretCave => ResetFromSecretCave(token),
                        Location.NonAreaZero => ResetEncounters(token),
                        Location.TownBorder => DownUp(token),
                        _ => EnterExitStation(token),
                    };
                    await task.ConfigureAwait(false);

                }

                if (Settings.LocationSelection != Location.NonAreaZero && Settings.LocationSelection != Location.TownBorder && GameWasReset == false)
                {
                    await ReturnFromStation(token).ConfigureAwait(false);
                    atStation = false;
                }

                if (!Settings.MakeASandwich && GameWasReset == false)
                {
                    var ping = string.Empty;
                    if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                        ping = Hub.Config.StopConditions.MatchFoundEchoMention;

                    Log($"{ping} 30 minutes have passed and you chose not to make a sandwich, stopping routine.");
                    return;
                }

                if (GameWasReset == true)
                {
                    Log("Game was reset for recovery, restarting routine.");
                    GameWasReset = false;
                }
            }
        }

        private async Task<bool> CheckEncounter(string print, PK9 pk)
        {
            var token = CancellationToken.None;
            var url = string.Empty;
            Settings.AddCompletedScans();

            if (pk.IsShiny)
            {
                Settings.AddShinyScans();                
                DumpPokemon(DumpSetting.DumpFolder, "overworld", pk);
            }

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

            if (!StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, UnwantedMarks))
            {
                if (Hub.Config.StopConditions.ShinyTarget is TargetShinyType.AnyShiny or TargetShinyType.StarOnly or TargetShinyType.SquareOnly && pk.IsShiny)
                {
                    url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                    EchoUtil.EchoEmbed("", print, url, markurl, false);
                }

                return true; //No match, return true to keep scanning
            }

            var ping = string.Empty;
            if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                ping = Hub.Config.StopConditions.MatchFoundEchoMention;

            if (Settings.StopOnMinMaxScale)
            {
                if (pk.Scale > 0 && pk.Scale < 255)
                {
                    Log("Undesired size found..");
                    url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                    EchoUtil.EchoEmbed("", print, url, markurl, false);
                    return true;
                }

                else if (pk.Scale is 0 or 255)
                {
                    string scalemsg = pk.Scale is 0 ? "XXXS" : "XXXL";
                    string ress = $"A special sized {scalemsg} {(Species)pk.Species} has been found!\n";
                    Log(ress);
                }
            }

            StopConditionSettings.HasMark(pk, out RibbonIndex specialmark);
            if (Settings.SpecialMarksOnly && specialmark is >= RibbonIndex.MarkLunchtime and <= RibbonIndex.MarkMisty || Settings.SpecialMarksOnly && specialmark is RibbonIndex.MarkUncommon)
            {
                Log($"Undesired {specialmark} found..");
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                EchoUtil.EchoEmbed("", print, url, markurl, false);
                return true;
            }

            if (Settings.StopOnOneInOneHundredOnly)
            {
                if ((Species)pk.Species is Species.Dunsparce or Species.Dudunsparce or Species.Tandemaus or Species.Maushold && pk.EncryptionConstant % 100 != 0)
                {
                    string segmsg = (Species)pk.Species is Species.Dunsparce or Species.Dudunsparce ? "2-Segment" : "Family Of 4";
                    string res3 = $"A non-special {segmsg} {(Species)pk.Species} has been found...\n";
                    Log(res3);
                    url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                    EchoUtil.EchoEmbed("", print, url, "", false);
                    return true; // 1/100 condition unsatisfied, continue scanning
                }

                else if ((Species)pk.Species is Species.Dunsparce or Species.Dudunsparce or Species.Tandemaus or Species.Maushold && pk.EncryptionConstant % 100 == 0)
                {
                    string segmsg = (Species)pk.Species is Species.Dunsparce or Species.Dudunsparce ? "3-Segment" : "Family Of 3";
                    string res3 = $"A special {segmsg} {(Species)pk.Species} has been found!\n";
                    Log(res3);
                }
            }

            var text = Settings.SpeciesToHunt.Replace(" ", "");
            string[] monlist = text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (monlist.Length != 0)
            {
                bool huntedspecies = monlist.Contains($"{(Species)pk.Species}");
                if (!huntedspecies)
                {
                    Log("Undesired species found..");
                    url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                    EchoUtil.EchoEmbed("", print, url, markurl, false);
                    return true;
                }
            }

            var mode = Settings.ContinueAfterMatch;
            var msg = $"Result found!\n{print}\n" + mode switch
            {
                ContinueAfterMatch.PauseWaitAcknowledge => "Waiting for instructions to continue.",
                ContinueAfterMatch.Continue => "Continuing..",
                ContinueAfterMatch.StopExit => "Stopping routine execution; restart the bot to search again.",
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (!string.IsNullOrWhiteSpace(ping))
                msg = $"{ping} {msg}";

            Log(msg);

            if (mode == ContinueAfterMatch.StopExit) // Stop & Exit: Condition satisfied.  Stop scanning and disconnect the bot
            {
                url = TradeExtensions<PK9>.PokeImg(pk, false, false);
                EchoUtil.EchoEmbed(ping, print, url, markurl, true);
                return false;
            }

            url = TradeExtensions<PK9>.PokeImg(pk, false, false);
            EchoUtil.EchoEmbed(ping, print, url, markurl, true);

            if (mode == ContinueAfterMatch.PauseWaitAcknowledge)
            {
                Log("Pressing HOME to freeze Overworld encounters from moving!");
                await Click(HOME, 0_700, token).ConfigureAwait(false);

                IsWaiting = true;
                while (IsWaiting)
                    await Task.Delay(5_000, token).ConfigureAwait(false);

                for (int i = 0; i < 2; i++)
                    await Click(B, 0_500, token).ConfigureAwait(false);
                await Click(HOME, 1_000, token).ConfigureAwait(false);
            }

            return false;
        }

        private async Task ResetFromSecretCave(CancellationToken token)
        {            
            await CollideToTheSpot(token).ConfigureAwait(false);
            await CollideToCave(token).ConfigureAwait(false);
        }

        private async Task ResetEncounters(CancellationToken token)
        {
            await Task.Delay(0_500, token).ConfigureAwait(false);
            await Click(A, 9_000, token).ConfigureAwait(false);
            // Back to overworld
            for (int i = 0; i < 2; i++) // extra Y click incase we enter map instead of picnic to close map
                await Click(Y, 2_000, token).ConfigureAwait(false);
            await Click(A, 4_000, token).ConfigureAwait(false);
        }

        private async Task MakeSandwich(CancellationToken token)
        {
            await Click(MINUS, 0_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 32323, 0_700, token).ConfigureAwait(false); // Face up to table
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            await Click(A, 1_500, token).ConfigureAwait(false);
            await Click(A, 1_500, token).ConfigureAwait(false); // Dummy press if we're in union circle, doesn't affect routine
            await Click(A, 10_000, token).ConfigureAwait(false);
            await Click(X, 2_500, token).ConfigureAwait(false);

            Log("Selecting ingredient 1..");
            for (int i = 0; i < Settings.PicnicFilters.Item1Clicks; i++) // Select first ingredient
                await Click(Settings.PicnicFilters.Item1DUP ? DUP : DDOWN, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(PLUS, 1_500, token).ConfigureAwait(false);

            Log("Selecting ingredient 2..");
            for (int i = 0; i < Settings.PicnicFilters.Item2Clicks; i++) // Select second ingredient
                await Click(Settings.PicnicFilters.Item2DUP ? DUP : DDOWN, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);

            Log("Selecting ingredient 3..");
            for (int i = 0; i < Settings.PicnicFilters.Item3Clicks; i++) // Select third ingredient
                await Click(Settings.PicnicFilters.Item3DUP ? DUP : DDOWN, 0_500, token).ConfigureAwait(false);

            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(PLUS, 1_000, token).ConfigureAwait(false);
            await Click(A, 8_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 30000, Settings.PicnicFilters.HoldUpToIngredients, token).ConfigureAwait(false); // Scroll up to the lettuce
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

            for (int i = 0; i < Settings.PicnicFilters.AmountOfIngredientsToHold; i++) // Amount of ingredients to drop
            {
                await Hold(A, 0_800, token).ConfigureAwait(false);

                await SetStick(LEFT, 0, -30000, 0_000 + Settings.PicnicFilters.HoldUpToIngredients, token).ConfigureAwait(false); // Navigate to ingredients
                await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
                await Task.Delay(0_500, token).ConfigureAwait(false);
                await Release(A, 0_800, token).ConfigureAwait(false);

                await SetStick(LEFT, 0, 30000, 0_000 + Settings.PicnicFilters.HoldUpToIngredients, token).ConfigureAwait(false); // Navigate to ingredients
                await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
                await Task.Delay(0_500, token).ConfigureAwait(false);
            }

            for (int i = 0; i < 12; i++) // If everything is properly positioned
                await Click(A, 0_800, token).ConfigureAwait(false);

            // Sandwich failsafe
            for (int i = 0; i < 5; i++) //Attempt this several times to ensure it goes through
                await SetStick(LEFT, 0, 30000, 1_000, token).ConfigureAwait(false); // Scroll to the absolute top
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

            while (await PicnicState(token).ConfigureAwait(false) == PicnicVal + 1) // Until we start eating the sandwich
            {
                await SetStick(LEFT, 0, -5000, 0_300, token).ConfigureAwait(false); // Scroll down slightly and press A a few times; repeat until un-stuck
                await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

                for (int i = 0; i < 6; i++)
                    await Click(A, 0_800, token).ConfigureAwait(false);
            }

            Log("Eating our sandwich..");

            while (await PicnicState(token).ConfigureAwait(false) == PicnicVal + 2)  // eating the sandwich
                await Task.Delay(1_000, token).ConfigureAwait(false);

            while (await PicnicState(token).ConfigureAwait(false) != PicnicVal) // Acknowledge the sandwich and return to the picnic            
                await Click(A, 5_000, token).ConfigureAwait(false); // Wait a long time to give the flag a chance to update and avoid sandwich re-entry            

            await Task.Delay(2_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -10000, 1_000, token).ConfigureAwait(false); // Face down to basket
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 5000, 0_400, token).ConfigureAwait(false); // Face up to basket
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_500, token).ConfigureAwait(false);
            Log("Returning to overworld..");
            await Click(Y, 2_500, token).ConfigureAwait(false);
            await Click(A, 3_500, token).ConfigureAwait(false);
            for (int i = 0; i < 10; i++)
                await Click(A, 0_800, token).ConfigureAwait(false);
        }

        private async Task<int> PicnicState(CancellationToken token)
        {
            var Data = await SwitchConnection.ReadBytesMainAsync(Offsets.LoadedIntoDesiredState, 1, token).ConfigureAwait(false);
            return Data[0]; // 1 when in picnic, 2 in sandwich menu, 3 when eating, 2 when done eating
        }

        private async Task<bool> PlayerCannotMove(CancellationToken token)
        {
            var Data = await SwitchConnection.ReadBytesAbsoluteAsync(PlayerCanMoveOffset, 1, token).ConfigureAwait(false);
            return Data[0] == 0x00; // 0 nope else yes
        }

        private async Task<bool> PlayerNotOnMount(CancellationToken token)
        {
            var Data = await SwitchConnection.ReadBytesAbsoluteAsync(PlayerOnMountOffset, 1, token).ConfigureAwait(false);
            return Data[0] == 0x00; // 0 nope else yes
        }

        private async Task Hold(SwitchButton b, int delay, CancellationToken token)
        {
            await SwitchConnection.SendAsync(SwitchCommand.Hold(b, true), token).ConfigureAwait(false);
            await Task.Delay(delay, token).ConfigureAwait(false);
        }

        private async Task Release(SwitchButton b, int delay, CancellationToken token)
        {
            await SwitchConnection.SendAsync(SwitchCommand.Release(b, true), token).ConfigureAwait(false);
            await Task.Delay(delay, token).ConfigureAwait(false);
        }

        private async Task ReturnFromStation(CancellationToken token)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            Log($"Returning to entrance from {Settings.LocationSelection}..");
            await Click(Y, 2_500, token).ConfigureAwait(false);
            await Click(ZR, 1_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 5000, Settings.LocationSelection is Location.SecretCave ? 0_550 : Settings.LocationSelection is Location.ResearchStation4 ? 0_250 : 0_450, token).ConfigureAwait(false); // reposition to fly point
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            for (int i = 0; i < 4; i++)
                await Click(A, 1_000, token).ConfigureAwait(false);
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false)) // fly animation
                await Click(A, 6_000, token).ConfigureAwait(false);

            await Task.Delay(2_000, token).ConfigureAwait(false);
        }

        private async Task NavigateToAreaZeroEntrance(CancellationToken token)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            Log("Navigating to Area Zero Entrance..");
            await Click(Y, 2_500, token).ConfigureAwait(false);
            await Click(ZR, 1_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 5000, 0_250, token).ConfigureAwait(false); // reposition to fly point
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(2_500, token).ConfigureAwait(false);
            for (int i = 0; i < 4; i++)
                await Click(A, 0_500, token).ConfigureAwait(false);
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false)) // fly animation
                await Click(A, 6_000, token).ConfigureAwait(false);
        }

        private async Task RepositionToGate(CancellationToken token)
        {
            Log("Facing toward Area Zero Gate..");
            await Task.Delay(2_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -32000, 0_800, token).ConfigureAwait(false); // Face down 
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            await Click(L, 1_000, token).ConfigureAwait(false); //recenter
            await SetStick(LEFT, 0, 32000, 3_000, token).ConfigureAwait(false); // walk up
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            await SetStick(RIGHT, 30000, 0, 0_300, token).ConfigureAwait(false); // reposition to fly point
            await SetStick(RIGHT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 32000, 6_000, token).ConfigureAwait(false); // walk up
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            Log("Entering Area Zero Gate now..");
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Task.Delay(1_500, token).ConfigureAwait(false);
        }

        private async Task NavigateToAreaZeroPicnic(CancellationToken token)
        {
            Log("Navigating to Area Zero Picnic Location..");
            await Task.Delay(0_500, token).ConfigureAwait(false);
            await Click(L, 1_000, token).ConfigureAwait(false); //recenter
            await SetStick(LEFT, 0, 32000, 3_000, token).ConfigureAwait(false); // walk up
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
        }

        private async Task NavigateToResearchStation(CancellationToken token)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            Log($"Navigating to {Settings.LocationSelection} Location..");
            await SetStick(LEFT, 0, 32000, 4_000, token).ConfigureAwait(false); // walk up to gate entrance
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            await Task.Delay(2_000, token).ConfigureAwait(false); // Walk up to portal

            await Click(A, 1_500, token).ConfigureAwait(false);

            for (int i = 0; i < (int)Settings.LocationSelection - 1; i++)
                await Click(DDOWN, 0_500, token).ConfigureAwait(false);
            if (Settings.LocationSelection is Location.SecretCave)
            {
                for (int i = 0; i < 3; i++)
                    await Click(DDOWN, 0_500, token).ConfigureAwait(false);
            }

            await Click(A, 1_000, token).ConfigureAwait(false); // wait to load to new station
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Task.Delay(1_500, token).ConfigureAwait(false);

            await SetStick(LEFT, -32000, 0, (int)Settings.LocationSelection is not 4 ? 0_500 : 0_300, token).ConfigureAwait(false); // Face left
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

            await Task.Delay(1_000, token).ConfigureAwait(false);

            await SetStick(LEFT, 0, -32000, 2_800, token).ConfigureAwait(false); // Face down 
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Task.Delay(2_500, token).ConfigureAwait(false);
        }

        private async Task EnterExitStation(CancellationToken token)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            await Click(B, 2_000, token).ConfigureAwait(false);
            for (int i = 0; i < 4; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);
            Log($"Entering {Settings.LocationSelection}..");
            await SetStick(LEFT, 0, -32000, 3_000, token).ConfigureAwait(false); // walk down
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);
            Log($"Waiting to load in..");
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Task.Delay(1_500, token).ConfigureAwait(false);

            Log($"Exiting {Settings.LocationSelection}..");
            await SetStick(LEFT, 0, -32000, 3_000, token).ConfigureAwait(false); // walk down
            await SetStick(LEFT, 0, 0, 0, token).ConfigureAwait(false);

            Log($"Waiting to load out..");
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Task.Delay(2_500, token).ConfigureAwait(false);
        }

        private readonly string CaveX = $"452479FD";
        private readonly string CaveY = $"C3AE1C05";
        private readonly string CaveZ = $"C50FDB92";

        private readonly string SpotX = $"45201840";
        private readonly string SpotY = $"C3AE8DFC";
        private readonly string SpotZ = $"C51143CB";

        private async Task CollideToCave(CancellationToken token)
        {
            await Task.Delay(0_050, token).ConfigureAwait(false);
            uint coordx = uint.Parse(CaveX, NumberStyles.AllowHexSpecifier);
            byte[] X1 = BitConverter.GetBytes(coordx);
            uint coordy = uint.Parse(CaveY, NumberStyles.AllowHexSpecifier);
            byte[] Y1 = BitConverter.GetBytes(coordy);
            uint coordz = uint.Parse(CaveZ, NumberStyles.AllowHexSpecifier);
            byte[] Z1 = BitConverter.GetBytes(coordz);

            X1 = X1.Concat(Y1).Concat(Z1).ToArray();
            float y = BitConverter.ToSingle(X1, 4);
            y += 20;
            WriteSingleLittleEndian(X1.AsSpan()[4..], y);

            Log("Navigating back to the cave..");
            for (int i = 0; i < 15; i++)
                await SwitchConnection.PointerPoke(X1, Offsets.CollisionPointer, token).ConfigureAwait(false);

            await Task.Delay(3_000, token).ConfigureAwait(false);
        }

        private async Task CollideToTheSpot(CancellationToken token)
        {
            uint coordx = uint.Parse(SpotX, NumberStyles.AllowHexSpecifier);
            byte[] X1 = BitConverter.GetBytes(coordx);
            uint coordy = uint.Parse(SpotY, NumberStyles.AllowHexSpecifier);
            byte[] Y1 = BitConverter.GetBytes(coordy);
            uint coordz = uint.Parse(SpotZ, NumberStyles.AllowHexSpecifier);
            byte[] Z1 = BitConverter.GetBytes(coordz);

            X1 = X1.Concat(Y1).Concat(Z1).ToArray();
            float Y = BitConverter.ToSingle(X1, 4);
            Y += 25;
            WriteSingleLittleEndian(X1.AsSpan()[4..], Y);

            Log("Navigating to despawn spawns..");
            await Click(B, 2_500, token).ConfigureAwait(false);
            for (int i = 0; i < 15; i++)
                await SwitchConnection.PointerPoke(X1, Offsets.CollisionPointer, token).ConfigureAwait(false);

            await Task.Delay(3_000, token).ConfigureAwait(false);
        }

        public async Task DownUp(CancellationToken token)
        {
            var ydown = (ushort)Hub.Config.OverworldSV.MovementFilters.MoveDownMs;
            var yup = (ushort)Hub.Config.OverworldSV.MovementFilters.MoveUpMs;

            await Click(B, 1_500, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -32323, ydown, token).ConfigureAwait(false); //↓
            await SetStick(LEFT, 0, 0, 0_500, token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 32323, yup, token).ConfigureAwait(false); // ↑
            await SetStick(LEFT, 0, 0, 0_500, token).ConfigureAwait(false);
            await Task.Delay(0_250, token).ConfigureAwait(false);
            await Click(X, 2_500, token).ConfigureAwait(false);
        }

        private async Task RolloverCorrectionSV(CancellationToken token, bool time)
        {
            Log("Applying rollover correction.");
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

            if (Settings.RolloverFilters.UseOvershoot)
            {
                await PressAndHold(DDOWN, Settings.RolloverFilters.HoldTimeForRollover, 1_000, token).ConfigureAwait(false);
                await Click(DUP, 0_500, token).ConfigureAwait(false);
            }
            else if (!Settings.RolloverFilters.UseOvershoot)
            {
                for (int i = 0; i < 39; i++)
                    await Click(DDOWN, 0_100, token).ConfigureAwait(false);
            }

            await Click(A, 1_250, token).ConfigureAwait(false);
            for (int i = 0; i < 2; i++)
                await Click(DDOWN, 0_150, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);

            if (!time)
            {
                for (int i = 0; i < scrollroll; i++) // 0 to roll day for DDMMYY, 1 to roll day for MMDDYY, 3 to roll hour
                    await Click(DRIGHT, 0_200, token).ConfigureAwait(false);
            }
            else if (time)
            {
                for (int i = 0; i < 3; i++)
                    await Click(DRIGHT, 0_200, token).ConfigureAwait(false);
            }

            await Click(DDOWN, 0_200, token).ConfigureAwait(false);

            for (int i = 0; i < 8; i++) // Mash DRIGHT to confirm
                await Click(DRIGHT, 0_200, token).ConfigureAwait(false);

            await Click(A, 0_500, token).ConfigureAwait(false); // Confirm date/time change
            await Click(HOME, 1_000, token).ConfigureAwait(false); // Back to title screen
            Log("Done.");
        }
    }
}
