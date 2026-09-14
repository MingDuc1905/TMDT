// ============================================================
// 📊 DataAnalytic — Dữ liệu phân tích 1 món ăn (dashboard quán)
// ============================================================
// Ý nghĩa: DTO (không phải bảng DB) chứa thống kê tổng hợp 1 món ăn
// Chức năng: Phục vụ trang Phân tích của quán — bán chạy, đánh giá, tồn kho
// KEYWORDS: analytic, phan tich, thong ke, mon an, ban chay, doanh thu
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: RestaurantController (action Analytics/ProductList — query
//     JOIN tbMonAn + tbDanhGia + tbChiTietDonHang để tổng hợp số liệu)
//   → VIEW: Views/Restaurant/Analytics.cshtml hiển thị bảng xếp hạng món
// ============================================================
namespace ShipFood.Models;

public class DataAnalytic
{
    public int maMonAn { get; set; }
    public string? tenMonAn { get; set; }
    public string? hinhAnh { get; set; }
    public string? tenDanhMuc { get; set; }
    public int? diemDanhGia { get; set; }
    public int? soDanhGia { get; set; }
    public decimal? giaTien { get; set; }
    public int? soLuongBanDuoc { get; set; }
    public bool conhang { get; set; } = true;
}
