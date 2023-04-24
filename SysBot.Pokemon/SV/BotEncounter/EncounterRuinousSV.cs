using PKHeX.Core;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;

namespace SysBot.Pokemon
{
    public class EncounterRuinousSV : EncounterBotSV
    {
        private int _encounterCount;

        public EncounterRuinousSV(PokeBotState cfg, PokeTradeHub<PK9> hub)
            : base(cfg, hub)
        {
        }

        public override async Task<bool> InnerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();

                await SetupBoxState(token);
                await EnableAlwaysCatch(token).ConfigureAwait(false);

                Log("Start battle with Legend");
                var later = DateTime.Now.AddSeconds(18);
                Log($"Press A till [{later}]", false);
                while (DateTime.Now <= later)
                    await Click(A, 200, token);

                later = DateTime.Now.AddSeconds(18);
                Log($"Press B till [{later}]", false);
                while (DateTime.Now <= later)
                    await Click(B, 200, token);

                Log("Catch using default ball");
                await Click(X, 750, token);
                await Click(A, 7_500, token);

                Log("Exit battle", false);
                PK9? b1s1 = null;
                byte[]? bytes;

                while (b1s1 == null || (Species)b1s1.Species == Species.None)
                {
                    (b1s1, bytes) = await ReadRawBoxPokemon(InjectBox, InjectSlot, token).ConfigureAwait(false);

                    if (b1s1 != null && b1s1.EncryptionConstant != null && (Species)b1s1.Species != Species.None)
                    {
                        _encounterCount++;
                        var print = _hub.Config.StopConditions.GetSpecialPrintName(b1s1);
                        var size = PokeSizeDetailedUtil.GetSizeRating(b1s1.Scale);
                        Log($"Encounter: {_encounterCount}");
                        Log($"{print}{Environment.NewLine}Scale: {size}");
                        _settings.AddCompletedLegends();
                        TradeExtensions<PK9>.EncounterLogs(b1s1, "EncounterLogPretty_LegendSV.txt");
                        TradeExtensions<PK9>.EncounterScaleLogs(b1s1, "EncounterLogScalePretty.txt");

                        if (_dumpSetting.Dump)
                            DumpPokemon(_dumpSetting.DumpFolder, "legends", b1s1, true);

                        if (_dumpSetting.DumpRaw && bytes != null)
                            DumpPokemon(_dumpSetting.DumpFolder, "legends", b1s1, bytes);

                        if (!await CheckEncounter(print, b1s1).ConfigureAwait(false))
                            return false;
                    }

                    await Click(B, 200, token).ConfigureAwait(false);
                }

                Log("Resetting game for a new Legend");
                await ReOpenGame(_hub.Config, token).ConfigureAwait(false);
                Log($"Single encounter duration: [{sw.Elapsed}]", false);
            }

            return true;
        }
    }
}
