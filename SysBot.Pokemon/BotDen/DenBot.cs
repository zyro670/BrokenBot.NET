using PKHeX.Core;
using SysBot.Base;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon
{
    public class DenBot : PokeRoutineExecutor
    {
        private readonly PokeTradeHub<PK8> Hub;
        private readonly DenSettings Settings;
        private readonly RaidBot Raid;
        private DenUtil.RaidData RaidInfo = new DenUtil.RaidData();
        private ulong InitialSeed;
        private ulong DestinationSeed;

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
                if (!Settings.HostAfterSkip || Settings.DenMode == DenMode.SeedSearch)
                    return;
            }
        }

        private async Task DoDenBot(CancellationToken token)
        {
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
            var denData = await DenData(Hub, token).ConfigureAwait(false);
            RaidInfo = DenUtil.GetRaid(RaidInfo, denData);
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
                        return;

                    EchoUtil.Echo($"{(!Hub.Config.StopConditions.PingOnMatch.Equals(string.Empty) ? $"<@{Hub.Config.StopConditions.PingOnMatch}> " : "")}Skipping complete\n");
                }
                else
                {
                    PerformSeedSearch();
                    EchoUtil.Echo($"{(!Hub.Config.StopConditions.PingOnMatch.Equals(string.Empty) ? $"<@{Hub.Config.StopConditions.PingOnMatch}> " : "")}Seed search complete, stopping the bot.\n");
                    return;
                }

                if (Settings.HostAfterSkip && Settings.DenMode != DenMode.SeedSearch)
                {
                    Config.Initialize(PokeRoutineType.RaidBot);
                    await SaveGame(Hub.Config, token).ConfigureAwait(false);
                    Log("\nInitializing RaidBot...");
                }
                else return;
            }
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
            int reset = Settings.TimeReset == TimeReset.Reset ? skips : 0;
            int resetNTP = Settings.TimeReset == TimeReset.ResetNTP ? 1 : 0;
            EchoUtil.Echo($"Beginning to skip {(skips > 1 ? $"{skips} frames" : "1 frame")}. Skipping should take around {(timeRemaining.Hours == 0 ? "" : timeRemaining.Hours + "h:")}{(timeRemaining.Minutes == 0 ? "" : timeRemaining.Minutes + "m:")}{(timeRemaining.Seconds < 1 ? "1s" : timeRemaining.Seconds + "s")}.");
            
            for (int i = 0; i < skips; i++)
            {
                if (i == firstQuarterLog | i == halfLog | i == lastQuarterLog)
                {
                    timeRemaining = TimeSpan.FromMilliseconds((0_360 + Settings.SkipDelay) * (skips - i));
                    Log($"{(skips - i > 1 ? $"{skips - i} skips" : "1 skip")} and around {(timeRemaining.Hours == 0 ? "" : timeRemaining.Hours + "h:")}{(timeRemaining.Minutes == 0 ? "" : timeRemaining.Minutes + "m:")}{(timeRemaining.Seconds < 1 ? "1s" : timeRemaining.Seconds + "s")} left.");
                }

                await DaySkip(reset, 0, token).ConfigureAwait(false);
                await Task.Delay(0_360 + Settings.SkipDelay).ConfigureAwait(false);
                if (i == lastQuarterLog || i + 2 == skips)
                    skips = await SkipCheck(skips, i, token).ConfigureAwait(false);
            }

            if (resetNTP == 1)
            {
                Log("Syncing time via NTP requires internet connection. Connecting to YComm in order to ensure we do.");
                await EnsureConnectedToYComm(Hub.Config, token).ConfigureAwait(false);
                await ResetNTP(token).ConfigureAwait(false);
            }
        }

        private async Task<bool> SkipCorrection(int skips, CancellationToken token)
        {
            var currentSeed = new RaidSpawnDetail(await DenData(Hub, token).ConfigureAwait(false), 0).Seed;
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
                    currentSeed = new RaidSpawnDetail(await DenData(Hub, token).ConfigureAwait(false), 0).Seed;
                }
                else if (skips < 0)
                {
                    Log($"Date must have rolled while skipping. We have overskipped our target.");
                    return false;
                }
                else return true;
            }
            return true;
        }

        private async Task<int> SkipCheck(int skips, int skipsDone, CancellationToken token)
        {
            var currentSeed = new RaidSpawnDetail(await DenData(Hub, token).ConfigureAwait(false), 0).Seed;
            var remaining = DenUtil.GetSkipsToTargetSeed(currentSeed, DestinationSeed, skips);
            bool dateRolled = remaining < skips - skipsDone;
            if (dateRolled)
                return remaining + skipsDone;
            else return skips;
        }

        private async Task<byte[]> DenData(PokeTradeHub<PK8> hub, CancellationToken token) => await Connection.ReadBytesAsync(DenUtil.GetDenOffset(hub), 0x18, token).ConfigureAwait(false);
    }
}
