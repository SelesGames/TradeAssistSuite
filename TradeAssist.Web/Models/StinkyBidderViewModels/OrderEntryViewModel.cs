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
    }
}