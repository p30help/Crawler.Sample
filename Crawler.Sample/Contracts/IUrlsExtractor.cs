using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Sample.Contracts
{
    public interface IUrlsExtractor
    {
        IEnumerable<string> Extract(string html);
    }
}
