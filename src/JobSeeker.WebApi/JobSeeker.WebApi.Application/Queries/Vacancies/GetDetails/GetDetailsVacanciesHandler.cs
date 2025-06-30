using JobSeeker.WebApi.Application.Exceptions;
using JobSeeker.WebApi.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetDetails;

public class GetDetailsVacanciesHandler(ApplicationDbContext applicationDbContext) : IRequestHandler<GetDetailsVacanciesRequest, GetDetailsVacanciesResponse>
{
    public async Task<GetDetailsVacanciesResponse> Handle(GetDetailsVacanciesRequest request, CancellationToken cancellationToken)
    {
        var response = await applicationDbContext.Vacancies
            .Where(x => x.Id == request.VacancyId)
            .Select(x => new GetDetailsVacanciesResponse
            {
                Role = x.Title,
                Company = x.Company.Name,
                HtmlDescription = x.HtmlDescription,
                Sources = x.VacancySources
                    .GroupBy(s => new { s.LocationId, s.SourceId })
                    .Select(g => new SourceDto
                    {
                        Location = string.IsNullOrWhiteSpace(g.First().Location!.Title) ? "Remote" : g.First().Location!.Title,
                        Domain = g.First().Source.TopLevelDomain,
                        SourceUrl = g.Select(s => string.Format(s.Source.UrlTemplate, s.SourceKey))
                    })
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null) throw new NotFoundException("Vacancy not found");

        return response;
    }
}