using Microsoft.AspNetCore.Mvc;
using FileStoringService.Application.Services;

namespace FileStoringService.Controllers;

[ApiController]
[Route("files/storage")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _service;
    public FilesController(IFileStorageService service) => _service = service;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null) return BadRequest("Необходимо указать файл для загрузки");
        var result = await _service.SaveFileAsync(file);
        return Ok(result);
    }

    [HttpGet("files/{id}")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        try
        {
            var (stream, contentType, fileName) = await _service.GetFileAsync(id);
            return File(stream, contentType, fileDownloadName: fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
