using Common.Models;
using FileStoringService.Application.Dtos;
using FileStoringService.Infrastructure.Repositories;
using Core.Utils;

namespace FileStoringService.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IFileRepository _repo;
    private readonly string _storagePath;

    public FileStorageService(IFileRepository repo, IConfiguration config)
    {
        _repo = repo;
        _storagePath = config.GetValue<string>("StoragePath") ?? "storage";
    }

    public async Task<UploadResponseDto> SaveFileAsync(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var hash = HashHelper.ComputeHash(bytes);
        var existing = await _repo.GetByHashAsync(hash);
        if (existing != null)
            return new UploadResponseDto { Id = existing.Id, AlreadyExists = true };

        var id = Guid.NewGuid();
        var ext = Path.GetExtension(file.FileName);
        var fileName = id + ext;
        var path = FileStorageHelper.GetStoragePath(_storagePath, fileName);
        FileStorageHelper.EnsureDirectory(path);
        await File.WriteAllBytesAsync(path, bytes);

        var meta = new FileMetadata
        {
            Id = id,
            Name = file.FileName,
            Hash = hash,
            Location = path
        };
        await _repo.AddAsync(meta);

        return new UploadResponseDto { Id = id, AlreadyExists = false };
    }

    public async Task<(Stream Content, string ContentType, string FileName)> GetFileAsync(Guid id)
    {
        var meta = await _repo.GetByIdAsync(id)
                  ?? throw new KeyNotFoundException($"Файл {id} не найден");
        var stream = new FileStream(meta.Location, FileMode.Open, FileAccess.Read);
        return (stream, "application/octet-stream", meta.Name);
    }
}
