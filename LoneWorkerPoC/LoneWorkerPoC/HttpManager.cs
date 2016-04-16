using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoneWorkerPoC
{
    public class HttpManager
    {
        private const string Url = "https://lone-worker-poc.herokuapp.com";

        public static async Task SendPostRequest(List<KeyValuePair<string, string>> value)
        {
            Debug.WriteLine(value);

            var handler = new HttpClientHandler();
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, Url + "/test") { Content = new FormUrlEncodedContent(value) };
            if (handler.SupportsTransferEncodingChunked())
            {
                request.Headers.TransferEncodingChunked = true;
            }
            var response = await httpClient.SendAsync(request);
            Debug.WriteLine(response);
        }

        public static async Task<string> SendGetRequest()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(Url + "/notif");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
