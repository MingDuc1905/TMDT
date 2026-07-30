// ============================================================
// 📐 tbBienTheMonAn — Model Biến thể món ăn (Size + Price Variant)
// ============================================================
// Ý nghĩa: Mỗi món có thể có nhiều biến thể size (M/L) với giá khác nhau
// Chức năng: FK→tbMonAn.mamon, FK cho tbChiTietDonHang.mamon, tbMonAnKhuyenMai.mamon
// KEYWORDS: variant, size, price, biến thể, product option, menu size
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbBienTheMonAn")]
public partial class tbBienTheMonAn
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }           // PK tự tăng — định danh biến thể size

    public int mamon { get; set; }       // FK → tbMonAn.mamon — món cha của biến thể này

    [MaxLength(10)]
    public string? size { get; set; }     // Tên size: "M", "L", "XL" (null = mặc định)

    [Column(TypeName = "money")]
    public decimal? giatien { get; set; } // GIÁ TIỀN thực tế của biến thể size này (VD: 50.000đ)
                                          // ⭐ Đây là GIÁ GỐC để tính giảm giá!

    // Navigation — EF Core load tự động
    [ForeignKey("mamon")]
    public virtual tbMonAn? tbMonAn { get; set; }
    // ⬆ Load món cha: tenmon, hinhanh, conhang, isDeleted

    public virtual ICollection<tbChiTietDonHang> tbChiTietDonHangs { get; set; } = new HashSet<tbChiTietDonHang>();
    // ⬆ Các chi tiết đơn hàng thuộc biến thể này (khi user đặt)

    public virtual ICollection<tbMonAnKhuyenMai> tbMonAnKhuyenMais { get; set; } = new HashSet<tbMonAnKhuyenMai>();
    // ⬆ Các KM áp dụng cho biến thể này (qua bảng trung gian)

    // ponytail: backward-compat alias — dùng tbChiTietDonHangs (số nhiều)
    [NotMapped]                           // Chỉ ở RAM, ko có cột trong DB
    public ICollection<tbChiTietDonHang> tbChiTietDonHang => tbChiTietDonHangs;
    // ⬆ Alias cho tương thích code cũ (số ít thay vì số nhiều)
}
