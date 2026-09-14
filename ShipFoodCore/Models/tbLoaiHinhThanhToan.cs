// ============================================================
// 💳 tbLoaiHinhThanhToan — Danh mục phương thức thanh toán
// ============================================================
// Ý nghĩa: Khai báo các hình thức thanh toán (COD, VNPAY, MoMo...)
// Chức năng: tbDonHang.mahttt FK → đây; Checkout lọc VNPAY + tiền mặt
// KEYWORDS: thanh toan, payment, phuong thuc, COD, VNPAY, MoMo
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: tbDonHang (FK mahttt), CartController.Checkout
//     (chỉ hiện "VNPAY" + "tiền mặt/COD"), PaymentController
//   → NOTE: Seed DB phải có đủ các phương thức cho Checkout hiển thị
// ============================================================
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
