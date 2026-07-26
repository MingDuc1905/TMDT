// ============================================================
// 📐 tbBienTheMonAn — Model Biến thể món ăn (Size + Price Variant)
// ============================================================
// Ý nghĩa: Mỗi món có thể có nhiều biến thể size (M/L) với giá khác nhau
// Chức năng: FK→tbMonAn.mamon, FK cho tbChiTietDonHang.mamon, tbMonAnKhuyenMai.mamon
// KEYWORDS: variant, size, price, biến thể, product option, menu size
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbBienTheMonAn")]
public partial class tbBienTheMonAn
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public int mamon { get; set; }

    [MaxLength(10)]
    public string? size { get; set; }

    [Column(TypeName = "money")]
    public decimal? giatien { get; set; }

    // Navigation
    [ForeignKey("mamon")]
    public virtual tbMonAn? tbMonAn { get; set; }

    public virtual ICollection<tbChiTietDonHang> tbChiTietDonHangs { get; set; } = new HashSet<tbChiTietDonHang>();
    public virtual ICollection<tbMonAnKhuyenMai> tbMonAnKhuyenMais { get; set; } = new HashSet<tbMonAnKhuyenMai>();

    // Singular alias
    [NotMapped]
    public ICollection<tbChiTietDonHang> tbChiTietDonHang => tbChiTietDonHangs;
}
