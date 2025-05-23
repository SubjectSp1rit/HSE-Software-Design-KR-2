using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("files/analysis")]
public class FileAnalysisProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public FileAnalysisProxyController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("FileAnalysis");
    }

    [HttpPost("analyze/{id}")]
    public async Task<IActionResult> Analyze(string id)
    {
        var response = await _httpClient.PostAsync($"/files/analysis/analyze/{id}", null);
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        return Content(content, response.Content.Headers.ContentType?.MediaType);
    }

    [HttpGet("result/{id}")]
    public async Task<IActionResult> GetResult(string id)
    {
        var response = await _httpClient.GetAsync($"/files/analysis/result/{id}");
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }

    [HttpGet("cloud/{*location}")]
    public async Task<IActionResult> GetCloud(string location)
    {
        var response = await _httpClient.GetAsync($"/files/analysis/cloud/{location}");
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var stream = await response.Content.ReadAsStreamAsync();
        return File(stream, response.Content.Headers.ContentType?.MediaType ?? "image/png");
    }
}