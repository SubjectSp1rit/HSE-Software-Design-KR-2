namespace WordCloudService.Models;

public class CloudRequestDto
{
    public string Format { get; set; } = "png";
    
    public int Width { get; set; }

    public int Height { get; set; }

    public int FontScale { get; set; }

    public string Scale { get; set; } = "linear";

    public List<WordDto> Words { get; set; } = new();
}