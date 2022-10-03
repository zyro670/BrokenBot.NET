using Discord;
using Discord.Net;
using Discord.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands
{
    public class PermuteModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        [Command("permute")]
        [Alias("p")]
        [Summary("Gets shiny path results for the specified filter and provided JSON.")]
        [RequireQueueRole(nameof(DiscordManager.RolesEtumrepDump))]
        public async Task PermuteAsync()
        {
            var ch = await Context.User.CreateDMChannelAsync().ConfigureAwait(false);
            var selectMenuBuilder = PermuteUtil.GetPermuteSelectMenu();
            var component = new ComponentBuilder().WithSelectMenu(selectMenuBuilder).Build();

            try
            {
                await ch.SendMessageAsync("**Permute Command Service**", false, null, null, null, null, component).ConfigureAwait(false);
            }
            catch (HttpException ex)
            {
                await Context.Message.ReplyAsync($"Could not send a DM: {ex.Message}").ConfigureAwait(false);
            }

            IEmote reaction = new Emoji("👍");
            await Context.Message.AddReactionAsync(reaction).ConfigureAwait(false);
        }
    }
}
