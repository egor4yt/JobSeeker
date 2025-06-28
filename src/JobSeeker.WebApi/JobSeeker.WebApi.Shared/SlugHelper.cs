using System.Text;
using System.Text.RegularExpressions;

namespace JobSeeker.WebApi.Shared;

public static partial class SlugHelper
{
    private static readonly Dictionary<char, string> RussianTransliterationMap = new Dictionary<char, string>
    {
        { 'а', "a" }, { 'б', "b" }, { 'в', "v" }, { 'г', "g" }, { 'д', "d" },
        { 'е', "e" }, { 'ё', "e" }, { 'ж', "zh" }, { 'з', "z" }, { 'и', "i" },
        { 'й', "y" }, { 'к', "k" }, { 'л', "l" }, { 'м', "m" }, { 'н', "n" },
        { 'о', "o" }, { 'п', "p" }, { 'р', "r" }, { 'с', "s" }, { 'т', "t" },
        { 'у', "u" }, { 'ф', "f" }, { 'х', "h" }, { 'ц', "ts" }, { 'ч', "ch" },
        { 'ш', "sh" }, { 'щ', "sch" }, { 'ъ', "" }, { 'ы', "y" }, { 'ь', "" },
        { 'э', "e" }, { 'ю', "yu" }, { 'я', "ya" }
    };

    public static string GenerateSlug(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        input = input.ToLowerInvariant();

        var sb = new StringBuilder();
        foreach (var c in input)
        {
            if (RussianTransliterationMap.TryGetValue(c, out var repl)) sb.Append(repl);
            else sb.Append(c);
        }

        var slug = sb.ToString();

        slug = SpaceRegex().Replace(slug, "-");
        slug = EnglishCharactersRegex().Replace(slug, "");
        slug = ChangeDuplicates().Replace(slug, "-");
        slug = slug.Trim('-');

        return slug;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex EnglishCharactersRegex();

    [GeneratedRegex("-+")]
    private static partial Regex ChangeDuplicates();
}