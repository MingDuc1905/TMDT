using Microsoft.EntityFrameworkCore;
using ShipFood.Models;

namespace ShipFood.Services;

/// <summary>
/// Background service: t? d?ng h?y c�c don h�ng "Ch? thanh to�n" qu� 15 ph�t
/// Ch?y m?i 5 ph�t, tr�nh database d?y don "ma"
/// </summary>
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
        _logger.LogInformation("AutoCancelPendingOrdersService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<dbFoodyEntities>();

                var cutoff = DateTime.Now.AddMinutes(-15);
                var expiredOrders = await db.tbDonHangs
                    .Where(dh => dh.trangthai == "Ch? thanh to�n" && dh.ngaydathang < cutoff)
                    .ToListAsync(stoppingToken);

                if (expiredOrders.Count > 0)
                {
                    foreach (var dh in expiredOrders)
                    {
                        dh.trangthai = "�? h?y";
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

            // Ch?y m?i 5 ph�t
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
