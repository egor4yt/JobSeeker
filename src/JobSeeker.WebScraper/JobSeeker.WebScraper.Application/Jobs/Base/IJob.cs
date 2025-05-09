using JobSeeker.WebScraper.Application.JobParameters.Base;

namespace JobSeeker.WebScraper.Application.Jobs.Base;

public interface IJob<in TParameter> where TParameter : IJobParameter
{
    Task RunAsync(TParameter parameter, CancellationToken cancellationToken = default);
}