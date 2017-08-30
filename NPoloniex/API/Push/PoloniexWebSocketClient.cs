using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NPoloniex.API.Push
{
    public class PoloniexWebSocketClient
    {
        static readonly Uri PoloniexWebSocketAddress = new Uri("wss://api2.poloniex.com:443");

        bool isConnected = false;
        bool isReceiving = false;

        ClientWebSocket client;

        public IOnTradeAction OnTradeAction { get; set; }

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
            var bytes = Encoding.Default.GetBytes(content);
            var buffer = new ArraySegment<byte>(bytes);
            return client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        const int BUFFER_SIZE = 16 * 1024; // from http://referencesource.microsoft.com/#System/net/System/Net/WebSockets/WebSocketHelpers.cs,285b8b64a4da6851
        readonly ArraySegment<byte> clientBuffer = ClientWebSocket.CreateClientBuffer(BUFFER_SIZE, BUFFER_SIZE);
        static readonly Encoding readEncoding = Encoding.Default;

        public async Task Connect()
        {
            if (isConnected)
                return;

            client = new ClientWebSocket();
            await client.ConnectAsync(PoloniexWebSocketAddress, CancellationToken.None);
            isConnected = true;

            BeginReceiveMessages();
        }

        // ^(\[187,.*) to only see order book for Gnosis
        // [187,10615808,[["o",1,"0.04490263","23.55422232"],["t","625586",0,"0.04490263","0.03820551",1503964002]]]
        async void BeginReceiveMessages()
        {
            isReceiving = true;

            while (isReceiving)
            {
                WebSocketReceiveResult receiveResult = null;

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        receiveResult = await client.ReceiveAsync(clientBuffer, CancellationToken.None);
                        ms.Write(clientBuffer.Array, clientBuffer.Offset, receiveResult.Count);
                    }
                    while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        JArray data = null;
                        Exception parseException = null;

                        ms.Seek(0, SeekOrigin.Begin);

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
                            var receivedString = readEncoding.GetString(clientBuffer.Array);
                            Console.WriteLine($"Exception parsing {receivedString}:\r\n{parseException.ToString()}");
                            continue;
                        }

                        ProcessDataArray(data);
                    }
                }



                /*var receiveResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                var messageLength = receiveResult.Count;

                JArray data = null;
                Exception parseException = null;

                try
                {
                    using (var ms = new MemoryStream(receiveBuffer, 0, messageLength, false))
                    using (var sr = new StreamReader(ms, readEncoding, false))
                    using (var reader = new JsonTextReader(sr))
                    {
                        data = JArray.Load(reader);
                    }
                }
                catch(Exception ex)
                {
                    parseException = ex;
                }

                if (parseException != null)
                {
                    var receivedString = readEncoding.GetString(receiveBuffer);
                    Console.WriteLine($"Exception parsing {receivedString}:\r\n{parseException.ToString()}");
                    continue;
                }
                
                ProcessDataArray(data);

                Array.Clear(receiveBuffer, 0, messageLength);

                */
            }

            void ProcessDataArray(JToken data)
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
                            //["o",1,"0.04490263","23.55422232"]

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

        void OnDisconnect()
        {
            isReceiving = false;
            isConnected = false;
        }
    }
}