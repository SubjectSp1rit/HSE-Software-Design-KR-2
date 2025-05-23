using FileAnalysisService.Application.Dtos;

namespace FileAnalysisService.Application.Services;

public interface IFileAnalyseService
{
    Task<AnalysisResultDto> AnalyzeAsync(Guid fileId);
    Task<AnalysisResultDto?> GetResultAsync(Guid fileId);
}
