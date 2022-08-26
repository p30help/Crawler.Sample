using Crawler.Sample.Tools;
using FluentAssertions;
using Xunit;

namespace Crawler.Sample.Tests
{
    public class UniqueUrlQueueTest
    {
        [Theory]
        [InlineData("james", "Justin", true)]
        [InlineData("james", "James", true)]
        [InlineData("ana", "ana", false)]
        public void UniqueUrlQueue_CheckForDuplicationUrls_ReturnOk(string url1, string url2, bool shouldUrl2Inserted)
        {
            // arrange
            var queue = new UniqueUrlQueue();
            queue.Enqueue(url1);

            // act
            var result = queue.Enqueue(url2);

            // assert
            result.Should().Be(shouldUrl2Inserted);
        }

    }
}
