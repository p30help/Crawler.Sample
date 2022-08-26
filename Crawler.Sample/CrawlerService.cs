using Crawler.Sample.Contracts;
using Crawler.Sample.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.Sample
{
    public class CrawlerService : ICrawler
    {
        /// <summary>
        /// the website root url
        /// </summary>
        public string RootUrl { get; private set; }

        public int ProgressPercentage
        {
            get
            {
                var percentage = (UrlsSucceed.Count + UrlsFailed.Count) / Convert.ToDecimal(UrlsQueue.InputsCount) * 100;
                return Convert.ToInt32(Math.Floor(percentage));
            }
        }

        public bool IsAllUrlProcessed
        {
            get
            {
                return UrlsQueue.InputsCount == UrlsSucceed.Count + UrlsFailed.Count;
            }
        }

        public int TotalCount
        {
            get
            {
                return UrlsQueue.InputsCount;
            }
        }

        public int SuccessCount
        {
            get
            {
                return UrlsSucceed.Count;
            }
        }

        public int FailedCount
        {
            get
            {
                return UrlsFailed.Count;
            }
        }

        /// <summary>
        /// total time spent for process of all urls
        /// </summary>
        public int TotalTime
        {
            get
            {
                return Convert.ToInt32(stopwatch.Elapsed.TotalSeconds);
            }
        }

        /// <summary>
        /// thread count that will process all of urls
        /// </summary>
        public int ThreadCount { get; set; } = 5;

        readonly UniqueUrlQueue UrlsQueue = new UniqueUrlQueue();

        /// <summary>
        /// if each url processed successfully add to this dictionary
        /// </summary>
        readonly ConcurrentDictionary<string, int> UrlsSucceed = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// if each url processed not successfully add to this dictionary
        /// </summary>
        readonly ConcurrentDictionary<string, int> UrlsFailed = new ConcurrentDictionary<string, int>();

        private readonly Stopwatch stopwatch = new Stopwatch();

        private readonly IUrlsExtractor _urlsExtractor;
        private readonly IUrlContentReader _urlContentReader;
        private readonly IUrlContentStore _urlContentStore;

        public CrawlerService(IUrlContentReader urlContentReader, IUrlsExtractor urlsExtractor,
            IUrlContentStore urlContentStore)
        {
            _urlsExtractor = urlsExtractor;
            _urlContentReader = urlContentReader;
            _urlContentStore = urlContentStore;
        }

        public Task StartAsync(string startUrl)
        {
            return StartAsync(startUrl, CancellationToken.None);
        }

        public async Task StartAsync(string startUrl, CancellationToken cancellationToken)
        {
            // clear old data before start new
            resetData();

            RootUrl = startUrl.ToLower();
            tryAddUrlToQueue(startUrl);

            try
            {
                // start timer
                stopwatch.Restart();

                var tasks = new List<Task>();
                for (int i = 0; i < ThreadCount; i++)
                {
                    tasks.Add(Task.Run(() => readAndProcessQueue(cancellationToken), cancellationToken));
                }

                // stop here until all of tasks become completed
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                // stop timer
                stopwatch.Stop();
            }
        }

        #region Private Methods

        private async Task readAndProcessQueue(CancellationToken cancellationToken)
        {
            try
            {
                // read url from queue
                while (UrlsQueue.TryDequeue(out var url) || !IsAllUrlProcessed)
                {
                    // check cancellation token
                    cancellationToken.ThrowIfCancellationRequested();

                    if (url == null)
                    {
                        // if url was null wait awhile
                        await Task.Delay(500);
                        continue;
                    }

                    try
                    {
                        // trigger process starting event
                        onUrlProcessing(url);

                        // start process url (read, render, save)
                        await processUrl(url);

                        // trigger success event
                        onUrlProcessSucceed(url);
                    }
                    catch (Exception e)
                    {
                        // trigger error event
                        onUrlProcessFailed(url, e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fatal Error: {e.Message}");
            }
        }

        private async Task processUrl(string url)
        {
            // get url content
            var urlContent = await _urlContentReader.ReadAsync(url);

            if (urlContent == null)
            {
                throw new Exception("Url is not valid");
            }

            // save 
            await _urlContentStore.SaveAsync(url, urlContent.Content, urlContent.ContentType);

            // find new links and add to queue
            findNewLinks(urlContent);
        }

        private void findNewLinks(UrlContentResult urlContent)
        {
            if (urlContent.IsTextContent)
            {
                var content = Encoding.Default.GetString(urlContent.Content);

                // extract all links from page and add to queue 
                foreach (var extractedUrl in _urlsExtractor.Extract(content))
                {
                    tryAddUrlToQueue(extractedUrl);
                }
            }
        }


        private void resetData()
        {
            UrlsQueue.Clear();
            UrlsSucceed.Clear();
            UrlsFailed.Clear();
        }

        /// <summary>
        /// try add url to queue if there are no problem
        /// </summary>
        /// <param name="url"></param>
        private void tryAddUrlToQueue(string url)
        {
            var urlCorrect = generateCorrectUrl(url);
            if (canAddUrlToQueue(urlCorrect) == false)
            {
                return;
            }

            // add to queue 
            UrlsQueue.Enqueue(urlCorrect);

            // trigger event
            OnUrlAdded(urlCorrect);
        }

        /// <summary>
        /// check url that can add to queue or not (check duplication and others..)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool canAddUrlToQueue(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (url.StartsWith(RootUrl) == false && url.StartsWith("http"))
            {
                return false;
            }

            if (url.StartsWith(RootUrl) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// change raw url to complete url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string generateCorrectUrl(string url)
        {
            var u = url.ToLower();

            if (u.StartsWith("../"))
            {
                u = u.Substring(2, url.Length - 2);
            }

            if (u.StartsWith("/../"))
            {
                u = u.Substring(3, url.Length - 3);
            }

            if (u.Contains(".css?") || u.Contains(".js?"))
            {
                u = u.Substring(0, u.IndexOf("?", StringComparison.Ordinal));
            }

            if (u.StartsWith("/"))
            {
                u = RootUrl + u;
            }

            return u;
        }

        #endregion

        #region Events & Event Handlers

        public delegate void UrlEventHandler(object o, string url);
        public event UrlEventHandler UrlAdded;
        public event UrlEventHandler UrlProcessing;
        public event UrlEventHandler UrlProcessSucceed;

        public delegate void ErrorOccurredEventHandler(object o, string url, Exception exp);
        public event ErrorOccurredEventHandler UrlProcessFailed;


        private void OnUrlAdded(string url)
        {
            UrlAdded?.Invoke(this, url);
        }

        private void onUrlProcessing(string url)
        {
            UrlProcessing?.Invoke(this, url);
        }

        private void onUrlProcessFailed(string url, Exception exp)
        {
            UrlsFailed.TryAdd(url, 0);

            UrlProcessFailed?.Invoke(this, url, exp);
        }

        private void onUrlProcessSucceed(string url)
        {
            UrlsSucceed.TryAdd(url, 0);

            UrlProcessSucceed?.Invoke(this, url);
        }

        #endregion

    }
}
