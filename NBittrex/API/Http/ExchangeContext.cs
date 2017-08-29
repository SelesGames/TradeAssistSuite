namespace NBittrex.API.Http
{
    internal class ExchangeContext
    {
        public string ApiKey { get; set; }
        public string Secret { get; set; }
        public bool Simulate { get; set; }
    }
}