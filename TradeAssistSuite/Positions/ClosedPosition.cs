using NPoloniex.API.Http;
using System;

namespace TradeAssistSuite
{
    public class ClosedPosition
    {
        public DateTime Began { get; set; }
        public DateTime Closed { get; set; }

        public Trade FirstTrade { get; set; }
        public Trade LastTrade { get; set; }

        public decimal AveragePurchasePrice { get; set; }
        public decimal AverageSalePrice { get; set; }

        public decimal Size { get; set; }

        public decimal Profit { get; set; }
    }
}
