namespace SysBot.Pokemon
{
    public enum AbilityType
    {
        First,
        Second,
        Hidden,
        Any,
    }

    public enum GenderType
    {
        Male,
        Female,
        Genderless,
        Any,
    }

    public enum ShinyType
    {
        NotShiny,
        Star,
        Square
    }

    public enum DenType
    {
        Vanilla,
        IoA,
        CT,
    }

    public enum BeamType
    {
        CommonWish = 3,
        RareWish = 4,
        Event = 5,
    }

    public enum GenderRatio
    {
        Male = 0,
        Male88 = 31,
        Male75 = 63,
        Even = 127,
        Female75 = 191,
        Female88 = 225,
        Female = 254,
        Genderless = 255,
    }

    public enum DenMode
    {
        Skip,
        SeedSearch,
        Inject,
    }

    public enum Characteristics
    {
        TakesPlentyOfSiestas,
        LikesToThrashAbout,
        CapableOfTakingHits,
        AlertToSounds,
        Mischievous,
        SomewhatVain,
        Any,
    }
}
