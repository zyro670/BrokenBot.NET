namespace SysBot.Pokemon;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;
using Base;
using static Base.SwitchButton;
using static Base.SwitchStick;

public class EggBotSVV2 : PokeRoutineExecutor9SV, IEncounterBot
{
    private readonly PokeTradeHub<PK9> _hub;
    private readonly IDumper _dumpSetting;
    private readonly EggSettingsSV _settings;
    public ICountSettings Counts => _settings;

    private const int WaitBetweenCollecting = 1; // Seconds
    private static readonly PK9 Blank = new();

    private const int InjectBox = 0;
    private int _injectSlot;

    private int _eggCount;

    public EggBotSVV2(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
    {
        _hub = hub;
        _settings = _hub.Config.EggSV;
        _dumpSetting = _hub.Config.Folder;
    }

    public void Acknowledge() => throw new NotImplementedException();

    public override async Task MainLoop(CancellationToken token)
    {
        if (_dumpSetting.Dump && string.IsNullOrWhiteSpace(_dumpSetting.DumpFolder))
        {
            Log("Check you settings, dump is enabled, but no folder set!");
            Log($"Ending {nameof(EggBotSVV2)} loop.");
            await HardStop().ConfigureAwait(false);

            return;
        }

        if (!Directory.Exists(_dumpSetting.DumpFolder))
        {
            Log($"Check you settings, dump is enabled, but folder [{_dumpSetting.DumpFolder}] doesn't exist!");
            Log($"Ending {nameof(EggBotSVV2)} loop.");
            await HardStop().ConfigureAwait(false);

            return;
        }

        await InitializeHardware(_hub.Config.EggSV, token).ConfigureAwait(false);

        Log("Identifying trainer data of the host console.");
        await IdentifyTrainer(token).ConfigureAwait(false);
        await SetupBoxState(token).ConfigureAwait(false);

        Log("Starting main EggBotV2 loop.");
        Config.IterateNextRoutine();
        while (!token.IsCancellationRequested && Config.NextRoutineType == PokeRoutineType.EggFetchV2)
        {
            try
            {
                if (!await InnerLoop(token).ConfigureAwait(false))
                    break;
            }
            catch (Exception e)
            {
                Log(e.Message);
                break;
            }
        }

        Log($"Ending {nameof(EggBotSVV2)} loop.");
        await HardStop().ConfigureAwait(false);
    }

    public override async Task HardStop()
    {
        // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
        await SetStick(LEFT, 0, 0, 0, CancellationToken.None).ConfigureAwait(false); // reset
        await CleanExit(CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Return true if we need to stop looping.
    /// </summary>
    private async Task<bool> InnerLoop(CancellationToken token)
    {
        Log("Starting inner loop");

        PK9? previousPk9 = null;

        // Start with initial presses
        await Click(A, 250, token);
        await Click(A, 250, token);

        var eggsPerBatch = 0;
        while (!token.IsCancellationRequested)
        {
            // Mash A button
            await Click(A, 150, token);

            if (eggsPerBatch >= 10)
            {
                eggsPerBatch = 0;
                for (int i = 0; i < 20; i++)
                {
                    await Click(B, 150, token);
                }

                Log($"Waiting [{WaitBetweenCollecting}] second(s) for picnic basket to fill");
                for (int i = 0; i < 10; i++)
                    await Click(B, 100, token);

                await Task.Delay(TimeSpan.FromSeconds(WaitBetweenCollecting), token);
            }

            // Validate result
            var (b1s1, bytes) = await ReadRawBoxPokemon(InjectBox, _injectSlot, token).ConfigureAwait(false);

            if (previousPk9?.EncryptionConstant != b1s1.EncryptionConstant && (Species)b1s1.Species != Species.None)
            {
                _eggCount++;
                var print = _hub.Config.StopConditions.GetSpecialPrintName(b1s1);
                var size = PokeSizeDetailedUtil.GetSizeRating(b1s1.Scale);
                Log($"Encounter: {_eggCount} (basket: {eggsPerBatch})");
                Log($"{print}{Environment.NewLine}Scale: {size}", b1s1.IsShiny);
                _settings.AddCompletedEggs();
                TradeExtensions<PK9>.EncounterLogs(b1s1, "EncounterLogPretty_EggSV.txt");
                TradeExtensions<PK9>.EncounterScaleLogs(b1s1, "EncounterLogScalePretty.txt");

                switch (_dumpSetting)
                {
                    case { Dump: true, DumpShinyOnly: true } when b1s1.IsShiny:
                    case { Dump: true, DumpShinyOnly: false }:
                        DumpPokemon(_dumpSetting.DumpFolder, "eggs", b1s1);
                        break;
                }

                switch (_dumpSetting)
                {
                    case { DumpRaw: true, DumpShinyOnly: true } when b1s1.IsShiny:
                    case { DumpRaw: true, DumpShinyOnly: false }:
                        DumpPokemon(_dumpSetting.DumpFolder, "eggs", b1s1, bytes);
                        break;
                }

                if (!await CheckEncounter(print, b1s1).ConfigureAwait(false))
                    return false;

                Log($"Clearing destination slot (B{InjectBox + 1}S{_injectSlot + 1}) for next Egg.", false);
                await SetBoxPokemonEgg(Blank, InjectBox, _injectSlot, token).ConfigureAwait(false);

                previousPk9 = b1s1;
                eggsPerBatch++;
            }
        }

        return true;
    }

    private async Task<bool> CheckEncounter(string print, PK9 pk)
    {
        var token = CancellationToken.None;

        if (!StopConditionSettings.EncounterFound(pk, _hub.Config.StopConditions, null))
        {
            if (_hub.Config.StopConditions.ShinyTarget is TargetShinyType.AnyShiny or TargetShinyType.StarOnly or TargetShinyType.SquareOnly && pk.IsShiny)
                _hub.LogEmbed(pk, false);

            return true; //No match, return true to keep scanning
        }

        if (_settings.MinMaxScaleOnly && pk.Scale is > 0 and < 255)
        {
            _hub.LogEmbed(pk, false);
            return true;
        }

        // no need to take a video clip of us receiving an egg.
        var mode = _settings.ContinueAfterMatch;
        var msg = $"Result found!\n{print}\n" + mode switch
        {
            ContinueAfterMatch.Continue => "Continuing..",
            ContinueAfterMatch.StopExit => "Stopping routine execution; restart the bot to search again.",
            _ => string.Empty,
        };

        if (!string.IsNullOrWhiteSpace(_hub.Config.StopConditions.MatchFoundEchoMention))
            msg = $"{_hub.Config.StopConditions.MatchFoundEchoMention} {msg}";

        Log(print);

        if (_settings.OneInOneHundredOnly)
        {
            if ((Species)pk.Species is Species.Dunsparce or Species.Tandemaus && pk.EncryptionConstant % 100 != 0)
            {
                _hub.LogEmbed(pk, false);
                return true; // 1/100 condition unsatisfied, continue scanning
            }
        }

        _hub.LogEmbed(pk, true);
        EchoUtil.Echo(msg);

        Log($"You're egg has been claimed and placed in B{InjectBox + 1}S{_injectSlot + 1}. Be sure to save your game!");

        if (mode == ContinueAfterMatch.Continue)
        {
            if (_injectSlot < 29)
            {
                Log("Continuing collecting eggs");
                _injectSlot += 1;
                return true;
            }

            Log($"Stop collecting eggs, maximum slot count of box 1 reached, slot: {_injectSlot}");
            await Click(HOME, 0_700, token).ConfigureAwait(false);
        }
        else
            await Click(HOME, 0_700, token).ConfigureAwait(false);

        return false;
    }

    private async Task SetupBoxState(CancellationToken token)
    {
        await SetCurrentBox(0, token).ConfigureAwait(false);

        var existing = await ReadBoxPokemon(InjectBox, _injectSlot, token).ConfigureAwait(false);
        if (existing.Species != 0 && existing.ChecksumValid)
        {
            Log("Destination slot is occupied! Dumping the Pokémon found there...");
            DumpPokemon(_dumpSetting.DumpFolder, "saved", existing);
        }

        Log("Clearing destination slot to start the bot.");
        await SetBoxPokemonEgg(Blank, InjectBox, _injectSlot, token).ConfigureAwait(false);
    }
}
