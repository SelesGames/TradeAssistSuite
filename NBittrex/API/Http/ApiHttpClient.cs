using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NBittrex.API.Http
{
    public class ApiHttpClient
    {
        private static HttpClient innerClient = new HttpClient();
        const string ApiUrlHttpsBase = "https://bittrex.com/api/v1.1/";
        const string ApiUrlHttpsRelativePublic = "public";
        const string ApiUrlHttpsRelativeTrading = "?";

        //readonly Authenticator authenticator;

        public string BaseUrl { get; }

        public static readonly Encoding Encoding = Encoding.ASCII;
        static readonly JsonSerializer jsonSerializer = 
            new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        public Public Public { get; }

        public ApiHttpClient()
        {
            BaseUrl = ApiUrlHttpsBase;
            Public = new Public(this);
        }

        public async Task<T> GetData<T>(string command, string relativeUrl, string[] parameters = null)
        {
            var url = $"{BaseUrl}{relativeUrl}/{command}";
            if (parameters != null)
                url = new StringBuilder(url).Append(AppendParameters(parameters)).ToString();

            var response = await innerClient.GetAsync(url);
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