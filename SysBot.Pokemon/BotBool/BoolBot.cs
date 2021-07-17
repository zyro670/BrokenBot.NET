using PKHeX.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsets;

namespace SysBot.Pokemon
{
    public class BoolBot : PokeRoutineExecutor
    {
        private readonly PokeTradeHub<PK8> Hub;
        private readonly BoolSettings Settings;

        public BoolBot(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg)
        {
            Hub = hub;
            Settings = hub.Config.Bool;
        }

        public override async Task MainLoop(CancellationToken token)
        {
            Log("Identifying trainer data of the host console.");
            await IdentifyTrainer(token).ConfigureAwait(false);

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
            uint offset = ResetLegendFlagOffset;
            var enumVal = Enum.GetNames(typeof(LairSpecies));
            Log("Beginning caught flag reset.");

            for (int i = 1; i < 48; i++)
            {
                var val = BitConverter.ToUInt16(await Connection.ReadBytesAsync(offset, 2, token).ConfigureAwait(false), 0);
                if (val == 1)
                {
                    Log($"Resetting caught flag for {enumVal[i]}.");
                    await Connection.WriteBytesAsync(new byte[] { 0 }, offset, token).ConfigureAwait(false);
                }

                offset += 0x38;
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
                    case DexRecSpecies.Ponyta or DexRecSpecies.Rapidash or DexRecSpecies.Slowpoke or DexRecSpecies.Farfetchd or DexRecSpecies.Weezing or DexRecSpecies.MrMime or
                    DexRecSpecies.Zigzagoon or DexRecSpecies.Linoone or DexRecSpecies.Darumaka or DexRecSpecies.Darmanitan or DexRecSpecies.Yamask or DexRecSpecies.Cofagrigus or DexRecSpecies.Corsola or DexRecSpecies.Shellos or DexRecSpecies.Meowth:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes(dex[i] == DexRecSpecies.Meowth ? (short)0x02 : (short)0x01), offset + 0x04, token).ConfigureAwait(false); break;

                    // Male only
                    case DexRecSpecies.NidoranM or DexRecSpecies.Nidorino or DexRecSpecies.Nidoking or DexRecSpecies.Hitmonlee or DexRecSpecies.Hitmonchan or DexRecSpecies.Tauros or DexRecSpecies.Tyrogue or
                    DexRecSpecies.Gallade or DexRecSpecies.Throh or DexRecSpecies.Sawk or DexRecSpecies.Rufflet or DexRecSpecies.Braviary or DexRecSpecies.IndeedeeM or DexRecSpecies.Impidimp or DexRecSpecies.Morgrem or DexRecSpecies.Grimmsnarl:
                        await Connection.WriteBytesAsync(BitConverter.GetBytes((short)0x00), gender, token).ConfigureAwait(false); break;

                    // Genderless
                    case DexRecSpecies.Magnemite or DexRecSpecies.Magneton or DexRecSpecies.Staryu or DexRecSpecies.Starmie or DexRecSpecies.Ditto or DexRecSpecies.Lunatone or DexRecSpecies.Solrock
                    or DexRecSpecies.Baltoy or DexRecSpecies.Claydol or DexRecSpecies.Beldum or DexRecSpecies.Metang or DexRecSpecies.Metagross or DexRecSpecies.Bronzor or DexRecSpecies.Bronzong or DexRecSpecies.Magnezone
                    or DexRecSpecies.Rotom or DexRecSpecies.Klink or DexRecSpecies.Klang or DexRecSpecies.Klinklang or DexRecSpecies.Cryogonal or DexRecSpecies.Golett or DexRecSpecies.Golurk or DexRecSpecies.Carbink or
                    DexRecSpecies.Dhelmise or DexRecSpecies.Sinistea or DexRecSpecies.Polteageist or DexRecSpecies.Falinks:
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