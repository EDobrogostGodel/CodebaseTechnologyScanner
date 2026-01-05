using CodebaseTechnologyScanner.API.Utils;

namespace CodebaseTechnologyScanner.Tests.Utils;

/// <summary>
/// Unit tests for HashHelper utility class.
/// </summary>
public class HashHelperTests
{
    [Fact]
    public async Task CalculateSHA256Async_SameContent_ReturnsSameHash()
    {
        // Arrange
        var content = "Test content for hashing"u8.ToArray();
        using var stream1 = new MemoryStream(content);
        using var stream2 = new MemoryStream(content);

        // Act
        var hash1 = await HashHelper.CalculateSHA256Async(stream1);
        var hash2 = await HashHelper.CalculateSHA256Async(stream2);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.NotEmpty(hash1);
        Assert.Equal(64, hash1.Length); // SHA256 produces 64 hex characters
    }

    [Fact]
    public async Task CalculateSHA256Async_DifferentContent_ReturnsDifferentHash()
    {
        // Arrange
        var content1 = "First content"u8.ToArray();
        var content2 = "Second content"u8.ToArray();
        using var stream1 = new MemoryStream(content1);
        using var stream2 = new MemoryStream(content2);

        // Act
        var hash1 = await HashHelper.CalculateSHA256Async(stream1);
        var hash2 = await HashHelper.CalculateSHA256Async(stream2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async Task CalculateSHA256Async_EmptyStream_ReturnsValidHash()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act
        var hash = await HashHelper.CalculateSHA256Async(emptyStream);

        // Assert
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public async Task CalculateSHA256Async_LargeContent_ReturnsValidHash()
    {
        // Arrange
        var largeContent = new byte[1024 * 1024]; // 1MB
        new Random(42).NextBytes(largeContent);
        using var stream = new MemoryStream(largeContent);

        // Act
        var hash = await HashHelper.CalculateSHA256Async(stream);

        // Assert
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
        Assert.All(hash, c => Assert.True(char.IsLetterOrDigit(c)));
    }
}
