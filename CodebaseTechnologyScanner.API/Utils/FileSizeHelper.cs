namespace CodebaseTechnologyScanner.API.Utils;

/// <summary>
/// Utility class for file size conversions and formatting.
/// </summary>
public static class FileSizeHelper
{
    private const long BytesPerMegabyte = 1024 * 1024;

    /// <summary>
    /// Converts megabytes to bytes.
    /// </summary>
    public static long MegabytesToBytes(long megabytes) => megabytes * BytesPerMegabyte;
    
    /// <summary>
    /// Converts bytes to megabytes.
    /// </summary>
    public static long BytesToMegabytes(long bytes) => bytes / BytesPerMegabyte;
    
    /// <summary>
    /// Formats bytes as a human-readable string.
    /// </summary>
    public static string FormatBytes(long bytes) => $"{BytesToMegabytes(bytes)} MB";
}
