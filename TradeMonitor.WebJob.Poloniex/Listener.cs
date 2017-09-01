using NPoloniex.API.Push;
using System;
using System.Threading.Tasks;
using static TradeAssist.Realtime.Constants;
using Doc = TradeMonitor.DocumentStorage;

namespace TradeMonitor.WebJob.Poloniex
{
    class Listener : IOnTradeAction
    {
        readonly Doc.Client docClient;

        public Listener()
        {
            docClient = Doc.Client.Current;
        }

        public async Task Start()
        {
            await docClient.Warmup();

            var client = new PoloniexWebSocketClient { OnTradeAction = this };
            await client.Connect();
            await client.SubscribeToTrades("BTC_ETH");
        }

        public async void OnTradeEvent(Trade t)
        {
            var output = $"Received: {t.CurrencyPair} {t.Quantity} units at {t.Price} [{t.TradeType}]\t\tuet: {t.UnixEpochTime}";
            Console.WriteLine(output);

            // core logic of how to handle trade goes here:
            bool wasThereAParseError = false;

            var trade = new Doc.Trade
            {
                Id = docClient.CreateNamespacedTradeId(t.TradeId),
                Exchange = "poloniex",
                Market = Canonical(t.CurrencyPair, PoloniexSeparator, useLowercaseInsteadOfUpper: true),
                Price = toDecimal(t.Price),
                Quantity = toDecimal(t.Quantity),
                Type = convertType(),
                Time = t.UnixEpochTime
            };

            if (wasThereAParseError)
            {
                Console.WriteLine($"PARSE ERROR: Unable to parse {output}");
                return;
            }

            try
            {
                var (isSaveSuccessful, response) = await docClient.SaveTrade(trade);
                if (isSaveSuccessful)
                {
                    Console.WriteLine($"Successfully saved document with id {trade.Id}");
                }
                else
                {
                    Console.WriteLine($"Unsuccessful save: {response}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex}");
            }

            decimal toDecimal(string val)
            {
                if (decimal.TryParse(val, out var result))
                    return result;
                else
                {
                    wasThereAParseError = true;
                    return -1;
                }
            }

            Doc.TradeType convertType()
            {
                if (t.TradeType == TradeType.Buy)
                {
                    return Doc.TradeType.Buy;
                }
                else if (t.TradeType == TradeType.Sell)
                {
                    return Doc.TradeType.Sell;
                }
                else
                {
                    wasThereAParseError = true;
                    return Doc.TradeType.Buy;
                }
            }
        }
    }
}