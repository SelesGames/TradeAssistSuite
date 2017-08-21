using System.Collections.Generic;

namespace testAzureFunction
{
    public class TradeGuardSummary
    {
        public string Id { get; set; }
        public List<Holding> Holdings { get; } = new List<Holding>();
    }

    public class Holding
    {
        public string CurrencyPair { get; set; }
        public decimal TotalUnits { get; set; }
        public decimal TotalProtected { get; set; }
        public double TotalPercentProtected { get; set; }
        public List<HoldingBreakdown> Breakdown { get; } = new List<HoldingBreakdown>();
    }

    public class HoldingBreakdown
    {
        public string Exchange { get; set; }
        public double PercentOnExchange { get; set; }
        public decimal ExchangeUnits { get; set; }
        public decimal ExchangeProtected { get; set; }
        public double ExchangePercentProtected { get; set; }
    }
}