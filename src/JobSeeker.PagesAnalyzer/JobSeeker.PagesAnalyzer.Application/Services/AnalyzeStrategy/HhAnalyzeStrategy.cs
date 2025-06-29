using System.Text;
using HtmlAgilityPack;
using JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy.Models;
using JobSeeker.PagesAnalyzer.Application.Services.Normalizer;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy;

/// <summary>
///     Represents the strategy for analyzing HTML content from the hh.ru domain to extract vacancy details.
/// </summary>
public class HhAnalyzeStrategy(ILogger<HhAnalyzeStrategy> logger, INormalizer normalizer) : IAnalyzeStrategy
{
    public const string Domain = "hh.ru";

    public async Task<Vacancy> AnalyzeAsync(Stream htmlContent, CancellationToken cancellationToken)
    {
        var response = new Vacancy();

        var document = new HtmlDocument();
        document.Load(htmlContent, Encoding.UTF8);
        var body = document.DocumentNode
            .SelectSingleNode("//body");

        if (body != null)
        {
            response.Role = body.SelectSingleNode("descendant::*[@data-qa=\"vacancy-title\"]")?.InnerText!;

            var descriptionNode = body.SelectSingleNode("descendant::*[@data-qa=\"vacancy-description\"]");
            response.Description = await normalizer.NormalizeAsync(descriptionNode?.InnerText ?? "", cancellationToken);
            response.HtmlDescription = await normalizer.NormalizeAsync(descriptionNode?.InnerHtml ?? "", cancellationToken);

            response.Company = body.SelectSingleNode("descendant::*[@data-qa=\"vacancy-company-name\"]")?
                .InnerText.Split(',')
                .FirstOrDefault()?
                .Replace("&nbsp;", " ");

            response.City = body.SelectSingleNode("descendant::*[@data-qa=\"vacancy-view-raw-address\"]")?
                .InnerText.Split(',')
                .FirstOrDefault();
        }
        else
            logger.LogWarning("HTML content is empty");

        return response;
    }
}