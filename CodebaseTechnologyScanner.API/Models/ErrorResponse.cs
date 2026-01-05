namespace CodebaseTechnologyScanner.API.Models;

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? Details { get; set; }
}
