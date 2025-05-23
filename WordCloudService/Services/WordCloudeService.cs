using WordCloudService.Models;

namespace WordCloudService.Services;

public class WordCloudeService : IWordCloudeService
{
    private readonly HttpClient _client;

    public WordCloudeService(HttpClient client) => _client = client;

    public async Task<byte[]> GenerateAsync(CloudRequestDto request)
    {
        var wordEntries = request.Words.Select(w => $"{w.Text}:{w.Weight}");
        var textParam = string.Join(",", wordEntries);
        
        var qcRequest = new
        {
            text = textParam,
            format = "png",
            width = request.Width,
            height = request.Height,
            fontScale = request.FontScale,
            scale = request.Scale,
            useWordList = true
        };

        var response = await _client.PostAsJsonAsync("wordcloud", qcRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}
