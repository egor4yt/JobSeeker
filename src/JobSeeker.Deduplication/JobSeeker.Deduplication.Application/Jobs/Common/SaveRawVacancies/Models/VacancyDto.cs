using JobSeeker.Deduplication.Domain.Entities;

namespace JobSeeker.Deduplication.Application.Jobs.Common.SaveRawAndCalculateSignatures.Models;

public class VacancyDto
{
    public string Role { get; set; } = null!;
    public string? Company { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }

    public RawVacancy ToRawVacancy(string downloadKey, string sourceDomain, string sourceId)
    {
        var response = new RawVacancy();

        response.Title = Role;
        response.Location = City ?? string.Empty;
        response.Company = Company ?? string.Empty;
        response.Description = Description ?? string.Empty;
        response.DeduplicationCompleted = false;
        response.DownloadKey = downloadKey;
        response.SourceDomain = sourceDomain;
        response.SourceId = sourceId;

        return response;
    }
}