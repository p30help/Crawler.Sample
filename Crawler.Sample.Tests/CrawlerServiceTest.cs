using Crawler.Sample.Contracts;
using Crawler.Sample.Implements;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Crawler.Sample.Tests
{
    public class CrawlerServiceTest
    {
        [Theory]
        [MemberData(nameof(GetSamplePages))]
        public void CrawlerService_CrawlPages_ReturnOk(int successCount, int errorsCount, params PageSample[] pages)
        {
            // arrange
            var startUrl = pages[0].Url;

            var urlContentReader = new Mock<IUrlContentReader>();
            foreach (var page in pages)
            {
                urlContentReader.Setup(x => x.ReadAsync(page.Url))
                    .ReturnsAsync(new UrlContentResult()
                    {
                        Content = page.Content,
                        ContentType = page.ContentType,
                        Url = page.Url
                    });
            }

            var urlsExtractor = new UrlExtractorByRegex();

            var storeUrlContent = new Mock<IUrlContentStore>();
            storeUrlContent.Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var crawler = new CrawlerService(urlContentReader.Object,
                urlsExtractor, storeUrlContent.Object)
            {
                ThreadCount = 5
            };

            // act
            Action comparison = () =>
            {
                crawler.StartAsync(startUrl).Wait();
            };

            // assert
            comparison.Should().NotThrow<Exception>();
            crawler.RootUrl.Should().Be(startUrl);
            crawler.IsAllUrlProcessed.Should().Be(true);
            crawler.ProgressPercentage.Should().Be(100);
            crawler.TotalCount.Should().Be(successCount + errorsCount);
            crawler.SuccessCount.Should().Be(successCount);
            crawler.FailedCount.Should().Be(errorsCount);
        }

        [Fact]
        public void CrawlerService_RunStartAsyncTwiceWithoutProblem_ReturnOk()
        {
            // arrange
            var startUrl = "https://p30help.ir";
            var crawler = createCrawlerService(startUrl);

            // act
            Action comparison = () =>
            {
                crawler.StartAsync(startUrl).Wait();

                crawler.StartAsync(startUrl).Wait();
            };

            // assert
            comparison.Should().NotThrow<Exception>();
            crawler.RootUrl.Should().Be(startUrl);
            crawler.IsAllUrlProcessed.Should().Be(true);
            crawler.ProgressPercentage.Should().Be(100);
            crawler.TotalCount.Should().Be(2);
            crawler.SuccessCount.Should().Be(2);
            crawler.FailedCount.Should().Be(0);
        }

        private CrawlerService createCrawlerService(string startUrl)
        {
            var urlContentReader = new Mock<IUrlContentReader>();
            urlContentReader.Setup(x => x.ReadAsync(startUrl))
                .ReturnsAsync(new UrlContentResult()
                {
                    Url = startUrl,
                    ContentType = "text/html",
                    Content = Encoding.Default.GetBytes("<a href=\"/privacy\">click here</a>"),
                });
            urlContentReader.Setup(x => x.ReadAsync(startUrl + "/privacy"))
                .ReturnsAsync(new UrlContentResult()
                {
                    Url = startUrl + "/privacy",
                    ContentType = "text/html",
                    Content = Encoding.Default.GetBytes("privacy read more"),
                });

            var urlsExtractor = new UrlExtractorByRegex();

            var storeUrlContent = new Mock<IUrlContentStore>();
            storeUrlContent.Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var crawler = new CrawlerService(urlContentReader.Object,
                urlsExtractor, storeUrlContent.Object)
            {
                ThreadCount = 5
            };

            return crawler;
        }

        public static IEnumerable<object[]> GetSamplePages()
        {
            // 
            yield return new object[]
            {
                4, // success count
                0, // error count
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir",
                    Content =  Encoding.Default.GetBytes("<p><a href=\"/about-us\">about us</a> " +
                                                         "<a href=\"/test/jobs\">jobs</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/about",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/contact-us\"> contact us</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/connect-us",
                    Content = Encoding.Default.GetBytes("<p></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/test/jobs",
                    Content = Encoding.Default.GetBytes("<p></p>"),
                }
            };

            //
            yield return new object[]
            {
                3, // success count
                1, // error count
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/about-us\">about us</a> " +
                                                        "<a href=\"/test/jobs\">jobs</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/about",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/contact-us\"> contact us</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/test/jobs",
                    Content = Encoding.Default.GetBytes("<p></p>"),
                }
            };

            //
            yield return new object[]
            {
                4, // success count
                0, // error count
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/about-us\">about us</a> " +
                                                        "<a href=\"/test/jobs\">jobs</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/about",
                    Content = Encoding.Default.GetBytes("<p>this is us</p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/test/jobs",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/file.pdf\">download book</a></p>"),
                },
                new PageSample
                {
                    ContentType = "image/png",
                    Url = "http://p30help.ir/file.pdf",
                    Content = null,
                }
            };

            //
            yield return new object[]
            {
                4, // success count
                1, // error count
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/about-us\">about us</a> <a href=\"/test/jobs\">jobs</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/about",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/contact-us\"> contact us</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://p30help.ir/test/jobs",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/file.pdf\">download book</a></p>"),
                },
                new PageSample
                {
                    ContentType = "image/png",
                    Url = "http://p30help.ir/file.pdf",
                    Content = null,
                }
            };

            //
            yield return new object[]
            {
                4, // success count
                1, // error count
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://google.com",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/about-us\">about us</a> " +
                                                        "<a href=\"/test/jobs\">jobs</a> " +
                                                        "<a href=\"http://yahoo.com/test\">yahoo</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://google.com/about-us",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/contact-us\">contact us</a></p>"),
                },
                new PageSample
                {
                    ContentType = "text/html",
                    Url = "http://google.com/test/jobs",
                    Content = Encoding.Default.GetBytes("<p><a href=\"/file.pdf\">download book</a></p>"),
                },
                new PageSample
                {
                    ContentType = "image/jpeg",
                    Url = "http://google.com/file.pdf",
                    Content = null,
                }
            };
        }

    }

    public class PageSample
    {
        public string ContentType { get; set; }

        public string Url { get; set; }

        public byte[] Content { get; set; }
    }

}
