using Newtonsoft.Json;

namespace NPoloniex.API.Http
{
    public class Balance
    {
        [JsonProperty("available")]
        public decimal Available { get; private set; }

        [JsonProperty("onOrders")]
        public decimal OnOrders { get; private set; }

        [JsonProperty("btcValue")]
        public decimal BtcValue { get; private set; }
    }
}