// ============================================================
// 📦 tbDonHang — Model Đơn hàng (Order)
// ============================================================
// Ý nghĩa: Bảng chính lưu tất cả đơn hàng của hệ thống
// Chức năng: Lưu thông tin đơn: quán, shipper, khách, trạng thái, giá, ship fee
// KEYWORDS: order, đơn hàng, don hang, madh, order tracking, trang thai
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbDonHang")]
public partial class tbDonHang
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int madh { get; set; }

    public int? maquan { get; set; }
    public int? mattdh { get; set; }

    public DateTime? ngaydathang { get; set; }

    [MaxLength(50)]
    public string? trangthai { get; set; }

    [Column(TypeName = "money")]
    public decimal? tongtien { get; set; }

    public int? hinhthucthanhtoan { get; set; }

    [MaxLength(200)]
    public string? ghichu { get; set; }

    public int? makhuyenmai { get; set; }

    [Column(TypeName = "money")]
    public decimal? phiship { get; set; }

    [Column(TypeName = "money")]
    public decimal? phidichvu { get; set; }

    public DateTime? ngaygiaohang { get; set; }

    public DateTime? ngaythanhtoan { get; set; }

    public int? mashipper { get; set; }

    [MaxLength(100)]
    public string? momo_trans_id { get; set; }

    // Navigation
    [ForeignKey("maquan")]
    public virtual tbQuanAn? tbQuanAn { get; set; }

    [ForeignKey("mattdh")]
    public virtual tbThongTinDatHang? tbThongTinDatHang { get; set; }

    [ForeignKey("hinhthucthanhtoan")]
    public virtual tbLoaiHinhThanhToan? tbLoaiHinhThanhToan { get; set; }

    [ForeignKey("makhuyenmai")]
    public virtual tbKhuyenMai? tbKhuyenMai { get; set; }

    [ForeignKey("mashipper")]
    public virtual tbShipper? tbShipper { get; set; }

    public virtual ICollection<tbChiTietDonHang> tbChiTietDonHangs { get; set; } = new HashSet<tbChiTietDonHang>();
    public virtual ICollection<tbTinNhan> tbTinNhans { get; set; } = new HashSet<tbTinNhan>();

    // Singular aliases for backward compatibility
    [NotMapped]
    public ICollection<tbChiTietDonHang> tbChiTietDonHang => tbChiTietDonHangs;
}
