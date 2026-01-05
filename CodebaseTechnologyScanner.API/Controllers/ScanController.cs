using CodebaseTechnologyScanner.API.Models;
using CodebaseTechnologyScanner.API.Services;
using CodebaseTechnologyScanner.API.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CodebaseTechnologyScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly IScanService _scanService;
    private readonly ILogger<ScanController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public ScanController(
        IScanService scanService, 
        ILogger<ScanController> logger, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _scanService = scanService;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// Uploads a ZIP file containing a project and performs technology detection scan.
    /// </summary>
    /// <param name="file">ZIP file containing the project to scan</param>
    /// <param name="projectName">Optional custom name for the project</param>
    /// <returns>Scan result with detected technologies and languages</returns>
    /// <response code="200">Scan completed successfully</response>
    /// <response code="400">Invalid file or missing required parameters</response>
    /// <response code="409">Duplicate scan detected</response>
    /// <response code="413">File size exceeds maximum allowed</response>
    /// <response code="500">Internal server error during processing</response>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ScanResult), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(DuplicateScanErrorResponse), 409)]
    [ProducesResponseType(typeof(ErrorResponse), 413)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<ScanResult>> UploadAndScan([FromForm] IFormFile file, [FromForm] string? projectName)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "No file uploaded",
                StatusCode = 400,
                Details = "A ZIP file is required"
            });
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Only ZIP files are supported",
                StatusCode = 400,
                Details = $"File type '{Path.GetExtension(file.FileName)}' is not allowed"
            });
        }

        var maxSize = _configuration.GetValue<long>("MaxUploadSizeBytes", 52428800);
        if (file.Length > maxSize)
        {
            return StatusCode(413, new ErrorResponse
            {
                Error = "File size exceeds maximum allowed size",
                StatusCode = 413,
                Details = $"Maximum allowed size is {FileSizeHelper.FormatBytes(maxSize)}"
            });
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var hash = await HashHelper.CalculateSHA256Async(memoryStream);
            memoryStream.Position = 0;

            var existingScan = await _scanService.CheckDuplicateAsync(hash);
            if (existingScan != null)
            {
                return Conflict(new DuplicateScanErrorResponse
                {
                    Error = "Duplicate scan detected. A scan with identical content already exists.",
                    StatusCode = 409,
                    ExistingScanId = existingScan.Id,
                    Details = $"Project '{existingScan.ProjectName}' was already scanned on {existingScan.Timestamp:yyyy-MM-dd HH:mm}"
                });
            }

            var name = projectName ?? Path.GetFileNameWithoutExtension(file.FileName);
            var result = await _scanService.ScanProjectAsync(memoryStream, name);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scan for file {FileName}", file.FileName);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while scanning the project",
                StatusCode = 500,
                Details = _environment.IsDevelopment() ? ex.Message : null
            });
        }
    }

    /// <summary>
    /// Gets a specific scan result by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the scan</param>
    /// <returns>Scan result if found</returns>
    /// <response code="200">Scan result found</response>
    /// <response code="404">Scan result not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScanResult), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<ActionResult<ScanResult>> GetScanResult(string id)
    {
        var result = await _scanService.GetScanResultAsync(id);
        
        if (result == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Scan result not found",
                StatusCode = 404,
                Details = $"No scan found with ID: {id}"
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets paginated scan history with optional filtering and sorting.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of results per page (1-100)</param>
    /// <param name="sortBy">Field to sort by (timestamp or projectName)</param>
    /// <param name="sortOrder">Sort order (asc or desc)</param>
    /// <param name="projectName">Filter by project name (partial match)</param>
    /// <param name="language">Filter by detected language</param>
    /// <param name="framework">Filter by detected framework</param>
    /// <returns>Paginated list of scan results</returns>
    /// <response code="200">Scan history retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PaginatedResponse<ScanResult>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<ActionResult<PaginatedResponse<ScanResult>>> GetScanHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "timestamp",
        [FromQuery] string? sortOrder = "desc",
        [FromQuery] string? projectName = null,
        [FromQuery] string? language = null,
        [FromQuery] string? framework = null)
    {
        if (page < 1)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Invalid query parameters",
                StatusCode = 400,
                Details = "Page number must be greater than 0"
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Invalid query parameters",
                StatusCode = 400,
                Details = "Page size must be between 1 and 100"
            });
        }

        try
        {
            var results = await _scanService.GetAllScanResultsAsync(
                page, pageSize, sortBy, sortOrder, projectName, language, framework);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching scan history with page={Page}, pageSize={PageSize}", page, pageSize);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while fetching scan history",
                StatusCode = 500,
                Details = _environment.IsDevelopment() ? ex.Message : null
            });
        }
    }

    /// <summary>
    /// Deletes a scan result by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the scan to delete</param>
    /// <returns>Confirmation of deletion</returns>
    /// <response code="200">Scan deleted successfully</response>
    /// <response code="404">Scan result not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(DeleteResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<ActionResult<DeleteResponse>> DeleteScanResult(string id)
    {
        var deleted = await _scanService.DeleteScanResultAsync(id);

        if (!deleted)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Scan result not found",
                StatusCode = 404,
                Details = $"No scan found with ID: {id}"
            });
        }

        return Ok(new DeleteResponse
        {
            Message = "Scan result deleted successfully",
            DeletedId = id
        });
    }
}
