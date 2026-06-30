using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbLoaiHinhThanhToan")]
public partial class tbLoaiHinhThanhToan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int mahttt { get; set; }

    [Required]
    [MaxLength(100)]
    public string tenhinhthuc { get; set; } = null!;

    [MaxLength(500)]
    public string? mota { get; set; }

    // Navigation
    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
}
