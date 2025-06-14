using System.Text.Json;
using JobSeeker.Deduplication.Application.Jobs.Base;
using JobSeeker.Deduplication.Application.Jobs.Common.SaveRawAndCalculateSignatures.Models;
using JobSeeker.Deduplication.Application.Services.Fingerprints;
using JobSeeker.Deduplication.Domain.Entities;
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
    IFingerprintStrategy<RawVacancy> fingerprintStrategy
) : IJob<JobParameters.Common.SaveRawVacancies>
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

        logger.LogInformation("Started calculating signatures for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);

        await RunAsync();

        logger.LogInformation("Finished calculating signatures for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);
    }

    private async Task RunAsync()
    {
        var request = new GetAllObjectsOptions
        {
            Bucket = Buckets.PagesAnalyzer,
            Path = $"{_parameter.ScrapTaskId}"
        };

        var objectsKeys = await objectStorage.GetAllObjectsRecursiveAsync(request, _cancellationToken);
        if (objectsKeys.Count == 0) return;


        var rawVacancies = await Task.WhenAll(objectsKeys.Select(GetVacanciesAsync));

        foreach (var rawVacancy in rawVacancies)
        {
            rawVacancy.Fingerprint = await fingerprintStrategy.CalculateAsync(rawVacancy, _cancellationToken);
        }

        var existsFingerprints = await dbContext.RawVacancies
            .Where(x => rawVacancies.Select(y => y.Fingerprint).Distinct().Contains(x.Fingerprint))
            .Select(x => x.Fingerprint)
            .ToListAsync(_cancellationToken);

        var newRawVacancies = rawVacancies.Where(x => existsFingerprints.Contains(x.Fingerprint) == false);

        await dbContext.RawVacancies.AddRangeAsync(newRawVacancies, _cancellationToken);
        await dbContext.SaveChangesAsync(_cancellationToken);

        await Task.WhenAll(objectsKeys.Select(DeleteObjectAsync));
    }

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
            var domain = objectKeyParts[5];

            var vacancy = await JsonSerializer.DeserializeAsync<VacancyDto>(stream, _jsonSerializerOptions, _cancellationToken);
            if (vacancy == null) throw new InvalidOperationException($"Failed to deserialize file '{objectKey}'");

            response = vacancy.ToRawVacancy(downloadKey, domain);

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