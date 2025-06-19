using JobSeeker.Deduplication.Application.Services.Tokenizer;
using JobSeeker.Deduplication.Caching.Helpers;
using JobSeeker.Deduplication.Domain.Entities;
using JobSeeker.Deduplication.Shared;
using StackExchange.Redis;

namespace JobSeeker.Deduplication.Application.Services.Lsh;

/// <summary>
///     Implementation of the ILshStrategy interface for processing and indexing raw vacancy entities using
///     locality-sensitive hashing (LSH)
/// </summary>
public class RawVacancyLshStrategy<TRawVacancy>(ITokenizer tokenizer, IConnectionMultiplexer connectionMultiplexer) : ILshStrategy<TRawVacancy> where TRawVacancy : RawVacancy
{
    private readonly MinHashHelper _minHashHelper = new MinHashHelper();
    private readonly IDatabase _redisDb = connectionMultiplexer.GetDatabase();

    public int Bands { get; init; } = 32;
    public int RowsPerBand { get; init; } = 4;

    public async Task IndexDocumentAsync(TRawVacancy entity, CancellationToken cancellationToken)
    {
        var tokens = (await tokenizer.TokenizeAsync(entity.Description, cancellationToken)).ToArray();
        var shinglesHashes = ShingleHelper.GetShinglesHashes(tokens).ToHashSet();
        var signatures = _minHashHelper.ComputeSignatures(shinglesHashes);

        var signatureCacheKey = CacheKeyHelper.GetLshSignatureKey(entity.Fingerprint);
        var serializedSignature = SerializationHelper.SerializeJson(signatures);

        var batch = _redisDb.CreateBatch();
        var batchTasks = new List<Task<bool>>();
        batchTasks.Add(batch.StringSetAsync(signatureCacheKey, serializedSignature, TimeSpan.FromDays(30)));

        if (signatures.Length != Bands * RowsPerBand)
            throw new ArgumentException("Signature length mismatch");

        for (var band = 0; band < Bands; band++)
        {
            uint bandHash = 0;
            for (var r = 0; r < RowsPerBand; r++)
            {
                bandHash = HashCombine(bandHash, signatures[band * RowsPerBand + r]);
            }

            var cacheKey = CacheKeyHelper.GetLshCacheKey(band, bandHash);
            batchTasks.Add(batch.SetAddAsync(cacheKey, entity.Fingerprint));
            batchTasks.Add(batch.KeyExpireAsync(cacheKey, TimeSpan.FromDays(7)));
        }

        batch.Execute();
        await Task.WhenAll(batchTasks);
    }

    /// <summary>
    ///     Combines two hash values into a single hash value
    /// </summary>
    /// <param name="h1">The first hash value</param>
    /// <param name="h2">The second hash value</param>
    /// <returns>The combined hash value</returns>
    private static uint HashCombine(uint h1, uint h2)
    {
        unchecked
        {
            var hash = 0x811C9DC5;

            hash = (hash ^ h1) * 0x01000193;
            hash = (hash ^ h2) * 0x01000193;

            return hash;
        }
    }
}