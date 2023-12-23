using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord;

namespace SysBot.Pokemon.Twitch;

public static class TwitchCommandsHelper<T> where T : PKM, new()
{
    // Helper functions for commands
    public static bool AddToWaitingList(string setstring, string display, string username, ulong mUserId, bool sub, out string msg)
    {
        if (!TwitchBot<T>.Info.GetCanQueue())
        {
            msg = "Sorry, I am not currently accepting queue requests!";
            return false;
        }

        var set = ShowdownUtil.ConvertToShowdown(setstring);
        if (set == null)
        {
            msg = $"Skipping trade, @{username}: Empty nickname provided for the species.";
            return false;
        }
        var template = AutoLegalityWrapper.GetTemplate(set);
        if (template.Species < 1)
        {
            msg = $"Skipping trade, @{username}: Please read what you are supposed to type as the command argument.";
            return false;
        }

        if (set.InvalidLines.Count != 0)
        {
            msg = $"Skipping trade, @{username}: Unable to parse Showdown Set:\n{string.Join("\n", set.InvalidLines)}";
            return false;
        }

        try
        {
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            PKM pkm = sav.GetLegal(template, out var result);

            var nickname = pkm.Nickname.ToLower();
            if (nickname == "egg" && Breeding.CanHatchAsEgg(pkm.Species))
                TradeExtensions<T>.EggTrade(pkm, template);

            if (pkm.Species == 132 && (nickname.Contains("atk") || nickname.Contains("spa") || nickname.Contains("spe") || nickname.Contains("6iv")))
                TradeExtensions<T>.DittoTrade(pkm);

            if (!pkm.CanBeTraded())
            {
                msg = $"Skipping trade, @{username}: Provided Pokémon content is blocked from trading!";
                return false;
            }

            if (pkm is T pk)
            {
                var valid = new LegalityAnalysis(pkm).Valid;
                if (valid)
                {
                    var tq = new TwitchQueue<T>(pk, new PokeTradeTrainerInfo(display, mUserId), username, sub);
                    TwitchBot<T>.QueuePool.RemoveAll(z => z.UserName == username); // remove old requests if any
                    TwitchBot<T>.QueuePool.Add(tq);
                    msg = $"@{username} - added to the waiting list. Please whisper your trade code to me! Your request from the waiting list will be removed if you are too slow!";
                    return true;
                }
            }

            var reason = result == "Timeout" ? "Set took too long to generate." : "Unable to legalize the Pokémon.";
            msg = $"Skipping trade, @{username}: {reason}";
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, nameof(TwitchCommandsHelper<T>));
            msg = $"Skipping trade, @{username}: An unexpected problem occurred.";
        }
        return false;
    }

    public static string ClearTrade(string user)
    {
        var result = TwitchBot<T>.Info.ClearTrade(user);
        return GetClearTradeMessage(result);
    }

    public static string ClearTrade(ulong userID)
    {
        var result = TwitchBot<T>.Info.ClearTrade(userID);
        return GetClearTradeMessage(result);
    }

    private static string GetClearTradeMessage(QueueResultRemove result)
    {
        return result switch
        {
            QueueResultRemove.CurrentlyProcessing => "Looks like you're currently being processed! Did not remove from queue.",
            QueueResultRemove.CurrentlyProcessingRemoved => "Looks like you're currently being processed! Removed from queue.",
            QueueResultRemove.Removed => "Removed you from the queue.",
            _ => "Sorry, you are not currently in the queue.",
        };
    }

    public static string GetCode(ulong parse)
    {
        var detail = TwitchBot<T>.Info.GetDetail(parse);
        return detail == null
            ? "Sorry, you are not currently in the queue."
            : $"Your trade code is {detail.Trade.Code:0000 0000}";
    }

    public static string FormatMessages(string[] strings, string startSpacer)
    {
        int GetMaxCharacterWidth()
        {
            int maxCharWidth = 0;
            for (int i = 0; i < 0x110000; i++)
            {
                char ch = (char)i;
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category == UnicodeCategory.OtherLetter || category == UnicodeCategory.OtherSymbol)
                    maxCharWidth = Math.Max(maxCharWidth, CharWidth(ch));
            }
            return maxCharWidth;
        }

        int CharWidth(char ch)
        {
            string str = ch.ToString();
            return str.Length;
        }

        List<string> CalculateFillers(List<string> strings, int maxCharWidth)
        {
            List<string> fillers = [];
            foreach (string str in strings)
            {
                int requiredFillers = Math.Max(0, (30 - str.Length - maxCharWidth) / maxCharWidth);
                int SubtractedFillers = requiredFillers / 12;
                int totalFillers = requiredFillers - SubtractedFillers;
                fillers.Add(new string('⠀', totalFillers));
            }
            return fillers;
        }

        int maxCharWidth = GetMaxCharacterWidth();
        List<string> stringList = strings.ToList();
        List<string> fillers = CalculateFillers(stringList, maxCharWidth);
        StringBuilder formattedText = new(startSpacer + Environment.NewLine);

        for (int i = 0; i < stringList.Count; i++)
        {
            string str = stringList[i].Replace(" ", "⠀");
            string spacer = " ";
            string formattedLine = str + fillers[i] + spacer + Environment.NewLine;
            formattedText.Append(formattedLine);
        }

        return formattedText.ToString();
    }
}
