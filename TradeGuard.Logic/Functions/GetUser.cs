using System.Threading.Tasks;

namespace TradeGuard.Logic.Functions
{
    public static partial class Func
    {
        static async Task<dynamic> GetUser(string userId)
        {
            await Task.Delay(0);
            return new
            {
                BittrexApiKey = "",
                Secret = "",
            };
        }
    }
}
