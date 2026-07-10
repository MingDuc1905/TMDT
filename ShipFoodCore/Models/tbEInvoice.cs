using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

/// <summary>
/// Hóa đơn điện tử (E-Invoice) — tự động sinh khi đơn hàng được thanh toán thành công
/// </summary>
[Table("tbEInvoice")]
public partial class tbEInvoice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int einvoice_id { get; set; }

    /// <summary>Mã hóa đơn số (VD: INV-FS-20260710-0001)</summary>
    [Required]
    [MaxLength(50)]
    public string invoice_number { get; set; } = null!;

    /// <summary>FK tới đơn hàng</summary>
    public int madh { get; set; }

    /// <summary>Ngày xuất hóa đơn</summary>
    public DateTime ngayxuat { get; set; } = DateTime.Now;

    /// <summary>Tổng tiền (khớp với tbDonHang.tongtien)</summary>
    [Column(TypeName = "money")]
    public decimal tongtien { get; set; }

    /// <summary>Đã ký số chưa</summary>
    public bool is_digital_signed { get; set; }

    /// <summary>Dữ liệu QR Code (Base64) — quét để xác nhận</summary>
    [MaxLength(2000)]
    public string? qr_code_data { get; set; }

    /// <summary>Loại chứng từ: 'EInvoice' hoặc 'EWaybill'</summary>
    [MaxLength(20)]
    public string loaichungtu { get; set; } = "EInvoice";

    // Navigation
    [ForeignKey("madh")]
    public virtual tbDonHang tbDonHang { get; set; } = null!;
}
