using FileStoringService.Application.Dtos;

namespace FileStoringService.Application.Services;

public interface IFileStorageService
{
    Task<UploadResponseDto> SaveFileAsync(IFormFile file);
    Task<(Stream Content, string ContentType, string FileName)> GetFileAsync(Guid id);
}
