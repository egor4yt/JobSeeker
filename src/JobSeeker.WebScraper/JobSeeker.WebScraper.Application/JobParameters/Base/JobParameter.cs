namespace JobSeeker.WebScraper.Application.JobParameters.Base;

public abstract class JobParameter : IJobParameter
{
    public Guid JobId { get; init; } = Guid.NewGuid();
}