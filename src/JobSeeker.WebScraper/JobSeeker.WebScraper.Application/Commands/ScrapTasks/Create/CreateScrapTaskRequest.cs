namespace JobSeeker.WebScraper.Application.Commands.ScrapTasks.Create;

public class CreateScrapTaskRequest
{
    public string SearchText { get; set; } = null!;
    public string? ExcludeWordsList { get; set; } = null!;
    public List<string> SearchDomains { get; set; } = null!;
}