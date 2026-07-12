using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ShipFood.Hubs;
using ShipFood.Models;

namespace ShipFood.Services;

/// <summary>
/// Task 3b: Auto "Đang chuẩn bị" Simulation
/// Khi nhà hàng bấm xác nhận đơn → chuyển "Đã xác nhận" → 
/// BackgroundService tự động chờ 5 giây → chuyển "Đang chuẩn bị" → 
/// SignalR thông báo tìm Shipper
/// </summary>
public class AutoPreparingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<Chats> _hubContext;
    private readonly ILogger<AutoPreparingService> _logger;

    public AutoPreparingService(
        IServiceScopeFactory scopeFactory,
        IHubContext<Chats> hubContext,
        ILogger<AutoPreparingService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoPreparingService started — polling for confirmed orders every 10s");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<dbFoodyEntities>();

                // Tìm đơn đã được nhà hàng xác nhận và chưa được xử lý
                // (Đơn ở trạng thái "Đã xác nhận" quá 5 giây → tự động chuyển "Đang chuẩn bị")
                var cutoff = DateTime.Now.AddSeconds(-5);
                // ponytail: AsNoTracking() cho SELECT, chỉ lấy ID tối thiểu, sau đó Attach để update
                var orderIds = await db.tbDonHangs
                    .AsNoTracking()
                    .Where(d => d.trangthai == "Đã xác nhận" && d.ngaydathang <= cutoff)
                    .Select(d => d.madh)
                    .Take(5)
                    .ToListAsync(stoppingToken);

                // Batch-fetch tất cả thông tin quán trước khi loop — 1 query thay vì N queries
                var orderInfos = await db.tbDonHangs
                    .AsNoTracking()
                    .Where(d => orderIds.Contains(d.madh))
                    .Select(d => new { d.madh, d.maquan, QuanTen = d.tbQuanAn!.tenquanan })
                    .ToListAsync(stoppingToken);

                var orderInfoDict = orderInfos.ToDictionary(o => o.madh);

                foreach (var orderId in orderIds)
                {
                    // Attach entity với chỉ ID, update 1 cột — tối ưu SQL
                    var donHang = new tbDonHang { madh = orderId };
                    db.tbDonHangs.Attach(donHang);
                    donHang.trangthai = "Đang chuẩn bị";
                    _logger.LogInformation("Order #{OrderId} auto-transitioned to 'Đang chuẩn bị'", orderId);

                    // SignalR broadcast tìm Shipper
                    try
                    {
                        var info = orderInfoDict.GetValueOrDefault(orderId);
                        await _hubContext.Clients.All.SendAsync(
                            "newOrderReady",
                            orderId,
                            info?.maquan,
                            info?.QuanTen ?? "",
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "SignalR broadcast failed for order #{OrderId}", orderId);
                    }
                }

                if (orderIds.Any())
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoPreparingService error");
            }

            // Poll every 30 seconds — giảm tải DB so với 10s trước đây
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
