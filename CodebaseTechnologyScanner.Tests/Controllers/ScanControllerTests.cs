using CodebaseTechnologyScanner.API.Controllers;
using CodebaseTechnologyScanner.API.Models;
using CodebaseTechnologyScanner.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Hosting;

namespace CodebaseTechnologyScanner.Tests.Controllers;

/// <summary>
/// Unit tests for ScanController.
/// Validates HTTP request handling, validation, and response formatting.
/// </summary>
public class ScanControllerTests
{
    private readonly Mock<IScanService> _serviceMock;
    private readonly Mock<ILogger<ScanController>> _loggerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly ScanController _controller;

    public ScanControllerTests()
    {
        _serviceMock = new Mock<IScanService>();
        _loggerMock = new Mock<ILogger<ScanController>>();
        _configMock = new Mock<IConfiguration>();
        _environmentMock = new Mock<IWebHostEnvironment>();

        // Setup configuration mock to return values properly
        _configMock.Setup(c => c.GetSection("MaxUploadSizeBytes").Value).Returns("52428800");
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        _controller = new ScanController(
            _serviceMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _environmentMock.Object);
    }

    [Fact]
    public async Task UploadAndScan_NoFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UploadAndScan(null!, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Equal(400, errorResponse.StatusCode);
        Assert.Contains("No file uploaded", errorResponse.Error);
    }

    [Fact]
    public async Task UploadAndScan_NonZipFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.txt");
        fileMock.Setup(f => f.Length).Returns(1024);

        // Act
        var result = await _controller.UploadAndScan(fileMock.Object, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Contains("Only ZIP files are supported", errorResponse.Error);
    }

    [Fact]
    public async Task UploadAndScan_FileTooLarge_Returns413()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.zip");
        fileMock.Setup(f => f.Length).Returns(100_000_000); // 100MB

        // Act
        var result = await _controller.UploadAndScan(fileMock.Object, null);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(413, statusCodeResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
        Assert.Contains("File size exceeds maximum", errorResponse.Error);
    }

    [Fact]
    public async Task UploadAndScan_ValidFile_ReturnsOk()
    {
        // Arrange
        var content = "Test ZIP content"u8.ToArray();
        var stream = new MemoryStream(content);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.zip");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var scanResult = new ScanResult 
        { 
            Id = "test-id",
            ProjectName = "Test Project",
            Technologies = new(),
            LanguageCounts = new()
        };

        _serviceMock.Setup(s => s.CheckDuplicateAsync(It.IsAny<string>()))
            .ReturnsAsync((ScanResult?)null);
        
        _serviceMock.Setup(s => s.ScanProjectAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(scanResult);

        // Act
        var result = await _controller.UploadAndScan(fileMock.Object, "Test Project");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedScan = Assert.IsType<ScanResult>(okResult.Value);
        Assert.Equal("test-id", returnedScan.Id);
        Assert.Equal("Test Project", returnedScan.ProjectName);
    }

    [Fact]
    public async Task UploadAndScan_DuplicateDetected_ReturnsConflict()
    {
        // Arrange
        var content = "Test ZIP content"u8.ToArray();
        var stream = new MemoryStream(content);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.zip");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var existingScan = new ScanResult 
        { 
            Id = "existing-id",
            ProjectName = "Existing Project",
            Timestamp = DateTime.UtcNow.AddDays(-1)
        };

        _serviceMock.Setup(s => s.CheckDuplicateAsync(It.IsAny<string>()))
            .ReturnsAsync(existingScan);

        // Act
        var result = await _controller.UploadAndScan(fileMock.Object, null);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var errorResponse = Assert.IsType<DuplicateScanErrorResponse>(conflictResult.Value);
        Assert.Equal(409, errorResponse.StatusCode);
        Assert.Equal("existing-id", errorResponse.ExistingScanId);
        Assert.Contains("Duplicate scan detected", errorResponse.Error);
    }

    [Fact]
    public async Task GetScanResult_ExistingId_ReturnsOk()
    {
        // Arrange
        var scanId = "test-id";
        var scanResult = new ScanResult { Id = scanId, ProjectName = "Test" };
        _serviceMock.Setup(s => s.GetScanResultAsync(scanId))
            .ReturnsAsync(scanResult);

        // Act
        var result = await _controller.GetScanResult(scanId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedScan = Assert.IsType<ScanResult>(okResult.Value);
        Assert.Equal(scanId, returnedScan.Id);
    }

    [Fact]
    public async Task GetScanResult_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var scanId = "non-existing";
        _serviceMock.Setup(s => s.GetScanResultAsync(scanId))
            .ReturnsAsync((ScanResult?)null);

        // Act
        var result = await _controller.GetScanResult(scanId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal(404, errorResponse.StatusCode);
    }

    [Fact]
    public async Task GetScanHistory_ValidParameters_ReturnsOk()
    {
        // Arrange
        var paginatedResponse = new PaginatedResponse<ScanResult>
        {
            Results = new List<ScanResult>(),
            Pagination = new PaginationMetadata
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalResults = 0,
                TotalPages = 0
            }
        };
        _serviceMock.Setup(s => s.GetAllScanResultsAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(paginatedResponse);

        // Act
        var result = await _controller.GetScanHistory();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedResponse<ScanResult>>(okResult.Value);
        Assert.NotNull(response.Pagination);
    }

    [Theory]
    [InlineData(0, 10)] // Invalid page
    [InlineData(-1, 10)] // Negative page
    public async Task GetScanHistory_InvalidPage_ReturnsBadRequest(int page, int pageSize)
    {
        // Act
        var result = await _controller.GetScanHistory(page: page, pageSize: pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Contains("Page number must be greater than 0", errorResponse.Details);
    }

    [Theory]
    [InlineData(1, 0)] // Zero page size
    [InlineData(1, 101)] // Too large
    public async Task GetScanHistory_InvalidPageSize_ReturnsBadRequest(int page, int pageSize)
    {
        // Act
        var result = await _controller.GetScanHistory(page: page, pageSize: pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
        Assert.Contains("Page size must be between 1 and 100", errorResponse.Details);
    }

    [Fact]
    public async Task DeleteScanResult_ExistingId_ReturnsOk()
    {
        // Arrange
        var scanId = "test-id";
        _serviceMock.Setup(s => s.DeleteScanResultAsync(scanId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteScanResult(scanId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var deleteResponse = Assert.IsType<DeleteResponse>(okResult.Value);
        Assert.Equal(scanId, deleteResponse.DeletedId);
    }

    [Fact]
    public async Task DeleteScanResult_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var scanId = "non-existing";
        _serviceMock.Setup(s => s.DeleteScanResultAsync(scanId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteScanResult(scanId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var errorResponse = Assert.IsType<ErrorResponse>(notFoundResult.Value);
        Assert.Equal(404, errorResponse.StatusCode);
    }
}
