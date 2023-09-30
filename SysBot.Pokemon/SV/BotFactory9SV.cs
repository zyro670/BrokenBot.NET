using PKHeX.Core;
using System;

namespace SysBot.Pokemon
{
    public class BotFactory9SV : BotFactory<PK9>
    {
        public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PK9> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.Display
                or PokeRoutineType.TradeCord
                => new PokeTradeBotSV(Hub, cfg),

            PokeRoutineType.OverworldBot => new OverworldBotSV(cfg, Hub),
            PokeRoutineType.EggFetch => new EggBotSV(cfg, Hub),
            PokeRoutineType.RaidBot => new RaidBotSV(cfg, Hub),
            PokeRoutineType.RotatingRaidBot => new RotatingRaidBotSV(cfg, Hub),
            PokeRoutineType.TIDResetBot => new TIDResetBotSV(cfg, Hub),
            PokeRoutineType.RemoteControl => new RemoteControlBotSV(cfg),
            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.Display
                or PokeRoutineType.TradeCord
                => true,

            PokeRoutineType.OverworldBot => true,
            PokeRoutineType.EggFetch => true,
            PokeRoutineType.RaidBot => true,
            PokeRoutineType.RotatingRaidBot => true,
            PokeRoutineType.TIDResetBot => true,
            PokeRoutineType.RemoteControl => true,

            _ => false,
        };
    }
}
