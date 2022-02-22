﻿using System;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class BotFactory8 : BotFactory<PK8>
    {
        public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PK8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.SurpriseTrade
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.SeedCheck
                or PokeRoutineType.FixOT
                or PokeRoutineType.TradeCord
                => new PokeTradeBot(Hub, cfg),

            PokeRoutineType.EggFetch => new EggBot(cfg, Hub),
            PokeRoutineType.FossilBot => new FossilBot(cfg, Hub),
            PokeRoutineType.RaidBot => new RaidBot(cfg, Hub),
            PokeRoutineType.EncounterLine => new EncounterBotLine(cfg, Hub),
            PokeRoutineType.Reset => new EncounterBotReset(cfg, Hub),
            PokeRoutineType.Dogbot => new EncounterBotDog(cfg, Hub),
            PokeRoutineType.LairBot => new LairBot(cfg, Hub),
            PokeRoutineType.DenBot => new DenBot(cfg, Hub),
            PokeRoutineType.BoolBot => new BoolBot(cfg, Hub),
            PokeRoutineType.SoJCamp => new SoJCamp(cfg, Hub),
            PokeRoutineType.CurryBot => new CurryBot(cfg, Hub),
            PokeRoutineType.RollingRaid => new RollingRaidBot(cfg, Hub),

            PokeRoutineType.RemoteControl => new RemoteControlBot(cfg),
            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.SurpriseTrade
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                or PokeRoutineType.SeedCheck
                or PokeRoutineType.FixOT
                or PokeRoutineType.TradeCord
                => true,

            PokeRoutineType.EggFetch => true,
            PokeRoutineType.FossilBot => true,
            PokeRoutineType.RaidBot => true,
            PokeRoutineType.EncounterLine => true,
            PokeRoutineType.Reset => true,
            PokeRoutineType.Dogbot => true,
            PokeRoutineType.LairBot => true,
            PokeRoutineType.DenBot => true,
            PokeRoutineType.BoolBot => true,
            PokeRoutineType.SoJCamp => true,
            PokeRoutineType.CurryBot => true,
            PokeRoutineType.RollingRaid => true,

            PokeRoutineType.RemoteControl => true,

            _ => false,
        };
    }
}
