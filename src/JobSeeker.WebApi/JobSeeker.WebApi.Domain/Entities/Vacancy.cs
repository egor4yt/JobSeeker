namespace JobSeeker.WebApi.Domain.Entities;

public class Vacancy
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CompanyId { get; set; }
    public int ProfessionKeyId { get; set; }

    public virtual ProfessionKey ProfessionKey { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<VacancySource> VacancySources { get; set; } = null!;
}