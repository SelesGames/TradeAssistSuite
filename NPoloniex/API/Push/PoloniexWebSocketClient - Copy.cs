using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NPoloniex.API.Push
{
    public class PoloniexWebSocketClient2 : IDisposable, IWebSocketMessageHandler
    {
        const string PoloniexWebSocketAddress = "wss://api2.poloniex.com:443";
        readonly BaseWebSocketClient client;

        public PoloniexWebSocketClient2()
        {
            client = new BaseWebSocketClient(PoloniexWebSocketAddress)
            {
                ReconnectOnServerClose = true,
                TextMessageHandler = this
            };
        }

        public IOnTradeAction OnTradeAction { get; set; }
        public IOnOrderAction OnOrderAction { get; set; }

        public async Task SubscribeToTicker()
        {
            // ensure that the markets list is initialized, 
            // as it will have the mapping between ids and currency pairs
            await Markets.Initialize();

            var stringRep = JsonConvert.SerializeObject(new { command = "subscribe", channel = 1002 });
            await SendString(stringRep);
        }

        public async Task SubscribeToTradesForAllCurrencies()
        {
            await Markets.Initialize();

            foreach (var market in Markets.CurrencyPairs)
            {
                var intKey = market.Key;
                var currencyPair = market.Value;
                var poloniexFormatCurrencyPair = $"{currencyPair.ToString()}";

                await SubscribeToTrades(poloniexFormatCurrencyPair);
            }
        }

        public async Task SubscribeToTrades(CurrencyPair currencyPair)
        {
            await Markets.Initialize();

            var stringRep = JsonConvert.SerializeObject(new { command = "subscribe", channel = currencyPair.ToString() });
            await SendString(stringRep);

            Console.WriteLine($"subscribed to order book updates for {currencyPair}");
        }

        Task SendString(string content)
        {
            return client.SendString(content);
        }

        public Task Connect()
        {
            return client.Connect();
        }


        // ^(\[187,.*) to only see order book for Gnosis
        // [187,10615808,[["o",1,"0.04490263","23.55422232"],["t","625586",0,"0.04490263","0.03820551",1503964002]]]
        public void OnMessageReceived(MemoryStream ms)
        {
            JArray data = null;
            Exception parseException = null;
            var readEncoding = client.Encoding;

            try
            {
                using (var sr = new StreamReader(ms, readEncoding, false))
                using (var reader = new JsonTextReader(sr))
                {
                    data = JArray.Load(reader);
                }
            }
            catch (Exception ex)
            {
                parseException = ex;
            }

            if (parseException != null)
            {
                var receivedString = readEncoding.GetString(ms.GetBuffer());
                Console.WriteLine($"Exception parsing {receivedString}:\r\n{parseException.ToString()}");
                return;
            }

            ProcessDataArray();

            void ProcessDataArray()
            {
                int messageType = data[0].ToObject<int>();

                switch (messageType)
                {
                    case 1010:
                        OnHeartBeat();
                        return;
                    case 1002:
                        if (data[1].ToObject<int?>() != 1)
                            OnTicker(data[2]);
                        return;
                    default:
                        break;
                }

                if (Markets.CurrencyPairs.TryGetValue(messageType, out var matchingCurrency))
                {
                    // [187,10615808,[["o",1,"0.04490263","23.55422232"],["t","625586",0,"0.04490263","0.03820551",1503964002]]]
                    int orderId = data[1].ToObject<int>();
                    var orderArray = data[2].ToArray();
                    foreach (var orderEntryArray in orderArray)
                    {
                        var orderType = orderEntryArray[0].ToObject<string>();
                        if (orderType == "t")
                        {
                            //["t","625586",0,"0.04490263","0.03820551",1503964002]
                            var trade = new Trade
                            {
                                CurrencyPair = matchingCurrency,
                                TradeId = orderEntryArray[1].ToObject<string>(),
                                TradeType = (TradeType)orderEntryArray[2].ToObject<int>(), // buy or sell
                                Price = orderEntryArray[3].ToObject<string>(),
                                Quantity = orderEntryArray[4].ToObject<string>(),
                                UnixEpochTime = orderEntryArray[5].ToObject<long>(),
                            };
                            OnTradeAction?.OnTradeEvent(trade);
                        }
                        else if (orderType == "o")
                        {
                            //["o",1,"0.04490263","23.55422232"]  1 = buy order, 0 = sell order
                            var order = new Order
                            {
                                CurrencyPair = matchingCurrency,
                                OrderType = (OrderType)orderEntryArray[1].ToObject<int>(), // bid or ask
                                Rate = orderEntryArray[2].ToObject<string>(),
                                Amount = orderEntryArray[3].ToObject<string>(),
                            };
                            OnOrderAction?.OnOrderEvent(order);
                        }
                    }
                }
            }
        }

        void OnTicker(JToken tickerInfo)
        {
            var currencyPairId = tickerInfo[0].ToObject<int>();
            var currencyPair = Markets.CurrencyPairs[currencyPairId];

            var tick = new Tick
            {
                CurrencyPair = currencyPair,
                Last = tickerInfo[1].ToObject<decimal>(),
                LowestAsk = tickerInfo[2].ToObject<decimal>(),
                HighestBid = tickerInfo[3].ToObject<decimal>(),
                PercentChange = tickerInfo[4].ToObject<decimal>(),
                BaseVolume = tickerInfo[5].ToObject<decimal>(),
                QuoteVolume = tickerInfo[6].ToObject<decimal>(),
                IsFrozen = tickerInfo[7].ToObject<bool>(),
                High24Hour = tickerInfo[8].ToObject<decimal>(),
                Low24Hour = tickerInfo[9].ToObject<decimal>(),
            };

            Ticker.Current.Update(tick);
        }

        void OnHeartBeat()
        {

        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}