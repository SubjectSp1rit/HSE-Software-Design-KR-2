using FileAnalysisService.Application.Dtos;

namespace FileAnalysisService.Infrastructure.Adapters;

public interface IWordCloudAdapter
{
    Task<byte[]> GenerateCloudAsync(Dictionary<string, int> frequencies);
}

public class WordCloudHttpAdapter : IWordCloudAdapter
{
    private readonly HttpClient _client;

    public WordCloudHttpAdapter(HttpClient client) => _client = client;

    public async Task<byte[]> GenerateCloudAsync(Dictionary<string, int> frequencies)
    {
        var req = new CloudRequestDto
        {
            Words = new List<WordDto>()
        };
        foreach (var kv in frequencies)
        {
            req.Words.Add(new WordDto { Text = kv.Key, Weight = kv.Value });
        }

        var response = await _client.PostAsJsonAsync("wordcloud", req);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}
