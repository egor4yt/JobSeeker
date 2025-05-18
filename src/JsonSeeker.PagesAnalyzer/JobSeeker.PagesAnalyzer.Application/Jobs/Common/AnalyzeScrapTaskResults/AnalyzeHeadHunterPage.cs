using System.Text;
using HtmlAgilityPack;
using JobSeeker.PagesAnalyzer.Application.Jobs.Common.AnalyzeScrapTaskResults.Models;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.Application.Jobs.Common.AnalyzeScrapTaskResults;

public partial class AnalyzeScrapTaskResultsJob
{
    private Task<Vacancy> GetHeadHunterVacancyAsync(Stream htmlContent)
    {
        var response = new Vacancy();

        var document = new HtmlDocument();
        document.Load(htmlContent, Encoding.UTF8);
        var body = document.DocumentNode
            .SelectSingleNode("//body");

        if (body != null)
        {
            response.Role = body.SelectSingleNode("descendant::*[@data-qa=\"vacancy-title\"]")?.InnerText!;
            response.Description = body.SelectSingleNode("descendant::*[@data-qa=\"vacancy-description\"]")?.InnerText;

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

        return Task.FromResult(response);
    }
}