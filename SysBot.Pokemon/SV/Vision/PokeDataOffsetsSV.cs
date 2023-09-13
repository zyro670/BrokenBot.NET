using System.Collections.Generic;

namespace SysBot.Pokemon
{
    /// <summary>
    /// Pokémon Scarlet/Violet RAM offsets
    /// </summary>
    public class PokeDataOffsetsSV
    {
        public const string SVGameVersion = "2.0.1";
        public const string ScarletID = "0100A3D008C5C000";
        public const string VioletID = "01008F6008C5E000";
        public IReadOnlyList<long> BoxStartPokemonPointer { get; } = new long[] { 0x4622A30, 0x198, 0x30, 0x10, 0x9D0, 0x0 };
        public IReadOnlyList<long> LinkTradePartnerPokemonPointer { get; } = new long[] { 0x44BB848, 0x48, 0x58, 0x40, 0x148 };
        public IReadOnlyList<long> LinkTradePartnerNIDPointer { get; } = new long[] { 0x44DFCA8, 0xF8, 0x8 };
        public IReadOnlyList<long> MyStatusPointer { get; } = new long[] { 0x4622A30, 0xE0, 0x30, 0xB8, 0x300, 0x10, 0x18 };
        public IReadOnlyList<long> Trader1MyStatusPointer { get; } = new long[] { 0x44BB848, 0x48, 0xB0, 0x0 }; // The trade partner status uses a compact struct that looks like MyStatus.
        public IReadOnlyList<long> Trader2MyStatusPointer { get; } = new long[] { 0x44BB848, 0x48, 0xE0, 0x0 };
        public IReadOnlyList<long> ConfigPointer { get; } = new long[] { 0x44C1C18, 0x1B8, 0x40 };
        public IReadOnlyList<long> CurrentBoxPointer { get; } = new long[] { 0x44C1C18, 0x128, 0x570 };
        public IReadOnlyList<long> PortalBoxStatusPointer { get; } = new long[] { 0x44DB380, 0x18, 0xA0, 0x1B8, 0x70, 0x28 };  // 9-A in portal, 4-6 in box.
        public IReadOnlyList<long> IsConnectedPointer { get; } = new long[] { 0x44E5140, 0x10 };
        public IReadOnlyList<long> OverworldPointer { get; } = new long[] { 0x44E5068, 0x348, 0x10, 0xD8, 0x28 };

        public const int BoxFormatSlotSize = 0x158;
        public const int BoxFormatSlotCount = 30;
        public const ulong LibAppletWeID = 0x010000000000100a; // One of the process IDs for the news.

        public IReadOnlyList<long> TeraRaidCodePointer { get; } = new long[] { 0x44C7730, 0x10, 0x78, 0x10, 0x1A9 };
        public IReadOnlyList<long> TeraRaidBlockPointer { get; } = new long[] { 0x44A98C8, 0x180, 0x40 };

        public ulong TeraLobby { get; } = 0x0415F430;
        public ulong LoadedIntoRaid { get; } = 0x0453B020;

        public IReadOnlyList<long> CollisionPointer { get; } = new long[] { 0x44CCA90, 0xAD8, 0x160, 0x60, 0x100 };

        public IReadOnlyList<long> SaveBlockPointer { get; } = new long[] { 0x44AAC88, 0xE0, 0x80, 0x8, 0x00};
    }
}
