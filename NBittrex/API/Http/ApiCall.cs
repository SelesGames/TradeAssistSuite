using Shared.Http;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NBittrex.API.Http
{
    public class ApiCall
    {
        static HttpClient client = new HttpClient();

        readonly bool simulate;

        public ApiCall(bool simulate)
        {
            this.simulate = simulate;
        }

        public async Task<T> CallWithJsonResponse<T>(string uri, bool hasEffects, params Tuple<string, string>[] headers)
        {
            if (simulate && hasEffects)
            {
                Debug.WriteLine("(simulated)" + GetCallDetails());
                return default(T);
            }

            Debug.WriteLine(GetCallDetails());

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            foreach (var header in headers)
            {
                request.Headers.Add(header.Item1, header.Item2);
            }

            using (var response = await client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Error: StatusCode={response.StatusCode} Call Details={GetCallDetails()}");
                }

                else
                {
                    var (jsonResponse, error) = await response.DeserializeJsonResponseAs<ApiCallResponse<T>>();

                    if (error.isError)
                    {
                        throw new Exception($"{error.contentAsString} Call Details={GetCallDetails()} Exception={error.exception}");
                    }
                    else
                    {
                        if (bool.TryParse(jsonResponse.Success, out bool result) ? result : false)
                        {
                            return jsonResponse.Result;
                        }
                        else
                        {
                            throw new Exception($"{jsonResponse.Message} Call Details={GetCallDetails()}");
                        }
                    }
                }
            }

            string GetCallDetails()
            {
                StringBuilder sb = new StringBuilder();
                var u = new Uri(uri);
                sb.Append(u.AbsolutePath);
                if (u.Query.StartsWith("?"))
                {
                    var queryParameters = u.Query.Substring(1).Split('&');
                    foreach (var p in queryParameters)
                    {
                        if (!(p.ToLower().StartsWith("api") || p.ToLower().StartsWith("nonce")))
                        {
                            var kv = p.Split('=');
                            if (kv.Length == 2)
                            {
                                if (sb.Length != 0)
                                {
                                    sb.Append(", ");
                                }

                                sb.Append(kv[0]).Append(" = ").Append(kv[1]);
                            }
                        }
                    }
                }
                return sb.ToString();
            }
        }
    }
}