using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("files/storage")]
public class FileStorageProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public FileStorageProxyController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("FileStorage");
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload()
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/files/storage/upload");
        requestMessage.Content = new StreamContent(Request.Body);
        requestMessage.Content.Headers.ContentType =
            System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Request.ContentType!);

        var response = await _httpClient.SendAsync(requestMessage);
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, response.Content.Headers.ContentType?.MediaType);
    }

    [HttpGet("files/{id}")]
    public async Task<IActionResult> GetFile(string id)
    {
        var response = await _httpClient.GetAsync($"/files/storage/files/{id}");
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var contentStream = await response.Content.ReadAsStreamAsync();
        return File(contentStream, response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
    }
}