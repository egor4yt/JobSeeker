using JobSeeker.WebApi.Domain.Entities;
using JobSeeker.WebApi.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Application.Services.DataSeeder;

public class SourcesSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var data = GetData();
        var newEntities = new List<Source>();

        foreach (var newEntity in data)
        {
            var existsEntity = await dbContext.Sources
                .FirstOrDefaultAsync(x => x.Id == newEntity.Id, cancellationToken);

            if (existsEntity == null)
                newEntities.Add(newEntity);
            else
            {
                existsEntity.TopLevelDomain = newEntity.TopLevelDomain;
                existsEntity.UrlTemplate = newEntity.UrlTemplate;
            }
        }

        if (newEntities.Count != 0) await dbContext.Sources.AddRangeAsync(newEntities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static List<Source> GetData()
    {
        return
        [
            new Source
            {
                Id = 1,
                TopLevelDomain = "hh.ru",
                UrlTemplate = "https://hh.ru/vacancy/{0}"
            }
        ];
    }
}