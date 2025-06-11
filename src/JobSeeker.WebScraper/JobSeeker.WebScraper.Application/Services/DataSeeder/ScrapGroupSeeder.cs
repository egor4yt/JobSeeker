using JobSeeker.WebScraper.Domain.Entities;
using JobSeeker.WebScraper.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebScraper.Application.Services.DataSeeder;

public class ScrapGroupSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var data = GetData();
        var newGroups = new List<ScrapGroup>();

        foreach (var scrapGroup in data)
        {
            var exists = await dbContext.ScrapGroups
                .AnyAsync(x => x.Id == scrapGroup.Id, cancellationToken);
            if (exists) continue;

            newGroups.Add(new ScrapGroup
            {
                Priority = scrapGroup.Priority,
                OccupationGroup = scrapGroup.OccupationGroup,
                Occupation = scrapGroup.Occupation,
                Specialization = scrapGroup.Specialization,
                SkillTag = scrapGroup.SkillTag
            });
        }

        if (newGroups.Count != 0)
        {
            await dbContext.ScrapGroups.AddRangeAsync(newGroups, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        return false;
    }

    private static List<ScrapGroup> GetData()
    {
        return
        [
            new ScrapGroup
            {
                Priority = 99,
                OccupationGroup = 1,
                Occupation = 1,
                Specialization = 1,
                SkillTag = 1
            }
        ];
    }
}