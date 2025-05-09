using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebScraper.Persistence.Configuration.Entities;

public class ScrapSourceConfiguration : IEntityTypeConfiguration<ScrapSource>
{
    public void Configure(EntityTypeBuilder<ScrapSource> builder)
    {
        builder
            .HasMany(x => x.ScrapTasks)
            .WithMany(x => x.ScrapSources)
            .UsingEntity<ScrapTaskToSourceMap>(
                left => left
                    .HasOne(x => x.ScrapTask)
                    .WithMany()
                    .HasForeignKey(x => x.ScrapTaskId),
                right => right
                    .HasOne(x => x.ScrapSource)
                    .WithMany()
                    .HasForeignKey(x => x.ScrapSourceId),
                joinEntity => { joinEntity.HasKey(x => new { x.ScrapTaskId, x.ScrapSourceId }); });

        builder
            .Property(x => x.Domain)
            .HasColumnType("varchar(64)")
            .IsRequired();

        builder
            .HasIndex(x => x.Domain, "UX_ScrapSource_Domain")
            .IsUnique();
    }
}