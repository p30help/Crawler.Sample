namespace Crawler.Sample.Contracts
{
    public class UrlContentResult
    {
        public string Url { get; set; }

        public byte[] Content { get; set; }

        public string ContentType { get; set; }


        public bool IsTextContent
        {
            get
            {
                if (ContentType.StartsWith("text"))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
