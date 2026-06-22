using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace ShipFood.Models;

[Table("tbThongTinDatHang")]
public partial class tbThongTinDatHang
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int mattdh { get; set; }

    [Required]
    [MaxLength(11)]
    public string sdt { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string diachi { get; set; } = null!;

    [Required]
    public Point toado { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string tennguoinhan { get; set; } = null!;

    public int? userid { get; set; }

    // Navigation
    [ForeignKey("userid")]
    public virtual tbKhachHang? tbKhachHang { get; set; }

    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
}
