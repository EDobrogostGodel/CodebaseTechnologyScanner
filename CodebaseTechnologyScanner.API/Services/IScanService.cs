using CodebaseTechnologyScanner.API.Models;

namespace CodebaseTechnologyScanner.API.Services;

public interface IScanService
{
    Task<ScanResult> ScanProjectAsync(Stream zipStream, string projectName);
    Task<ScanResult?> GetScanResultAsync(string id);
    Task<PaginatedResponse<ScanResult>> GetAllScanResultsAsync(
        int page = 1,
        int pageSize = 20,
        string? sortBy = "timestamp",
        string? sortOrder = "desc",
        string? projectName = null,
        string? language = null,
        string? framework = null);
    Task<bool> DeleteScanResultAsync(string id);
    Task<ScanResult?> CheckDuplicateAsync(string fileHash);
}
