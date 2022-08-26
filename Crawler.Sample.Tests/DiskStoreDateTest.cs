using Crawler.Sample.Implements;
using FluentAssertions;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Crawler.Sample.Tests
{
    public class DiskStoreDateTest
    {
        [Theory]
        [InlineData("test_file", "test_file.html")]
        [InlineData("test_file.html", "test_file.html")]
        [InlineData("department/test_file.html", "department_test_file.html")]
        [InlineData("https://a1/a2/test_file", "a1_a2_test_file.html")]
        [InlineData("http://a1/a2/test?main:file.exe", "a1_a2_testmainfile.exe.html")]
        public void DiskStoreDate_CheckSaveHtmlFile_ReturnOk(string fileName, string outputFileName)
        {
            // arrange
            var rootDir = Path.GetDirectoryName(System.Reflection.Assembly.
                GetExecutingAssembly().Location);
            var diskStore = new DiskStoreUrlContent(rootDir);
            var data = Encoding.Default.GetBytes("<a>hello</a>");

            var outputFile = rootDir + "/" + outputFileName;
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            // act
            Action comparison = () =>
            {
                diskStore.SaveAsync(fileName, data, "text/html").Wait();
            };

            // assert
            comparison.Should().NotThrow<Exception>();
            File.Exists(outputFile).Should().Be(true);

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }

        [Theory]
        [InlineData("test_file.png", "test_file.png")]
        [InlineData("test_file", "test_file")]
        [InlineData("department/test_file.png", "department_test_file.png")]
        [InlineData("a1/a2/test_file", "a1_a2_test_file")]
        public void DiskStoreDate_CheckSaveFile_ReturnOk(string fileName, string outputFileName)
        {
            // arrange
            var rootDir = Path.GetDirectoryName(System.Reflection.Assembly.
                GetExecutingAssembly().Location);
            var diskStore = new DiskStoreUrlContent(rootDir);
            var data = Encoding.Default.GetBytes("image");

            var outputFile = rootDir + "/" + outputFileName;
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            // act
            Action comparison = () =>
            {
                diskStore.SaveAsync(fileName, data, "image/png").Wait();
            };

            // assert
            comparison.Should().NotThrow<Exception>();
            File.Exists(outputFile).Should().Be(true);

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }
}
