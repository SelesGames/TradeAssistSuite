namespace NBittrex.API.Http
{
    public enum OrderBookType
    {
        Buy,
        Sell,
        Both
    }

    public enum FillType
    {
        Fill,
        Partial_Fill
    }

    public enum OrderType
    {
        Buy,
        Sell
    }

    public enum OpenOrderType
    {
        Limit_Buy,
        Limit_Sell
    }
}