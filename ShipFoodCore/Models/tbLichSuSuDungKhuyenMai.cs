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
