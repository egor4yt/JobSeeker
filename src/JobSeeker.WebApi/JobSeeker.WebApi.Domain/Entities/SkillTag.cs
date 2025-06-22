namespace JobSeeker.WebApi.Domain.Entities;

public class SkillTag
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int SpecializationId { get; set; }

    public virtual Specialization Specialization { get; set; } = null!;
    public virtual ICollection<ProfessionKey> ProfessionKeys { get; set; } = null!;
}