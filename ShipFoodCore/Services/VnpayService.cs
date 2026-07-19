using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ShipFood.Services;

/// <summary>
/// VNPAY Payment Integration Service (Sandbox)
/// Tạo payment URL với HMAC-SHA512, xác thực IPN callback
/// </summary>
public class VnpayService
{
    private readonly ILogger<VnpayService> _logger;

    // VNPAY Sandbox URL
    private readonly string _baseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    // Đọc từ Environment Variables (cấu hình trên Render)
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _vnpayUrl;

    public VnpayService(IConfiguration configuration, ILogger<VnpayService> logger)
    {
        _logger = logger;

        // Ưu tiên Environment Variables → appsettings.json
        _tmnCode = Environment.GetEnvironmentVariable("VNPAY_TMN_CODE")
            ?? configuration["VNPAY:TMN_CODE"]
            ?? "";

        _hashSecret = Environment.GetEnvironmentVariable("VNPAY_HASH_SECRET")
            ?? configuration["VNPAY:HASH_SECRET"]
            ?? "";

        _vnpayUrl = Environment.GetEnvironmentVariable("VNPAY_URL")
            ?? configuration["VNPAY:URL"]
            ?? _baseUrl;

        if (string.IsNullOrEmpty(_tmnCode) || string.IsNullOrEmpty(_hashSecret))
        {
            _logger.LogWarning("VNPAY credentials not configured. Set VNPAY_TMN_CODE and VNPAY_HASH_SECRET env vars.");
        }
        else
        {
            _logger.LogInformation("VNPAYService initialized (TMN_CODE length: {CodeLen}, URL: {Url})",
                _tmnCode.Length, _vnpayUrl);
        }
    }

    /// <summary>
    /// Tạo chữ ký HMAC-SHA512 từ dữ liệu đầu vào
    /// </summary>
    public string ComputeHmacSha512(string data)
    {
        if (string.IsNullOrEmpty(_hashSecret))
        {
            _logger.LogError("VNPAY_HASH_SECRET is not configured");
            return "";
        }

        var keyBytes = Encoding.UTF8.GetBytes(_hashSecret);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
    }

    /// <summary>
    /// Tạo URL thanh toán VNPAY
    /// </summary>
    /// <param name="orderId">Mã đơn hàng</param>
    /// <param name="amount">Số tiền (VND)</param>
    /// <param name="orderInfo">Thông tin đơn hàng</param>
    /// <param name="ipAddress">IP của khách hàng</param>
    /// <param name="returnUrl">URL redirect sau khi thanh toán</param>
    /// <returns>URL thanh toán VNPAY (sandbox)</returns>
    public string CreatePaymentUrl(int orderId, long amount, string orderInfo, string ipAddress, string returnUrl)
    {
        if (string.IsNullOrEmpty(_tmnCode))
        {
            _logger.LogError("VNPAY_TMN_CODE is not configured");
            return "";
        }

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "vnp_Amount", (amount * 100).ToString() }, // VNPAY nhân 100
            { "vnp_Command", "pay" },
            { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", ipAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", orderInfo },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", returnUrl },
            { "vnp_TmnCode", _tmnCode },
            { "vnp_TxnRef", orderId.ToString() },
            { "vnp_Version", "2.1.0" }
        };

        // Xây dựng chuỗi query params (sắp xếp theo alphabet — SortedDictionary đã làm việc này)
        var queryString = string.Join("&", vnpParams.Select(kvp =>
            $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));

        // Tạo chữ ký
        var rawData = string.Join("&", vnpParams.Select(kvp =>
            $"{kvp.Key}={kvp.Value}"));
        var secureHash = ComputeHmacSha512(rawData);

        // URL hoàn chỉnh
        var paymentUrl = $"{_vnpayUrl}?{queryString}&vnp_SecureHash={secureHash}";

        _logger.LogInformation(
            "VNPAY payment URL created: OrderId={OrderId}, Amount={Amount}, TxnRef={TxnRef}",
            orderId, amount, orderId);

        return paymentUrl;
    }

    /// <summary>
    /// Xác thực chữ ký từ VNPAY callback (IPN hoặc Return URL)
    /// </summary>
    /// <param name="vnpParams">Tất cả tham số VNPAY gửi về (bao gồm cả vnp_SecureHash)</param>
    /// <returns>true nếu chữ ký hợp lệ</returns>
    public bool VerifySignature(IDictionary<string, string> vnpParams)
    {
        if (!vnpParams.TryGetValue("vnp_SecureHash", out var receivedHash))
        {
            _logger.LogWarning("VNPAY callback missing vnp_SecureHash");
            return false;
        }

        // ponytail: vnp_SecureHashType không cần thiết cho xác thực
        vnpParams.Remove("vnp_SecureHash");
        vnpParams.Remove("vnp_SecureHashType");

        // Sắp xếp params theo alphabet
        var sortedParams = new SortedDictionary<string, string>(vnpParams, StringComparer.Ordinal);
        var rawData = string.Join("&", sortedParams.Select(kvp =>
            $"{kvp.Key}={kvp.Value}"));

        var computedHash = ComputeHmacSha512(rawData);

        var isValid = string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            _logger.LogWarning(
                "VNPAY signature verification FAILED. Computed: {Computed}, Received: {Received}",
                computedHash, receivedHash);
        }

        return isValid;
    }
}
