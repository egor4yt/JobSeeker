using System.Text.RegularExpressions;
using JobSeeker.Deduplication.Application.Services.Normalizer;

namespace JobSeeker.Deduplication.Application.Services.Tokenizer;

public partial class SimpleTokenizer(INormalizer normalizer) : ITokenizer
{
    private static readonly Regex TokenRegex = CompileNonWordRegex();
    private static readonly List<string> BadWords =
    [
        "ооо",
        "ано",
        "пао",
        "оао",
        "ип",
        "и",
        "или"
    ];

    public async Task<IEnumerable<string>> TokenizeAsync(string text, CancellationToken cancellationToken)
    {
        var normalizedText = await normalizer.NormalizeAsync(text, cancellationToken);
        
        var tokens = TokenRegex
            .Matches(normalizedText)
            .Select(x => x.Value)
            .Where(x => BadWords.Contains(x) == false);

        return tokens;
    }

    public IEnumerable<string> Tokenize(string text)
    {
        var normalizedText = normalizer.Normalize(text);

        var tokens = TokenRegex
            .Matches(normalizedText)
            .Select(x => x.Value);

        return tokens;
    }

    [GeneratedRegex(@"[[\p{L}\p{Nd}]+", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CompileNonWordRegex();
}