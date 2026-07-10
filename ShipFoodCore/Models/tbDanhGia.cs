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
    /// Phản hồi của quán ăn dành cho đánh giá của khách hàng
    /// </summary>
    [MaxLength(500)]
    public string? phanHoiCuaQuan { get; set; }

    // Navigation
    [ForeignKey("mactdh")]
    public virtual tbChiTietDonHang? tbChiTietDonHang { get; set; }
}
