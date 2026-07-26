// ============================================================
// ⏰ AutoCancelPendingOrdersService — Tự động hủy đơn chờ thanh toán
// ============================================================
// Ý nghĩa: Background service tự động hủy các đơn "Chờ thanh toán" quá hạn
// Chức năng: Kiểm tra mỗi 5 phút, hủy đơn quá 30 phút chưa thanh toán
// KEYWORDS: background service, auto cancel, pending, timeout, cleanup, đơn chờ thanh toán, tự động hủy
//
// LUỒNG DỮ LIỆU:
//   App khởi động ⭢ Program.cs gọi services.AddHostedService<AutoCancelPendingOrdersService>()
//   ExecuteAsync() chạy vòng lặp vô hạn ⭢ mỗi 5 phút kiểm tra DB
//   Query: tbDonHang WHERE trangthai="Chờ thanh toán" AND ngaydathang < Now - 30 phút
//   Tìm thấy đơn hết hạn ⭢ dh.trangthai = "Đã hủy" ⭢ db.SaveChangesAsync()
//   Đơn bị hủy ⭢ không SignalR (vì service chạy nền, ko có hubContext)
//   ⚠️ LƯU Ý: Không tự động hoàn tiền — nếu user đã chuyển khoản, cần thủ công
//
// FILES LIÊN QUAN:
//   REGISTERED IN: Program.cs (AddHostedService)
//   CALLS:      DbContext.tbDonHangs (query + update status)
//   LIÊN QUAN:  tbDonHang.cs (trangthai = "Chờ thanh toán", "Đã hủy")
//   LIÊN QUAN:  PaymentController.cs (có thể gọi hoàn tiền nếu đã thanh toán — tương lai)
// ============================================================
using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Services;

public class AutoCancelPendingOrdersService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AutoCancelPendingOrdersService> _logger;

    public AutoCancelPendingOrdersService(IServiceProvider services, ILogger<AutoCancelPendingOrdersService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoCancelPendingOrdersService started (timeout: 30 minutes)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<dbFoodyEntities>();

                // ponytail: 30 phút timeout thay vì 15 — bank webhook có thể chậm do ngân hàng xử lý
                var cutoff = DateTime.Now.AddMinutes(-30);
                var expiredOrders = await db.tbDonHangs
                    .Where(dh => dh.trangthai == "Chờ thanh toán" && dh.ngaydathang < cutoff)
                    .ToListAsync(stoppingToken);

                if (expiredOrders.Count > 0)
                {
                    foreach (var dh in expiredOrders)
                    {
                        dh.trangthai = "Đã hủy";
                        _logger.LogInformation("Auto-cancel pending order #{OrderId} (created: {Created})", dh.madh, dh.ngaydathang);
                    }
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Auto-canceled {Count} expired pending orders", expiredOrders.Count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "AutoCancelPendingOrdersService error");
            }

            // Chạy mỗi 5 phút
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
