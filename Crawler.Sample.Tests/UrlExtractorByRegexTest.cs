using Crawler.Sample.Implements;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Crawler.Sample.Tests
{
    public class UrlExtractorByRegexTest
    {
        [Theory]
        [InlineData("<p>please click in <a class=\"bold\" href=\"http://google.com\">here</a></p>", new string[] { "http://google.com" })]
        [InlineData(" <p>please click in <a class=\"bold\" href=\"../about\">here</a> - <a href=\"http://p30help.ir\">here</a></p>", new string[] { "../about", "http://p30help.ir" })]
        public void UrlExtractorByRegex_ExtractHrefFromContent_ReturnOk(string html, string[] urls)
        {
            // arrange

            // act
            var urlExtractor = new UrlExtractorByRegex();
            var result = urlExtractor.Extract(html);

            // assert
            result.Count().Should().Be(urls.Length);
            result.Should().Contain(urls);
        }

    }
}
