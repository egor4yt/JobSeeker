using JobSeeker.WebApi.Application.Queries.Base;
using MediatR;

namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetSearchResults;

public class GetSearchResultsVacancyRequest : IPaging, IRequest<GetSearchResultsVacancyResponse>
{
    public int ProfessionKeyId { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}