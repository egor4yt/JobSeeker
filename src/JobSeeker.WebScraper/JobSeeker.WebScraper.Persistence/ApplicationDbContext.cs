﻿using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebScraper.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ScrapTask> ScrapTasks { get; set; }
    public DbSet<ScrapGroup> ScrapGroups { get; set; }
    public DbSet<ScrapTaskResult> ScrapTaskResults { get; set; }
    public DbSet<ScrapTaskConfiguration> ScrapTaskConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}