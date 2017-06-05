using Newtonsoft.Json;

namespace NPoloniex.API.Http
{
    public class MarketData
    {
        [JsonProperty("last")]
        public decimal Last { get; internal set; }

        [JsonProperty("lowestAsk")]
        public decimal LowestAsk { get; internal set; }

        [JsonProperty("highestBid")]
        public decimal HighestBid { get; internal set; }

        [JsonProperty("percentChange")]
        public decimal PercentChange { get; internal set; }

        [JsonProperty("baseVolume")]
        public decimal BaseVolume { get; internal set; }

        [JsonProperty("quoteVolume")]
        public decimal Quotevolume { get; internal set; }

        //public double OrderSpread => (LowestAsk - HighestBid).Normalize();

        public decimal OrderSpreadPercentage =>  LowestAsk / HighestBid - 1;

        [JsonProperty("isFrozen")]
        internal byte IsFrozenInternal
        {
            set { IsFrozen = value != 0; }
        }
        public bool IsFrozen { get; private set; }
    }
}