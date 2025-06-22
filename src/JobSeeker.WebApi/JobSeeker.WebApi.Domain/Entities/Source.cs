namespace JobSeeker.WebApi.Domain.Entities;

public class Source
{
    public int Id { get; set; }

    public string TopLevelDomain { get; set; } = null!;

    /// <summary>
    ///     String format of 'https://domain.com/vacancy/{0}/details', where arg {0} will be replaced to vacancy id
    /// </summary>
    public string UrlTemplate { get; set; } = null!;

    public virtual ICollection<VacancySource> VacancySources { get; set; } = null!;
}