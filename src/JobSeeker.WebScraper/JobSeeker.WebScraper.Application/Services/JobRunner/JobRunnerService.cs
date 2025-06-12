using Hangfire;
using Hangfire.Server;
using JobSeeker.WebScraper.Application.JobParameters.Base;
using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Services.JobRunner;

[AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public class JobRunnerService(IServiceProvider serviceProvider)
{
    /// <summary>
    ///     Executes the job asynchronously using the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter for the job execution.</param>
    /// <param name="context">
    ///     The Hangfire context associated with the job execution. Must be null when scheduled, because hangfire automatically
    ///     creates it.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <typeparam name="TJobParameter">The type of the job parameter, which must implement <see cref="IJobParameter" />.</typeparam>
    public async Task RunAsync<TJobParameter>(TJobParameter parameter, PerformContext context, CancellationToken cancellationToken) where TJobParameter : IJobParameter
    {
        await using var asyncScope = serviceProvider.CreateAsyncScope();
        var job = asyncScope.ServiceProvider.GetRequiredService<IJob<TJobParameter>>();
        var logger = asyncScope.ServiceProvider.GetRequiredService<ILogger<TJobParameter>>();
        var jobClient = asyncScope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

        var scopeData = new Dictionary<string, object>
        {
            ["JobId"] = context.BackgroundJob.Id,
            ["JobParameterType"] = typeof(TJobParameter)
        };

        using (logger.BeginScope(scopeData))
        {
            Exception? exception = null;
            var delay = TimeSpan.FromSeconds(30);

            try
            {
                logger.LogInformation("Started job type of {JobParameterType}", job.GetType().Name);

                await job.RunAsync(parameter, cancellationToken);

                logger.LogInformation("Finished job type of {JobParameterType}", job.GetType().Name);
            }
            catch (TimeoutException e)
            {
                exception = e;
                delay = TimeSpan.FromMinutes(5);
            }
            catch (NoAvailableProxyException e)
            {
                exception = e;
                delay = TimeSpan.FromMinutes(5);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Job failed without restart");
                throw;
            }

            if (exception != null)
            {
                jobClient.Schedule(
                    context.BackgroundJob.Job.Queue,
                    () => RunAsync(parameter, null!, CancellationToken.None),
                    delay);

                logger.LogWarning("Job failed: {FailureReason}", exception.Message);

                throw exception;
            }
        }
    }
}