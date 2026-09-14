// ============================================================
// 🗂️ DataAnalyticDanhMuc — Phân tích theo danh mục (dashboard quán)
// ============================================================
// Ý nghĩa: DTO (không phải bảng DB) chứa thống kê tổng hợp theo từng danh mục
// Chức năng: Phục vụ trang Phân tích của quán — số món, số lượng bán, doanh thu
// KEYWORDS: danh muc, category, phan tich, doanh thu, thong ke, analytic
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: RestaurantController (action Analytics — GROUP BY
//     tbDanhMuc + tbMonAn + tbChiTietDonHang)
//   → VIEW: Views/Restaurant/Analytics.cshtml (biểu đồ theo danh mục)
// ============================================================
namespace ShipFood.Models;

public class DataAnalyticDanhMuc
{
    public int? maDanhMuc { get; set; }
    public string? tenDanhMuc { get; set; }
    public int? soLuongMonAn { get; set; }
    public int? tongSoLuongBanRa { get; set; }
    public string? hinhAnh { get; set; }
    public double? doanhThu { get; set; }
}
