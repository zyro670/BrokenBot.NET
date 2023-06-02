﻿using PKHeX.Core;
using SysBot.Pokemon.SV.BotEncounter;
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
                => new PokeTradeBotSV(Hub, cfg),

            PokeRoutineType.EggFetch => new EggBotSV(cfg, Hub),
            PokeRoutineType.EggFetchV2 => new EggBotSVV2(cfg, Hub),
            PokeRoutineType.EncounterCapture => new EncounterCaptureSV(cfg, Hub),
            PokeRoutineType.EncounterRuinous => new EncounterRuinousSV(cfg, Hub),
            PokeRoutineType.EncounterGimmighoul => new EncounterGimmighoulSV(cfg, Hub),
            PokeRoutineType.RaidBot => new RaidSV(cfg, Hub),
            PokeRoutineType.RemoteControl => new RemoteControlBotSV(cfg),
            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.FlexTrade or PokeRoutineType.Idle
                or PokeRoutineType.LinkTrade
                or PokeRoutineType.Clone
                or PokeRoutineType.Dump
                => true,

            PokeRoutineType.EggFetch => true,
            PokeRoutineType.EggFetchV2 => true,
            PokeRoutineType.EncounterCapture => true,
            PokeRoutineType.EncounterRuinous => true,
            PokeRoutineType.EncounterGimmighoul => true,
            PokeRoutineType.RaidBot => true,
            PokeRoutineType.RemoteControl => true,

            _ => false,
        };
    }
}
