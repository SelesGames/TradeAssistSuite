namespace TradeAssist.Realtime.Ticker
{
    public interface IOnPriceChangeAction
    {
        void OnPriceChange(PriceChange pc);
    }
}
