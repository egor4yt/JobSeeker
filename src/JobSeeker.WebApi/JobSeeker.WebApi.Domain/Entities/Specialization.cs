namespace JobSeeker.WebApi.Domain.Entities;

public class Specialization
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int OccupationId { get; set; }

    public virtual Occupation Occupation { get; set; } = null!;
    public virtual ICollection<SkillTag> SkillTags { get; set; } = null!;
    public virtual ICollection<ProfessionKey> ProfessionKeys { get; set; } = null!;
}