using JobSeeker.WebApi.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetSearchResults;

public class GetSearchResultsVacancyHandler(ApplicationDbContext applicationDbContext) : IRequestHandler<GetSearchResultsVacancyRequest, GetSearchResultsVacancyResponse>
{
    public async Task<GetSearchResultsVacancyResponse> Handle(GetSearchResultsVacancyRequest request, CancellationToken cancellationToken)
    {
        var response = new GetSearchResultsVacancyResponse();

        var vacancies = await applicationDbContext.Vacancies
            .Where(x => x.ProfessionKeyId == request.ProfessionKeyId)
            .Select(x => new VacancyDto
            {
                Role = x.Title,
                VacancyId = x.Id,
                CompanyTitle = x.Company.Name,
                ShortDescription = x.Description.Substring(0, 100) + "..."
            })
            .ToListAsync(cancellationToken);

        response.Vacancies.AddRange(vacancies);

        return response;
    }
}