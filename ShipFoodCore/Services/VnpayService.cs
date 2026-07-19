using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ShipFood.Services;

public class VnpayService
{
    private readonly ILogger<VnpayService> _logger;

    private readonly string _baseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _vnpayUrl;

    public VnpayService(IConfiguration configuration, ILogger<VnpayService> logger)
    {
        _logger = logger;

        _tmnCode = (Environment.GetEnvironmentVariable("VNPAY_TMN_CODE")
            ?? configuration["VNPAY:TMN_CODE"]
            ?? "").Trim();

        _hashSecret = (Environment.GetEnvironmentVariable("VNPAY_HASH_SECRET")
            ?? configuration["VNPAY:HASH_SECRET"]
            ?? "").Trim();

        _vnpayUrl = (Environment.GetEnvironmentVariable("VNPAY_API_URL")
            ?? Environment.GetEnvironmentVariable("VNPAY_URL")
            ?? configuration["VNPAY:URL"]
            ?? _baseUrl).Trim();

        if (string.IsNullOrEmpty(_tmnCode) || string.IsNullOrEmpty(_hashSecret))
        {
            _logger.LogWarning("VNPAY credentials not configured.");
        }
        else
        {
            _logger.LogInformation("VnpayService initialized (TMN_CODE: {CodeLen} chars, HashSecret: {HashLen} chars, URL: {Url})",
                _tmnCode.Length, _hashSecret.Length, _vnpayUrl);
        }
    }

    public string CreatePaymentUrl(int orderId, long amount, string orderInfo, string ipAddress, string returnUrl)
    {
        if (string.IsNullOrEmpty(_tmnCode))
        {
            _logger.LogError("VNPAY_TMN_CODE is not configured");
            return "";
        }

        var vnpParams = new SortedList<string, string>(new VnPayCompare())
        {
            { "vnp_Amount", (amount * 100).ToString() },
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

        var data = new StringBuilder();
        foreach (var kv in vnpParams)
        {
            if (!string.IsNullOrEmpty(kv.Value))
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
        }

        var queryString = data.ToString();
        var signData = queryString.Length > 0
            ? queryString.Remove(queryString.Length - 1, 1)
            : queryString;

        var secureHash = HmacSHA512(_hashSecret, signData);

        var paymentUrl = _vnpayUrl + "?" + queryString + "vnp_SecureHash=" + secureHash;

        _logger.LogInformation("VNPAY URL created: TxnRef={TxnRef}, Amount={Amount}, HashLen={HashLen}",
            orderId, amount, secureHash.Length);

        return paymentUrl;
    }

    public bool VerifySignature(IDictionary<string, string> vnpParams)
    {
        if (!vnpParams.TryGetValue("vnp_SecureHash", out var receivedHash))
        {
            _logger.LogWarning("VNPAY callback missing vnp_SecureHash");
            return false;
        }

        vnpParams.Remove("vnp_SecureHash");
        vnpParams.Remove("vnp_SecureHashType");

        var responseData = new SortedList<string, string>(new VnPayCompare());
        foreach (var kv in vnpParams)
        {
            if (!string.IsNullOrEmpty(kv.Value))
                responseData.Add(kv.Key, kv.Value);
        }

        var data = new StringBuilder();
        foreach (var kv in responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
        }

        if (data.Length > 0)
            data.Remove(data.Length - 1, 1);

        var rspRaw = data.ToString();
        var myChecksum = HmacSHA512(_hashSecret, rspRaw);

        var isValid = myChecksum.Equals(receivedHash, StringComparison.InvariantCultureIgnoreCase);

        if (!isValid)
        {
            _logger.LogWarning(
                "VNPAY signature FAILED. Computed({ComputedLen}): {Computed}, Received({ReceivedLen}): {Received}",
                myChecksum.Length, myChecksum, receivedHash?.Length ?? 0, receivedHash);
        }

        return isValid;
    }

    private static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        foreach (var theByte in hashValue)
            hash.Append(theByte.ToString("x2"));
        return hash.ToString();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
