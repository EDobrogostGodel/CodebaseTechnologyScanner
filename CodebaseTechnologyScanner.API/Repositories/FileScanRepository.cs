using System.Text.Json;
using CodebaseTechnologyScanner.API.Models;

namespace CodebaseTechnologyScanner.API.Repositories;

public class FileScanRepository : IScanRepository
{
    private readonly string _storageFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FileScanRepository(IConfiguration configuration)
    {
        _storageFilePath = configuration["StorageFilePath"] ?? "scan-results.json";
        InitializeStorageFile();
    }

    private void InitializeStorageFile()
    {
        if (!File.Exists(_storageFilePath))
        {
            File.WriteAllText(_storageFilePath, "[]");
        }
    }

    public async Task<ScanResult> SaveScanResultAsync(ScanResult scanResult)
    {
        await _semaphore.WaitAsync();
        try
        {
            var results = await ReadAllResultsAsync();
            results.Add(scanResult);
            await WriteAllResultsAsync(results);
            return scanResult;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ScanResult?> GetScanResultByIdAsync(string id)
    {
        var results = await ReadAllResultsAsync();
        return results.FirstOrDefault(r => r.Id == id);
    }

    public async Task<List<ScanResult>> GetAllScanResultsAsync()
    {
        return await ReadAllResultsAsync();
    }

    public async Task<ScanResult?> GetScanResultByHashAsync(string fileHash)
    {
        var results = await ReadAllResultsAsync();
        return results.FirstOrDefault(r => r.FileHash == fileHash);
    }

    public async Task<bool> DeleteScanResultAsync(string id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var results = await ReadAllResultsAsync();
            var result = results.FirstOrDefault(r => r.Id == id);
            
            if (result == null)
            {
                return false;
            }

            results.Remove(result);
            await WriteAllResultsAsync(results);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<List<ScanResult>> ReadAllResultsAsync()
    {
        var json = await File.ReadAllTextAsync(_storageFilePath);
        return JsonSerializer.Deserialize<List<ScanResult>>(json) ?? new List<ScanResult>();
    }

    private async Task WriteAllResultsAsync(List<ScanResult> results)
    {
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_storageFilePath, json);
    }
}
