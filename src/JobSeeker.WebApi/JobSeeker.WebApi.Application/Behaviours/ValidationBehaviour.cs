using FluentValidation;
using MediatR;

namespace JobSeeker.WebApi.Application.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var validationContext = new ValidationContext<TRequest>(request);
            var validationTasks = validators.Select(x => x.ValidateAsync(validationContext, cancellationToken));
            var validationResults = await Task.WhenAll(validationTasks);
            var validationFailures = validationResults
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .ToList();

            if (validationFailures.Count != 0) throw new Application.Exceptions.ValidationException(validationFailures);
        }

        return await next(cancellationToken);
    }
}