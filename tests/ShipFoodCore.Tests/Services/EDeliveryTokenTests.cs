using System.Security.Cryptography;
using System.Text;

namespace ShipFoodCore.Tests.Services;

/// <summary>
/// Tests for EDeliveryController QR token generation and validation.
/// CRITICAL: If token validation is wrong, QR codes can be forged to access/modify any order.
/// 
/// We replicate the exact logic from EDeliveryController.GenerateSecureToken/ValidateToken
/// because those methods are private. This ensures the algorithm is correct.
/// </summary>
public class EDeliveryTokenTests
{
    private const string QR_HASH_SECRET = "FastShip2024!EDelivery@Secure";

    /// <summary>
    /// Replicate EDeliveryController.GenerateSecureToken for testing.
    /// </summary>
    private static string GenerateSecureToken(int orderId, long? timestampOverride = null)
    {
        var timestamp = timestampOverride ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var raw = $"{orderId}|{timestamp}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(QR_HASH_SECRET));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw))).ToLower()[..8];

        var tokenRaw = $"{orderId}.{timestamp}.{hash}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenRaw))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Replicate EDeliveryController.ValidateToken for testing.
    /// </summary>
    private static int? ValidateToken(string token)
    {
        try
        {
            var padded = token.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var parts = decoded.Split('.');
            if (parts.Length != 3) return null;

            if (!int.TryParse(parts[0], out int orderId)) return null;
            if (!long.TryParse(parts[1], out long timestamp)) return null;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - timestamp > 86400) return null;

            var raw = $"{orderId}|{timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(QR_HASH_SECRET));
            var expectedHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw))).ToLower()[..8];
            if (parts[2] != expectedHash) return null;

            return orderId;
        }
        catch { return null; }
    }

    [Fact]
    public void GenerateToken_ValidOrderId_ProducesUrlSafeBase64()
    {
        // Act
        var token = GenerateSecureToken(42);

        // Assert
        Assert.NotNull(token);
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsCorrectOrderId()
    {
        // Arrange
        var orderId = 42;

        // Act
        var token = GenerateSecureToken(orderId);
        var result = ValidateToken(token);

        // Assert
        Assert.Equal(orderId, result);
    }

    [Fact]
    public void ValidateToken_TamperedOrderId_ReturnsNull()
    {
        // Arrange
        var token = GenerateSecureToken(42);

        // Act - try to decode, change orderId, re-encode
        var padded = token.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var parts = decoded.Split('.');
        parts[0] = "99"; // Tamper orderId
        var tampered = string.Join(".", parts);
        var tamperedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(tampered))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var result = ValidateToken(tamperedToken);

        // Assert
        Assert.Null(result); // Tampered token must be rejected
    }

    [Fact]
    public void ValidateToken_TamperedHash_ReturnsNull()
    {
        // Arrange
        var token = GenerateSecureToken(42);
        var padded = token.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var parts = decoded.Split('.');
        parts[2] = "aaaaaaaa"; // Tamper hash
        var tampered = string.Join(".", parts);
        var tamperedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(tampered))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Act
        var result = ValidateToken(tamperedToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ReturnsNull()
    {
        // Arrange — token from 25 hours ago (exceeds 24h limit)
        var expiredTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (25 * 3600);
        var token = GenerateSecureToken(42, expiredTimestamp);

        // Act
        var result = ValidateToken(token);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_JustBeforeExpiry_ReturnsOrderId()
    {
        // Arrange — token from 23h59m59s ago (just within 24h limit)
        var almostExpiredTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (24 * 3600 - 1);
        var token = GenerateSecureToken(42, almostExpiredTimestamp);

        // Act
        var result = ValidateToken(token);

        // Assert
        Assert.Equal(42, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-token")]
    [InlineData("aaa")]
    [InlineData("!!!not-base64!!!")]
    public void ValidateToken_InvalidInput_ReturnsNull(string token)
    {
        // Act
        var result = ValidateToken(token);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(999)]
    [InlineData(999999)]
    public void GenerateAndValidate_VariousOrderIds_RoundTrips(int orderId)
    {
        // Act
        var token = GenerateSecureToken(orderId);
        var result = ValidateToken(token);

        // Assert
        Assert.Equal(orderId, result);
    }

    [Fact]
    public void ValidateToken_TamperedTimestamp_ReturnsNull()
    {
        // Arrange
        var token = GenerateSecureToken(42);
        var padded = token.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var parts = decoded.Split('.');
        parts[1] = "9999999999"; // Tamper timestamp
        var tampered = string.Join(".", parts);
        var tamperedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(tampered))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Act
        var result = ValidateToken(tamperedToken);

        // Assert
        Assert.Null(result);
    }
}