using PKHeX.Core;
using SysBot.Base;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon
{
    public sealed class DenBot : EncounterBot
    {
        private readonly DenSettings Settings;
        private DenUtil.RaidData RaidInfo = new();
        private ulong InitialSeed;
        private ulong DestinationSeed;

        public DenBot(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg, hub)
        {
            Settings = hub.Config.Den;
        }

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            RaidInfo.TrainerInfo = sav;
            if (Settings.Star < 0 || Settings.Star > 4)
            {
                Log("Please enter a valid star count.");
                return;
            }
            else if (Settings.Randroll < 1 || Settings.Randroll > 100)
            {
                Log("Please enter a valid randroll");
                return;
            }
            else if (Settings.SkipCount < 0)
            {
                Log("Please enter a valid skip count.");
                return;
            }

            RaidInfo.Settings = Settings;
            var denBytes = await DenData(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, token).ConfigureAwait(false);
            RaidInfo.DenID = DenUtil.GetDenID(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType);

            var eventOfs = DenUtil.GetEventDenOffset((int)Hub.Config.ConsoleLanguage, RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, out _);
            var eventDenBytes = RaidInfo.Settings.DenBeamType == BeamType.Event ? await Connection.ReadBytesAsync(eventOfs, 0x23D4, token).ConfigureAwait(false) : new byte[] { };
            RaidInfo = DenUtil.GetRaid(RaidInfo, denBytes, eventDenBytes);

            while (!token.IsCancellationRequested)
            {
                if (Settings.DenMode == DenMode.Skip)
                {
                    var skips = Settings.SkipCount;
                    InitialSeed = RaidInfo.Den.Seed;
                    DestinationSeed = DenUtil.GetTargetSeed(RaidInfo.Den.Seed, skips);
                    Log($"\nInitial seed: {InitialSeed:X16}.\nDestination seed: {DestinationSeed:X16}.");
                    await PerformDaySkip(skips, token).ConfigureAwait(false);
                    if (!await SkipCorrection(skips, token).ConfigureAwait(false))
                        return;

                    EchoUtil.Echo($"{Hub.Config.StopConditions.MatchFoundEchoMention}Skipping complete, stopping the bot.\n");
                    return;
                }
                else if (Settings.DenMode == DenMode.SeedSearch)
                {
                    PerformSeedSearch(token);
                    if (token.IsCancellationRequested)
                        return;

                    EchoUtil.Echo($"{Hub.Config.StopConditions.MatchFoundEchoMention}Seed search complete, stopping the bot.\n");
                    return;
                }
                else
                {
                    var seedstr = Settings.SeedToInject.StartsWith("0x") ? Settings.SeedToInject[2..] : Settings.SeedToInject;
                    Log("Attempting to inject the seed...");
                    denBytes[0x10] = (byte)Settings.Star;
                    denBytes[0x11] = (byte)Settings.Randroll;

                    if (ulong.TryParse(seedstr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong seedH))
                        BitConverter.GetBytes(seedH).CopyTo(denBytes, 0x8);
                    else
                    {
                        Log("Please enter a seed in hex format.");
                        return;
                    }

                    denBytes[0x12] = RaidInfo.Den.IsEvent ? (byte)BeamType.CommonWish : (byte)Settings.DenBeamType;
                    denBytes[0x13] = (byte)(RaidInfo.Den.IsEvent ? 3 : 1);
                    await Connection.WriteBytesAsync(denBytes, DenUtil.GetDenOffset(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, out _), token).ConfigureAwait(false);
                    Log("Seed injected, stopping the bot.");
                    return;
                }
            }
        }

        private Tuple<ulong, ulong> PerformSeedSearch(CancellationToken token)
        {
            Log("Searching for a matching seed... Search may take a while.");
            SeedSearchUtil.SpecificSeedSearch(RaidInfo, out long frames, out ulong seed, out ulong threeDay, out string ivSpread, token);
            if (token.IsCancellationRequested)
                return new Tuple<ulong, ulong> (seed, threeDay);

            if (ivSpread == string.Empty)
            {
                Log($"No results found within the specified search range.");
                return new Tuple<ulong, ulong>(0, 0);
            }

            var species = RaidInfo.Den.IsEvent ? RaidInfo.RaidDistributionEncounter.Species : RaidInfo.RaidEncounter.Species;
            var specName = SpeciesName.GetSpeciesNameGeneration((int)species, 2, 8);
            var form = TradeExtensions<PK8>.FormOutput((int)(RaidInfo.Den.IsEvent ? RaidInfo.RaidDistributionEncounter.Species : RaidInfo.RaidEncounter.Species), (int)(RaidInfo.Den.IsEvent ? RaidInfo.RaidDistributionEncounter.AltForm : RaidInfo.RaidEncounter.AltForm), out _);
            var results = $"\n\nDesired species: {(uint)RaidInfo.Den.Stars + 1}★ - {specName}{form}\n" +
                          $"\n{ivSpread}\n" +
                          $"\nStarting seed: {RaidInfo.Den.Seed:X16}\n" +
                          $"Target frame seed: {seed:X16}\n" +
                          $"Three day roll: {threeDay:X16}\n" +
                          $"Skips to target frame: {frames:N0}\n";

            EchoUtil.Echo(results);
            return new Tuple<ulong, ulong>(seed, threeDay);
        }

        private async Task PerformDaySkip(int skips, CancellationToken token)
        {
            var timeRemaining = TimeSpan.FromMilliseconds((0_360 + Settings.SkipDelay) * skips);
            var firstQuarterLog = Math.Round(skips * 0.25, 0, MidpointRounding.ToEven);
            var halfLog = Math.Round(skips * 0.5, 0, MidpointRounding.ToEven);
            var lastQuarterLog = Math.Round(skips * 0.75, 0, MidpointRounding.ToEven);
            EchoUtil.Echo($"Beginning to skip {(skips > 1 ? $"{skips} frames" : "1 frame")}. Skipping should take around {(timeRemaining.Days == 0 ? "" : timeRemaining.Days + "d:")}{(timeRemaining.Hours == 0 ? "" : timeRemaining.Hours + "h:")}{(timeRemaining.Minutes == 0 ? "" : timeRemaining.Minutes + "m:")}{(timeRemaining.Seconds < 1 ? "1s" : timeRemaining.Seconds + "s")}.");

            int remaining = await SkipCheck(skips, 0, token).ConfigureAwait(false);
            while (remaining != 0 && !token.IsCancellationRequested)
            {
                if (remaining == firstQuarterLog | remaining == halfLog | remaining == lastQuarterLog)
                {
                    timeRemaining = TimeSpan.FromMilliseconds((0_360 + Settings.SkipDelay) * remaining);
                    Log($"{(remaining > 1 ? $"{remaining} skips" : "1 skip")} and around {(timeRemaining.Days == 0 ? "" : timeRemaining.Days + "d:")}{(timeRemaining.Hours == 0 ? "" : timeRemaining.Hours + "h:")}{(timeRemaining.Minutes == 0 ? "" : timeRemaining.Minutes + "m:")}{(timeRemaining.Seconds < 1 ? "1s" : timeRemaining.Seconds + "s")} left.");
                }

                await DaySkip(token).ConfigureAwait(false);
                await Task.Delay(0_360 + Settings.SkipDelay).ConfigureAwait(false);
                --remaining;
                if (remaining == lastQuarterLog || remaining + 3 == skips)
                    remaining = await SkipCheck(skips, remaining, token).ConfigureAwait(false);
            }

            if (token.IsCancellationRequested)
                return;

            if (Settings.TimeReset == TimeReset.Reset)
                await ResetTime(token).ConfigureAwait(false);
            else if (Settings.TimeReset == TimeReset.ResetNTP)
            {
                if (!await IsGameConnectedToYComm(token).ConfigureAwait(false))
                    Log("Syncing time via NTP requires internet connection. Connecting to YComm in order to ensure we're connected.");

                await EnsureConnectedToYComm(Hub.Config, token).ConfigureAwait(false);
                await ResetNTP(token).ConfigureAwait(false);
            }
        }

        private async Task<bool> SkipCorrection(int skips, CancellationToken token)
        {
            var currentSeed = new RaidSpawnDetail(await DenData(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, token).ConfigureAwait(false), 0).Seed;
            if (currentSeed == InitialSeed)
            {
                Log("No frames were skipped. Ensure \"Synchronize Clock via Internet\" is enabled, are using sys-botbase that allows time change, and haven't used anything that shifts RAM. \"SkipDelay\" may also need to be increased.");
                return false;
            }

            while (currentSeed != DestinationSeed && !token.IsCancellationRequested)
            {
                skips = DenUtil.GetSkipsToTargetSeed(currentSeed, DestinationSeed, skips);
                if (skips > 0)
                {
                    Log($"Fell short by {skips} skips! Resuming skipping until destination seed is reached.");
                    await PerformDaySkip(skips, token).ConfigureAwait(false);
                    if (token.IsCancellationRequested)
                        return false;
                    currentSeed = new RaidSpawnDetail(await DenData(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, token).ConfigureAwait(false), 0).Seed;
                }
                else if (skips < 0)
                {
                    Log("Date must have rolled while skipping. We have overskipped our target.");
                    return false;
                }
                else return true;
            }
            return true;
        }

        private async Task<int> SkipCheck(int skips, int skipsDone, CancellationToken token)
        {
            var currentSeed = new RaidSpawnDetail(await DenData(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, token).ConfigureAwait(false), 0).Seed;
            var remaining = DenUtil.GetSkipsToTargetSeed(currentSeed, DestinationSeed, skips);
            bool dateRolled = remaining < skips - skipsDone;
            if (dateRolled)
                return remaining + skipsDone;
            else return remaining;
        }

        private async Task<byte[]> DenData(uint id, DenType type, CancellationToken token) => await Connection.ReadBytesAsync(DenUtil.GetDenOffset(id, type, out _), 0x18, token).ConfigureAwait(false);
    }
}
