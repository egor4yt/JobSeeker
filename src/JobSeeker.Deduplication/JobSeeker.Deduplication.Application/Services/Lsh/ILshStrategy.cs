namespace JobSeeker.Deduplication.Application.Services.Lsh;

/// <summary>
///     Strategy for performing Locality-Sensitive Hashing (LSH) for deduplication or similarity detection.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be processed by the LSH strategy</typeparam>
public interface ILshStrategy<TEntity>
{
    /// <summary>
    ///     Number of bands used in a Locality-Sensitive Hashing (LSH) strategy
    ///     This value determines the division of the signature matrix into bands, where each band is compared to detect similarity between
    ///     entities
    /// </summary>
    int Bands { get; init; }

    /// <summary>
    ///     Number of rows in each band, used for splitting MinHash signatures into bands
    /// </summary>
    int RowsPerBand { get; init; }

    /// <summary>
    ///     Asynchronously indexes the given document for locality-sensitive hashing (LSH) by computing signatures
    ///     and storing them in cache for similarity detection or deduplication
    /// </summary>
    /// <param name="entity">The entity to be indexed in the LSH storage</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task<uint[]> IndexDocumentAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously computes similarity candidates for the provided entity based on its locality-sensitive hashing (LSH) signature
    ///     by retrieving possible matches from cached signatures and calculating similarity scores
    /// </summary>
    /// <param name="entity">The entity for which to identify similarity candidates</param>
    /// <param name="potentialCandidates">A list of prefiltered potential candidates to evaluate</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A dictionary containing candidate identifiers as keys and their corresponding similarity scores as values</returns>
    Task<Dictionary<string, double>> GetCandidatesAsync(TEntity entity, IList<TEntity> potentialCandidates, CancellationToken cancellationToken);
}