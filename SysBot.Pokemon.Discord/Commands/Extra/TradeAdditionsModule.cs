﻿using PKHeX.Core;
using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Collections;

namespace SysBot.Pokemon.Discord
{
    [Summary("Generates and queues various silly trade additions")]
    public class TradeAdditionsModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
        private readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;
        private readonly ExtraCommandUtil<T> Util = new();
        private readonly LairBotSettings LairSettings = SysCord<T>.Runner.Hub.Config.LairSWSH;
        private readonly RollingRaidSettings RollingRaidSettings = SysCord<T>.Runner.Hub.Config.RollingRaidSWSH;
        private readonly ArceusBotSettings ArceusSettings = SysCord<T>.Runner.Hub.Config.ArceusLA;
        private static RaidSettingsSV RaidSettingsSV = SysCord<T>.Runner.Hub.Config.RaidSV;

        [Command("giveawayqueue")]
        [Alias("gaq")]
        [Summary("Prints the users in the giveway queues.")]
        [RequireSudo]
        public async Task GetGiveawayListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Giveaways";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("giveawaypool")]
        [Alias("gap")]
        [Summary("Show a list of Pokémon available for giveaway.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task DisplayGiveawayPoolCountAsync()
        {
            var pool = Info.Hub.Ledy.Pool;
            if (pool.Count > 0)
            {
                var test = pool.Files;
                var lines = pool.Files.Select((z, i) => $"{i + 1}: {z.Key} = {(Species)z.Value.RequestInfo.Species}");
                var msg = string.Join("\n", lines);
                await Util.ListUtil(Context, "Giveaway Pool Details", msg).ConfigureAwait(false);
            }
            else await ReplyAsync("Giveaway pool is empty.").ConfigureAwait(false);
        }

        [Command("giveaway")]
        [Alias("ga", "giveme", "gimme")]
        [Summary("Makes the bot trade you the specified giveaway Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task GiveawayAsync([Remainder] string content)
        {
            var code = Info.GetRandomTradeCode();
            await GiveawayAsync(code, content).ConfigureAwait(false);
        }

        [Command("giveaway")]
        [Alias("ga", "giveme", "gimme")]
        [Summary("Makes the bot trade you the specified giveaway Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task GiveawayAsync([Summary("Giveaway Code")] int code, [Remainder] string content)
        {
            T pk;
            content = ReusableActions.StripCodeBlock(content);
            var pool = Info.Hub.Ledy.Pool;
            if (pool.Count == 0)
            {
                await ReplyAsync("Giveaway pool is empty.").ConfigureAwait(false);
                return;
            }
            else if (content.ToLower() == "random") // Request a random giveaway prize.
                pk = Info.Hub.Ledy.Pool.GetRandomSurprise();
            else if (Info.Hub.Ledy.Distribution.TryGetValue(content, out LedyRequest<T>? val) && val is not null)
                pk = val.RequestInfo;
            else
            {
                await ReplyAsync($"Requested Pokémon not available, use \"{Info.Hub.Config.Discord.CommandPrefix}giveawaypool\" for a full list of available giveaways!").ConfigureAwait(false);
                return;
            }

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);
        }

        [Command("fixOT")]
        [Alias("fix", "f")]
        [Summary("Fixes OT and Nickname of a Pokémon you show via Link Trade if an advert is detected.")]
        [RequireQueueRole(nameof(DiscordManager.RolesFixOT))]
        public async Task FixAdOT()
        {
            var code = Info.GetRandomTradeCode();
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.FixOT, PokeTradeType.FixOT).ConfigureAwait(false);
        }

        [Command("fixOT")]
        [Alias("fix", "f")]
        [Summary("Fixes OT and Nickname of a Pokémon you show via Link Trade if an advert is detected.")]
        [RequireQueueRole(nameof(DiscordManager.RolesFixOT))]
        public async Task FixAdOT([Summary("Trade Code")] int code)
        {
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.FixOT, PokeTradeType.FixOT).ConfigureAwait(false);
        }

        [Command("fixOTList")]
        [Alias("fl", "fq")]
        [Summary("Prints the users in the FixOT queue.")]
        [RequireSudo]
        public async Task GetFixListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.FixOT);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("itemTrade")]
        [Alias("it", "item")]
        [Summary("Makes the bot trade you a Pokémon holding the requested item, or Ditto if stat spread keyword is provided.")]
        [RequireQueueRole(nameof(DiscordManager.RolesSupportTrade))]
        public async Task ItemTrade([Remainder] string item)
        {
            var code = Info.GetRandomTradeCode();
            await ItemTrade(code, item).ConfigureAwait(false);
        }

        [Command("itemTrade")]
        [Alias("it", "item")]
        [Summary("Makes the bot trade you a Pokémon holding the requested item.")]
        [RequireQueueRole(nameof(DiscordManager.RolesSupportTrade))]
        public async Task ItemTrade([Summary("Trade Code")] int code, [Remainder] string item)
        {
            Species species = Info.Hub.Config.Trade.ItemTradeSpecies == Species.None ? Species.Diglett : Info.Hub.Config.Trade.ItemTradeSpecies;
            var set = new ShowdownSet($"{SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 8)} @ {item.Trim()}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pkm = sav.GetLegal(template, out var result);
            pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
            if (pkm.HeldItem == 0 && !Info.Hub.Config.Trade.Memes)
            {
                await ReplyAsync($"{Context.User.Username}, the item you entered wasn't recognized.").ConfigureAwait(false);
                return;
            }

            var la = new LegalityAnalysis(pkm);
            if (Info.Hub.Config.Trade.Memes && await TrollAsync(Context, pkm is not T || !la.Valid, pkm, true).ConfigureAwait(false))
                return;
            
            if (pkm is not T pk || !la.Valid)
            {
                var reason = result == "Timeout" ? "That set took too long to generate." : "I wasn't able to create something from that.";
                var imsg = $"Oops! {reason} Here's my best attempt for that {species}!";
                await Context.Channel.SendPKMAsync(pkm, imsg).ConfigureAwait(false);
                return;
            }
            pk.ResetPartyStats();

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.SupportTrade).ConfigureAwait(false);
        }

        [Command("dittoTrade")]
        [Alias("dt", "ditto")]
        [Summary("Makes the bot trade you a Ditto with a requested stat spread and language.")]
        [RequireQueueRole(nameof(DiscordManager.RolesSupportTrade))]
        public async Task DittoTrade([Summary("A combination of \"ATK/SPA/SPE\" or \"6IV\"")] string keyword, [Summary("Language")] string language, [Summary("Nature")] string nature)
        {
            var code = Info.GetRandomTradeCode();
            await DittoTrade(code, keyword, language, nature).ConfigureAwait(false);
        }

        [Command("dittoTrade")]
        [Alias("dt", "ditto")]
        [Summary("Makes the bot trade you a Ditto with a requested stat spread and language.")]
        [RequireQueueRole(nameof(DiscordManager.RolesSupportTrade))]
        public async Task DittoTrade([Summary("Trade Code")] int code, [Summary("A combination of \"ATK/SPA/SPE\" or \"6IV\"")] string keyword, [Summary("Language")] string language, [Summary("Nature")] string nature)
        {
            keyword = keyword.ToLower().Trim();
            if (Enum.TryParse(language, true, out LanguageID lang))
                language = lang.ToString();
            else
            {
                await Context.Message.ReplyAsync($"Couldn't recognize language: {language}.").ConfigureAwait(false);
                return;
            }
            nature = nature.Trim()[..1].ToUpper() + nature.Trim()[1..].ToLower();
            var set = new ShowdownSet($"{keyword}(Ditto)\nLanguage: {language}\nNature: {nature}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pkm = sav.GetLegal(template, out var result);
            TradeExtensions<T>.DittoTrade((T)pkm);

            var la = new LegalityAnalysis(pkm);
            if (Info.Hub.Config.Trade.Memes && await TrollAsync(Context, pkm is not T || !la.Valid, pkm).ConfigureAwait(false))
                return;
            
            if (pkm is not T pk || !la.Valid)
            {
                var reason = result == "Timeout" ? "That set took too long to generate." : "I wasn't able to create something from that.";
                var imsg = $"Oops! {reason} Here's my best attempt for that Ditto!";
                await Context.Channel.SendPKMAsync(pkm, imsg).ConfigureAwait(false);
                return;
            }

            pk.ResetPartyStats();
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.SupportTrade).ConfigureAwait(false);
        }

        [Command("peek")]
        [Summary("Take and send a screenshot from the specified Switch.")]
        [RequireOwner]
        public async Task Peek(string address)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            var bot = SysCord<T>.Runner.GetBot(address);
            if (bot == null)
            {
                await ReplyAsync($"No bot found with the specified address ({address}).").ConfigureAwait(false);
                return;
            }

            var c = bot.Bot.Connection;
            var bytes = await c.Screengrab(token).ConfigureAwait(false) ?? Array.Empty<byte>();
            if (bytes.Length == 1)
            {
                await ReplyAsync($"Failed to take a screenshot for bot at {address}. Is the bot connected?").ConfigureAwait(false);
                return;
            }
            MemoryStream ms = new(bytes);

            var img = "cap.jpg";
            var embed = new EmbedBuilder{ ImageUrl = $"attachment://{img}", Color = Color.Purple }.WithFooter(new EmbedFooterBuilder { Text = $"Captured image from bot at address {address}." });
            await Context.Channel.SendFileAsync(ms, img, "", false, embed : embed.Build());
        }

