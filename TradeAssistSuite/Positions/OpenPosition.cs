using NPoloniex.API.Http;
using System;

namespace TradeAssistSuite
{
    public class OpenPosition
    {
        public DateTime Began { get; set; }

        public Trade FirstTrade { get; set; }

        public decimal AveragePurchasePrice { get; set; }
        public decimal AverageSalePrice { get; set; }

        public decimal CurrentBalance { get; set; }
        public decimal Size { get; set; }

        /// <summary>
        /// Last price used as cost basis to calculate
        /// </summary>
        public decimal LastPrice { get; set; }
        public decimal RealizedProfit { get; set; }
        public decimal UnrealizedProfit { get; set; }
        public decimal CurrentTotalProfit { get; set; }

        // intended for updating values as realtime price updates to the ticker come in
        public void RecalculateProfitsFromCurrentPriceChange(decimal currentPrice)
        {
            var priceDifferential = currentPrice - AveragePurchasePrice;
            UnrealizedProfit = Math.Round(priceDifferential * CurrentBalance, 8);
            CurrentTotalProfit = UnrealizedProfit + RealizedProfit;
            LastPrice = currentPrice;
        }
    }
}