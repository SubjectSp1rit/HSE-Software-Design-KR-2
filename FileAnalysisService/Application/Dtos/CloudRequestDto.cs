namespace FileAnalysisService.Application.Dtos;

public class WordDto
{
    public string Text { get; set; } = string.Empty;
    public int Weight { get; set; }
}

public class CloudRequestDto
{
    public string Format { get; set; } = "png";
    public int Width { get; set; } = 500;
    public int Height { get; set; } = 500;
    public int FontScale { get; set; } = 15;
    public string Scale { get; set; } = "linear";
    public List<WordDto> Words { get; set; } = new();
}