namespace Common.Models;

public class AnalysisResult
{
    public Guid FileId { get; set; }
    public int ParagraphCount { get; set; }
    public int WordCount { get; set; }
    public int CharCount { get; set; }
    public string CloudLocation { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}