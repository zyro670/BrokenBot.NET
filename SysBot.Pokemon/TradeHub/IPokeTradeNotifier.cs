using PKHeX.Core;
using System;
using System.Collections.Generic;

namespace SysBot.Pokemon
{
    public interface IPokeTradeNotifier<T> where T : PKM, new()
    {
        /// <summary> Notifies when a trade bot is initializing at the start. </summary>
        void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info);
        /// <summary> Notifies when a trade bot is searching for the partner. </summary>
        void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info);
        /// <summary> Notifies when a trade bot notices the trade was canceled. </summary>
        void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg);
        /// <summary> Notifies when a trade bot finishes the trade. </summary>
        void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result);

        /// <summary> Sends a notification when called with parameters. </summary>
        void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message);
        /// <summary> Sends a notification when called with parameters. </summary>
        void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message);
        /// <summary> Sends a notification when called with parameters. </summary>
        void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message);

        /// <summary> Notifies when a trade bot is initializing at the start. </summary>
        Action<PokeRoutineExecutor<T>>? OnFinish { set; }

        //// <summary> Sends an embed with Discord interactions for PLA's Etumrep Dump. </summary>
        void SendEtumrepEmbed(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, IReadOnlyList<PA8> pkms);

        //// <summary> Sends an embed with Discord interactions for PLA's Etumrep Dump when criteria was not met. </summary>
        void SendIncompleteEtumrepEmbed(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string msg, IReadOnlyList<PA8> pkms);
    }
}