using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbUser")]
public partial class tbUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int userid { get; set; }

    [Required]
    [MaxLength(50)]
    public string username { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string pwd { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string loaitaikhoan { get; set; } = null!;

    [Required]
    [MaxLength(11)]
    public string sdt { get; set; } = null!;

    [Column(TypeName = "money")]
    public decimal? vitien { get; set; }

    [Required]
    [MaxLength(50)]
    public string email { get; set; } = null!;

    public int trangthai { get; set; }

    // Navigation properties
    public virtual tbAdmin? tbAdmin { get; set; }
    public virtual tbKhachHang? tbKhachHang { get; set; }
    public virtual tbQuanAn? tbQuanAn { get; set; }
    public virtual tbShipper? tbShipper { get; set; }
}
