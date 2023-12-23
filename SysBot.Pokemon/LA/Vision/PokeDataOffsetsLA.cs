using System.Collections.Generic;

namespace SysBot.Pokemon;

/// <summary>
/// Pokémon Legends: Arceus RAM offsets
/// </summary>
public class PokeDataOffsetsLA
{
    public const string LAGameVersion = "1.1.1";
    public const string LegendsArceusID = "01001F5010DFA000";
    public IReadOnlyList<long> BoxStartPokemonPointer { get; } = new long[] { 0x42BA6B0, 0x1F0, 0x68 };
    public IReadOnlyList<long> LinkTradePartnerPokemonPointer { get; } = new long[] { 0x42BEAD8, 0x188, 0x78, 0x98, 0x58, 0x0 };
    public IReadOnlyList<long> LinkTradePartnerNamePointer { get; } = new long[] { 0x42ED070, 0xC8, 0x88 };
    public IReadOnlyList<long> LinkTradePartnerTIDPointer { get; } = new long[] { 0x42ED070, 0xC8, 0x78 };
    public IReadOnlyList<long> LinkTradePartnerNIDPointer { get; } = new long[] { 0x42EA508, 0xE0, 0x8 };
    public IReadOnlyList<long> TradePartnerStatusPointer { get; } = new long[] { 0x42BEAD8, 0x188, 0x78, 0xBC };
    public IReadOnlyList<long> MyStatusPointer { get; } = new long[] { 0x42BA6B0, 0x218, 0x68 };
    public IReadOnlyList<long> TextSpeedPointer { get; } = new long[] { 0x42BA6B0, 0x1E0, 0x68 };
    public IReadOnlyList<long> CurrentBoxPointer { get; } = new long[] { 0x42BA6B0, 0x1F8, 0x4A9 };
    public IReadOnlyList<long> SoftbanPointer { get; } = new long[] { 0x42BA6B0, 0x268, 0x70 };
    public IReadOnlyList<long> OverworldPointer { get; } = new long[] { 0x42C30E8, 0x1A9 };

    public const int BoxFormatSlotSize = 0x168;

    //ArcBot Additions 1.1.0
    public const uint MenuOffset = 0x042D4B04;
    public const uint ActivateDistortion = 0x024A0428;
    public const uint InvincibleTrainer1 = 0x024E9444;
    public const uint InvincibleTrainer2 = 0x024E964C;
    public const uint InfPP1 = 0x007AB30C;
    public const uint InfPP2 = 0x007AB31C;
    public const string WildPokemonPtrLA = "[[[[[main+42a6f00]+D0]+B8]+300]+70]+60]+98]+10]";
    public const string PlayerCoordPtrLA = "[[[[[[main+42D4720]+18]+48]+1F0]+18]+370]+90";
    public const string TimePtrLA = "[[[[main+42D4720]+18]+100]+18]+28";
    public IReadOnlyList<long> InventoryKeyItems { get; } = new long[] { 0x42BA6B0, 0x230, 0xAF4 };
    public IReadOnlyList<long> PokeDex { get; } = new long[] { 0x42BA6B0, 0x248, 0x58, 0x18, 0x1C };
}
