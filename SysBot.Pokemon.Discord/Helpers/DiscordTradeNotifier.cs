using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static SysBot.Pokemon.Discord.Helpers.ColorHelper;
using static PKHeX.Core.AutoMod.Aesthetics;
using static PKHeX.Core.PKM;

namespace SysBot.Pokemon.Discord
{
    public class DiscordTradeNotifier<T> : IPokeTradeNotifier<T> where T : PKM, new()
    {
        private T Data { get; }
        private PokeTradeTrainerInfo Info { get; }
        private int Code { get; }
        private SocketUser Trader { get; }
        public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
        public readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;
       

        public DiscordTradeNotifier(T data, PokeTradeTrainerInfo info, int code, SocketUser trader)
        {
            Data = data;
            Info = info;
            Code = code;
            Trader = trader;
        }

        public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var tradeType = info.Type;
            string tradeMessage;

            if (tradeType != PokeTradeType.Mystery)
            {
                // Check if Data.Nickname is not null before including it in the message
                var nicknameMessage = !string.IsNullOrEmpty(info.TradeData.Nickname) ? $" ({info.TradeData.Nickname})" : "";
                var formattedCode = $"{info.Code:D8}";
                formattedCode = formattedCode.Insert(4, " "); // Insert a space after the fourth character
                tradeMessage = $"Initializing trade for **{nicknameMessage}**. Please be ready. Your code is **{formattedCode}**.";
            }
            else
            {
                tradeMessage = "Initializing trade.";
            }

            var embed = new EmbedBuilder
            {
                Title = "Trade Initialized",
                Description = tradeMessage,
                Color = GetDiscordColor(info.TradeData.IsShiny ? ShinyMap[((Species)info.TradeData.Species, info.TradeData.Form)] : (PersonalColor)info.TradeData.PersonalInfo.Color),
            }
            .WithCurrentTimestamp();

            Trader.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }





        public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
        {
            var name = Info.TrainerName;
            var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", {name}";
            var embed = new EmbedBuilder
            {
                Title = "Trade Searching",
                Description = $"I'm waiting for you**{trainer}**! Your code is **{Code:0000 0000}**. My IGN is **{routine.InGameName}**.",
                Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
            }
            .WithCurrentTimestamp();

            Trader.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
        {
            

            OnFinish?.Invoke(routine);
            var embed = new EmbedBuilder
            {
                Title = "Trade Canceled",
                Description = $"Trade canceled: **{msg}**",
                Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
            }
            .WithCurrentTimestamp();
            
            
            Trader.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
        {
            OnFinish?.Invoke(routine);
            var tradedToUser = Data.Species;
            var message = tradedToUser != 0
            ? $"Trade finished. Enjoy your {(info.Type == PokeTradeType.Mystery ? "**Mystery Pokemon**" : $"**{(Species)tradedToUser}**")}!"
    :       "Trade finished!";
            var tradeFinishedEmbed = new EmbedBuilder
            {
                Title = "Trade Finished",
                Description = message,
                Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
            }
            .WithCurrentTimestamp()
            .Build();

            Trader.SendMessageAsync(embed: tradeFinishedEmbed).ConfigureAwait(false);

            if (result.Species != 0 && Hub.Config.Discord.ReturnPKMs)
            {
                var pkmMessage = "Here's What You Traded Me!";

                var pkmEmbed = new EmbedBuilder
                {
                    Description = pkmMessage,
                    Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
                }
                .WithCurrentTimestamp()
                .Build();

                Trader.SendMessageAsync(embed: pkmEmbed).ConfigureAwait(false);

                Trader.SendPKMAsync(result).ConfigureAwait(false);
            }
        }



        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        {
            var embed = new EmbedBuilder
            {
                Title = "Notification",
                Description = message,
                Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
            };
            embed.WithCurrentTimestamp();
            Trader.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
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
            var embed = new EmbedBuilder
            {
                Title = "Notification",
                Description = msg,
                Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
            };
            embed.WithCurrentTimestamp();
            Trader.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
        {
            if (result.Species != 0 && (Hub.Config.Discord.ReturnPKMs || info.Type == PokeTradeType.Dump))
                Trader.SendPKMAsync(result, message).ConfigureAwait(false);
        }

        private void SendNotificationZ3(SeedSearchResult r)
        {
            var lines = r.ToString();

            var embed = new EmbedBuilder
            {
                Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
            };
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
                Color = GetDiscordColor(Data.IsShiny ? ShinyMap[((Species)Data.Species, Data.Form)] : (PersonalColor)Data.PersonalInfo.Color),
                Description = "Here are all the Pokémon you dumped!"
            }.WithAuthor(x => { x.Name = "Pokémon Legends: Arceus Dump"; });

            var ch = Trader.CreateDMChannelAsync().Result;
            ch.SendFilesAsync(list, msg, false, embed: embed.Build()).ConfigureAwait(false);
        }

        public void SendEtumrepEmbed(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, IReadOnlyList<PA8> pkms)
        {
            OnFinish?.Invoke(routine);
            _ = Task.Run(async () => await EtumrepUtil.SendEtumrepEmbedAsync(Trader, pkms).ConfigureAwait(false));
        }
    }
}
