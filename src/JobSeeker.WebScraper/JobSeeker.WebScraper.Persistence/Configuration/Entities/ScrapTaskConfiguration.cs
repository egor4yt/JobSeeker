using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebScraper.Persistence.Configuration.Entities;

public class ScrapTaskConfiguration : IEntityTypeConfiguration<ScrapTask>
{
    public void Configure(EntityTypeBuilder<ScrapTask> builder)
    {
        builder
            .Property(x => x.ExcludeWordsList)
            .HasComment("List of words separated with ','")
            .HasColumnType("varchar(256)");

        builder
            .Property(x => x.SearchText)
            .HasColumnType("varchar(64)")
            .IsRequired();
    }
}