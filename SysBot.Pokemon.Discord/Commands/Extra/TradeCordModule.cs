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
        private readonly TradeCordHelper Helper = new(SysCordInstance.Self.Hub.Config.TradeCord);

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

            var timeRemaining = TradeCordHelperUtil.EventVoteTimer - DateTime.Now;
            if (timeRemaining.TotalSeconds > 0)
            {
                await ReplyAsync($"Please try again in about {(timeRemaining.Hours > 1 ? $"{timeRemaining.Hours} hours and " : timeRemaining.Hours > 0 ? $"{timeRemaining.Hours} hour and " : "")}{(timeRemaining.Minutes < 2 ? "1 minute" : $"{timeRemaining.Minutes} minutes")}");
                return;
            }

            TradeCordHelperUtil.EventVoteTimer = DateTime.Now.AddMinutes(Hub.Config.TradeCord.TradeCordEventCooldown + Hub.Config.TradeCord.TradeCordEventDuration);
            List<PokeEventType> events = new();
            PokeEventType[] vals = (PokeEventType[])Enum.GetValues(typeof(PokeEventType));
            while (events.Count < 5)
            {
                var rng = new Random();
                var rand = vals[rng.Next(vals.Length)];
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
                if (TradeCordHelperUtil.UserCommandTimestamps.ContainsKey(id))
                    TradeCordHelperUtil.UserCommandTimestamps[id].Add(DateTime.UtcNow);
                else TradeCordHelperUtil.UserCommandTimestamps.Add(id, new List<DateTime> { DateTime.UtcNow });

                var count = TradeCordHelperUtil.UserCommandTimestamps[id].Count;
                if (count >= 15 && TradeCordHelperUtil.SelfBotScanner(id, Hub.Config.TradeCord.TradeCordCooldown))
                {
                    var t = Task.Run(async () => await Util.ReactionVerification(Context).ConfigureAwait(false));
                    if (t.Result)
                        return;
                }
            }

            TradeCordCooldown(id);
            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Catch };
            var result = Helper.ProcessTradeCord(ctx, new string[] { });

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
                        await Context.Channel.SendPKMAsync(result.EggPoke, $"{result.Message}\n{ReusableActions.GetFormattedShowdownText(result.EggPoke)}").ConfigureAwait(false);
                        return;
                    }
                }
            }
            else if (result.FailedCatch)
            {
                var rng = new Random();
                var spookyRng = rng.Next(101);
                var imgRng = rng.Next(5);
                string[] sketchyCatches = { "https://i.imgur.com/BOb6IbW.png", "https://i.imgur.com/oSUQhYv.png", "https://i.imgur.com/81hlmGV.png", "https://i.imgur.com/7LBHLmf.png", "https://i.imgur.com/NEWEVtm.png" };
                var ball = (Ball)rng.Next(2, 26);
                var enumVals = (int[])Enum.GetValues(typeof(Gen8Dex));
                var speciesRand = rng.Next(enumVals.Length);
                var descF = $"You threw {(ball == Ball.Ultra ? "an" : "a")} {ball} Ball at a wild {(spookyRng >= 90 ? "...whatever that thing is" : SpeciesName.GetSpeciesNameGeneration(speciesRand, 2, 8))}...";
                msg = $"{(spookyRng >= 90 ? "One wiggle... Two... It breaks free and stares at you, smiling. You run for dear life." : "...but it managed to escape!")}";

                if (spookyRng >= 90 && result.Item != string.Empty)
                {
                    bool article = TradeCordHelperUtil.ArticleChoice(result.Item[0]);
                    msg += $"&^&\nAs you were running for your life, you tripped on {(article ? "an" : "a")} {result.Item}!";
                }
                else msg += result.Message;

                var authorF = new EmbedAuthorBuilder { Name = name };
                var footerF = new EmbedFooterBuilder { Text = $"{(spookyRng >= 90 ? $"But deep inside you know there is no escape... {(result.EggPokeID != 0 ? $"Egg ID {result.EggPokeID}" : "")}" : result.EggPokeID != 0 ? $"Egg ID {result.EggPokeID}" : "")}" };
                var embedF = new EmbedBuilder
                {
                    Color = Color.Teal,
                    ImageUrl = spookyRng >= 90 ? sketchyCatches[imgRng] : "",
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

            var form = nidoranGender != string.Empty ? nidoranGender : TradeCordHelperUtil.FormOutput(result.Poke.Species, result.Poke.Form, out _);
            var finalName = speciesName + form;
            var pokeImg = TradeExtensions.PokeImg(result.Poke, result.Poke.CanGigantamax, Hub.Config.TradeCord.UseFullSizeImages);
            var ballImg = $"https://raw.githubusercontent.com/BakaKaito/HomeImages/main/Ballimg/50x50/{((Ball)result.Poke.Ball).ToString().ToLower()}ball.png";
            var desc = $"You threw {(result.Poke.Ball == 2 ? "an" : "a")} {(Ball)result.Poke.Ball} Ball at a {(result.Poke.IsShiny ? $"**shiny** wild **{finalName}**" : $"wild {finalName}")}...";

            var author = new EmbedAuthorBuilder { Name = name };
            var footer = new EmbedFooterBuilder
            {
                Text = $"Catch {result.User.UserInfo.CatchCount} | Pokémon ID {result.PokeID}{(result.EggPokeID == 0 ? "" : $" | Egg ID {result.EggPokeID}")}",
                IconUrl = TradeCordHelperUtil.TimeOfDayString(result.User.UserInfo.TimeZoneOffset),
            };

            var embed = new EmbedBuilder
            {
                Color = (result.Poke.IsShiny && result.Poke.FatefulEncounter) || result.Poke.ShinyXor == 0 ? Color.Gold : result.Poke.IsShiny ? Color.LightOrange : Color.Teal,
                ImageUrl = pokeImg,
                Description = desc,
                Author = author,
                Footer = footer,
            };

            if (!Hub.Config.TradeCord.UseLargerPokeBalls)
                embed.Author.IconUrl = ballImg;
            else embed.ThumbnailUrl = ballImg;

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
            else if (TradeCordHelper.TradeCordTrades.TryGetValue(Context.User.Id, out _))
            {
                msg = "Please wait until your previous trade is fully processed.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Trade };
            var result = Helper.ProcessTradeCord(ctx, new string[] { id });
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }
            await Context.AddToQueueAsync(code, Context.User.Username, sig, result.Poke, PokeRoutineType.TradeCord, PokeTradeType.TradeCord, result.PokeID).ConfigureAwait(false);
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.List };
            var result = Helper.ProcessTradeCord(ctx, new string[] { content });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Info };
            var result = Helper.ProcessTradeCord(ctx, new string[] { id });
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }

            bool canGmax = new ShowdownSet(ShowdownParsing.GetShowdownText(result.Poke)).CanGigantamax;
            var pokeImg = TradeExtensions.PokeImg(result.Poke, canGmax, Hub.Config.TradeCord.UseFullSizeImages);
            string flavorText = $"\n\n{Helper.GetDexFlavorText(result.Poke.Species, result.Poke.Form, canGmax)}";

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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.MassRelease };
            var result = Helper.ProcessTradeCord(ctx, new string[] { species });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Release };
            var result = Helper.ProcessTradeCord(ctx, new string[] { id });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.DaycareInfo };
            var result = Helper.ProcessTradeCord(ctx, new string[] { });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Daycare };
            var result = Helper.ProcessTradeCord(ctx, new string[] { action, id });
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
            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, GifteeName = mentionName, GifteeID = mentionID, Context = TCCommandContext.Gift };
            var result = Helper.ProcessTradeCord(ctx, new string[] { id });
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
            var tid = $"{pkm.TrainerID7}";
            var sid = $"{pkm.TrainerSID7}";
            var lang = $"{(LanguageID)pkm.Language}";

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.TrainerInfoSet };
            var result = Helper.ProcessTradeCord(ctx, new string[] { ot, gender, tid, sid, lang });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.TrainerInfo };
            var result = Helper.ProcessTradeCord(ctx, new string[] { });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.FavoritesInfo };
            var result = Helper.ProcessTradeCord(ctx, new string[] { });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Favorites };
            var result = Helper.ProcessTradeCord(ctx, new string[] { id });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Dex };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            if (result.User.Dex.DexCompletionCount >= 1)
                embed.WithFooter(new EmbedFooterBuilder { Text = $"You have {result.User.Dex.DexCompletionCount} unused {(result.User.Dex.DexCompletionCount == 1 ? "perk" : "perks")}!\nType \"{Hub.Config.Discord.CommandPrefix}perks\" to view available perk names!" });

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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Perks };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            if (result.Success && result.User.Dex.DexCompletionCount >= 1)
                embed.WithFooter(new EmbedFooterBuilder { Text = $"You have {result.User.Dex.DexCompletionCount} unused {(result.User.Dex.DexCompletionCount == 1 ? "perk" : "perks")}!" });

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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Boost };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Buddy };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
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
                footerMsg = $"\n\n{Helper.GetDexFlavorText(result.Poke.Species, result.Poke.Form, canGmax)}";
            else
            {
                double status = result.Poke.CurrentFriendship / (double)result.Poke.PersonalInfo.HatchCycles;
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
            var form = TradeCordHelperUtil.FormOutput(result.Poke.Species, result.Poke.Form, out _).Replace("-", "");
            var lvlProgress = (Experience.GetEXPToLevelUpPercentage(result.Poke.CurrentLevel, result.Poke.EXP, result.Poke.PersonalInfo.EXPGrowth) * 100.0).ToString("N1");
            msg = $"\n**Nickname:** {result.User.Buddy.Nickname}" +
                  $"\n**Species:** {SpeciesName.GetSpeciesNameGeneration(result.Poke.Species, 2, 8)} {GameInfo.GenderSymbolUnicode[result.Poke.Gender].Replace("-", "")}" +
                  $"\n**Form:** {(form == string.Empty ? "-" : form)}" +
                  $"\n**Ability:** {result.User.Buddy.Ability}" +
                  $"\n**Level:** {result.Poke.CurrentLevel}" +
                  $"\n**Friendship:** {result.Poke.CurrentFriendship}" +
                  $"\n**Held item:** {GameInfo.Strings.itemlist[result.Poke.HeldItem]}" +
                  $"\n**Time of day:** {TradeCordHelperUtil.TimeOfDayString(result.User.UserInfo.TimeZoneOffset, false)}" +
                  $"{(!result.Poke.IsEgg && result.Poke.CurrentLevel < 100 ? $"\n**Progress to next level:** {lvlProgress}%" : "")}";

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

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Nickname };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordEvolution")]
        [Alias("evolve", "evo")]
        [Summary("Evolves the active buddy, if applicable.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TradeCordEvolution([Remainder][Summary("Usable item or Alcremie form.")] string input = "")
        {
            string name = $"{Context.User.Username}'s Evolution";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            input = input.Replace(" ", "").ToLower();
            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.Evolution };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }

            bool canGmax = new ShowdownSet(ShowdownParsing.GetShowdownText(result.Poke)).CanGigantamax;
            var pokeImg = TradeExtensions.PokeImg(result.Poke, canGmax, Hub.Config.TradeCord.UseFullSizeImages);
            string flavorText = Helper.GetDexFlavorText(result.Poke.Species, result.Poke.Form, canGmax);

            var author = new EmbedAuthorBuilder { Name = name };
            var embed = new EmbedBuilder
            {
                Color = result.Poke.IsShiny ? Color.Blue : Color.DarkBlue,
                ThumbnailUrl = pokeImg,
                Description = result.Message,
                Author = author,
            }.WithFooter(x => { x.Text = flavorText; x.IconUrl = "https://i.imgur.com/nXNBrlr.png"; });

            await Context.Channel.SendMessageAsync(null, false, embed : embed.Build()).ConfigureAwait(false);
        }

        [Command("TradeCordGiveItem")]
        [Alias("giveitem")]
        [Summary("Gives an item for your buddy to hold.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task GiveItem([Remainder][Summary("Item name")] string input)
        {
            string name = $"{Context.User.Username}'s Give Item";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            input = input.Replace(" ", "").ToLower();
            var item = TradeExtensions.EnumParse<TCItems>(input);
            if (item <= 0)
            {
                msg = "Not a valid item or item cannot be held.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.GiveItem };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordGiftItem")]
        [Alias("giftitem")]
        [Summary("Gifts an item to the mentioned user.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task GiftItem([Remainder][Summary("Item name")] string input)
        {
            string name = $"{Context.User.Username}'s Gift Item";
            var embed = new EmbedBuilder { Color = Color.Purple };

            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }
            else if (Context.Message.MentionedUsers.Count == 0)
            {
                msg = "Please mention a user you're gifting an item to.";
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
                msg = $"You tried to gift your item to {Context.Message.MentionedUsers.First().Username} but it had no idea what to do with it!";
                await Util.EmbedUtil(Context, name, msg, embed).ConfigureAwait(false);
                return;
            }

            input = input.ToLower().Split('<')[0].Trim();
            var count = input.Split(' ').Last();
            if (!int.TryParse(count, out _))
            {
                msg = "Could not parse the item count.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            input = input.Replace(count, "").Replace(" ", "");
            var item = TradeExtensions.EnumParse<TCItems>(input);
            if (item <= 0)
            {
                msg = "Not a valid item or item cannot be gifted.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var mentionID = Context.Message.MentionedUsers.First().Id;
            var mentionName = Context.Message.MentionedUsers.First().Username;
            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, GifteeID = mentionID, GifteeName = mentionName, Context = TCCommandContext.GiftItem };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input, count });
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordTakeItem")]
        [Alias("takeitem")]
        [Summary("Takes an item from your active buddy.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task TakeItem()
        {
            string name = $"{Context.User.Username}'s Take Item";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.TakeItem };
            var result = Helper.ProcessTradeCord(ctx, new string[] { });
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordItemList")]
        [Alias("itemlist", "il")]
        [Summary("Shows a list of items and their counts.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task ItemList([Remainder][Summary("Item name or search filter")] string input)
        {
            string name = $"{Context.User.Username}'s Item List";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            input = input.ToLower().Replace(" ", "");
            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.ItemList };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            if (!result.Success)
            {
                await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
                return;
            }

            await Util.ListUtil(Context, result.EmbedName, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordDropItem")]
        [Alias("dropitem", "drop")]
        [Summary("Drops one or more items.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task DropItem([Remainder][Summary("Item name or filter")] string input)
        {
            string name = $"{Context.User.Username}'s Item Drop";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            input = input.Replace(" ", "").ToLower();
            var item = TradeExtensions.EnumParse<TCItems>(input);
            if (item <= 0)
            {
                msg = "Not a valid item or item cannot be dropped.";
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.DropItem };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            await Util.EmbedUtil(Context, name, result.Message).ConfigureAwait(false);
        }

        [Command("TradeCordTimeZone")]
        [Alias("timezone", "tz")]
        [Summary("Set UTC time offset for certain time of day events.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTradeCord))]
        public async Task SetTimeZone([Summary("UTC hour offset (negative, zero, or positive)")] string input)
        {
            string name = $"{Context.User.Username}'s Time Zone";
            if (!TradeCordParanoiaChecks(out string msg))
            {
                await Util.EmbedUtil(Context, name, msg).ConfigureAwait(false);
                return;
            }

            var ctx = new TradeCordHelper.TC_CommandContext { Username = Context.User.Username, ID = Context.User.Id, Context = TCCommandContext.TimeZone };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
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
            bool mute = TradeCordHelperUtil.MuteList.Remove(usr.Id);
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

            var ctx = new TradeCordHelper.TC_CommandContext { ID = id, Context = TCCommandContext.DeleteUser };
            var result = Helper.ProcessTradeCord(ctx, new string[] { input });
            result.Message = result.Success ? "Successfully deleted specified user's data." : "No data found for this user.";
            await Util.EmbedUtil(Context, result.EmbedName, result.Message).ConfigureAwait(false);
        }

        private void TradeCordCooldown(ulong id)
        {
            if (Info.Hub.Config.TradeCord.TradeCordCooldown > 0)
            {
                if (!TradeCordHelperUtil.TradeCordCooldownDict.ContainsKey(id))
                    TradeCordHelperUtil.TradeCordCooldownDict.Add(id, DateTime.Now);
                else TradeCordHelperUtil.TradeCordCooldownDict[id] = DateTime.Now;
            }
        }

        private bool TradeCordCanCatch(ulong id, out TimeSpan timeRemaining)
        {
            timeRemaining = new();
            if (TradeCordHelperUtil.TradeCordCooldownDict.ContainsKey(id))
            {
                var timer = TradeCordHelperUtil.TradeCordCooldownDict[id].AddSeconds(Hub.Config.TradeCord.TradeCordCooldown);
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
            if (!Directory.Exists("TradeCord"))
                Directory.CreateDirectory("TradeCord");
            else if (TradeCordHelperUtil.MuteList.Contains(id))
            {
                msg = "Command ignored due to suspicion of you running a script. Contact the bot owner if this is a false-positive.";
                return false;
            }

            if (!Hub.Config.Legality.AllowBatchCommands)
                Hub.Config.Legality.AllowBatchCommands = true;

            if (!Hub.Config.Legality.AllowTrainerDataOverride)
                Hub.Config.Legality.AllowTrainerDataOverride = true;

            msg = string.Empty;
            List<int> rateCheck = new();
            IEnumerable<int> p = new[] { Hub.Config.TradeCord.TradeCordCooldown, Hub.Config.TradeCord.CatchRate, Hub.Config.TradeCord.CherishRate, Hub.Config.TradeCord.EggRate, Hub.Config.TradeCord.ItemRate, Hub.Config.TradeCord.GmaxRate, Hub.Config.TradeCord.SquareShinyRate, Hub.Config.TradeCord.StarShinyRate };
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