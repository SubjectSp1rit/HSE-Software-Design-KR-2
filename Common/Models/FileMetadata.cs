namespace Common.Models;

public class FileMetadata
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}