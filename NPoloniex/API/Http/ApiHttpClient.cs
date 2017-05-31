using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NPoloniex.API.Http
{
    public class ApiHttpClient
    {
        private static HttpClient innerClient = new HttpClient();
        const string ApiUrlHttpsBase = "https://poloniex.com/";
        const string ApiUrlHttpsRelativePublic = "public?command=";
        const string ApiUrlHttpsRelativeTrading = "tradingApi";

        readonly Authenticator authenticator;

        public string BaseUrl { get; }

        public static readonly Encoding Encoding = Encoding.ASCII;
        static readonly JsonSerializer jsonSerializer = 
            new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        public Trading Trading { get; }
        public Wallet Wallet { get; }

        public ApiHttpClient(Authenticator authenticator)
        {
            BaseUrl = ApiUrlHttpsBase;
            this.authenticator = authenticator;
            Trading = new Trading(this);
            Wallet = new Http.Wallet(this);
        }

       /* public T GetData<T>(string command, params object[] parameters)
        {
            var relativeUrl = CreateRelativeUrl(command, parameters);

            var jsonString = QueryString(relativeUrl);
            var output = JsonSerializer.DeserializeObject<T>(jsonString);

            return output;
        }*/

        public async Task<T> PostData<T>(string command, string relativeUrl, Dictionary<string, string> postData)
        {
            // add command and nonce
            postData.Add("command", command);
            postData.Add("nonce", NonceCalculator.GetCurrentHttpPostNonce());

            var content = new FormUrlEncodedContent(postData);
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + relativeUrl)
            {
                Content = content,
            };

            var contentBytes = await content.ReadAsByteArrayAsync();

            authenticator.SignRequest(request, contentBytes);

            var response = await innerClient.SendAsync(request);

            T responseObject = default(T);
            Exception deserializeException = null;

            try
            {
                using (var s = await response.Content.ReadAsStreamAsync())
                using (var sr = new StreamReader(s))
                using (var jr = new JsonTextReader(sr))
                {
                    responseObject = jsonSerializer.Deserialize<T>(jr);
                }
            }
            catch(Exception ex)
            {
                deserializeException = ex;
            }

            if (deserializeException != null)
            {
                var stringContent = await response.Content.ReadAsStringAsync();
                throw new HttpResponseDeserializationException(stringContent, deserializeException);
            }

            return responseObject;
        }

        private static string CreateRelativeUrl(string command, object[] parameters)
        {
            var relativeUrl = command;
            if (parameters.Length != 0) {
                relativeUrl += "&" + string.Join("&", parameters);
            }

            return relativeUrl;
        }
    }

    public class HttpResponseDeserializationException : Exception
    {
        public string StringContent { get; }

        public HttpResponseDeserializationException(string stringContent, Exception innerException) 
            : base("Problem deserialzing the Http Response content.", innerException)
        {
            StringContent = stringContent;
        }
    }
}