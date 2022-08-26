using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.Sample.Contracts
{
    public interface ICrawler
    {
        public Task StartAsync(string startUrl);

        public Task StartAsync(string startUrl, CancellationToken cancellationToken);
    }
}
