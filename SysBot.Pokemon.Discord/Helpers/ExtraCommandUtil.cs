using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SysBot.Pokemon.Discord
{
    public class ExtraCommandUtil
    {
        private static readonly Dictionary<ulong, ReactMessageContents> ReactMessageDict = new();
        private static bool DictWipeRunning = false;

        private class ReactMessageContents
        {
            public List<string> Pages { get; set; } = new();
            public EmbedBuilder Embed { get; set; } = new();
            public ulong MessageID { get; set; }
            public DateTime EntryTime { get; set; }
        }

        public async Task ListUtil(SocketCommandContext ctx, string nameMsg, string entry)
        {
            List<string> pageContent = TradeExtensions.ListUtilPrep(entry);
            bool canReact = ctx.Guild.CurrentUser.GetPermissions(ctx.Channel as IGuildChannel).AddReactions;
            var embed = new EmbedBuilder { Color = Color.DarkBlue }.AddField(x =>
            {
                x.Name = nameMsg;
                x.Value = pageContent[0];
                x.IsInline = false;
            }).WithFooter(x =>
            {
                x.IconUrl = "https://i.imgur.com/nXNBrlr.png";
                x.Text = $"Page 1 of {pageContent.Count}";
            });

            if (!canReact && pageContent.Count > 1)
            {
                embed.AddField(x =>
                {
                    x.Name = "Missing \"Add Reactions\" Permission";
                    x.Value = "Displaying only the first page of the list due to embed field limits.";
                });
            }

            var msg = await ctx.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            if (pageContent.Count > 1 && canReact)
            {
                bool exists = ReactMessageDict.TryGetValue(ctx.User.Id, out _);
                if (exists)
                    ReactMessageDict[ctx.User.Id] = new() { Embed = embed, Pages = pageContent, MessageID = msg.Id, EntryTime = DateTime.Now };
                else ReactMessageDict.Add(ctx.User.Id, new() { Embed = embed, Pages = pageContent, MessageID = msg.Id, EntryTime = DateTime.Now });

                IEmote[] reactions = { new Emoji("⬅️"), new Emoji("➡️"), new Emoji("⬆️"), new Emoji("⬇️") };
                _ = Task.Run(() => msg.AddReactionsAsync(reactions).ConfigureAwait(false));
                if (!DictWipeRunning)
                    _ = Task.Run(() => DictWipeMonitor().ConfigureAwait(false));
            }
        }

        private async Task DictWipeMonitor()
        {
            DictWipeRunning = true;
            while (true)
            {
                for (int i = 0; i < ReactMessageDict.Count; i++)
                {
                    var entry = ReactMessageDict.ElementAt(i);
                    var delta = (DateTime.Now - entry.Value.EntryTime).TotalSeconds;
                    if (delta > 120.0)
                        ReactMessageDict.Remove(entry.Key);
                }

                await Task.Delay(10_000).ConfigureAwait(false);
            }
        }

        public static async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cachedMsg, ISocketMessageChannel _, SocketReaction reaction)
        {
            if (!TradeExtensions.TCInitialized || !reaction.User.IsSpecified)
                return;

            var user = reaction.User.Value;
            if (user.IsBot || !ReactMessageDict.ContainsKey(user.Id))
                return;

            IUserMessage msg;
            if (!cachedMsg.HasValue)
                msg = await cachedMsg.GetOrDownloadAsync().ConfigureAwait(false);
            else msg = cachedMsg.Value;

            if (msg.Embeds.Count < 1)
                return;

            bool invoker = msg.Embeds.First().Fields[0].Name.Contains(user.Username);
            if (!invoker)
                return;

            IEmote[] reactions = { new Emoji("⬅️"), new Emoji("➡️"), new Emoji("⬆️"), new Emoji("⬇️") };
            if (!reactions.Contains(reaction.Emote))
                return;

            var contents = ReactMessageDict[user.Id];
            bool oldMessage = msg.Id != contents.MessageID;
            if (oldMessage)
                return;

            int page = contents.Pages.IndexOf((string)contents.Embed.Fields[0].Value);
            if (reaction.Emote.Name == reactions[0].Name || reaction.Emote.Name == reactions[1].Name)
            {
                if (reaction.Emote.Name == reactions[0].Name)
                {
                    if (page == 0)
                        page = contents.Pages.Count - 1;
                    else page--;
                }
                else
                {
                    if (page + 1 == contents.Pages.Count)
                        page = 0;
                    else page++;
                }

                contents.Embed.Fields[0].Value = contents.Pages[page];
                contents.Embed.Footer.Text = $"Page {page + 1} of {contents.Pages.Count}";
                await msg.RemoveReactionAsync(reactions[reaction.Emote.Name == reactions[0].Name ? 0 : 1], user);
                await msg.ModifyAsync(msg => msg.Embed = contents.Embed.Build()).ConfigureAwait(false);
            }
            else if (reaction.Emote.Name == reactions[2].Name || reaction.Emote.Name == reactions[3].Name)
            {
                List<string> tempList = new();
                foreach (var p in contents.Pages)
                {
                    var split = p.Replace(", ", ",").Split(',');
                    tempList.AddRange(split);
                }

                var tempEntry = string.Join(", ", reaction.Emote.Name == reactions[2].Name ? tempList.OrderBy(x => x.Split(' ')[1]) : tempList.OrderByDescending(x => x.Split(' ')[1]));
                contents.Pages = TradeExtensions.ListUtilPrep(tempEntry);
                contents.Embed.Fields[0].Value = contents.Pages[page];
                contents.Embed.Footer.Text = $"Page {page + 1} of {contents.Pages.Count}";
                await msg.RemoveReactionAsync(reactions[reaction.Emote.Name == reactions[2].Name ? 2 : 3], user);
                await msg.ModifyAsync(msg => msg.Embed = contents.Embed.Build()).ConfigureAwait(false);
            }
        }

        public async Task<bool> ReactionVerification(SocketCommandContext ctx)
        {
            var sw = new Stopwatch();
            IEmote reaction = new Emoji("👍");
            var msg = await ctx.Channel.SendMessageAsync($"{ctx.User.Username}, please react to the attached emoji in order to confirm you're not using a script.").ConfigureAwait(false);
            await msg.AddReactionAsync(reaction).ConfigureAwait(false);

            sw.Start();
            while (sw.ElapsedMilliseconds < 20_000)
            {
                await msg.UpdateAsync().ConfigureAwait(false);
                var react = msg.Reactions.FirstOrDefault(x => x.Value.ReactionCount > 1 && x.Value.IsMe);
                if (react.Key == default)
                    continue;

                if (react.Key.Name == reaction.Name)
                {
                    var reactUsers = await msg.GetReactionUsersAsync(reaction, 100).FlattenAsync().ConfigureAwait(false);
                    var usr = reactUsers.FirstOrDefault(x => x.Id == ctx.User.Id && !x.IsBot);
                    if (usr == default)
                        continue;

                    await msg.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
                    return false;
                }
            }
            await msg.AddReactionAsync(new Emoji("❌")).ConfigureAwait(false);
            TradeExtensions.MuteList.Add(ctx.User.Id);
            return true;
        }

        public async Task<int> EventVoteCalc(SocketCommandContext ctx, List<PokeEventType> events)
        {
            IEmote[] reactions = { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"), new Emoji("5️⃣") };
            string text = "The community vote has started! You have 30 seconds to vote for the next event!\n";
            for (int i = 0; i < events.Count; i++)
                text += $"{i + 1}. {events[i]}\n";

            var embed = new EmbedBuilder { Color = Color.DarkBlue }.AddField(x =>
            {
                x.Name = "Community Event Vote";
                x.Value = text;
                x.IsInline = false;
            });

            var msg = await ctx.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            await msg.AddReactionsAsync(reactions).ConfigureAwait(false);

            await Task.Delay(30_000).ConfigureAwait(false);
            await msg.UpdateAsync().ConfigureAwait(false);
            List<int> reactList = new();
            for (int i = 0; i < 5; i++)
                reactList.Add(msg.Reactions.Values.ToArray()[i].ReactionCount);
            return reactList.IndexOf(reactList.Max());
        }

        public async Task EmbedUtil(SocketCommandContext ctx, string name, string value, EmbedBuilder? embed = null)
        {
            if (embed == null)
                embed = new EmbedBuilder { Color = Color.DarkBlue };

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
    }
}