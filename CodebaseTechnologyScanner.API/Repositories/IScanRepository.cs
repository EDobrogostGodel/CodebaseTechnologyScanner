using CodebaseTechnologyScanner.API.Models;

namespace CodebaseTechnologyScanner.API.Repositories;

public interface IScanRepository
{
    Task<ScanResult> SaveScanResultAsync(ScanResult scanResult);
    Task<ScanResult?> GetScanResultByIdAsync(string id);
    Task<List<ScanResult>> GetAllScanResultsAsync();
    Task<ScanResult?> GetScanResultByHashAsync(string fileHash);
    Task<bool> DeleteScanResultAsync(string id);
}
