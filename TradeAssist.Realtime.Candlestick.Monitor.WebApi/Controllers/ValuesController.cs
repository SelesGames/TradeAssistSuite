using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TradeAssist.Realtime.Candlestick.Monitor.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UpdateTradesController : Controller
    {
        /*// GET api/updateTrades
        [HttpGet]
        public IEnumerable<string> Get(string exchange, string currencyPair)
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/updateTrades/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }*/

        // POST api/updateTrades
        [HttpPost]
        public async Task Post([FromBody]string exchange, [FromBody]string currencyPair)
        {
            var updatedTo = await GetUpdatedTo();

            var bittrexClient = NBittrex.API.Http.Exchange.CreatePublic();
            //await bittrexClient.GetOrderHistory()

        }

        Task<object> GetUpdatedTo()
        {
            return null;
        }

        /*// PUT api/updateTrades/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/updateTrades/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
