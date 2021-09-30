using System;
using Discord;
using PKHeX.Core;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SysBot.Pokemon.Discord
{
    [Summary("Commands for Lair Bot.")]
    public class LairModule : ModuleBase<SocketCommandContext>
    {
        [Command("hunt")]
        [Alias("h", "find", "encounter")]
        [Summary("Hunt the specified Pokémon species. Enter without spaces or symbols.")]
        [RequireSudo]
        public async Task Hunt([Summary("Sets the Lair Pokémon Species")] string species)
        {
            var parse = EnumParse<LairSpecies>(species);
            if (parse == default)
            {
                await ReplyAsync("Not a valid Lair Species.").ConfigureAwait(false);
                return;
            }

            SysCordInstance.Self.Hub.Config.Lair.LairSpecies = parse;
            var msg = $"{Context.User.Mention} Legendary Species has been set to {parse}.";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("huntBulk")]
        [Alias("hb")]
        [Summary("Sets all three Scientist Notes. Enter all three species without spaces or symbols in their names; species separated by spaces.")]
        [RequireSudo]
        public async Task Hunt([Summary("Sets the Lair Pokémon Species in bulk.")] string species1, string species2, string species3)
        {
            string[] input = new string[] { species1, species2, species3 };
            for (int i = 0; i < input.Length; i++)
            {
                var parse = EnumParse<LairSpecies>(input[i]);
                if (parse == default)
                {
                    await ReplyAsync($"{input[i]} is not a valid Lair Species.").ConfigureAwait(false);
                    return;
                }

                SysCordInstance.Self.Hub.Config.Lair.LairSpeciesQueue[i] = parse;
                if (i == 2)
                {
                    LairBotUtil.DiscordQueueOverride = true;
                    var msg = $"{Context.User.Mention} Lair Species have been set to {string.Join(", ", SysCordInstance.Self.Hub.Config.Lair.LairSpeciesQueue)}.";
                    await ReplyAsync(msg).ConfigureAwait(false);
                }
            }
        }

        [Command("catchlairmons")]
        [Alias("clm", "catchlair")]
        [Summary("Toggle to catch lair encounters (Legendary will always be caught).")]
        [RequireSudo]
        public async Task ToggleCatchLairMons()
        {
            SysCordInstance.Self.Hub.Config.Lair.CatchLairPokémon ^= true;
            var msg = SysCordInstance.Self.Hub.Config.Lair.CatchLairPokémon ? "Catching Lair Pokémon!" : "Not Catching Lair Pokémon!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("resetlegendflag")]
        [Alias("rlf", "resetlegend", "legendreset")]
        [Summary("Toggle the Legendary Caught Flag reset.")]
        [RequireSudo]
        public async Task ToggleResetLegendaryCaughtFlag()
        {
            SysCordInstance.Self.Hub.Config.Lair.ResetLegendaryCaughtFlag ^= true;
            var msg = SysCordInstance.Self.Hub.Config.Lair.ResetLegendaryCaughtFlag ? "Legendary Caught Flag Enabled!" : "Legendary Caught Flag Disabled!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("setLairBall")]
        [Alias("slb", "setBall")]
        [Summary("Set the ball for catching Lair Pokémon.")]
        [RequireSudo]
        public async Task SetLairBall([Summary("Sets the ball for catching Lair Pokémon.")] string ball)
        {
            var parse = EnumParse<Ball>(ball);
            if (parse == default)
            {
                await ReplyAsync("Not a valid ball. Correct format is, for example, \"$slb Love\".").ConfigureAwait(false);
                return;
            }

            SysCordInstance.Self.Hub.Config.Lair.LairBall = parse;
            var msg = $"Now catching in {parse} Ball!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("lairEmbed")]
        [Alias("le")]
        [Summary("Initialize posting of Lair shiny result embeds to specified Discord channels.")]
        [RequireSudo]
        public async Task InitializeEmbeds()
        {
            if (SysCordInstance.Self.Hub.Config.Lair.ResultsEmbedChannels == string.Empty)
            {
                await ReplyAsync("No channels to post embeds in.").ConfigureAwait(false);
                return;
            }

            List<ulong> channels = new();
            foreach (var channel in SysCordInstance.Self.Hub.Config.Lair.ResultsEmbedChannels.Split(',', ' '))
            {
                if (ulong.TryParse(channel, out ulong result) && !channels.Contains(result))
                    channels.Add(result);
            }

            if (channels.Count == 0)
            {
                await ReplyAsync("No valid channels found.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync(!LairBotUtil.EmbedsInitialized ? "Lair Embed task started!" : "Lair Embed task stopped!").ConfigureAwait(false);
            if (LairBotUtil.EmbedsInitialized)
                LairBotUtil.EmbedSource.Cancel();
            else _ = Task.Run(async () => await LairEmbedLoop(channels));
            LairBotUtil.EmbedsInitialized ^= true;
        }

        private T EnumParse<T>(string input) where T : struct, Enum => !Enum.TryParse(input, true, out T result) ? new() : result;

        private async Task LairEmbedLoop(List<ulong> channels)
        {
            while (!LairBotUtil.EmbedSource.IsCancellationRequested)
            {
                if (LairBotUtil.EmbedMon.Item1 != null)
                {
                    var url = TradeExtensions.PokeImg(LairBotUtil.EmbedMon.Item1, LairBotUtil.EmbedMon.Item1.CanGigantamax, false);
                    var ballStr = $"{(Ball)LairBotUtil.EmbedMon.Item1.Ball}".ToLower();
                    var ballUrl = $"https://serebii.net/itemdex/sprites/pgl/{ballStr}ball.png";
                    var ping = SysCordInstance.Self.Hub.Config.StopConditions.PingOnMatch != string.Empty ? $"<@{SysCordInstance.Self.Hub.Config.StopConditions.PingOnMatch}>" : "";
                    var author = new EmbedAuthorBuilder { IconUrl = ballUrl, Name = LairBotUtil.EmbedMon.Item2 ? "Legendary Caught!" : "Result found, but not quite Legendary!" };
                    var embed = new EmbedBuilder { Color = Color.Blue, ThumbnailUrl = url }.WithAuthor(author).WithDescription(ShowdownParsing.GetShowdownText(LairBotUtil.EmbedMon.Item1));

                    if (ulong.TryParse(SysCordInstance.Self.Hub.Config.StopConditions.PingOnMatch, out ulong usr))
                    {
                        var user = await Context.Client.Rest.GetUserAsync(usr).ConfigureAwait(false);
                        embed.WithFooter(x => { x.Text = $"Requested by: {user}"; });
                    }

                    foreach (var guild in Context.Client.Guilds)
                    {
                        foreach (var channel in channels)
                        {
                            if (guild.Channels.FirstOrDefault(x => x.Id == channel) != default)
                                await guild.GetTextChannel(channel).SendMessageAsync(ping, embed: embed.Build()).ConfigureAwait(false);
                        }
                    }
                    LairBotUtil.EmbedMon.Item1 = null;
                }
                else await Task.Delay(1_000).ConfigureAwait(false);
            }
            LairBotUtil.EmbedSource = new();
        }
    }
}
