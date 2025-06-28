using System.Text.Json;
using JobSeeker.WebApi.Application.Jobs.Base;
using JobSeeker.WebApi.Application.Jobs.Common.DownloadDeduplicatedVacancies.Models;
using JobSeeker.WebApi.Domain.Entities;
using JobSeeker.WebApi.ObjectStorage;
using JobSeeker.WebApi.ObjectStorage.Models;
using JobSeeker.WebApi.Persistence;
using JobSeeker.WebApi.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebApi.Application.Jobs.Common.DownloadDeduplicatedVacancies;

public class DownloadDeduplicatedVacanciesJob(
    ILogger<DownloadDeduplicatedVacanciesJob> logger,
    IObjectStorage objectStorage,
    ApplicationDbContext dbContext) : IJob<JobParameters.Common.DownloadDeduplicatedVacancies>
{
    /// <summary>
    ///     Maximum number of object keys that can be processed in parallel in a single chunk
    /// </summary>
    private const int MaxChunkSize = 5;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(MaxChunkSize, MaxChunkSize);

    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.DownloadDeduplicatedVacancies _parameter = null!;

    public async Task RunAsync(JobParameters.Common.DownloadDeduplicatedVacancies parameter, CancellationToken cancellationToken = default)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogDebug("Started downloading deduplicated vacancy group {@Parameter}", _parameter);

        await RunAsync();

        logger.LogDebug("Finished downloading deduplicated vacancy group {@Parameter}", _parameter);
    }

    private async Task RunAsync()
    {
        var professionKey = await dbContext.ProfessionKeys
            .SingleOrDefaultAsync(x => x.OccupationGroup.Id == _parameter.OccupationGroup
                                       && x.Occupation.Id == _parameter.Occupation
                                       && x.Specialization.Id == _parameter.Specialization
                                       && x.SkillTag.Id == _parameter.SkillTag, _cancellationToken);

        if (professionKey == null) throw new Exception("Profession Key not found");

        var downloadedVacancies = await GetAllVacancies();

        var newVacancies = await CreateAndUpdateVacancies(downloadedVacancies, professionKey.Id);
        if (newVacancies.Count != 0) await dbContext.Vacancies.AddRangeAsync(newVacancies, _cancellationToken);
        await dbContext.SaveChangesAsync(_cancellationToken);
    }

    private async Task<List<VacancyDto>> GetAllVacancies()
    {
        var request = new GetAllObjectsOptions
        {
            Bucket = Buckets.Deduplication,
            Path = $"{_parameter.OccupationGroup}/{_parameter.Occupation}/{_parameter.Specialization}/{_parameter.SkillTag}"
        };

        var objectsKeys = await objectStorage.GetAllObjectsRecursiveAsync(request, _cancellationToken);
        if (objectsKeys.Count == 0) return [];

        var response = await Task.WhenAll(objectsKeys.Select(GetVacancyAsync));
        return response.Where(x => x != null).Select(x => x!).ToList();
    }

    private async Task<VacancyDto?> GetVacancyAsync(string objectKey)
    {
        await _semaphoreSlim.WaitAsync(_cancellationToken);

        VacancyDto? response;

        try
        {
            logger.LogDebug("Started processing {ObjectKey}", objectKey);

            var getObjectRequest = new GetObjectOptions
            {
                Bucket = Buckets.Deduplication,
                Path = objectKey
            };

            await using var stream = await objectStorage.GetObjectStreamAsync(getObjectRequest, _cancellationToken);

            response = await JsonSerializer.DeserializeAsync<VacancyDto>(stream, _jsonSerializerOptions, _cancellationToken);
            if (response == null) throw new InvalidOperationException($"Failed to deserialize file '{objectKey}'");

            logger.LogDebug("Finished downloading {ObjectKey}", objectKey);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to process file '{ObjectKey}'", objectKey);
            throw;
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        return response;
    }

    private async Task<Dictionary<string, Company>> CreateCompanies(IList<VacancyDto> vacanciesToCreate)
    {
        var potentialCompanies = vacanciesToCreate
            .DistinctBy(x => x.Company)
            .Select(x => new Company
            {
                Name = x.Company,
                Slug = SlugHelper.GenerateSlug(x.Company)
            })
            .ToList();

        var response = await dbContext.Companies
            .Where(x => potentialCompanies.Select(c => c.Slug).Contains(x.Slug))
            .ToDictionaryAsync(x => x.Slug, x => x, _cancellationToken);

        var companiesToCreate = potentialCompanies
            .Where(x => response.ContainsKey(x.Slug) == false)
            .ToList();

        await dbContext.Companies.AddRangeAsync(companiesToCreate, _cancellationToken);

        companiesToCreate.ForEach(x => response.Add(x.Slug, x));

        return response;
    }

    private async Task<Dictionary<string, Location>> CreateLocations(IList<VacancyDto> vacanciesToCreate)
    {
        var potentialLocations = vacanciesToCreate
            .SelectMany(x => x.Sources)
            .DistinctBy(x => x.Location)
            .Where(x => string.IsNullOrWhiteSpace(x.Location) == false)
            .Select(x => new Location
            {
                Title = x.Location!,
                Slug = SlugHelper.GenerateSlug(x.Location!)
            })
            .ToList();

        var response = await dbContext.Locations
            .Where(x => potentialLocations.Select(c => c.Slug).Contains(x.Slug))
            .ToDictionaryAsync(x => x.Slug, x => x, _cancellationToken);

        var locationsToCreate = potentialLocations
            .Where(x => response.ContainsKey(x.Slug) == false)
            .ToList();

        await dbContext.Locations.AddRangeAsync(locationsToCreate, _cancellationToken);

        locationsToCreate.ForEach(x => response.Add(x.Slug, x));

        return response;
    }

    private async Task<List<Vacancy>> CreateAndUpdateVacancies(IList<VacancyDto> vacanciesToCreate, int professionKeyId)
    {
        var newVacancies = new List<Vacancy>();

        var companies = await CreateCompanies(vacanciesToCreate);
        var locations = await CreateLocations(vacanciesToCreate);
        var sources = await dbContext.Sources.ToListAsync(_cancellationToken);

        var existsVacancies = await dbContext.Vacancies
            .Include(x => x.Company)
            .Include(x => x.VacancySources)
            .Where(x => x.ProfessionKeyId == professionKeyId
                        && x.ActualityDate > DateTime.UtcNow.AddDays(-30))
            .OrderBy(x => x.ActualityDate)
            .GroupBy(x => new { x.CompanyId, x.ActualityDate })
            .Select(x => x.First())
            .ToListAsync(_cancellationToken);

        foreach (var vacancy in vacanciesToCreate)
        {
            var newVacancy = new Vacancy();
            newVacancy.Title = vacancy.Title;
            newVacancy.Description = vacancy.Description;
            newVacancy.ActualityDate = vacancy.CreatedAt;
            newVacancy.ProfessionKeyId = professionKeyId;
            newVacancy.VacancySources = [];

            var companySlug = SlugHelper.GenerateSlug(vacancy.Company);
            var company = companies[companySlug];

            var existsVacancy = existsVacancies.FirstOrDefault(x => x.Company.Slug == company.Slug
                                                                    && x.ActualityDate == newVacancy.ActualityDate
                                                                    && x.ProfessionKeyId == professionKeyId
                                                                    && x.Company.Name == company.Name);

            if (existsVacancy != null)
            {
                UpdateVacancySources(existsVacancy, vacancy, locations, sources);
                logger.LogDebug("Updated vacancy {VacancyTitle}", existsVacancy.Title);

                continue;
            }

            UpdateVacancySources(newVacancy, vacancy, locations, sources);
            newVacancy.Company = company;
            newVacancies.Add(newVacancy);

            logger.LogDebug("Added new vacancy {VacancyTitle}", newVacancy.Title);
        }

        return newVacancies;
    }

    private void UpdateVacancySources(Vacancy vacancy, VacancyDto vacancyDto, Dictionary<string, Location> locations, List<Source> sources)
    {
        vacancy.VacancySources.Clear();
        
        foreach (var vacancySource in vacancyDto.Sources)
        {
            var locationSlug = SlugHelper.GenerateSlug(vacancySource.Location);
            var location = string.IsNullOrWhiteSpace(locationSlug) == false ? locations[locationSlug] : null;

            var topLevelDomain = string.Join('.', vacancySource.SourceDomain.Split('.').TakeLast(2)).ToLowerInvariant();
            var sourceId = sources.FirstOrDefault(x => x.TopLevelDomain == topLevelDomain)?.Id;
            if (sourceId.HasValue == false)
            {
                logger.LogWarning("Unknown domain {Domain}", vacancySource.SourceDomain);
                continue;
            }

            var source = new VacancySource
            {
                SourceKey = vacancySource.SourceId,
                SourceId = sourceId.Value,
                Location = location
            };

            vacancy.VacancySources.Add(source);
        }
    }
}