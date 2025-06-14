using JobSeeker.Deduplication.Application.JobParameters.Base;

namespace JobSeeker.Deduplication.Application.JobParameters.Common;

public class SaveRawVacancies : JobParameter
{
    public int ScrapTaskId { get; set; }
}