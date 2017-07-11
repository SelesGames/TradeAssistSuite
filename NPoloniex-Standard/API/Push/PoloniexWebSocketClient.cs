using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public async Task SubscribeToTicker()
        {
            // ensure that the markets list is initialized, 
            // as it will have the mapping between ids and currency pairs
            await Markets.Initialize();

            var stringRep = JsonConvert.SerializeObject(new { command = "subscribe", channel = 1002 });
            var bytes = Encoding.Default.GetBytes(stringRep);
            var buffer = new ArraySegment<byte>(bytes);
            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }



        const int BUFFER_SIZE = 1024;
        readonly byte[] receiveBuffer = new byte[BUFFER_SIZE];
        static readonly Encoding readEncoding = Encoding.Default;

        async void BeginReceiveMessages()
        {
            isReceiving = true;

            while (isReceiving)
            {
                var receiveResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                var messageLength = receiveResult.Count;

                JArray data = null;

                using (var ms = new MemoryStream(receiveBuffer, 0, messageLength, false))
                using (var sr = new StreamReader(ms, readEncoding, false))
                using (var reader = new JsonTextReader(sr))
                {
                    data = JArray.Load(reader);
                }

                //var deserialized = readEncoding.GetString(receiveBuffer, 0, messageLength);

                //var data = JArray.Parse(deserialized);
                int messageType = data[0].ToObject<int>();

                /*var message = new Message()
                {
                    MessageId = data[0].ToObject<int>(),
                    MessageVal = data[1].ToObject<int?>(),
                    Payload = new Payload()
                    {
                        PayloadId = data[2][0].ToObject<int>(),
                        Items = data[2].Skip(1).Select(x => x.ToString()).ToArray()
                    }
                };*/

                //var jsonArray = JsonConvert.DeserializeObject<List<dynamic>>(deserialized);

                //long messageType = jsonArray[0];

                switch (messageType)
                {
                    case 1010:
                        OnHeartBeat();
                        break;
                    case 1002:
                        //if (jsonArray[1] != 1)
                        //    OnTicker(jsonArray[2]);
                        if (data[1].ToObject<int?>() != 1)
                            OnTicker(data[2]);
                        break;
                    default:
                        break;
                }

                Array.Clear(receiveBuffer, 0, messageLength);

                //Console.WriteLine(deserialized);
            }
        }

        /*void OnTicker(dynamic tickerInfo)
        {
            Console.WriteLine(tickerInfo);
        }*/

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

        /*IEnumerable<object> ReadMessage()
        {
            using (var ms = new StreamReader(new MemoryStream(receiveBuffer)))
            using (JsonTextReader reader = new JsonTextReader(ms))
            {
                var first = reader.Value;
                while (reader.Read())
                {
                    yield return reader.Value;
                }
            }

            return new List<object>();
        }

        dynamic ReadMessage2()
        {
            using (var ms = new StreamReader(new MemoryStream(receiveBuffer)))
            using (JsonTextReader reader = new JsonTextReader(ms))
            {
                reader.Read();
                var messageId = reader.ReadAsInt32();
                var messageVal = reader.ReadAsInt32();
                var currencyId = reader.ReadAsInt32();

                return new { messageId, messageVal, currencyId };
            }
        }*/

        void OnHeartBeat()
        {

        }

        void OnDisconnect()
        {
            isReceiving = false;
            isConnected = false;
        }

        public async Task Connect()
        {
            if (isConnected)
                return;

            client = new ClientWebSocket();
            await client.ConnectAsync(PoloniexWebSocketAddress, CancellationToken.None);
            isConnected = true;

            BeginReceiveMessages();
        }
    }
}
