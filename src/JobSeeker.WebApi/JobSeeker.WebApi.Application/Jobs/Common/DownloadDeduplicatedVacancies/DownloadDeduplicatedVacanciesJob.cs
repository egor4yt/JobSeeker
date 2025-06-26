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

        var companiesSlugs = downloadedVacancies.Select(v => SlugHelper.GenerateSlug(v.Company));
        var companies = await dbContext.Companies
            .Where(x => companiesSlugs.Contains(x.Slug))
            .ToDictionaryAsync(x => x.Slug, x => x, _cancellationToken);

        var newVacancies = CreateVacancies(downloadedVacancies, companies, professionKey.Id);

        await dbContext.Vacancies.AddRangeAsync(newVacancies, _cancellationToken);
        await dbContext.SaveChangesAsync(_cancellationToken);
        
        //TODO: update old vacancies
        
        // var oldVacancies = await dbContext.Vacancies
        //     .Where(x => x.ProfessionKeyId == professionKey.Id)
        //     .ToListAsync(_cancellationToken);
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

    private List<Vacancy> CreateVacancies(IList<VacancyDto> vacancies, Dictionary<string, Company> companies, int professionKeyId)
    {
        var newVacancies = new List<Vacancy>();

        foreach (var vacancy in vacancies)
        {
            var newVacancy = new Vacancy();
            newVacancy.Title = vacancy.Title;
            newVacancy.Description = vacancy.Description;
            newVacancy.ProfessionKeyId = professionKeyId;

            var companySlug = SlugHelper.GenerateSlug(vacancy.Company);

            if (companies.TryGetValue(companySlug, out var company))
                newVacancy.Company = company;
            else
            {
                var newCompany = new Company
                {
                    Name = vacancy.Company,
                    Slug = companySlug
                };
                newVacancy.Company = newCompany;

                companies.Add(companySlug, newCompany);
            }

            newVacancies.Add(newVacancy);
        }

        return newVacancies;
    }
}