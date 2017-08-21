using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBittrex
{
    /*
    {
      "MarketName": "BTC-ANS",
      "Nounce": 108653,
      "Buys": [
        {
          "Type": 2,
          "Rate": 0.0038,
          "Quantity": 2072.62484431
        }
      ],
      "Sells": [
        {
          "Type": 0,
          "Rate": 0.00387,
          "Quantity": 216.5228125
        }
      ],
      "Fills": [
        {
          "OrderType": "BUY",
          "Rate": 0.00387,
          "Quantity": 82.13456691,
          "TimeStamp": "2017-08-04T00:27:31.293"
        }
       ]
    }*/

    public class UpdateExchangeState
    {
        public string Marketname { get; set; }
        public long Nounce { get; set; }
        public List<Transaction> Buys { get; set; }
        public List<Transaction> Sells { get; set; }
        public List<Fill> Fills { get; set; }
    }

    public class Transaction
    {
        public int Type { get; set; }
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Fill
    {
        public string OrderType { get; set; }
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
