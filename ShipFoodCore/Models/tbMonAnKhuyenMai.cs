using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbMonAnKhuyenMai")]
public partial class tbMonAnKhuyenMai
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public int? makm { get; set; }

    /// <summary>
    /// FK tới tbBienTheMonAn.id (biến thể size của món ăn)
    /// </summary>
    public int? mamon { get; set; }

    public int? soluong { get; set; }

    [MaxLength(50)]
    public string? trangthai { get; set; }

    public int phantramgiam { get; set; }

    // Navigation
    [ForeignKey("makm")]
    public virtual tbKhuyenMai? tbKhuyenMai { get; set; }

    [ForeignKey("mamon")]
    public virtual tbBienTheMonAn? tbBienTheMonAn { get; set; }

    // ─── Backward-compatible ───
    [NotMapped]
    public tbMonAn? tbMonAn => tbBienTheMonAn?.tbMonAn;
}
