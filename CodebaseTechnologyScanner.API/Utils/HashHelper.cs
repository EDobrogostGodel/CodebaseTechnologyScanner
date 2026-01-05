using System.Security.Cryptography;

namespace CodebaseTechnologyScanner.API.Utils;

/// <summary>
/// Utility class for cryptographic hash operations.
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Calculates SHA256 hash of a stream.
    /// </summary>
    /// <param name="stream">The stream to hash</param>
    /// <returns>Lowercase hexadecimal string representation of the hash</returns>
    public static async Task<string> CalculateSHA256Async(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
