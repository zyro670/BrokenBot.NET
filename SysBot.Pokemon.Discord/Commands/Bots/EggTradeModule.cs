using Discord.Commands;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SysBot.Pokemon.Discord;
using SysBot.Pokemon;


public class EggTradeModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
    private readonly PersonalTable9SV personalTable9SV; // Declare personalTable9SV variable

    [Command("eggtrade")]
    [Alias("et")]
    [Summary("Trades an egg to the user based on the provided Showdown Set.")]
    public async Task TradeEggAsync([Summary("Showdown Set")][Remainder] string content)
    {
        content = ReusableActions.StripCodeBlock(content);
        var set = new ShowdownSet(content);

        // Ensure the input is a valid Showdown Set
        if (set.InvalidLines.Count != 0)
        {
            var msg = $"Unable to parse Showdown Set:\n{string.Join("\n", set.InvalidLines)}";
            await ReplyAsync(msg).ConfigureAwait(false);
            return;
        }

        try
        {
            // Get the ITrainerInfo instance from the appropriate source (modify as needed)
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>(); // Modify the arguments as needed

            // Call the TradeEggAsync method from the EggTradeHelper class
            var egg = await EggTradeHelper.TradeEggAsync(sav, set).ConfigureAwait(false);

            // Get a Link Trade code
            int code = Info.GetRandomTradeCode();

            // Add the egg to the Link Trade queue with the user's name
            await AddTradeToQueueAsync(code, Context.User.Username, egg).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(EggTradeModule<T>));
            var msg = $"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
    }
    private async Task<PKM> TradeEggAsync(ITrainerInfo sav, int speciesId)
    {
        var set = new ShowdownSet($"Pokemon/{speciesId}"); // Corrected syntax here
        return await EggTradeHelper.TradeEggAsync(sav, set).ConfigureAwait(false);
    }


    private async Task AddTradeToQueueAsync(int code, string trainerName, PKM pkm)
    {
        if (!(pkm is T pk))
        {
            await ReplyAsync("Invalid Pokémon type!").ConfigureAwait(false);
            return;
        }

        if (!pk.CanBeTraded())
        {
            await ReplyAsync("Provided Pokémon content is blocked from trading!").ConfigureAwait(false);
            return;
        }

        var la = new LegalityAnalysis(pk);
        if (!la.Valid)
        {
            await ReplyAsync($"{typeof(T).Name} attachment is not legal, and cannot be traded!").ConfigureAwait(false);
            return;
        }
        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);
    }
    public static class EggTradeHelper
    {
        public static async Task<PKM> TradeEggAsync(ITrainerInfo sav, ShowdownSet set)
        {
            LogUtil.LogText($"TradeEggAsync - Species: {set.Species}");

            if (!Breeding.CanHatchAsEgg(set.Species))
            {
                throw new Exception($"Sorry, {set.Species} cannot be hatched as an egg.");
            }

            try
            {
                var template = AutoLegalityWrapper.GetTemplate(set);
                var pkm = sav.GetLegal(template, out var result);
                TradeExtensions<PK9>.EggTrade(pkm, template); // Assuming PK9, change to corresponding PKX class if needed

                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[template.Species];

                if (!la.Valid)
                {
                    var reason = result == "Timeout" ? $"That {spec} set took too long to generate." : result == "VersionMismatch" ? "Request refused: PKHeX and Auto-Legality Mod version mismatch." : $"I wasn't able to create a {spec} from that set.";
                    var imsg = $"Oops! {reason}";
                    if (result == "Failed")
                        imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";
                    throw new Exception(imsg);
                }

                pkm.ResetPartyStats();
                return pkm;
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(EggTradeHelper));
                var msg = $"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```";
                throw new Exception(msg);
            }
        }
    }
}