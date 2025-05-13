using Amazon.S3;
using Amazon.S3.Model;

namespace JobSeeker.WebScraper.ObjectStorage;

public class MinioStorage(IAmazonS3 client) : IObjectStorage
{
    public async Task PutObjectAsync(string bucket, string path, string fileName, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            await client.HeadBucketAsync(new HeadBucketRequest { BucketName = bucket }, cancellationToken);
        }
        catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await client.PutBucketAsync(new PutBucketRequest { BucketName = bucket }, cancellationToken);
        }

        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = $"{path}/{fileName}",
            InputStream = stream
        };

        await client.PutObjectAsync(request, cancellationToken);
    }
}