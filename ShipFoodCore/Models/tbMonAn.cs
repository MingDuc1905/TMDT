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

    [MaxLength(50)]
    public string? hinhanh { get; set; }

    public int? maquanan { get; set; }

    public int? madanhmuc { get; set; }

    /// <summary>
    /// Trạng thái còn hàng (Task 2c: AJAX Toggle 1-Click)
    /// true = còn hàng, false = hết hàng
    /// </summary>
    public bool conhang { get; set; } = true;

    // Non-mapped property for cart quantity
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

    // Singular alias
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

    /// <summary>
    /// Chi tiết đơn hàng qua biến thể (backward-compat).
    /// Truy cập qua navigation: món → biến thể → chi tiết đơn hàng.
    /// </summary>
    [NotMapped]
    public List<tbChiTietDonHang> tbChiTietDonHangs =>
        tbBienTheMonAns?.SelectMany(b => b.tbChiTietDonHangs ?? Enumerable.Empty<tbChiTietDonHang>()).ToList()
        ?? new List<tbChiTietDonHang>();

    [NotMapped]
    public List<tbChiTietDonHang> tbChiTietDonHang => tbChiTietDonHangs;
}
