using Crawler.Sample.Implements;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crawler.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Application Starting!");

            var client = new HttpClient();

            // use builder for create an object of CrawlerService
            var crawler = new CrawlerBuilder()
                .SetupStore(new DiskStoreUrlContent("c:\\crawlerFiles"))
                .SetupUrlContentReader(new UrlContentReaderByHttpClient(client))
                .SetupUrlsExporter(new UrlExtractorByRegex())
                .SetThreadCount(10)
                .Build();

            // add some event to print log on console
            crawler.UrlProcessSucceed += Crawler_UrlProcessSucceed;
            crawler.UrlProcessFailed += CrawlerUrlProcessFailed;

            await crawler.StartAsync("http://p30help.ir");

            // print final result
            Console.WriteLine($"Task finished - Result => Success: {crawler.SuccessCount} - Errors: {crawler.FailedCount} - Time: {crawler.TotalTime} sec");

            Console.ReadKey();
        }

        private static void CrawlerUrlProcessFailed(object o, string url, Exception exp)
        {
            Console.WriteLine($"Error for {url} - Message : {exp.Message}");
        }

        private static void Crawler_UrlProcessSucceed(object o, string url)
        {
            var crawler = (CrawlerService)o;

            Console.WriteLine($"{crawler.ProgressPercentage}% - {url}");
        }
    }
}
