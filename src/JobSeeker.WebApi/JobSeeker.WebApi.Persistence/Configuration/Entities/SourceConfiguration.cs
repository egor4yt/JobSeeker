using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder
            .Property(x => x.TopLevelDomain)
            .HasColumnType("varchar(32)")
            .IsRequired();

        builder
            .Property(x => x.UrlTemplate)
            .HasColumnType("varchar(128)")
            .IsRequired();
    }
}