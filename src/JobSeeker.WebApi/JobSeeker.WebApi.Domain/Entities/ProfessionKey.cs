namespace JobSeeker.WebApi.Domain.Entities;

public class ProfessionKey
{
    public int Id { get; set; }
    public int OccupationGroupId { get; set; }
    public int? OccupationId { get; set; }
    public int? SpecializationId { get; set; }
    public int? SkillTagId { get; set; }

    /// <summary>
    ///     User-friendly profession title
    /// </summary>
    public string Title { get; set; } = null!;

    public virtual ICollection<Vacancy> Vacancies { get; set; } = null!;
    public virtual OccupationGroup OccupationGroup { get; set; } = null!;
    public virtual Occupation Occupation { get; set; } = null!;
    public virtual Specialization Specialization { get; set; } = null!;
    public virtual SkillTag SkillTag { get; set; } = null!;
}