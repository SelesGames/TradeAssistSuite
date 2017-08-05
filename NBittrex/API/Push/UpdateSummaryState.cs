using System.Collections.Generic;

namespace NBittrex
{
    /*    {
      "MarketName": "ETH-RLC",
      "High": 0.002136,
      "Low": 0.00158949,
      "Volume": 87798.54800921,
      "Last": 0.00187165,
      "BaseVolume": 163.66531362,
      "TimeStamp": "2017-07-07T07:14:29.06",
      "Bid": 0.00181001,
      "Ask": 0.00188957,
      "OpenBuyOrders": 49,
      "OpenSellOrders": 118,
      "PrevDay": 0.00209301,
      "Created": "2017-04-30T18:41:22.823"
    },*/

    public class UpdateSummaryState
    {
        public long Nounce { get; set; }
        public List<Delta> Deltas { get; set; }
    }

    public class Delta
    {
        public string MarketName { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
        public decimal Last { get; set; }
        public decimal BaseVolume { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal PrevDay { get; set; }
    }
}