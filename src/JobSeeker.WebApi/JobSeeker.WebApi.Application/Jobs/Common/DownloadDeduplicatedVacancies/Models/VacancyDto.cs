using System.Text.Json.Serialization;

namespace JobSeeker.WebApi.Application.Jobs.Common.DownloadDeduplicatedVacancies.Models;

public class VacancyDto
{
    public string Title { get; set; }
    public string Description { get; set; } = null!;
    public string HtmlDescription { get; set; } = null!;
    public string Company { get; set; } = null!;
    public List<SourceDto> Sources { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    [JsonIgnore]
    public string ObjectKey { get; set; }
}