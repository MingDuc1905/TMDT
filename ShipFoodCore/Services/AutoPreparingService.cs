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
                var confirmedOrders = await db.tbDonHangs
                    .Where(d => d.trangthai == "Đã xác nhận" && d.ngaydathang <= cutoff)
                    .Include(d => d.tbQuanAn)
                    .Take(5)
                    .ToListAsync(stoppingToken);

                foreach (var donHang in confirmedOrders)
                {
                    donHang.trangthai = "Đang chuẩn bị";
                    _logger.LogInformation("Order #{OrderId} auto-transitioned to 'Đang chuẩn bị'", donHang.madh);

                    // SignalR broadcast tìm Shipper
                    try
                    {
                        await _hubContext.Clients.All.SendAsync(
                            "newOrderReady",
                            donHang.madh,
                            donHang.maquan,
                            donHang.tbQuanAn?.tenquanan ?? "",
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "SignalR broadcast failed for order #{OrderId}", donHang.madh);
                    }
                }

                if (confirmedOrders.Any())
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoPreparingService error");
            }

            // Poll every 10 seconds
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
