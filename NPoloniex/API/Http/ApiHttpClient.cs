using Shared.Http;
using System;
using System.Collections.Generic;
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

        public async Task<T> GetData<T>(string command, string relativeUrl, string[] parameters = null)
        {
            var url = $"{BaseUrl}{relativeUrl}?command={command}";
            if (parameters != null)
                url = new StringBuilder(url).Append(AppendParameters(parameters)).ToString();

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
            var (deserializedResponse, error) = await response.DeserializeJsonResponseAs<T>();
            if (error.isError)
            {
                throw new HttpResponseDeserializationException(error.contentAsString, error.exception);
            }
            else
            {
                return deserializedResponse;
            }
        }

        private static string AppendParameters(string[] parameters)
        {
            if (parameters.Length != 0) {
                return "?" + string.Join("&", parameters);
            }

            return null;
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