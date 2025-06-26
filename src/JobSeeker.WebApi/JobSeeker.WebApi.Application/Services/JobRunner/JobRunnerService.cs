using Hangfire;
using Hangfire.Server;
using JobSeeker.WebApi.Application.JobParameters.Base;
using JobSeeker.WebApi.Application.Jobs.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebApi.Application.Services.JobRunner;

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
            try
            {
                logger.LogInformation("Started job type of {JobParameterType}", job.GetType().Name);

                await job.RunAsync(parameter, cancellationToken);

                logger.LogInformation("Finished job type of {JobParameterType}", job.GetType().Name);
            }
            catch (TimeoutException e)
            {
                jobClient.Schedule(
                    () => RunAsync(parameter, null!, CancellationToken.None),
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