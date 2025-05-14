using JobSeeker.WebScraper.ObjectStorage.Models;

namespace JobSeeker.WebScraper.ObjectStorage;

public interface IObjectStorage
{
    Task PutObjectAsync(PutObjectOptions options, CancellationToken cancellationToken = default);
}