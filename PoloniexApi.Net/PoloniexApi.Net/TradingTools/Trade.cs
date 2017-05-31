using Newtonsoft.Json;
using System;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public class Trade// : Order, ITrade
    {
/* returned fields:          
        [
          {
            "globalTradeID": 25129732,
            "tradeID": "6325758",
            "date": "2016-04-05 08:08:40",
            "rate": "0.02565498",
            "amount": "0.10000000",
            "total": "0.00256549",
            "fee": "0.00200000",
            "orderNumber": "34225313575",
            "type": "sell",
            "category": "exchange"
          }
        ]
*/

        [JsonProperty("globalTradeID")]
        public int GlobalTradeId { get; set; }

        [JsonProperty("tradeID")]
        public string TradeId { get; set; }

        [JsonProperty("date")]
        private string DateInternal
        {
            set { Date = Helper.ParseDateTime(value); }
        }
        public DateTime Date { get; private set; }

        [JsonProperty("rate")]
        public decimal Rate { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("type")]
        private string TypeInternal
        {
            get { return OrderTypeHelpers.ToString(Type); }
            set { Type = OrderTypeHelpers.ToOrderType(value); }
        }
        public OrderType Type { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}
