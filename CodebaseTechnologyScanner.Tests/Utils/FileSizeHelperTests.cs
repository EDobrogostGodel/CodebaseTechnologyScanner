using CodebaseTechnologyScanner.API.Utils;

namespace CodebaseTechnologyScanner.Tests.Utils;

/// <summary>
/// Unit tests for FileSizeHelper utility class.
/// </summary>
public class FileSizeHelperTests
{
    [Theory]
    [InlineData(0, "0 MB")]
    [InlineData(1048576, "1 MB")] // 1 MB
    [InlineData(52428800, "50 MB")] // 50 MB
    [InlineData(1073741824, "1024 MB")] // 1 GB
    public void FormatBytes_VariousSizes_FormatsCorrectly(long bytes, string expected)
    {
        // Act
        var result = FileSizeHelper.FormatBytes(bytes);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, 1048576)]
    [InlineData(50, 52428800)]
    [InlineData(100, 104857600)]
    public void MegabytesToBytes_VariousValues_ConvertsCorrectly(long mb, long expectedBytes)
    {
        // Act
        var result = FileSizeHelper.MegabytesToBytes(mb);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Theory]
    [InlineData(1048576, 1)]
    [InlineData(52428800, 50)]
    [InlineData(104857600, 100)]
    public void BytesToMegabytes_VariousValues_ConvertsCorrectly(long bytes, long expectedMb)
    {
        // Act
        var result = FileSizeHelper.BytesToMegabytes(bytes);

        // Assert
        Assert.Equal(expectedMb, result);
    }

    [Fact]
    public void BytesToMegabytes_ZeroBytes_ReturnsZero()
    {
        // Act
        var result = FileSizeHelper.BytesToMegabytes(0);

        // Assert
        Assert.Equal(0, result);
    }
}
