using Discord;
using System;
using System.Collections.Generic;

namespace SysBot.Base
{
    public static class EchoUtil
    {
        public static readonly List<Action<string>> Forwarders = new();
        public static readonly List<Action<string, Embed>> EmbedForwarders = new();
        public static readonly List<Action<byte[], string, EmbedBuilder>> RaidForwarders = new();

        public static void Echo(string message)
        {
            foreach (var fwd in Forwarders)
            {
                try
                {
                    fwd(message);
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"Exception: {ex} occurred while trying to echo: {message} to the forwarder: {fwd}", "Echo");
                    LogUtil.LogSafe(ex, "Echo");
                }
            }
            LogUtil.LogInfo(message, "Echo");
        }

        public static void EchoEmbed(string ping, string message, string url, string markurl, bool result)
        {
            foreach (var fwd in EmbedForwarders)
            {
                try
                {
                    if (!result)
                    {
                        ping = string.Empty;
                        if (string.IsNullOrEmpty(markurl))
                            markurl = $"https://i.imgur.com/t2M8qF4.png";

                        else
                            markurl = $"https://i.imgur.com/t2M8qF4.png";
                    }
                    else if (result)
                    {
                        if (string.IsNullOrEmpty(markurl))
                            markurl = "https://i.imgur.com/T8vEiIk.jpg";
                    }

                    var author = new EmbedAuthorBuilder { IconUrl = markurl, Name = result ? "Match found!" : "Unwanted match..." };
                    var embed = new EmbedBuilder
                    {
                        Color = result ? Color.Teal : Color.Red,
                        ThumbnailUrl = url
                    }.WithAuthor(author).WithDescription(message);

                    fwd(ping, embed.Build());
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"Exception: {ex} occurred while trying to echo: {message} to the forwarder: {fwd}", "Echo");
                    LogUtil.LogSafe(ex, "Echo");
                }
            }
            LogUtil.LogInfo(message, "Echo");
        }

        public static void RaidEmbed(byte[] bytes, string fileName, EmbedBuilder embeds)
        {
            foreach (var fwd in RaidForwarders)
            {
                try
                {
                    fwd(bytes, fileName, embeds);
                }
                catch (Exception ex)
                {
                    LogUtil.LogInfo($"Exception: {ex} occurred while trying to echo: RaidEmbed to the forwarder: {fwd}", "Echo");
                    LogUtil.LogSafe(ex, "Echo");
                }
            }
        }
    }
}