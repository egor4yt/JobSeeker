using MurmurHash.Net;

namespace JobSeeker.Deduplication.Shared;

public static class ShingleHelper
{
    /// <summary>
    ///     The proprietary value used by the creator of the algorithm in the reference implementation of MurmurHash 3.
    ///     The constant was chosen empirically: it "mixes well" the initial state, does not generate obvious collisions on small inputs
    ///     and gives a full avalanche effect.
    /// </summary>
    private const uint ShingleHashSeed = 0xc58f1a7b;

    public static IEnumerable<uint> GetShinglesHashes(string[] tokens, int shinglesLength = 3)
    {
        for (var i = 0; i <= tokens.Length - shinglesLength; i++)
        {
            var shingle = string.Join(' ', tokens, i, shinglesLength);
            var bytes = System.Text.Encoding.UTF8.GetBytes(shingle);
            var hash = MurmurHash3.Hash32(bytes, ShingleHashSeed);

            yield return hash;
        }
    }
}