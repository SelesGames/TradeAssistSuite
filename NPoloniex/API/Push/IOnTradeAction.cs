namespace NPoloniex.API.Push
{
    public interface IOnTradeAction
    {
        void OnTradeEvent(Trade trade);
    }
}