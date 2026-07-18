using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Services;

/// <summary>
/// Dịch vụ tự động gợi ý voucher theo khung giờ và thông tin khách hàng
/// Giống như Grab/ShopeeFood tự động popup voucher phù hợp
/// </summary>
public class VoucherService
{
    private readonly dbFoodyEntities _db;
    /// Lấy voucher gợi ý theo khung giờ hiện tại
    /// Trả về voucher phù hợp nhất với thời điểm hiện tại
    /// </summary>
    public async Task<tbKhuyenMai?> GetTimeSlotVoucher()
    {
        var hour = DateTime.Now.Hour;
        var now = DateTime.Now;
        string timeSlotCode;

        // ═══ Xác định khung giờ ═══
        if (hour >= 6 && hour < 10)
            timeSlotCode = "SÁNG KHOẺ";
        else if (hour >= 10 && hour < 14)
            timeSlotCode = "TRƯA NGON";
        else if (hour >= 14 && hour < 17)
            timeSlotCode = "XẾ MÊ";
        else if (hour >= 17 && hour < 22)
            timeSlotCode = "TỐI VUI";
        else // 22:00 - 06:00
            timeSlotCode = "KHUYA";

        // Tìm voucher khớp khung giờ
        var voucher = await _db.tbKhuyenMai
            .Where(k => k.tenkm.Contains(timeSlotCode)
                       && (k.ngayketthuc == null || k.ngayketthuc >= now)
                       && (k.ngaybatdau == null || k.ngaybatdau <= now))
            .OrderByDescending(k => k.phantramgiam)
            .FirstOrDefaultAsync();

        return voucher;
    }

    /// <summary>
    /// Gợi ý voucher cho khách hàng dựa trên:
    /// 1. Khung giờ hiện tại
    /// 2. Lần đầu đặt hàng (ưu tiên voucher ĐẶT LẦN ĐẦU)
    /// 3. Giá trị đơn hàng (MIỄN PHÍ SHIP nếu đủ điều kiện)
    /// </summary>
    public async Task<List<tbKhuyenMai>> GetRecommendedVouchers(int? userId, decimal? tongTien = null)
    {
        var now = DateTime.Now;
        var hour = DateTime.Now.Hour;
        var vouchers = new List<tbKhuyenMai>();

        // 1. Luôn gợi ý voucher theo khung giờ
        var timeSlotVoucher = await GetTimeSlotVoucher();
        if (timeSlotVoucher != null)
            vouchers.Add(timeSlotVoucher);

        // 2. Kiểm tra lần đầu đặt hàng
        if (userId != null)
        {
            var orderCount = await _db.tbDonHang
                .Where(dh => dh.tbThongTinDatHang != null && dh.tbThongTinDatHang.userid == userId)
                .CountAsync();

            if (orderCount == 0)
            {
                // Lần đầu → gợi ý voucher ĐẶT LẦN ĐẦU
                var firstOrderVoucher = await _db.tbKhuyenMai
                    .Where(k => k.tenkm.Contains("ĐẶT LẦN ĐẦU")
                               && (k.ngayketthuc == null || k.ngayketthuc >= now)
                               && (k.ngaybatdau == null || k.ngaybatdau <= now))
                    .FirstOrDefaultAsync();
                if (firstOrderVoucher != null)
                    vouchers.Add(firstOrderVoucher);
            }
        }

        // 3. Gợi ý MIỄN PHÍ SHIP nếu đơn hàng đủ điều kiện
        // ponytail: fix P9 — ch? g?i ? voucher free ship khi don >= 50K
        // Tru?c dây query step 4 (l?y voucher applied) tr? v? voucher nhung ko set ShippingFeeDiscount d?ng
        // => don du?i 50K van du?c g?i free ship (leak discount)
        if (tongTien.HasValue && tongTien.Value >= 50000)
        {
            var freeShipVoucher = await _db.tbKhuyenMai
                .Where(k => k.tenkm.Contains("MIỄN PHÍ SHIP")
                           && (k.ngayketthuc == null || k.ngayketthuc >= now)
                           && (k.ngaybatdau == null || k.ngaybatdau <= now))
                .FirstOrDefaultAsync();
            if (freeShipVoucher != null)
                vouchers.Add(freeShipVoucher);
        }

        // 4. Thêm các voucher phổ biến còn lại (tối đa 5)
        var existingCodes = vouchers.Select(v => v.tenkm).ToHashSet();
        var additionalVouchers = await _db.tbKhuyenMai
            .Where(k => !existingCodes.Contains(k.tenkm)
                       && (k.ngayketthuc == null || k.ngayketthuc >= now)
                       && (k.ngaybatdau == null || k.ngaybatdau <= now))
            .OrderByDescending(k => k.phantramgiam)
            .Take(5 - vouchers.Count)
            .ToListAsync();

        vouchers.AddRange(additionalVouchers);

        return vouchers;
    }

    /// <summary>
    /// Lấy thông tin khung giờ hiện tại (dùng để hiển thị UI)
    /// </summary>
    public static (string name, string icon, string description) GetCurrentTimeSlotInfo()
    {
        var hour = DateTime.Now.Hour;

        if (hour >= 6 && hour < 10)
            return ("Sáng", "🌅", "Bữa sáng nhẹ nhàng — Giảm đến 15%");
        if (hour >= 10 && hour < 14)
            return ("Trưa", "☀️", "Bữa trưa no nê — Giảm đến 25%");
        if (hour >= 14 && hour < 17)
            return ("Xế", "🌤️", "Xế chiều refresh — Giảm đến 10%");
        if (hour >= 17 && hour < 22)
            return ("Tối", "🌆", "Bữa tối ấm cúng — Giảm đến 25%");
        return ("Khuya", "🌙", "Đêm khuya đói bụng — Giảm đến 30%");
    }
}
