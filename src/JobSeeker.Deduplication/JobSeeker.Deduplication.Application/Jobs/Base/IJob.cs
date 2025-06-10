using JobSeeker.Deduplication.Application.JobParameters.Base;

namespace JobSeeker.Deduplication.Application.Jobs.Base;

public interface IJob<in TParameter> where TParameter : IJobParameter
{
    Task RunAsync(TParameter parameter, CancellationToken cancellationToken = default);
}