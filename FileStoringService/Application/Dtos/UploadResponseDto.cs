namespace FileStoringService.Application.Dtos;

public class UploadResponseDto
{
    public Guid Id { get; set; }
    public bool AlreadyExists { get; set; }
}
