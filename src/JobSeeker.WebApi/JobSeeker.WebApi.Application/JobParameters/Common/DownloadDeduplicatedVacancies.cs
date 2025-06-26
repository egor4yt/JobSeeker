using JobSeeker.WebApi.Application.JobParameters.Base;

namespace JobSeeker.WebApi.Application.JobParameters.Common;

public class DownloadDeduplicatedVacancies : JobParameter
{
    public int OccupationGroup { get; set; }
    public int? Occupation { get; set; }
    public int? Specialization { get; set; }
    public int? SkillTag { get; set; }
}