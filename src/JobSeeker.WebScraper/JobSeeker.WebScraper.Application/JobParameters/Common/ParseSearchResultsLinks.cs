using JobSeeker.WebScraper.Application.JobParameters.Base;

namespace JobSeeker.WebScraper.Application.JobParameters.Common;

public class ParseSearchResultsLinks : JobParameter
{
    /// <summary>
    ///     Entry point to parse search results pages links
    /// </summary>
    public required string BaseSearchUrl { get; set; }
}