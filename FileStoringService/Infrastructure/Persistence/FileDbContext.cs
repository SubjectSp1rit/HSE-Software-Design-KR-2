using Microsoft.EntityFrameworkCore;
using Common.Models;

namespace FileStoringService.Infrastructure.Persistence;

public class FileDbContext : DbContext
{
    public FileDbContext(DbContextOptions<FileDbContext> options)
        : base(options) { }

    public DbSet<FileMetadata> Files { get; set; } = null!;
}
