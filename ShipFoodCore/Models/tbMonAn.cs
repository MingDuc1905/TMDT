// ============================================================
// 🍕 tbMonAn — Model Món ăn (Food Item)
// ============================================================
// Ý nghĩa: Lưu thông tin món ăn: tên, mô tả, hình ảnh, danh mục, biến thể
// Chức năng: Soft-delete (isDeleted), còn hàng (conhang), biến thể size+giá
// KEYWORDS: food, món ăn, mon an, menu, product, bien the, category
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbMonAn")]
public partial class tbMonAn
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int mamon { get; set; }

    [Required]
    [MaxLength(100)]
    public string tenmon { get; set; } = null!;

    [MaxLength(500)]
    public string? mota { get; set; }

    [MaxLength(500)]
    public string? hinhanh { get; set; }

    public int? maquanan { get; set; }

    public int? madanhmuc { get; set; }

    /// <summary>
    /// Trạng thái còn hàng (Task 2c: AJAX Toggle 1-Click)
    /// true = còn hàng, false = hết hàng
    /// </summary>
    public bool conhang { get; set; } = true;

    /// <summary>
    /// Soft delete flag: 1 = đã xóa (bảo toàn lịch sử hóa đơn), 0 = đang hoạt động
    /// </summary>
    public bool isDeleted { get; set; }

    // ponytail: non-mapped, runtime-only cart quantity (không phải navigation alias)
    [NotMapped]
    public int soLuong { get; set; }

    // Navigation
    [ForeignKey("maquanan")]
    public virtual tbQuanAn? tbQuanAn { get; set; }

    [ForeignKey("madanhmuc")]
    public virtual tbDanhMuc? tbDanhMuc { get; set; }

    /// <summary>
    /// Danh sách biến thể (size + giá) của món ăn này.
    /// Mỗi món có ít nhất 1 biến thể mặc định (size = NULL).
    /// </summary>
    public virtual ICollection<tbBienTheMonAn> tbBienTheMonAns { get; set; } = new HashSet<tbBienTheMonAn>();

    // ponytail: backward-compat alias — code mới dùng tbBienTheMonAns (số nhiều)
    [NotMapped]
    public ICollection<tbBienTheMonAn> tbBienTheMonAn => tbBienTheMonAns;

    // ─── Backward-compatible (cho Views cũ) ───

    /// <summary>
    /// Giá tiền. Backing field cho phép setter (dùng khi tạo tbMonAn trong Cart).
    /// Nếu chưa được set, fallback về giá của biến thể đầu tiên.
    /// </summary>
    private decimal? _giatien;

    [NotMapped]
    public decimal? giatien
    {
        get => _giatien ?? tbBienTheMonAns?.FirstOrDefault()?.giatien;
        set => _giatien = value;
    }

    // ponytail: backward-compat alias — không có FK trực tiếp, cần Include(tbBienTheMonAns) trước
    [NotMapped]
    public List<tbChiTietDonHang> tbChiTietDonHangs =>
        tbBienTheMonAns?.SelectMany(b => b.tbChiTietDonHangs ?? Enumerable.Empty<tbChiTietDonHang>()).ToList()
        ?? new List<tbChiTietDonHang>();

    // ponytail: backward-compat alias — dùng tbChiTietDonHangs (số nhiều)
    [NotMapped]
    public List<tbChiTietDonHang> tbChiTietDonHang => tbChiTietDonHangs;
}
