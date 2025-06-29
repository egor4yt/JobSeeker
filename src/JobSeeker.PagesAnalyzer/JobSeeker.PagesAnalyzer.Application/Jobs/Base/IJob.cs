using JobSeeker.PagesAnalyzer.Application.JobParameters.Base;

namespace JobSeeker.PagesAnalyzer.Application.Jobs.Base;

public interface IJob<in TParameter> where TParameter : IJobParameter
{
    Task RunAsync(TParameter parameter, CancellationToken cancellationToken = default);
}