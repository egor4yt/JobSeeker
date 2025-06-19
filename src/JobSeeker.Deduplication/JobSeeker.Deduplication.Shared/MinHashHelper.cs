namespace JobSeeker.Deduplication.Shared;

public class MinHashHelper
{
    /// <summary>
    ///     Prime number used as a multiplier to generate hash seeds.
    ///     Selected for good distribution properties in hash functions.
    /// </summary>
    private const uint HashMultiplier = 374761393u;

    /// <summary>
    ///     Magic constant used as an offset in hash seed generation.
    ///     Derived from the golden ratio ((sqrt(5)-1)/2) * 2^32.
    ///     Helps avoid collisions and provides better distribution.
    /// </summary>
    private const uint GoldenRatioHash = 0x9E3779B9u;

    /// <summary>
    ///     Array of precomputed hash seeds used in MinHash computation.
    ///     Derived using hash count, a multiplier, and a constant offset for consistent and distributed hash value generation.
    /// </summary>
    private readonly uint[] _seeds;

    public MinHashHelper(int hashCount = 128)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hashCount);

        _seeds = Enumerable.Range(0, hashCount)
            .Select(i => (uint)(i * HashMultiplier + GoldenRatioHash))
            .ToArray();
    }

    /// <summary>
    ///     Computes a MinHash signature for the provided set of shingles.
    /// </summary>
    /// <param name="shingles">A set of precomputed hashes representing the shingles of a document.</param>
    /// <returns>An array of unsigned 32-bit integers representing the MinHash signature.</returns>
    public uint[] ComputeSignatures(HashSet<uint> shingles)
    {
        var signature = new uint[_seeds.Length];
        Array.Fill(signature, uint.MaxValue);

        foreach (var shingleHash in shingles)
        {
            for (var i = 0; i < _seeds.Length; i++)
            {
                var combined = Combine(shingleHash, _seeds[i]);
                if (combined < signature[i]) signature[i] = combined;
            }
        }

        return signature;
    }

    /// <summary>
    ///     Simple custom hashing algorithm based on Murmur.
    /// </summary>
    /// <param name="value">The original 32-bit integer value to be combined.</param>
    /// <param name="seed">The seed value used to vary the hash output.</param>
    /// <returns>A 32-bit unsigned integer representing the new hash value.</returns>
    private static uint Combine(uint value, uint seed)
    {
        unchecked
        {
            var h1 = value ^ seed;

            h1 *= 0x85ebca6b;
            h1 ^= h1 >> 13;
            h1 *= 0xc2b2ae35;
            h1 ^= h1 >> 16;

            return h1;
        }
    }
}