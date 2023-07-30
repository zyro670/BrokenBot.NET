using PKHeX.Core;
using SysBot.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon
{
    public abstract class EncounterBotSV : PokeRoutineExecutor9SV, IEncounterBot
    {
        protected readonly PokeTradeHub<PK9> _hub;
        protected readonly IDumper _dumpSetting;

        protected readonly EncounterSettingsSV _settings;
        public ICountSettings Counts => _settings;

        protected const int InjectBox = 0;
        protected const int InjectSlot = 0;
        protected static readonly PK9 Blank = new();

        protected EncounterBotSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : base(cfg)
        {
            _hub = hub;
            _settings = _hub.Config.EncounterSV;
            _dumpSetting = _hub.Config.Folder;
        }

        private bool _isWaiting;
        public void Acknowledge() => _isWaiting = false;

        public override async Task HardStop()
        {
            // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
            await SetStick(LEFT, 0, 0, 0, CancellationToken.None).ConfigureAwait(false); // reset
            await CleanExit(CancellationToken.None).ConfigureAwait(false);
        }

        public override async Task MainLoop(CancellationToken token)
        {
            await InitializeHardware(_hub.Config.EncounterSV, token).ConfigureAwait(false);

            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);
            await SetupBoxState(token).ConfigureAwait(false);

            Log($"Starting main {nameof(EncounterBotSV)}/{nameof(Config.NextRoutineType)} loop.");
            Config.IterateNextRoutine();
            while (!token.IsCancellationRequested
                && (Config.NextRoutineType == PokeRoutineType.EncounterCapture || 
                Config.NextRoutineType == PokeRoutineType.EncounterRuinous || 
                Config.NextRoutineType == PokeRoutineType.EncounterGimmighoul)
                )
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

            Log($"Ending {nameof(EncounterBotSV)} loop.");
            await HardStop().ConfigureAwait(false);
        }

        public abstract Task<bool> InnerLoop(CancellationToken token);

        protected async Task SetupBoxState(CancellationToken token)
        {
            await SetCurrentBox(0, token).ConfigureAwait(false);

            var existing = await ReadBoxPokemon(InjectBox, InjectSlot, token).ConfigureAwait(false);
            if (existing.Species != 0 && existing.ChecksumValid)
            {
                Log("Destination slot is occupied! Dumping the Pokémon found there...");
                DumpPokemon(_dumpSetting.DumpFolder, "saved", existing);
            }

            Log("Clearing destination slot to start the bot.", false);
            await SetBoxPokemonEgg(Blank, InjectBox, InjectSlot, token).ConfigureAwait(false);
        }

        protected async Task EnableAlwaysCatch(CancellationToken token)
        {
            if (!_hub.Config.EncounterSV.EnableCatchCheat)
                return;

            Log("Enable critical capture cheat", false);
            // Source: https://gbatemp.net/threads/pokemon-scarlet-violet-cheat-database.621563/

            // Original cheat:
            /*
             * [100% Fast capture on(v1.3.2)]
             * 04000000 01857FE8 52800028
             * 04000000 01857FF4 14000020
             * 04000000 0185804C 52800028
             * 04000000 01858084 52800028
             */

            await SwitchConnection.WriteBytesMainAsync(BitConverter.GetBytes(0x52800028), 0x01857FE8, token);
            await SwitchConnection.WriteBytesMainAsync(BitConverter.GetBytes(0x14000020), 0x01857FF4, token);
            await SwitchConnection.WriteBytesMainAsync(BitConverter.GetBytes(0x52800028), 0x0185804C, token);
            await SwitchConnection.WriteBytesMainAsync(BitConverter.GetBytes(0x52800028), 0x01858084, token);
        }

        protected async Task<bool> CheckEncounter(string print, PK9 pk)
        {
            var token = CancellationToken.None;

            if (!StopConditionSettings.EncounterFound(pk, _hub.Config.StopConditions, null))
            {
                if (_hub.Config.StopConditions.ShinyTarget is TargetShinyType.AnyShiny or TargetShinyType.StarOnly or TargetShinyType.SquareOnly && pk.IsShiny)
                    _hub.LogEmbed(pk, false);

                return true; //No match, return true to keep scanning
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

            _hub.LogEmbed(pk, true);
            EchoUtil.Echo(msg);

            Log("You're Pokémon has been catched and placed in B1S1. Be sure to save your game!");

            if (mode == ContinueAfterMatch.Continue)
                return true;

            await Click(HOME, 0_700, token).ConfigureAwait(false);

            return false;
        }
    }
}
