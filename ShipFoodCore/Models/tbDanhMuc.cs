using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbDanhMuc")]
public partial class tbDanhMuc
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int madanhmuc { get; set; }

    [Required]
    [MaxLength(100)]
    public string tendanhmuc { get; set; } = null!;

    [MaxLength(250)]
    public string? mota { get; set; }

    [MaxLength(500)]
    public string? hinhanh { get; set; }

    // Navigation
    public virtual ICollection<tbMonAn> tbMonAns { get; set; } = new HashSet<tbMonAn>();
}
