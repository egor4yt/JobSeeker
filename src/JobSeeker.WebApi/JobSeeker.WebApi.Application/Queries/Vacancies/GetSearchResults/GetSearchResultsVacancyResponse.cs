namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetSearchResults;

public class GetSearchResultsVacancyResponse
{
    public List<VacancyDto> Vacancies { get; } = [];
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