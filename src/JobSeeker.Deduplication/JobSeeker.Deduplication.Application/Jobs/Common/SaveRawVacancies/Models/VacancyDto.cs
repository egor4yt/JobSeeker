using JobSeeker.Deduplication.Domain.Entities;

namespace JobSeeker.Deduplication.Application.Jobs.Common.SaveRawVacancies.Models;

public class VacancyDto
{
    public string Role { get; set; } = null!;
    public string? Company { get; set; }
    public string? Description { get; set; }
    public string? HtmlDescription { get; set; }
    public string? City { get; set; }

    public RawVacancy ToRawVacancy(string downloadKey, string sourceDomain, string sourceId,
        string occupationGroup, string occupation, string specialization, string skillTag)
    {
        var response = new RawVacancy();

        response.Title = Role;
        response.Location = City ?? string.Empty;
        response.Company = Company ?? string.Empty;
        response.Description = Description ?? string.Empty;
        response.DeduplicationCompleted = false;
        response.SourceDomain = sourceDomain;
        response.SourceId = sourceId;
        response.HtmlDescription = HtmlDescription ?? string.Empty;

        response.CreatedAt = DateTime.UtcNow;
        response.OccupationGroup = int.Parse(occupationGroup);
        if (int.TryParse(occupation, out var occupationInt)) response.Occupation = occupationInt;
        if (int.TryParse(specialization, out var specializationInt)) response.Specialization = specializationInt;
        if (int.TryParse(skillTag, out var skillTagInt)) response.SkillTag = skillTagInt;

        return response;
    }
}