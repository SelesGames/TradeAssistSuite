using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI
{
    sealed class ApiWebClient
    {
        private static HttpClient innerClient = new HttpClient();

        public string BaseUrl { get; private set; }

        private Authenticator _authenticator;
        public Authenticator Authenticator {
            private get { return _authenticator; }

            set {
                _authenticator = value;
                Encryptor.Key = Encoding.GetBytes(value.PrivateKey);
            }
        }

        private HMACSHA512 _encryptor = new HMACSHA512();
        public HMACSHA512 Encryptor {
            private get { return _encryptor; }
            set { _encryptor = value; }
        }

        public static readonly Encoding Encoding = Encoding.ASCII;
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        public ApiWebClient(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public T GetData<T>(string command, params object[] parameters)
        {
            var relativeUrl = CreateRelativeUrl(command, parameters);

            var jsonString = QueryString(relativeUrl);
            var output = JsonSerializer.DeserializeObject<T>(jsonString);

            return output;
        }

        public async Task<T> PostData<T>(string command, Dictionary<string, string> postData)
        {
            postData.Add("command", command);
            postData.Add("nonce", Helper.GetCurrentHttpPostNonce());

            var jsonString = await PostFormData(Helper.ApiUrlHttpsRelativeTrading, postData);// postData.ToHttpPostString());
            var output = JsonSerializer.DeserializeObject<T>(jsonString);

            return output;
        }

        private string QueryString(string relativeUrl)
        {
            var request = CreateHttpWebRequest("GET", relativeUrl);

            return request.GetResponseString();
        }

        private async Task<string> PostFormData(string relativeUrl, Dictionary<string, string> postData)//string postData)
        {
            var content = new FormUrlEncodedContent(postData);
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + relativeUrl)
            {
                Content = content,
            };
            request.Headers.Add("Key", Authenticator.PublicKey);

            var signedContent = Encryptor.ComputeHash(await content.ReadAsByteArrayAsync());
            var signedContentHex = signedContent.ToStringHex();

            request.Headers.Add("Sign", signedContentHex);

            var response = await innerClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;


            /*client.PostAsync()
            var request = CreateHttpWebRequest("POST", BaseUrl + relativeUrl);
            request.ContentType = "application/x-www-form-urlencoded";*/

            //var postBytes = Encoding.GetBytes(postData);
            //request.ContentLength = postBytes.Length;

            //request.Headers["Key"] = Authenticator.PublicKey;
            //request.Headers["Sign"] = Encryptor.ComputeHash(postBytes).ToStringHex();

            /*using (var requestStream = request.GetRequestStream()) {
                requestStream.Write(postBytes, 0, postBytes.Length);
            }*/

            //return request.GetResponseString();
        }

        private static string CreateRelativeUrl(string command, object[] parameters)
        {
            var relativeUrl = command;
            if (parameters.Length != 0) {
                relativeUrl += "&" + string.Join("&", parameters);
            }

            return relativeUrl;
        }

        private HttpWebRequest CreateHttpWebRequest(string method, string relativeUrl)
        {
            var request = WebRequest.CreateHttp(BaseUrl + relativeUrl);
            request.Method = method;
            request.UserAgent = "Poloniex API .NET v" + Helper.AssemblyVersionString;

            request.Timeout = Timeout.Infinite;

            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            return request;
        }
    }
}
