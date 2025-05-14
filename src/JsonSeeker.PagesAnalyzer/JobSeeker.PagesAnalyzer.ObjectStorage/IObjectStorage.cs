using JobSeeker.PagesAnalyzer.ObjectStorage.Models;

namespace JobSeeker.PagesAnalyzer.ObjectStorage;

public interface IObjectStorage
{
    Task PutObjectAsync(PutObjectOptions options, CancellationToken cancellationToken = default);
}