using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoneWorkerPoC
{
    public class HttpManager
    {
        public static async Task SendPostRequest(List<KeyValuePair<string, string>> value)
        {
            var url = "https://lone-worker-poc.herokuapp.com/test";

            Debug.WriteLine(value);

            var handler = new HttpClientHandler();
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(value) };
            if (handler.SupportsTransferEncodingChunked())
            {
                request.Headers.TransferEncodingChunked = true;
            }
            var response = await httpClient.SendAsync(request);
            Debug.WriteLine(response);
        }
    }
}
