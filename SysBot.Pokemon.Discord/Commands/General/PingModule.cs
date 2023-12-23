using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Makes the bot respond, indicating that it is running.")]
    public async Task PingAsync()
    {
        await ReplyAsync("Pong!").ConfigureAwait(false);
    }

    [Command("speak")]
    [Alias("talk", "say")]
    [Summary("Tells the bot to speak during times when people are on the island.")]
    [RequireSudo]
    public async Task SpeakAsync([Remainder] string request)
    {
        await ReplyAsync(request).ConfigureAwait(false);
    }
}