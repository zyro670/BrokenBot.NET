using PKHeX.Core;
using SysBot.Base;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace SysBot.Pokemon
{
    public abstract class PokeRoutineExecutor<T> : PokeRoutineExecutorBase where T : PKM, new()
    {
        protected PokeRoutineExecutor(IConsoleBotManaged<IConsoleConnection, IConsoleConnectionAsync> cfg) : base(cfg)
        {
        }

        public abstract Task<T> ReadPokemon(ulong offset, CancellationToken token);
        public abstract Task<T> ReadPokemon(ulong offset, int size, CancellationToken token);
        public abstract Task<T> ReadPokemonPointer(IEnumerable<long> jumps, int size, CancellationToken token);
        public abstract Task<T> ReadBoxPokemon(int box, int slot, CancellationToken token);

        public async Task<T?> ReadUntilPresent(ulong offset, int waitms, int waitInterval, int size, CancellationToken token)
        {
            int msWaited = 0;
            while (msWaited < waitms)
            {
                var pk = await ReadPokemon(offset, size, token).ConfigureAwait(false);
                if (pk.Species != 0 && pk.ChecksumValid)
                    return pk;
                await Task.Delay(waitInterval, token).ConfigureAwait(false);
                msWaited += waitInterval;
            }
            return null;
        }

        public async Task<T?> ReadUntilPresentPointer(IReadOnlyList<long> jumps, int waitms, int waitInterval, int size, CancellationToken token)
        {
            int msWaited = 0;
            while (msWaited < waitms)
            {
                var pk = await ReadPokemonPointer(jumps, size, token).ConfigureAwait(false);
                if (pk.Species != 0 && pk.ChecksumValid)
                    return pk;
                await Task.Delay(waitInterval, token).ConfigureAwait(false);
                msWaited += waitInterval;
            }
            return null;
        }

        protected async Task<(bool, ulong)> ValidatePointerAll(IEnumerable<long> jumps, CancellationToken token)
        {
            var solved = await SwitchConnection.PointerAll(jumps, token).ConfigureAwait(false);
            return (solved != 0, solved);
        }

        public static void DumpPokemon(string folder, string subfolder, PKM pk, bool boxData = false)
        {
            var fn = DumpPokemonPath(folder, subfolder, pk);

            if (string.IsNullOrWhiteSpace(fn))
                return;

            var data = boxData
                ? pk.DecryptedBoxData
                : pk.DecryptedPartyData;

            File.WriteAllBytes(fn, data);

            LogUtil.LogInfo($"Saved file: {fn}", "Dump");
        }

        public static void DumpPokemon(string folder, string subfolder, PKM pk, byte[] bytes)
        {
            var fn = DumpPokemonPath(folder, subfolder, pk, true);

            if (string.IsNullOrWhiteSpace(fn))
                return;

            File.WriteAllBytes(fn, bytes);
            LogUtil.LogInfo($"Saved raw file: {fn}", "Dump");
        }

        private static string? DumpPokemonPath(string folder, string subfolder, PKM pk, bool isEncrypted = false)
        {
            string form = pk.Form > 0 ? $"-{pk.Form:00}" : string.Empty;
            string ballFormatted = string.Empty;
            string shinytype = string.Empty;
            string marktype = string.Empty;
            if (pk.IsShiny)
            {
                if (pk.Format >= 8 && (pk.ShinyXor == 0 || pk.FatefulEncounter || pk.Version == (int)GameVersion.GO))
                    shinytype = " ■";
                else
                    shinytype = " ★";
            }

            string IVList = pk.IV_HP + "." + pk.IV_ATK + "." + pk.IV_DEF + "." + pk.IV_SPA + "." + pk.IV_SPD + "." + pk.IV_SPE;

            string TIDFormatted = pk.Generation >= 7 ? $"{pk.TrainerTID7:000000}" : $"{pk.TID16:00000}";

            if (pk.Ball != (int)Ball.None)
                ballFormatted = " - " + GameInfo.Strings.balllist[pk.Ball].Split(' ')[0];

            string speciesName = SpeciesName.GetSpeciesNameGeneration(pk.Species, (int)LanguageID.English, pk.Format);
            if (pk is IGigantamax gmax && gmax.CanGigantamax)
                speciesName += "-Gmax";

            string OTInfo = string.IsNullOrEmpty(pk.OT_Name) ? "" : $" - {pk.OT_Name} - {TIDFormatted}{ballFormatted}";

            if (pk is PK8 or PK9)
            {
                bool hasMark = StopConditionSettings.HasMark((PK9)pk, out RibbonIndex mark);
                if (hasMark)
                    marktype = hasMark ? $"{mark.ToString().Replace("Mark", "")}Mark - " : "";
            }

            string filename = $"{pk.Species:000}{form}{shinytype} - {speciesName} - {marktype}{IVList}{OTInfo} - {pk.EncryptionConstant:X8}";
            string filetype = $".{(isEncrypted ? "e" : "")}";
            if (pk is PK8)
                filetype += "pk8";
            if (pk is PB8)
                filetype += "pb8";
            if (pk is PA8)
                filetype += "pa8";
            if (pk is PK9)
                filetype += "pk9";

            if (!Directory.Exists(folder))
                return null;

            var dir = Path.Combine(folder, subfolder);
            Directory.CreateDirectory(dir);
            
            return Path.Combine(dir, filename + filetype);
        }

        public async Task<bool> TryReconnect(int attempts, int extraDelay, SwitchProtocol protocol, CancellationToken token)
        {
            // USB can have several reasons for connection loss, some of which is not recoverable (power loss, sleep). Only deal with WiFi for now.
            if (protocol is SwitchProtocol.WiFi)
            {
                // If ReconnectAttempts is set to -1, this should allow it to reconnect (essentially) indefinitely.
                for (int i = 0; i < (uint)attempts; i++)
                {
                    LogUtil.LogInfo($"Trying to reconnect... ({i + 1})", Connection.Label);
                    Connection.Reset();
                    if (Connection.Connected)
                        break;

                    await Task.Delay(30_000 + (int)extraDelay, token).ConfigureAwait(false);
                }
            }
            return Connection.Connected;
        }

        public async Task VerifyBotbaseVersion(CancellationToken token)
        {
            var data = await SwitchConnection.GetBotbaseVersion(token).ConfigureAwait(false);
            var version = decimal.TryParse(data, CultureInfo.InvariantCulture, out var v) ? v : 0;
            if (version < BotbaseVersion)
            {
                var protocol = Config.Connection.Protocol;
                var msg = protocol is SwitchProtocol.WiFi ? "sys-botbase" : "usb-botbase";
                msg += $" version is not supported. Expected version {BotbaseVersion} or greater, and current version is {version}. Please download the latest version from: ";
                if (protocol is SwitchProtocol.WiFi)
                    msg += "https://github.com/olliz0r/sys-botbase/releases/latest";
                else
                    msg += "https://github.com/Koi-3088/usb-botbase/releases/latest";
                throw new Exception(msg);
            }
        }

        // Check if either Tesla or dmnt are active if the sanity check for Trainer Data fails, as these are common culprits.
        private const ulong ovlloaderID = 0x420000000007e51a; // Tesla Menu
        private const ulong dmntID = 0x010000000000000d;      // dmnt used for cheats

        public async Task CheckForRAMShiftingApps(CancellationToken token)
        {
            Log("Trainer data is not valid.");

            bool found = false;
            var msg = "";
            if (await SwitchConnection.IsProgramRunning(ovlloaderID, token).ConfigureAwait(false))
            {
                msg += "Found Tesla Menu";
                found = true;
            }

            if (await SwitchConnection.IsProgramRunning(dmntID, token).ConfigureAwait(false))
            {
                if (found)
                    msg += " and ";
                msg += "dmnt (cheat codes?)";
                found = true;
            }
            if (found)
            {
                msg += ".";
                Log(msg);
                Log("Please remove interfering applications and reboot the Switch.");
            }
        }
    }
}