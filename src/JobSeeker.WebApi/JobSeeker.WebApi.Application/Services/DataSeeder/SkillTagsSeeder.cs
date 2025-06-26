using JobSeeker.WebApi.Domain.Entities;
using JobSeeker.WebApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Services.DataSeeder;

public class SkillTagsSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var data = GetData();
        var newEntities = new List<SkillTag>();

        foreach (var newEntity in data)
        {
            var existsEntity = await dbContext.SkillTags
                .FirstOrDefaultAsync(x => x.Id == newEntity.Id, cancellationToken);

            if (existsEntity == null)
                newEntities.Add(newEntity);
            else
                existsEntity.Title = newEntity.Title;
        }

        if (newEntities.Count != 0) await dbContext.SkillTags.AddRangeAsync(newEntities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static List<SkillTag> GetData()
    {
        return
        [
            new SkillTag
            {
                Id = 1,
                Title = ".NET / C#",
                SpecializationId = 1
            }
        ];
    }
}