using System.Text.Json;
using JobSeeker.Deduplication.Application.Jobs.Base;
using JobSeeker.Deduplication.Application.Jobs.Common.SaveRawVacancies.Models;
using JobSeeker.Deduplication.Application.Services.Fingerprints;
using JobSeeker.Deduplication.Application.Services.Lsh;
using JobSeeker.Deduplication.Domain.Entities;
using JobSeeker.Deduplication.MessageBroker.Producers;
using JobSeeker.Deduplication.ObjectStorage;
using JobSeeker.Deduplication.ObjectStorage.Models;
using JobSeeker.Deduplication.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.Application.Jobs.Common.SaveRawVacancies;

public class SaveRawVacanciesJob(
    ILogger<SaveRawVacanciesJob> logger,
    IObjectStorage objectStorage,
    ApplicationDbContext dbContext,
    IFingerprintStrategy<RawVacancy> fingerprintStrategy,
    ILshStrategy<RawVacancy> lshStrategy,
    IMessageProducer<MessageBroker.Messages.ScrapTask.RawSaved> producer) : IJob<JobParameters.Common.SaveRawVacancies>
{
    /// <summary>
    ///     Maximum number of object keys that can be processed in parallel in a single chunk
    /// </summary>
    private const int MaxChunkSize = 5;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(MaxChunkSize, MaxChunkSize);

    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.SaveRawVacancies _parameter = null!;

    public async Task RunAsync(JobParameters.Common.SaveRawVacancies parameter, CancellationToken cancellationToken)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogInformation("Started downloading raw vacancies for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);

        var messages = await RunAsync();

        logger.LogInformation("Finished downloading raw vacancies for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);

        foreach (var message in messages)
        {
            await producer.ProduceAsync(message, _cancellationToken);
        }
    }

    private async Task<List<MessageBroker.Messages.ScrapTask.RawSaved>> RunAsync()
    {
        var response = new List<MessageBroker.Messages.ScrapTask.RawSaved>();
        var request = new GetAllObjectsOptions
        {
            Bucket = Buckets.PagesAnalyzer,
            Path = $"{_parameter.ScrapTaskId}"
        };

        var objectsKeys = await objectStorage.GetAllObjectsRecursiveAsync(request, _cancellationToken);
        if (objectsKeys.Count == 0) return response;

        var downloadedVacancies = await Task.WhenAll(objectsKeys.Select(GetVacanciesAsync));
        await CalculateFingerprintsAsync(downloadedVacancies);

        var downloadedGroups = downloadedVacancies
            .GroupBy(x => new { x.OccupationGroup, x.Occupation, x.Specialization, x.SkillTag });

        var newVacancies = new List<RawVacancy>();

        foreach (var downloadedGroup in downloadedGroups)
        {
            var existsVacancies = await dbContext.RawVacancies
                .Where(x => x.OccupationGroup == downloadedGroup.Key.OccupationGroup
                            && x.Occupation == downloadedGroup.Key.Occupation
                            && x.Specialization == downloadedGroup.Key.Specialization
                            && x.SkillTag == downloadedGroup.Key.SkillTag)
                .Select(x => new
                {
                    x.Fingerprint,
                    x.SourceDomain,
                    x.SourceId
                })
                .ToListAsync(_cancellationToken);

            newVacancies.AddRange(downloadedGroup
                .Where(x =>
                    existsVacancies
                        .Any(v => v.Fingerprint == x.Fingerprint
                                  && v.SourceDomain == x.SourceDomain
                                  && v.SourceId == x.SourceId) == false
                ));

            response.Add(new MessageBroker.Messages.ScrapTask.RawSaved
            {
                OccupationGroup = downloadedGroup.Key.OccupationGroup,
                Occupation = downloadedGroup.Key.Occupation,
                Specialization = downloadedGroup.Key.Specialization,
                SkillTag = downloadedGroup.Key.SkillTag
            });
        }

        if (newVacancies.Count != 0)
        {
            await IndexLshAsync(newVacancies);

            await dbContext.RawVacancies.AddRangeAsync(newVacancies, _cancellationToken);
            await dbContext.SaveChangesAsync(_cancellationToken);
        }

#if DEBUG == false
        await Task.WhenAll(objectsKeys.Select(DeleteObjectAsync));
#endif
        
        return response;
    }

    /// <summary>
    ///     Calculates the unique fingerprints for a collection of raw vacancy data
    /// </summary>
    /// <remarks>Updates field <see cref="RawVacancy.Fingerprint" /> of all elements in <paramref name="rawVacancies" /></remarks>
    /// <param name="rawVacancies">The collection of raw vacancies for which fingerprints need to be calculated</param>
    private async Task CalculateFingerprintsAsync(IEnumerable<RawVacancy> rawVacancies)
    {
        foreach (var rawVacancy in rawVacancies)
        {
            rawVacancy.Fingerprint = await fingerprintStrategy.CalculateAsync(rawVacancy, _cancellationToken);
        }
    }

    /// <summary>
    ///     Indexes a list of raw vacancies into an LSH (Locality-Sensitive Hashing) storage for deduplication purposes
    /// </summary>
    /// <param name="rawVacancies">The collection of raw vacancy entities to be indexed</param>
    private async Task IndexLshAsync(IEnumerable<RawVacancy> rawVacancies)
    {
        // The order is not important because the LSH calculation uses the same fields as the fingerprint calculation.
        var uniqueVacancies = rawVacancies
            .GroupBy(x => x.Fingerprint)
            .Select(x => x.First());

        foreach (var vacancy in uniqueVacancies)
        {
            await lshStrategy.IndexDocumentAsync(vacancy, _cancellationToken);
        }
    }

    /// <summary>
    ///     Retrieves and processes a raw vacancy from the object storage based on the specified object key
    /// </summary>
    /// <param name="objectKey">The key of the object to retrieve from S3 storage</param>
    /// <returns>A deserialized <see cref="RawVacancy" /> instance containing some vacancy data</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be deserialized</exception>
    private async Task<RawVacancy> GetVacanciesAsync(string objectKey)
    {
        await _semaphoreSlim.WaitAsync(_cancellationToken);

        RawVacancy response;

        try
        {
            logger.LogDebug("Started processing {ObjectKey}", objectKey);

            var getObjectRequest = new GetObjectOptions
            {
                Bucket = Buckets.PagesAnalyzer,
                Path = objectKey
            };

            await using var stream = await objectStorage.GetObjectStreamAsync(getObjectRequest, _cancellationToken);

            var objectKeyParts = objectKey.Split('/');
            var downloadKey = objectKeyParts[0];
            var occupationGroup = objectKeyParts[1];
            var occupation = objectKeyParts[2];
            var specialization = objectKeyParts[3];
            var skillTag = objectKeyParts[4];
            var domain = objectKeyParts[5];
            var sourceId = objectKeyParts.Last().Split('.')[0];

            var vacancy = await JsonSerializer.DeserializeAsync<VacancyDto>(stream, _jsonSerializerOptions, _cancellationToken);
            if (vacancy == null) throw new InvalidOperationException($"Failed to deserialize file '{objectKey}'");

            response = vacancy.ToRawVacancy(downloadKey, domain, sourceId, occupationGroup, occupation, specialization, skillTag);

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

    /// <summary>
    ///     Deletes an object from the specified storage bucket
    /// </summary>
    /// <param name="objectKey">The key identifying the object to be deleted</param>
    private async Task DeleteObjectAsync(string objectKey)
    {
        await _semaphoreSlim.WaitAsync(_cancellationToken);

        try
        {
            var deleteRequest = new DeleteObjectOptions
            {
                Bucket = Buckets.PagesAnalyzer,
                Path = objectKey
            };
            await objectStorage.DeleteObjectAsync(deleteRequest, _cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete file '{ObjectKey}'", objectKey);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}