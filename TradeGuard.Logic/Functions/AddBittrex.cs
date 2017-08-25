using System.Threading.Tasks;
using TradeAssist.DocumentStorage;

namespace TradeGuard.Logic
{
    public static partial class Functions
    {
        public static async Task<bool> AddBittrex(string userId, string apiKey, string secret)
        {
            var client = DbClientFactory.Current.Get();
            var user = await client.UsersDynamic.Get(userId);

            user.Bittrex.ApiKey = apiKey;
            user.Bittrex.Secret = secret;

            var result = client.UsersDynamic.Update(userId, user);

            return result != null;
        }
    }
}