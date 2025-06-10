using JobSeeker.Deduplication.Application.JobParameters.Base;

namespace JobSeeker.Deduplication.Application.JobParameters.Common;

public class ParseSearchResultsLinks : JobParameter
{
    public int ScrapTaskId { get; set; }
}