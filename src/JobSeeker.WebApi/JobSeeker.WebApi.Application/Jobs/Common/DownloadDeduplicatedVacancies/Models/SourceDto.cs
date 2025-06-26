namespace JobSeeker.WebApi.Application.Jobs.Common.DownloadDeduplicatedVacancies.Models;

public class SourceDto
{
    public string SourceDomain { get; set; } = null!;
    public string SourceId { get; set; } = null!;
    public string? Location { get; set; }
}