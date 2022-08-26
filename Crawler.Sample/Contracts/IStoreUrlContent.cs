using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Sample.Contracts
{
    public interface IUrlContentStore
    {
        Task SaveAsync(string filename, byte[] byteArray, string contentType);

        //Task SaveAsync(string filename, string data);
        //
        //Task SaveFileAsync(string filename, string url);
    }
}
