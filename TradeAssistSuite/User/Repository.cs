using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeAssist.DocumentStorage;

namespace TradeAssistSuite.User
{
    class Repository
    {
        const string endpoint = "https://localhost:8081/";
        const string authKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        readonly TADocumentClient client;

        public Repository()
        {
            DbClientFactory.Current.SetCredentials(endpoint, authKey);
            client = DbClientFactory.Current.Get();
        }

        public async Task<string> CreateUser()
        {
            var id = BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0).ToString();
            var result = await client.Users.Create(
                new TradeAssistUser
            {
                id = id,
            });

            return result.Id;
        }

        public async Task<TradeAssistUser> GetUser(string userId)
        {
            var user = await client.Users.Get(userId);
            return user;
        }

        public async Task<bool> AddBittrex(string apiKey, string secret)
        {
            var userId = Environment.Current.UserId;

            var user = await client.UsersDynamic.Get(userId);

            user.Bittrex = new { ApiKey = apiKey, Secret = secret };
            //user.Bittrex.ApiKey = apiKey;
            //user.Bittrex.Secret = secret;

            var result = await client.UsersDynamic.Update(userId, user);

            return result != null;
        }
    }
}
