// ============================================================
// 🏪 tbQuanAn — Model Quán ăn (Restaurant)
// ============================================================
// Ý nghĩa: Lưu thông tin quán ăn: tên, địa chỉ, tọa độ, đánh giá, trạng thái
// Chức năng: FK→tbUser.userid, navigation tới tbMonAn, tbDonHang
// KEYWORDS: restaurant, quán ăn, quan an, merchant, shop, food store
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbQuanAn")]
public partial class tbQuanAn
{
    // ════════════════════════════════════════════════════════════
    // 🏪 KHỐI THÔNG TIN QUÁN ĂN (Restaurant)
    // ════════════════════════════════════════════════════════════
    // KEYWORDS: quan an, tenquanan, diachi, diemdanhgia, trangthai
    // → userid = PK (1:1 với tbUser — role "Quán ăn")
    // → TẠO BỞI: HomeController.Signup (loaitaikhoan="Quán ăn"),
    //   AdminController.PostTaiKhoan
    // → TRANGTHAI: "Đang mở cửa"/"Đóng cửa" (OrderStatus)
    // → NAVIGATION: tbUser (tài khoản), tbMonAns (thực đơn),
    //   tbDonHangs (đơn hàng của quán)

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int userid { get; set; }

    [Required]
    [MaxLength(100)]
    public string tenquanan { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string diachi { get; set; } = null!;

    [MaxLength(100)]
    public string? toado { get; set; }

    public int? soluotdanhgia { get; set; }

    [Column(TypeName = "decimal(2,1)")]
    public decimal? diemdanhgia { get; set; }

    [MaxLength(50)]
    public string? trangthai { get; set; }

    [MaxLength(500)]
    public string? hinhanh { get; set; }

    // Navigation
    public virtual tbUser tbUser { get; set; } = null!;
    public virtual ICollection<tbDonHang> tbDonHangs { get; set; } = new HashSet<tbDonHang>();
    public virtual ICollection<tbMonAn> tbMonAns { get; set; } = new HashSet<tbMonAn>();

    // ponytail: backward-compat aliases — dùng tbDonHangs / tbMonAns (số nhiều)
    [NotMapped]
    public ICollection<tbDonHang> tbDonHang => tbDonHangs;
    [NotMapped]
    public ICollection<tbMonAn> tbMonAn => tbMonAns;
}
