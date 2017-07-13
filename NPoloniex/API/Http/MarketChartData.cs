using Newtonsoft.Json;
using System;
using static NPoloniex.API.Http.NonceCalculator;


namespace NPoloniex.API.Http
{
    public class MarketChartData
    {
        [JsonProperty("date")]
        private ulong DateInternal
        {
            set { Date = UnixTimeStampToDateTime(value); }
        }
        public DateTime Date { get; private set; }

        [JsonProperty("open")]
        public decimal Open { get; private set; }

        [JsonProperty("close")]
        public decimal Close { get; private set; }

        [JsonProperty("high")]
        public decimal High { get; private set; }

        [JsonProperty("low")]
        public decimal Low { get; private set; }

        [JsonProperty("volume")]
        public decimal Volume { get; private set; }

        [JsonProperty("quoteVolume")]
        public decimal QuoteVolume { get; private set; }

        [JsonProperty("weightedAverage")]
        public double WeightedAverage { get; private set; }
    }
}