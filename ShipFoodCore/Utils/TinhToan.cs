// ============================================================
// 🧮 TinhToan — Helper tính toán cho toàn bộ hệ thống
// ============================================================
// Ý nghĩa: Các hàm tiện ích tính phí ship, tổng tiền, URL hình ảnh, parse tọa độ
// Chức năng: TinhTienShip, TinhTongTien, HinhAnhUrl, AvatarUrl, TryParseToado
// KEYWORDS: helper, tính toán, phí ship, hình ảnh, url, tọa độ, coordinate, parse
// ============================================================
using ShipFood.Models;

namespace ShipFood.Utils;

public class TinhToan
{
    /// <summary>
    /// Tọa độ mặc định trung tâm TP.HCM (nếu không parse được)
    /// </summary>
    public const double DEFAULT_LAT = 10.8231;
    public const double DEFAULT_LNG = 106.6297;

    public static decimal? TinhTienShip(decimal? khoangCach)
    {
        return khoangCach * 15000;
    }

    public static decimal? TinhTongTien(tbDonHang donHang)
    {
        decimal? sum = 0;
        foreach (var i in donHang.tbChiTietDonHang)
        {
            sum += i.dongia * i.soluong;
        }
        return sum;
    }

    /// <summary>
    /// Parse tọa độ từ chuỗi VARCHAR(100) dạng "lat,lng"
    /// Nếu chuỗi rỗng, NULL hoặc sai định dạng → trả về tọa độ mặc định (trung tâm TP.HCM)
    /// Không crash dù dữ liệu đầu vào có vấn đề
    /// </summary>
    /// <summary>
    /// Trả về URL hình ảnh hoàn chỉnh cho món ăn.
    /// Nếu hinhanh là full URL (http:// hoặc https://) → dùng trực tiếp.
    /// Nếu không → prepend đường dẫn local ~/Source/images/MonAn/
    /// Nếu rỗng/null → trả về placeholder pizza.jpg
    /// </summary>
    public static string HinhAnhUrl(string? hinhanh)
    {
        if (string.IsNullOrWhiteSpace(hinhanh))
            return "/Source/Home/img/pizza.jpg";
        if (hinhanh.StartsWith("http://") || hinhanh.StartsWith("https://"))
            return hinhanh;
        return "/Source/images/MonAn/" + hinhanh;
    }

    /// <summary>
    /// Trả về URL hình ảnh avatar cho quán ăn / shipper.
    /// Avatar lưu ở thư mục riêng: ~/Source/Restaurant/images/avatar/
    /// Nếu rỗng/null → trả về placeholder quán ăn
    /// </summary>
    public static string AvatarUrl(string? hinhanh)
    {
        if (string.IsNullOrWhiteSpace(hinhanh))
            return "/Source/Home/img/pizza.jpg";
        if (hinhanh.StartsWith("http://") || hinhanh.StartsWith("https://"))
            return hinhanh;
        return "/Source/Restaurant/images/avatar/" + hinhanh;
    }

    /// <summary>
    /// Chuyển giờ lưu trong DB (UTC — server Render chạy UTC) sang giờ Việt Nam (GMT+7)
    /// để hiển thị đúng cho người dùng.
    /// Việt Nam cố định UTC+7 quanh năm (không DST) nên cộng 7h là đủ và đơn giản nhất.
    /// Trả về null nếu input null.
    /// </summary>
    public static DateTime? GioVietNam(DateTime? utc)
    {
        return utc?.AddHours(7);
    }

    public static (double Lat, double Lng) TryParseToado(string? toado)
    {
        if (string.IsNullOrWhiteSpace(toado))
            return (DEFAULT_LAT, DEFAULT_LNG);

        try
        {
            var parts = toado.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var lng))
            {
                // Validate bounds (Việt Nam)
                if (lat >= 8.0 && lat <= 24.0 && lng >= 102.0 && lng <= 110.0)
                    return (lat, lng);
            }
        }
        catch (FormatException)
        {
            // Sai định dạng → dùng mặc định
        }
        catch (Exception)
        {
            // Các lỗi khác → vẫn không crash
        }

        return (DEFAULT_LAT, DEFAULT_LNG);
    }
}
