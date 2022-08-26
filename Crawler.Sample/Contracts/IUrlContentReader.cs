using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Sample.Contracts
{
    public interface IUrlContentReader
    {
        Task<UrlContentResult> ReadAsync(string url);
    }
}
