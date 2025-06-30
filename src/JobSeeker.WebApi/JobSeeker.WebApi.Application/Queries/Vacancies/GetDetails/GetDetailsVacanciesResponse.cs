namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetDetails;

public class GetDetailsVacanciesResponse
{
    public string Role { get; set; }
    public string Company { get; set; }
    public string HtmlDescription { get; set; }
    public IEnumerable<SourceDto> Sources { get; set; }
    
}

public class SourceDto
{
    public string Location { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public IEnumerable<string> SourceUrl { get; set; } = null!;
}