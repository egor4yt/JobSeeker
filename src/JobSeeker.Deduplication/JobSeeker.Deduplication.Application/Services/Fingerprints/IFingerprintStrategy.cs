namespace JobSeeker.Deduplication.Application.Services.Fingerprints;

public interface IFingerprintStrategy<in TEntity>
{
    Task<string> CalculateAsync(TEntity entity, CancellationToken cancellationToken);
}