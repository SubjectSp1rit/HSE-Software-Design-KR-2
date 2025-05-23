namespace FileAnalysisService.Infrastructure.Adapters;

public interface IFileStorerAdapter
{
    Task<Stream> GetFileStreamAsync(Guid id);
}

public class FileStorerHttpAdapter : IFileStorerAdapter
{
    private readonly HttpClient _client;
    public FileStorerHttpAdapter(HttpClient client) => _client = client;

    public async Task<Stream> GetFileStreamAsync(Guid id)
    {
        var res = await _client.GetAsync($"/files/storage/files/{id}");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStreamAsync();
    }
}