        [Command("hunt")]
        [Alias("h")]
        [Summary("Sets all three Scientist Notes. Enter all three species without spaces or symbols in their names; species separated by spaces.")]
        [RequireSudo]
        public async Task Hunt([Summary("Sets the Lair Pokémon Species in bulk.")] string species1, string species2, string species3)
        {
            string[] input = new string[] { species1, species2, species3 };
            for (int i = 0; i < input.Length; i++)
            {
                var parse = TradeExtensions<T>.EnumParse<LairSpecies>(input[i]);
                if (parse == default)
                {
                    await ReplyAsync($"{input[i]} is not a valid Lair Species.").ConfigureAwait(false);
                    return;
                }

                LairSettings.LairSpeciesQueue[i] = parse;
                if (i == 2)
                {
                    LairBotUtil.DiscordQueueOverride = true;
                    var msg = $"{Context.User.Mention} Lair Species have been set to {string.Join(", ", LairSettings.LairSpeciesQueue)}.";
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
            LairSettings.CatchLairPokémon ^= true;
            var msg = LairSettings.CatchLairPokémon ? "Catching Lair Pokémon!" : "Not Catching Lair Pokémon!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("resetlegendflag")]
        [Alias("rlf", "resetlegend", "legendreset")]
        [Summary("Toggle the Legendary Caught Flag reset.")]
        [RequireSudo]
        public async Task ToggleResetLegendaryCaughtFlag()
        {
            LairSettings.ResetLegendaryCaughtFlag ^= true;
            var msg = LairSettings.ResetLegendaryCaughtFlag ? "Legendary Caught Flag Enabled!" : "Legendary Caught Flag Disabled!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("setLairBall")]
        [Alias("slb", "setBall")]
        [Summary("Set the ball for catching Lair Pokémon.")]
        [RequireSudo]
        public async Task SetLairBall([Summary("Sets the ball for catching Lair Pokémon.")] string ball)
        {
            var parse = TradeExtensions<T>.EnumParse<LairBall>(ball);
            if (parse == default)
            {
                await ReplyAsync("Not a valid ball. Correct format is, for example, \"$slb Love\".").ConfigureAwait(false);
                return;
            }

            LairSettings.LairBall = parse;
            var msg = $"Now catching in {parse} Ball!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("lairEmbed")]
        [Alias("le")]
        [Summary("Initialize posting of Lair shiny result embeds to specified Discord channels.")]
        [RequireSudo]
        public async Task InitializeEmbeds()
        {
            if (LairSettings.ResultsEmbedChannels == string.Empty)
            {
                await ReplyAsync("No channels to post embeds in.").ConfigureAwait(false);
                return;
            }

            List<ulong> channels = new();
            foreach (var channel in LairSettings.ResultsEmbedChannels.Split(',', ' '))
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

        private async Task LairEmbedLoop(List<ulong> channels)
        {
            var ping = SysCord<T>.Runner.Hub.Config.StopConditions.MatchFoundEchoMention;
            while (!LairBotUtil.EmbedSource.IsCancellationRequested)
            {
                if (LairBotUtil.EmbedMon.Item1 != null)
                {
                    var url = TradeExtensions<T>.PokeImg(LairBotUtil.EmbedMon.Item1, LairBotUtil.EmbedMon.Item1.CanGigantamax, false);
                    var ballStr = $"{(Ball)LairBotUtil.EmbedMon.Item1.Ball}".ToLower();
                    var ballUrl = $"https://serebii.net/itemdex/sprites/pgl/{ballStr}ball.png";
                    var author = new EmbedAuthorBuilder { IconUrl = ballUrl, Name = LairBotUtil.EmbedMon.Item2 ? "Legendary Caught!" : "Result found, but not quite Legendary!" };
                    var embed = new EmbedBuilder { Color = Color.Blue, ThumbnailUrl = url }.WithAuthor(author).WithDescription(ShowdownParsing.GetShowdownText(LairBotUtil.EmbedMon.Item1));

                    var userStr = ping.Replace("<@", "").Replace(">", "");
                    if (ulong.TryParse(userStr, out ulong usr))
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

        [Command("raidEmbed")]
        [Alias("re")]
        [Summary("Initialize posting of RollingRaidBot embeds to specified Discord channels.")]
        [RequireSudo]
        public async Task InitializeRaidEmbeds()
        {
            if (RollingRaidSettings.RollingRaidEmbedChannels == string.Empty)
            {
                await ReplyAsync("No channels to post embeds in.").ConfigureAwait(false);
                return;
            }

            List<ulong> channels = new();
            List<ITextChannel> embedChannels = new();
            if (!RollingRaidBot.RollingRaidEmbedsInitialized)
            {
                var chStrings = RollingRaidSettings.RollingRaidEmbedChannels.Split(',');
                foreach (var channel in chStrings)
                {
                    if (ulong.TryParse(channel, out ulong result) && !channels.Contains(result))
                        channels.Add(result);
                }

                if (channels.Count == 0)
                {
                    await ReplyAsync("No valid channels found.").ConfigureAwait(false);
                    return;
                }

                foreach (var guild in Context.Client.Guilds)
                {
                    foreach (var id in channels)
                    {
                        var channel = guild.Channels.FirstOrDefault(x => x.Id == id);
                        if (channel is not null && channel is ITextChannel ch)
                            embedChannels.Add(ch);
                    }
                }

                if (embedChannels.Count == 0)
                {
                    await ReplyAsync("No matching guild channels found.").ConfigureAwait(false);
                    return;
                }
            }

            RollingRaidBot.RollingRaidEmbedsInitialized ^= true;
            await ReplyAsync(!RollingRaidBot.RollingRaidEmbedsInitialized ? "RollingRaid Embed task stopped!" : "RollingRaid Embed task started!").ConfigureAwait(false);

            if (!RollingRaidBot.RollingRaidEmbedsInitialized)
            {
                RollingRaidBot.RaidEmbedSource.Cancel();
                return;
            }

            RollingRaidBot.RaidEmbedSource = new();
            _ = Task.Run(async () => await RollingRaidEmbedLoop(embedChannels).ConfigureAwait(false));
        }

        private static async Task RollingRaidEmbedLoop(List<ITextChannel> channels)
        {
            while (!RollingRaidBot.RaidEmbedSource.IsCancellationRequested)
            {
                if (RollingRaidBot.EmbedQueue.TryDequeue(out var embedInfo))
                {
                    var url = TradeExtensions<T>.PokeImg(embedInfo.Item1, embedInfo.Item1.CanGigantamax, false);
                    var embed = new EmbedBuilder
                    {
                        Title = embedInfo.Item3,
                        Description = embedInfo.Item2,
                        Color = Color.Blue,
                        ThumbnailUrl = url,
                    };

                    foreach (var channel in channels)
                    {
                        try
                        {
                            await channel.SendMessageAsync(null, false, embed: embed.Build()).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
                else await Task.Delay(0_500).ConfigureAwait(false);
            }
        }

        public static async Task<bool> TrollAsync(SocketCommandContext context, bool invalid, PKM pkm, bool itemTrade = false)
        {
            var rng = new Random();
            bool noItem = pkm.HeldItem == 0 && itemTrade;
            var path = Info.Hub.Config.Trade.MemeFileNames.Split(',');
            if (Info.Hub.Config.Trade.MemeFileNames == "" || path.Length == 0)
                path = new string[] { "https://i.imgur.com/qaCwr09.png" }; //If memes enabled but none provided, use a default one.

            if (invalid || !ItemRestrictions.IsHeldItemAllowed(pkm) || noItem || (pkm.Nickname.ToLower() == "egg" && !Breeding.CanHatchAsEgg(pkm.Species)))
            {
                var msg = $"{(noItem ? $"{context.User.Username}, the item you entered wasn't recognized." : $"Oops! I wasn't able to create that {GameInfo.Strings.Species[pkm.Species]}.")} Here's a meme instead!\n";
                await context.Channel.SendMessageAsync($"{(invalid || noItem ? msg : "")}{path[rng.Next(path.Length)]}").ConfigureAwait(false);
                return true;
            }
            return false;
        }


        // NotTrade Additions
        [Command("arceusEmbed")]
        [Alias("ae")]
        [Summary("Initialize posting of ArceusBot embeds to specified Discord channels.")]
        [RequireOwner]
        public async Task InitializArceusEmbeds()
        {
            if (ArceusSettings.ArceusEmbedChannels == string.Empty)
            {
                await ReplyAsync("No channels to post embeds in.").ConfigureAwait(false);
                return;
            }

            List<ulong> channels = new();
            foreach (var channel in ArceusSettings.ArceusEmbedChannels.Split(',', ' '))
            {
                if (ulong.TryParse(channel, out ulong result) && !channels.Contains(result))
                    channels.Add(result);
            }

            if (channels.Count == 0)
            {
                await ReplyAsync("No valid channels found.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync(!ArceusBot.EmbedsInitialized ? "Arceus Embed task started!" : "Arceus Embed task stopped!").ConfigureAwait(false);
            if (ArceusBot.EmbedsInitialized)
                ArceusBot.EmbedSource.Cancel();
            else _ = Task.Run(async () => await ArceusEmbedLoop(channels, ArceusBot.EmbedSource.Token).ConfigureAwait(false));
            ArceusBot.EmbedsInitialized ^= true;
        }

        private async Task ArceusEmbedLoop(List<ulong> channels, CancellationToken token)
        {
            var ping = SysCord<T>.Runner.Hub.Config.StopConditions.MatchFoundEchoMention;
            while (!ArceusBot.EmbedSource.IsCancellationRequested)
            {
                if (ArceusBot.EmbedMons.Count > 0)
                {
                    var mons = ArceusBot.EmbedMons;
                    for (int i = 0; i < mons.Count; i++)
                    {
                        if (mons[i].Item1 == null)
                        {
                            ArceusBot.EmbedMons.Remove(mons[i]);
                            continue;
                        }

                        PA8 mon = mons[i].Item1 ?? new();
                        var url = TradeExtensions<PA8>.PokeImg(mon, mon.CanGigantamax, SysCord<T>.Runner.Hub.Config.TradeCord.UseFullSizeImages);
                        string shinyurl = "https://img.favpng.com/6/14/25/computer-icons-icon-design-photography-royalty-free-png-favpng-mtjTHeWQe8FUAUB3RdJ3B2KJG.jpg";
                        var location = Hub.Config.ArceusLA.SpecialConditions.ScanLocation;
                        string msg = mons[i].Item2 ? "Match found!" : "Unwanted match...";
                        if (Hub.Config.ArceusLA.DistortionConditions.ShinyAlphaOnly && !mon.IsAlpha && Hub.Config.ArceusLA.BotType == ArceusMode.DistortionReader)
                            msg = "Not an Alpha...";

                        string stats;
                        if (Hub.Config.ArceusLA.SpeciesToHunt.Length == 0 || mon.IVTotal != 0)
                            stats = $"{(mon.ShinyXor == 0 ? "■ - " : mon.ShinyXor <= 16 ? "★ - " : "")}{SpeciesName.GetSpeciesNameGeneration(mon.Species, 2, 8)}{TradeExtensions<T>.FormOutput(mon.Species, mon.Form, out _)}\nIVs: {string.Join("/", mon.IVs)}";
                        else
                        {
                            stats = "Hunting for a special Alpha!";
                            shinyurl = "https://previews.123rf.com/images/fokaspokas/fokaspokas1809/fokaspokas180901587/109973633-loupe-search-or-magnifying-linear-icon-thin-outline-black-glass-icon-with-soft-shadow-on-transparent.jpg";
                        }

                        if (mon.IsAlpha)
                            shinyurl = "https://cdn.discordapp.com/emojis/944278189000228894.webp?size=96&quality=lossless";

                        var author = new EmbedAuthorBuilder { IconUrl = shinyurl, Name = msg };
                        var footer = new EmbedFooterBuilder
                        {
                            Text = Hub.Config.ArceusLA.BotType switch
                            {
                                ArceusMode.DistortionReader => "Found in a space-time distortion.",
                                ArceusMode.MassiveOutbreakHunter when Hub.Config.ArceusLA.OutbreakConditions.TypeOfScan == OutbreakScanType.OutbreakOnly => "Found in a mass outbreak.",
                                ArceusMode.MassiveOutbreakHunter when (Species)mon.Species is Species.Shieldon or Species.Bastiodon or Species.Cranidos or Species.Rampardos or Species.Scizor or Species.Sneasel or
                                Species.Weavile or Species.Magnemite or Species.Magneton or Species.Magnezone or Species.Sylveon or Species.Leafeon or Species.Glaceon or Species.Flareon or Species.Jolteon or Species.Vaporeon
                                or Species.Umbreon or Species.Espeon => "Found in a space-time distortion.",
                                _ => "Found in a massive mass outbreak.",
                            }
                        };

                        var embed = new EmbedBuilder { Color = Color.Gold, ThumbnailUrl = url }.WithAuthor(author).WithDescription(stats).WithFooter(footer);
                        var guilds = Context.Client.Guilds;
                        foreach (var guild in guilds)
                        {
                            foreach (var channel in channels)
                            {
                                var ch = guild.Channels.FirstOrDefault(x => x.Id == channel);
                                if (ch != default && ch is ISocketMessageChannel sock)
                                {
                                    try
                                    {
                                        await sock.SendMessageAsync(mons[i].Item2 ? ping : "", embed: embed.Build()).ConfigureAwait(false);
                                    }
                                    catch { }
                                }
                            }
                        }
                        ArceusBot.EmbedMons.Remove(mons[i]);
                    }
                }
                else await Task.Delay(1_000, token).ConfigureAwait(false);
            }
            ArceusBot.EmbedSource = new();
        }

        [Command("raidEmbedSV")]
        [Alias("resv")]
        [Summary("Initialize posting of RaidBotSV embeds to specified Discord channels.")]
        [RequireSudo]
        public async Task InitializeRaidEmbedsSV()
        {
            if (RaidSettingsSV.RaidEmbedChannelsSV == string.Empty)
            {
                await ReplyAsync("No channels to post embeds in.").ConfigureAwait(false);
                return;
            }

            List<ulong> channels = new();
            List<ITextChannel> embedChannels = new();

            if (!RaidSV.RaidSVEmbedsInitialized)
            {
                var chStrings = RaidSettingsSV.RaidEmbedChannelsSV.Split(',');
                foreach (var channel in chStrings)
                {
                    if (ulong.TryParse(channel, out ulong result) && !channels.Contains(result))
                        channels.Add(result);
                }

                if (channels.Count == 0)
                {
                    await ReplyAsync("No valid channels found.").ConfigureAwait(false);
                    return;
                }

                foreach (var guild in Context.Client.Guilds)
                {
                    foreach (var id in channels)
                    {
                        var channel = guild.Channels.FirstOrDefault(x => x.Id == id);
                        if (channel is not null && channel is ITextChannel ch)
                            embedChannels.Add(ch);
                    }
                }

                if (embedChannels.Count == 0)
                {
                    await ReplyAsync("No matching guild channels found.").ConfigureAwait(false);
                    return;
                }
            }

            RaidSV.RaidSVEmbedsInitialized ^= true;
            await ReplyAsync(!RaidSV.RaidSVEmbedsInitialized ? "Raid Embed task stopped!" : "Raid Embed task started!").ConfigureAwait(false);

            if (!RaidSV.RaidSVEmbedsInitialized)
            {
                RaidSV.RaidSVEmbedSource.Cancel();
                return;
            }

            RaidSV.RaidSVEmbedSource = new();
            _ = Task.Run(async () => await RaidSVEmbedLoop(embedChannels).ConfigureAwait(false));
        }

        private static async Task RaidSVEmbedLoop(List<ITextChannel> channels)
        {
            while (!RaidSV.RaidSVEmbedSource.IsCancellationRequested)
            {
                if (RaidSV.EmbedQueue.TryDequeue(out var embedInfo))
                {
                    var img = "zap.jpg";
                    var turl = string.Empty;
                    var form = string.Empty;                    
                    PK9 pk = new()
                    {
                        Species = (ushort)RaidSettingsSV.RaidSpecies,
                        Form = (byte)RaidSettingsSV.RaidSpeciesForm
                    };
                    if (pk.Form != 0)
                        form = $"-{pk.Form}";
                    _ = RaidSettingsSV.RaidSpeciesIsShiny ? CommonEdits.SetIsShiny(pk, true) : CommonEdits.SetIsShiny(pk, false);
                    if ((Species)pk.Species <= Species.Enamorus)
                        turl = TradeExtensions<PK9>.PokeImg(pk, false, false);
                    if ((Species)pk.Species > Species.Enamorus || (Species)pk.Species is Species.Wooper && pk.Form != 0 || (Species)pk.Species is Species.Tauros && pk.Form != 0)
                    {
                        if (RaidSettingsSV.RaidSpeciesIsShiny)
                            turl = $"https://raw.githubusercontent.com/zyro670/PokeTextures/main/Placeholder_Sprites/scaled_up_sprites/Shiny/" + $"{pk.Species}{form}" + ".png";
                        else if (!RaidSettingsSV.RaidSpeciesIsShiny)
                            turl = $"https://raw.githubusercontent.com/zyro670/PokeTextures/main/Placeholder_Sprites/scaled_up_sprites/" + $"{pk.Species}{form}" + ".png";
                    }

                    _ = new EmbedBuilder();
                    EmbedBuilder? embed = embedInfo.Item2;
                    embed.ThumbnailUrl = turl;
                    foreach (var channel in channels)
                    {
                        if (embedInfo.Item1 != null)
                        {
                            MemoryStream? ms = new(embedInfo.Item1);
                            try
                            {
                                if (!RaidSettingsSV.TakeScreenshot)
                                    await channel.SendMessageAsync(null, false, embed: embed.Build()).ConfigureAwait(false);
                                else
                                    await channel.SendFileAsync(ms, img, "", false, embed: embed.Build()).ConfigureAwait(false);
                            }
                            catch { }
                        }
                    }
                }
                else await Task.Delay(0_500).ConfigureAwait(false);
            }
        }

        [Command("SVEmbed")]
        [Alias("svem", "sve")]
        [Summary("Initialize posting of SV shiny result embeds to specified Discord channels.")]
        [RequireSudo]
        public async Task InitializeEmbedsSV()
        {
            if (SysCord<T>.Runner.Hub.Config.StopConditions.ResultsEmbedChannels == string.Empty)
            {
                await ReplyAsync("No channels to post embeds in.").ConfigureAwait(false);
                return;
            }

            List<ulong> channels = new();
            foreach (var channel in SysCord<T>.Runner.Hub.Config.StopConditions.ResultsEmbedChannels.Split(',', ' '))
            {
                if (ulong.TryParse(channel, out ulong result) && !channels.Contains(result))
                    channels.Add(result);
            }

            if (channels.Count == 0)
            {
                await ReplyAsync("No valid channels found.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync(!EggBotSV.EmbedsInitialized ? "Scarlet | Violet Embed task started!" : "Scarlet | Violet Embed task stopped!").ConfigureAwait(false);
            if (EggBotSV.EmbedsInitialized)
                EggBotSV.EmbedSource.Cancel();
            else _ = Task.Run(async () => await SVEmbedLoop(channels));
            EggBotSV.EmbedsInitialized ^= true;
        }

        private async Task SVEmbedLoop(List<ulong> channels)
        {
            var ping = SysCord<T>.Runner.Hub.Config.StopConditions.MatchFoundEchoMention;
            while (!EggBotSV.EmbedSource.IsCancellationRequested)
            {
                if (EggBotSV.EmbedMon.Item1 != null)
                {
                    var url = TradeExtensions<PK9>.PokeImg(EggBotSV.EmbedMon.Item1, false, false);
                    var is1of100 = (Species)EggBotSV.EmbedMon.Item1.Species is Species.Dunsparce or Species.Tandemaus;
                    var spec = string.Empty;
                    if (is1of100)
                    {
                        if (EggBotSV.EmbedMon.Item1.EncryptionConstant % 100 == 0)
                        {
                            if ((Species)EggBotSV.EmbedMon.Item1.Species is Species.Dunsparce)
                                spec = "\n3 Segment";
                            else
                                spec = "\nFamily of 3";
                        }
                        if (EggBotSV.EmbedMon.Item1.EncryptionConstant % 100 != 0)
                        {
                            if ((Species)EggBotSV.EmbedMon.Item1.Species is Species.Dunsparce)
                                spec = "\n2 Segment";
                            else
                                spec = "\nFamily of 4";
                        }
                    }

                    var gender = EggBotSV.EmbedMon.Item1.Gender == 0 ? " - (M)" : EggBotSV.EmbedMon.Item1.Gender == 1 ? " - (F)" : "";

                    var description = $"{(EggBotSV.EmbedMon.Item1.ShinyXor == 0 ? "■ - " : EggBotSV.EmbedMon.Item1.ShinyXor <= 16 ? "★ - " : "")}{SpeciesName.GetSpeciesNameGeneration(EggBotSV.EmbedMon.Item1.Species, 2, 8)}{TradeExtensions<T>.FormOutput(EggBotSV.EmbedMon.Item1.Species, EggBotSV.EmbedMon.Item1.Form, out _)}{gender}{spec}\nIVs: {EggBotSV.EmbedMon.Item1.IV_HP}/{EggBotSV.EmbedMon.Item1.IV_ATK}/{EggBotSV.EmbedMon.Item1.IV_DEF}/{EggBotSV.EmbedMon.Item1.IV_SPA}/{EggBotSV.EmbedMon.Item1.IV_SPD}/{EggBotSV.EmbedMon.Item1.IV_SPE}";
                    if (SysCord<T>.Runner.Hub.Config.StopConditions.ShinyTarget == TargetShinyType.NonShiny)
                        description = $"{SpeciesName.GetSpeciesNameGeneration(EggBotSV.EmbedMon.Item1.Species, 2, 8)}{TradeExtensions<T>.FormOutput(EggBotSV.EmbedMon.Item1.Species, EggBotSV.EmbedMon.Item1.Form, out _)}{gender}{spec}\nIVs: {EggBotSV.EmbedMon.Item1.IV_HP}/{EggBotSV.EmbedMon.Item1.IV_ATK}/{EggBotSV.EmbedMon.Item1.IV_DEF}/{EggBotSV.EmbedMon.Item1.IV_SPA}/{EggBotSV.EmbedMon.Item1.IV_SPD}/{EggBotSV.EmbedMon.Item1.IV_SPE}";

                    var markurl = string.Empty;
                    if (EggBotSV.EmbedMon.Item2)
                        markurl = "https://i.imgur.com/T8vEiIk.jpg";
                    else
                        markurl = $"https://i.imgur.com/t2M8qF4.png";

                    var author = new EmbedAuthorBuilder { IconUrl = markurl, Name = EggBotSV.EmbedMon.Item2 ? "Match found!" : "Unwanted match..." };
                    var embed = new EmbedBuilder
                    {
                        Color = EggBotSV.EmbedMon.Item2 ? Color.Teal : Color.Red,
                        ThumbnailUrl = url
                    }.WithAuthor(author).WithDescription(description);

                    var guilds = Context.Client.Guilds;
                    foreach (var guild in guilds)
                    {
                        foreach (var channel in channels)
                        {
                            var ch = guild.Channels.FirstOrDefault(x => x.Id == channel);
                            if (ch != default && ch is ISocketMessageChannel sock)
                            {
                                try
                                {
                                    await sock.SendMessageAsync(EggBotSV.EmbedMon.Item2 ? ping : "", embed: embed.Build()).ConfigureAwait(false);
                                }
                                catch { }
                            }
                        }
                    }
                    EggBotSV.EmbedMon.Item1 = null;
                }
                else await Task.Delay(1_000).ConfigureAwait(false);
            }
            EggBotSV.EmbedSource = new();
        }

        [Command("repeek")]
        [Summary("Take and send a screenshot from the specified Switch.")]
        [RequireOwner]
        public async Task RePeek(string address)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            var bot = SysCord<T>.Runner.GetBot(address);
            if (bot == null)
            {
                await ReplyAsync($"No bot found with the specified address ({address}).").ConfigureAwait(false);
                return;
            }

            var c = bot.Bot.Connection;
            c.Reset();
            var bytes = Task.Run(async () => await c.Screengrab(token).ConfigureAwait(false)).Result ?? Array.Empty<byte>();
            MemoryStream ms = new(bytes);
            var img = "cap.jpg";
            var embed = new EmbedBuilder { ImageUrl = $"attachment://{img}", Color = Color.Purple }.WithFooter(new EmbedFooterBuilder { Text = $"Captured image from bot at address {address}." });
            await Context.Channel.SendFileAsync(ms, img, "", false, embed: embed.Build());
        }

        [Command("setCatchLimit")]
        [Alias("scl")]
        [Summary("Set the Catch Limit for Raids in SV.")]
        [RequireSudo]
        public async Task SetOffsetIncrement([Summary("Set the Catch Limit for Raids in SV.")] int limit)
        {
            int parse = SysCord<T>.Runner.Hub.Config.RaidSV.CatchLimit = limit;

            var msg = $"{Context.User.Mention} Catch Limit for Raids has been set to {parse}.";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("clearRaidSVBans")]
        [Alias("crb")]
        [Summary("Clears the RaidSV ban list.")]
        [RequireSudo]
        public async Task ClearRaidBansSV()
        {
            SysCord<T>.Runner.Hub.Config.RaidSV.RaiderBanList.Clear();
            var msg = "RaidSV ban list has been cleared.";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

    }
}