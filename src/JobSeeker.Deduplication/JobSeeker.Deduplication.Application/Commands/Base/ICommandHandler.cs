namespace JobSeeker.Deduplication.Application.Commands.Base;

public interface ICommandHandler<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TRequest>
{
    Task ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}