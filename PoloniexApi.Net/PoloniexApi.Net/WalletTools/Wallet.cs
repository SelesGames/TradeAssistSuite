using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public class Wallet : IWallet
    {
        private ApiWebClient ApiWebClient { get; set; }

        internal Wallet(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private async Task<IDictionary<string, Balance>> GetCompleteBalances()
        {
            var postData = new Dictionary<string, string>();

            var data = await PostData<IDictionary<string, Balance>>("returnCompleteBalances", postData);
            return data;
        }

        private async Task<IDictionary<string, string>> GetDepositAddresses()
        {
            var postData = new Dictionary<string, string>();

            var data = await PostData<IDictionary<string, string>>("returnDepositAddresses", postData);
            return data;
        }

        private async Task<IDepositWithdrawalList> GetDepositsAndWithdrawals(DateTime startTime, DateTime endTime)
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
        }

        public Task<IDictionary<string, Balance>> GetCompleteBalancesAsync()
        {
            return GetCompleteBalances();
        }

        public Task<IDictionary<string, string>> GetDepositAddressesAsync()
        {
            return GetDepositAddresses();
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync(DateTime startTime, DateTime endTime)
        {
            return GetDepositsAndWithdrawals(startTime, endTime);
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync()
        {
            return GetDepositsAndWithdrawals(Helper.DateTimeUnixEpochStart, DateTime.MaxValue);
        }

        public Task<IGeneratedDepositAddress> PostGenerateNewDepositAddressAsync(string currency)
        {
            return PostGenerateNewDepositAddress(currency);
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address, string paymentId)
        {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, paymentId));
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address)
        {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, null));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<T> PostData<T>(string command, Dictionary<string, string> postData)
        {
            return ApiWebClient.PostData<T>(command, postData);
        }
    }
}
