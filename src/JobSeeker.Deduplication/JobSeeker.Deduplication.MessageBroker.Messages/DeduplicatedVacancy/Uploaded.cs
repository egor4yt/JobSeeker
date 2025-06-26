namespace JobSeeker.Deduplication.MessageBroker.Messages.DeduplicatedVacancy;

public class Uploaded
{
    public int OccupationGroup { get; set; }
    public int? Occupation { get; set; }
    public int? Specialization { get; set; }
    public int? SkillTag { get; set; }
}