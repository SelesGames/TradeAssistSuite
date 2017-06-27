using Newtonsoft.Json;

namespace TradeAssist.DocumentStorage
{
    public class StinkBid
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "currencyPair")]
        public string CurrencyPair { get; set; }

        [JsonProperty(PropertyName = "triggerPrice")]
        public decimal TriggerPrice { get; set; }

        [JsonProperty(PropertyName = "buyPrice")]
        public decimal BuyPrice { get; set; }

        [JsonProperty(PropertyName = "amountInBtc")]
        public decimal AmountInBtc { get; set; }

        [JsonProperty(PropertyName = "orderStatus")]
        public string OrderStatus { get; set; }
    }
}