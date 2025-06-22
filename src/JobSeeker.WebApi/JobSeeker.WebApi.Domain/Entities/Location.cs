namespace JobSeeker.WebApi.Domain.Entities;

public class Location
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public virtual ICollection<VacancySource> VacancySources { get; set; } = null!;
}