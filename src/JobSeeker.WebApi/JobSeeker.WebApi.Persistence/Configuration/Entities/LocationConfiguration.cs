using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder
            .Property(x => x.Title)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .Property(x => x.Slug)
            .HasColumnType("varchar(256)")
            .IsRequired();

        builder
            .HasIndex(x => x.Slug, "UX_Location_Slug")
            .IsUnique();
    }
}