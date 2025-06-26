using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.WebApi.Api.Controllers.V1;

/// <summary>
///     Base API controller version 1.0
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    ///     Mediator instance in the current HTTP request scope
    /// </summary>
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;
}