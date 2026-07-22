namespace ShipFood.Utils;

/// <summary>
/// Order status constants — thay thế magic strings khắp controllers/views/js
/// </summary>
public static class OrderStatus
{
    // ─── Order Lifecycle ───
    public const string ChoThanhToan = "Chờ thanh toán";
    public const string DaDat = "Đã đặt";
    public const string DaXacNhan = "Đã xác nhận";
    public const string DangChuanBi = "Đang chuẩn bị";
    public const string ChoShipper = "Chờ shipper lấy hàng";
    public const string DaNhan = "Đã nhận";
    public const string DaLay = "Đã lấy";
    public const string DangGiao = "Đang giao";
    public const string HoanThanh = "Hoàn thành";
    public const string DaHuy = "Đã hủy";

    // ─── Shipper Status ───
    public const string DangHoatDong = "Đang hoạt động";
    public const string KhongHoatDong = "Không hoạt động";

    // ─── Restaurant Status ───
    public const string DangMoCua = "Đang mở cửa";
    public const string DongCua = "Đóng cửa";

    // ─── Role Names ───
    public const string RoleShipper = "Shipper";
    public const string RoleQuanAn = "Quán ăn";
    public const string RoleAdmin = "Admin";
    public const string RoleKhachHang = "Khách hàng";

    // ─── Valid Transition Map ───
    public static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        [DaNhan] = new[] { DaXacNhan, ChoShipper },
        [DaLay] = new[] { DaXacNhan, ChoShipper, DangGiao },
        [DangGiao] = new[] { DaLay },
        [HoanThanh] = new[] { DangGiao, DaLay },
    };

    // ─── Status display order (for progress bars) ───
    public static readonly string[] StatusFlow =
    {
        DaDat, DaXacNhan, DangChuanBi, ChoShipper, DaLay, DangGiao, HoanThanh
    };

    // ─── Auto-message templates ───
    public static readonly Dictionary<string, string> AutoMessages = new()
    {
        [DaXacNhan] = "✅ Quán đã xác nhận đơn hàng! Đang chuẩn bị món.",
        [ChoShipper] = "👨‍🍳 Quán đã chuẩn bị xong món! Đang chờ shipper đến lấy.",
        [DaNhan] = "🛵 Shipper đã nhận đơn! Đang đến quán lấy hàng.",
        [DaLay] = "📦 Shipper đã lấy hàng từ quán! Đang trên đường giao đến bạn.",
        [DangGiao] = "🚚 Đơn hàng đang được giao đến bạn!",
        [HoanThanh] = "✅ Đơn hàng đã giao thành công! Cảm ơn bạn đã sử dụng FastShip.",
        [DaHuy] = "❌ Đơn hàng đã bị hủy.",
    };

    /// <summary>
    /// Kiểm tra transition hợp lệ
    /// </summary>
    public static bool IsValidTransition(string newStatus, string currentStatus)
    {
        return AllowedTransitions.TryGetValue(newStatus, out var validPrev)
               && validPrev.Contains(currentStatus);
    }
}
