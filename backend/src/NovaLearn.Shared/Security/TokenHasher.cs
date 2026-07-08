using System.Security.Cryptography;
using System.Text;

namespace NovaLearn.Shared.Security;

/// <summary>
/// Hashes opaque bearer secrets (e.g. refresh tokens) before they are persisted.
/// A refresh token is a high-entropy random value, so a fast one-way SHA-256 digest is
/// sufficient and appropriate — unlike passwords, it needs no slow adaptive KDF.
/// Storing only the hash means a database leak does not expose usable tokens.
/// </summary>
public static class TokenHasher
{
    /// <summary>Produces a stable, URL-safe Base64 SHA-256 hash of <paramref name="token"/>.</summary>
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>Constant-time comparison of a raw token against a stored hash.</summary>
    public static bool Verify(string token, string hash)
    {
        string computed = Hash(token);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(hash));
    }
}
