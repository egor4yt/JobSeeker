namespace JobSeeker.Deduplication.Domain.Entities;

/// <summary>
///     Represents the raw vacancy data
/// </summary>
public class RawVacancy
{
    public int Id { get; set; }
    public int? OriginalRawVacancyId { get; set; }
    public string? Location { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string SourceDomain { get; set; } = null!;
    public string SourceId { get; set; } = null!;
    public bool DeduplicationCompleted { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     SHA-256 fingerprint
    /// </summary>
    public string Fingerprint { get; set; } = null!;

    public int OccupationGroup { get; set; }
    public int? Occupation { get; set; }
    public int? Specialization { get; set; }
    public int? SkillTag { get; set; }

    public virtual RawVacancy? OriginalRawVacancy { get; set; }
    public virtual ICollection<RawVacancy> DuplicateVacancies { get; set; } = null!;
}