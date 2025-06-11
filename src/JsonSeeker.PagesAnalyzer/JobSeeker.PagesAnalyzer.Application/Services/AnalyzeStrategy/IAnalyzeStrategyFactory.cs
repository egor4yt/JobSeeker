namespace JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy;

/// <summary>
///     Factory interface for retrieving an HTML-analyzing strategy
/// </summary>
public interface IAnalyzeStrategyFactory
{
    /// <summary>
    ///     Retrieves an implementation of <see cref="IAnalyzeStrategy" /> for the specified domain.
    /// </summary>
    /// <param name="domain">The domain name for which the corresponding analyzing strategy is needed.</param>
    /// <returns>
    ///     An implementation of <see cref="IAnalyzeStrategy" /> if a strategy for the specified domain is found; otherwise, null.
    /// </returns>
    IAnalyzeStrategy? GetStrategy(string domain);
}