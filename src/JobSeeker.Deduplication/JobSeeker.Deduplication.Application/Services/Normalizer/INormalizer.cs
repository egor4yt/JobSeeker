namespace JobSeeker.Deduplication.Application.Services.Normalizer;

/// <summary>
///     Interface for normalizing text input
/// </summary>
public interface INormalizer
{
    /// <summary>
    ///     Asynchronously normalizes the input text by performing predefined transformations such as lowercasing,
    ///     replacing specific characters or sequences, and removing certain words
    /// </summary>
    /// <param name="text">The input text to be normalized</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>A normalized version of the input text as a string</returns>
    Task<string> NormalizeAsync(string text, CancellationToken cancellationToken);

    /// <summary>
    ///     Synchronously normalizes the input text by performing predefined transformations such as lowercasing,
    ///     replacing specific characters or sequences, and removing certain words
    /// </summary>
    /// <param name="text">The input text to be normalized</param>
    /// <returns>A normalized version of the input text as a string</returns>
    string Normalize(string text);
}