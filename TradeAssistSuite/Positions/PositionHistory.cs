using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssist.Suite
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
