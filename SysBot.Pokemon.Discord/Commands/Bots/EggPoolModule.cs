using Discord;
using Discord.Commands;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Distribution Pool Module")]
    public class EggPoolModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
        private readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;
        private string? _lastInitialLetter; // Keep this class-level field

        private string GetPokemonEggInitialLetter(T pokemon)
        {
            return pokemon.FileName[0].ToString().ToUpper(); // Assuming the Pokémon's name is accessible via a Name property
        }

        [Command("giveegg")]
        [Alias("ge")]
        [Summary("Makes the bot trade you the specified Pokémon egg from the EggTrade Pool .")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task LedyEggTradeAsync([Remainder] string content)
        {
            var code = Info.GetRandomTradeCode();
            content = ReusableActions.StripCodeBlock(content);
            string normalizedContent = content.ToLowerInvariant().Replace(" ", "").Replace("-", "");

            T pk;
            var eggTradePool = Info.Hub.LedyEgg.EggTradePool;
            if (eggTradePool.Count == 0)
            {
                var eggtradePoolEmpty = $"The EggTrade Pool is empty.";
                var embedEggtradePoolEmpty = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = Color.Blue
                }
                .WithDescription(eggtradePoolEmpty)
                .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                    .Build();
                await ReplyAsync(null, false, embedEggtradePoolEmpty).ConfigureAwait(false);
                return;
            }
            else if (normalizedContent == "random")
            {
                pk = Info.Hub.LedyEgg.EggTradePool.GetRandomSurprise();
            }
            else if (Info.Hub.LedyEgg.EggTrade.TryGetValue(normalizedContent, out LedyRequest<T>? val) && val is not null)
            {
                pk = val.RequestInfo;
            }
            else
            {
                var eggNotAvailable = $"Requested Pokémon not available, use \"{Info.Hub.Config.Discord.CommandPrefix}eggTradePool\" for a full list of available eggs!";
                var embedEggNotAvailable = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = Color.Blue
                }
                .WithDescription(eggNotAvailable)
                .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                    .Build();
                await ReplyAsync(null,false,embedEggNotAvailable).ConfigureAwait(false);
                return;
            }

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);
        }

        [Command("eggTradePool")]
        [Alias("etp")]
        [Summary("Show a list of Pokémon Eggs available in the EggTrade Pool.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task DisplayEggTradePoolCountAsync()
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var eggTradePool = Info.Hub.LedyEgg.EggTradePool;
            if (eggTradePool.Count > 0)
            {
                var lines = eggTradePool.Files.Select((z, i) => $"{i + 1}: **{z.Key.ToTitleCase().Replace(" ", "").Replace("-", "")}**");
                var msg = string.Join("\n", lines);

                List<string> pageContent = ExtraCommandUtil<T>.ListUtilPrep(msg);
                await ExtraCommandUtil<T>.ListUtil(Context, "EggTrade Pool Details", pageContent).ConfigureAwait(false);
            }
            else
            {
                var eggTradePoolEmpty = $"EggTrade Pool is empty.";
                var embedEggtradePoolEmpty = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = Color.Blue
                }
                .WithDescription(eggTradePoolEmpty)
                .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                .Build();
                await ReplyAsync(null, false, embedEggtradePoolEmpty).ConfigureAwait(false);
            }
        }

        [Command("eggTradePoolReload")]
        [Alias("etpr")]
        [Summary("Reloads the bot pool from the setting's folder.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task ReloadEggTradePoolAsync()
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var me = SysCord<T>.Runner;
            var hub = me.Hub;
            var failedtoReload = $"Failed to reload the EggTrade folder.";
            var embedFailedtoReload = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = Context.User.Username,
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Color = Color.Blue
            }
            .WithDescription(failedtoReload)
            .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                .Build();
            var reloaded = $"EggTrade Folder Reloaded. Pool count: **{hub.LedyEgg.EggTradePool.Count}**";
            var embedReloaded = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = Context.User.Username,
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Color = Color.Blue
            }
            .WithDescription(reloaded)
            .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                .Build();
            var pool = hub.LedyEgg.EggTradePool.Reload(hub.Config.Folder.EggTradeFolder);
            if (!pool)
                await ReplyAsync(null, false, embedFailedtoReload).ConfigureAwait(false);
            else
                await ReplyAsync(null,false, embedReloaded).ConfigureAwait(false);
        }

        [Command("eggtradepoolstats")]
        [Alias("etpstats")]
        [Summary("Displays the details of Pokémon files in the random pool.")]
        public async Task DisplayTotalEggTradeFilesCount()
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var me = SysCord<T>.Runner;
            var hub = me.Hub;
            var eggTradePool = hub.LedyEgg.EggTradePool;
            var count = eggTradePool.Count;
            if (count is > 0 and < 20)
            {
                var lines = eggTradePool.Files.Select((z, i) => $"{i + 1:00}: {z.Key} = {(Species)z.Value.RequestInfo.Species}");
                var msg = string.Join("\n", lines);

                var embed = new EmbedBuilder();
                embed.AddField(x =>
                {
                    x.Name = $"Count: **{count}**";
                    x.Value = msg;
                    x.IsInline = false;
                });
                
                await ReplyAsync("Pool Details", embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                var poolCount = $"Pool Details";
                var embedPoolCount = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "**Pool Details**",
                            Value = $"Count: **{count}**",
                            IsInline = false
                        }
                    },
                    Color = Color.Blue
                }
                .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                    .Build();
                await ReplyAsync(null, false, embedPoolCount).ConfigureAwait(false);
            }
        }

        [Command("EggTradequeue")]
        [Alias("etq")]
        [Summary("Prints the users in the EggTrade Pool queues.")]
        [RequireSudo]
        public async Task GetEggTradePoolListAsync()
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
            var eggTradePoolQueue = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = Context.User.Username,
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "**EggTrade Pool Queue**",
                        Value = msg,
                        IsInline = false
                    }
                },
                Color = Color.Blue
            }
            .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
            .WithCurrentTimestamp()
                .Build();
            await ReplyAsync(null, false, eggTradePoolQueue).ConfigureAwait(false);
           

            
        }

        [Command("randomegg")]
        [Alias("re")]
        [Summary("Gives a random Pokémon from the EggTrade Pool.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task RandomPokemonEggAsync()
        {
            var eggTradePool = Info.Hub.LedyEgg.EggTradePool;
            if (eggTradePool.Count == 0)
            {
                var eggTradePoolEmpty = $"The EggTrade Pool is empty.";
                var embedEggtradePoolEmpty = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = Color.Blue
                }
                .WithDescription(eggTradePoolEmpty)
                .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                    .Build();
                await ReplyAsync(null, false, embedEggtradePoolEmpty).ConfigureAwait(false);
                return;
            }

            T pk;
            List<T> filteredPool = eggTradePool.Where(p => GetPokemonEggInitialLetter(p) != _lastInitialLetter).ToList();

            if (filteredPool.Count == 0) // fallback to the complete pool if the filtered list is empty
            {
                filteredPool = eggTradePool;
            }

            var randomIndex = new Random().Next(filteredPool.Count);
            pk = filteredPool[randomIndex];

            _lastInitialLetter = GetPokemonEggInitialLetter(pk); // Update the last initial letter

            var code = Info.GetRandomTradeCode();
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);
        }

        [Command("mysteryegg")]
        [Alias("me")]
        [Summary("Gives a random Pokémon from the EggTrade Pool.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task MysteryPokemonAsync()
        {


            var eggTradePool = Info.Hub.LedyEgg.EggTradePool;
            if (eggTradePool.Count == 0)
            {
                var eggTradePoolEmpty = $"The EggTrade Pool is empty.";
                var embedEggtradePoolEmpty = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = Color.Blue
                }
                .WithDescription(eggTradePoolEmpty)
                .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                .WithCurrentTimestamp()
                    .Build();
                await ReplyAsync(null, false, embedEggtradePoolEmpty).ConfigureAwait(false);
                return;
            }

            T pk;
            List<T> filteredPool = eggTradePool.Where(p => GetPokemonEggInitialLetter(p) != _lastInitialLetter).ToList();

            if (filteredPool.Count == 0) // fallback to the complete pool if the filtered list is empty
            {
                filteredPool = eggTradePool;
            }

            var randomIndex = new Random().Next(filteredPool.Count);
            pk = filteredPool[randomIndex];

            _lastInitialLetter = GetPokemonEggInitialLetter(pk); // Update the last initial letter

            var code = Info.GetRandomTradeCode();
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToMysteryEggQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Mystery, Context.User).ConfigureAwait(false);

        }



        [Command("pageegg")]
        [Summary("Displays a specific page of Pokémon available in the EggTrade Pool.")]
        [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
        public async Task DisplayEggTradePoolPageAsync(int pageNumber)
        {
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            var eggTradePool = Info.Hub.LedyEgg.EggTradePool;
            const int itemsPerPage = 16; // Number of items per page

            int skipItems = (pageNumber - 1) * itemsPerPage;
            int totalNumberOfPages = (int)Math.Ceiling((double)eggTradePool.Count / itemsPerPage);

            if (eggTradePool.Count > skipItems)
            {
                var pageItems = eggTradePool.Files
                    .Skip(skipItems)
                    .Take(itemsPerPage)
                    .Select((z, i) => $"{skipItems + i + 1}: **{z.Key.ToTitleCase().Replace(" ", "").Replace("-", "")}**");

                var msg = string.Join("\n", pageItems);

                List<string> pageContent = ExtraCommandUtil<T>.ListUtilPrep(msg);
                await ExtraCommandUtil<T>.ListUtil(Context, $"EggTrade Pool Details - Page {pageNumber} of {totalNumberOfPages}", pageContent).ConfigureAwait(false);
            }
            else
            {
                Embed embed = new EmbedBuilder()
                { Author = new EmbedAuthorBuilder() { Name = Context.User.Username, IconUrl = Context.User.GetAvatarUrl() } }
                    
                    .WithColor(Color.Blue)
                    .WithDescription($"Page {pageNumber} does not exist.")
                    .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
                    .WithCurrentTimestamp()
                    .Build();
                
                await ReplyAsync(null, false, embed).ConfigureAwait(false);
            }
        }

    }
}