using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbShipper")]
public partial class tbShipper
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int userid { get; set; }

    [Required]
    [MaxLength(50)]
    public string tenshipper { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string diachi { get; set; } = null!;

    [MaxLength(100)]
    public string? toado { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? diemdanhgia { get; set; }

    public int? soluotdanhgia { get; set; }

    [MaxLength(50)]
    public string? trangthai { get; set; }

    [MaxLength(100)]
    public string? hinhanh { get; set; }

    // Navigation
    public virtual tbUser tbUser { get; set; } = null!;
    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
    public virtual ICollection<tbTinNhan> tbTinNhans { get; set; } = new HashSet<tbTinNhan>();
}
