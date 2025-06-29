using JobSeeker.WebApi.Application.Queries.ProfessionsKeys.GetAll;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.WebApi.Api.Controllers.V1;

/// <summary>
///     Profession keys controller
/// </summary>
[Route("profession-key")]
public class ProfessionKeyController : ApiControllerBase
{
    /// <summary>
    ///     Get all profession keys
    /// </summary>
    /// <returns>Profession keys</returns>
    [HttpGet("all")]
    [ProducesResponseType(typeof(GetAllProfessionsKeysResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllProfessionsKeysRequest();
        var response = await Mediator.Send(query);
        return Ok(response);
    }
}