using System.Collections.Concurrent;
using Amazon.S3;
using Amazon.S3.Model;
using JobSeeker.PagesAnalyzer.ObjectStorage.Models;

namespace JobSeeker.PagesAnalyzer.ObjectStorage;

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

    public async Task<List<string>> GetAllObjectsRecursiveAsync(GetAllObjectsOptions options, CancellationToken cancellationToken = default)
    {
        var response = new List<string>();
        string? continuationToken = null;

        do
        {
            var request = new ListObjectsV2Request
            {
                BucketName = options.Bucket,
                ContinuationToken = continuationToken,
                Prefix = options.Path
            };

            var result = await client.ListObjectsV2Async(request, cancellationToken);
            if (result.KeyCount != 0) response.AddRange(result.S3Objects.Select(x => x.Key));

            if (result.IsTruncated.HasValue == false) break;
            continuationToken = result.IsTruncated.Value ? result.NextContinuationToken : null;
        } while (continuationToken != null);


        return response;
    }

    public async Task<Stream> GetObjectStreamAsync(GetObjectOptions options, CancellationToken cancellationToken = default)
    {
        return await client.GetObjectStreamAsync(options.Bucket, options.Path, null, cancellationToken);
    }

    public async Task DeleteObjectAsync(DeleteObjectOptions options, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = options.Bucket,
            Key = options.Path
        };
        await client.DeleteObjectAsync(request, cancellationToken);
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