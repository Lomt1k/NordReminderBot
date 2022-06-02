using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NordDailyReminder.Networking
{
    public struct HttpParameter
    {
        public string name { get; }
        public string value { get; }

        public HttpParameter(string _name, string _value)
        {
            name = _name;
            value = _value;
        }
    }

    internal static class SimpleHttpClient
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<HttpResponseMessage> GetAsync(string request)
        {
            try
            {
                var response = await client.GetAsync(request);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"\nHttpRequestException: {e.Message} ");
                return null;
            }
        }

        public static async Task<HttpResponseMessage> GetAsync(string requestBody, IEnumerable<HttpParameter> parameters)
        {
            var sb = new StringBuilder(requestBody);
            bool isFirstParameter = true;
            foreach (var parameter in parameters)
            {
                char prefix = isFirstParameter ? '?' : '&';
                isFirstParameter = false;

                var encodedName = WebUtility.UrlEncode(parameter.name);
                var encodedValue = WebUtility.UrlEncode(parameter.value);
                sb.Append(prefix + encodedName + '=' + encodedValue);
            }
            return await GetAsync(sb.ToString());
        }

    }
}
