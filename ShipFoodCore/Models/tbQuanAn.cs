using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbQuanAn")]
public partial class tbQuanAn
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int userid { get; set; }

    [Required]
    [MaxLength(100)]
    public string tenquanan { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string diachi { get; set; } = null!;

    [MaxLength(100)]
    public string? toado { get; set; }

    public int? soluotdanhgia { get; set; }

    [Column(TypeName = "decimal(2,1)")]
    public decimal? diemdanhgia { get; set; }

    [MaxLength(50)]
    public string? trangthai { get; set; }

    [MaxLength(500)]
    public string? hinhanh { get; set; }

    // Navigation
    public virtual tbUser tbUser { get; set; } = null!;
    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
    public virtual ICollection<tbMonAn> tbMonAns { get; set; } = new HashSet<tbMonAn>();

    // Singular aliases for backward compatibility
    [NotMapped]
    public ICollection<tbDonHang> tbDonHang => tbDonHangs;
    [NotMapped]
    public ICollection<tbMonAn> tbMonAn => tbMonAns;
}
