using JobSeeker.WebScraper.Application.JobParameters.Base;
using JobSeeker.WebScraper.Application.Jobs.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Services.JobRunner;

public class JobRunnerService(IServiceProvider serviceProvider)
{
    public async Task RunAsync<TJobParameter>(TJobParameter parameter, CancellationToken cancellationToken) where TJobParameter : IJobParameter
    {
        await using var asyncScope = serviceProvider.CreateAsyncScope();
        var job = asyncScope.ServiceProvider.GetRequiredService<IJob<TJobParameter>>();
        var logger = asyncScope.ServiceProvider.GetRequiredService<ILogger<TJobParameter>>();

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
            catch (Exception e)
            {
                logger.LogError("Job failed: {FailureReason}", e.Message);
                throw;
            }
        }
    }
}