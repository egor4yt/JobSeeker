using JobSeeker.WebApi.Domain.Entities;
using JobSeeker.WebApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Services.DataSeeder;

public class OccupationsSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var data = GetData();
        var newEntities = new List<Occupation>();

        foreach (var newEntity in data)
        {
            var existsEntity = await dbContext.Occupations
                .FirstOrDefaultAsync(x => x.Id == newEntity.Id, cancellationToken);

            if (existsEntity == null)
                newEntities.Add(newEntity);
            else
                existsEntity.Title = newEntity.Title;
        }

        if (newEntities.Count != 0) await dbContext.Occupations.AddRangeAsync(newEntities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static List<Occupation> GetData()
    {
        return
        [
            new Occupation
            {
                Id = 1,
                Title = "Разработка веб-приложений",
                OccupationGroupId = 1
            }
        ];
    }
}