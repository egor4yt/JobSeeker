using JobSeeker.WebApi.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Queries.ProfessionsKeys.GetAll;

public class GetAllProfessionsKeysHandler(ApplicationDbContext applicationDbContext) : IRequestHandler<GetAllProfessionsKeysRequest, GetAllProfessionsKeysResponse>
{
    public async Task<GetAllProfessionsKeysResponse> Handle(GetAllProfessionsKeysRequest request, CancellationToken cancellationToken)
    {
        var response = new GetAllProfessionsKeysResponse();

        response.Items = await applicationDbContext.ProfessionKeys
            .Select(x => new ProfessionKeyDto
            {
                Id = x.Id,
                Title = x.Title
            })
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

        return response;
    }
}