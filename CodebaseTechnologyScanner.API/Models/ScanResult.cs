namespace CodebaseTechnologyScanner.API.Models;

public class ScanResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ProjectName { get; set; } = string.Empty;
    public List<TechnologyInfo> Technologies { get; set; } = new();
    public Dictionary<string, int> LanguageCounts { get; set; } = new();
    public string? FileHash { get; set; }
}
