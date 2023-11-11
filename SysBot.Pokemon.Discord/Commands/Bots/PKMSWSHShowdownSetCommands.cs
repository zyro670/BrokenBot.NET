using Discord;
using Discord.Commands;
using PKHeX.Core;
using Sysbot.Pokemon.Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Queues new Link Code trades")]
    public class PKMSWSHShowdownSetCommands<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        public static Dictionary<string, PKMSWSHShowdownSets> SWSHShowdownSetsCollection = PKMSWSHModule.SWSHShowdownSetsCollection;

        [Command("showdownSWSH")]
        [Alias("swsh")]
        public async Task ShowdownAsync(params string[] userSetParts)
        {
            await CallForShowdownSets(userSetParts);
            // Delete the command message from the channel
            await Context.Message.DeleteAsync();
            var requester = Context.User.Username;
            var privateMsgNotification = $"**{requester}** the requested showdown set's have been sent to your DM's for privacy.";

            var embedPrivateMsgNotification = new EmbedBuilder()
            {

                Color = Color.Blue
            }
            .WithDescription(privateMsgNotification)
            .WithImageUrl("https://i.imgur.com/5akyLET.png")
            .Build();
            await Context.Channel.SendMessageAsync(null, false, embedPrivateMsgNotification);
        }


        public async Task CallForShowdownSets(params string[] userSetParts)
        {
            string userSet = string.Join("-", userSetParts);
            var processed = ($"Processed showdown command with parameter: **{userSet}**");
            var embedProcessed = new EmbedBuilder()
            {
                Color = Color.Blue
            }
            .WithDescription(processed)
            .Build();
            await Context.Channel.SendMessageAsync(null, false, embedProcessed);

            List<PKMSWSHShowdownSets> matchedSets = new List<PKMSWSHShowdownSets>();

            if (userSetParts.Length == 1)
            {
                foreach (var set in SWSHShowdownSetsCollection)
                {
                    int distance = UseFuzzyStringMatching(set.Value.Name, userSet);
                    if (distance <= 2)
                        matchedSets.Add(set.Value);
                }
            }
            else
            {
                if (SWSHShowdownSetsCollection.TryGetValue(userSet, out var specificSet))
                {
                    matchedSets.Add(specificSet);
                }
            }

            if (matchedSets.Count > 0)
            {
                foreach (var goodSet in matchedSets)
                {
                    StringBuilder message = new StringBuilder("```\n");
                    string displayName = goodSet.Name;
                    if (!string.IsNullOrEmpty(goodSet.Form))
                        displayName += $"-{goodSet.Form}";

                    message.AppendLine(displayName);
                    if (goodSet.Ability != null) message.AppendLine($"Ability: {goodSet.Ability}");
                    
                    if (goodSet.Item != null) message.AppendLine($"Item: {goodSet.Item}");
                    if (goodSet.Level != null) message.AppendLine($"Level: {goodSet.Level}");
                    if (goodSet.Shiny != null) message.AppendLine($"Shiny: {goodSet.Shiny}");
                    if (goodSet.Nature != null) message.AppendLine($"{goodSet.Nature} Nature");
                    foreach (var move in goodSet.Moves)
                    {
                        if (move != null) message.AppendLine($"- {move}");
                    }
                    message.Append("```");

                    string legalSet = ($"Below is a legal set for: **{displayName}**.");
                    var embedLegalSet = new EmbedBuilder
                    {
                        Color = Color.Blue
                    }
                    .WithDescription(legalSet)
                    .Build();

                    // Send the message to the user's DMs
                    await Context.User.SendMessageAsync(null, false, embedLegalSet);

                    await Context.User.SendMessageAsync(message.ToString());
                }
            }
            else
            {
                // If no set found, send that message to the user's DMs
                var embedNoShowdownSetFound = new EmbedBuilder
                {
                    Color = Color.Blue
                }
                .WithDescription($"No showdown set found for: **{userSet}**.")
                .Build();

                await Context.User.SendMessageAsync(null, false, embedNoShowdownSetFound);
            }
        }


        public static int UseFuzzyStringMatching(string s, string t)
        {
            s = RemoveDiacritics(s);
            t = RemoveDiacritics(t);

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
