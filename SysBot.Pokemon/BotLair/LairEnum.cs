namespace SysBot.Pokemon
{
    public enum LairSpecies : ushort
    {
        None = 0,
        Zapdos = 145,
        Moltres = 146,
        Articuno = 144,
        Mewtwo = 150,
        Suicune = 245,
        Entei = 244,
        Raikou = 243,
        Lugia = 249,
        HoOh = 250,
        Latias = 380,
        Latios = 381,
        Groudon = 383,
        Kyogre = 382,
        Rayquaza = 384,
        Uxie = 480,
        Azelf = 482,
        Mesprit = 481,
        Dialga = 483,
        Palkia = 484,
        Giratina = 487,
        Heatran = 485,
        Cresselia = 488,
        Tornadus = 641,
        Thundurus = 642,
        Landorus = 645,
        Reshiram = 643,
        Zekrom = 644,
        Kyurem = 646,
        Xerneas = 716,
        Yveltal = 717,
        Zygarde = 718,
        TapuKoko = 785,
        TapuLele = 786,
        TapuBulu = 787,
        TapuFini = 788,
        Solgaleo = 791,
        Lunala = 792,
        Necrozma = 800,
        Nihilego = 793,
        Buzzwole = 794,
        Pheromosa = 795,
        Xurkitree = 796,
        Kartana = 798,
        Celesteela = 797,
        Guzzlord = 799,
        Blacephalon = 806,
        Stakataka = 805
    }

    public enum LairSpeciesBlock
    {
        Zapdos,
        Moltres,
        Articuno,
        Mewtwo,
        Suicune,
        Entei,
        Raikou,
        Lugia,
        HoOh,
        Latias,
        Latios,
        Groudon,
        Kyogre,
        Rayquaza,
        Uxie,
        Azelf,
        Mesprit,
        Dialga,
        Palkia,
        Giratina,
        Heatran,
        Cresselia,
        Tornadus,
        Thundurus,
        Landorus,
        Reshiram,
        Zekrom,
        Kyurem,
        Xerneas,
        Yveltal,
        Zygarde,
        TapuKoko,
        TapuLele,
        TapuBulu,
        TapuFini,
        Solgaleo,
        Lunala,
        Necrozma,
        Nihilego,
        Buzzwole,
        Pheromosa,
        Xurkitree,
        Kartana,
        Celesteela,
        Guzzlord,
        Blacephalon,
        Stakataka
    }

    public enum SelectPath
    {
        GoLeft,
        GoRight,
    }

    public enum PriorityMoves : int
    {
        Accelerock = 709,
        FakeOut = 252,
        ExtremeSpeed = 245,
        Feint = 364,
        FirstImpression = 660,
        AquaJet = 453,
        BulletPunch = 418,
        IceShard = 420,
        MachPunch = 183,
        QuickAttack = 98,
        ShadowSneak = 425,
        SuckerPunch = 389,
        VacuumWave = 410,
        WaterShuriken = 594
    }

    public enum MoveCategory
    {
        Status,
        Physical,
        Special
    }

    public enum StatusCondition
    {
        NoCondition,
        Paralyzed,
        Asleep,
        Frozen,
        Burned,
        Poisoned,
    }

    public enum MoveTarget
    {
        AllAdjacentOpponents,
        AllAdjacent,
        AllAllies,
        AllyOrSelf,
        Ally,
        AnyExceptSelf,
        Self,
        SideSelf,
        SideAll,
        Counter,
        All,
        Opponent,
        RandomOpponent,
    }

    public enum DmaxMoves
    {
        MaxStrike,
        MaxKnuckle,
        MaxAirstream,
        MaxOoze,
        MaxQuake,
        MaxRockfall,
        MaxFlutterby,
        MaxPhantasm,
        MaxSteelspike,
        MaxFlare,
        MaxGeyser,
        MaxOvergrowth,
        MaxLightning,
        MaxMindstorm,
        MaxHailstorm,
        MaxWyrmwind,
        MaxDarkness,
        MaxStarfall,
        MaxGuard,
    }

    public enum LairBotModes
    {
        OffsetLog,
        LairBot,
    }

    public enum LairBall : byte
    {
        Master = 1,
        Ultra = 2,
        Great = 3,
        Poke = 4,
        Safari = 5,
        Net = 6,
        Dive = 7,
        Nest = 8,
        Repeat = 9,
        Timer = 10,
        Luxury = 11,
        Premier = 12,
        Dusk = 13,
        Heal = 14,
        Quick = 15,
        Cherish = 16,
        Fast = 17,
        Level = 18,
        Lure = 19,
        Heavy = 20,
        Love = 21,
        Friend = 22,
        Moon = 23,
        Sport = 24,
        Dream = 25,
        Beast = 26
    }
}