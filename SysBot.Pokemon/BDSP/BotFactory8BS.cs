using System;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class BotFactory8BS : BotFactory<PB8>
    {
        public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PB8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Dump
                or PokeRoutineType.FixOT
                or PokeRoutineType.TradeCord
                => new PokeTradeBotBS(Hub, cfg),

            PokeRoutineType.RemoteControl => new RemoteControlBot(cfg),

            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Dump
                or PokeRoutineType.FixOT
                or PokeRoutineType.TradeCord
                => true,

            PokeRoutineType.RemoteControl => true,

            _ => false,
        };
    }
}
