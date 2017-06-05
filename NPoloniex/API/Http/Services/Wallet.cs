using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NPoloniex.API.Http
{
    public class Wallet
    {
        const string ApiUrlHttpsRelativeTrading = "tradingApi";

        private ApiHttpClient ApiHttpClient { get; }

        internal Wallet(ApiHttpClient apiWebClient)
        {
            ApiHttpClient = apiWebClient;
        }

        public Task<IDictionary<string, Balance>> GetCompleteBalances()
        {
            var postData = new Dictionary<string, string>();
            return ApiHttpClient.PostData<IDictionary<string, Balance>>("returnCompleteBalances", ApiUrlHttpsRelativeTrading, postData);
        }

        public Task<IDictionary<string, string>> GetDepositAddresses()
        {
            var postData = new Dictionary<string, string>();
            return ApiHttpClient.PostData<IDictionary<string, string>>("returnDepositAddresses", ApiUrlHttpsRelativeTrading, postData);
        }

       /* private async Task<IDepositWithdrawalList> GetDepositsAndWithdrawals(DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, string> {
                { "start", Helper.DateTimeToUnixTimeStamp(startTime).ToString() },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime).ToString() }
            };

            var data = await PostData<DepositWithdrawalList>("returnDepositsWithdrawals", postData);
            return data;
        }

        private async Task<IGeneratedDepositAddress> PostGenerateNewDepositAddress(string currency)
        {
            var postData = new Dictionary<string, string> {
                { "currency", currency }
            };

            var data = await PostData<IGeneratedDepositAddress>("generateNewAddress", postData);
            return data;
        }

        private void PostWithdrawal(string currency, double amount, string address, string paymentId)
        {
            var postData = new Dictionary<string, string> {
                { "currency", currency },
                { "amount", amount.ToStringNormalized() },
                { "address", address }
            };

            if (paymentId != null) {
                postData.Add("paymentId", paymentId);
            }

            PostData<IGeneratedDepositAddress>("withdraw", postData);
        }*/
    }
}
