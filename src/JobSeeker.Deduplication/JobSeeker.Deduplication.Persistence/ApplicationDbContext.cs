using JobSeeker.Deduplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.Deduplication.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<RawVacancy> RawVacancies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}