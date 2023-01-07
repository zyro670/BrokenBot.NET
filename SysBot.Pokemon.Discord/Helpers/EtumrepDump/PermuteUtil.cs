using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SysBot.Base;
using PermuteMMO.Lib;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord
{
    public class PermuteUtil
    {
        // Thanks to Zyro for his initial implementation of multis!
        // https://github.com/zyro670/NotForkBot.NET/commit/b64bbfc344fbbdaba9982a2b111c116698c830fe

        private static SlotDetail[][] FakeSlots = Array.Empty<SlotDetail[]>();

        private static void InitializeSlots()
        {
            FakeSlots = new SlotDetail[][]
            {
                new SlotDetail[]
                {
                    new(100, "Bidoof", false, new[] { 3, 6 }, 0),
                    new(2, "Bidoof", true, new[] { 17, 19 }, 3),
                    new(20, "Eevee", false, new[] { 3, 6 }, 0),
                    new(1, "Eevee", true, new[] { 17, 19 }, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Combee", false, new [] {17, 20}, 0),
                    new(2, "Combee", true , new [] {32, 35}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Qwilfish", false, new [] {41, 44}, 0),
                    new(1, "Qwilfish", true , new [] {56, 59}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Abra", false, new[] { 12, 15 }, 0),
                    new(2, "Abra", true, new[] { 27, 30 }, 3),
                    new(30, "Kadabra", false, new[] { 16, 19 }, 0),
                    new(1, "Kadabra", true, new[] { 31, 34 }, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Basculin-2", false, new [] {41, 44}, 0),
                    new(2, "Basculin-2", true , new [] {56, 59}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Magikarp", false, new [] {16, 19}, 0),
                    new(2, "Magikarp", true , new [] {31, 34}, 3),
                    new(30, "Gyarados", false, new [] {53, 56}, 0),
                    new(1, "Gyarados", true , new [] {68, 71}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Shellos", false, new [] {26, 29}, 0),
                    new(2, "Shellos", true , new [] {41, 44}, 3),
                    new(50, "Gastrodon", false, new [] {33, 36}, 0),
                    new(1, "Gastrodon", true , new [] {48, 51}, 3),
                },
                new SlotDetail[]
                {
                    new(40, "Ralts", false, new [] {19, 22}, 0),
                    new(1, "Ralts", true , new [] {34, 37}, 3),
                    new(100, "Budew", false, new [] {19, 22}, 0),
                    new(2, "Budew", true , new [] {34, 37}, 3),
                    new(50, "Roselia", false, new [] {19, 22}, 0),
                    new(1, "Roselia", true , new [] {34, 37}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Hippopotas", false, new [] {21, 24}, 0),
                    new(2, "Hippopotas", true , new [] {36, 39}, 3),
                    new(30, "Hippowdon", false, new [] {34, 37}, 0),
                    new(1, "Hippowdon", true , new [] {49, 52}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Aipom", false, new [] {24, 27}, 0),
                    new(2, "Aipom", true , new [] {39, 42}, 3),
                },
                new SlotDetail[]
                {
                    new(20, "Pikachu", false , new [] {9, 12}, 0),
                    new(1, "Pikachu", true, new [] {24, 27}, 3),
                    new(10, "Pichu", false, new [] {9, 12}, 0),
                    new(1, "Pichu", true , new [] {24, 27}, 3),
                    new(100, "Kricketot", false, new [] {6, 9}, 0),
                    new(2, "Kricketot", true , new [] {21, 24}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Psyduck", false, new [] {13, 16}, 0),
                    new(25, "Psyduck", false, new [] {13, 16}, 0),
                    new(2, "Psyduck", true, new [] {28, 31}, 3),
                    new(100, "Buneary", false , new [] {13, 16}, 0),
                    new(25, "Buneary", false , new [] {13, 16}, 0),
                    new(2, "Buneary", true , new [] {28, 31}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Petilil", false, new [] {33, 36}, 0),
                    new(2, "Petilil", true , new [] {48, 51}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Petilil", false, new [] {33, 36}, 0),
                    new(2, "Petilil", true , new [] {48, 51}, 3),
                    new(50, "Gastly", false , new [] {21, 24}, 0),
                    new(2, "Gastly", true, new [] {36, 39}, 3),
                    new(30, "Haunter", false, new [] {33, 36}, 0),
                    new(1, "Haunter", true , new [] {48, 51}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Glameow", false, new [] {34, 37}, 0),
                    new(1, "Glameow", true, new [] {49, 52}, 3),
                    new(40, "Purugly", false , new [] {41, 44}, 0),
                    new(1, "Purugly", true , new [] {56, 59}, 3),
                },
                new SlotDetail[]
                {
                    new(30, "Teddiursa", false , new [] {26, 29}, 0),
                    new(2, "Teddiursa", true, new [] {41, 44}, 3),
                    new(100, "Ursaring", false, new [] {37, 40}, 0),
                    new(1, "Ursaring", true , new [] {52, 55}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Beautifly", false, new [] {15, 18}, 0),
                    new(1, "Beautifly", true, new [] {30, 33}, 3),
                    new(100, "Mothim", false , new [] {20, 23}, 0),
                    new(1, "Mothim", true , new [] {35, 38}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Dustox", false, new [] {15, 18}, 0),
                    new(1, "Dustox", true, new [] {30, 33}, 3),
                    new(100, "Mothim", false , new [] {20, 23}, 0),
                    new(1, "Mothim", true , new [] {35, 38}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Murkrow", false, new [] {31, 34}, 0),
                    new(2, "Murkrow", true , new [] {46, 49}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Swinub", false, new [] {29, 32}, 0),
                    new(2, "Swinub", true , new [] {44, 47}, 3),
                    new(50, "Piloswine", false, new [] {47, 50}, 0),
                    new(1, "Piloswine", true , new [] {62, 65}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Paras", false, new [] {20, 23}, 0),
                    new(2, "Paras", true , new [] {35, 38}, 3),
                    new(50, "Parasect", false, new [] {25, 28}, 0),
                    new(1, "Parasect", true , new [] {40, 43}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Paras", false, new [] {20, 23}, 0),
                    new(2, "Paras", true , new [] {35, 38}, 3),
                    new(50, "Parasect", false, new [] {25, 28}, 0),
                    new(1, "Parasect", true , new [] {40, 43}, 3),
                    new(70, "Zubat", false, new [] {18, 21}, 0),
                    new(2, "Zubat", true , new [] {33, 36}, 3),
                    new(30, "Golbat", false, new [] {25, 28}, 0),
                    new(1, "Golbat", true , new [] {40, 43}, 3),
                },
                new SlotDetail[]
                {
                    new(100, "Rufflet", false, new [] {55, 58}, 0),
                    new(2, "Rufflet", true , new [] {70, 73}, 3),
                },
            };

            for (int s = 0; s < FakeSlots.Length; s++)
            {
                for (int i = 0; i < FakeSlots[s].Length; i++)
                    FakeSlots[s][i].SetSpecies();

                var key = (ulong)(0x1337BABE12345678 + Util.Rand.Next(1, 999999999));
                SpawnGenerator.EncounterTables.Add(key, FakeSlots[s]);
            }
        }

        public static async Task HandlePermuteRequestAsync(SocketMessageComponent component, string service, string id)
        {
            if (FakeSlots.Length is 0)
                InitializeSlots();

            if (id is "permute_yes")
            {
                var msg = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id}) wants to use PermuteMMO. Waiting for the user to choose a service...";
                LogUtil.LogInfo(msg, "[PermuteMMO Request]");

                var emb = component.Message.Embeds.First();
                var selectMenuBuilder = GetPermuteServiceSelectMenu();
                var menu = new ComponentBuilder().WithSelectMenu(selectMenuBuilder).Build();
                var embed = new EmbedBuilder
                {
                    Color = Color.Gold,
                    Description = "Please select PermuteMMO service you would like to use!",
                }.WithAuthor(x => { x.Name = emb.Author?.Name ?? "PermuteMMO Service"; }).Build();

                await component.Message.ReplyAsync(null, false, embed, null, null, menu).ConfigureAwait(false);
            }
            else if (id.Contains("permute_json_select"))
            {
                var spawnerVal = service is "multi" && component.Data.Values.First() is not "multi" ? component.Data.Values.First() : "";
                var msg = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id}) chose {(spawnerVal is not "" ? spawnerVal : service)}. {(service is "multi" && spawnerVal is "" ? "Waiting for the user to choose a spawner..." : "Waiting for the user to choose a filter...")}";
                LogUtil.LogInfo(msg, "[PermuteMMO Request]");

                var selectMenuBuilder = HandlePermuteSelectMenu(service, spawnerVal);
                var menu = new ComponentBuilder().WithSelectMenu(selectMenuBuilder).Build();
                var embed = new EmbedBuilder
                {
                    Color = Color.Gold,
                    Description = $"{(service is "multi" && spawnerVal is "" ? "Please select a spawner you would like to permute!" : "Please select your shiny path filter for PermuteMMO!")}",
                }.WithAuthor(x => { x.Name = "PermuteMMO Service"; }).Build();

                await component.Message.ModifyAsync(x => { x.Embed = embed; x.Components = menu; }).ConfigureAwait(false);
            }
            else if (id.Contains("permute_ready"))
            {
                var spawner = component.Data.CustomId.Split(';')[2];
                var filter = component.Data.CustomId.Split(';')[3];

                if (service is "mmo")
                    await PromptPermuteAsync(component, service, filter).ConfigureAwait(false);
                else await PromptPermuteMultiAsync(component, service, spawner, filter).ConfigureAwait(false);
            }
            else if (id is "permute_no")
            {
                var msg = component.Message.Embeds.First().Description.Split('\n').First();
                await UpdatePermuteEmbed(component.Message, msg, Color.LightOrange).ConfigureAwait(false);

                var username = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id})";
                LogUtil.LogInfo($"{username} did not wish to use PermuteMMO.", "[PermuteMMO Request]");
            }
        }

        public static async Task VerifyAndRunPermuteAsync(SocketModal modal, string service)
        {
            if (service is "multi")
                await RunPermuteMultiAsync(modal).ConfigureAwait(false);
            else await RunPermuteMmoAsync(modal).ConfigureAwait(false);
        }

        private static async Task RunPermuteMmoAsync(SocketModal modal)
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
                await ModalEmbedFollowupAsync(modal, msg, Color.Red);
                return;
            }

            bool invalidBonus = (info.BonusCount is not 0 and not 6 and not 7) || (info.BonusTable == "0x0000000000000000" && info.BonusCount != 0);
            if (invalidBonus)
            {
                msg += "Incorrect second wave spawn count specified, or no second wave provided with a non-zero second wave count.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowupAsync(modal, msg, Color.Red);
                return;
            }

            bool invalidBase = info.BaseCount is < 8 or > 15;
            if (invalidBase)
            {
                msg += "Incorrect first wave count specified.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowupAsync(modal, msg, Color.Red);
                return;
            }

            var filter = modal.Data.CustomId.Split(';')[2];
            await DoPermutationsAsync(modal, info!, filter, name).ConfigureAwait(false);
        }

        private static async Task RunPermuteMultiAsync(SocketModal modal)
        {
            var data = modal.Data;
            var name = $"{modal.User.Username}#{modal.User.Discriminator} ({modal.User.Id})";
            var msg = $"{name} has submitted their multispawner modal. Running PermuteMMO...";
            LogUtil.LogInfo(msg, "[PermuteMMO]");

            msg = "Invalid input: ";
            var seedInput = data.Components.FirstOrDefault(x => x.CustomId == "seed")?.Value;
            if (seedInput == default || !ulong.TryParse(seedInput, out var seed))
            {
                msg += "Invalid seed.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowupAsync(modal, msg, Color.Red);
                return;
            }

            var advancesInput = data.Components.First(x => x.CustomId == "advances")?.Value;
            if (advancesInput == default || !int.TryParse(advancesInput, out var advances) || advances < 0)
            {
                msg += "Invalid advances.";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowupAsync(modal, msg, Color.Red);
                return;
            }

            advances = advances == 0 ? 1 : advances > 20 ? 20 : advances;
            var filter = modal.Data.CustomId.Split(';')[3];
            var info = new UserEnteredSpawnInfo { Seed = seedInput, };
            await DoPermutationsAsync(modal, info, filter, name, "multi", advances);
        }

        public static async Task HandlePermuteButtonAsync(SocketMessageComponent component, string service)
        {
            var filter = component.Data.Values.First();
            var spawner = service is "multi" ? component.Data.CustomId.Split(';')[2] : "";
            var msg = $"{component.User.Username}#{component.User.Discriminator} ({component.User.Id}) has selected a filter: {filter}. {(service is "multi" ? "Waiting for seed and advances input..." : "Waiting for JSON input...")}";
            LogUtil.LogInfo(msg, "[PermuteMMO Request]");

            var buttonReady = new ButtonBuilder() { CustomId = $"permute_ready;{service};{spawner};{filter}", Label = "Ready", Style = ButtonStyle.Success };
            var components = new ComponentBuilder().WithButton(buttonReady);
            var desc = $"{(service is "mmo" ? "Please configure and generate your JSON by clicking [this link](https://shinyhunter.club/tools/permutemmo-spawners). Once done, click the button to let me know you're ready!" : "Click the button once you're ready to input your seed and advances.")}";

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

        private static async Task PromptPermuteAsync(SocketMessageComponent component, string service, string filter)
        {
            var box = new TextInputBuilder() { CustomId = $"permute_json;{service};{filter}", Label = "PermuteMMO JSON", Placeholder = "Paste the JSON output here...", Required = true }.WithStyle(TextInputStyle.Paragraph);
            var mod = new ModalBuilder() { Title = "PermuteMMO Service", CustomId = $"permute_json;{service};{filter}" }.AddTextInput(box);
            await component.RespondWithModalAsync(mod.Build()).ConfigureAwait(false);
        }

        private static async Task PromptPermuteMultiAsync(SocketMessageComponent component, string service, string spawner, string filter)
        {
            var box1 = new TextInputBuilder() { CustomId = "seed", Label = "Seed", Placeholder = "Paste your seed here...", Required = true }.WithStyle(TextInputStyle.Short);
            var box2 = new TextInputBuilder() { CustomId = "advances", Label = "Advances (max 20)", Placeholder = "Enter the number of advances...", Required = true }.WithStyle(TextInputStyle.Short);
            var mod = new ModalBuilder() { Title = "PermuteMMO Service", CustomId = $"permute_json;{service};{spawner};{filter}" }.AddTextInput(box1).AddTextInput(box2);

            await component.RespondWithModalAsync(mod.Build()).ConfigureAwait(false);
        }

        private static async Task DoPermutationsAsync(SocketModal modal, UserEnteredSpawnInfo info, string filter, string name, string service = "mmo", int advances = 15)
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
                if (service is "mmo")
                {
                    var spawner = info.GetSpawn();
                    seed = info.GetSeed();
                    meta = Permuter.Permute(spawner, seed, advances);
                }
                else
                {
                    var spawnerInput = int.Parse(modal.Data.CustomId.Split(';')[2]);
                    var key = SpawnGenerator.EncounterTables.FirstOrDefault(x => x.Value == FakeSlots[spawnerInput]).Key;
                    int count = spawnerInput is (2 or 12 or 13) ? 3 : 2;
                    var details = new SpawnCount(count, count);
                    var set = new SpawnSet(key, count);
                    var spawner = SpawnInfo.GetLoop(details, set, SpawnType.Regular);
                    advances = count is 3 ? 9 : advances;
                    seed = info.GetSeed();
                    meta = Permuter.Permute(spawner, seed, advances);
                }
            }
            catch
            {
                msg += "Failed to calculate shiny paths due to an unexpected error: is the provided info filled out with valid parameters?";
                LogUtil.LogInfo($"{name}: {msg}", "[PermuteMMO]");
                await ModalEmbedFollowupAsync(modal, msg, Color.Red);
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
                Description = $"**Here are your results for {path}!**\n[Check this link]({interpretUrl}) to learn more about path notations and how to find the minimum shiny rolls you'll need! Make sure you have enough for the path you choose!",
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

            int retryCount = 5;
            const int retryTime = 5_000;

            while (retryCount > 0)
            {
                try
                {
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                        x.Flags = flag;
                        x.Components = components;
                    }).ConfigureAwait(false);

                    break;
                }
                catch
                {
                    retryCount--;
                    if (retryCount is not 0)
                        await Task.Delay(retryTime).ConfigureAwait(false);
                }
            }
        }

        private static async Task ModalEmbedFollowupAsync(SocketModal modal, string desc, Color color)
        {
            var embed = new EmbedBuilder
            {
                Color = color,
                Description = desc,
            }.WithAuthor(x => { x.Name = "PermuteMMO Service"; });

            await modal.FollowupAsync(null, null, false, false, null, null, embed: embed.Build()).ConfigureAwait(false);
        }

        private static SelectMenuBuilder HandlePermuteSelectMenu(string service, string value) => service switch
        {
            "multi" when value is not "" => GetPathFilterMenu(service, value),
            "multi" => GetSpawnerSelectMenu(service),
            "mmo" => GetPathFilterMenu(service, value),
            _ => GetPathFilterMenu(service, value),
        };

        private static SelectMenuBuilder GetPathFilterMenu(string service, string value) => new()
        {
            CustomId = $"permute_json_filter;{service};{value}",
            MinValues = 1,
            MaxValues = 1,
            Placeholder = "Select your shiny path filter for PermuteMMO...",
            Options = new()
            {
                new SelectMenuOptionBuilder("Shiny", "shiny", "All shiny paths."),
                new SelectMenuOptionBuilder("Shiny AND alpha", "shalpha", "Only shiny alpha paths."),
                new SelectMenuOptionBuilder("Alpha", "alpha", "Only alpha paths, including non-shiny."),
            }
        };

        public static SelectMenuBuilder GetPermuteServiceSelectMenu() => new()
        {
            CustomId = "permute_json_select",
            MinValues = 1,
            MaxValues = 1,
            Placeholder = "Select which service you would like to use...",
            Options = new()
            {
                new SelectMenuOptionBuilder("MO/MMO", "mmo", "Massive Outbreak or Massive Mass Outbreak"),
                new SelectMenuOptionBuilder("Multi-Spawner", "multi", "Multi-Spawner for regular spawns."),
            }
        };

        private static SelectMenuBuilder GetSpawnerSelectMenu(string service) => new()
        {
            CustomId = $"permute_json_select;{service}",
            MinValues = 1,
            MaxValues = 1,
            Placeholder = "Select your species...",
            Options = new()
            {
                new SelectMenuOptionBuilder("Eevee/Bidoof", "0", "All shiny Eevee/Bidoof paths"),
                new SelectMenuOptionBuilder("Combee", "1", "All shiny Combee paths"),
                new SelectMenuOptionBuilder("Qwilfish", "2", "All shiny Qwilfish paths"),
                new SelectMenuOptionBuilder("Abra", "3", "All shiny Abra paths"),
                new SelectMenuOptionBuilder("Basculin", "4", "All shiny Basculin paths"),
                new SelectMenuOptionBuilder("Magikarp", "5", "All shiny Magikarp paths"),
                new SelectMenuOptionBuilder("Shellos", "6", "All shiny Shellos paths"),
                new SelectMenuOptionBuilder("Ralts", "7", "All shiny Ralts paths"),
                new SelectMenuOptionBuilder("Hippopotas", "8", "All shiny Hippopotas paths"),
                new SelectMenuOptionBuilder("Aipom", "9", "All shiny Aipom paths"),
                new SelectMenuOptionBuilder("Kricketot/Pichu/Pikachu", "10", "All shiny Kricketot/Pichu/Pikachu paths"),
                new SelectMenuOptionBuilder("Psyduck/Buneary", "11", "All shiny Psyduck/Buneary paths"),
                new SelectMenuOptionBuilder("Petilil", "12", "All shiny Petilil paths"),
                new SelectMenuOptionBuilder("Petilil/Gastly/Haunter", "13", "All shiny Petilil/Gastly/Haunter paths"),
                new SelectMenuOptionBuilder("Glameow/Purugly", "14", "All shiny Glameow/Purugly paths"),
                new SelectMenuOptionBuilder("Teddiursa/Ursaring", "15", "All shiny Teddiursa/Ursaring paths"),
                new SelectMenuOptionBuilder("Beautifly/Mothim", "16", "All shiny Beautifly/Mothim paths"),
                new SelectMenuOptionBuilder("Dustox/Mothim", "17", "All shiny Dustox/Mothim paths"),
                new SelectMenuOptionBuilder("Murkrow", "18", "All shiny Murkrow paths"),
                new SelectMenuOptionBuilder("Swinub/Piloswine", "19", "All shiny Swinub/Piloswine paths"),
                new SelectMenuOptionBuilder("Paras/Parasect", "20", "All shiny Paras/Parasect paths"),
                new SelectMenuOptionBuilder("Paras/Parasect/Zubat/Golbat", "21", "All shiny Paras/Parasect/Zubat/Golbat paths"),
                new SelectMenuOptionBuilder("Rufflet", "22", "All shiny Rufflet paths")
            }
        };
    }
}
