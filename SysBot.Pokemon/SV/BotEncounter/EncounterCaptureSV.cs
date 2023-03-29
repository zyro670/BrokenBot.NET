using PKHeX.Core;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon
{
    public class EncounterCaptureSV : EncounterBotSV
    {
        private int _encounterCount;

        public EncounterCaptureSV(PokeBotState cfg, PokeTradeHub<PK9> hub)
            : base(cfg, hub)
        {
        }

        public override async Task<bool> InnerLoop(CancellationToken token)
        {
            await SetupBoxState(token).ConfigureAwait(false);
            await EnableAlwaysCatch(token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                Log("Waiting for a catch...");

                PK9? b1s1 = null;
                byte[]? bytes;

                while (b1s1 == null || (Species)b1s1.Species == Species.None)
                {
                    b1s1 = await ReadBoxPokemon(InjectBox, InjectSlot, token).ConfigureAwait(false);
                    
                    if (b1s1 != null && b1s1.EncryptionConstant != null && (Species)b1s1.Species != Species.None)
                    {
                        _encounterCount++;
                        var print = _hub.Config.StopConditions.GetSpecialPrintName(b1s1);
                        var size = PokeSizeDetailedUtil.GetSizeRating(b1s1.Scale);
                        Log($"Encounter: {_encounterCount}");
                        Log($"{print}{Environment.NewLine}Scale: {size}");
                        _settings.AddCompletedEncounters();
                        TradeExtensions<PK9>.EncounterLogs(b1s1, "EncounterLogPretty_WildSV.txt");
                        TradeExtensions<PK9>.EncounterScaleLogs(b1s1, "EncounterLogScalePretty.txt");

                        if (_dumpSetting.Dump)
                            DumpPokemon(_dumpSetting.DumpFolder, "wild", b1s1, true);

                        if (!await CheckEncounter(print, b1s1).ConfigureAwait(false))
                            return false;
                    }

                    await Task.Delay(1000, token);
                }

                Log("Clearing destination slot for next Wild.", false);
                await SetBoxPokemonEgg(Blank, InjectBox, InjectSlot, token).ConfigureAwait(false);

                Log($"Single encounter duration: [{sw.Elapsed}]", false);
            }

            return true;
        }
    }
}
