using Crawler.Sample.Contracts;
using System;

namespace Crawler.Sample
{
    public class CrawlerBuilder
    {
        private IUrlsExtractor _urlsExtractor;
        private IUrlContentReader _urlContentReader;
        private IUrlContentStore _urlContentStore;
        private int _threadCount;

        public CrawlerBuilder SetupStore(IUrlContentStore urlContentStore)
        {
            _urlContentStore = urlContentStore;

            return this;
        }

        public CrawlerBuilder SetupUrlsExporter(IUrlsExtractor urlsExtractor)
        {
            _urlsExtractor = urlsExtractor;

            return this;
        }

        public CrawlerBuilder SetupUrlContentReader(IUrlContentReader urlContentReader)
        {
            _urlContentReader = urlContentReader;

            return this;
        }

        public CrawlerBuilder SetThreadCount(int threadCount)
        {
            _threadCount = threadCount;

            return this;
        }

        public CrawlerService Build()
        {
            if (_urlContentReader == null)
            {
                throw new Exception("Please setup UrlContentReader");
            }

            if (_urlsExtractor == null)
            {
                throw new Exception("Please setup UrlsExtractor");
            }

            if (_urlContentStore == null)
            {
                throw new Exception("Please setup StoreData");
            }

            var crawler = new CrawlerService(_urlContentReader, _urlsExtractor, _urlContentStore);

            crawler.ThreadCount = _threadCount;

            return crawler;
        }
    }
}
