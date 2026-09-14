// ============================================================
// 📜 LichSuDonHang — Lịch sử đơn hàng (DTO đọc nhanh)
// ============================================================
// Ý nghĩa: DTO (không phải bảng DB) — tóm tắt 1 đơn hàng cho danh sách lịch sử
// Chức năng: Chứa thông tin đơn + shipper phụ trách để hiển thị gọn
// KEYWORDS: lich su don, order history, shipper, DTO, don hang
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: ShipperController/HomeController query tạo danh sách,
//     Views/Cart/LichSuDatHang.cshtml hiển thị
//   → LƯU Ý: KHÔNG phải bảng DB — là view-model tổng hợp từ nhiều bảng
// ============================================================
namespace ShipFood.Models;

public class LichSuDonHang
{
    public int madh { get; set; }
    public DateTime? ngaydathang { get; set; }
    public string? trangthai { get; set; }
    public string? diachi { get; set; }
    public string? tennguoinhan { get; set; }
    public decimal? phiship { get; set; }
    public int? mashipper { get; set; }
}
