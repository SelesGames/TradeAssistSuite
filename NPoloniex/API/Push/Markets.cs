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
        static readonly TaskCompletionSource<int> initializationTask = new TaskCompletionSource<int>();

        public static Dictionary<int, CurrencyPair> CurrencyPairs => GetCurrencyPairs();

        public static Task Initialize()
        {
            if (status == BootupStatus.None)
            {
                status = BootupStatus.Initializing;
                StartInitialization();
            }

            return initializationTask.Task;

            /*async void init()
            {
                initializationTask = new TaskCompletionSource<int>();
                status = BootupStatus.Initializing;
                currencyPairs = await MarketsHelper.GetMarkets();
                status = BootupStatus.Initialized;
                initializationTask.TrySetResult(0);
            }*/
        }

        static async void StartInitialization()
        {
            status = BootupStatus.Initializing;
            currencyPairs = await MarketsHelper.GetMarkets();
            status = BootupStatus.Initialized;
            initializationTask.TrySetResult(0);
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