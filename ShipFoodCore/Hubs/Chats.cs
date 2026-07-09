using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ShipFood.Hubs;

public class Chats : Hub
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<Chats> _logger;

    // ponytail: Redis keys prefix cho connection tracking
    private const string CONN_KEY_PREFIX = "UserConnection:";
    private const string USER_KEY_PREFIX = "UserConn:";

    public Chats(IDistributedCache cache, ILogger<Chats> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gửi tin nhắn giữa shipper và khách hàng
    /// </summary>
    public async Task Message(string message, int id)
    {
        await Clients.All.SendAsync("message", message, id);
    }

    /// <summary>
    /// Gửi tin nhắn từ admin đến khách hàng/shipper THEO GROUP (không dùng ConnectionId)
    /// Chỉ gửi qua group — bền vững khi admin reload trang
    /// </summary>
    public async Task AdminSendMessage(string message, int orderId, string connectionId)
    {
        await Clients.Group($"order_{orderId}").SendAsync("adminMessage", message, orderId, "Admin");
    }

    /// <summary>
    /// Khách hàng gửi tin nhắn đến admin THEO GROUP
    /// </summary>
    public async Task CustomerSendMessage(string message, int orderId, string userName)
    {
        await Clients.Group($"order_{orderId}").SendAsync("customerMessage", message, orderId, userName, Context.ConnectionId);
    }

    /// <summary>
    /// Restaurant tham gia group để nhận đơn hàng mới real-time
    /// </summary>
    public async Task JoinRestaurantGroup(int restaurantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");
    }

    /// <summary>
    /// Shipper tham gia group để nhận đơn mới real-time
    /// </summary>
    public async Task JoinShipperGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "shippers");
    }

    /// <summary>
    /// Restaurant báo 'Chuẩn bị xong' → broadcast đến tất cả Shipper
    /// </summary>
    public async Task NotifyShippersNewPickup(int orderId, string restaurantName, string pickupAddress)
    {
        await Clients.Group("shippers").SendAsync("newPickupOrder", new
        {
            orderId = orderId,
            restaurantName = restaurantName,
            pickupAddress = pickupAddress
        });
    }

    /// <summary>
    /// Tham gia group theo đơn hàng (để nhận tin nhắn riêng)
    /// </summary>
    public async Task JoinOrderGroup(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    /// <summary>
    /// Tham gia group hỗ trợ khách hàng riêng (dùng cho yêu cầu chung không có đơn hàng)
    /// </summary>
    public async Task JoinCustomerSupportGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{userId}");
    }

    /// <summary>
    /// Gửi tin nhắn trong group đơn hàng
    /// </summary>
    public async Task SendToOrderGroup(string message, int orderId, string senderName, string senderRole)
    {
        await Clients.Group($"order_{orderId}").SendAsync("orderMessage", message, orderId, senderName, senderRole, Context.ConnectionId);
    }

    /// <summary>
    /// Gửi tín hiệu "có tin nhắn mới" đến một user cụ thể (dùng thay cho polling)
    /// Admin gọi phương thức này khi gửi tin nhắn cho khách hàng
    /// </summary>
    public async Task NotifyNewMessage(int userId, int count)
    {
        // Gửi real-time đến đúng user qua group customer_{userId}
        await Clients.Group($"customer_{userId}").SendAsync("unreadCountUpdate", count);
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null)
        {
            var userIdStr = httpContext.Request.Query["userId"].FirstOrDefault();
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId) && userId > 0)
            {
                // ponytail: Redis-based connection tracking — tồn tại qua restart
                try
                {
                    await _cache.SetStringAsync($"{CONN_KEY_PREFIX}{Context.ConnectionId}", userId.ToString());
                    await _cache.SetStringAsync($"{USER_KEY_PREFIX}{userId}", Context.ConnectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis cache unavailable for connection tracking — using in-memory fallback");
                }

                await Clients.All.SendAsync("userOnline", userId, true);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userIdStr = await _cache.GetStringAsync($"{CONN_KEY_PREFIX}{Context.ConnectionId}");
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                await _cache.RemoveAsync($"{CONN_KEY_PREFIX}{Context.ConnectionId}");
                await _cache.RemoveAsync($"{USER_KEY_PREFIX}{userId}");

                await Clients.All.SendAsync("userOnline", userId, false);
                await Clients.All.SendAsync("shipperOffline", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache error during disconnect cleanup");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Cập nhật toạ độ shipper real-time khi đang giao hàng
    /// Shipper gọi method này, server broadcast đến group order_{orderId}
    /// </summary>
    public async Task UpdateLocation(int orderId, double lat, double lng)
    {
        await Clients.Group($"order_{orderId}").SendAsync("shipperLocationUpdate", orderId, lat, lng);
    }

    /// <summary>
    /// Thông báo cho tất cả shipper rằng đơn hàng đã được shipper khác nhận
    /// → Các shipper còn lại xóa đơn khỏi FREE-PICK list real-time
    /// </summary>
    public async Task NotifyOrderAccepted(int orderId, int acceptedShipperId)
    {
        await Clients.Group("shippers").SendAsync("orderAccepted", orderId, acceptedShipperId);
    }

    /// <summary>
    /// Kiểm tra xem user có đang online không (qua Redis connection tracking)
    /// Graceful degradation: nếu Redis down, báo offline
    /// </summary>
    public async Task<bool> IsUserOnline(int userId)
    {
        try
        {
            var connId = await _cache.GetStringAsync($"{USER_KEY_PREFIX}{userId}");
            return !string.IsNullOrEmpty(connId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable when checking online status for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Lấy connectionId của user (để gửi tin nhắn real-time trực tiếp)
    /// Graceful degradation: nếu Redis down, trả về null
    /// </summary>
    public async Task<string?> GetUserConnectionId(int userId)
    {
        try
        {
            return await _cache.GetStringAsync($"{USER_KEY_PREFIX}{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable when getting connection for user {UserId}", userId);
            return null;
        }
    }
}
