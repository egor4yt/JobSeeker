namespace JobSeeker.Deduplication.Caching.Helpers;

public static class CacheKeyHelper
{
    private const string ServiceKeyPrefix = "deduplication:";
    
    public static string GetLshCacheKey(int band, uint bucketHash)
    {
        return $"{ServiceKeyPrefix}:lsh:{band}:{bucketHash}";
    }
    
    public static string GetLshSignatureKey(string fingerprint)
    {
        return $"{ServiceKeyPrefix}:lsh:signature:{fingerprint}";
    }
}