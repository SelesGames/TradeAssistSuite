using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPoloniex.API.Push
{
    public static class Markets
    {
        static Dictionary<int, CurrencyPair> currencyPairs;
        //static DateTime lastRefreshed;
        static BootupStatus status = BootupStatus.None;
        static Task initializationTask;

        public static Dictionary<int, CurrencyPair> CurrencyPairs => GetCurrencyPairs();

        public static Task Initialize()
        {
            if (initializationTask == null)
                initializationTask = init();

            return initializationTask;

            async Task init()
            {
                status = BootupStatus.Initializing;
                currencyPairs = await MarketsHelper.GetMarkets();
                status = BootupStatus.Initialized;
            }
        }

        static Dictionary<int, CurrencyPair> GetCurrencyPairs()
        {
            if (status == BootupStatus.Initialized)
                return currencyPairs;
            else
                throw new UnauthorizedAccessException("Must call and await \"Markets.Initialize() before accessing CurrencyPairs");
        }
    }
}