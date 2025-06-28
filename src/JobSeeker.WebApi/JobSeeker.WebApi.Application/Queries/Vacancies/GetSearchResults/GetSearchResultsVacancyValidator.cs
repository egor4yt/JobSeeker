using FluentValidation;

namespace JobSeeker.WebApi.Application.Queries.Vacancies.GetSearchResults;

public class GetSearchResultsVacancyValidator : AbstractValidator<GetSearchResultsVacancyRequest>
{
    public GetSearchResultsVacancyValidator()
    {
        RuleFor(x => x.ProfessionKeyId)
            .GreaterThan(0);

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0);
    }
}