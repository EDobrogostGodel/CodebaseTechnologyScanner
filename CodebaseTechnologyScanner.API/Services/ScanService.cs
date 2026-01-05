using System.IO.Compression;
using CodebaseTechnologyScanner.API.Models;
using CodebaseTechnologyScanner.API.Repositories;
using CodebaseTechnologyScanner.API.Utils;

namespace CodebaseTechnologyScanner.API.Services;

public class ScanService : IScanService
{
    private readonly IScanRepository _repository;
    private readonly TechnologyDetector _detector;
    private readonly ILogger<ScanService> _logger;

    public ScanService(IScanRepository repository, TechnologyDetector detector, ILogger<ScanService> logger)
    {
        _repository = repository;
        _detector = detector;
        _logger = logger;
    }

    public async Task<ScanResult> ScanProjectAsync(Stream zipStream, string projectName)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        try
        {
            string fileHash = await HashHelper.CalculateSHA256Async(zipStream);
            zipStream.Position = 0;

            Directory.CreateDirectory(tempPath);

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory(tempPath);
            }

            var technologies = await _detector.DetectTechnologiesAsync(tempPath);
            var languageCounts = _detector.AnalyzeLanguages(tempPath);

            var scanResult = new ScanResult
            {
                ProjectName = projectName,
                Technologies = technologies,
                LanguageCounts = languageCounts,
                FileHash = fileHash
            };

            await _repository.SaveScanResultAsync(scanResult);

            return scanResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning project {ProjectName}", projectName);
            throw;
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    public async Task<ScanResult?> GetScanResultAsync(string id)
    {
        return await _repository.GetScanResultByIdAsync(id);
    }

    public async Task<PaginatedResponse<ScanResult>> GetAllScanResultsAsync(
        int page = 1,
        int pageSize = 20,
        string? sortBy = "timestamp",
        string? sortOrder = "desc",
        string? projectName = null,
        string? language = null,
        string? framework = null)
    {
        var allResults = await _repository.GetAllScanResultsAsync();

        var filtered = allResults.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(projectName))
        {
            filtered = filtered.Where(r => r.ProjectName.Contains(projectName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            filtered = filtered.Where(r => r.LanguageCounts.ContainsKey(language));
        }

        if (!string.IsNullOrWhiteSpace(framework))
        {
            filtered = filtered.Where(r => r.Technologies.Any(t => 
                t.Type.Equals("Framework", StringComparison.OrdinalIgnoreCase) && 
                t.Name.Equals(framework, StringComparison.OrdinalIgnoreCase)));
        }

        filtered = sortBy?.ToLower() switch
        {
            "projectname" => sortOrder?.ToLower() == "asc" 
                ? filtered.OrderBy(r => r.ProjectName) 
                : filtered.OrderByDescending(r => r.ProjectName),
            _ => sortOrder?.ToLower() == "asc" 
                ? filtered.OrderBy(r => r.Timestamp) 
                : filtered.OrderByDescending(r => r.Timestamp)
        };

        var filteredList = filtered.ToList();
        var totalResults = filteredList.Count;
        var totalPages = (int)Math.Ceiling(totalResults / (double)pageSize);

        var paginatedResults = filteredList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResponse<ScanResult>
        {
            Results = paginatedResults,
            Pagination = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalResults = totalResults,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            }
        };
    }

    public async Task<bool> DeleteScanResultAsync(string id)
    {
        return await _repository.DeleteScanResultAsync(id);
    }

    public async Task<ScanResult?> CheckDuplicateAsync(string fileHash)
    {
        return await _repository.GetScanResultByHashAsync(fileHash);
    }
}
