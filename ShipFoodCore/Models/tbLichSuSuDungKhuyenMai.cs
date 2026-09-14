// ============================================================
// 📋 tbLichSuSuDungKhuyenMai — Lịch sử dùng mã giảm giá của user
// ============================================================
// Ý nghĩa: Ghi lại mỗi lần user áp dụng mã KM (chống dùng lặp)
// Chức năng: CartController CheckCoupon đếm số lần → chặn dùng lại
// KEYWORDS: lich su khuyen mai, coupon history, su dung ma, chong lap
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: tbUser (FK userid), tbKhuyenMai (FK makm), tbDonHang (FK madh)
//   → LOGIC: 1 user chỉ được dùng 1 mã 1 lần — check count > 0 → từ chối
//   → GHI: PaymentController lưu ngaydung = UtcNow khi đặt đơn có mã
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

/// <summary>
/// Lưu vết lịch sử sử dụng mã giảm giá của từng User
/// Giúp kiểm tra tần suất sử dụng mã trước khi cho phép áp dụng
/// </summary>
[Table("tbLichSuSuDungKhuyenMai")]
public partial class tbLichSuSuDungKhuyenMai
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public int userid { get; set; }

    public int makm { get; set; }

    public DateTime ngaydung { get; set; } = DateTime.Now;

    public int? madh { get; set; }

    // Navigation
    [ForeignKey("userid")]
    public virtual tbUser tbUser { get; set; } = null!;

    [ForeignKey("makm")]
    public virtual tbKhuyenMai tbKhuyenMai { get; set; } = null!;

    [ForeignKey("madh")]
    public virtual tbDonHang? tbDonHang { get; set; }
}
