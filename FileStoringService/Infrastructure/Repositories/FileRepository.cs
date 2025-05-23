using Common.Models;
using FileStoringService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly FileDbContext _ctx;
    public FileRepository(FileDbContext ctx) => _ctx = ctx;

    public async Task<FileMetadata?> GetByHashAsync(string hash)
        => await _ctx.Files.FirstOrDefaultAsync(f => f.Hash == hash);

    public async Task<FileMetadata?> GetByIdAsync(Guid id)
        => await _ctx.Files.FindAsync(id);

    public async Task AddAsync(FileMetadata file)
    {
        _ctx.Files.Add(file);
        await _ctx.SaveChangesAsync();
    }
}
