using Hangfire;
using JobSeeker.WebScraper.Application.JobParameters.Base;
using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Services.JobRunner;

[AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public class JobRunnerService(IServiceProvider serviceProvider)
{
    public async Task RunAsync<TJobParameter>(TJobParameter parameter, CancellationToken cancellationToken) where TJobParameter : IJobParameter
    {
        await using var asyncScope = serviceProvider.CreateAsyncScope();
        var job = asyncScope.ServiceProvider.GetRequiredService<IJob<TJobParameter>>();
        var logger = asyncScope.ServiceProvider.GetRequiredService<ILogger<TJobParameter>>();
        var jobClient = asyncScope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

        var scopeData = new Dictionary<string, object>
        {
            ["JobId"] = parameter.JobId,
            ["JobParameterType"] = typeof(TJobParameter)
        };

        using (logger.BeginScope(scopeData))
        {
            try
            {
                logger.LogInformation("Started job type of {JobParameterType}", job.GetType().Name);

                await job.RunAsync(parameter, cancellationToken);

                logger.LogInformation("Finished job type of {JobParameterType}", job.GetType().Name);
            }
            catch (TimeoutException e)
            {
                jobClient.Schedule(
                    () => RunAsync(parameter, CancellationToken.None),
                    TimeSpan.FromMinutes(5));
                
                logger.LogError("Job failed: {FailureReason}", e.Message);
                throw;
            }
            catch (NoAvailableProxyException e)
            {
                jobClient.Schedule(
                    () => RunAsync(parameter, CancellationToken.None),
                    TimeSpan.FromMinutes(5));
                
                logger.LogError("Job failed: {FailureReason}", e.Message);
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Job failed without restart");
                throw;
            }
        }
    }
}