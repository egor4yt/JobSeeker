using JobSeeker.WebApi.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetSearchResults;

public class GetSearchResultsVacancyHandler(ApplicationDbContext applicationDbContext) : IRequestHandler<GetSearchResultsVacancyRequest, GetSearchResultsVacancyResponse>
{
    public async Task<GetSearchResultsVacancyResponse> Handle(GetSearchResultsVacancyRequest request, CancellationToken cancellationToken)
    {
        var response = new GetSearchResultsVacancyResponse();

        var baseQuery = applicationDbContext.Vacancies
            .Where(x => x.ProfessionKeyId == request.ProfessionKeyId);

        var vacancies = await baseQuery
            .OrderByDescending(x => x.ActualityDate)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(x => new VacancyDto
            {
                Role = x.Title,
                VacancyId = x.Id,
                CompanyTitle = x.Company.Name,
                ShortDescription = x.Description.Substring(0, 150) + "...",
                DaysAgoCreated = (DateTime.UtcNow - x.ActualityDate).Days
            })
            .ToListAsync(cancellationToken);

        response.Vacancies.AddRange(vacancies);
        response.Total = await baseQuery.CountAsync(cancellationToken);

        return response;
    }
}