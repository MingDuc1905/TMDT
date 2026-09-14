// ============================================================
// 🎫 tbKhuyenMai — Bảng mã khuyến mãi / giảm giá
// ============================================================
// Ý nghĩa: Khai báo các mã KM (%, điều kiện áp dụng, hiệu lực)
// Chức năng: CartController.CheckCoupon validate mã, VoucherService gợi ý
// KEYWORDS: khuyen mai, coupon, voucher, ma giam gia, discount, phan tram
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: CartController (CheckCoupon/Checkout/GetTopCoupons),
//     VoucherService (gợi ý theo giờ), tbLichSuSuDungKhuyenMai
//     (đếm lượt dùng — mỗi mã 1 lần/user), tbMonAnKhuyenMai (áp cho món)
//   → dieukien: chuỗi "Đơn từ 200.000đ" — parse số trong CartController
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbKhuyenMai")]
public partial class tbKhuyenMai
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int makm { get; set; }

    [Required]
    [MaxLength(100)]
    public string tenkm { get; set; } = null!;

    [MaxLength(500)]
    public string? mota { get; set; }

    [Required]
    [MaxLength(200)]
    public string loaikm { get; set; } = null!;

    public int? phantramgiam { get; set; }

    [MaxLength(500)]
    public string? dieukien { get; set; }

    public DateTime? ngaybatdau { get; set; }
    public DateTime? ngayketthuc { get; set; }

        // Navigation — Liên kết với các bảng khác
    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
    // ⬆ 1 đơn hàng áp dụng mã KM này (tbDonHang.makm → tbKhuyenMai.makm)
    public virtual ICollection<tbMonAnKhuyenMai> tbMonAnKhuyenMais { get; set; } = new HashSet<tbMonAnKhuyenMai>();
    // ⬆ 1 KM có thể áp dụng cho NHIỀU món ăn qua bảng trung gian tbMonAnKhuyenMai
}
