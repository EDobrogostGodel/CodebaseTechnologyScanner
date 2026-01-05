namespace CodebaseTechnologyScanner.API.Models;

public class DuplicateScanErrorResponse : ErrorResponse
{
    public string? ExistingScanId { get; set; }
}
