using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbMonAn")]
public partial class tbMonAn
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int mamon { get; set; }

    [Required]
    [MaxLength(100)]
    public string tenmon { get; set; } = null!;

    [MaxLength(500)]
    public string? mota { get; set; }

    [Column(TypeName = "money")]
    public decimal? giatien { get; set; }

    [MaxLength(50)]
    public string? hinhanh { get; set; }

    public int? maquanan { get; set; }

    public int? madanhmuc { get; set; }

    /// <summary>
    /// Trạng thái còn hàng (Task 2c: AJAX Toggle 1-Click)
    /// true = còn hàng, false = hết hàng
    /// </summary>
    public bool conhang { get; set; } = true;

    // Non-mapped property for cart quantity
    [NotMapped]
    public int soLuong { get; set; }

    // Navigation
    [ForeignKey("maquanan")]
    public virtual tbQuanAn? tbQuanAn { get; set; }

    [ForeignKey("madanhmuc")]
    public virtual tbDanhMuc? tbDanhMuc { get; set; }

    public virtual ICollection<tbChiTietDonHang> tbChiTietDonHangs { get; set; } = new HashSet<tbChiTietDonHang>();
    public virtual ICollection<tbMonAnKhuyenMai> tbMonAnKhuyenMais { get; set; } = new HashSet<tbMonAnKhuyenMai>();

    // Singular aliases for backward compatibility
    [NotMapped]
    public ICollection<tbChiTietDonHang> tbChiTietDonHang => tbChiTietDonHangs;
}
