using JobSeeker.WebScraper.Application.JobParameters.Base;

namespace JobSeeker.WebScraper.Application.JobParameters.Common;

public class ParseSearchResultsLinks : JobParameter
{
    public int ScrapTaskId { get; set; }
}