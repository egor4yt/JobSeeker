using JobSeeker.WebApi.Application.JobParameters.Base;

namespace JobSeeker.WebApi.Application.Jobs.Base;

public interface IJob<in TParameter> where TParameter : IJobParameter
{
    Task RunAsync(TParameter parameter, CancellationToken cancellationToken = default);
}