using JobSeeker.WebApi.Domain.Entities;
using JobSeeker.WebApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Services.DataSeeder;

public class SpecializationSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var data = GetData();
        var newEntities = new List<Specialization>();

        foreach (var newEntity in data)
        {
            var existsEntity = await dbContext.Specializations
                .FirstOrDefaultAsync(x => x.Id == newEntity.Id, cancellationToken);

            if (existsEntity == null)
                newEntities.Add(newEntity);
            else
                existsEntity.Title = newEntity.Title;
        }


        if (newEntities.Count != 0) await dbContext.Specializations.AddRangeAsync(newEntities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static List<Specialization> GetData()
    {
        return
        [
            new Specialization
            {
                Id = 1,
                Title = "Backend",
                OccupationId = 1
            }
        ];
    }
}