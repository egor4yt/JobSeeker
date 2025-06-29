namespace JobSeeker.WebUi.Application.Contracts.WebApi.Vacancies;

public class SearchResponse
{
    public List<VacancyDto> Vacancies { get; set; }
    public int Total { get; set; }
}

public class VacancyDto
{
    public int VacancyId { get; set; }
    public string CompanyTitle { get; set; } = null!;
    public string ShortDescription { get; set; } = null!;
    public string Role { get; set; } = null!;
    public int DaysAgoCreated { get; set; }
}