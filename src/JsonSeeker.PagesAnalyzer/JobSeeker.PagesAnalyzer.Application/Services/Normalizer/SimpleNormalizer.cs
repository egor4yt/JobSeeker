using System.Text.RegularExpressions;

namespace JobSeeker.PagesAnalyzer.Application.Services.Normalizer;

public partial class SimpleNormalizer : INormalizer
{
    private static readonly Regex MultipleWhiteSpaces = MultipleWhiteSpacesRegex();


    public Task<string> NormalizeAsync(string text, CancellationToken cancellationToken)
    {
        var synchronousResponse = Normalize(text);
        return Task.FromResult(synchronousResponse);
    }

    public string Normalize(string text)
    {
        return string.IsNullOrWhiteSpace(text) 
            ? string.Empty 
            : MultipleWhiteSpaces.Replace(text.Trim(), " ");
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultipleWhiteSpacesRegex();
}