using System.Text.Json;
using JobSeeker.PagesAnalyzer.Application.Jobs.Base;
using JobSeeker.PagesAnalyzer.Application.Jobs.Common.AnalyzeScrapTaskResults.Models;
using JobSeeker.PagesAnalyzer.MessageBroker.Producers;
using JobSeeker.PagesAnalyzer.ObjectStorage;
using JobSeeker.PagesAnalyzer.ObjectStorage.Models;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.Application.Jobs.Common.AnalyzeScrapTaskResults;

public partial class AnalyzeScrapTaskResultsJob(
    ILogger<AnalyzeScrapTaskResultsJob> logger,
    IObjectStorage objectStorage,
    IMessageProducer<MessageBroker.Messages.ScrapTask.Analyzed> producer
) : IJob<JobParameters.Common.AnalyzeScrapTaskResults>
{
    /// <summary>
    ///     Maximum number of object keys that can be processed in parallel in a single chunk
    /// </summary>
    private const int MaxChunkSize = 5;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(MaxChunkSize, MaxChunkSize);

    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.AnalyzeScrapTaskResults _parameter = null!;

    public async Task RunAsync(JobParameters.Common.AnalyzeScrapTaskResults parameter, CancellationToken cancellationToken)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogInformation("Started analyzing scrap task results for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);

        await RunAsync();

        logger.LogInformation("Finished analyzing scrap task results for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);

        var message = new MessageBroker.Messages.ScrapTask.Analyzed
        {
            ScrapTaskId = _parameter.ScrapTaskId
        };
        await producer.ProduceAsync(message, _cancellationToken);
    }

    private async Task RunAsync()
    {
        var request = new GetAllObjectsOptions
        {
            Bucket = Buckets.WebScraper,
            Path = $"{_parameter.ScrapTaskId}"
        };

        var objectsKeys = await objectStorage.GetAllObjectsRecursiveAsync(request, _cancellationToken);
        if (objectsKeys.Count != 0) await Task.WhenAll(objectsKeys.Select(ProcessObjectAsync));
    }

    private async Task ProcessObjectAsync(string objectKey)
    {
        await _semaphoreSlim.WaitAsync(_cancellationToken);

        try
        {
            logger.LogDebug("Started analyzing {ObjectKey}", objectKey);

            var getObjectRequest = new GetObjectOptions
            {
                Bucket = Buckets.WebScraper,
                Path = objectKey
            };

            await using var stream = await objectStorage.GetObjectStreamAsync(getObjectRequest, _cancellationToken);

            var objectKeyParts = objectKey.Split('/');
            var domain = objectKeyParts[1];
            Vacancy vacancy;

            if (domain.EndsWith("hh.ru")) vacancy = await GetHeadHunterVacancyAsync(stream);
            // else if (domain.EndsWith("other-domain.com")) newResponseItem = await GetOtherDomainVacancyAsync(scrapTask);
            else
            {
                logger.LogWarning("Unsupported domain {Domain}", domain);
                return;
            }

            vacancy.Url = "https://" + string.Join('/', objectKeyParts.Skip(1))[..^5];

            await UploadVacanciesDetailsAsync(vacancy, objectKeyParts);

            var deleteRequest = new DeleteObjectOptions
            {
                Bucket = Buckets.WebScraper,
                Path = objectKey
            };
            await objectStorage.DeleteObjectAsync(deleteRequest, _cancellationToken);

            logger.LogDebug("Finished analyzing {ObjectKey}", objectKey);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to analyze file {ObjectKey}", objectKey);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task UploadVacanciesDetailsAsync(Vacancy vacancy, string[] pathParts)
    {
        using var fileStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(fileStream, vacancy, _jsonSerializerOptions, _cancellationToken);
        fileStream.Position = 0;

        var putObjectRequest = new PutObjectOptions
        {
            Path = $"{string.Join('/', pathParts.Take(pathParts.Length - 1))}",
            Bucket = Buckets.PagesAnalyzer,
            FileName = $"{pathParts.Last()[..^5]}.json",
            ContentType = "text/json",
            FileStream = fileStream
        };
        await objectStorage.PutObjectAsync(putObjectRequest, _cancellationToken);
    }
}