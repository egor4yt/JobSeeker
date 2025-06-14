namespace JobSeeker.Deduplication.Application.Services.Tokenizer;

/// <summary>
///     Interface that provides functionality for tokenizing text input into a sequence of tokens
/// </summary>
public interface ITokenizer
{
    /// <summary>
    ///     Asynchronously tokenizes a given text string into a collection of strings
    /// </summary>
    /// <param name="text">The input text to be tokenized</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>A collection of tokenized strings</returns>
    Task<IEnumerable<string>> TokenizeAsync(string text, CancellationToken cancellationToken);

    /// <summary>
    ///     Synchronously tokenizes a given text string into a collection of strings
    /// </summary>
    /// <param name="text">The input text to be tokenized</param>
    /// <returns>A collection of tokenized strings</returns>
    IEnumerable<string> Tokenize(string text);
}