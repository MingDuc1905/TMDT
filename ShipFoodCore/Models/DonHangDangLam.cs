using Microsoft.EntityFrameworkCore;

namespace ShipFood.Models;

[Keyless]
public class DonHangDangLam
{
    public int madh { get; set; }
    public DateTime? ngaydathang { get; set; }
    public string? tennguoinhan { get; set; }
    public string? diachi { get; set; }
    public string? tenquanan { get; set; }
    public string? DiaChiQuan { get; set; }
    public decimal? phiship { get; set; }
    public decimal? tongtien { get; set; }
    public string? trangthai { get; set; }
    public string? sdt { get; set; }
    public int? userid { get; set; }
}
