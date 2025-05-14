using Amazon.S3;
using Amazon.S3.Model;
using JobSeeker.PagesAnalyzer.ObjectStorage.Models;

namespace JobSeeker.PagesAnalyzer.ObjectStorage;

public class MinioStorage(IAmazonS3 client) : IObjectStorage
{
    public async Task PutObjectAsync(PutObjectOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await client.HeadBucketAsync(new HeadBucketRequest { BucketName = options.Bucket }, cancellationToken);
        }
        catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await client.PutBucketAsync(new PutBucketRequest { BucketName = options.Bucket }, cancellationToken);
        }

        var request = new PutObjectRequest
        {
            BucketName = options.Bucket,
            Key = $"{options.Path}/{options.FileName}",
            InputStream = options.FileStream,
            ContentType = options.ContentType
        };

        await client.PutObjectAsync(request, cancellationToken);
    }
}