using System.Security.Cryptography;

namespace Core.Utils;

public static class HashHelper
{
    public static string ComputeHash(byte[] data)
    {
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(data);
        return BitConverter.ToString(hashBytes)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }
}