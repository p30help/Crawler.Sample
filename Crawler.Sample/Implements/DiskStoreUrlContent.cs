using Crawler.Sample.Contracts;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Sample.Implements
{
    public class DiskStoreUrlContent : IUrlContentStore
    {
        string OutputDir { get; }


        public DiskStoreUrlContent(string outputDir)
        {
            OutputDir = outputDir;

            if (Directory.Exists(outputDir) == false)
            {
                Directory.CreateDirectory(outputDir);
            }
        }

        public async Task SaveAsync(string filename, byte[] byteArray, string contentType)
        {
            var correctedFilename = correctFileName(filename);
            var filePath = generateOutputFilePath(correctedFilename);

            if (contentType.StartsWith("text"))
            {
                filePath += filePath.EndsWith(".html") ? "" : ".html";
            }

            deleteIfExisted(filePath);

            await File.WriteAllBytesAsync(filePath, byteArray);
        }

        private string generateOutputFilePath(string filename)
        {
            return OutputDir + @"\" + filename;
        }

        private string correctFileName(string filename)
        {
            var correctedFilename = filename
                .Replace("http://", "")
                .Replace("https://", "")
                .Replace("/", "_")
                .Replace(":", "")
                .Replace("?", "");

            return correctedFilename;
        }

        private void deleteIfExisted(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
