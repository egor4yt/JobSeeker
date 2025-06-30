using FluentValidation;

namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetDetails;

public class GetDetailsVacanciesValidator : AbstractValidator<GetDetailsVacanciesRequest>
{
    public GetDetailsVacanciesValidator()
    {
        RuleFor(x => x.VacancyId)
            .GreaterThan(0);
    }
}