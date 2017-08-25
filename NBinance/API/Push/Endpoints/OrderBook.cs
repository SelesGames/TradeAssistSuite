using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NBinance.API.Push.Endpoints
{
    /// <summary>
    /// Real-time stream of order book updates
    /// https://www.binance.com/restapipub.html#depth-wss-endpoint
    /// </summary>
    public class OrderBook
    {
        const string baseEndpoint = "wss://stream.binance.com:9443/ws/";

        public IDisposable Subscribe(string market, IOnDataHandler<OrderBookEvent> onOrderBookEventHandler)
        {
            var formattedMarket = market.ToLower();

            var indexOf = formattedMarket.IndexOfAny(new[] { '-', '_' });
            if (indexOf > -1)
            {
                formattedMarket = formattedMarket.Remove(indexOf, 1);
            }

            var translator = new Translator(onOrderBookEventHandler);

            var client = new JsonWebSocketClient<InternalOrderBookEvent>($"{baseEndpoint}{formattedMarket}@depth")
            {
                OnDataHandler = translator,
                ReconnectOnServerClose = true
            };

            client.Connect();
            return client;
        }

        class Translator : IOnDataHandler<InternalOrderBookEvent>
        {
            readonly IOnDataHandler<OrderBookEvent> proxy;

            public Translator(IOnDataHandler<OrderBookEvent> proxy)
            {
                this.proxy = proxy;
            }

            public void HandleData(InternalOrderBookEvent o)
            {
                var translated = new OrderBookEvent
                {
                    EventType = o.EventType,
                    EventTime = o.EventTime,
                    Market = o.Market,
                    UpdateId = o.UpdateId,
                    Bids = o.Bids.Select(PriceInfo).ToList(),
                    Asks = o.Asks.Select(PriceInfo).ToList()
                };

                proxy?.HandleData(translated);

                PriceInfo PriceInfo(object[] x)
                {
                    return new PriceInfo { Price = x[0].ToString(), Quantity = x[1].ToString() };
                }
            }
        }
    }

    /*
    {
	    "e": "depthUpdate",						// event type
	    "E": 1499404630606, 					// event time
	    "s": "ETHBTC", 							// symbol
	    "u": 7913455, 							// updateId to sync up with updateid in /api/v1/depth
	    "b": [									// bid depth delta
		    [
			    "0.10376590", 					// price (need to upate the quantity on this price)
			    "59.15767010", 					// quantity
			    []								// can be ignored
		    ],
	    ],
	    "a": [									// ask depth delta
		    [
			    "0.10376586", 					// price (need to upate the quantity on this price)
			    "159.15767010", 				// quantity
			    []								// can be ignored
		    ],
		    [
			    "0.10383109",
			    "345.86845230",
			    []
		    ],
		    [
			    "0.10490700",
			    "0.00000000", 					//quantitiy=0 means remove this level
			    []
		    ]
	    ] 
    }*/
    class InternalOrderBookEvent
    {
        [JsonProperty("e")] public string EventType { get; set; }
        [JsonProperty("E")] public long EventTime { get; set; }
        [JsonProperty("s")] public string Market { get; set; }
        [JsonProperty("u")] public long UpdateId { get; set; }
        [JsonProperty("b")] public List<object[]> Bids { get; set; }
        [JsonProperty("a")] public List<object[]> Asks { get; set; }
    }

    public class OrderBookEvent
    {
        public string EventType { get; set; }
        public long EventTime { get; set; }
        public string Market { get; set; }
        public long UpdateId { get; set; }
        public List<PriceInfo> Bids { get; set; }
        public List<PriceInfo> Asks { get; set; }
    }

    public class PriceInfo
    {
        public string Price { get; set; }
        public string Quantity { get; set; }
    }
}