using PKHeX.Core;
using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SysBot.Pokemon.Discord
{
    [Summary("Generates and queues various silly trade additions")]
    public class TradeCordModule : ModuleBase<SocketCommandContext>
    {
        private static TradeQueueInfo<PK8> Info => SysCordInstance.Self.Hub.Queues.Info;
        private readonly PokeTradeHub<PK8> Hub = SysCordInstance.Self.Hub;
        private readonly ExtraCommandUtil Util = new();

        [Command("TradeCordList")]
        [Alias("tcl", "tcq")]
        [Summary("Prints users in the TradeCord queue.")]
        [RequireSudo]
        public async Task GetTradeCordListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.TradeCord);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending TradeCord Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordVote")]
        [Alias("v", "vote")]
        [Summary("Vote for an event from a randomly selected list.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task EventVote()
        {
            DateTime.TryParse(Info.Hub.Config.TradeCord.EventEnd, out DateTime endTime);
            bool ended = (Hub.Config.TradeCord.EnableEvent && endTime != default && DateTime.Now > endTime) || !Hub.Config.TradeCord.EnableEvent;
            if (!ended)
            {
                var dur = endTime - DateTime.Now;
                var msg = $"{(dur.Days > 0 ? $"{dur.Days}d " : "")}{(dur.Hours > 0 ? $"{dur.Hours}h " : "")}{(dur.Minutes < 2 ? "1m" : dur.Minutes > 0 ? $"{dur.Minutes}m" : "")}";
                await ReplyAsync($"{Hub.Config.TradeCord.PokeEventType} event is already ongoing and will last {(endTime == default ? "until the bot owner stops it" : $"for about {msg}")}.");
                return;
            }

            bool canReact = Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).AddReactions;
            if (!canReact)
            {
                await ReplyAsync("Cannot start the vote due to missing permissions.");
                return;
            }

            var timeRemaining = TradeExtensions.EventVoteTimer - DateTime.Now;
            if (timeRemaining.TotalSeconds > 0)
            {
                await ReplyAsync($"Please try again in about {(timeRemaining.Hours > 1 ? $"{timeRemaining.Hours} hours and " : timeRemaining.Hours > 0 ? $"{timeRemaining.Hours} hour and " : "")}{(timeRemaining.Minutes < 2 ? "1 minute" : $"{timeRemaining.Minutes} minutes")}");
                return;
            }

            TradeExtensions.EventVoteTimer = DateTime.Now.AddMinutes(Hub.Config.TradeCord.TradeCordEventCooldown + Hub.Config.TradeCord.TradeCordEventDuration);
            List<PokeEventType> events = new();
            PokeEventType[] vals = (PokeEventType[])Enum.GetValues(typeof(PokeEventType));
            while (events.Count < 5)
            {
                var rand = vals[TradeExtensions.Random.Next(vals.Length)];
                if (!events.Contains(rand))
                    events.Add(rand);
            }

            var t = Task.Run(async () => await Util.EventVoteCalc(Context, events).ConfigureAwait(false));
            var index = t.Result;
            Hub.Config.TradeCord.PokeEventType = events[index];
            Hub.Config.TradeCord.EnableEvent = true;
            Hub.Config.TradeCord.EventEnd = DateTime.Now.AddMinutes(Hub.Config.TradeCord.TradeCordEventDuration).ToString();
            await ReplyAsync($"{events[index]} event has begun and will last {(Hub.Config.TradeCord.TradeCordEventDuration < 2 ? "1 minute" : $"{Hub.Config.TradeCord.TradeCordEventDuration} minutes")}!");
        }

        [Command("TradeCordCatch")]
        [Alias("k", "catch")]
        [Summary("Catch a random Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCord()
        {
            string name = $"{Context.User.Username}'s Catch";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var id = Context.User.Id;
            if (!TradeCordCanCatch(id, out TimeSpan timeRemaining))
            {
                msg = $"{Context.User.Username}, you're too quick!\nPlease try again in {(timeRemaining.TotalSeconds < 2 ? 1 : timeRemaining.TotalSeconds):N0} {(_ = timeRemaining.TotalSeconds < 2 ? "second" : "seconds")}!";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            if (Info.Hub.Config.TradeCord.TradeCordCooldown > 0)
            {
                if (TradeExtensions.UserCommandTimestamps.ContainsKey(id))
                    TradeExtensions.UserCommandTimestamps[id].Add(DateTime.UtcNow);
                else TradeExtensions.UserCommandTimestamps.Add(id, new List<DateTime> { DateTime.UtcNow });

                var count = TradeExtensions.UserCommandTimestamps[id].Count;
                if (count >= 15 && TradeExtensions.SelfBotScanner(id, Hub.Config.TradeCord.TradeCordCooldown))
                {
                    var t = Task.Run(async () => await Util.ReactionVerification(Context).ConfigureAwait(false));
                    if (t.Result)
                        return;
                }
            }

            TradeCordCooldown(id);
            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Catch };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { }, true, Hub.Config.TradeCord);
            if (!result.Success)
            {
                if (result.Poke.Species != 0)
                {
                    var la = new LegalityAnalysis(result.Poke);
                    if (!la.Valid)
                    {
                        await Context.Channel.SendPKMAsync(result.Poke, $"Something went wrong!\n{ReusableActions.GetFormattedShowdownText(result.Poke)}").ConfigureAwait(false);
                        return;
                    }
                }

                if (result.EggPoke.Species != 0)
                {
                    var la = new LegalityAnalysis(result.EggPoke);
                    if (!la.Valid)
                    {
                        await Context.Channel.SendPKMAsync(result.EggPoke, $"Something went wrong!\n{ReusableActions.GetFormattedShowdownText(result.EggPoke)}").ConfigureAwait(false);
                        return;
                    }
                }
            }
            else if (result.FailedCatch)
            {
                var spookyRng = TradeExtensions.Random.Next(101);
                var imgRng = TradeExtensions.Random.Next(1, 3);
                string imgGarf = "https://i.imgur.com/BOb6IbW.png";
                string imgConk = "https://i.imgur.com/oSUQhYv.png";
                var ball = (Ball)TradeExtensions.Random.Next(2, 26);
                var speciesRand = TradeExtensions.RandomInit().SpeciesRNG;
                var descF = $"You threw {(ball == Ball.Ultra ? "an" : "a")} {(ball == Ball.Cherish ? Ball.Poke : ball)} Ball at a wild {(spookyRng >= 90 ? "...whatever that thing is" : SpeciesName.GetSpeciesNameGeneration(speciesRand, 2, 8))}...";
                msg = $"{(spookyRng >= 90 ? "One wiggle... Two... It breaks free and stares at you, smiling. You run for dear life." : "...but it managed to escape!")}{result.Message}";

                var authorF = new EmbedAuthorBuilder { Name = name };
                var footerF = new EmbedFooterBuilder { Text = $"{(spookyRng >= 90 ? $"But deep inside you know there is no escape... {(result.EggPokeID != 0 ? $"Egg ID {result.EggPokeID}" : "")}" : result.EggPokeID != 0 ? $"Egg ID {result.EggPokeID}" : "")}" };
                var embedF = new EmbedBuilder
                {
                    Color = Color.Teal,
                    ImageUrl = spookyRng >= 90 && imgRng == 1 ? imgGarf : spookyRng >= 90 && imgRng == 2 ? imgConk : "",
                    Description = descF,
                    Author = authorF,
                    Footer = footerF,
                };

                await Util.EmbedUtil(Context, result.EmbedName, msg, embedF).ConfigureAwait(false);
                return;
            }

            var nidoranGender = string.Empty;
            var speciesName = SpeciesName.GetSpeciesNameGeneration(result.Poke.Species, 2, 8);
            if (result.Poke.Species == 32 || result.Poke.Species == 29)
            {
                nidoranGender = speciesName.Last().ToString();
                speciesName = speciesName.Remove(speciesName.Length - 1);
            }

            var form = nidoranGender != string.Empty ? nidoranGender : TradeExtensions.FormOutput(result.Poke.Species, result.Poke.Form, out _);
            var finalName = speciesName + form;
            var pokeImg = TradeExtensions.PokeImg(result.Poke, result.Poke.CanGigantamax, Hub.Config.TradeCord.UseFullSizeImages);
            var ballImg = $"https://raw.githubusercontent.com/BakaKaito/HomeImages/main/Ballimg/50x50/{((Ball)result.Poke.Ball).ToString().ToLower()}ball.png";
            var desc = $"You threw {(result.Poke.Ball == 2 ? "an" : "a")} {(Ball)result.Poke.Ball} Ball at a {(result.Poke.IsShiny ? $"**shiny** wild **{finalName}**" : $"wild {finalName}")}...";

            var author = new EmbedAuthorBuilder { Name = name };
            if (!Hub.Config.TradeCord.UseLargerPokeBalls)
            {
                author.IconUrl = ballImg;
                ballImg = "";
            }

            var footer = new EmbedFooterBuilder { Text = $"Catch {result.User.CatchCount} | Pokémon ID {result.PokeID}{(result.EggPokeID == 0 ? "" : $" | Egg ID {result.EggPokeID}")}" };
            var embed = new EmbedBuilder
            {
                Color = (result.Poke.IsShiny && result.Poke.FatefulEncounter) || result.Poke.ShinyXor == 0 ? Color.Gold : result.Poke.ShinyXor <= 16 ? Color.LightOrange : Color.Teal,
                ImageUrl = pokeImg,
                ThumbnailUrl = ballImg,
                Description = desc,
                Author = author,
                Footer = footer,
            };

            await Util.EmbedUtil(Context, result.EmbedName, result.Message, embed).ConfigureAwait(false);
        }

        [Command("TradeCord")]
        [Alias("tc")]
        [Summary("Trade a caught Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeForTradeCord([Summary("Trade Code")] int code, [Summary("Numerical catch ID")] string id)
        {
            string name = $"{Context.User.Username}'s Trade";
            var sig = Context.User.GetFavor();

            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Trade };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { id }, true, Hub.Config.TradeCord);
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }
            await Context.AddToQueueAsync(code, Context.User.Username, sig, result.Poke, PokeRoutineType.TradeCord, PokeTradeType.TradeCord).ConfigureAwait(false);
        }

        [Command("TradeCord")]
        [Alias("tc")]
        [Summary("Trade a caught Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeForTradeCord([Summary("Numerical catch ID")] string id)
        {
            var code = Info.GetRandomTradeCode();
            await TradeForTradeCord(code, id).ConfigureAwait(false);
        }

        [Command("TradeCordCatchList")]
        [Alias("l", "list")]
        [Summary("List user's Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task PokeList([Summary("Species name of a Pokémon")][Remainder] string content)
        {
            string name = $"{Context.User.Username}'s List";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.List };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { content }, false, Hub.Config.TradeCord);
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, result.EmbedName, result.Message).ConfigureAwait(false);
                return;
            }
            await Util.ListUtil(Context, result.EmbedName, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordInfo")]
        [Alias("i", "info")]
        [Summary("Displays details for a user's Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordInfo([Summary("Numerical catch ID")] string id)
        {
            string name = $"{Context.User.Username}'s Pokémon Info";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Info };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { id }, false, Hub.Config.TradeCord);
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }

            bool canGmax = new ShowdownSet(ShowdownParsing.GetShowdownText(result.Poke)).CanGigantamax;
            var pokeImg = TradeExtensions.PokeImg(result.Poke, canGmax, Hub.Config.TradeCord.UseFullSizeImages);
            string flavorText = $"\n\n{TradeExtensions.DexFlavor(result.Poke.Species, result.Poke.Form, canGmax)}";

            var embed = new EmbedBuilder { Color = result.Poke.IsShiny ? Color.Blue : Color.DarkBlue, ThumbnailUrl = pokeImg }.WithFooter(x => { x.Text = flavorText; x.IconUrl = "https://i.imgur.com/nXNBrlr.png"; });
            msg = $"\n\n{ReusableActions.GetFormattedShowdownText(result.Poke)}";

            await Util.EmbedUtil(Context, result.EmbedName, msg, embed).ConfigureAwait(false);
        }

        [Command("TradeCordMassRelease")]
        [Alias("mr", "massrelease")]
        [Summary("Mass releases every non-shiny and non-Ditto Pokémon or specific species if specified.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task MassRelease([Remainder] string species = "")
        {
            string name = $"{Context.User.Username}'s Mass Release";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.MassRelease };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { species }, true, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordRelease")]
        [Alias("r", "release")]
        [Summary("Releases a user's specific Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task Release([Summary("Numerical catch ID")] string id)
        {
            string name = $"{Context.User.Username}'s Release";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Release };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { id }, true, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordDaycare")]
        [Alias("dc")]
        [Summary("Check what's inside the daycare.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task DaycareInfo()
        {
            string name = $"{Context.User.Username}'s Daycare Info";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.DaycareInfo };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { }, false, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordDaycare")]
        [Alias("dc")]
        [Summary("Adds (or removes) Pokémon to (from) daycare.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task Daycare([Summary("Action to do (withdraw, deposit)")] string action, [Summary("Catch ID or elaborate action (\"All\" if withdrawing")] string id)
        {
            string name = $"{Context.User.Username}'s Daycare";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Daycare };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { action, id }, true, Hub.Config.TradeCord);
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }
            await Util.EmbedUtil(Context, result.EmbedName, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordGift")]
        [Alias("gift", "g")]
        [Summary("Gifts a Pokémon to a mentioned user.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task Gift([Summary("Numerical catch ID")] string id, [Summary("User mention")] string _)
        {
            var embed = new EmbedBuilder { Color = Color.Purple };
            string name = $"{Context.User.Username}'s Gift";

            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg, embed).ConfigureAwait(false);
                return;
            }
            else if (Context.Message.MentionedUsers.Count == 0)
            {
                msg = "Please mention a user you're gifting a Pokémon to.";
                await Util.EmbedUtil(Context, name, msg, embed).ConfigureAwait(false);
                return;
            }
            else if (Context.Message.MentionedUsers.First().Id == Context.User.Id)
            {
                msg = "...Why?";
                await Util.EmbedUtil(Context, name, msg, embed).ConfigureAwait(false);
                return;
            }
            else if (Context.Message.MentionedUsers.First().IsBot)
            {
                msg = $"You tried to gift your Pokémon to {Context.Message.MentionedUsers.First().Username} but it came back!";
                await Util.EmbedUtil(Context, name, msg, embed).ConfigureAwait(false);
                return;
            }

            var mentionID = Context.Message.MentionedUsers.First().Id;
            var mentionName = Context.Message.MentionedUsers.First().Username;
            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, GifteeName = mentionName, GifteeID = mentionID, Context = TCCommandContext.Gift };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { id }, true, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message, embed).ConfigureAwait(false);
        }

        [Command("TradeCordTrainerInfoSet")]
        [Alias("tis")]
        [Summary("Sets individual trainer info for caught Pokémon.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TrainerInfoSet()
        {
            string name = $"{Context.User.Username}'s Trainer Info";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var attachments = Context.Message.Attachments;
            if (attachments.Count == 0 || attachments.Count > 1)
            {
                msg = $"Please attach a {(attachments.Count == 0 ? "" : "single ")}file.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var download = await NetUtil.DownloadPKMAsync(attachments.First()).ConfigureAwait(false);
            if (!download.Success)
            {
                msg = $"File download failed: \n{download.ErrorMessage}";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var pkm = download.Data!;
            var la = new LegalityAnalysis(pkm);
            if (!la.Valid || !(pkm is PK8))
            {
                msg = "Please upload a legal Gen8 Pokémon.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ot = pkm.OT_Name;
            var gender = $"{(Gender)pkm.OT_Gender}";
            var tid = $"{pkm.DisplayTID}";
            var sid = $"{pkm.DisplaySID}";
            var lang = $"{(LanguageID)pkm.Language}";

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.TrainerInfoSet };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { ot, gender, tid, sid, lang }, true, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordTrainerInfo")]
        [Alias("ti")]
        [Summary("Displays currently set trainer info.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TrainerInfo()
        {
            var name = $"{Context.User.Username}'s Trainer Info";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.TrainerInfo };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { }, false, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordFavorites")]
        [Alias("fav")]
        [Summary("Display favorites list.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordFavorites()
        {
            var name = $"{Context.User.Username}'s Favorites";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.FavoritesInfo };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { }, false, Hub.Config.TradeCord);
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }
            await Util.ListUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordFavorites")]
        [Alias("fav")]
        [Summary("Add/Remove a Pokémon to a favorites list.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordFavorites([Summary("Catch ID")] string id)
        {
            var name = $"{Context.User.Username}'s Favorite";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Favorites };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { id }, true, Hub.Config.TradeCord);
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }
            await Util.EmbedUtil(Context, result.EmbedName, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordDex")]
        [Alias("dex")]
        [Summary("Show missing dex entries, dex stats, boosted species.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordDex([Summary("Optional parameter \"missing\" for missing entries.")] string input = "")
        {
            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            input = input.ToLower();
            var name = $"{Context.User.Username}'s {(input == "missing" ? "Missing Entries" : "Dex Info")}";

            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }
            else if (input != "" && input != "missing")
            {
                msg = "Incorrect command input.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Dex };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { input }, false, Hub.Config.TradeCord);
            if (result.User.DexCompletionCount >= 1)
                embed.WithFooter(new EmbedFooterBuilder { Text = $"You have {result.User.DexCompletionCount} unused {(result.User.DexCompletionCount == 1 ? "perk" : "perks")}!\nType \"{Hub.Config.Discord.CommandPrefix}perks\" to view available perk names!" });

            if (input == "missing")
            {
                await Util.ListUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }
            await Util.EmbedUtil(Context, name, result.Message, embed).ConfigureAwait(false);
        }

        [Command("TradeCordDexPerks")]
        [Alias("dexperks", "perks")]
        [Summary("Display and use available Dex completion perks.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordDexPerks([Summary("Optional perk name and amount to add, or \"clear\" to remove all perks.")][Remainder] string input = "")
        {
            var embed = new EmbedBuilder { Color = Color.DarkBlue };
            string name = $"{Context.User.Username}'s Perks";
            input = input.ToLower();

            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Perks };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { input }, input != "", Hub.Config.TradeCord);
            if (result.Success && result.User.DexCompletionCount >= 1)
                embed.WithFooter(new EmbedFooterBuilder { Text = $"You have {result.User.DexCompletionCount} unused {(result.User.DexCompletionCount == 1 ? "perk" : "perks")}!" });

            await Util.EmbedUtil(Context, name, result.Message, embed).ConfigureAwait(false);
        }

        [Command("TradeCordSpeciesBoost")]
        [Alias("boost", "b")]
        [Summary("If set as an active perk, enter Pokémon species to boost appearance of.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordSpeciesBoost([Remainder] string input)
        {
            string name = $"{Context.User.Username}'s Species Boost";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Boost };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { input }, true, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordBuddy")]
        [Alias("buddy")]
        [Summary("View buddy or set a specified Pokémon as one.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordBuddy([Remainder] string input = "")
        {
            string name = $"{Context.User.Username}'s Buddy";
            input = input.ToLower();
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Buddy };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { input }, input != "", Hub.Config.TradeCord);
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }
            else if (result.Success && input != string.Empty)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }

            string footerMsg = string.Empty;
            bool canGmax = new ShowdownSet(ShowdownParsing.GetShowdownText(result.Poke)).CanGigantamax;
            if (!result.Poke.IsEgg)
                footerMsg = $"\n\n{TradeExtensions.DexFlavor(result.Poke.Species, result.Poke.Form, canGmax)}";
            else
            {
                double status = result.User.Buddy.HatchSteps / (double)result.Poke.PersonalInfo.HatchCycles;
                if (status is >= 0 and <= 0.25)
                    footerMsg = "It looks as though this Egg will take a long time yet to hatch.";
                else if (status is > 0.25 and <= 0.5)
                    footerMsg = "What Pokémon will hatch from this Egg? It doesn't seem close to hatching.";
                else if (status is > 0.5 and <= 0.75)
                    footerMsg = "It appears to move occasionally. It may be close to hatching.";
                else footerMsg = "Sounds can be heard coming from inside! This Egg will hatch soon!";
            }

            var pokeImg = TradeExtensions.PokeImg(result.Poke, canGmax, Hub.Config.TradeCord.UseFullSizeImages);
            var ballImg = $"https://raw.githubusercontent.com/BakaKaito/HomeImages/main/Ballimg/50x50/{((Ball)result.Poke.Ball).ToString().ToLower()}ball.png";
            var form = TradeExtensions.FormOutput(result.Poke.Species, result.Poke.Form, out _).Replace("-", "");
            var lvlProgress = (Experience.GetEXPToLevelUpPercentage(result.Poke.CurrentLevel, result.Poke.EXP, result.Poke.PersonalInfo.EXPGrowth) * 100.0).ToString("N1");
            msg = $"\nNickname: {result.User.Buddy.Nickname}" +
                  $"\nSpecies: {SpeciesName.GetSpeciesNameGeneration(result.Poke.Species, 2, 8)}" +
                  $"\nForm: {(form == string.Empty ? "Base" : form)}" +
                  $"\nAbility: {result.User.Buddy.Ability}" +
                  $"\nLevel: {result.Poke.CurrentLevel}" +
                  $"{(!result.Poke.IsEgg && result.Poke.CurrentLevel < 100 ? $"\nProgress to next level: {lvlProgress}%" : "")}";

            var author = new EmbedAuthorBuilder { Name = result.EmbedName, IconUrl = ballImg };
            var embed = new EmbedBuilder { Color = result.Poke.IsShiny ? Color.Blue : Color.DarkBlue, ThumbnailUrl = pokeImg }.WithFooter(x =>
            {
                x.Text = footerMsg;
                x.IconUrl = "https://i.imgur.com/nXNBrlr.png";
            }).WithAuthor(author).WithDescription(msg);

            await Context.Message.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordNickname")]
        [Alias("nickname", "nick")]
        [Summary("Sets a nickname for the active buddy.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordNickname([Remainder] string input)
        {
            string name = $"{Context.User.Username}'s Buddy Nickname";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            if (input.ToLower() != "clear")
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (!char.IsLetterOrDigit(input, i) && !char.IsWhiteSpace(input, i))
                    {
                        msg = "Emotes cannot be used in a nickname";
                        await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                        return;
                    }
                }
            }

            var ctx = new TradeExtensions.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Nickname };
            var result = TradeExtensions.ProcessTradeCord(ctx, new string[] { input }, true, Hub.Config.TradeCord);
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordMuteClear")]
        [Alias("mc")]
        [Summary("Remove the mentioned user from the mute list.")]
        [RequireSudo]
        public async Task TradeCordCommandClear([Remainder] string _)
        {
            if (Context.Message.MentionedUsers.Count == 0)
            {
                await ReplyAsync("Please mention a user.").ConfigureAwait(false);
                return;
            }

            var usr = Context.Message.MentionedUsers.First();
            bool mute = TradeExtensions.MuteList.Remove(usr.Id);
            var msg = mute ? $"{usr.Username} was unmuted." : $"{usr.Username} isn't muted.";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("TradeCordDeleteUser")]
        [Alias("du")]
        [Summary("Delete a user and all their catches via a provided numerical user ID.")]
        [RequireOwner]
        public async Task TradeCordDeleteUser(string input)
        {
            if (!ulong.TryParse(input, out ulong id))
            {
                await ReplyAsync("Could not parse user. Make sure you're entering a numerical user ID.").ConfigureAwait(false);
                return;
            }

            if (!TradeExtensions.DeleteUserData(id))
            {
                await ReplyAsync("Could not find data for this user.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync("Successfully removed the specified user's data.").ConfigureAwait(false);
        }

        private void TradeCordCooldown(ulong id)
        {
            if (Info.Hub.Config.TradeCord.TradeCordCooldown > 0)
            {
                if (!TradeExtensions.TradeCordCooldown.ContainsKey(id))
                    TradeExtensions.TradeCordCooldown.Add(id, DateTime.Now);
                else TradeExtensions.TradeCordCooldown[id] = DateTime.Now;
            }
        }

        private bool TradeCordCanCatch(ulong id, out TimeSpan timeRemaining)
        {
            timeRemaining = new();
            if (TradeExtensions.TradeCordCooldown.ContainsKey(id))
            {
                var timer = TradeExtensions.TradeCordCooldown[id].AddSeconds(Hub.Config.TradeCord.TradeCordCooldown);
                timeRemaining = timer - DateTime.Now;
                if (DateTime.Now < timer)
                    return false;
            }
            return true;
        }

        private bool TradeCordParanoiaChecks(out string msg)
        {
            msg = string.Empty;
            if (!Hub.Config.TradeCord.TradeCordChannels.Contains(Context.Channel.Id.ToString()) && !Hub.Config.TradeCord.TradeCordChannels.Equals(""))
            {
                msg = "You're typing the command in the wrong channel!";
                return false;
            }

            var id = Context.User.Id;
            if (!Directory.Exists("TradeCord") || !Directory.Exists($"TradeCord\\Backup\\{id}"))
            {
                Directory.CreateDirectory($"TradeCord\\{id}");
                Directory.CreateDirectory($"TradeCord\\Backup\\{id}");
            }
            else if (TradeExtensions.MuteList.Contains(id))
            {
                msg = "Command ignored due to suspicion of you running a script. Contact the bot owner if this is a false-positive.";
                return false;
            }

            if (!Hub.Config.Legality.AllowBatchCommands)
                Hub.Config.Legality.AllowBatchCommands = true;

            if (!Hub.Config.Legality.AllowTrainerDataOverride)
                Hub.Config.Legality.AllowTrainerDataOverride = true;

            if (Hub.Config.TradeCord.ConfigUpdateInterval < 30)
                Hub.Config.TradeCord.ConfigUpdateInterval = 60;

            msg = string.Empty;
            List<int> rateCheck = new();
            IEnumerable<int> p = new[] { Info.Hub.Config.TradeCord.TradeCordCooldown, Info.Hub.Config.TradeCord.CatchRate, Info.Hub.Config.TradeCord.CherishRate, Info.Hub.Config.TradeCord.EggRate, Info.Hub.Config.TradeCord.GmaxRate, Info.Hub.Config.TradeCord.SquareShinyRate, Info.Hub.Config.TradeCord.StarShinyRate };
            rateCheck.AddRange(p);
            if (rateCheck.Any(x => x < 0 || x > 100))
            {
                msg = "TradeCord settings cannot be less than zero or more than 100.";
                return false;
            }
            return true;
        }
    }
}