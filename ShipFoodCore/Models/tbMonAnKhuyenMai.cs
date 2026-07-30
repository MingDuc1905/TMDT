using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

// ============================================================
// 📎 tbMonAnKhuyenMai — Bảng trung gian: Món ăn ↔ Khuyến mãi
// ============================================================
// Ý nghĩa: 1 KM có thể áp dụng cho NHIỀU món, 1 món có thể có NHIỀU KM
// Chức năng: Lưu % giảm riêng cho từng món (có thể khác % của KM gốc)
//            Kiểm soát trạng thái KM còn hạn/dừng theo món
// KEYWORDS: product-discount, menu-offer, promotion-link
// ============================================================

[Table("tbMonAnKhuyenMai")]
public partial class tbMonAnKhuyenMai
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }          // PK tự tăng — không có ý nghĩa business

    public int? makm { get; set; }       // FK → tbKhuyenMai.makm: mã giảm giá cha

    /// <summary>
    /// FK tới tbBienTheMonAn.id (biến thể size của món ăn)
    /// </summary>
    public int? mamon { get; set; }      // FK → tbBienTheMonAn.id: món được giảm giá

    public int? soluong { get; set; }    // Số lượng món được áp dụng KM (null = không giới hạn)

    [MaxLength(50)]
    public string? trangthai { get; set; } // "Còn hạn" / "Hết hạn" — điều kiện lọc khi hiển thị

    public int phantramgiam { get; set; }  // % giảm RIÊNG cho món này (0-100)
                                          // Có thể khác với phantramgiam của tbKhuyenMai gốc

    // Navigation — EF Core load tự động khi Include
    [ForeignKey("makm")]
    public virtual tbKhuyenMai? tbKhuyenMai { get; set; }
    // ⬆ Load thông tin KM gốc: tenkm, ngaybatdau, ngayketthuc, dieukien

    [ForeignKey("mamon")]
    public virtual tbBienTheMonAn? tbBienTheMonAn { get; set; }
    // ⬆ Load thông tin biến thể size món: giatien, size (M/L), mamon (FK→tbMonAn)

    // ─── Backward-compatible ───
    // ponytail: backward-compat — cần Include(tbBienTheMonAn).ThenInclude(tbMonAn) trước
    [NotMapped]                          // Chỉ ở RAM, ko có cột trong DB
    public tbMonAn? tbMonAn => tbBienTheMonAn?.tbMonAn;
    // ⬆ Tiện lợi: truy cập nhanh món ăn cha qua chuỗi: KM → BienThe → MonAn
}
