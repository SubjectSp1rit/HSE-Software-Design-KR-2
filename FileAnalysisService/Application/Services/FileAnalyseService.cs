using System.Text;
using System.Text.Json;
using FileAnalysisService.Application.Dtos;
using Common.Models;
using FileAnalysisService.Infrastructure.Persistence;
using FileAnalysisService.Infrastructure.Adapters;

namespace FileAnalysisService.Application.Services;

public class FileAnalyseService : IFileAnalyseService
{
    private readonly AnalysisDbContext _db;
    private readonly IFileStorerAdapter _fileAdapter;
    private readonly IWordCloudAdapter _cloudAdapter;
    private readonly string _storagePath;

    public FileAnalyseService(
        AnalysisDbContext db,
        IFileStorerAdapter fileAdapter,
        IWordCloudAdapter cloudAdapter,
        IConfiguration config)
    {
        _db = db;
        _fileAdapter = fileAdapter;
        _cloudAdapter = cloudAdapter;
        _storagePath = config.GetValue<string>("StoragePath") ?? "storage";
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    public async Task<AnalysisResultDto> AnalyzeAsync(Guid fileId)
    {
        // Если уже есть результат — возвращает его, а не пересчитывает
        var existing = await _db.Results.FindAsync(fileId);
        if (existing != null)
            return MapDto(existing);

        // Чтение содержимого файла
        await using var fs = await _fileAdapter.GetFileStreamAsync(fileId);
        using var reader = new StreamReader(fs, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();

        // Подсчет метрик
        var paragraphCount = text
            .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Length;
        var wordCount = text
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
            .Length;
        var charCount = text.Length;
        var frequencies = text
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
            .GroupBy(w => w)
            .ToDictionary(g => g.Key, g => g.Count());

        // Генераация облака слов
        var imageBytes = await _cloudAdapter.GenerateCloudAsync(frequencies);
        var imageName = $"{fileId}.png";
        var imagePath = Path.Combine(_storagePath, imageName);
        await File.WriteAllBytesAsync(imagePath, imageBytes);

        // Сохранение результата в БД
        var result = new AnalysisResult
        {
            FileId = fileId,
            ParagraphCount = paragraphCount,
            WordCount = wordCount,
            CharCount = charCount,
            CloudLocation = imageName,
            AnalyzedAt = DateTime.UtcNow
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        // DTO
        var dto = MapDto(result);

        // Сохраняет результат в файле
        var json = JsonSerializer.Serialize(dto);
        var jsonFileName = imageName + ".txt";
        var jsonPath = Path.Combine(_storagePath, jsonFileName);
        await File.WriteAllTextAsync(jsonPath, json);

        return dto;
    }

    public async Task<AnalysisResultDto?> GetResultAsync(Guid fileId)
    {
        var res = await _db.Results.FindAsync(fileId);
        return res == null ? null : MapDto(res);
    }

    private static AnalysisResultDto MapDto(AnalysisResult r) => new AnalysisResultDto
    {
        FileId = r.FileId,
        ParagraphCount = r.ParagraphCount,
        WordCount = r.WordCount,
        CharCount = r.CharCount,
        CloudImagePath = r.CloudLocation,
        AnalyzedAt = r.AnalyzedAt
    };
}
