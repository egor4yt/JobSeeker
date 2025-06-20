namespace JobSeeker.Deduplication.Application.Services.Normalizer;

public class SimpleNormalizer : INormalizer
{
    public Task<string> NormalizeAsync(string text, CancellationToken cancellationToken)
    {
        var synchronousResponse = Normalize(text);
        return Task.FromResult(synchronousResponse);
    }

    public string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        return text
            .ToLowerInvariant()
            .Replace("c#", "csharp")
            .Replace("–", "-")
            .Replace("ё", "е");
    }
}