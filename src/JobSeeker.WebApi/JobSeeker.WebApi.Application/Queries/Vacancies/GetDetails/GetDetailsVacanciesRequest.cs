using MediatR;

namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetDetails;

public class GetDetailsVacanciesRequest : IRequest<GetDetailsVacanciesResponse>
{
    public int VacancyId { get; set; }
}