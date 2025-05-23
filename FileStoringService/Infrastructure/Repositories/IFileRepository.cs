using Common.Models;

namespace FileStoringService.Infrastructure.Repositories;

public interface IFileRepository
{
    Task<FileMetadata?> GetByHashAsync(string hash);
    Task<FileMetadata?> GetByIdAsync(Guid id);
    Task AddAsync(FileMetadata file);
}
