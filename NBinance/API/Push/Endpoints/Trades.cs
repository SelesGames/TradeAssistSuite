using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NBinance.API.Push.Endpoints
{
    /// <summary>
    /// Realtime stream of trade events
    /// https://www.binance.com/restapipub.html#trades-wss-endpoint
    /// </summary>
    public class Trades
    {
        const string baseEndpoint = "wss://stream.binance.com:9443/ws/";

        public IDisposable Subscribe(string market, IOnDataHandler<Trade> onTradeEmitted)
        {
            var formattedMarket = market.ToLower();

            var indexOf = formattedMarket.IndexOfAny(new[] { '-', '_' });
            if (indexOf > -1)
            {
                formattedMarket = formattedMarket.Remove(indexOf, 1);
            }

            var client = new JsonWebSocketClient<Trade>($"{baseEndpoint}{formattedMarket}@aggTrade")
            {
                OnDataHandler = onTradeEmitted,
                ReconnectOnServerClose = true
            };

            client.Connect();
            return client;
        }
    }

    /* Example data:
    {
	    "e": "aggTrade",		// event type
	    "E": 1499405254326,		// event time
	    "s": "ETHBTC",			// symbol
	    "a": 70232,				// aggregated tradeid
	    "p": "0.10281118",		// price
	    "q": "8.15632997",		// quantity
	    "f": 77489,				// first breakdown trade id
	    "l": 77489,				// last breakdown trade id
	    "T": 1499405254324,		// trade time
	    "m": false,				// whether buyer is a maker
	    "M": true				// can be ignore
    } */
    public class Trade
    {
        [JsonProperty("e")] public string EventType { get; set; }
        [JsonProperty("E")] public long EventTime { get; set; }
        [JsonProperty("s")] public string Market { get; set; }
        [JsonProperty("a")] public long AggregatedTradeId { get; set; }
        [JsonProperty("p")] public string Price { get; set; }
        [JsonProperty("q")] public string Quantity { get; set; }
        [JsonProperty("f")] public long FirstBreakdownTradeId { get; set; }
        [JsonProperty("l")] public long LastBreakdownTradeId { get; set; }
        [JsonProperty("T")] public long TradeTime { get; set; }
        [JsonProperty("m")] public bool IsBuyerMaker { get; set; }
        [JsonProperty("M")] public bool MUnknown { get; set; }
    }
}
