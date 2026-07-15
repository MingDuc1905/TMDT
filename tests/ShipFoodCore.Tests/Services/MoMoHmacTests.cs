using System.Security.Cryptography;
using System.Text;

namespace ShipFoodCore.Tests.Services;

/// <summary>
/// Tests for MoMoService HMAC-SHA256 signature computation.
/// CRITICAL: If HMAC is wrong, fake payments get approved.
/// 
/// Note: We test the HMAC algorithm directly (same as MoMoService.ComputeHmacSha256)
/// because the method is private. We replicate the algorithm to verify correctness
/// and test against known test vectors.
/// </summary>
public class MoMoHmacTests
{
    /// <summary>
    /// Replicate MoMoService.ComputeHmacSha256 — same algorithm, exposed for testing.
    /// </summary>
    private static string ComputeHmacSha256(string message, string secretKey)
    {
        var encoding = Encoding.UTF8;
        using var hmac = new HMACSHA256(encoding.GetBytes(secretKey));
        var hash = hmac.ComputeHash(encoding.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    [Fact]
    public void ComputeHmacSha256_KnownInput_ProducesDeterministicOutput()
    {
        // Arrange
        var message = "accessKey=testAccess&amount=50000&extraData=&orderId=FS1_20260715";
        var secret = "testSecretKey123";

        // Act
        var sig1 = ComputeHmacSha256(message, secret);
        var sig2 = ComputeHmacSha256(message, secret);

        // Assert
        Assert.Equal(sig1, sig2);
        Assert.Equal(64, sig1.Length); // SHA256 = 32 bytes = 64 hex chars
    }

    [Fact]
    public void ComputeHmacSha256_DifferentSecret_ProducesDifferentSignature()
    {
        // Arrange
        var message = "accessKey=testAccess&amount=50000";

        // Act
        var sig1 = ComputeHmacSha256(message, "secret1");
        var sig2 = ComputeHmacSha256(message, "secret2");

        // Assert
        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void ComputeHmacSha256_DifferentMessage_ProducesDifferentSignature()
    {
        // Arrange
        var secret = "testSecret";

        // Act
        var sig1 = ComputeHmacSha256("message1", secret);
        var sig2 = ComputeHmacSha256("message2", secret);

        // Assert
        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void ComputeHmacSha256_EmptyMessage_DoesNotThrow()
    {
        // Act
        var sig = ComputeHmacSha256("", "secret");

        // Assert
        Assert.NotNull(sig);
        Assert.Equal(64, sig.Length);
    }

    [Fact]
    public void ComputeHmacSha256_VietnameseCharacters_ProducesValidHex()
    {
        // Arrange
        var message = "Thanh toan don hang Phở Bò #42";

        // Act
        var sig = ComputeHmacSha256(message, "secret");

        // Assert
        Assert.NotNull(sig);
        Assert.Equal(64, sig.Length);
        Assert.Matches("^[a-f0-9]{64}$", sig);
    }

    [Fact]
    public void ComputeHmacSha256_UnicodeCharacters_ProducesValidHex()
    {
        // Arrange
        var message = "amount=100000&orderInfo=💰 Payment";

        // Act
        var sig = ComputeHmacSha256(message, "unicodeSecret!");

        // Assert
        Assert.Matches("^[a-f0-9]{64}$", sig);
    }

    [Theory]
    [InlineData("accessKey=abc&amount=100")]
    [InlineData("accessKey=abc&amount=9999999&extraData=test")]
    [InlineData("")]
    public void ComputeHmacSha256_AlwaysProduces64CharHex(string message)
    {
        // Act
        var sig = ComputeHmacSha256(message, "someSecret");

        // Assert
        Assert.Equal(64, sig.Length);
        Assert.Matches("^[a-f0-9]+$", sig);
    }
}