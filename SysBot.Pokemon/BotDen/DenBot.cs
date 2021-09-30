using PKHeX.Core;
using SysBot.Base;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon
{
    public class DenBot : PokeRoutineExecutor
    {
        private readonly PokeTradeHub<PK8> Hub;
        private readonly DenSettings Settings;
        private readonly RaidBot Raid;
        private DenUtil.RaidData RaidInfo = new();
        private ulong InitialSeed;
        private ulong DestinationSeed;
        private bool Stop;

        public DenBot(PokeBotState cfg, PokeTradeHub<PK8> hub, RaidBot raid) : base(cfg)
        {
            Hub = hub;
            Settings = hub.Config.Den;
            Raid = raid;
        }

        public override async Task MainLoop(CancellationToken token)
        {
            Log("Identifying trainer data of the host console.");
            RaidInfo.TrainerInfo = await IdentifyTrainer(token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                Config.IterateNextRoutine();
                var task = Config.CurrentRoutineType switch
                {
                    PokeRoutineType.DenBot => DoDenBot(token),
                    _ => Raid.RunAsync(token),
                };
                await task.ConfigureAwait(false);

                if (!Settings.HostAfterSkip || Stop)
                    return;
            }
        }

        private async Task DoDenBot(CancellationToken token)
        {
            async Task<bool> DenFunc(CancellationToken token)
            {
                if (Settings.Star < 0 || Settings.Star > 4)
                {
                    Log("Please enter a valid star count.");
                    return false;
                }
                else if (Settings.Randroll < 1 || Settings.Randroll > 100)
                {
                    Log("Please enter a valid randroll");
                    return false;
                }
                else if (Settings.SkipCount < 0)
                {
                    Log("Please enter a valid skip count.");
                    return false;
                }

                RaidInfo.Settings = Settings;
                var denData = await DenData(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, token).ConfigureAwait(false);
                RaidInfo.DenID = DenUtil.GetDenID(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType);

                var eventOfs = DenUtil.GetEventDenOffset((int)Hub.Config.ConsoleLanguage, RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, out _);
                var eventData = RaidInfo.Settings.DenBeamType == BeamType.Event ? await Connection.ReadBytesAsync(eventOfs, 0x23D4, token).ConfigureAwait(false) : new byte[] { };

                RaidInfo = DenUtil.GetRaid(RaidInfo, denData, eventData);
                Log("Starting main DenBot loop.");

                while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.DenBot)
                {
                    Config.IterateNextRoutine();
                    if (Settings.DenMode == DenMode.Skip)
                    {
                        var skips = Settings.SkipCount;
                        InitialSeed = RaidInfo.Den.Seed;
                        DestinationSeed = DenUtil.GetTargetSeed(RaidInfo.Den.Seed, skips);
                        Log($"\nInitial seed: {InitialSeed:X16}.\nDestination seed: {DestinationSeed:X16}.");
                        await PerformDaySkip(skips, token).ConfigureAwait(false);
                        if (!await SkipCorrection(skips, token).ConfigureAwait(false))
                            return false;

                        EchoUtil.Echo($"{(!Hub.Config.StopConditions.PingOnMatch.Equals(string.Empty) ? $"<@{Hub.Config.StopConditions.PingOnMatch}> " : "")}Skipping complete\n");
                    }
                    else if (Settings.DenMode == DenMode.SeedSearch)
                    {
                        PerformSeedSearch();
                        EchoUtil.Echo($"{(!Hub.Config.StopConditions.PingOnMatch.Equals(string.Empty) ? $"<@{Hub.Config.StopConditions.PingOnMatch}> " : "")}Seed search complete, stopping the bot.\n");
                    }
                    else
                    {
                        var seedstr = Settings.SeedToInject.StartsWith("0x") ? Settings.SeedToInject[2..] : Settings.SeedToInject;
                        Log("Attempting to inject the seed...");
                        denData[0x10] = (byte)Settings.Star;
                        denData[0x11] = (byte)Settings.Randroll;

                        if (ulong.TryParse(seedstr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong seedH))
                            BitConverter.GetBytes(seedH).CopyTo(denData, 0x8);
                        else
                        {
                            Log("Please enter a seed in hex format.");
                            return false;
                        }

                        denData[0x12] = RaidInfo.Den.IsEvent ? (byte)BeamType.CommonWish : (byte)Settings.DenBeamType;
                        denData[0x13] = (byte)(RaidInfo.Den.IsEvent ? 3 : 1);
                        await Connection.WriteBytesAsync(denData, DenUtil.GetDenOffset(RaidInfo.Settings.DenID, RaidInfo.Settings.DenType, out _), token).ConfigureAwait(false);
                        Log("\nSeed injected.");
                    }

                    if (Settings.HostAfterSkip && Settings.DenMode != DenMode.SeedSearch)
                    {
                        Config.Initialize(PokeRoutineType.RaidBot);
                        await SaveGame(Hub.Config, token).ConfigureAwait(false);
                        Log("\nInitializing RaidBot...");
                    }
                    else return false;
                }
                return true;
            }

            if (!await DenFunc(token).ConfigureAwait(false))
                Stop = true;
        }

        private Tuple<ulong, ulong> PerformSeedSearch()
        {
            Log("Searching for a matching seed... Search may take a while.");
            SeedSearchUtil.SpecificSeedSearch(RaidInfo, out long frames, out ulong seed, out ulong threeDay, out string ivSpread);
            if (ivSpread == string.Empty)
            {
                Log($"No results found within the specified search range.");
                return new Tuple<ulong, ulong>(0, 0);
            }

            var species = RaidInfo.Den.IsEvent ? RaidInfo.RaidDistributionEncounter.Species : RaidInfo.RaidEncounter.Species;
            var specName = SpeciesName.GetSpeciesNameGeneration((int)species, 2, 8);
            var form = TradeExtensions.FormOutput((int)(RaidInfo.Den.IsEvent ? RaidInfo.RaidDistributionEncounter.Species : RaidInfo.RaidEncounter.Species), (int)(RaidInfo.Den.IsEvent ? RaidInfo.RaidDistributionEncounter.AltForm : RaidInfo.RaidEncounter.AltForm), out _);
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
            while (remaining != 0)
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

            while (currentSeed != DestinationSeed)
            {
                skips = DenUtil.GetSkipsToTargetSeed(currentSeed, DestinationSeed, skips);
                if (skips > 0)
                {
                    Log($"Fell short by {skips} skips! Resuming skipping until destination seed is reached.");
                    await PerformDaySkip(skips, token).ConfigureAwait(false);
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
