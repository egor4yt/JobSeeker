namespace JobSeeker.WebScraper.Application.JobParameters.Base;

public abstract class JobParameter : IJobParameter
{
    public required Guid JobId { get; init; }
}