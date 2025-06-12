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
            var group = await dbContext.ScrapGroups
                .Include(x => x.ScrapTaskConfigurations)
                .FirstOrDefaultAsync(x => x.OccupationGroup == scrapGroup.OccupationGroup
                                          && x.Occupation == scrapGroup.Occupation
                                          && x.Specialization == scrapGroup.Specialization
                                          && x.SkillTag == scrapGroup.SkillTag, cancellationToken);

            var newConfigurations = scrapGroup.ScrapTaskConfigurations
                .Select(x => new ScrapTaskConfiguration
                {
                    Priority = x.Priority,
                    Entrypoint = x.Entrypoint,
                    ScrapGroup = group!
                }).ToList();

            if (group != null)
            {
                dbContext.ScrapTaskConfigurations.RemoveRange(group.ScrapTaskConfigurations);
                await dbContext.ScrapTaskConfigurations.AddRangeAsync(newConfigurations, cancellationToken);
            }
            else
                newGroups.Add(new ScrapGroup
                {
                    Priority = scrapGroup.Priority,
                    OccupationGroup = scrapGroup.OccupationGroup,
                    Occupation = scrapGroup.Occupation,
                    Specialization = scrapGroup.Specialization,
                    SkillTag = scrapGroup.SkillTag,
                    ScrapTaskConfigurations = newConfigurations
                });
        }


        if (newGroups.Count != 0) await dbContext.ScrapGroups.AddRangeAsync(newGroups, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
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
                SkillTag = 1,
                ScrapTaskConfigurations =
                [
                    new ScrapTaskConfiguration
                    {
                        Priority = 100,
                        Entrypoint = "https://krasnoyarsk.hh.ru/search/vacancy" +
                                     "?search_field=name" +
                                     "&search_field=company_name" +
                                     "&search_field=description" +
                                     "&text=C%23+junior"
                    }
                ]
            }
        ];
    }
}