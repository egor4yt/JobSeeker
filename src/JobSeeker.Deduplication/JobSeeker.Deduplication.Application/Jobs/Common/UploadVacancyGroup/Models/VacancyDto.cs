using System.Text.Json.Serialization;
using JobSeeker.Deduplication.Domain.Entities;

namespace JobSeeker.Deduplication.Application.Jobs.Common.UploadVacancyGroup.Models;

public class VacancyDto
{
    public VacancyDto(RawVacancy from)
    {
        Title = from.Title;
        Description = from.Description;
        Company = from.Company;
        CreatedAt = from.CreatedAt;

        var occupationGroup = from.OccupationGroup.ToString();
        var occupation = from.Occupation?.ToString() ?? "null";
        var specialization = from.Occupation?.ToString() ?? "null";
        var skillTag = from.Occupation?.ToString() ?? "null";
        ObjectPath = $"{occupationGroup}/{occupation}/{specialization}/{skillTag}";

        Sources = [new SourceDto(from)];
    }

    public string Title { get; set; }
    public string Description { get; set; } = null!;
    public string Company { get; set; } = null!;
    public List<SourceDto> Sources { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public string ObjectPath { get; set; }
}