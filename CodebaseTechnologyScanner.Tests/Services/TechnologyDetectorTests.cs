using CodebaseTechnologyScanner.API.Models;
using CodebaseTechnologyScanner.API.Repositories;
using CodebaseTechnologyScanner.API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CodebaseTechnologyScanner.Tests.Services;

/// <summary>
/// Unit tests for TechnologyDetector service.
/// Validates technology and language detection logic.
/// </summary>
public class TechnologyDetectorTests
{
    private readonly Mock<ILogger<TechnologyDetector>> _loggerMock;
    private readonly TechnologyDetector _detector;

    public TechnologyDetectorTests()
    {
        _loggerMock = new Mock<ILogger<TechnologyDetector>>();
        _detector = new TechnologyDetector(_loggerMock.Object);
    }

    [Fact]
    public void AnalyzeLanguages_CSharpFiles_CountsCorrectly()
    {
        // Arrange
        var tempDir = CreateTempDirectoryWithFiles(new[]
        {
            "Program.cs",
            "Controller.cs",
            "Service.cs",
            "test.txt"
        });

        try
        {
            // Act
            var result = _detector.AnalyzeLanguages(tempDir);

            // Assert
            Assert.Contains("C#", result.Keys);
            Assert.Equal(3, result["C#"]);
            Assert.DoesNotContain("txt", result.Keys);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void AnalyzeLanguages_MultipleLanguages_CountsAll()
    {
        // Arrange
        var tempDir = CreateTempDirectoryWithFiles(new[]
        {
            "App.tsx",
            "Component.tsx",
            "script.js",
            "main.py",
            "test.java"
        });

        try
        {
            // Act
            var result = _detector.AnalyzeLanguages(tempDir);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(2, result["TypeScript"]);
            Assert.Equal(1, result["JavaScript"]);
            Assert.Equal(1, result["Python"]);
            Assert.Equal(1, result["Java"]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void AnalyzeLanguages_NoCodeFiles_ReturnsEmpty()
    {
        // Arrange
        var tempDir = CreateTempDirectoryWithFiles(new[]
        {
            "README.md",
            "data.json",
            "image.png"
        });

        try
        {
            // Act
            var result = _detector.AnalyzeLanguages(tempDir);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DetectTechnologiesAsync_PackageJson_DetectsNpm()
    {
        // Arrange
        var tempDir = CreateTempDirectoryWithFiles(new[] { "package.json" });

        try
        {
            // Act
            var result = await _detector.DetectTechnologiesAsync(tempDir);

            // Assert
            var npmTech = result.FirstOrDefault(t => t.Name == "npm");
            Assert.NotNull(npmTech);
            Assert.Equal("Package Manager", npmTech.Type);
            Assert.Equal("package.json", npmTech.DetectedFrom);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DetectTechnologiesAsync_Dockerfile_DetectsDocker()
    {
        // Arrange
        var tempDir = CreateTempDirectoryWithFiles(new[] { "Dockerfile" });

        try
        {
            // Act
            var result = await _detector.DetectTechnologiesAsync(tempDir);

            // Assert
            var dockerTech = result.FirstOrDefault(t => t.Name == "Docker");
            Assert.NotNull(dockerTech);
            Assert.Equal("Container", dockerTech.Type);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DetectTechnologiesAsync_ViteConfig_DetectsVite()
    {
        // Arrange
        var tempDir = CreateTempDirectoryWithFiles(new[] { "vite.config.ts" });

        try
        {
            // Act
            var result = await _detector.DetectTechnologiesAsync(tempDir);

            // Assert
            var viteTech = result.FirstOrDefault(t => t.Name == "Vite");
            Assert.NotNull(viteTech);
            Assert.Equal("Build Tool", viteTech.Type);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DetectTechnologiesAsync_ValidPackageJsonWithReact_DetectsReact()
    {
        // Arrange
        var packageJsonContent = @"{
            ""dependencies"": {
                ""react"": ""^18.2.0"",
                ""react-dom"": ""^18.2.0""
            }
        }";
        var tempDir = CreateTempDirectoryWithFiles(new[] { "package.json" });
        await File.WriteAllTextAsync(Path.Combine(tempDir, "package.json"), packageJsonContent);

        try
        {
            // Act
            var result = await _detector.DetectTechnologiesAsync(tempDir);

            // Assert
            var reactTech = result.FirstOrDefault(t => t.Name == "React");
            Assert.NotNull(reactTech);
            Assert.Equal("Framework", reactTech.Type);
            Assert.Equal("18.2.0", reactTech.Version);
            Assert.Equal("package.json", reactTech.DetectedFrom);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DetectTechnologiesAsync_InvalidPackageJson_DoesNotThrow()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var tempDir = CreateTempDirectoryWithFiles(new[] { "package.json" });
        await File.WriteAllTextAsync(Path.Combine(tempDir, "package.json"), invalidJson);

        try
        {
            // Act
            var result = await _detector.DetectTechnologiesAsync(tempDir);

            // Assert - Should detect npm but not crash
            var npmTech = result.FirstOrDefault(t => t.Name == "npm");
            Assert.NotNull(npmTech);
            
            // Verify logger was called with warning
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private string CreateTempDirectoryWithFiles(string[] fileNames)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        foreach (var fileName in fileNames)
        {
            File.WriteAllText(Path.Combine(tempDir, fileName), "dummy content");
        }

        return tempDir;
    }
}
