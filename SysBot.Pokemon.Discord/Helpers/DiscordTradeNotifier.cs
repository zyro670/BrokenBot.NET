using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace SysBot.Pokemon.Discord;

public class DiscordTradeNotifier<T>(T data, PokeTradeTrainerInfo info, int code, SocketUser trader, SocketCommandContext channel) : IPokeTradeNotifier<T> where T : PKM, new()
{
    private T Data { get; } = data;
    private PokeTradeTrainerInfo Info { get; } = info;
    private int Code { get; } = code;
    private SocketUser Trader { get; } = trader;
    private SocketCommandContext Context { get; } = channel;
    public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
    public readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
        Trader.SendMessageAsync($"Initializing trade{receive}. Please be ready. Your code is **{Code:0000 0000}**.").ConfigureAwait(false);
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var name = Info.TrainerName;
        var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", {name}";
        Trader.SendMessageAsync($"I'm waiting for you{trainer}! Your code is **{Code:0000 0000}**. My IGN is **{routine.InGameName}**.").ConfigureAwait(false);
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        if (info.Type == PokeTradeType.TradeCord)
            TradeCordHelper<T>.HandleTradedCatches(Trader.Id, false);

        OnFinish?.Invoke(routine);
        Trader.SendMessageAsync($"Trade canceled: {msg}").ConfigureAwait(false);
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        if (info.Type == PokeTradeType.TradeCord)
            TradeCordHelper<T>.HandleTradedCatches(Trader.Id, true);
        OnFinish?.Invoke(routine);
        var tradedToUser = Data.Species;
        var message = tradedToUser != 0 ? $"Trade finished. Enjoy your {(Species)tradedToUser}!" : "Trade finished!";
        Trader.SendMessageAsync(message).ConfigureAwait(false);
        if (result.Species != 0 && Hub.Config.Discord.ReturnPKMs)
            Trader.SendPKMAsync(result, "Here's what you traded me!").ConfigureAwait(false);

