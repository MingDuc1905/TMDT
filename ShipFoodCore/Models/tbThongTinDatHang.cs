// ============================================================
// 📍 tbThongTinDatHang — Thông tin giao hàng của đơn
// ============================================================
// Ý nghĩa: Địa chỉ + SĐT + tọa độ + người nhận cho 1 đơn hàng
// Chức năng: Được Checkout lưu/dùng lại (dedupe theo sdt+diachi+tennguoinhan),
//         map.js dùng toado để vẽ điểm giao hàng
// KEYWORDS: thong tin dat hang, dia chi giao, shipping address, toa do, sdt
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   → FILE: tbDonHang (FK mattdh), tbKhachHang (FK userid),
//     CartController.Checkout (dedupe GROUP BY), map.js (tọa độ),
//     Views Checkout/OrderTracking (hiển thị địa chỉ)
//   → toado: chuỗi "lat,lng" — parse bằng TinhToan.TryParseToado
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [MaxLength(100)]
    public string? toado { get; set; }

    [Required]
    [MaxLength(50)]
    public string tennguoinhan { get; set; } = null!;

    public int? userid { get; set; }

    // Navigation
    [ForeignKey("userid")]
    public virtual tbKhachHang? tbKhachHang { get; set; }

    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
}
