// ============================================================
// 🅿️ PayPalService — Tích hợp thanh toán PayPal (Sandbox)
// ============================================================
// Ý nghĩa: Service kết nối PayPal REST API v2 để tạo Order và Capture payment
// Chức năng: OAuth2 token, Create Order (intent=CAPTURE), Capture, VND→USD conversion
// KEYWORDS: paypal, payment, sandbox, capture, oauth2, order, thanh toán quốc tế
//
// LUỒNG DỮ LIỆU:
//   CartController.Payment ⭢ chọn PayPal ⭢ PaymentController.PayPalCreate()
//   PaymentController.PayPalCreate ⭢ gọi CreateOrderAsync() ⭢ PayPal REST API (sandbox)
//   PayPal trả về approve link ⭢ redirect browser ⭢ user duyệt trên PayPal
//   User duyệt xong ⭢ PayPal redirect về PaymentController.PayPalReturn()
//   PaymentController.PayPalReturn ⭢ gọi CaptureOrderAsync() ⭢ PayPal capture
//   Capture thành công ⭢ cập nhật tbDonHang.trangthai = "Hoàn thành"
//
// FILES LIÊN QUAN:
//   CALLED BY:  PaymentController.cs (PayPalCreate, PayPalReturn)
//   CALLS:      PayPal REST API (https://api-m.sandbox.paypal.com)
//   LIÊN QUAN:  tbDonHang.cs (order state), tbThongTinDatHang.cs (user info)
//   LIÊN QUAN:  CartController.cs (khởi tạo luồng thanh toán)
// ============================================================
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShipFood.Services;

public class PayPalService
{
    private readonly ILogger<PayPalService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;

    // ponytail: t? gi� VND->USD, PayPal kh�ng h? tr? VND tr?c ti?p
    private const decimal VndToUsdRate = 25000m;

    public PayPalService(IConfiguration configuration, ILogger<PayPalService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;

        _clientId = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID")
            ?? configuration["PayPal:ClientId"]
            ?? "";
        _clientSecret = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_SECRET")
            ?? configuration["PayPal:ClientSecret"]
            ?? "";
        var mode = Environment.GetEnvironmentVariable("PAYPAL_MODE")
            ?? configuration["PayPal:Mode"]
            ?? "Sandbox";

        _baseUrl = mode.Equals("Live", StringComparison.OrdinalIgnoreCase)
            ? "https://api-m.paypal.com"
            : "https://api-m.sandbox.paypal.com";

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
        {
            _logger.LogWarning("PayPal credentials not configured. Set PAYPAL_CLIENT_ID and PAYPAL_CLIENT_SECRET env vars.");
        }
    }

