// ============================================================
// 🏦 BankHelper — Xử lý thông tin ngân hàng cho VietQR API
// ============================================================
// Ý nghĩa: Map tên ngân hàng → BIN code cho API VietQR
// Chức năng: GetVietQrBinCode, BankBinMap (35+ ngân hàng Việt Nam)
// KEYWORDS: bank, ngân hàng, bin code, vietqr, qr code, thanh toán, chuyển khoản
// ============================================================
namespace ShipFood.Helpers;

public static class BankHelper
{
    // Map từ tên ngân hàng (BANK_ID env var) → BIN code cho VietQR API
    private static readonly Dictionary<string, string> BankBinMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Big 4
        ["VietinBank"] = "970415",
        ["ICB"] = "970415",
        ["Vietcombank"] = "970436",
        ["VCB"] = "970436",
        ["BIDV"] = "970418",
        ["Agribank"] = "970405",
        ["VBA"] = "970405",

        // Private banks
        ["Techcombank"] = "970407",
        ["TCB"] = "970407",
        ["VPBank"] = "970432",
        ["VPB"] = "970432",
        ["MB Bank"] = "970422",
        ["MBB"] = "970422",
        ["ACB"] = "970416",
        ["Sacombank"] = "970403",
        ["STB"] = "970403",
        ["HDBank"] = "970437",
        ["HDB"] = "970437",
        ["TPBank"] = "970423",
        ["TPB"] = "970423",
        ["VIB"] = "970441",
        ["MSB"] = "970426",
        ["SHB"] = "970408",
        ["OCB"] = "970448",
        ["Nam A Bank"] = "970428",
        ["NAB"] = "970428",
        ["PVcomBank"] = "970412",
        ["SeABank"] = "970440",
        ["SSB"] = "970440",
        ["Eximbank"] = "970431",
        ["EIB"] = "970431",
        ["LienVietPostBank"] = "970449",
        ["LPB"] = "970449",
        ["Saigonbank"] = "970444",
        ["BaoViet Bank"] = "970438",
        ["BVB"] = "970438",
        ["VietBank"] = "970433",
        ["VBB"] = "970433",
        ["KienLongBank"] = "970452",
        ["KLB"] = "970452",
        ["SCB"] = "970429",
        ["DongA Bank"] = "970406",
        ["DAB"] = "970406",
        ["OceanBank"] = "970414",
        ["CB"] = "970414",
        ["PG Bank"] = "970410",
        ["GPBank"] = "970411",
        ["VRB"] = "970421",
        ["ABBANK"] = "970425",
        ["ABB"] = "970425",
    };

    /// <summary>
    /// Lấy BIN code cho VietQR API từ tên ngân hàng (hoặc BIN/short code đã có)
    /// Nếu đầu vào đã là BIN (6 số) hoặc short code có trong map, trả về trực tiếp
    /// </summary>
    public static string GetVietQrBinCode(string? bankId)
    {
        if (string.IsNullOrEmpty(bankId))
            return "970436"; // default Vietcombank

        // Nếu đã là BIN code (6 chữ số), dùng luôn
        if (bankId.Length == 6 && bankId.All(char.IsDigit))
            return bankId;

        // Tra map theo tên/short code
        if (BankBinMap.TryGetValue(bankId.Trim(), out var bin))
            return bin;

        // Fallback: trả về nguyên bản (có thể là short code hợp lệ chưa có trong map)
        return bankId;
    }
}
