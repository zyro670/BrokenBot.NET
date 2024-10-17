using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Pokemon.RaidBotSV;

namespace SysBot.Pokemon.Discord;

[Summary("Generates and queues various silly trade additions")]
public class TradeAdditionsModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
    private readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;
    private readonly ExtraCommandUtil<T> Util = new();
    private readonly LairBotSettings LairSettings = SysCord<T>.Runner.Hub.Config.LairSWSH;
    private readonly RollingRaidSettings RollingRaidSettings = SysCord<T>.Runner.Hub.Config.RollingRaidSWSH;

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
        var set = new ShowdownSet($"{SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 9)} @ {item.Trim()}");
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
        pkm.LegalizePokemon();

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
        var bytes = await c.PixelPeek(token).ConfigureAwait(false) ?? [];
        if (bytes.Length == 1)
        {
            await ReplyAsync($"Failed to take a screenshot for bot at {address}. Is the bot connected?").ConfigureAwait(false);
            return;
        }
        MemoryStream ms = new(bytes);

        var img = "cap.jpg";
        var embed = new EmbedBuilder { ImageUrl = $"attachment://{img}", Color = Color.Purple }.WithFooter(new EmbedFooterBuilder { Text = $"Captured image from bot at address {address}." });
        await Context.Channel.SendFileAsync(ms, img, "", false, embed: embed.Build());
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
        if (!RollingRaidBotSWSH.RollingRaidEmbedsInitialized)
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

        RollingRaidBotSWSH.RollingRaidEmbedsInitialized ^= true;
        await ReplyAsync(!RollingRaidBotSWSH.RollingRaidEmbedsInitialized ? "RollingRaid Embed task stopped!" : "RollingRaid Embed task started!").ConfigureAwait(false);

        if (!RollingRaidBotSWSH.RollingRaidEmbedsInitialized)
        {
            RollingRaidBotSWSH.RaidEmbedSource.Cancel();
            return;
        }

        RollingRaidBotSWSH.RaidEmbedSource = new();
        _ = Task.Run(async () => await RollingRaidEmbedLoop(embedChannels).ConfigureAwait(false));
    }

    private static async Task RollingRaidEmbedLoop(List<ITextChannel> channels)
    {
        while (!RollingRaidBotSWSH.RaidEmbedSource.IsCancellationRequested)
        {
            if (RollingRaidBotSWSH.EmbedQueue.TryDequeue(out var embedInfo))
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
        var bytes = Task.Run(async () => await c.PixelPeek(token).ConfigureAwait(false)).Result ?? Array.Empty<byte>();
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

    [Command("addRaidParams")]
    [Alias("arp")]
    [Summary("Adds new raid parameter.")]
    [RequireSudo]
    public async Task AddNewRaidParam([Summary("Seed")] string seed, [Summary("Species Type")] string species, [Summary("Content Type")] string content)
    {
        int type = int.Parse(content);

        var description = string.Empty;
        var prevpath = "bodyparam.txt";
        var filepath = "RaidFilesSV\\bodyparam.txt";
        if (File.Exists(prevpath))
            Directory.Move(filepath, prevpath + Path.GetFileName(filepath));

        if (File.Exists(filepath))
            description = File.ReadAllText(filepath);

        var data = string.Empty;
        var prevpk = "pkparam.txt";
        var pkpath = "RaidFilesSV\\pkparam.txt";
        if (File.Exists(prevpk))
            Directory.Move(pkpath, prevpk + Path.GetFileName(pkpath));

        if (File.Exists(pkpath))
            data = File.ReadAllText(pkpath);

        var parse = TradeExtensions<T>.EnumParse<Species>(species);
        if (parse == default)
        {
            await ReplyAsync($"{species} is not a valid Species.").ConfigureAwait(false);
            return;
        }

        RotatingRaidSettingsSV.RotatingRaidParameters newparam = new()
        {
            CrystalType = (TeraCrystalType)type,
            Description = new[] { description },
            PartyPK = new[] { data },
            Species = parse,
            SpeciesForm = 0,
            Seed = seed,
            IsCoded = true,
            Title = $"{parse} ☆ - {(TeraCrystalType)type}",
        };

        SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaidEmbedParameters.Add(newparam);
        var msg = $"A new raid for {newparam.Species} has been added!";
        await ReplyAsync(msg).ConfigureAwait(false);
    }

    [Command("removeRaidParams")]
    [Alias("rrp")]
    [Summary("Removes a raid parameter.")]
    [RequireSudo]
    public async Task RemoveRaidParam([Summary("Seed Index")] int index)
    {
        var list = SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaidEmbedParameters;
        if (index >= 0 && index < list.Count)
        {
            var raid = list[index];
            list.RemoveAt(index);
            var msg = $"Raid for {raid.Title} | {raid.Seed:X8} has been removed!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
        else
            await ReplyAsync("Invalid raid parameter index.").ConfigureAwait(false);
    }

    [Command("toggleRaidParams")]
    [Alias("trp")]
    [Summary("Toggles raid parameter.")]
    [RequireSudo]
    public async Task ToggleRaidParam([Summary("Seed Index")] int index)
    {
        var list = SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaidEmbedParameters;
        if (index >= 0 && index < list.Count)
        {
            var raid = list[index];
            raid.ActiveInRotation = !raid.ActiveInRotation;
            var m = raid.ActiveInRotation ? "enabled" : "disabled";
            var msg = $"Raid for {raid.Title} | {raid.Seed:X8} has been {m}!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
        else
            await ReplyAsync("Invalid raid parameter Index.").ConfigureAwait(false);
    }

    [Command("togglecodeRaidParams")]
    [Alias("tcrp")]
    [Summary("Toggles code raid parameter.")]
    [RequireSudo]
    public async Task ToggleCodeRaidParam([Summary("Seed Index")] int index)
    {
        var list = SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaidEmbedParameters;
        if (index >= 0 && index < list.Count)
        {
            var raid = list[index];
            raid.IsCoded = !raid.IsCoded;
            var m = raid.IsCoded ? "coded" : "uncoded";
            var msg = $"Raid for {raid.Title} | {raid.Seed:X8} is now {m}!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
        else
            await ReplyAsync("Invalid raid parameter Index.").ConfigureAwait(false);
    }

    [Command("changeRaidParamTitle")]
    [Alias("crpt")]
    [Summary("Changes the title of a  raid parameter.")]
    [RequireSudo]
    public async Task ChangeRaidParamTitle([Summary("Seed Index")] int index, [Summary("Title")] string title)
    {
        var list = SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaidEmbedParameters;
        if (index >= 0 && index < list.Count)
        {
            var raid = list[index];
            raid.Title = title;
            var msg = $"Raid Title for {raid.Title} | {raid.Seed:X8} has been changed to: {title}!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
        else
            await ReplyAsync("Invalid raid parameter Index.").ConfigureAwait(false);
    }

    [Command("viewraidList")]
    [Alias("vrl", "rotatinglist")]
    [Summary("Prints the raids in the current collection.")]
    public async Task GetRaidListAsync()
    {
        var list = SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaidEmbedParameters;
        int count = list.Count;
        int fields = (int)Math.Ceiling((double)count / 15);
        var embed = new EmbedBuilder
        {
            Title = "Raid List"
        };
        for (int i = 0; i < fields; i++)
        {
            int start = i * 15;
            int end = Math.Min(start + 14, count - 1);
            var fieldBuilder = new StringBuilder();
            for (int j = start; j <= end; j++)
            {
                var raid = list[j];
                int paramNumber = j;
                fieldBuilder.AppendLine($"{paramNumber}.) {raid.Title} - {raid.Seed} - Status: {(raid.ActiveInRotation ? "Active" : "Inactive")}");
            }
            embed.AddField($"Raid List - Part {i + 1}", fieldBuilder.ToString(), false);
        }
        await ReplyAsync($"These are the raids currently in the list (total: {count}):", embed: embed.Build()).ConfigureAwait(false);
    }

    [Command("toggleRaidPK")]
    [Alias("trpk")]
    [Summary("Toggles raid parameter.")]
    [RequireSudo]
    public async Task ToggleRaidParamPK([Summary("Seed Index")] int index, [Summary("Showdown Set")][Remainder] string content)
    {
        var list = SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaidEmbedParameters;
        if (index >= 0 && index < list.Count)
        {
            var raid = list[index];
            raid.PartyPK = new[] { content };
            var m = string.Join("\n", raid.PartyPK);
            var msg = $"RaidPK for {raid.Title} | {raid.Seed:X8} has been updated to:\n{m}!";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
        else
            await ReplyAsync("Invalid raid parameter Index.").ConfigureAwait(false);
    }

    [Command("raidhelp")]
    [Alias("rh")]
    [Summary("Prints the raid help command list.")]
    public async Task GetRaidHelpListAsync()
    {
        var embed = new EmbedBuilder();
        List<string> cmds =
        [
            "$scl - Sets the catch limit for your raids.\n",
            "$crb - Clear all in raider ban list.\n",
            "$vrl - View all raids in the list.\n",
            "$arp - Add parameter to the collection.\nEx: [Command] [Index] [Species] [Difficulty]\n",
            "$rrp - Remove parameter from the collection.\nEx: [Command] [Index]\n",
            "$trp - Toggle the parameter as Active/Inactive in the collection.\nEx: [Command] [Index]\n",
            "$tcrp - Toggle the parameter as Coded/Uncoded in the collection.\nEx: [Command] [Index]\n",
            "$trpk - Set a PartyPK for the parameter via a showdown set.\nEx: [Command] [Index] [ShowdownSet]\n",
            "$crpt - Set the title for the parameter.\nEx: [Command] [Index]"
        ];
        string msg = string.Join("", cmds.ToList());
        embed.AddField(x =>
        {
            x.Name = "Raid Help Commands";
            x.Value = msg;
            x.IsInline = false;
        });
        await ReplyAsync("Here's your raid help!", embed: embed.Build()).ConfigureAwait(false);
    }

    [Command("unbanraider")]
    [Alias("ubr")]
    [Summary("Removes the specificed NID from the banlist for Raids in SV.")]
    [RequireSudo]
    public async Task UnbanRaider([Summary("Removes the specificed NID from the banlist for Raids in SV.")] string nid)
    {
        var list = SysCord<T>.Runner.Hub.Config.RaidSV.RaiderBanList.List.ToArray();
        string msg = $"{Context.User.Mention} no user found with that NID.";
        for (int i = 0; i < list.Length; i++)
            if ($"{list[i].ID}".Equals(nid))
            {
                msg = $"{Context.User.Mention} user {list[i].Name} - {list[i].ID} has been unbanned.";
                SysCord<T>.Runner.Hub.Config.RaidSV.RaiderBanList.List.ToList().Remove(list[i]);
            }
        await ReplyAsync(msg).ConfigureAwait(false);
    }

    [Command("unbanrotatingraider")]
    [Alias("ubrr")]
    [Summary("Removes the specificed NID from the banlist for Raids in SV.")]
    [RequireSudo]
    public async Task UnbanRotatingRaider([Summary("Removes the specificed NID from the banlist for Raids in SV.")] string nid)
    {
        var list = SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaiderBanList.List.ToArray();
        string msg = $"{Context.User.Mention} no user found with that NID.";
        for (int i = 0; i < list.Length; i++)
            if ($"{list[i].ID}".Equals(nid))
            {
                msg = $"{Context.User.Mention} user {list[i].Name} - {list[i].ID} has been unbanned.";
                SysCord<T>.Runner.Hub.Config.RotatingRaidSV.RaiderBanList.List.ToList().Remove(list[i]);
            }
        await ReplyAsync(msg).ConfigureAwait(false);
    }

    [Command("modifyraidercount")]
    [Alias("mrc")]
    [Summary("Modifies the raider's penalty count for SV")]
    [RequireSudo]
    public async Task ModifyRaiderCount([Summary("Modifies the raider's penalty count for SV")] string nid, int count)
    {
        var path = "raidfilessv\\temp-session.json";
        var json = File.ReadAllText(path);
        var jsonData = JsonConvert.DeserializeObject<List<RaidSessionDetails>>(json)!;

        foreach (var j in jsonData)
        {
            if (j.ID.ToString() == nid)
            {
                j.PenaltyCount = count;
                await ReplyAsync($"Penalty Count for {j.Name} ({j.ID}) has been modified to {j.PenaltyCount}.").ConfigureAwait(false);
                json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(path, json);
                break;
            }
        }
    }
}