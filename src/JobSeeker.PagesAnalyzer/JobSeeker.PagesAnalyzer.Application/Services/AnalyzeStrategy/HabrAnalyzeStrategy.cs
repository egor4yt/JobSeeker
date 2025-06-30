using System.Text;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy.Models;
using JobSeeker.PagesAnalyzer.Application.Services.Normalizer;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy;

/// <summary>
///     Represents the strategy for analyzing HTML content from the hh.ru domain to extract vacancy details.
/// </summary>
public class HabrAnalyzeStrategy(ILogger<HabrAnalyzeStrategy> logger, INormalizer normalizer) : IAnalyzeStrategy
{
    public const string Domain = "habr.com";

    public async Task<Vacancy> AnalyzeAsync(Stream htmlContent, CancellationToken cancellationToken)
    {
        var response = new Vacancy();

        var document = new HtmlDocument();
        document.Load(htmlContent, Encoding.UTF8);
        var body = document.DocumentNode
            .SelectSingleNode("//body");

        if (body != null)
        {
            response.Role = body.QuerySelector(".page-title__title")?.InnerText!;

            var descriptionNode = body.QuerySelector(".vacancy-description__text .style-ugc");
            response.Description = await normalizer.NormalizeAsync(descriptionNode?.InnerText ?? "", cancellationToken);
            response.HtmlDescription = await normalizer.NormalizeAsync(descriptionNode?.InnerHtml ?? "", cancellationToken);

            response.Company = body.QuerySelector(".company_name")?.InnerText;

            var basicSection = body.QuerySelector(".basic-section");
            if (basicSection != null)
            {
                var isRemote = basicSection.InnerText.Contains("Можно удалённо");
                response.City = isRemote ? null : basicSection.QuerySelector("a.link-comp[href*=\"?city_id\"]")?.InnerText;
            }
        }
        else
            logger.LogWarning("HTML content is empty");

        return response;
    }
}