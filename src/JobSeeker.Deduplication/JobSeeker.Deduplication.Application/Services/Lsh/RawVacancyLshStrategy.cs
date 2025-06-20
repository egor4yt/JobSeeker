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
    private const double JaccardThreshold = 0.5;
    private readonly MinHashHelper _minHashHelper = new MinHashHelper();
    private readonly IDatabase _redisDb = connectionMultiplexer.GetDatabase();

    public int Bands { get; init; } = 32;
    public int RowsPerBand { get; init; } = 4;

    public async Task<uint[]> IndexDocumentAsync(TRawVacancy entity, CancellationToken cancellationToken)
    {
        var tokens = (await tokenizer.TokenizeAsync(entity.Description, cancellationToken)).ToArray();
        var shinglesHashes = ShingleHelper.GetShinglesHashes(tokens).ToHashSet();
        var signature = _minHashHelper.ComputeSignatures(shinglesHashes);

        var signatureCacheKey = CacheKeyHelper.GetLshSignatureKey(entity.Fingerprint);
        var serializedSignature = SerializationHelper.SerializeJson(signature);

        var batch = _redisDb.CreateBatch();
        var batchTasks = new List<Task<bool>>();
        batchTasks.Add(batch.StringSetAsync(signatureCacheKey, serializedSignature, TimeSpan.FromDays(30)));

        if (signature.Length != Bands * RowsPerBand)
            throw new ArgumentException("Signature length mismatch");

        for (var band = 0; band < Bands; band++)
        {
            uint bandHash = 0;
            for (var r = 0; r < RowsPerBand; r++)
            {
                bandHash = HashCombine(bandHash, signature[band * RowsPerBand + r]);
            }

            var cacheKey = CacheKeyHelper.GetLshCacheKey(band, bandHash);
            batchTasks.Add(batch.SetAddAsync(cacheKey, entity.Fingerprint));
            batchTasks.Add(batch.KeyExpireAsync(cacheKey, TimeSpan.FromDays(7)));
        }

        batch.Execute();
        await Task.WhenAll(batchTasks);
        return signature;
    }

    public async Task<Dictionary<string, double>> GetCandidatesAsync(TRawVacancy entity, IList<TRawVacancy> potentialCandidates, CancellationToken cancellationToken)
    {
        var response = new Dictionary<string, double>();
        var signatureCacheKey = CacheKeyHelper.GetLshSignatureKey(entity.Fingerprint);
        var serializedSignature = await _redisDb.StringGetAsync(signatureCacheKey);

        uint[] signature;
        if (serializedSignature.HasValue == false)
            signature = await IndexDocumentAsync(entity, cancellationToken);
        else
            signature = SerializationHelper.DeserializeJson<uint[]>(serializedSignature!) ?? throw new NullReferenceException("Invalid signature");

        if (signature.Length != Bands * RowsPerBand)
            throw new ArgumentException("Signature length mismatch");

        HashSet<string> candidateIds = [];
        for (var band = 0; band < Bands; band++)
        {
            uint bandHash = 0;
            for (var r = 0; r < RowsPerBand; r++)
            {
                bandHash = HashCombine(bandHash, signature[band * RowsPerBand + r]);
            }

            var cacheKey = CacheKeyHelper.GetLshCacheKey(band, bandHash);
            var setMembers = await _redisDb.SetMembersAsync(cacheKey);

            foreach (var setMember in setMembers)
            {
                if (setMember != entity.Fingerprint && potentialCandidates.Any(x => x.Fingerprint == setMember))
                    candidateIds.Add(setMember!);
            }
        }

        if (candidateIds.Count == 0)
            return [];

        var signaturesKeys = candidateIds
            .Select(x => new RedisKey(CacheKeyHelper.GetLshSignatureKey(x)))
            .ToArray();

        var otherSignatures = await _redisDb.StringGetAsync(signaturesKeys);
        for (var i = 0; i < signaturesKeys.Length; i++)
        {
            if (otherSignatures[i].HasValue == false) continue;

            var otherSignature = SerializationHelper.DeserializeJson<uint[]>(otherSignatures[i]!) ?? throw new NullReferenceException("Invalid signature");
            var approxJaccard = MinHashHelper.JaccardSimilarity(signature, otherSignature);

            if (approxJaccard >= JaccardThreshold)
                response.Add(candidateIds.ElementAt(i), approxJaccard);
        }

        return response;
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