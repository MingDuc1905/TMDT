// ============================================================
// 📂 tbDanhMuc — Model Danh mục món ăn (Category)
// ============================================================
// Ý nghĩa: Phân loại món ăn theo danh mục (Món chính, Đồ uống, Tráng miệng...)
// Chức năng: Tên danh mục, mô tả, hình ảnh, icon
// KEYWORDS: category, danh mục, food category, phân loại, menu category
// ============================================================
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

    [MaxLength(50)]
    public string? icon { get; set; }

    // Navigation
    public virtual ICollection<tbMonAn> tbMonAns { get; set; } = new HashSet<tbMonAn>();
}
