using NPoloniex.API.Push;
using System;
using System.Threading.Tasks;

namespace TradeMonitor.WebJob.Poloniex
{
    class Listener : IOnTradeAction
    {
        public async Task Start()
        {
            var client = new PoloniexWebSocketClient { OnTradeAction = this };
            await client.Connect();

        }

        public void OnTradeEvent(Trade trade)
        {
            var output = $"Received: {trade.CurrencyPair} {trade.Quantity} units at {trade.Price} [{trade.TradeType}]\t\tuet: {trade.UnixEpochTime}";
            Console.WriteLine(output);

            // core logic of how to handle trade goes here:
        }
    }
}