using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.Discord
{
    public class EtumrepUtil
    {
        private static TradeQueueInfo<PA8> Info => SysCord<PA8>.Runner.Hub.Queues.Info;
        private static readonly PokeTradeHubConfig Config = Info.Hub.Config;

        private static int ServerCount { get => Config.EtumrepDump.Servers.Count; }

        internal class EtumrepUser
        {
            internal EtumrepUser(TcpClient client, AuthenticatedStream stream, SocketMessageComponent component)
            {
                Client = client;
                Stream = stream;
                Component = component;
                BotName = $"{component.Message.Author.Username}#{component.Message.Author.Discriminator}";
                SeedCheckerName = $"{component.User.Username}#{component.User.Discriminator}";
                SeedCheckerID = component.User.Id;
            }

            public TcpClient Client { get; }
            public AuthenticatedStream Stream { get; }
            public SocketMessageComponent Component { get; }
            public string BotName { get; }
            public string SeedCheckerName { get; }
            public ulong SeedCheckerID { get; }
            public bool IsAuthenticated { get; set; }
            public byte[] Data { get; set; } = Array.Empty<byte>();
        }

        internal class UserAuth
        {
            public string HostName { get; set; } = string.Empty;
            public ulong HostID { get; set; }
            public string HostPassword { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public string SeedCheckerName { get; set; } = string.Empty;
            public ulong SeedCheckerID { get; set; }
        }

        public static async Task SendEtumrepEmbedAsync(SocketUser user, IReadOnlyList<PA8> pkms)
        {
            var list = new List<FileAttachment>();
            for (int i = 0; i < pkms.Count; i++)
            {
                var pk = pkms[i];
                var ms = new MemoryStream(pk.Data);
                var name = Util.CleanFileName(pk.FileName);
                list.Add(new(ms, name));
            }

            var embed = new EmbedBuilder { Color = Color.Blue };
            var dmCh = await user.CreateDMChannelAsync().ConfigureAwait(false);

            bool exists = Config.EtumrepDump.Servers.Count > 0 && Config.EtumrepDump.Servers.FirstOrDefault(x => x.IP != string.Empty && x.Token != string.Empty) is not null;
            if (exists)
            {
                var buttonYes = new ButtonBuilder() { CustomId = "etumrep_yes", Label = "Yes", Style = ButtonStyle.Primary };
                var buttonNo = new ButtonBuilder() { CustomId = "etumrep_no", Label = "No", Style = ButtonStyle.Secondary };
                var components = new ComponentBuilder().WithButton(buttonYes).WithButton(buttonNo);

                embed.Description = "Here are all the Pokémon you dumped!\nWould you like to calculate your seed using EtumrepMMO?";
                embed.WithAuthor(x => { x.Name = "EtumrepMMO Service"; });

                await dmCh.SendFilesAsync(list, null, false, embed: embed.Build(), null, null, null, components: components.Build()).ConfigureAwait(false);
                return;
            }

            embed.Description = "Here are all the Pokémon you dumped!";
            embed.WithAuthor(x => { x.Name = "Pokémon Legends: Arceus Dump"; });
            await dmCh.SendFilesAsync(list, null, false, embed: embed.Build()).ConfigureAwait(false);
        }

        public static async Task HandleEtumrepRequestAsync(SocketMessageComponent component, string id)
        {
            if (id is "etumrep_yes")
            {
                for (int i = 0; i < ServerCount; i++)
                {
                    var server = Config.EtumrepDump.Servers[i];
                    if (server.IP == string.Empty || server.Token == string.Empty)
                        continue;

                    var msg = $"Attempting to connect to {server.Name}, please wait...";
                    await UpdateEtumrepEmbed(component.Message, msg, Color.Green).ConfigureAwait(false);
                    LogUtil.LogInfo(msg, "[EtumrepMMO Handler]");

                    var user = await AuthenticateConnection(server.IP, server.Port, component).ConfigureAwait(false);
                    if (user is null)
                    {
                        msg = $"Unable to connect to {server.Name}. Server might be offline.";
                        await UpdateEtumrepEmbed(component.Message, msg, Color.Red).ConfigureAwait(false);
                        LogUtil.LogInfo(msg, "[EtumrepMMO Handler]");
                        continue;
                    }

                    if (!user.IsAuthenticated)
                    {
                        DisposeStream(user);
                        msg = $"{server.Name} rejected the connection.";
                        await UpdateEtumrepEmbed(component.Message, msg, Color.Red).ConfigureAwait(false);
                        LogUtil.LogInfo($"{user.BotName}: {msg}", "[EtumrepMMO Handler]");
                        continue;
                    }

                    var authenticated = await Authenticate(user, server).ConfigureAwait(false);
                    if (!authenticated)
                    {
                        DisposeStream(user);
                        continue;
                    }

                    var canQueue = await GetServerConfirmation(user, $"{server.Name} is at full capacity.").ConfigureAwait(false);
                    if (!canQueue)
                    {
                        DisposeStream(user);
                        continue;
                    }

                    await PrepareData(user).ConfigureAwait(false);
                    bool success = await EtumrepRequest(user).ConfigureAwait(false);
                    if (!success)
                    {
                        DisposeStream(user);
                        continue;
                    }

                    break;
                }
            }
            else if (id is "etumrep_no")
            {
                var msg = component.Message.Embeds.First().Description.Split('\n').First();
                await UpdateEtumrepEmbed(component.Message, msg, Color.DarkBlue).ConfigureAwait(false);

                var username = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id})";
                LogUtil.LogInfo($"{username} did not wish to calculate their seed using EtumrepMMO.", "[EtumrepMMO Handler]");
            }
        }

        private static async Task<EtumrepUser?> AuthenticateConnection(string addr, int port, SocketMessageComponent component)
        {
            IPAddress ip = default!;
            var author = component.Message.Author;
            bool success = IPAddress.TryParse(addr, out IPAddress? address);

            if (success && address is not null)
                ip = address;
            else
            {
                try
                {
                    var dns = await Dns.GetHostEntryAsync(addr).ConfigureAwait(false);
                    var IPAdr = dns.AddressList.FirstOrDefault(x => x.ToString().Split('.').Length >= 4);
                    if (IPAdr is not null)
                        ip = IPAdr;
                    else return null;
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"{author.Username}#{author.Discriminator}: Failed to resolve IP/host address. Dequeueing {component.User.Username}#{component.User.Discriminator}...\n{ex.Message}", "[Connection Authentication]");
                    return null;
                }
            }

            var ep = new IPEndPoint(ip, port);
            var client = new TcpClient();

            try
            {
                client.Connect(ep);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo($"{author.Username}#{author.Discriminator}: Failed to connect to server. Dequeueing {component.User.Username}#{component.User.Discriminator}...\n{ex.Message}", "[Connection Authentication]");
                return null;
            }

            EtumrepUser? user = null;
            try
            {
                var clientStream = client.GetStream();

                // Wait for up to 10 minutes to receive result to not overcomplicate with back and forth pings? 
                clientStream.Socket.ReceiveTimeout = 600_000;
                clientStream.Socket.SendTimeout = 600_000;

                var authStream = new NegotiateStream(clientStream, false);
                user = new EtumrepUser(client, authStream, component);
                var credentials = new NetworkCredential();

                await authStream.AuthenticateAsClientAsync(credentials, "").ConfigureAwait(false);
                user.IsAuthenticated = true;

                LogUtil.LogInfo($"{user.BotName}: Initial server authentication complete. Continuing to authenticate {user.SeedCheckerName}...", "[Connection Authentication]");
                return user;
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo($"{user?.BotName}: Failed to authenticate with server. Dequeueing {user?.SeedCheckerName}...\n{ex.Message}", "[Connection Authentication]");
                return user;
            }
        }

        private static async Task<bool> Authenticate(EtumrepUser user, EtumrepDumpSettings.EtumrepServer server)
        {
            var auth = new UserAuth()
            {
                HostID = SysCord<PA8>.App.Owner.Id,
                HostName = $"{SysCord<PA8>.App.Owner.Username}#{SysCord<PA8>.App.Owner.Discriminator}",
                HostPassword = server.LimitInputLength(server.Password, false),
                SeedCheckerID = user.SeedCheckerID,
                SeedCheckerName = server.LimitInputLength(user.SeedCheckerName, true),
                Token = server.LimitInputLength(server.Token, false),
            };

            try
            {
                var authStr = JsonConvert.SerializeObject(auth);
                var authBytes = Encoding.Unicode.GetBytes(authStr);
                await user.Stream.WriteAsync(authBytes).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo($"{user.BotName}: Error while sending user authentication to server. Dequeueing {user.SeedCheckerName}...\n{ex.Message}", "[Server Authentication]");
                return false;
            }

            bool success = await GetServerConfirmation(user, "Server rejected the user authentication.").ConfigureAwait(false);
            return success;
        }

        private static async Task PrepareData(EtumrepUser user)
        {
            var msg = "Successfully authenticated with the server, downloading files...";
            await UpdateEtumrepEmbed(user.Component.Message, msg, Color.Green).ConfigureAwait(false);
            LogUtil.LogInfo($"{user.BotName}: Successfully authenticated with the server, downloading data from {user.SeedCheckerName} ({user.SeedCheckerID})...", "[EtumrepMMO Handler]");

            var att = user.Component.Message.Attachments.ToArray();
            byte[] data = new byte[376 * att.Length];
            var client = new HttpClient();

            for (int i = 0; i < att.Length; i++)
            {
                var download = await client.GetByteArrayAsync(att[i].Url).ConfigureAwait(false);
                download.CopyTo(data, 376 * i);
            }

            user.Data = data;
        }

        private static async Task<bool> EtumrepRequest(EtumrepUser user)
        {

            var msg = "Sending data to server, beginning seed calculation. Please wait...";
            await UpdateEtumrepEmbed(user.Component.Message, msg, Color.DarkGreen).ConfigureAwait(false);
            LogUtil.LogInfo($"{user.BotName}: Sending data from {user.SeedCheckerName} to server for seed calculation.", "[EtumrepMMO Queue]");

            try
            {
                await user.Stream.WriteAsync(user.Data).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                DisposeStream(user);
                var msgE = $"Data communication with the server failed.\n{ex.Message}";
                await UpdateEtumrepEmbed(user.Component.Message, msgE, Color.Red).ConfigureAwait(false);
                LogUtil.LogInfo($"{user.BotName}: {msgE} ({user.SeedCheckerName} - {user.SeedCheckerID})", "[EtumrepMMO Queue]");
                return false;
            }

            byte[] buffer = new byte[8];
            try
            {
                await user.Stream.ReadAsync(buffer).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                DisposeStream(user);
                var msgE = $"Failed to retrieve the result from the server.\n{ex.Message}";
                await UpdateEtumrepEmbed(user.Component.Message, msgE, Color.Red).ConfigureAwait(false);
                LogUtil.LogInfo($"{user.BotName}: {msgE} ({user.SeedCheckerName} - {user.SeedCheckerID})", "[EtumrepMMO Queue]");
                return false;
            }

            DisposeStream(user);
            var seed = BitConverter.ToUInt64(buffer, 0);

            if (seed is 0)
            {
                msg = "Failed to calculate seed. Please make sure shown Pokémon are the first four spawns, and have come from an MO or MMO.";
                await UpdateEtumrepEmbed(user.Component.Message, msg, Color.Gold).ConfigureAwait(false);
                LogUtil.LogInfo($"{user.BotName}: Seed calculation for {user.SeedCheckerName} returned no valid results.", "[EtumrepMMO Queue]");
            }
            else
            {
                var components = new ComponentBuilder();
                var buttonYes = new ButtonBuilder() { CustomId = "permute_yes", Label = "Yes", Style = ButtonStyle.Primary };
                components.WithButton(buttonYes);

                var buttonNo = new ButtonBuilder() { CustomId = "permute_no", Label = "No", Style = ButtonStyle.Secondary };
                components.WithButton(buttonNo);

                var seedMsg = $"Your seed is `{seed}`";
                msg = $"Result received! {seedMsg}\nWould you like to calculate your shiny paths using PermuteMMO?";
                await UpdateEtumrepEmbed(user.Component.Message, msg, Color.Gold, components.Build(), seedMsg).ConfigureAwait(false);
                LogUtil.LogInfo($"{user.BotName}: Seed calculation for {user.SeedCheckerName} completed successfully.", "[EtumrepMMO Queue]");
            }

            return true;
        }

        private static async Task UpdateEtumrepEmbed(SocketUserMessage message, string desc, Color color, MessageComponent? components = null, string? seed = null)
        {
            var embed = new EmbedBuilder
            {
                Color = color,
                Description = desc,
            }.WithAuthor(x => { x.Name = "EtumrepMMO Service"; });

            await message.ModifyAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = components;
                x.Content = seed;
            }).ConfigureAwait(false);
        }

        private static async Task<bool> GetServerConfirmation(EtumrepUser user, string reasonIfFailed)
        {
            var conf = new byte[1];
            try
            {
                // Wait for confirmation to proceed.
                await user.Stream.ReadAsync(conf.AsMemory(0, 1)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo($"{user.BotName}: Error while waiting for server confirmation. Dequeueing {user.SeedCheckerName}...\n{ex.Message}", "[Server Confirmation]");
                await UpdateEtumrepEmbed(user.Component.Message, reasonIfFailed, Color.Red).ConfigureAwait(false);
                return false;
            }

            bool success = BitConverter.ToBoolean(conf, 0);
            if (!success)
            {
                await UpdateEtumrepEmbed(user.Component.Message, reasonIfFailed, Color.Red).ConfigureAwait(false);
                LogUtil.LogInfo($"{user.BotName}: {reasonIfFailed}", "[Server Confirmation]");
            }

            return success;
        }

        private static void DisposeStream(EtumrepUser user)
        {
            try
            {
                user.Client.Close();
                user.Stream.Dispose();
            }
            catch (Exception ex)
            {
                string msg = $"Error occurred while disposing the connection stream.\n{ex.Message}";
                LogUtil.LogInfo(msg, "[DisposeStream]");
            }
        }
    }
}
