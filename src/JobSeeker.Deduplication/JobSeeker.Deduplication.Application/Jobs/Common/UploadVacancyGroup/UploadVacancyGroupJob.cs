using System.Text.Json;
using JobSeeker.Deduplication.Application.Jobs.Base;
using JobSeeker.Deduplication.Application.Jobs.Common.UploadVacancyGroup.Models;
using JobSeeker.Deduplication.Domain.Entities;
using JobSeeker.Deduplication.MessageBroker.Producers;
using JobSeeker.Deduplication.ObjectStorage;
using JobSeeker.Deduplication.ObjectStorage.Models;
using JobSeeker.Deduplication.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.Application.Jobs.Common.UploadVacancyGroup;

public class UploadVacancyGroupJob(
    ILogger<UploadVacancyGroupJob> logger,
    IObjectStorage objectStorage,
    ApplicationDbContext dbContext,
    IMessageProducer<MessageBroker.Messages.DeduplicatedVacancy.Uploaded> producer) : IJob<JobParameters.Common.UploadVacancyGroup>
{
    /// <summary>
    ///     Maximum number of object keys that can be processed in parallel in a single chunk
    /// </summary>
    private const int MaxChunkSize = 5;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(MaxChunkSize, MaxChunkSize);

    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.UploadVacancyGroup _parameter = null!;

    public async Task RunAsync(JobParameters.Common.UploadVacancyGroup parameter, CancellationToken cancellationToken)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogDebug("Started uploading deduplicated vacancy group {@Parameter}", _parameter);

        await RunAsync();

        logger.LogDebug("Finished uploading deduplicated vacancy group {@Parameter}", _parameter);
        
        var message = new MessageBroker.Messages.DeduplicatedVacancy.Uploaded
        {
            OccupationGroup = _parameter.OccupationGroup,
            Occupation = _parameter.Occupation,
            Specialization = _parameter.Specialization,
            SkillTag = _parameter.SkillTag
        };
        await producer.ProduceAsync(message, _cancellationToken);
    }

    private async Task RunAsync()
    {
        var rawVacancies = await dbContext.RawVacancies
            .AsNoTracking()
            .Where(x => x.OccupationGroup == _parameter.OccupationGroup
                        && x.Occupation == _parameter.Occupation
                        && x.Specialization == _parameter.Specialization
                        && x.SkillTag == _parameter.SkillTag
                        && x.CreatedAt > DateTime.UtcNow.AddDays(-30)
                        && x.DeduplicationCompleted)
            .OrderBy(x => x.CreatedAt)
            .ToDictionaryAsync(x => x.Id, x => x, _cancellationToken);

        var uploadObjects = GetVacancies(rawVacancies);
        var uploadTasks = uploadObjects.Select((obj, index) => UploadVacancyAsync(obj.Value, index));
        await Task.WhenAll(uploadTasks);
    }

    private static Dictionary<int, VacancyDto> GetVacancies(IDictionary<int, RawVacancy> rawVacancies)
    {
        // originalVacancyId : vacancyDto
        var response = new Dictionary<int, VacancyDto>();

        foreach (var (currentVacancyId, currentVacancy) in rawVacancies)
        {
            var originalVacancyId = currentVacancy.OriginalRawVacancyId ?? currentVacancyId;

            if (response.TryGetValue(originalVacancyId, out var vacancyDto) == false)
            {
                var originalVacancy = rawVacancies[originalVacancyId];
                vacancyDto = new VacancyDto(originalVacancy);
                response.Add(originalVacancyId, vacancyDto);
            }

            if (originalVacancyId != currentVacancyId)
            {
                var newSourceDto = new SourceDto(currentVacancy);
                vacancyDto.Sources.Add(newSourceDto);
            }
        }

        return response;
    }

    private async Task UploadVacancyAsync(VacancyDto vacancyDto, int fileId)
    {
        await _semaphoreSlim.WaitAsync(_cancellationToken);

        try
        {
            using var fileStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(fileStream, vacancyDto, _jsonSerializerOptions, _cancellationToken);
            fileStream.Position = 0;

            var putObjectRequest = new PutObjectOptions
            {
                Path = vacancyDto.ObjectPath,
                Bucket = Buckets.Deduplication,
                FileName = $"{fileId}.json",
                ContentType = "text/json",
                FileStream = fileStream
            };
            await objectStorage.PutObjectAsync(putObjectRequest, _cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to upload vacancy {@VacancyDto}", vacancyDto);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}