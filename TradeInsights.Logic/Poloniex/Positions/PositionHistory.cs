using System.Collections.Generic;

namespace TradeInsights.Logic.Poloniex.Positions
{
    public class PositionHistory
    {
        public OpenPosition Open { get; set; }
        public List<ClosedPosition> Closed { get; } = new List<ClosedPosition>();

        public decimal OverallProfit { get; set; }
        // public Position BestTrade { get; set; }
        // public Position WorstTrade { get; set; }
    }
}
