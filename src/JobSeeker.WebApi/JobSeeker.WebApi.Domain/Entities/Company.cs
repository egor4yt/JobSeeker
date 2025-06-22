namespace JobSeeker.WebApi.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public virtual ICollection<Vacancy> Vacancies { get; set; }
}