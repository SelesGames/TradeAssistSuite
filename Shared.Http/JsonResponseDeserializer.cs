using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shared.Http
{
    internal static class JsonResponseDeserializer
    {
        static readonly JsonSerializer jsonSerializer =
            new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        public static async Task<(T response, (bool isError, string contentAsString, Exception exception) error)> DeserializeJsonResponseAs<T>(this HttpResponseMessage response)
        {
            T responseObject = default(T);
            string contentAsString = null;
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
                contentAsString = await response.Content.ReadAsStringAsync();
            }

            return (responseObject, (deserializeException != null, contentAsString, deserializeException));
        }
    }
}