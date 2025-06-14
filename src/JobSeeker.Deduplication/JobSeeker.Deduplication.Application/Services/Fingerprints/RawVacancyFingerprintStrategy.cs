using System.Security.Cryptography;
using System.Text;
using JobSeeker.Deduplication.Application.Services.Tokenizer;
using JobSeeker.Deduplication.Domain.Entities;

namespace JobSeeker.Deduplication.Application.Services.Fingerprints;

public class RawVacancyFingerprintStrategy<TEntity>(ITokenizer tokenizer) : IFingerprintStrategy<TEntity> where TEntity : RawVacancy
{
    public async Task<string> CalculateAsync(TEntity rawVacancy, CancellationToken cancellationToken)
    {
        var text = $"{rawVacancy.Company} {rawVacancy.Title} {rawVacancy.Description}";
        var tokens = await tokenizer.TokenizeAsync(text, cancellationToken);
        var sortedText = string.Join(" ", tokens.Order(StringComparer.Ordinal));
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sortedText));

        return Convert.ToHexString(hashBytes);
    }
}