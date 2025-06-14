namespace JobSeeker.Deduplication.Application.Services.Fingerprints;

public interface IFingerprintStrategy<TEntity>
{
    Task<string> CalculateAsync(TEntity entity, CancellationToken cancellationToken);
}