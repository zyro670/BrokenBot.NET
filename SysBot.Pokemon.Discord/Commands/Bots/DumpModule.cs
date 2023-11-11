using Discord;
using Discord.Commands;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Queues new Dump trades")]
    public class DumpModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

        [Command("dump")]
        [Alias("d")]
        [Summary("Dumps the Pokémon you show via Link Trade.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task DumpAsync(int code)
        {
            
            var iconURL = Context.User.GetAvatarUrl();
            var dumpMessage = $" You have been added to the Pokémon **Dump** queue. \n Check your DM's for further instructions.";
            var embedDumpMessage = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = Context.User.Username,
                    IconUrl = iconURL
                },
                Color = Color.Blue
            }
            
            .WithDescription(dumpMessage)
            .WithImageUrl("https://i.imgur.com/sJnvhDE.jpg")
            .WithThumbnailUrl("https://i.imgur.com/5akyLET.png")
            .WithCurrentTimestamp()
            .Build();

            await Context.Channel.SendMessageAsync(null, false, embedDumpMessage);

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.Dump, PokeTradeType.Dump).ConfigureAwait(false);
        }

        [Command("dump")]
        [Alias("d")]
        [Summary("Dumps the Pokémon you show via Link Trade.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task DumpAsync([Summary("Trade Code")][Remainder] string code)
        {
            int tradeCode = Util.ToInt32(code);
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, tradeCode == 0 ? Info.GetRandomTradeCode() : tradeCode, Context.User.Username, sig, new T(), PokeRoutineType.Dump, PokeTradeType.Dump).ConfigureAwait(false);
        }

        [Command("dump")]
        [Alias("d")]
        [Summary("Dumps the Pokémon you show via Link Trade.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task DumpAsync()
        {
            var code = Info.GetRandomTradeCode();
            await DumpAsync(code).ConfigureAwait(false);
        }

        [Command("dumpList")]
        [Alias("dl", "dq")]
        [Summary("Prints the users in the Dump queue.")]
        [RequireSudo]
        public async Task GetListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.Dump);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        //Dump Module Additions
        [Command("display")]
        [Alias("dp")]
        [Summary("Display & show stats of the Pokémon you show via Link Trade.")]
        //[RequireQueueRole(nameof(DiscordManager.RolesDisplay))]
        public async Task DisplayAsync(int code)
        {
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.Display, PokeTradeType.Display).ConfigureAwait(false);
        }

        [Command("display")]
        [Alias("dp")]
        [Summary("Display & show stats of the Pokémon you show via Link Trade.")]
        //[RequireQueueRole(nameof(DiscordManager.RolesDisplay))]
        public async Task DisplayAsync([Summary("Trade Code")][Remainder] string code)
        {
            int tradeCode = Util.ToInt32(code);
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, tradeCode == 0 ? Info.GetRandomTradeCode() : tradeCode, Context.User.Username, sig, new T(), PokeRoutineType.Display, PokeTradeType.Display).ConfigureAwait(false);
        }

        [Command("display")]
        [Alias("dp")]
        [Summary("Display & show stats of the Pokémon you show via Link Trade.")]
        //[RequireQueueRole(nameof(DiscordManager.RolesDisplay))]
        public async Task DisplayAsync()
        {
            var code = Info.GetRandomTradeCode();
            await DisplayAsync(code).ConfigureAwait(false);
        }

        [Command("displayList")]
        [Alias("dpl", "dpq")]
        [Summary("Prints the users in the Display queue.")]
        [RequireSudo]
        public async Task GetDisplayListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.Display);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }
    }
}