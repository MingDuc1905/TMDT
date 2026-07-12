using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ShipFood.Services;

/// <summary>
/// MoMo Payment Integration Service (Sandbox)
/// Xử lý tạo request thanh toán MoMo và kiểm tra trạng thái giao dịch
/// </summary>
public class MoMoService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MoMoService> _logger;
    private readonly HttpClient _httpClient;

    // MoMo API credentials — chỉ đọc từ Environment Variables, KHÔNG hardcode
    private readonly string _endpoint;
    private readonly string _partnerCode;
    private readonly string _accessKey;
    private readonly string _secretKey;

    public MoMoService(IConfiguration configuration, ILogger<MoMoService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        // ════════════════════════════════════════════════════════════
        // ⚠️  BẢO MẬT: Toàn bộ MoMo credentials đọc từ biến môi trường
        //     Không hardcode bất kỳ key nào trong mã nguồn!
        //
        // Các biến môi trường cần cấu hình (trên Render hoặc .env):
        //   MOMO_ENDPOINT      = https://test-payment.momo.vn/v2/gateway/api/create
        //   MOMO_PARTNER_CODE  = (PartnerCode do MoMo cấp)
        //   MOMO_ACCESS_KEY    = (AccessKey do MoMo cấp)
        //   MOMO_SECRET_KEY    = (SecretKey do MoMo cấp)
        //   MOMO_QUERY_ENDPOINT= https://test-payment.momo.vn/v2/gateway/api/query
        // ════════════════════════════════════════════════════════════

        _endpoint = GetRequiredEnv("MOMO_ENDPOINT", "https://test-payment.momo.vn/v2/gateway/api/create");
        _partnerCode = GetRequiredEnv("MOMO_PARTNER_CODE");
        _accessKey = GetRequiredEnv("MOMO_ACCESS_KEY");
        _secretKey = GetRequiredEnv("MOMO_SECRET_KEY");
    }

    /// <summary>
    /// Đọc biến môi trường, throw exception nếu thiếu (trừ khi có defaultValue)
    /// </summary>
    private string GetRequiredEnv(string name, string? defaultValue = null)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrEmpty(value))
            return value;

        // Thử đọc từ IConfiguration (appsettings.json / secret.json)
        value = _configuration[name.Replace("_", ":")];
        if (!string.IsNullOrEmpty(value))
            return value;

        if (defaultValue != null)
        {
            _logger.LogWarning("MoMo: Environment variable {VarName} not set — using default value (development only)", name);
            return defaultValue;
        }

        var msg = $"MoMo: Environment variable '{name}' is not set. " +
                  $"Please configure it on Render (Settings → Environment Variables) or locally in .env file.";
        _logger.LogError(msg);
        throw new InvalidOperationException(msg);
    }

    /// <summary>
    /// Tạo yêu cầu thanh toán MoMo
    /// </summary>
    public async Task<MoMoCreatePaymentResponse> CreatePaymentAsync(MoMoCreatePaymentRequest request)
    {
        try
        {
            var orderId = request.OrderId;
            var orderInfo = request.OrderInfo;
            var amount = request.Amount.ToString("F0");
            var extraData = request.ExtraData ?? "";

            // Tạo requestId unique
            var requestId = $"{orderId}_{DateTime.Now:yyyyMMddHHmmss}";

            // Build raw signature
            var rawSignature = $"accessKey={_accessKey}&amount={amount}&extraData={extraData}&ipnUrl={request.IpnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_partnerCode}&redirectUrl={request.RedirectUrl}&requestId={requestId}&requestType={request.RequestType}";

            // HMAC SHA256 signature
            var signature = ComputeHmacSha256(rawSignature, _secretKey);

            var payload = new Dictionary<string, object>
            {
                { "partnerCode", _partnerCode },
                { "partnerName", "FastShip" },
                { "storeId", "FastShipStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", request.RedirectUrl },
                { "ipnUrl", request.IpnUrl },
                { "lang", "vi" },
                { "extraData", extraData },
                { "requestType", request.RequestType },
                { "signature", signature },
                { "autoCapture", true }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            _logger.LogInformation("MoMo CreatePayment: OrderId={OrderId}, Amount={Amount}", orderId, amount);

            var response = await _httpClient.PostAsync(_endpoint, jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("MoMo CreatePayment Response: {Response}", responseBody);

            var result = JsonSerializer.Deserialize<MoMoCreatePaymentResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new MoMoCreatePaymentResponse
            {
                ResultCode = -1,
                Message = "Failed to parse MoMo response"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoMo CreatePayment failed for OrderId={OrderId}", request.OrderId);
            return new MoMoCreatePaymentResponse
            {
                ResultCode = -1,
                Message = $"Lỗi kết nối MoMo: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái giao dịch MoMo
    /// </summary>
    public async Task<MoMoTransactionStatusResponse> CheckTransactionAsync(string orderId)
    {
        try
        {
            var requestId = $"check_{orderId}_{DateTime.Now:yyyyMMddHHmmss}";
            var rawSignature = $"accessKey={_accessKey}&orderId={orderId}&partnerCode={_partnerCode}&requestId={requestId}";
            var signature = ComputeHmacSha256(rawSignature, _secretKey);

            var payload = new Dictionary<string, object>
            {
                { "partnerCode", _partnerCode },
                { "requestId", requestId },
                { "orderId", orderId },
                { "signature", signature },
                { "lang", "vi" }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var queryEndpoint = Environment.GetEnvironmentVariable("MOMO_QUERY_ENDPOINT") ?? "https://test-payment.momo.vn/v2/gateway/api/query";

            var response = await _httpClient.PostAsync(queryEndpoint, jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<MoMoTransactionStatusResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new MoMoTransactionStatusResponse
            {
                ResultCode = -1,
                Message = "Failed to parse MoMo response"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoMo CheckTransaction failed for OrderId={OrderId}", orderId);
            return new MoMoTransactionStatusResponse
            {
                ResultCode = -1,
                Message = $"Lỗi kết nối MoMo: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Hoàn tiền MoMo (Refund) — gọi API refund của MoMo Sandbox
    /// Được sử dụng khi hủy đơn hàng đã thanh toán qua MoMo
    /// </summary>
    public async Task<MoMoCreatePaymentResponse> RefundAsync(string orderId, long amount, string description = "", long? transId = null)
    {
        try
        {
            var refundId = $"REFUND_{orderId}_{DateTime.Now:yyyyMMddHHmmss}";
            var transIdValue = transId?.ToString() ?? "";
            // MoMo Refund API yêu cầu transId trong raw signature nếu có
            var rawSignature = string.IsNullOrEmpty(transIdValue)
                ? $"accessKey={_accessKey}&amount={amount}&description={description}&orderId={orderId}&partnerCode={_partnerCode}&requestId={refundId}&requestType=refund"
                : $"accessKey={_accessKey}&amount={amount}&description={description}&orderId={orderId}&partnerCode={_partnerCode}&requestId={refundId}&requestType=refund&transId={transIdValue}";
            var signature = ComputeHmacSha256(rawSignature, _secretKey);

            var refundEndpoint = Environment.GetEnvironmentVariable("MOMO_REFUND_ENDPOINT")
                ?? "https://test-payment.momo.vn/v2/gateway/api/refund";

            var payload = new Dictionary<string, object>
            {
                { "partnerCode", _partnerCode },
                { "requestId", refundId },
                { "orderId", orderId },
                { "amount", amount },
                { "description", description },
                { "transId", transIdValue },
                { "requestType", "refund" },
                { "signature", signature },
                { "lang", "vi" }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            _logger.LogInformation("MoMo Refund: OrderId={OrderId}, Amount={Amount}", orderId, amount);

            var response = await _httpClient.PostAsync(refundEndpoint, jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("MoMo Refund Response: {Response}", responseBody);

            var result = JsonSerializer.Deserialize<MoMoCreatePaymentResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new MoMoCreatePaymentResponse
            {
                ResultCode = -1,
                Message = "Failed to parse MoMo refund response"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoMo Refund failed for OrderId={OrderId}", orderId);
            return new MoMoCreatePaymentResponse
            {
                ResultCode = -1,
                Message = $"Lỗi kết nối MoMo Refund: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Xác thực signature từ MoMo IPN callback
    /// </summary>
    public bool VerifyIpnSignature(Dictionary<string, string> ipnParams)
    {
        try
        {
            // MoMo gửi signature trong params
            if (!ipnParams.TryGetValue("signature", out var receivedSignature))
                return false;

            // Build raw signature từ các params (loại trừ signature)
            var keys = ipnParams.Keys.Where(k => k != "signature").OrderBy(k => k).ToList();
            var rawSignature = string.Join("&", keys.Select(k => $"{k}={ipnParams[k]}"));

            var computedSignature = ComputeHmacSha256(rawSignature, _secretKey);
            return computedSignature == receivedSignature;
        }
        catch
        {
            return false;
        }
    }

    private static string ComputeHmacSha256(string message, string secretKey)
    {
        var encoding = Encoding.UTF8;
        using var hmac = new HMACSHA256(encoding.GetBytes(secretKey));
        var hash = hmac.ComputeHash(encoding.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

// ─── DTOs ───

public class MoMoCreatePaymentRequest
{
    public string OrderId { get; set; } = "";
    public string OrderInfo { get; set; } = "";
    public long Amount { get; set; }
    public string RedirectUrl { get; set; } = "";
    public string IpnUrl { get; set; } = "";
    public string RequestType { get; set; } = "captureWallet";
    public string? ExtraData { get; set; }
    public string? Lang { get; set; } = "vi";
}

public class MoMoCreatePaymentResponse
{
    public string? PartnerCode { get; set; }
    public string? OrderId { get; set; }
    public string? RequestId { get; set; }
    public int ResultCode { get; set; }
    public string? Message { get; set; }
    public string? PayUrl { get; set; }
    public string? DeepLink { get; set; }
    public string? QrCodeUrl { get; set; }
    public long? Amount { get; set; }
    public string? ResponseTime { get; set; }

    public bool IsSuccess => ResultCode == 0;
}

public class MoMoTransactionStatusResponse
{
    public string? PartnerCode { get; set; }
    public string? OrderId { get; set; }
    public string? RequestId { get; set; }
    public int ResultCode { get; set; }
    public string? Message { get; set; }
    public long? Amount { get; set; }
    public long? TransId { get; set; }
    public string? PayType { get; set; }
    public long? ResponseTime { get; set; }
    public string? ExtraData { get; set; }

    public bool IsSuccess => ResultCode == 0;
}
