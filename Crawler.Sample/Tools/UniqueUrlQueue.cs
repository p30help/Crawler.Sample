using System.Collections.Concurrent;

namespace Crawler.Sample.Tools
{
    public class UniqueUrlQueue
    {
        public int InputsCount
        {
            get { return Urls.Count; }
        }

        readonly ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();

        readonly ConcurrentDictionary<string, int> Urls = new ConcurrentDictionary<string, int>();

        public bool Enqueue(string url)
        {
            // add to dictionary for prevent duplicate url
            if (Urls.TryAdd(url, 0) == false)
            {
                return false;
            }

            Urls.TryAdd(url, 0);

            Queue.Enqueue(url);

            return true;
        }

        public bool TryDequeue(out string url)
        {
            return Queue.TryDequeue(out url);
        }

        public void Clear()
        {
            Queue?.Clear();
            Urls?.Clear();
        }
    }
}
