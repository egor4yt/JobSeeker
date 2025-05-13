using JobSeeker.WebScraper.Application.JobParameters.Base;

namespace JobSeeker.WebScraper.Application.JobParameters.Common;

public class UploadSearchResults : JobParameter
{
    public int ScrapTaskId { get; set; }
}