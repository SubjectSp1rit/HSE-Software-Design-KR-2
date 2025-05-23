using Microsoft.EntityFrameworkCore;
using Common.Models;

namespace FileAnalysisService.Infrastructure.Persistence;

public class AnalysisDbContext : DbContext
{
    public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options)
        : base(options) { }

    public DbSet<AnalysisResult> Results { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalysisResult>()
            .HasKey(ar => ar.FileId);

        base.OnModelCreating(modelBuilder);
    }
}
