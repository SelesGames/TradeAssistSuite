using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NPoloniex.API.Http;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeAssist.Realtime.SignalR
{
    class VolumeWeightedPrice
    {
        /*CurrencyPair currencyPair;
        Tick lastPoloniexTick;
        Tick lastBittrexTick;*/

        // Accomodate a price change
        public void Recalculate(Tick tick)
        {

        }
    }

    public class HistoricChartDataCollection
    {
        public static HistoricChartDataCollection Current { get; } = new HistoricChartDataCollection();

        Dictionary<string, HistoricChartData> collection = new Dictionary<string, HistoricChartData>();

        public HistoricChartData this[string key] => collection.TryGetValue(key, out var val) ? val : null;

        public List<string> MarketMonitorList { get; set; }

        public async void InvokeMonitor()
        {
            foreach (var market in MarketMonitorList)
            {
                var client = new ApiHttpClient();
                var oneYearAgo = DateTime.UtcNow - new TimeSpan(TimeSpan.TicksPerDay * 365);
                var chartData = await client.Public.GetChartData(market, NPoloniex.API.Http.CandlestickSize.Day, oneYearAgo, DateTime.UtcNow);
                var yearHigh = chartData.Max(o => o.High);
                var yearLow = chartData.Min(o => o.Low);
                var historicData = new HistoricChartData(market, yearHigh, yearLow);
                collection[market] = historicData;

                await Task.Delay(1000);
            }
        }
    }

    public class HistoricChartData
    {
        const decimal firstFibonacci = 0.618m;
        const decimal secondFibonacci = 0.500m;
        const decimal thirdFibonacci = 0.382m;
        const decimal fourthFibonacci = 0.236m;

        decimal diff;

        public string CurrencyPair { get; }
        public decimal YearHigh { get; }
        public decimal YearLow { get; }

        public FibonacciRetracementLevels FibonacciRetracementLevels { get; }

        public HistoricChartData(string currencyPair, decimal yearHigh, decimal yearLow)
        {
            CurrencyPair = currencyPair;
            YearHigh = yearHigh;
            YearLow = yearLow;

            diff = YearHigh - YearLow;

            FibonacciRetracementLevels = new FibonacciRetracementLevels
            {
                First = calculateFib(firstFibonacci),
                Second = calculateFib(secondFibonacci),
                Third = calculateFib(thirdFibonacci),
                Fourth = calculateFib(fourthFibonacci),
            };


            decimal calculateFib(decimal ratio)
            {
                var intermediate = diff * ratio;
                var fibLevel = YearLow + intermediate;
                return fibLevel;
            }
        }
    }

    public class FibonacciRetracementLevels
    {
        public decimal First { get; set; }
        public decimal Second { get; set; }
        public decimal Third { get; set; }
        public decimal Fourth { get; set; }
    }

    [HubName("price")]
    public class PriceHub : Hub
    {
        public HistoricChartData GetPriceInfo(string currencyPair)
        {
            return HistoricChartDataCollection.Current[currencyPair];
        }
    }
}