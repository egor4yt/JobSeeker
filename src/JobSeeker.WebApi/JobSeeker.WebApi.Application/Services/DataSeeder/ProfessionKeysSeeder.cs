using JobSeeker.WebApi.Domain.Entities;
using JobSeeker.WebApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Services.DataSeeder;

public class ProfessionKeysSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var data = GetData();
        var newEntities = new List<ProfessionKey>();

        foreach (var newEntity in data)
        {
            var existsEntity = await dbContext.ProfessionKeys
                .FirstOrDefaultAsync(x => x.OccupationGroupId == newEntity.OccupationGroupId
                                          && x.OccupationId == newEntity.OccupationId
                                          && x.SpecializationId == newEntity.SpecializationId
                                          && x.SkillTagId == newEntity.SkillTagId, cancellationToken);

            if (existsEntity == null)
                newEntities.Add(newEntity);
            else
                existsEntity.Title = newEntity.Title;
        }

        if (newEntities.Count != 0) await dbContext.ProfessionKeys.AddRangeAsync(newEntities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static List<ProfessionKey> GetData()
    {
        return
        [
            new ProfessionKey
            {
                OccupationGroupId = 1,
                OccupationId = 1,
                SpecializationId = 1,
                SkillTagId = 1,
                Title = "C# backend разработчик"
            }
        ];
    }
}