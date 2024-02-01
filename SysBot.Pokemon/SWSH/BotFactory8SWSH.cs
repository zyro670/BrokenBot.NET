using System;
using PKHeX.Core;

namespace SysBot.Pokemon;

public sealed class BotFactory8SWSH : BotFactory<PK8>
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
            => new PokeTradeBotSWSH(Hub, cfg),

        PokeRoutineType.EggFetch => new EncounterBotEggSWSH(cfg, Hub),
        PokeRoutineType.FossilBot => new EncounterBotFossilSWSH(cfg, Hub),
        PokeRoutineType.RaidBot => new RaidBotSWSH(cfg, Hub),
        PokeRoutineType.EncounterLine => new EncounterBotLineSWSH(cfg, Hub),
        PokeRoutineType.Reset => new EncounterBotResetSWSH(cfg, Hub),
        PokeRoutineType.DogBot => new EncounterBotDogSWSH(cfg, Hub),
        PokeRoutineType.LairBot => new LairBotSWSH(cfg, Hub),
        PokeRoutineType.DenBot => new DenBotSWSH(cfg, Hub),
        PokeRoutineType.BoolBot => new BoolBotSWSH(cfg, Hub),
        PokeRoutineType.SoJCamp => new SoJCampSWSH(cfg, Hub),
        PokeRoutineType.CurryBot => new CurryBotSWSH(cfg, Hub),
        PokeRoutineType.RollingRaid => new RollingRaidBotSWSH(cfg, Hub),
        PokeRoutineType.OverworldBot => new OverworldBotSWSH(cfg, Hub),
        PokeRoutineType.TIDResetBot => new TIDResetBotSWSH(cfg, Hub),

        PokeRoutineType.RemoteControl => new RemoteControlBotSWSH(cfg),
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
        PokeRoutineType.DogBot => true,
        PokeRoutineType.LairBot => true,
        PokeRoutineType.DenBot => true,
        PokeRoutineType.BoolBot => true,
        PokeRoutineType.SoJCamp => true,
        PokeRoutineType.CurryBot => true,
        PokeRoutineType.RollingRaid => true,
        PokeRoutineType.OverworldBot => true,
        PokeRoutineType.TIDResetBot => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
