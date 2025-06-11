using System.Diagnostics;
using System.Text;
using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.MessageBroker.Producers;
using JobSeeker.WebScraper.ObjectStorage;
using JobSeeker.WebScraper.ObjectStorage.Models;
using JobSeeker.WebScraper.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.UploadSearchResults;

public class UploadSearchResultsJob(
    ILogger<UploadSearchResultsJob> logger,
    ApplicationDbContext dbContext,
    PlaywrightFactoryService playwrightFactory,
    IObjectStorage objectStorage,
    IMessageProducer<MessageBroker.Messages.ScrapTask.Completed> producer) : IJob<JobParameters.Common.UploadSearchResults>
{
    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.UploadSearchResults _parameter = null!;

    public async Task RunAsync(JobParameters.Common.UploadSearchResults parameter, CancellationToken cancellationToken = default)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogInformation("Started uploading scrap task results {ScrapTaskId}", _parameter.ScrapTaskId);

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        await RunAsync();
        stopWatch.Stop();

        logger.LogInformation("Finished uploading scrap task results {ScrapTaskId} for {ElapsedMilliseconds}ms", _parameter.ScrapTaskId, stopWatch.ElapsedMilliseconds);

        var message = new MessageBroker.Messages.ScrapTask.Completed
        {
            ScrapTaskId = _parameter.ScrapTaskId
        };
        await producer.ProduceAsync(message, _cancellationToken);
    }

    private async Task RunAsync()
    {
        var scrapTaskResults = await dbContext.ScrapTaskResults
            .Where(x => x.ScrapTaskId == _parameter.ScrapTaskId
                        && x.Uploaded == false)
            .ToListAsync(_cancellationToken);
        if (scrapTaskResults.Count == 0) return;

        var groups = scrapTaskResults
            .GroupBy(x => new Uri(x.Link).Host)
            .Select(x => new
            {
                Domain = x.Key,
                Chunks = x.Chunk(10)
            });
        foreach (var grouping in groups)
        {
            var session = await playwrightFactory.CreateSessionAsync(grouping.Domain, _cancellationToken);

            foreach (var chunk in grouping.Chunks)
            {
                var tasks = chunk.Select(async scrapTaskResult =>
                {
                    Stream pageContent;
                    try
                    {
                        pageContent = await DownloadContentAsync(scrapTaskResult.Link, session);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to download page {ScrapTaskResultId}", scrapTaskResult.Id);
                        throw;
                    }

                    try
                    {
                        var uri = new Uri(scrapTaskResult.Link);
                        var pathParts = uri.LocalPath
                            .Split('/', StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

                        var fileOptions = new PutObjectOptions
                        {
                            Bucket = Buckets.WebScraper,
                            Path = $"{_parameter.ScrapTaskId}/{uri.Host}/{string.Join('/', pathParts.Take(pathParts.Count - 1))}",
                            FileName = $"{pathParts.Last()}.html",
                            ContentType = "text/html; charset=utf-8",
                            FileStream = pageContent
                        };

                        await objectStorage.PutObjectAsync(fileOptions, _cancellationToken);
                        logger.LogDebug("Vacancy {VacancyFileName} of {Domain} uploaded", pathParts.Last(), uri.Host);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to upload scrap task result {ScrapTaskResultId}", scrapTaskResult.Id);
                        throw;
                    }

                    scrapTaskResult.Uploaded = true;
                    dbContext.ScrapTaskResults.Update(scrapTaskResult);
                });
                await Task.WhenAll(tasks);
                await dbContext.SaveChangesAsync(_cancellationToken);
            }

            await session.DisposeAsync();
        }
    }

    private async Task<Stream> DownloadContentAsync(string url, PlaywrightSession session)
    {
        var page = await session.LoadPageAsync(url, _cancellationToken);
        var bytes = Encoding.UTF8.GetBytes(await page.ContentAsync());
        return new MemoryStream(bytes);
    }
}