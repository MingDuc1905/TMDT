// ============================================================
// 🏃 DonHangDangLam — Đơn hàng đang làm (DTO đọc nhanh cho shipper)
// ============================================================
// Ý nghĩa: DTO [Keyless] (không phải bảng DB) — kết quả query tổng hợp
//         các đơn đang thực hiện để shipper nhận việc
// Chức năng: Chứa thông tin đơn + quán + người nhận cho danh sách shipper
// KEYWORDS: don dang lam, shipper, nhan don, order, keyless, DTO
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: ShipperController (action Index — query từ tbDonHang +
//     tbThongTinDatHang + tbQuanAn), Views/Shipper/Index.cshtml
//   → [Keyless]: không cần khóa chính vì là view-model, không map bảng
// ============================================================
using Microsoft.EntityFrameworkCore;

namespace ShipFood.Models;

[Keyless]
public class DonHangDangLam
{
    public int madh { get; set; }
    public DateTime? ngaydathang { get; set; }
    public string? tennguoinhan { get; set; }
    public string? diachi { get; set; }
    public string? tenquanan { get; set; }
    public string? DiaChiQuan { get; set; }
    public decimal? phiship { get; set; }
    public decimal? tongtien { get; set; }
    public string? trangthai { get; set; }
    public string? sdt { get; set; }
    public int? userid { get; set; }
}
