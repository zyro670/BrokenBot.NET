using PKHeX.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsets;

namespace SysBot.Pokemon
{
    public sealed class BoolBot : EncounterBot
    {
        private readonly BoolSettings Settings;

        public BoolBot(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg, hub)
        {
            Settings = Hub.Config.Bool;
        }

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            Log("Starting main BoolBot loop.");
            Config.IterateNextRoutine();

            var task = Settings.BoolType switch
            {
                BoolMode.Skipper => Skipper(token),
                BoolMode.Injector => Injector(token),
                BoolMode.ResetLegendaryLairFlags => ResetLegendaryLairFlags(token),
                _ => Skipper(token),
            };
            await task.ConfigureAwait(false);
        }

        private async Task ResetLegendaryLairFlags(CancellationToken token)
        {
            var enumVal = Enum.GetNames(typeof(LairSpeciesBlock));
            Log("Beginning caught flag reset.");
            for (uint i = 0; i < enumVal.Length; i++)
            {
                uint offset = ResetLegendFlagOffset + (i * 0x38);
                var val = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset, 2, token).ConfigureAwait(false), 0);
                if (val == 1)
                {
                    Log($"Resetting caught flag for {enumVal[i]}.");
                    await Connection.WriteBytesAsync(new byte[1], offset, token).ConfigureAwait(false);
                }
            }
            Log("Caught flag reset complete.");
            return;
        }

        private async Task Injector(CancellationToken token)
        {
            uint offset = DexRecMon;
            uint gender = DexRecMonGender;
            var dex = Settings.DexRecConditions.SpeciesTargets;

            for (int i = 0; i < dex.Length; i++)
            {
                Log($"Changing slot {i + 1} species to {dex[i]}.");
                await Connection.WriteBytesAsync(BitConverter.GetBytes((short)dex[i]), offset, token).ConfigureAwait(false);
                switch (dex[i])
                {
                    case DexRecSpecies.Meowstic or DexRecSpecies.Indeedee when Version == GameVersion.SH:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x01), offset + 0x04, token).ConfigureAwait(false);
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x01), gender, token).ConfigureAwait(false); break;

                    case DexRecSpecies.Meowstic or DexRecSpecies.Indeedee when Version == GameVersion.SW:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), offset + 0x04, token).ConfigureAwait(false);
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), gender, token).ConfigureAwait(false); break;

                    // Shield Exclusives
                    case DexRecSpecies.Ponyta or DexRecSpecies.Kabuto or DexRecSpecies.Corsola or DexRecSpecies.Larvitar or DexRecSpecies.Pupitar
                        or DexRecSpecies.Lotad or DexRecSpecies.Lombre or DexRecSpecies.Sableye or DexRecSpecies.Lunatone or DexRecSpecies.Gible or DexRecSpecies.Croagunk or DexRecSpecies.Solosis or
                        DexRecSpecies.Duosion or DexRecSpecies.Vullaby or DexRecSpecies.Mandibuzz or DexRecSpecies.Spritzee or DexRecSpecies.Aromatisse or DexRecSpecies.Skrelp
                        or DexRecSpecies.Dragalge or DexRecSpecies.Goomy or DexRecSpecies.Sliggoo or DexRecSpecies.Oranguru or DexRecSpecies.Drampa or DexRecSpecies.Cursola or
                        DexRecSpecies.Eiscue or DexRecSpecies.Arcanine when Version == GameVersion.SW:
                        {
                            Log($"{dex[i]} is not possible on this game version!");
                            await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), offset, token).ConfigureAwait(false);
                            await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), offset + 0x04, token).ConfigureAwait(false);
                            await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), gender, token).ConfigureAwait(false); break;
                        }
                    // Sword Exclusives
                    case DexRecSpecies.Farfetchd or DexRecSpecies.Omanyte or DexRecSpecies.Seedot or DexRecSpecies.Nuzleaf or DexRecSpecies.Mawile or DexRecSpecies.Solrock or DexRecSpecies.Bagon or
                    DexRecSpecies.Darumaka or DexRecSpecies.Scraggy or DexRecSpecies.Gothita or DexRecSpecies.Gothorita or DexRecSpecies.Gothitelle or DexRecSpecies.Rufflet or DexRecSpecies.Braviary or
                    DexRecSpecies.Deino or DexRecSpecies.Zweilous or DexRecSpecies.Swirlix or DexRecSpecies.Slurpuff or DexRecSpecies.Clauncher or DexRecSpecies.Clawitzer or DexRecSpecies.Passimian or
                    DexRecSpecies.Turtonator or DexRecSpecies.Jangmoo or DexRecSpecies.Hakamoo or DexRecSpecies.Stonjourner or DexRecSpecies.Ninetales when Version == GameVersion.SH:
                        {
                            Log($"{dex[i]} is not possible on this game version!");
                            await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), offset, token).ConfigureAwait(false);
                            await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), offset + 0x04, token).ConfigureAwait(false);
                            await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), gender, token).ConfigureAwait(false); break;
                        }

                    // Alt forms
                    case DexRecSpecies.Ponyta or DexRecSpecies.Slowpoke or DexRecSpecies.Farfetchd or DexRecSpecies.Weezing or DexRecSpecies.MrMime or
                DexRecSpecies.Zigzagoon or DexRecSpecies.Linoone or DexRecSpecies.Darumaka or DexRecSpecies.Yamask or DexRecSpecies.Corsola or DexRecSpecies.Shellos or DexRecSpecies.Meowth:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes(dex[i] == DexRecSpecies.Meowth ? (short)0x02 : (short)0x01), offset + 0x04, token).ConfigureAwait(false); break;
                    // Male only
                    case DexRecSpecies.NidoranM or DexRecSpecies.Hitmonlee or DexRecSpecies.Hitmonchan or DexRecSpecies.Tauros or DexRecSpecies.Tyrogue or
                 DexRecSpecies.Throh or DexRecSpecies.Sawk or DexRecSpecies.Rufflet or DexRecSpecies.Braviary or DexRecSpecies.Impidimp or DexRecSpecies.Morgrem or DexRecSpecies.Grimmsnarl:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), gender, token).ConfigureAwait(false); break;

                    // Genderless
                    case DexRecSpecies.Magnemite or DexRecSpecies.Ditto or DexRecSpecies.Lunatone or DexRecSpecies.Solrock or DexRecSpecies.Baltoy or DexRecSpecies.Claydol or DexRecSpecies.Beldum or
                    DexRecSpecies.Bronzor or DexRecSpecies.Bronzong or DexRecSpecies.Rotom or DexRecSpecies.Klink or DexRecSpecies.Klang or DexRecSpecies.Cryogonal or DexRecSpecies.Golett or
                    DexRecSpecies.Golurk or DexRecSpecies.Carbink or DexRecSpecies.Dhelmise or DexRecSpecies.Sinistea or DexRecSpecies.Falinks:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x02), gender, token).ConfigureAwait(false); break;

                    default:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), offset + 0x04, token).ConfigureAwait(false);
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x01), gender, token).ConfigureAwait(false); break;
                }
                offset += 0x20;
                gender += 0x20;
            }
            Log("Species update complete.");

            if (Settings.DexRecConditions.LocationTarget == DexRecLoc.None)
            {
                Log("No desired location selected.");
                return;
            }

            Log($"Updating current location to {Settings.DexRecConditions.LocationTarget}.");
            ulong location = (ulong)Settings.DexRecConditions.LocationTarget;
            await Connection.WriteBytesAsync(BitConverter.GetBytes(location), DexRecLocation, token).ConfigureAwait(false);

            Settings.DexRecConditions.LocationTarget = DexRecLoc.None;
            Log("Location update complete.");
            return;
        }

        private async Task Skipper(CancellationToken token)
        {
            DexRecSpecies[] dex = Settings.DexRecConditions.SpeciesTargets;
            DexRecLoc loc = Settings.DexRecConditions.LocationTarget;
            DexRecSpecies species;
            uint offset = DexRecMon;
            string log = string.Empty;

            bool empty = dex.All(x => x == DexRecSpecies.None) && loc == DexRecLoc.None;
            Log("Starting DaySkipping to update recommendations! Ensure that Date/Time Sync is ON, and that when the menu is open the cursor is hovered over the Pokédex!");
            if (empty)
                Log("No target set, skipping indefinitely.. When you see a species or location you want, stop the bot.");

            while (!token.IsCancellationRequested)
            {
                Log("DaySkipping to update recommendations!");
                await DaySkip(token).ConfigureAwait(false);
                await Task.Delay(0_500, token).ConfigureAwait(false);

                Log("Checking Pokédex for current recommendations.");
                await Click(X, 1_000, token).ConfigureAwait(false);
                await Click(A, 5_000, token).ConfigureAwait(false);

                ulong currentlocation = BitConverter.ToUInt64(await Connection.ReadBytesAsync(DexRecLocation, 8, token).ConfigureAwait(false), 0);
                int s = 0;
                do
                {
                    byte[] currentspecies = await SwitchConnection.ReadBytesAsync(offset, 2, token).ConfigureAwait(false);
                    species = (DexRecSpecies)BitConverter.ToUInt16(currentspecies.Slice(0, 2), 0);
                    if (species != 0)
                        log += $"\n - {species}";

                    if (!empty)
                    {
                        for (int d = 0; d < dex.Length; d++)
                        {
                            if (dex[d] == DexRecSpecies.None)
                                break;

                            if (species == dex[d])
                            {
                                Log($"Recommended species found: {species}!");
                                _ = loc == DexRecLoc.None;
                                return;
                            }
                        }
                    }

                    offset += 0x20;
                    s++;
                } while (s < 4);

                Log($"Current location: {(DexRecLoc)currentlocation}\nCurrent species: {log}");
                offset = DexRecMon;
                log = $"";

                if (loc != DexRecLoc.None)
                {
                    Log($"Searching for location: {loc}.");
                    if ((ulong)loc == currentlocation)
                    {
                        Log($"Recommendation matches target location: {loc}.");
                        _ = loc == DexRecLoc.None;
                        return;
                    }
                }

                while (!await IsOnOverworld(Hub.Config, token).ConfigureAwait(false))
                    await Click(B, 0_500, token).ConfigureAwait(false);
            }
        }
    }
}