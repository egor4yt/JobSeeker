using JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy.Models;

namespace JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy;

/// <summary>
///     Defines a strategy for analyzing HTML content and extracting vacancy information
/// </summary>
public interface IAnalyzeStrategy
{
    /// <summary>
    ///     Analyzes the provided HTML content and extracts vacancy information
    /// </summary>
    /// <param name="htmlContent">The stream containing the HTML content to analyze</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>Extracted vacancy information</returns>
    Task<Vacancy> AnalyzeAsync(Stream htmlContent, CancellationToken cancellationToken);
}