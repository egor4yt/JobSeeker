using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebScraper.Persistence.Configuration.Entities;

public class ScrapTaskResultConfiguration : IEntityTypeConfiguration<ScrapTaskResult>
{
    public void Configure(EntityTypeBuilder<ScrapTaskResult> builder)
    {
        builder
            .HasOne(x => x.ScrapTask)
            .WithMany(x => x.ScrapTaskResults)
            .HasForeignKey(x => x.ScrapTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.Link)
            .HasColumnType("varchar(256)")
            .IsRequired();

        builder
            .Property(x => x.Uploaded)
            .HasComment("Indicates whether the result of the scraping task has been uploaded to S3 or not")
            .IsRequired();
    }
}