// ============================================================
// ⭐ tbDanhGia — Model Đánh giá (Review)
// ============================================================
// Ý nghĩa: Khách hàng đánh giá món ăn sau khi nhận hàng
// Chức năng: Điểm số (1-5), nhận xét, hình ảnh, phản hồi của quán
// KEYWORDS: review, đánh giá, rating, feedback, comment, phản hồi
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbDanhGia")]
public partial class tbDanhGia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int madg { get; set; }

    public int? mactdh { get; set; }
    public int? diemdanhgia { get; set; }

    [MaxLength(500)]
    public string? nhanxet { get; set; }

    [MaxLength(500)]
    public string? hinhanh { get; set; }

    /// <summary>
    /// Thời gian đánh giá — ⚠️ FIX: bổ sung trường ngày tháng còn thiếu
    /// </summary>
    public DateTime? ngaydanhgia { get; set; }

    /// <summary>
    /// Phản hồi của quán ăn dành cho đánh giá của khách hàng
    /// </summary>
    [MaxLength(500)]
    public string? phanHoiCuaQuan { get; set; }

    // Navigation
    [ForeignKey("mactdh")]
    public virtual tbChiTietDonHang? tbChiTietDonHang { get; set; }
}
