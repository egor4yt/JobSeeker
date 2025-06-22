namespace JobSeeker.WebApi.Domain.Entities;

public class Occupation
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int OccupationGroupId { get; set; }

    public virtual OccupationGroup OccupationGroup { get; set; } = null!;
    public virtual ICollection<Specialization> Specializations { get; set; } = null!;
    public virtual ICollection<ProfessionKey> ProfessionKeys { get; set; } = null!;
}