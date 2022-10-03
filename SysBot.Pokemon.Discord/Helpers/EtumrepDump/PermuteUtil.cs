using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SysBot.Base;
using PermuteMMO.Lib;

namespace SysBot.Pokemon.Discord
{
    public class PermuteUtil
    {
        public static async Task HandlePermuteRequestAsync(SocketMessageComponent component, string id)
        {
            if (id is "permute_yes")
            {
                var msg = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id}) wants to use PermuteMMO. Waiting for the user to choose a filter...";
                LogUtil.LogInfo(msg, "[PermuteMMO Request]");

                var emb = component.Message.Embeds.First();
                var selectMenuBuilder = GetPermuteSelectMenu();
                var menu = new ComponentBuilder().WithSelectMenu(selectMenuBuilder).Build();
                var embed = new EmbedBuilder
                {
                    Color = Color.Gold,
                    Description = "Please select your shiny path filter for PermuteMMO!",
                }.WithAuthor(x => { x.Name = emb.Author?.Name ?? "PermuteMMO Service"; }).Build();

                await component.RespondAsync(null, null, false, false, null, menu, embed).ConfigureAwait(false);
            }
            else if (id.Contains("permute_ready"))
            {
                var filter = component.Data.CustomId.Split(';')[1];
                var box = new TextInputBuilder() { CustomId = $"permute_json;{filter}", Label = "PermuteMMO JSON", Placeholder = "Paste the JSON output here...", Required = true }.WithStyle(TextInputStyle.Paragraph);
                var mod = new ModalBuilder() { Title = "PermuteMMO Service", CustomId = $"permute_json;{filter}" }.AddTextInput(box);
                await component.RespondWithModalAsync(mod.Build()).ConfigureAwait(false);
            }
            else if (id is "permute_no")
            {
                var msg = component.Message.Embeds.First().Description.Split('\n').First();
                await UpdatePermuteEmbed(component.Message, msg, Color.LightOrange).ConfigureAwait(false);

                var username = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id})";
                LogUtil.LogInfo($"{username} did not wish to use PermuteMMO.", "[PermuteMMO Request]");
            }
        }

        public static async Task VerifyAndRunPermuteAsync(SocketModal modal)
        {
            var data = modal.Data;
            var json = data.Components.First().Value;

            var name = $"{modal.User.Username}#{modal.User.Discriminator} ({modal.User.Id})";
            var msg = $"{name} has submitted their JSON. Running PermuteMMO...";
            LogUtil.LogInfo(msg, "[PermuteMMO]");

            UserEnteredSpawnInfo? info;
            try
            {
                info = JsonConvert.DeserializeObject<UserEnteredSpawnInfo>(json);
            }
            catch
            {
                info = null;
            }

            msg = "Provided JSON is invalid: ";
            if (info is null)
            {
                msg += "Invalid JSON format.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowup(modal, msg, Color.Red);
                return;
            }

            bool invalidBonus = (info.BonusCount is not 0 and not 6 and not 7) || (info.BonusTable == "0x0000000000000000" && info.BonusCount != 0);
            if (invalidBonus)
            {
                msg += "Incorrect second wave spawn count specified, or no second wave provided with a non-zero second wave count.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowup(modal, msg, Color.Red);
                return;
            }

            bool invalidBase = info.BaseCount is < 8 or > 15;
            if (invalidBase)
            {
                msg += "Incorrect first wave count specified.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowup(modal, msg, Color.Red);
                return;
            }

            var filter = modal.Data.CustomId.Split(';')[1];
            await DoPermutationsAsync(modal, info!, filter, name).ConfigureAwait(false);
        }

        public static async Task GetPermuteFilterAsync(SocketMessageComponent component)
        {
            var filter = component.Data.Values.First();
            var msg = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id}) has selected a filter: {filter}. Waiting for JSON input...";
            LogUtil.LogInfo(msg, "[PermuteMMO Request]");

            var buttonReady = new ButtonBuilder() { CustomId = $"permute_ready;{filter}", Label = "Ready", Style = ButtonStyle.Success };
            var components = new ComponentBuilder().WithButton(buttonReady);
            var desc = "Please configure and generate your JSON by clicking [this link](https://shinyhunter.club/tools/permutemmo-spawners). Once done, click the button to let me know you're ready!";

            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Description = desc,
            }.WithAuthor(x => { x.Name = "PermuteMMO Service"; });

            await component.Message.ModifyAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = components.Build();
            }).ConfigureAwait(false);
        }

        private static async Task DoPermutationsAsync(SocketModal modal, UserEnteredSpawnInfo info, string filter, string name)
        {
            string msg = string.Empty;
            PermuteMeta.SatisfyCriteria = (result, advances) => filter switch
            {
                "shiny" => result.IsShiny,
                "shalpha" => result.IsShiny && result.IsAlpha,
                "alpha" => result.IsAlpha || (result.IsShiny && result.IsAlpha),
                _ => result.IsShiny,
            };

            string path = filter switch
            {
                "shiny" => "all shiny paths",
                "shalpha" => "all shiny alpha paths",
                "alpha" => "all alpha paths",
                _ => "all shiny paths",
            };

            ulong seed;
            PermuteMeta meta;
            try
            {
                seed = info.GetSeed();
                var spawner = info.GetSpawn();
                meta = Permuter.Permute(spawner, seed);
            }
            catch
            {
                msg += "Failed to calculate shiny paths due to an unexpected error: is the provided JSON filled out with valid parameters?";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowup(modal, msg, Color.Red);
                return;
            }

            if (!meta.HasResults)
            {
                msg = "No shiny path results found.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");

                var embedNoRes = new EmbedBuilder
                {
                    Color = Color.Red,
                    Description = msg,
                }.WithAuthor(x => { x.Name = "PermuteMMO Service"; });

                await modal.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embedNoRes.Build();
                    x.Components = new ComponentBuilder().Build();
                }).ConfigureAwait(false);
                return;
            }

            msg = $"Permutation complete! Sending {name} their results!";
            LogUtil.LogInfo(msg, "[PermuteMMO]");

            var interpretUrl = "https://github.com/kwsch/PermuteMMO/wiki#interpreting-output";
            var shinyRollUrl = "https://cdn.discordapp.com/attachments/958046779750875146/998925407745220678/IMG_4793-1.png";
            var embed = new EmbedBuilder
            {
                Color = Color.Gold,
                Description = $"**Here are your results for {path}!**\n[Check this link]({interpretUrl}) to learn more about path notations! Make sure you have enough shiny rolls for the path you choose!",
                ImageUrl = shinyRollUrl,
            }.WithAuthor(x => { x.Name = "PermuteMMO Service"; });

            var res = string.Join("\n", meta.GetLines());
            var bytes = Encoding.UTF8.GetBytes(res);
            var ms = new MemoryStream(bytes);
            var att = new FileAttachment[] { new FileAttachment(ms, $"PermuteMMO_{seed}.txt") };

            await modal.ModifyOriginalResponseAsync(x =>
            {
                x.Attachments = att;
                x.Embed = embed.Build();
                x.Components = new ComponentBuilder().Build();
            }).ConfigureAwait(false);
        }

        private static async Task UpdatePermuteEmbed(SocketUserMessage message, string desc, Color color, MessageComponent? components = null, MessageFlags flag = MessageFlags.None, string? authorName = null)
        {
            var embed = new EmbedBuilder
            {
                Color = color,
                Description = desc,
            }.WithAuthor(x => { x.Name = authorName ?? "PermuteMMO Service"; });

            await message.ModifyAsync(x =>
            {
                x.Embed = embed.Build();
                x.Flags = flag;
                x.Components = components;
            }).ConfigureAwait(false);
        }

        private static async Task ModalEmbedFollowup(SocketModal modal, string desc, Color color)
        {
            var embed = new EmbedBuilder
            {
                Color = color,
                Description = desc,
            }.WithAuthor(x => { x.Name = "PermuteMMO Service"; });

            await modal.FollowupAsync(null, null, false, false, null, null, embed: embed.Build()).ConfigureAwait(false);
        }

        public static SelectMenuBuilder GetPermuteSelectMenu()
        {
            var shiny = new SelectMenuOptionBuilder("Shiny", "shiny", "All shiny paths.");
            var shalpha = new SelectMenuOptionBuilder("Shiny AND alpha", "shalpha", "Only shiny alpha paths.");
            var alpha = new SelectMenuOptionBuilder("Alpha", "alpha", "Only alpha paths, including non-shiny.");
            var component = new SelectMenuBuilder() { CustomId = "permute_json_filter", MinValues = 1, MaxValues = 1, Placeholder = "Select your shiny path filter for PermuteMMO...", Options = new() { shiny, shalpha, alpha } };
            return component;
        }
    }
}
