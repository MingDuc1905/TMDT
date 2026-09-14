// ============================================================
// 👤 tbKhachHang — Model Khách hàng (Customer)
// ============================================================
// Ý nghĩa: Thông tin chi tiết khách hàng: tên, hình ảnh, địa chỉ đặt
// Chức năng: FK→tbUser.userid, navigation tới tbThongTinDatHang, tbTinNhan
// KEYWORDS: customer, khách hàng, khach hang, client, user profile
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbKhachHang")]
public partial class tbKhachHang
{
    // ════════════════════════════════════════════════════════════
    // 👤 KHỐI THÔNG TIN KHÁCH HÀNG (Customer)
    // ════════════════════════════════════════════════════════════
    // KEYWORDS: khach hang, tenkh, hinhanh, customer
    // → userid = PK (1:1 với tbUser — role "Khách hàng")
    // → TẠO BỞI: HomeController.Signup (loaitaikhoan="Khách hàng"),
    //   Google/Facebook OAuth auto-create
    // → NAVIGATION: tbUser (tài khoản), tbThongTinDatHangs (địa chỉ đặt),
    //   tbTinNhans (chat với admin/shipper/quán)

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int userid { get; set; }

    [Required]
    [MaxLength(50)]
    public string tenkh { get; set; } = null!;

    [MaxLength(500)]
    public string? hinhanh { get; set; }

    // Navigation
    public virtual tbUser tbUser { get; set; } = null!;
    public virtual ICollection<tbThongTinDatHang> tbThongTinDatHangs { get; set; } = new HashSet<tbThongTinDatHang>();
    public virtual ICollection<tbTinNhan> tbTinNhans { get; set; } = new HashSet<tbTinNhan>();
}
