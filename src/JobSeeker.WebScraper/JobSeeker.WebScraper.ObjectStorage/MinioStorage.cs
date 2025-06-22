using System.Collections.Concurrent;
using Amazon.S3;
using Amazon.S3.Model;
using JobSeeker.WebScraper.ObjectStorage.Models;

namespace JobSeeker.WebScraper.ObjectStorage;

public class MinioStorage(IAmazonS3 client) : IObjectStorage
{
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    private static readonly ConcurrentDictionary<string, bool> ExistsBuckets = [];

    public async Task PutObjectAsync(PutObjectOptions options, CancellationToken cancellationToken)
    {
        await TryCreateBucket(options.Bucket, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = options.Bucket,
            Key = $"{options.Path}/{options.FileName}",
            InputStream = options.FileStream,
            ContentType = options.ContentType
        };

        await client.PutObjectAsync(request, cancellationToken);
    }

    private async Task TryCreateBucket(string bucketName, CancellationToken cancellationToken)
    {
        if (ExistsBuckets.TryGetValue(bucketName, out var bucketExists) && bucketExists) return;

        await Semaphore.WaitAsync(cancellationToken);

        try
        {
            await client.HeadBucketAsync(new HeadBucketRequest { BucketName = bucketName }, cancellationToken);
            ExistsBuckets.TryAdd(bucketName, true);
        }
        catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, cancellationToken);
            ExistsBuckets.TryAdd(bucketName, true);
        }
        catch
        {
            ExistsBuckets.TryAdd(bucketName, false);
            throw;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}