using JobSeeker.WebApi.Application.Queries.Vacancies.GetSearchResults;
using Microsoft.AspNetCore.Mvc;

namespace JobSeeker.WebApi.Api.Controllers.V1;

/// <summary>
///     Vacancies controller
/// </summary>
[Route("vacancies")]
public class VacancyController : ApiControllerBase
{
    /// <summary>
    ///     Search vacancies
    /// </summary>
    /// <returns>Vacancies</returns>
    [HttpGet("search/{professionKeyId:int}")]
    [ProducesResponseType(typeof(GetSearchResultsVacancyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromRoute] int professionKeyId, [FromQuery] int take, [FromQuery] int skip)
    {
        var query = new GetSearchResultsVacancyRequest
        {
            ProfessionKeyId = professionKeyId,
            Skip = skip,
            Take = take
        };

        var response = await Mediator.Send(query);
        return Ok(response);
    }
}