using JobSeeker.PagesAnalyzer.Application.JobParameters.Base;

namespace JobSeeker.PagesAnalyzer.Application.JobParameters.Common;

public class AnalyzeScrapTaskResults : JobParameter
{
    public int ScrapTaskId { get; set; }
}