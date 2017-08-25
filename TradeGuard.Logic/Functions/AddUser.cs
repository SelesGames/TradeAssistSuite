using System;
using System.Threading.Tasks;
using TradeAssist.DocumentStorage;

namespace TradeGuard.Logic
{
    public static partial class Functions
    {
        public static async Task<(string userId, dynamic error)> AddUser(string userId = null)
        {
            userId = userId ?? Guid.NewGuid().ToString();

            var user = new TradeAssistUser
            {
                id = userId,
            };

            var client = DbClientFactory.Current.Get();
            var result = await client.UsersDynamic.Create(userId);
            return (userId, null);
        }
    }
}
