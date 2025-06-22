using JobSeeker.Deduplication.Domain.Entities;

namespace JobSeeker.Deduplication.Application.Jobs.Common.UploadVacancyGroup.Models;

public class SourceDto
{
    public SourceDto(RawVacancy from)
    {
        SourceDomain = from.SourceDomain;
        SourceId = from.SourceId;
        Location = from.Location;
    }

    public string SourceDomain { get; set; } = null!;
    public string SourceId { get; set; } = null!;
    public string? Location { get; set; }
}