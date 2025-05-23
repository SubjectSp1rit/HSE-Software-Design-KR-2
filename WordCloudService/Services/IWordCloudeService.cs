using WordCloudService.Models;

namespace WordCloudService.Services;

public interface IWordCloudeService
{
    Task<byte[]> GenerateAsync(CloudRequestDto request);
}
