using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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

        public Public Public { get; }

        public ApiHttpClient()
        {
            BaseUrl = ApiUrlHttpsBase;
            Trading = new Trading(this);
            Wallet = new Wallet(this);
            Public = new Public(this);
        }

        public ApiHttpClient(Authenticator authenticator) : this()
        {
            this.authenticator = authenticator;
        }

        public async Task<T> GetData<T>(string command, string relativeUrl)
        {
            var url = $"{BaseUrl}{relativeUrl}?command={command}";
            var response = await innerClient.GetAsync(url);
            var responseObject = await DeserializeResponseJson<T>(response);
            return responseObject;
        }

        public async Task<T> PostData<T>(string command, string relativeUrl, Dictionary<string, string> postData)
        {
            if (authenticator == null) throw new Exception("Cannot use the private api without Authenticator set");
            
            // add command and nonce
            postData.Add("command", command);
            postData.Add("nonce", NonceCalculator.GetCurrentHttpPostNonce());

            var content = new FormUrlEncodedContent(postData);
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + relativeUrl)
            {
                Content = content,
            };

            // sign the content, using the credentials stored in the Authenticator
            var contentBytes = await content.ReadAsByteArrayAsync();
            authenticator.SignRequest(request, contentBytes);

            var response = await innerClient.SendAsync(request);
            var responseObject = await DeserializeResponseJson<T>(response);
            return responseObject;
        }

        async Task<T> DeserializeResponseJson<T>(HttpResponseMessage response)
        {
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
            catch (Exception ex)
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