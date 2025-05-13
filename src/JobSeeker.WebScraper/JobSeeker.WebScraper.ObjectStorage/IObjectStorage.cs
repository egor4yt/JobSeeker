namespace JobSeeker.WebScraper.ObjectStorage;

public interface IObjectStorage
{
    Task PutObjectAsync(string bucket, string path, string fileName, Stream bytes, CancellationToken cancellationToken = default);
}