using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ShipFood.Services;

namespace ShipFoodCore.Tests.Services;

/// <summary>
/// Tests for MoMoService — MoMo payment integration.
/// Tests verify IPN signature verification (public method) and payment request construction.
/// CRITICAL: If VerifyIpnSignature is bypassed, fake payments get approved.
/// </summary>
public class MoMoServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger<MoMoService>> _mockLogger;
    private readonly Mock<HttpClient> _mockHttpClient;

    public MoMoServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<MoMoService>>();
        _mockHttpClient = new Mock<HttpClient>();

        // Set up environment variables for MoMoService constructor
        Environment.SetEnvironmentVariable("MOMO_ENDPOINT", "https://test-payment.momo.vn/v2/gateway/api/create");
        Environment.SetEnvironmentVariable("MOMO_PARTNER_CODE", "MOMOTEST");
        Environment.SetEnvironmentVariable("MOMO_ACCESS_KEY", "test_access_key");
        Environment.SetEnvironmentVariable("MOMO_SECRET_KEY", "test_secret_key_12345");
    }

    [Fact]
    public void VerifyIpnSignature_ValidSignature_ReturnsTrue()
    {
        // Arrange
        var service = new MoMoService(_mockConfig.Object, _mockLogger.Object, new HttpClient());

        // Build a valid IPN signature using the known secret key
        var secretKey = "test_secret_key_12345";
        var ipnParams = new Dictionary<string, string>
        {
            { "accessKey", "test_access_key" },
            { "amount", "50000" },
            { "extraData", "" },
            { "orderId", "FS1_20260715120000" },
            { "orderInfo", "Thanh toan don hang FastShip #1" },
            { "orderType", "momo_wallet" },
            { "partnerCode", "MOMOTEST" },
            { "payType", "QR" },
            { "requestId", "FS1_20260715120000_20260715120000" },
            { "responseTime", "1689420000000" },
            { "resultCode", "0" },
            { "transId", "12345678" }
        };

        // Compute expected signature
        var keys = ipnParams.Keys.Where(k => k != "signature").OrderBy(k => k).ToList();
        var rawSignature = string.Join("&", keys.Select(k => $"{k}={ipnParams[k]}"));
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawSignature));
        ipnParams["signature"] = Convert.ToHexString(hash).Replace("-", "").ToLower();

        // Act
        var result = service.VerifyIpnSignature(ipnParams);

        // Assert
        Assert.True(result, "Valid IPN signature should be accepted");
    }

    [Fact]
    public void VerifyIpnSignature_TamperedAmount_ReturnsFalse()
    {
        // Arrange
        var service = new MoMoService(_mockConfig.Object, _mockLogger.Object, new HttpClient());
        var secretKey = "test_secret_key_12345";

        var ipnParams = new Dictionary<string, string>
        {
            { "accessKey", "test_access_key" },
            { "amount", "50000" },
            { "extraData", "" },
            { "orderId", "FS1_20260715120000" },
            { "orderInfo", "Thanh toan" },
            { "orderType", "momo_wallet" },
            { "partnerCode", "MOMOTEST" },
            { "payType", "QR" },
            { "requestId", "req1" },
            { "responseTime", "1689420000000" },
            { "resultCode", "0" },
            { "transId", "12345678" }
        };

        // Compute valid signature
        var keys = ipnParams.Keys.Where(k => k != "signature").OrderBy(k => k).ToList();
        var rawSignature = string.Join("&", keys.Select(k => $"{k}={ipnParams[k]}"));
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawSignature));
        ipnParams["signature"] = Convert.ToHexString(hash).Replace("-", "").ToLower();

        // Now tamper the amount AFTER computing signature
        ipnParams["amount"] = "99999";

        // Act
        var result = service.VerifyIpnSignature(ipnParams);

        // Assert
        Assert.False(result, "Tampered amount must be rejected");
    }

    [Fact]
    public void VerifyIpnSignature_TamperedOrderId_ReturnsFalse()
    {
        // Arrange
        var service = new MoMoService(_mockConfig.Object, _mockLogger.Object, new HttpClient());
        var secretKey = "test_secret_key_12345";

        var ipnParams = new Dictionary<string, string>
        {
            { "accessKey", "test_access_key" },
            { "amount", "50000" },
            { "extraData", "" },
            { "orderId", "FS1_20260715120000" },
            { "orderInfo", "Thanh toan" },
            { "orderType", "momo_wallet" },
            { "partnerCode", "MOMOTEST" },
            { "payType", "QR" },
            { "requestId", "req1" },
            { "responseTime", "1689420000000" },
            { "resultCode", "0" },
            { "transId", "12345678" }
        };

        var keys = ipnParams.Keys.Where(k => k != "signature").OrderBy(k => k).ToList();
        var rawSignature = string.Join("&", keys.Select(k => $"{k}={ipnParams[k]}"));
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawSignature));
        ipnParams["signature"] = Convert.ToHexString(hash).Replace("-", "").ToLower();

        // Tamper orderId
        ipnParams["orderId"] = "FS999_20260715120000";

        // Act
        var result = service.VerifyIpnSignature(ipnParams);

        // Assert
        Assert.False(result, "Tampered orderId must be rejected");
    }

    [Fact]
    public void VerifyIpnSignature_WrongSecretKey_ReturnsFalse()
    {
        // Arrange — use wrong secret key to compute signature
        var service = new MoMoService(_mockConfig.Object, _mockLogger.Object, new HttpClient());

        var ipnParams = new Dictionary<string, string>
        {
            { "accessKey", "test_access_key" },
            { "amount", "50000" },
            { "extraData", "" },
            { "orderId", "FS1_20260715120000" },
            { "orderInfo", "Thanh toan" },
            { "orderType", "momo_wallet" },
            { "partnerCode", "MOMOTEST" },
            { "payType", "QR" },
            { "requestId", "req1" },
            { "responseTime", "1689420000000" },
            { "resultCode", "0" },
            { "transId", "12345678" }
        };

        // Sign with WRONG key
        var keys = ipnParams.Keys.Where(k => k != "signature").OrderBy(k => k).ToList();
        var rawSignature = string.Join("&", keys.Select(k => $"{k}={ipnParams[k]}"));
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes("wrong_secret_key"));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawSignature));
        ipnParams["signature"] = Convert.ToHexString(hash).Replace("-", "").ToLower();

        // Act
        var result = service.VerifyIpnSignature(ipnParams);

        // Assert
        Assert.False(result, "Wrong secret key must produce invalid signature");
    }

    [Fact]
    public void VerifyIpnSignature_MissingSignature_ReturnsFalse()
    {
        // Arrange
        var service = new MoMoService(_mockConfig.Object, _mockLogger.Object, new HttpClient());
        var ipnParams = new Dictionary<string, string>
        {
            { "amount", "50000" },
            { "orderId", "FS1" }
            // No "signature" key
        };

        // Act
        var result = service.VerifyIpnSignature(ipnParams);

        // Assert
        Assert.False(result, "Missing signature must return false");
    }

    [Fact]
    public void VerifyIpnSignature_EmptyParams_ReturnsFalse()
    {
        // Arrange
        var service = new MoMoService(_mockConfig.Object, _mockLogger.Object, new HttpClient());

        // Act
        var result = service.VerifyIpnSignature(new Dictionary<string, string>());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyIpnSignature_ExtraParamInSignature_AffectsResult()
    {
        // Arrange — MoMo builds raw signature sorted by key, excluding "signature"
        var service = new MoMoService(_mockConfig.Object, _mockLogger.Object, new HttpClient());
        var secretKey = "test_secret_key_12345";

        var ipnParams = new Dictionary<string, string>
        {
            { "accessKey", "test_access_key" },
            { "amount", "50000" },
            { "extraData", "" },
            { "orderId", "FS1_20260715120000" },
            { "partnerCode", "MOMOTEST" },
            { "resultCode", "0" }
        };

        // Compute signature without extra params
        var keys = ipnParams.Keys.Where(k => k != "signature").OrderBy(k => k).ToList();
        var rawSignature = string.Join("&", keys.Select(k => $"{k}={ipnParams[k]}"));
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawSignature));
        ipnParams["signature"] = Convert.ToHexString(hash).Replace("-", "").ToLower();

        // Now add extra param that wasn't in the original signature
        ipnParams["unexpectedParam"] = "injected";

        // Act — VerifyIpnSignature rebuilds raw signature with ALL non-signature keys (sorted)
        var result = service.VerifyIpnSignature(ipnParams);

        // Assert — Adding a key changes the sorted signature → false
        Assert.False(result, "Extra param changes signature order — must be rejected");
    }
}