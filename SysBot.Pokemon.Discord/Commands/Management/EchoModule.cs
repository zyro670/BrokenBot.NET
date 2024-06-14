using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SysBot.Base;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public class EchoModule : ModuleBase<SocketCommandContext>
{
    private class EchoChannel
    {
        public readonly ulong ChannelID;
        public readonly string ChannelName;
        public readonly Action<string> Action;            
        public readonly Action<byte[], string, EmbedBuilder> RaidAction;
        public string EmbedResult = string.Empty;

        public EchoChannel(ulong channelId, string channelName, Action<string> action, Action<byte[], string, EmbedBuilder> raidAction)
        {
            ChannelID = channelId;
            ChannelName = channelName;
            Action = action;
            RaidAction = raidAction;                
        }
    }

    private class EncounterEchoChannel
    {
        public readonly ulong ChannelID;
        public readonly string ChannelName;
        public readonly Action<string, Embed> EmbedAction;
        public string EmbedResult = string.Empty;

        public EncounterEchoChannel(ulong channelId, string channelName, Action<string, Embed> embedaction)
        {
            ChannelID = channelId;
            ChannelName = channelName;
            EmbedAction = embedaction;
        }
    }

    private static readonly Dictionary<ulong, EchoChannel> Channels = [];
    private static readonly Dictionary<ulong, EncounterEchoChannel> EncounterChannels = [];

    public static void RestoreChannels(DiscordSocketClient discord, DiscordSettings cfg)
    {
        foreach (var ch in cfg.EchoChannels)
        {
            if (discord.GetChannel(ch.ID) is ISocketMessageChannel c)
                AddEchoChannel(c, ch.ID);
        }

        foreach (var ch in cfg.EncounterEchoChannels)
        {
            if (discord.GetChannel(ch.ID) is ISocketMessageChannel c)
                AddEncounterEchoChannel(c, ch.ID);
        }

        if (SysCordSettings.Settings.EchoOnBotStart)
            EchoUtil.Echo("Added echo notification to Discord channel(s) on Bot startup.");
    }

    [Command("echoHere")]
    [Summary("Makes the bot echo special messages to the channel.")]
    [RequireSudo]
    public async Task AddEchoAsync()
    {
        var c = Context.Channel;
        var cid = c.Id;
        if (Channels.TryGetValue(cid, out _))
        {
            await ReplyAsync("Already notifying here.").ConfigureAwait(false);
            return;
        }

        AddEchoChannel(c, cid);

        // Add to discord global loggers (saves on program close)
        SysCordSettings.Settings.EchoChannels.AddIfNew([GetReference(Context.Channel)]);
        await ReplyAsync("Added Echo output to this channel!").ConfigureAwait(false);
    }

    private static void AddEchoChannel(ISocketMessageChannel c, ulong cid)
    {
        void Echo(string msg) => c.SendMessageAsync(msg);            
        async Task RaidEmbedAsync(byte[] bytes, string fileName, EmbedBuilder embed)
        {
            try
            {
                if (bytes is not null && bytes.Length > 0)
                {
                    await c.SendFileAsync(new MemoryStream(bytes), fileName, "", false, embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await c.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Failed to send embed to channel '{c.Name}': {ex.Message}", nameof(AddEchoChannel));
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
        Action<byte[], string, EmbedBuilder> rb = async (bytes, fileName, embed) => await RaidEmbedAsync(bytes, fileName, embed).ConfigureAwait(false);
        Action<string> l = Echo;
        EchoUtil.Forwarders.Add(l);
        EchoUtil.RaidForwarders.Add(rb);
        var entry = new EchoChannel(cid, c.Name, l, rb);
        Channels.Add(cid, entry);
    }

    [Command("embedHere")]
    [Summary("Makes the bot echo special Encounter messages to the channel.")]
    [RequireSudo]
    public async Task AddEncounterEchoAsync()
    {
        var c = Context.Channel;
        var cid = c.Id;
        if (EncounterChannels.TryGetValue(cid, out _))
        {
            await ReplyAsync("Already notifying here.").ConfigureAwait(false);
            return;
        }

        AddEncounterEchoChannel(c, cid);

        // Add to discord global loggers (saves on program close)
        SysCordSettings.Settings.EncounterEchoChannels.AddIfNew(new[] { GetReference(Context.Channel) });
        await ReplyAsync("Added Echo output to this channel!").ConfigureAwait(false);
    }

    private static void AddEncounterEchoChannel(ISocketMessageChannel c, ulong cid)
    {
        void EncounterEchoEmbed(string ping, Embed embed) => c.SendMessageAsync(ping, false, embed);
        Action<string, Embed> lb = EncounterEchoEmbed;

        EchoUtil.EmbedForwarders.Add(lb);
        var entry = new EncounterEchoChannel(cid, c.Name, lb);
        EncounterChannels.Add(cid, entry);
    }

    public static bool IsEchoChannel(ISocketMessageChannel c)
    {
        var cid = c.Id;
        return Channels.TryGetValue(cid, out _);
    }

    public static bool IsEmbedEchoChannel(ISocketMessageChannel c)
    {
        var cid = c.Id;
        return EncounterChannels.TryGetValue(cid, out _);
    }

    [Command("echoInfo")]
    [Summary("Dumps the special message (Echo) settings.")]
    [RequireSudo]
    public async Task DumpEchoInfoAsync()
    {
        foreach (var c in Channels)
            await ReplyAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
    }

    [Command("echoClear")]
    [Summary("Clears the special message echo settings in that specific channel.")]
    [RequireSudo]
    public async Task ClearEchosAsync()
    {
        var id = Context.Channel.Id;
        if (!Channels.TryGetValue(id, out var echo))
        {
            await ReplyAsync("Not echoing in this channel.").ConfigureAwait(false);
            return;
        }
        EchoUtil.Forwarders.Remove(echo.Action);
        EchoUtil.RaidForwarders.Remove(echo.RaidAction);
        Channels.Remove(Context.Channel.Id);
        SysCordSettings.Settings.EchoChannels.RemoveAll(z => z.ID == id);
        await ReplyAsync($"Echoes cleared from channel: {Context.Channel.Name}").ConfigureAwait(false);
    }

    [Command("echoClearAll")]
    [Summary("Clears all the special message Echo channel settings.")]
    [RequireSudo]
    public async Task ClearEchosAllAsync()
    {
        foreach (var l in Channels)
        {
            var entry = l.Value;
            await ReplyAsync($"Echoing cleared from {entry.ChannelName} ({entry.ChannelID}!").ConfigureAwait(false);
            EchoUtil.Forwarders.Remove(entry.Action);
        }
        EchoUtil.Forwarders.RemoveAll(y => Channels.Select(x => x.Value.Action).Contains(y));
        EchoUtil.RaidForwarders.RemoveAll(y => Channels.Select(x => x.Value.RaidAction).Contains(y));
        Channels.Clear();
        SysCordSettings.Settings.EchoChannels.Clear();
        await ReplyAsync("Echoes cleared from all channels!").ConfigureAwait(false);
    }

    [Command("embedInfo")]
    [Summary("Dumps the special message (Echo) settings.")]
    [RequireSudo]
    public async Task DumpEmbedEchoInfoAsync()
    {
        foreach (var c in EncounterChannels)
            await ReplyAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
    }

    [Command("embedClear")]
    [Summary("Clears the special message echo settings in that specific channel.")]
    [RequireSudo]
    public async Task ClearEmbedEchosAsync()
    {
        var id = Context.Channel.Id;
        if (!EncounterChannels.TryGetValue(id, out var echo))
        {
            await ReplyAsync("Not echoing in this channel.").ConfigureAwait(false);
            return;
        }
        EchoUtil.EmbedForwarders.Remove(echo.EmbedAction);
        EncounterChannels.Remove(Context.Channel.Id);
        SysCordSettings.Settings.EncounterEchoChannels.RemoveAll(z => z.ID == id);
        await ReplyAsync($"Embed echoes cleared from channel: {Context.Channel.Name}").ConfigureAwait(false);
    }

    [Command("embedClearAll")]
    [Summary("Clears all the special message Echo embed channel settings.")]
    [RequireSudo]
    public async Task ClearEmbedEchosAllAsync()
    {
        foreach (var l in EncounterChannels)
        {
            var entry = l.Value;
            await ReplyAsync($"Embed echoing cleared from {entry.ChannelName} ({entry.ChannelID}!").ConfigureAwait(false);
            EchoUtil.EmbedForwarders.Remove(entry.EmbedAction);
        }
        EchoUtil.EmbedForwarders.RemoveAll(y => EncounterChannels.Select(x => x.Value.EmbedAction).Contains(y));
        EncounterChannels.Clear();
        SysCordSettings.Settings.EncounterEchoChannels.Clear();
        await ReplyAsync("Embed echoes cleared from all channels!").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

}