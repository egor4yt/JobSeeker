namespace JobSeeker.WebApi.Domain.Entities;

public class VacancySource
{
    public int Id { get; set; }
    public string SourceKey { get; set; } = null!;
    public int SourceId { get; set; }
    public int LocationId { get; set; }
    public int VacancyId { get; set; }

    public virtual Source Source { get; set; } = null!;
    public virtual Vacancy Vacancy { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
}