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

    // Navigation
    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
    public virtual ICollection<tbMonAnKhuyenMai> tbMonAnKhuyenMais { get; set; } = new HashSet<tbMonAnKhuyenMai>();
}
