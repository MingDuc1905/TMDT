using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbChiTietDonHang")]
public partial class tbChiTietDonHang
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int mactdh { get; set; }

    public int? madh { get; set; }

    /// <summary>
    /// FK tới tbBienTheMonAn.id (biến thể size của món ăn)
    /// </summary>
    public int? mamon { get; set; }

    public int? soluong { get; set; }

    [Column(TypeName = "money")]
    public decimal? dongia { get; set; }

    // Navigation
    [ForeignKey("madh")]
    public virtual tbDonHang? tbDonHang { get; set; }

    [ForeignKey("mamon")]
    public virtual tbBienTheMonAn? tbBienTheMonAn { get; set; }

    public virtual ICollection<tbDanhGia> tbDanhGias { get; set; } = new HashSet<tbDanhGia>();

    // Singular alias for backward compatibility
    [NotMapped]
    public ICollection<tbDanhGia> tbDanhGia => tbDanhGias;

    // ─── Backward-compatible: tbMonAn qua tbBienTheMonAn ───
    [NotMapped]
    public tbMonAn? tbMonAn => tbBienTheMonAn?.tbMonAn;
}
