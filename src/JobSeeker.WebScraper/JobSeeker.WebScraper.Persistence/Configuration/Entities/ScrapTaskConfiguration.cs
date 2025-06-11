using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebScraper.Persistence.Configuration.Entities;

public class ScrapTaskConfiguration : IEntityTypeConfiguration<ScrapTask>
{
    public void Configure(EntityTypeBuilder<ScrapTask> builder)
    {
        builder
            .HasOne(x => x.ScrapGroup)
            .WithMany(x => x.ScrapTasks)
            .HasForeignKey(x => x.ScrapGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne(x => x.ScrapSource)
            .WithMany(x => x.ScrapTasks)
            .HasForeignKey(x => x.ScrapSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(x => x.EntrypointPath)
            .HasColumnType("varchar(2048)")
            .IsRequired();

        builder
            .Property(x => x.ErrorDetails)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(x => x.Priority, "IX_ScrapTask_Priority");
    }
}