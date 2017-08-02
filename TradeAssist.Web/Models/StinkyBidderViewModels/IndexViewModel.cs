using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeAssist.Web.Models.StinkyBidderViewModels
{
    public class IndexViewModel
    {
        public List<string> Markets { get; } = new List<string>();
        public string SelectedMarket { get; set; }
    }
}
