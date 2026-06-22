using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbTinNhan")]
public partial class tbTinNhan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int matn { get; set; }

    public int? madh { get; set; }

    [MaxLength(500)]
    public string? noidung { get; set; }

    public int? mashipper { get; set; }
    public int? makh { get; set; }

    // Navigation
    [ForeignKey("madh")]
    public virtual tbDonHang? tbDonHang { get; set; }

    [ForeignKey("makh")]
    public virtual tbKhachHang? tbKhachHang { get; set; }

    [ForeignKey("mashipper")]
    public virtual tbShipper? tbShipper { get; set; }
}
