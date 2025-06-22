namespace JobSeeker.WebApi.Domain.Entities;

public class OccupationGroup
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;

    public virtual ICollection<Occupation> Occupations { get; set; } = null!;
    public virtual ICollection<ProfessionKey> ProfessionKeys { get; set; } = null!;
}