using System.Collections.Generic;

namespace TradeAssist.Web.Models.StinkyBidderViewModels
{
    public class OrderEntryViewModel
    {
        public string CurrencyPair { get; set; }

        public decimal Price { get; set; }

        public decimal AmountInBtc { get; set; }

        public bool IsSilent { get; set; }

        public decimal AveragePriceAcrossExchanges { get; set; }

        public List<decimal> RecommendedBuyAmountBasedOnUserCurrentTotalBalance { get; set; }

        public bool ShowFirstFibonacciRetracementLevel { get; set; }

        public decimal FirstFibonacciRetracementLevel { get; set; }

        public bool ShowSecondFibonacciRetracementLevel { get; set; }

        public decimal SecondFibonacciRetracementLevel { get; set; }

        public bool ShowThirdFibonacciRetracementLevel { get; set; }

        public decimal ThirdFibonacciRetracementLevel { get; set; }

        public bool ShowFourthFibonacciRetracementLevel { get; set; }

        public decimal FourthFibonacciRetracementLevel { get; set; }
    }
}