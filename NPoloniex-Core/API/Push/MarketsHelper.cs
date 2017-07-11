using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;

namespace NPoloniex.API.Push
{
    static class MarketsHelper
    {
        const string MARKETS_FILE_PATH = "markets.txt";

        public static async Task<Dictionary<int, CurrencyPair>> GetMarkets()
        {
            Dictionary<int, CurrencyPair> markets;

            //markets = await GetMarketsFromLocalStorage();
            //if (markets == null || !markets.Any())
                markets = await GetMarketsFromPoloniexHtml();

            return markets;
        }

        static async Task<Dictionary<int, CurrencyPair>> GetMarketsFromPoloniexHtml()
        {
            var url = "https://poloniex.com/";
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            /* httpClient.DefaultRequestHeaders
                 .UserAgent.ParseAdd(
                 "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/6.0;)");*/

            var pageSource = await httpClient.GetStringAsync("exchange");
            var beginSignalString = "var markets = ";
            var beginIndex = pageSource.IndexOf(beginSignalString) + beginSignalString.Length;
            var endIndex = pageSource.IndexOf(';', beginIndex);
            var length = endIndex - beginIndex;
            var substring = pageSource.Substring(beginIndex, length);

            var jObject = (JObject)JsonConvert.DeserializeObject(substring);

            var objList = jObject
                .Children()
                .Skip(1)
                .First()
                .Children()
                .SelectMany(o => o.Children())
                .SelectMany(o => o.Children())
                .Select(o => o.ToObject<MarketDto>())
                .ToDictionary(o => o.id, o => (CurrencyPair)o.currencyPair);

            Console.WriteLine(objList);

            //var fileDump = objList.Aggregate(new StringBuilder(), (o, s) => o.AppendLine(s)).ToString();
            //await WriteTextAsync(MARKETS_FILE_PATH, fileDump);

            return objList;
        }

        /*static async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();

                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }

        static async Task<List<string>> GetMarketsFromLocalStorage()
        {
            List<string> markets = null;

            if (File.Exists(MARKETS_FILE_PATH))
            {
                try
                {
                    var text = await ReadTextAsync(MARKETS_FILE_PATH);
                    markets = text.Split('\n').ToList();
                }
                catch { }
            }

            return markets ?? new List<string>();
        }

        static async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }*/
    }

    class MarketDto
    {
        public int id, baseId, quoteId;
        public string @base, quote, currencyPair;
    };
}
