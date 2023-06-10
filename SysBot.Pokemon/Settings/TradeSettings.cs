﻿using System.Collections.Generic;
using PKHeX.Core;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class TradeSettings : IBotStateSettings, ICountSettings
    {
        private const string TradeCode = nameof(TradeCode);
        private const string TradeConfig = nameof(TradeConfig);
        private const string Dumping = nameof(Dumping);
        private const string Counts = nameof(Counts);
        public override string ToString() => "Trade Bot Settings";

        [Category(TradeConfig), Description("Time to wait for a trade partner in seconds.")]
        public int TradeWaitTime { get; set; } = 30;

        [Category(TradeConfig), Description("Max amount of time in seconds pressing A to wait for a trade to process.")]
        public int MaxTradeConfirmTime { get; set; } = 25;

        [Category(TradeCode), Description("Minimum Link Code.")]
        public int MinTradeCode { get; set; } = 8180;

        [Category(TradeCode), Description("Maximum Link Code.")]
        public int MaxTradeCode { get; set; } = 8199;

        [Category(Dumping), Description("Link Trade: Dumping routine will stop after a maximum number of dumps from a single user.")]
        public int MaxDumpsPerTrade { get; set; } = 20;

        [Category(Dumping), Description("Link Trade: Dumping routine will stop after spending x seconds in trade.")]
        public int MaxDumpTradeTime { get; set; } = 180;

        [Category(Dumping), Description("Dump Trade: If enabled, Dumping routine will output legality check information to the user.")]
        public bool DumpTradeLegalityCheck { get; set; } = true;

        [Category(TradeConfig), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        [Category(TradeConfig), Description("Max amount of time pressing A to wait for a trade to end before trying to exit to overworld.")]
        public int TradeAnimationMaxDelaySeconds { get; set; } = 90; // 150 maybe

        [Category(TradeConfig), Description("Spin while waiting for trade partner.")]
        public bool SpinTrade { get; set; } = false;

        [Category(TradeConfig), Description("Display all trades.")]
        public bool TradeDisplay { get; set; } = true;

        [Category(TradeConfig), Description("Select default species for \"ItemTrade\", if configured.")]
        public Species ItemTradeSpecies { get; set; } = Species.None;

        [Category(TradeConfig), Description("Silly, useless feature to post a meme when certain illegal or disallowed trade requests are made.")]
        public bool Memes { get; set; } = false;

        [Category(TradeConfig), Description("Enter either direct picture or gif links, or file names with extensions. For example, file1.png, file2.jpg, etc.")]
        public string MemeFileNames { get; set; } = string.Empty;

        /// <summary>
        /// Gets a random trade code based on the range settings.
        /// </summary>
        public int GetRandomTradeCode() => Util.Rand.Next(MinTradeCode, MaxTradeCode + 1);

        private int _completedSurprise;
        private int _completedDistribution;
        private int _completedTrades;
        private int _completedSeedChecks;
        private int _completedClones;
        private int _completedDumps;
        private int _completedEtumrepDumps;
        private int _completedFixOTs;
        private int _completedSupportTrades;
        private int _completedDisplays;

        [Category(Counts), Description("Completed Surprise Trades")]
        public int CompletedSurprise
        {
            get => _completedSurprise;
            set => _completedSurprise = value;
        }

        [Category(Counts), Description("Completed Link Trades (Distribution)")]
        public int CompletedDistribution
        {
            get => _completedDistribution;
            set => _completedDistribution = value;
        }

        [Category(Counts), Description("Completed Link Trades (Specific User)")]
        public int CompletedTrades
        {
            get => _completedTrades;
            set => _completedTrades = value;
        }

        [Category(Counts), Description("Completed Seed Check Trades")]
        public int CompletedSeedChecks
        {
            get => _completedSeedChecks;
            set => _completedSeedChecks = value;
        }

        [Category(Counts), Description("Completed Clone Trades (Specific User)")]
        public int CompletedClones
        {
            get => _completedClones;
            set => _completedClones = value;
        }

        [Category(Counts), Description("Completed Dump Trades (Specific User)")]
        public int CompletedDumps
        {
            get => _completedDumps;
            set => _completedDumps = value;
        }

        [Category(Counts), Description("Completed Etumrep Dump Trades (Specific User)")]
        public int CompletedEtumrepDumps
        {
            get => _completedEtumrepDumps;
            set => _completedEtumrepDumps = value;
        }

        [Category(Counts), Description("Completed FixOT Trades (Specific User)")]
        public int CompletedFixOTs
        {
            get => _completedFixOTs;
            set => _completedFixOTs = value;
        }

        [Category(Counts), Description("Completed Support Trades (Specific User)")]
        public int CompletedSupportTrades
        {
            get => _completedSupportTrades;
            set => _completedSupportTrades = value;
        }

        [Category(Counts), Description("Completed Display Trades (Specific User)")]
        public int CompletedDisplays
        {
            get => _completedDisplays;
            set => _completedDisplays = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public void AddCompletedTrade() => Interlocked.Increment(ref _completedTrades);
        public void AddCompletedSeedCheck() => Interlocked.Increment(ref _completedSeedChecks);
        public void AddCompletedSurprise() =>Interlocked.Increment(ref _completedSurprise);
        public void AddCompletedDistribution() => Interlocked.Increment(ref _completedDistribution);
        public void AddCompletedDumps() => Interlocked.Increment(ref _completedDumps);
        public void AddCompletedDisplays() => Interlocked.Increment(ref _completedDisplays);
        public void AddCompletedEtumrepDumps() => Interlocked.Increment(ref _completedEtumrepDumps);
        public void AddCompletedClones() => Interlocked.Increment(ref _completedClones);
        public void AddCompletedFixOTs() => Interlocked.Increment(ref _completedFixOTs);
        public void AddCompletedSupportTrades() => Interlocked.Increment(ref _completedSupportTrades);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedSeedChecks != 0)
                yield return $"Seed Check Trades: {CompletedSeedChecks}";
            if (CompletedClones != 0)
                yield return $"Clone Trades: {CompletedClones}";
            if (CompletedDumps != 0)
                yield return $"Dump Trades: {CompletedDumps}";
            if (CompletedTrades != 0)
                yield return $"Link Trades: {CompletedTrades}";
            if (CompletedDistribution != 0)
                yield return $"Distribution Trades: {CompletedDistribution}";
            if (CompletedSurprise != 0)
                yield return $"Surprise Trades: {CompletedSurprise}";
            if (CompletedEtumrepDumps != 0)
                yield return $"Etumrep Dump Trades: {CompletedEtumrepDumps}";
            if (CompletedFixOTs != 0)
                yield return $"FixOT Trades: {CompletedFixOTs}";
            if (CompletedSupportTrades != 0)
                yield return $"Support Trades: {CompletedSupportTrades}";
            if (CompletedDisplays != 0)
                yield return $"Display Trades: {CompletedDisplays}";
        }
    }
}
