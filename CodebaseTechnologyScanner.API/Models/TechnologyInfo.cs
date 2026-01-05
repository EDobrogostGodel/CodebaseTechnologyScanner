namespace CodebaseTechnologyScanner.API.Models;

public class TechnologyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? DetectedFrom { get; set; }
}
