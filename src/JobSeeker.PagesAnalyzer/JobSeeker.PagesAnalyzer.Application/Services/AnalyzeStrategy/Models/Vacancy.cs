namespace JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy.Models;

public class Vacancy
{
    public string Role { get; set; } = null!;
    public string? Company { get; set; }
    public string? Description { get; set; }
    public string? HtmlDescription { get; set; }
    public string? City { get; set; }
}