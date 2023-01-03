﻿using System;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class BotFactory8LA : BotFactory<PA8>
    {
        public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PA8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.EtumrepDump
                or PokeRoutineType.FixOT
                => new PokeTradeBotLA(Hub, cfg),

            PokeRoutineType.ArceusBot => new ArceusBot(cfg, Hub),
            PokeRoutineType.RemoteControl => new RemoteControlBotLA(cfg),

            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.EtumrepDump
                or PokeRoutineType.FixOT
                => true,

            PokeRoutineType.ArceusBot => true,
            PokeRoutineType.RemoteControl => true,

            _ => false,
        };
    }
}
