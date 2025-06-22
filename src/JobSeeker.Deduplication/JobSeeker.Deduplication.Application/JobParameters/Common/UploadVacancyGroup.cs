using JobSeeker.Deduplication.Application.JobParameters.Base;

namespace JobSeeker.Deduplication.Application.JobParameters.Common;

public class UploadVacancyGroup : JobParameter
{
    public int OccupationGroup { get; set; }
    public int? Occupation { get; set; }
    public int? Specialization { get; set; }
    public int? SkillTag { get; set; }
}