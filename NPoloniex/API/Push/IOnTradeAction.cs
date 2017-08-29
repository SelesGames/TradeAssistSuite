namespace NPoloniex.API.Push
{
    public interface IOnTradeAction
    {
        void OnPriceChange(Trade trade);
    }
}