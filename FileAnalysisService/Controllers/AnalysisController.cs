using Microsoft.AspNetCore.Mvc;
using FileAnalysisService.Application.Services;

namespace FileAnalysisService.Controllers;

[ApiController]
[Route("files/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly IFileAnalyseService _service;
    public AnalysisController(IFileAnalyseService service) => _service = service;

    [HttpPost("analyze/{id}")]
    public async Task<IActionResult> Analyze(Guid id)
    {
        var result = await _service.AnalyzeAsync(id);
        return Ok(result);
    }

    [HttpGet("result/{id}")]
    public async Task<IActionResult> GetResult(Guid id)
    {
        var result = await _service.GetResultAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("cloud/{*location}")]
    public IActionResult GetCloud(string location)
    {
        var path = System.IO.Path.Combine("storage", location);
        if (!System.IO.File.Exists(path)) return NotFound();
        var bytes = System.IO.File.ReadAllBytes(path);
        return File(bytes, "image/png");
    }
}