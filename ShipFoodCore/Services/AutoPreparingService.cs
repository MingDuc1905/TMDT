using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ShipFood.Hubs;
using ShipFood.Models;

namespace ShipFood.Services;

/// <summary>
/// AutoPreparingService — REMOVED
/// Auto-transition from "Đã xác nhận" → "Đang chuẩn bị" was breaking the order flow.
/// The restaurant should manually click "Chuẩn bị xong" when food is ready.
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
        _logger.LogInformation("AutoPreparingService DISABLED — restaurant must manually confirm food ready");

        // Keep service alive but do nothing — auto-transition removed
        // Orders stay at "Đã xác nhận" until restaurant clicks "Chuẩn bị xong"
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
