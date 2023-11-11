using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using PKHeX.Core;
using SysBot.Base;
using System.Threading;

namespace SysBot.Pokemon.Discord
{
    public class ExtraCommandUtil<T> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
        private static readonly PokeTradeHubConfig Config = Info.Hub.Config;
        private static readonly Dictionary<ulong, ReactMessageContents> ReactMessageDict = new();
        private static bool DictWipeRunning = false;
        private static readonly IEmote[] Reactions = { new Emoji("⬅️"), new Emoji("➡️") };
        private class ReactMessageContents
        {
            public List<string> Pages { get; set; } = new();
            public EmbedBuilder Embed { get; set; } = new();
            public ulong MessageID { get; set; }
            public DateTime EntryTime { get; set; }
        }

        public static List<string> GetPageContent(List<string> content, int pageNumber, int itemsPerPage)
        {
            int skipItems = (pageNumber - 1) * itemsPerPage;
            return content.Skip(skipItems).Take(itemsPerPage).ToList();
        }

        public static async Task ListUtil(SocketCommandContext ctx, string nameMsg, List<string> pageContent)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            LogUtil.LogText($"Starting ListUtil for {nameMsg}.");

            bool canReact = ctx.Guild.CurrentUser.GetPermissions(ctx.Channel as IGuildChannel).AddReactions;
            LogUtil.LogText($"Checked permissions: {canReact}");

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = ctx.User.Username,
                    IconUrl = ctx.User.GetAvatarUrl()
                },
                Color = GetBorderColor(false) // Use the instance to access GetBorderColor
            }
            .AddField(nameMsg, pageContent[0], false)
            .WithFooter($"Page 1 of {pageContent.Count}", "https://i.imgur.com/nXNBrlr.png")
            .WithThumbnailUrl("https://i.imgur.com/5akyLET.png") // Add this line for thumbnail
            .WithCurrentTimestamp();

            var msg = await ctx.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            LogUtil.LogText($"Message sent. Took {sw.ElapsedMilliseconds} ms");

            if (pageContent.Count > 1 && canReact)
            {
                ReactMessageContents newContents = new() { Embed = embed, Pages = pageContent, MessageID = msg.Id, EntryTime = DateTime.UtcNow };
                ReactMessageDict[ctx.User.Id] = newContents;

                // Use only left and right arrows
                var minimalReactions = new[] { Reactions[0], Reactions[1] };

                // Offload the reaction-adding to a separate task
                _ = Task.Run(async () =>
                {
                    var reactionTasks = minimalReactions.Select(r => msg.AddReactionAsync(r));
                    await Task.WhenAll(reactionTasks).ConfigureAwait(false);
                    LogUtil.LogText($"Reactions added. Took {sw.ElapsedMilliseconds} ms");
                });

                if (!DictWipeRunning)
                {
                    DictWipeRunning = true;
                    _ = Task.Run(DictWipeMonitor);
                }
            }

            sw.Stop();
            LogUtil.LogText($"ListUtil completed in {sw.ElapsedMilliseconds} ms.");
        }

        private static async Task DictWipeMonitor()
        {
            DictWipeRunning = true;
            while (true)
            {
                await Task.Delay(10_000).ConfigureAwait(false);
                for (int i = 0; i < ReactMessageDict.Count; i++)
                {
                    var entry = ReactMessageDict.ElementAt(i);
                    var delta = (DateTime.Now - entry.Value.EntryTime).TotalSeconds;
                    if (delta > 90.0)
                        ReactMessageDict.Remove(entry.Key);
                }
            }
        }


        public static async Task HandleReactionAsync(Cacheable<IUserMessage, UInt64> cachedMessage, Cacheable<IMessageChannel, UInt64> channel, SocketReaction reaction)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            LogUtil.LogText("HandleReactionAsync started.");

            try
            {
                // Get the message from the cache or download it if not available
                IUserMessage msg = cachedMessage.HasValue ? cachedMessage.Value : await cachedMessage.GetOrDownloadAsync().ConfigureAwait(false);

                // Get the user who reacted
                var user = reaction.User.Value;

                // Exit if the user is a bot or not in the dictionary
                if (user.IsBot || !ReactMessageDict.TryGetValue(user.Id, out var contents))
                    return;

                // Check if the message corresponds to the current user's paginated message
                bool invoker = msg.Embeds.First().Fields[0].Name == contents.Embed.Fields[0].Name;
                if (!invoker)
                    return;

                // Ensure the message IDs match to avoid outdated references
                bool oldMessage = msg.Id != contents.MessageID;
                if (oldMessage)
                    return;

                // Get the current page index
                int page = contents.Pages.IndexOf((string)contents.Embed.Fields[0].Value);
                if (page == -1)
                    return;

                // Handle page navigation based on the reaction
                if (reaction.Emote.Name == "⬅️" || reaction.Emote.Name == "➡️")
                {
                    switch (reaction.Emote.Name)
                    {
                        case "⬅️":
                            page = (page == 0) ? contents.Pages.Count - 1 : page - 1;
                            break;

                        case "➡️":
                            page = (page + 1 == contents.Pages.Count) ? 0 : page + 1;
                            break;

                        default:
                            return;
                    }

                    // Update the embed with the new page content and footer
                    contents.Embed.Fields[0].Value = contents.Pages[page];
                    contents.Embed.Footer.Text = $"Page {page + 1} of {contents.Pages.Count}";
                    await msg.RemoveReactionAsync(reaction.Emote, user).ConfigureAwait(false);
                    await msg.ModifyAsync(msg => msg.Embed = contents.Embed.Build()).ConfigureAwait(false);
                }

                sw.Stop();
                LogUtil.LogText($"HandleReactionAsync completed in {sw.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                sw.Stop();
                var msg = $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}";
                LogUtil.LogError(msg, "[HandleReactionAsync Event]");
                LogUtil.LogText($"HandleReactionAsync failed in {sw.ElapsedMilliseconds} ms.");
            }
        }

        public async Task<bool> ReactionVerification(SocketCommandContext ctx)
        {
            IEmote reaction = new Emoji("👍");
            var msg = await ctx.Channel.SendMessageAsync($"{ctx.User.Username}, please react to the attached emoji in order to confirm you're not using a script.").ConfigureAwait(false);
            await msg.AddReactionAsync(reaction).ConfigureAwait(false);

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(20000);

            try
            {
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    await msg.UpdateAsync().ConfigureAwait(false);
                    var react = msg.Reactions.FirstOrDefault(x => x.Value.ReactionCount > 1 && x.Value.IsMe);
                    if (react.Key != default && react.Key.Name == reaction.Name)
                    {
                        var reactUsers = await msg.GetReactionUsersAsync(reaction, 100).FlattenAsync().ConfigureAwait(false);
                        var usr = reactUsers.FirstOrDefault(x => x.Id == ctx.User.Id && !x.IsBot);
                        if (usr != default)
                        {
                            await msg.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
                            return false;
                        }
                    }
                    await Task.Delay(500, tokenSource.Token).ConfigureAwait(false);  // Optional: Add a short delay to reduce API calls
                }
            }
            catch (TaskCanceledException)
            {

                // Task was cancelled because timeout reached.
            }

            await msg.AddReactionAsync(new Emoji("❌")).ConfigureAwait(false);
            return true;
        }



        public async Task EmbedUtil(SocketCommandContext ctx, string name, string value, EmbedBuilder? embed = null)
        {
            embed ??= new EmbedBuilder { Color = GetBorderColor(false) };

            var splitName = name.Split(new string[] { "&^&" }, StringSplitOptions.None);
            var splitValue = value.Split(new string[] { "&^&" }, StringSplitOptions.None);

            for (int i = 0; i < splitName.Length; i++)
            {
                embed.AddField(x =>
                {
                    x.Name = splitName[i];
                    x.Value = splitValue[i];
                    x.IsInline = false;
                });
            }
            await ctx.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }


        public static Task ButtonExecuted(SocketMessageComponent component)
        {
            _ = Task.Run(async () =>
            {
                var id = component.Data.CustomId;
                if (id.Contains("etumrep") && !component.HasResponded)
                {
                    try
                    {
                        await component.DeferAsync().ConfigureAwait(false);
                        await EtumrepUtil.HandleEtumrepRequestAsync(component, id).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        var msg = $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}";
                        Base.LogUtil.LogError(msg, "[ButtonExecuted Event]");
                    }
                }
                else if (id.Contains("permute"))
                {
                    var service = id.Contains(';') ? id.Split(';')[1] : "";
                    await PermuteUtil.HandlePermuteRequestAsync(component, service, id).ConfigureAwait(false);
                }
            });

            return Task.CompletedTask;
        }

        public static Task SelectMenuExecuted(SocketMessageComponent component)
        {
            _ = Task.Run(async () =>
            {
                var id = component.Data.CustomId;
                string service = id.Contains(';') ? id.Split(';')[1] : component.Data.Values.First() ?? "";
                await component.DeferAsync().ConfigureAwait(false);

                if (id.Contains("permute_json_filter"))
                    await PermuteUtil.HandlePermuteButtonAsync(component, service).ConfigureAwait(false);
                else if (id.Contains("permute_json_select"))
                    await PermuteUtil.HandlePermuteRequestAsync(component, service, id).ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        public static Task ModalSubmitted(SocketModal modal)
        {
            _ = Task.Run(async () =>
            {
                await modal.DeferAsync().ConfigureAwait(false);
                var id = modal.Data.CustomId;
                string service = id.Contains(';') ? id.Split(';')[1] : "";
                if (id.Contains("permute_json"))
                    await PermuteUtil.VerifyAndRunPermuteAsync(modal, service).ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        private static List<string> SpliceAtWord(string entry, int start, int length)
        {
            int counter = 0;
            List<string> list = new();
            var temp = entry.Contains(',') ? entry.Split(',').Skip(start) : entry.Contains('|') ? entry.Split('|').Skip(start) : entry.Split('\n').Skip(start);

            if (entry.Length < length)
            {
                list.Add(entry ?? "");
                return list;
            }

            foreach (var line in temp)
            {
                counter += line.Length + 2;
                if (counter < length)
                    list.Add(line.Trim());
                else break;
            }
            return list;
        }

        public static List<string> ListUtilPrep(string entry)
        {
            List<string> pageContent = new();
            if (entry.Length > 320)
            {
                var index = 0;
                while (true)
                {
                    var splice = SpliceAtWord(entry, index, 320);
                    if (splice.Count == 0)
                        break;

                    index += splice.Count;
                    pageContent.Add(string.Join(entry.Contains(',') ? ", " : entry.Contains('|') ? " | " : "\n", splice));
                }
            }
            else pageContent.Add(entry == "" ? "No results found." : entry);
            return pageContent;
        }

        public static Color GetBorderColor(bool gift, PKM? pkm = null)
        {
            bool swsh = typeof(T) == typeof(PK8);
            if (pkm is null && swsh)
                return gift ? Color.Purple : Color.Blue;
            else if (pkm is null && !swsh)
                return gift ? Color.DarkPurple : Color.DarkBlue;
            else if (pkm is not null && swsh)
                return (pkm.IsShiny && pkm.FatefulEncounter) || pkm.ShinyXor == 0 ? Color.Gold : pkm.IsShiny ? Color.LightOrange : Color.Teal;
            else if (pkm is not null && !swsh)
                return (pkm.IsShiny && pkm.FatefulEncounter) || pkm.ShinyXor == 0 ? Color.Gold : pkm.IsShiny ? Color.DarkOrange : Color.DarkTeal;
            throw new NotImplementedException();
        }
    }
}