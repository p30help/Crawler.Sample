using Crawler.Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Crawler.Sample.Implements
{
    public class UrlExtractorByRegex : IUrlsExtractor
    {
        public IEnumerable<string> Extract(string html)
        {
            string hrefPattern = @"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>[^>\s]+))";

            Match regexMatch = Regex.Match(html, hrefPattern,
                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                TimeSpan.FromSeconds(1));

            while (regexMatch.Success)
            {
                yield return regexMatch.Groups[1].Value;

                regexMatch = regexMatch.NextMatch();
            }
        }
    }
}
