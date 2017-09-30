namespace NPoloniex.API.Push
{
    public class Order
    {
        public string CurrencyPair { get; set; }
        public OrderType OrderType { get; set; }
        public string Rate { get; set; }
        public string Amount { get; set; }
    }

    public enum OrderType
    {
        Ask = 0,
        Bid = 1
    }
}