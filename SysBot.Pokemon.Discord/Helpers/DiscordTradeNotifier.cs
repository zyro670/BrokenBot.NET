using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace SysBot.Pokemon.Discord
{
    public class DiscordTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private SocketUser Trader { get; }
        private SocketCommandContext Context { get; }
        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
        public readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;

        public DiscordTradeNotifier(T data, PokeTradeTrainerInfo info, int code, SocketUser trader, SocketCommandContext channel)
        {
            Data = data;
            Info = info;
            Code = code;
            Trader = trader;
            Context = channel;
        }

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

            var SVmon = TradeExtensions<PK9>.SVTrade;
            var LAmon = TradeExtensions<PA8>.LATrade;
            var BDSPmon = TradeExtensions<PB8>.BDSPTrade;
            var SWSHmon = TradeExtensions<PK8>.SWSHTrade;
            PKM fin = result;
            switch (result)
            {
                case PK9: fin = SVmon; break;
                case PA8: fin = LAmon; break;
                case PB8: fin = BDSPmon; break;
                case PK8: fin = SWSHmon; break;
            }

            if (fin.Species != 0 && Hub.Config.Trade.TradeDisplay)
            {
                var shiny = fin.ShinyXor == 0 ? "■" : fin.ShinyXor <= 16 ? "★" : "";
                var set = new ShowdownSet($"{fin.Species}");
                var ballImg = $"https://raw.githubusercontent.com/BakaKaito/HomeImages/main/Ballimg/50x50/" + $"{(Ball)fin.Ball}ball".ToLower() + ".png";
                var gender = fin.Gender == 0 ? " - (M)" : fin.Gender == 1 ? " - (F)" : "";
                var pokeImg = TradeExtensions<T>.PokeImg(fin, false, false);
                string scale = "";
                if (fin is PK9 fin9)
                    scale = $"Scale: {PokeSizeDetailedUtil.GetSizeRating(fin9.Scale)} ({fin9.Scale})\n";

                var trademessage = $"Pokémon IVs: {fin.IV_HP}/{fin.IV_ATK}/{fin.IV_DEF}/{fin.IV_SPA}/{fin.IV_SPD}/{fin.IV_SPE}\n" +
                    $"Ability: {GameInfo.GetStrings(1).Ability[fin.Ability]}\n" +
                    $"{(Nature)fin.Nature} Nature\n{scale}" +
                    (StopConditionSettings.HasMark((IRibbonIndex)fin, out RibbonIndex mark) ? $"\nPokémon Mark: {mark.ToString().Replace("Mark", "")}{Environment.NewLine}" : "");

                string markEntryText = "";
                var index = (int)mark - (int)RibbonIndex.MarkLunchtime;
                if (index > 0)
                    markEntryText = MarkTitle[index];

                var specitem = fin.HeldItem != 0 ? $"{SpeciesName.GetSpeciesNameGeneration(fin.Species, 2, fin.Generation <= 8 ? 8 : 9)}{TradeExtensions<T>.FormOutput(fin.Species, fin.Form, out _) + " (" + ShowdownParsing.GetShowdownText(fin).Split('@', '\n')[1].Trim() + ")"}" : $"{SpeciesName.GetSpeciesNameGeneration(fin.Species, 2, fin.Generation <= 8 ? 8 : 9) + TradeExtensions<T>.FormOutput(fin.Species, fin.Form, out _)}{markEntryText}";

                var msg = "Displaying your ";
                var mode = info.Type;
                switch (mode)
                {
                    case PokeTradeType.Specific: msg += "request!"; break;
                    case PokeTradeType.Clone: msg += "clone!"; break;
                    case PokeTradeType.Display: msg += "trophy!"; break;
                    case PokeTradeType.EtumrepDump or PokeTradeType.Dump or PokeTradeType.Seed: msg += "dump!"; break;
                    case PokeTradeType.SupportTrade or PokeTradeType.Giveaway: msg += $"gift!"; break;
                    case PokeTradeType.FixOT: msg += $"fixed OT!"; break;
                    case PokeTradeType.TradeCord: msg += $"prize!"; break;
                }
                string TIDFormatted = fin.Generation >= 7 ? $"{fin.TrainerTID7:000000}" : $"{fin.TID16:00000}";
                var footer = new EmbedFooterBuilder { Text = $"Trainer Info: {fin.OT_Name}/{TIDFormatted}" };
                var author = new EmbedAuthorBuilder { Name = $"{Context.User.Username}'s Pokémon" };
                if (!Hub.Config.TradeCord.UseLargerPokeBalls)                
                    ballImg = "";                
                author.IconUrl = ballImg;
                var embed = new EmbedBuilder { Color = fin.IsShiny && fin.ShinyXor == 0 ? Color.Gold : fin.IsShiny ? Color.LighterGrey : Color.Teal, Author = author, Footer = footer, ThumbnailUrl = pokeImg };
                embed.AddField(x =>
                {
                    x.Name = $"{shiny} {specitem}{gender}";
                    x.Value = trademessage;
                    x.IsInline = false;
                });

                Context.Channel.SendMessageAsync(Trader.Username + " - " + msg, embed: embed.Build()).ConfigureAwait(false);
                switch (fin)
                {
                    case PK9: TradeExtensions<PK9>.SVTrade = new(); break;
                    case PA8: TradeExtensions<PA8>.LATrade = new(); break;
                    case PB8: TradeExtensions<PB8>.BDSPTrade = new(); break;
                    case PK8: TradeExtensions<PK8>.SWSHTrade = new(); break;
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
}