    /// <summary>
    /// L?y Access Token t? PayPal (OAuth2 client_credentials)
    /// </summary>
    private async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });
            var response = await _httpClient.PostAsync($"{_baseUrl}/v1/oauth2/token", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal GetAccessToken failed: {Status} {Body}", response.StatusCode, body);
                return null;
            }

            var json = JsonSerializer.Deserialize<PayPalTokenResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return json?.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal GetAccessToken error");
            return null;
        }
    }

    /// <summary>
    /// T?o Order tr�n PayPal (intent=CAPTURE)
    /// Tr? v? approve link cho frontend chuy?n h??ng
    /// </summary>
    public async Task<PayPalCreateOrderResult> CreateOrderAsync(string orderId, decimal amountVnd, string returnUrl, string cancelUrl)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (token == null)
                return new PayPalCreateOrderResult { Success = false, Message = "Kh�ng th? x�c th?c PayPal" };

            var usdAmount = Math.Round(amountVnd / VndToUsdRate, 2);
            if (usdAmount < 0.5m) usdAmount = 0.5m; // PayPal minimum

            var payload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = orderId,
                        description = $"FastShip Order #{orderId}",
                        amount = new
                        {
                            currency_code = "USD",
                            value = usdAmount.ToString("F2")
                        }
                    }
                },
                payment_source = new
                {
                    paypal = new
                    {
                        experience_context = new
                        {
                            payment_method_preference = "IMMEDIATE_PAYMENT_REQUIRED",
                            landing_page = "LOGIN",
                            user_action = "PAY_NOW",
                            return_url = returnUrl,
                            cancel_url = cancelUrl
                        }
                    }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"{_baseUrl}/v2/checkout/orders", jsonContent);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal CreateOrder failed: {Status} {Body}", response.StatusCode, body);
                return new PayPalCreateOrderResult { Success = false, Message = $"PayPal error: {response.StatusCode}" };
            }

            var order = JsonSerializer.Deserialize<PayPalOrderResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (order?.Links == null)
            {
                _logger.LogError("PayPal CreateOrder: no links in response: {Body}", body);
                return new PayPalCreateOrderResult { Success = false, Message = "PayPal response invalid" };
            }

            var approveLink = order.Links.FirstOrDefault(l => l.Rel == "payer-action")?.Href;
            if (approveLink == null)
                approveLink = order.Links.FirstOrDefault(l => l.Rel == "approve")?.Href;

            return new PayPalCreateOrderResult
            {
                Success = true,
                PayPalOrderId = order.Id,
                ApproveLink = approveLink ?? "",
                Message = "PayPal order created"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal CreateOrder error for OrderId={OrderId}", orderId);
            return new PayPalCreateOrderResult { Success = false, Message = $"L?i: {ex.Message}" };
        }
    }

    /// <summary>
    /// Capture (thu ti?n) t? PayPal Order sau khi kh�ch duy?t
    /// </summary>
    public async Task<PayPalCaptureResult> CaptureOrderAsync(string paypalOrderId)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (token == null)
                return new PayPalCaptureResult { Success = false, Message = "Kh�ng th? x�c th?c PayPal" };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"{_baseUrl}/v2/checkout/orders/{paypalOrderId}/capture", null);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal CaptureOrder failed: {Status} {Body}", response.StatusCode, body);
                return new PayPalCaptureResult { Success = false, Message = $"PayPal capture error: {response.StatusCode}" };
            }

            var capture = JsonSerializer.Deserialize<PayPalCaptureResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (capture?.Status == "COMPLETED")
            {
                var captureId = capture.PurchaseUnits?.FirstOrDefault()
                    ?.Payments?.Captures?.FirstOrDefault()
                    ?.Id ?? "";

                return new PayPalCaptureResult
                {
                    Success = true,
                    CaptureId = captureId,
                    Status = capture.Status,
                    Message = "Payment captured successfully"
                };
            }

            return new PayPalCaptureResult
            {
                Success = false,
                Status = capture?.Status ?? "UNKNOWN",
                Message = $"PayPal status: {capture?.Status}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayPal CaptureOrder error for PayPalOrderId={PayPalOrderId}", paypalOrderId);
            return new PayPalCaptureResult { Success = false, Message = $"L?i: {ex.Message}" };
        }
    }
}

// ─── DTOs ───

public class PayPalTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class PayPalOrderResponse
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public List<PayPalLink>? Links { get; set; }
}

public class PayPalLink
{
    public string? Href { get; set; }
    public string? Rel { get; set; }
    public string? Method { get; set; }
}

public class PayPalCaptureResponse
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public List<PayPalPurchaseUnit>? PurchaseUnits { get; set; }
}

public class PayPalPurchaseUnit
{
    public PayPalPayments? Payments { get; set; }
}

public class PayPalPayments
{
    public List<PayPalCaptureDetail>? Captures { get; set; }
}

public class PayPalCaptureDetail
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public PayPalAmount? Amount { get; set; }
}

public class PayPalAmount
{
    public string? CurrencyCode { get; set; }
    public string? Value { get; set; }
}

public class PayPalCreateOrderResult
{
    public bool Success { get; set; }
    public string? PayPalOrderId { get; set; }
    public string? ApproveLink { get; set; }
    public string? Message { get; set; }
}

public class PayPalCaptureResult
{
    public bool Success { get; set; }
    public string? CaptureId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
}
