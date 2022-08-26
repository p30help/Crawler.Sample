using Crawler.Sample.Contracts;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crawler.Sample.Implements
{
    public class UrlContentReaderByHttpClient : IUrlContentReader
    {
        private readonly HttpClient _httpClient;

        public UrlContentReaderByHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UrlContentResult> ReadAsync(string url)
        {
            try
            {
                var res = await _httpClient.GetAsync(url);
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"response code is {res.StatusCode}");
                }

                var bytes = await res.Content.ReadAsByteArrayAsync();

                return new UrlContentResult()
                {
                    ContentType = res.Content?.Headers?.ContentType?.MediaType,
                    Content = bytes,
                    Url = url
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Http Error on {url} => {e.Message}", e);
            }
        }
    }
}
