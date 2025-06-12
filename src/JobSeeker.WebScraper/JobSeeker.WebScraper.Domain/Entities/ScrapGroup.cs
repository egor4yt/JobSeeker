namespace JobSeeker.WebScraper.Domain.Entities;

public class ScrapGroup
{
    public int Id { get; set; }
    /// <summary>
    ///     Lower priority tasks are processed sooner
    /// </summary>
    public int Priority { get; set; }

    public int OccupationGroup { get; set; }
    public int? Occupation { get; set; }
    public int? Specialization { get; set; }
    public int? SkillTag { get; set; }
    public virtual ICollection<ScrapTask> ScrapTasks { get; set; } = null!;
    public virtual ICollection<ScrapTaskConfiguration> ScrapTaskConfigurations { get; set; } = null!;
}