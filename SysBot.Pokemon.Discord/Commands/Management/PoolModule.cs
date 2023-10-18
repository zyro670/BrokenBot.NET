using Discord;
using Discord.Commands;
using PKHeX.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Distribution Pool Module")]
    public class PoolModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private const int PageSize = 10;
        private int currentPage = 0;

        [Command("poolReload")]
        [Alias("pr")]
        [Summary("Reloads the bot pool from the setting's folder.")]
        [RequireSudo]
        public async Task ReloadPoolAsync()
        {
            var me = SysCord<T>.Runner;
            var hub = me.Hub;

            var pool = hub.Ledy.Pool.Reload(hub.Config.Folder.DistributeFolder);
            if (!pool)
                await ReplyAsync("Failed to reload from folder.").ConfigureAwait(false);
            else
                await ReplyAsync($"Reloaded from folder. Pool count: {hub.Ledy.Pool.Count}").ConfigureAwait(false);
        }

        [Command("pool")]
        [Summary("Displays the details of Pokémon files in the random pool.")]
        public async Task DisplayPoolCountAsync()
        {
            await DisplayPage(currentPage);
        }

        [Command("nextpage")]
        [Summary("Displays the next page of Pokémon files in the random pool.")]
        public async Task NextPageAsync()
        {
            currentPage++;
            await DisplayPoolCountAsync();
        }

        [Command("backpage")]
        [Summary("Displays the previous page of Pokémon files in the random pool.")]
        public async Task BackPageAsync()
        {
            if (currentPage > 0)
            {
                currentPage--;
                await DisplayPoolCountAsync();
            }
            else
            {
                await ReplyAsync("You are already on the first page.").ConfigureAwait(false);
            }
        }

        [Command("page")]
        [Summary("Jumps to a specific page of Pokémon files in the random pool.")]
        public async Task JumpToPageAsync(int page)
        {
            var me = SysCord<T>.Runner;
            var hub = me.Hub;
            var maxPage = (hub.Ledy.Pool.Count + PageSize - 1) / PageSize;

            if (page >= 1 && page <= maxPage)
            {
                currentPage = page - 1;
                await DisplayPoolCountAsync();
            }
            else
            {
                await ReplyAsync($"Invalid page number. Please enter a number between 1 and {maxPage}.").ConfigureAwait(false);
            }
        }

        private async Task DisplayPage(int page)
        {
            var me = SysCord<T>.Runner;
            var hub = me.Hub;
            var pool = hub.Ledy.Pool;

            var lines = pool.Files
                .Skip(page * PageSize)
                .Take(PageSize)
                .Select((z, i) => $"{i + 1 + (page * PageSize):00}: {GetFileNameWithoutExtension(z.Value.RequestInfo.FileName)} - **{(Species)z.Value.RequestInfo.Species}**");

            var msg = string.Join("\n", lines);
            var totalPageCount = (pool.Count + PageSize - 1) / PageSize;

            var embed = new EmbedBuilder();
            var random = new Random();
            var randomColor = new Color(random.Next(256), random.Next(256), random.Next(256));

            embed.Color = randomColor;
            embed.AddField(x =>
            {
                x.Name = $"Count: {pool.Count}";
                x.Value = msg;
                x.IsInline = false;
            });

            await ReplyAsync($"Pool Details (Page {page + 1}/{totalPageCount})", embed: embed.Build()).ConfigureAwait(false);
        }

        private string GetFileNameWithoutExtension(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }
    }
}