        if (Hub.Config.Trade.TradeDisplay && info.Type is not PokeTradeType.Dump or PokeTradeType.EtumrepDump)
        {
            PKM emb = info.TradeData;
            if (emb.Species == 0 || info.Type is PokeTradeType.Clone or PokeTradeType.Display)
                emb = result;

            if (emb.Species != 0)
            {
                var shiny = emb.ShinyXor == 0 ? "■" : emb.ShinyXor <= 16 ? "★" : "";
                var set = new ShowdownSet($"{emb.Species}");
                var ballImg = $"https://raw.githubusercontent.com/BakaKaito/HomeImages/main/Ballimg/50x50/" + $"{(Ball)emb.Ball}ball".ToLower() + ".png";
                var gender = emb.Gender == 0 ? " - (M)" : emb.Gender == 1 ? " - (F)" : "";
                var pokeImg = TradeExtensions<T>.PokeImg(emb, false, false);
                string scale = "";

                if (emb is PK9 fin9)
                    scale = $"Scale: {PokeSizeDetailedUtil.GetSizeRating(fin9.Scale)} ({fin9.Scale})";
                if (emb is PA8 fin8a)
                    scale = $"Scale: {PokeSizeDetailedUtil.GetSizeRating(fin8a.Scale)} ({fin8a.Scale})";
                if (emb is PB8 fin8b)
                    scale = $"Scale: {PokeSizeDetailedUtil.GetSizeRating(fin8b.HeightScalar)} ({fin8b.HeightScalar})";
                if (emb is PK8 fin8)
                    scale = $"Scale: {PokeSizeDetailedUtil.GetSizeRating(fin8.HeightScalar)} ({fin8.HeightScalar})";

                var trademessage = $"Pokémon IVs: {emb.IV_HP}/{emb.IV_ATK}/{emb.IV_DEF}/{emb.IV_SPA}/{emb.IV_SPD}/{emb.IV_SPE}\n" +
                    $"Ability: {GameInfo.GetStrings(1).Ability[emb.Ability]}\n" +
                    $"{(Nature)emb.Nature} Nature\n{scale}" +
                    (StopConditionSettings.HasMark((IRibbonIndex)emb, out RibbonIndex mark) ? $"\nPokémon Mark: {mark.ToString().Replace("Mark", "")}{Environment.NewLine}" : "");

                string markEntryText = "";
                var index = (int)mark - (int)RibbonIndex.MarkLunchtime;
                if (index > 0)
                    markEntryText = MarkTitle[index];

                var specitem = emb.HeldItem != 0 ? $"{SpeciesName.GetSpeciesNameGeneration(emb.Species, 2, emb.Generation <= 8 ? 8 : 9)}{TradeExtensions<T>.FormOutput(emb.Species, emb.Form, out _) + " (" + ShowdownParsing.GetShowdownText(emb).Split('@', '\n')[1].Trim() + ")"}" : $"{SpeciesName.GetSpeciesNameGeneration(emb.Species, 2, emb.Generation <= 8 ? 8 : 9) + TradeExtensions<T>.FormOutput(emb.Species, emb.Form, out _)}{markEntryText}";

                var msg = "Displaying your ";
                var mode = info.Type;
                switch (mode)
                {
                    case PokeTradeType.Specific: msg += "request!"; break;
                    case PokeTradeType.Clone: msg += "clone!"; break;
                    case PokeTradeType.Display: msg += "trophy!"; break;
                    case PokeTradeType.SupportTrade or PokeTradeType.Giveaway: msg += $"gift!"; break;
                    case PokeTradeType.FixOT: msg += $"fixed OT!"; break;
                    case PokeTradeType.TradeCord: msg += $"prize!"; break;
                    case PokeTradeType.Seed: msg += $"seed check!"; break;
                }
                string TIDFormatted = emb.Generation >= 7 ? $"{emb.TrainerTID7:000000}" : $"{emb.TID16:00000}";
                var footer = new EmbedFooterBuilder { Text = $"Trainer Info: {emb.OT_Name}/{TIDFormatted}" };
                var author = new EmbedAuthorBuilder { Name = $"{Context.User.Username}'s Pokémon" };
                if (!Hub.Config.TradeCord.UseLargerPokeBalls)
                    ballImg = "";
                author.IconUrl = ballImg;
                var embed = new EmbedBuilder { Color = emb.IsShiny && emb.ShinyXor == 0 ? Color.Gold : emb.IsShiny ? Color.LighterGrey : Color.Teal, Author = author, Footer = footer, ThumbnailUrl = pokeImg };
                embed.AddField(x =>
                {
                    x.Name = $"{shiny} {specitem}{gender}";
                    x.Value = trademessage;
                    x.IsInline = false;
                });
                Context.Channel.SendMessageAsync(Trader.Username + " - " + msg, embed: embed.Build()).ConfigureAwait(false);
            }              
        }
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
    {
        Trader.SendMessageAsync(message).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
    {
        if (message.ExtraInfo is SeedSearchResult r)
        {
            SendNotificationZ3(r);
            return;
        }

        var msg = message.Summary;
        if (message.Details.Count > 0)
            msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
        Trader.SendMessageAsync(msg).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        if (result.Species != 0 && (Hub.Config.Discord.ReturnPKMs || info.Type == PokeTradeType.Dump))
            Trader.SendPKMAsync(result, message).ConfigureAwait(false);
    }

    private void SendNotificationZ3(SeedSearchResult r)
    {
        var lines = r.ToString();

        var embed = new EmbedBuilder { Color = Color.LighterGrey };
        embed.AddField(x =>
        {
            x.Name = $"Seed: {r.Seed:X16}";
            x.Value = lines;
            x.IsInline = false;
        });
        var msg = $"Here are the details for `{r.Seed:X16}`:";
        Trader.SendMessageAsync(msg, embed: embed.Build()).ConfigureAwait(false);
    }

    public void SendIncompleteEtumrepEmbed(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string msg, IReadOnlyList<PA8> pkms)
    {
        var list = new List<FileAttachment>();
        for (int i = 0; i < pkms.Count; i++)
        {
            var pk = pkms[i];
            var ms = new MemoryStream(pk.Data);
            var name = Util.CleanFileName(pk.FileName);
            list.Add(new(ms, name));
        }

        var embed = new EmbedBuilder
        {
            Color = Color.Blue,
            Description = "Here are all the Pokémon you dumped!",
        }.WithAuthor(x => { x.Name = "Pokémon Legends: Arceus Dump"; });

        var ch = Trader.CreateDMChannelAsync().Result;
        ch.SendFilesAsync(list, msg, false, embed: embed.Build()).ConfigureAwait(false);
    }

    public void SendEtumrepEmbed(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, IReadOnlyList<PA8> pkms)
    {
        OnFinish?.Invoke(routine);
        _ = Task.Run(() => EtumrepUtil.SendEtumrepEmbedAsync(Trader, pkms).ConfigureAwait(false));
    }

    public readonly string[] MarkTitle =
    {
        " the Peckish"," the Sleepy"," the Dozy"," the Early Riser"," the Cloud Watcher"," the Sodden"," the Thunderstruck"," the Snow Frolicker"," the Shivering"," the Parched"," the Sandswept"," the Mist Drifter",
        " the Chosen One"," the Catch of the Day"," the Curry Connoisseur"," the Sociable"," the Recluse"," the Rowdy"," the Spacey"," the Anxious"," the Giddy"," the Radiant"," the Serene"," the Feisty"," the Daydreamer",
        " the Joyful"," the Furious"," the Beaming"," the Teary-Eyed"," the Chipper"," the Grumpy"," the Scholar"," the Rampaging"," the Opportunist"," the Stern"," the Kindhearted"," the Easily Flustered"," the Driven",
        " the Apathetic"," the Arrogant"," the Reluctant"," the Humble"," the Pompous"," the Lively"," the Worn-Out", " of the Distant Past", " the Twinkling Star", " the Paldea Champion", " the Great", " the Teeny", " the Treasure Hunter",
        " the Reliable Partner", " the Gourmet", " the One-in-a-Million", " the Former Alpha", " the Unrivaled", " the Former Titan",
    };
}
