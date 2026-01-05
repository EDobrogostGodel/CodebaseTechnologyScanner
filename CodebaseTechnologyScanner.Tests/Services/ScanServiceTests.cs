using CodebaseTechnologyScanner.API.Models;
using CodebaseTechnologyScanner.API.Repositories;
using CodebaseTechnologyScanner.API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CodebaseTechnologyScanner.Tests.Services;

/// <summary>
/// Unit tests for ScanService business logic.
/// Tests pagination, filtering, sorting, and scan operations.
/// </summary>
public class ScanServiceTests
{
    private readonly Mock<IScanRepository> _repositoryMock;
    private readonly Mock<TechnologyDetector> _detectorMock;
    private readonly Mock<ILogger<ScanService>> _loggerMock;
    private readonly ScanService _service;

    public ScanServiceTests()
    {
        _repositoryMock = new Mock<IScanRepository>();
        _loggerMock = new Mock<ILogger<ScanService>>();
        
        var detectorLoggerMock = new Mock<ILogger<TechnologyDetector>>();
        _detectorMock = new Mock<TechnologyDetector>(detectorLoggerMock.Object);
        
        _service = new ScanService(
            _repositoryMock.Object,
            _detectorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetScanResultAsync_ExistingId_ReturnsResult()
    {
        // Arrange
        var scanId = Guid.NewGuid().ToString();
        var expectedResult = new ScanResult { Id = scanId, ProjectName = "Test" };
        _repositoryMock.Setup(r => r.GetScanResultByIdAsync(scanId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetScanResultAsync(scanId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(scanId, result.Id);
        Assert.Equal("Test", result.ProjectName);
    }

    [Fact]
    public async Task GetScanResultAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var scanId = Guid.NewGuid().ToString();
        _repositoryMock.Setup(r => r.GetScanResultByIdAsync(scanId))
            .ReturnsAsync((ScanResult?)null);

        // Act
        var result = await _service.GetScanResultAsync(scanId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllScanResultsAsync_WithProjectNameFilter_ReturnsFiltered()
    {
        // Arrange
        var scans = new List<ScanResult>
        {
            new() { Id = "1", ProjectName = "React App", Timestamp = DateTime.UtcNow },
            new() { Id = "2", ProjectName = "Vue App", Timestamp = DateTime.UtcNow },
            new() { Id = "3", ProjectName = "React Dashboard", Timestamp = DateTime.UtcNow }
        };
        _repositoryMock.Setup(r => r.GetAllScanResultsAsync())
            .ReturnsAsync(scans);

        // Act
        var result = await _service.GetAllScanResultsAsync(
            page: 1, 
            pageSize: 10, 
            projectName: "React");

        // Assert
        Assert.Equal(2, result.Results.Count);
        Assert.All(result.Results, r => Assert.Contains("React", r.ProjectName));
    }

    [Fact]
    public async Task GetAllScanResultsAsync_WithLanguageFilter_ReturnsMatching()
    {
        // Arrange
        var scans = new List<ScanResult>
        {
            new() { 
                Id = "1", 
                ProjectName = "App1",
                LanguageCounts = new() { { "TypeScript", 10 }, { "JavaScript", 5 } }
            },
            new() { 
                Id = "2", 
                ProjectName = "App2",
                LanguageCounts = new() { { "C#", 20 } }
            },
            new() { 
                Id = "3", 
                ProjectName = "App3",
                LanguageCounts = new() { { "TypeScript", 15 } }
            }
        };
        _repositoryMock.Setup(r => r.GetAllScanResultsAsync())
            .ReturnsAsync(scans);

        // Act
        var result = await _service.GetAllScanResultsAsync(
            page: 1,
            pageSize: 10,
            language: "TypeScript");

        // Assert
        Assert.Equal(2, result.Results.Count);
        Assert.All(result.Results, r => Assert.Contains("TypeScript", r.LanguageCounts.Keys));
    }

    [Fact]
    public async Task GetAllScanResultsAsync_SortByTimestampDesc_ReturnsSorted()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var scans = new List<ScanResult>
        {
            new() { Id = "1", ProjectName = "First", Timestamp = now.AddDays(-2) },
            new() { Id = "2", ProjectName = "Second", Timestamp = now.AddDays(-1) },
            new() { Id = "3", ProjectName = "Third", Timestamp = now }
        };
        _repositoryMock.Setup(r => r.GetAllScanResultsAsync())
            .ReturnsAsync(scans);

        // Act
        var result = await _service.GetAllScanResultsAsync(
            page: 1,
            pageSize: 10,
            sortBy: "timestamp",
            sortOrder: "desc");

        // Assert
        Assert.Equal("Third", result.Results[0].ProjectName);
        Assert.Equal("Second", result.Results[1].ProjectName);
        Assert.Equal("First", result.Results[2].ProjectName);
    }

    [Fact]
    public async Task GetAllScanResultsAsync_SortByProjectNameAsc_ReturnsSorted()
    {
        // Arrange
        var scans = new List<ScanResult>
        {
            new() { Id = "1", ProjectName = "Zebra" },
            new() { Id = "2", ProjectName = "Apple" },
            new() { Id = "3", ProjectName = "Mango" }
        };
        _repositoryMock.Setup(r => r.GetAllScanResultsAsync())
            .ReturnsAsync(scans);

        // Act
        var result = await _service.GetAllScanResultsAsync(
            page: 1,
            pageSize: 10,
            sortBy: "projectName",
            sortOrder: "asc");

        // Assert
        Assert.Equal("Apple", result.Results[0].ProjectName);
        Assert.Equal("Mango", result.Results[1].ProjectName);
        Assert.Equal("Zebra", result.Results[2].ProjectName);
    }

    [Fact]
    public async Task GetAllScanResultsAsync_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var scans = Enumerable.Range(1, 25)
            .Select(i => new ScanResult 
            { 
                Id = i.ToString(), 
                ProjectName = $"Project{i}",
                Timestamp = baseTime.AddMinutes(i) // Make timestamps unique and ordered
            })
            .ToList();
        _repositoryMock.Setup(r => r.GetAllScanResultsAsync())
            .ReturnsAsync(scans);

        // Act
        var result = await _service.GetAllScanResultsAsync(
            page: 2,
            pageSize: 10,
            sortBy: "timestamp",
            sortOrder: "desc");

        // Assert
        Assert.Equal(10, result.Results.Count);
        Assert.Equal("Project15", result.Results[0].ProjectName); // Descending order: 25, 24, 23... -> Page 2 starts at 15
        Assert.Equal("Project6", result.Results[9].ProjectName);
        Assert.Equal(25, result.Pagination.TotalResults);
        Assert.Equal(3, result.Pagination.TotalPages);
        Assert.Equal(2, result.Pagination.CurrentPage);
        Assert.True(result.Pagination.HasNextPage);
        Assert.True(result.Pagination.HasPreviousPage);
    }

    [Fact]
    public async Task CheckDuplicateAsync_ExistingHash_ReturnsExisting()
    {
        // Arrange
        var hash = "abc123";
        var existingScan = new ScanResult { Id = "1", FileHash = hash };
        _repositoryMock.Setup(r => r.GetScanResultByHashAsync(hash))
            .ReturnsAsync(existingScan);

        // Act
        var result = await _service.CheckDuplicateAsync(hash);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hash, result.FileHash);
    }

    [Fact]
    public async Task DeleteScanResultAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var scanId = "test-id";
        _repositoryMock.Setup(r => r.DeleteScanResultAsync(scanId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteScanResultAsync(scanId);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteScanResultAsync(scanId), Times.Once);
    }

    [Fact]
    public async Task DeleteScanResultAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var scanId = "non-existing";
        _repositoryMock.Setup(r => r.DeleteScanResultAsync(scanId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteScanResultAsync(scanId);

        // Assert
        Assert.False(result);
    }
}
