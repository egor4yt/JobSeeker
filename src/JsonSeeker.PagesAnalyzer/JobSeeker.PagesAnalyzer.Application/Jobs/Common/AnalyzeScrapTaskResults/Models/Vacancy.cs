namespace JobSeeker.PagesAnalyzer.Application.Jobs.Common.AnalyzeScrapTaskResults.Models;

public class Vacancy
{
    public string Role { get; set; } = null!;
    public string? Company { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? Url { get; set; }
}