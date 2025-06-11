using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebScraper.Persistence.Configuration.Entities;

public class ScrapSourceConfiguration : IEntityTypeConfiguration<ScrapSource>
{
    public void Configure(EntityTypeBuilder<ScrapSource> builder)
    {
        builder
            .Property(x => x.Domain)
            .HasColumnType("varchar(64)")
            .IsRequired();

        builder
            .HasIndex(x => x.Domain, "UX_ScrapSource_Domain")
            .IsUnique();
    }
}