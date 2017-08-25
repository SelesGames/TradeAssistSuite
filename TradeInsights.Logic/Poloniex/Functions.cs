using NPoloniex.API;
using NPoloniex.API.Http;
using System;
using System.Threading.Tasks;
using TradeInsights.Logic.Poloniex.Positions;

namespace TradeInsights.Logic.Poloniex
{
    public static class Functions
    {
        public static async Task<PositionHistory> GetPositionHistory(string currency)
        {
            currency = currency.ToUpper();

            CurrencyPair currencyPair;

            if (CurrencyPair.IsCurrencyPair(currency))
                currencyPair = currency;
            else
                currencyPair = CurrencyPair.PrependBitcoin(currency);


            var authenticator = new Authenticator(
                "F5QR8MJE-HN5LH4WJ-8X9758YH-NDLRE7NJ",
                "0be35048de6102dfa9927504b4099aac222636f2dd96983f9713fe0c9b93d489f38ae08d9e3e3b4b3509ef77c182f9000a4b8b21c49d8af84ad0863c6937f932");
            var client = new ApiHttpClient(authenticator);

            var trades = await client.Trading.GetTrades(currencyPair, DateTime.Parse("1/1/2008"), DateTime.UtcNow);

            var balances = await client.Wallet.GetCompleteBalances();
            //var currentTick = Ticker.Current[currencyPair];
            //var currentPrice = currentTick.Last;

            var balance = balances[currencyPair.QuoteCurrency];
            var balanceSize = balance.Available + balance.OnOrders;
            var currentValue = balance.BtcValue;

            var calculatedCurrentPrice = balance.BtcValue / balanceSize;
            var roundedCalculatedCurrentPrice = Math.Round(calculatedCurrentPrice, 8);

            var positionHistory = PositionCalculator.ExtractPositions(balanceSize, roundedCalculatedCurrentPrice, trades);

            return positionHistory;
        }
    }
}