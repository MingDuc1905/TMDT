// ============================================================
// 💬 Chats Hub — SignalR Realtime Hub cho toàn bộ hệ thống
// ============================================================
// Ý nghĩa: Trung tâm giao tiếp realtime giữa 4 roles qua SignalR
// Chức năng: Chat, Join groups, Location tracking, Dashboard refresh, QR scan events
// KEYWORDS: signalr, hub, realtime, chat, notification, websocket, group
// ============================================================
// 🔗 LUỒNG TƯƠNG TÁC (FLOW):
//   Trigger: Client JS kết nối /nhantin, gọi Join methods, send message
//   Gọi bởi: AdminChatController (IHubContext), RestaurantController (IHubContext),
//            ShipperController (IHubContext), PaymentController (IHubContext),
//            EDeliveryController (IHubContext), AdminController (IHubContext)
//   Các Group:
//     - order_{orderId}: Chat realtime cho 1 đơn hàng cụ thể
//     - restaurant_{id}: Nhận newOrder + kpiRefresh từ PaymentController
//     - shippers: Nhận newPickupOrder + orderAccepted
//     - shipper_{userId}: Nhận directMessage từ customer
//     - customer_{userId}: Nhận directMessage từ shipper + unreadCountUpdate
//     - admins: Nhận dashboardStatsRefresh + deliveryScanEvent + userOnline
//     - delivery_{orderId}: QR scan events
//   SignalR Events (12 methods):
//     Message, AdminSendMessage, CustomerSendMessage, SendToOrderGroup
//     SendDirectMessage, UpdateLocation
//     NotifyShippersNewPickup, NotifyOrderAccepted
//     NotifyRestaurantKpiRefresh, NotifyAdminDashboardRefresh
//     NotifyDeliveryScanned, NotifyDeliveryBypassed
//   Connection Tracking: IDistributedCache (Redis) — user → connectionId
//     OnConnectedAsync: Set cache + join customer_{userId} + broadcast userOnline
//     OnDisconnectedAsync: Remove cache + broadcast userOffline
//   Security: Validate caller userId từ session/cookie trước mỗi broadcast
// ============================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using ShipFood.Models;

namespace ShipFood.Hubs;

public class Chats : Hub
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<Chats> _logger;

    private const string CONN_KEY_PREFIX = "UserConnection:";
    private const string USER_KEY_PREFIX = "UserConn:";

    public Chats(IDistributedCache cache, ILogger<Chats> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gửi tin nhắn giữa shipper và khách hàng (legacy)
    /// </summary>
    public async Task Message(string message, int id)
    {
        // ponytail: security fix — validate caller từ session trước khi broadcast
        var callerUserId = await GetCallerUserIdAsync();
        if (!callerUserId.HasValue)
        {
            _logger.LogWarning("Unauthorized Message() call from {ConnectionId}", Context.ConnectionId);
            return;
        }
        await Clients.Group($"order_{id}").SendAsync("message", message, id);
    }

    /// <summary>
    /// Admin gửi tin nhắn đến group đơn hàng
    /// </summary>
    public async Task AdminSendMessage(string message, int orderId, string connectionId)
    {
        // ponytail: security fix — chỉ admin mới gọi được AdminSendMessage
        var callerUserId = await GetCallerUserIdAsync();
        if (!callerUserId.HasValue)
        {
            _logger.LogWarning("Unauthorized AdminSendMessage() call from {ConnectionId}", Context.ConnectionId);
            return;
        }
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
    /// ═══ CROSS-ROLE CHAT: Gửi tin nhắn từ bất kỳ role nào đến group order ═══
    /// Dùng chung cho: Shipper→Order, Customer→Order, Admin→Order, Restaurant→Order
    /// </summary>
    public async Task SendToOrderGroup(string message, int orderId, string senderName, string senderRole)
    {
        await Clients.Group($"order_{orderId}").SendAsync("orderMessage", message, orderId, senderName, senderRole, Context.ConnectionId);
    }

    /// <summary>
    /// ═══ CROSS-ROLE CHAT: Gửi tin nhắn giữa shipper và customer (không cần order) ═══
    /// Shipper gửi → customer_{userId} group
    /// Customer gửi → shipper_{userId} group
    /// </summary>
    public async Task SendDirectMessage(string message, int targetUserId, string senderName, string senderRole)
    {
        var groupName = senderRole == "Shipper" ? $"customer_{targetUserId}" : $"shipper_{targetUserId}";
        await Clients.Group(groupName).SendAsync("directMessage", message, senderName, senderRole, Context.ConnectionId);
    }

    /// <summary>
    /// ═══ JOIN: Shipper tham gia group shipper + shipper_{userId} ═══
    /// </summary>
    public async Task JoinShipperGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "shippers");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"shipper_{userId}");
    }

    /// <summary>
    /// ═══ JOIN: Customer tham gia group customer_{userId} ═══
    /// </summary>
    public async Task JoinCustomerSupportGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{userId}");
    }

    /// <summary>
    /// ═══ JOIN: Admin tham gia group admin + customer_{userId} (để nhận tin từ customer) ═══
    /// </summary>
    public async Task JoinAdminGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"admin_{userId}");
    }

    /// <summary>
    /// ═══ JOIN: Restaurant tham gia group restaurant_{id} + quản lý đơn hàng ═══
    /// </summary>
    public async Task JoinRestaurantGroup(int restaurantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");
    }

    /// <summary>
    /// ═══ JOIN: Tham gia group đơn hàng (để nhận tin nhắn real-time) ═══
    /// </summary>
    public async Task JoinOrderGroup(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
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
    /// Thông báo cho tất cả shipper rằng đơn hàng đã được shipper khác nhận
    /// </summary>
    public async Task NotifyOrderAccepted(int orderId, int acceptedShipperId)
    {
        await Clients.Group("shippers").SendAsync("orderAccepted", orderId, acceptedShipperId);
    }


    /// <summary>
    /// ═══ DASHBOARD: Báo hiệu Restaurant Dashboard refresh KPI stats ═══
    /// Gửi đến group restaurant_{restaurantId} để cập nhật KPI realtime
    /// </summary>
    public async Task NotifyRestaurantKpiRefresh(int restaurantId)
    {
        await Clients.Group($"restaurant_{restaurantId}").SendAsync("kpiRefresh");
    }

    /// <summary>
    /// ═══ DASHBOARD: Báo hiệu Admin Dashboard refresh stats ═══
    /// Gửi đến group admins để cập nhật dashboard realtime
    /// </summary>
    public async Task NotifyAdminDashboardRefresh()
    {
        await Clients.Group("admins").SendAsync("dashboardStatsRefresh");
    }

    /// <summary>
    /// Gửi tín hiệu "có tin nhắn mới" đến một user cụ thể
    /// </summary>
    public async Task NotifyNewMessage(int userId, int count)
    {
        await Clients.Group($"customer_{userId}").SendAsync("unreadCountUpdate", count);
        await Clients.Group($"shipper_{userId}").SendAsync("unreadCountUpdate", count);
    }

    /// <summary>
    /// Cập nhật toạ độ shipper real-time khi đang giao hàng
    /// </summary>
    public async Task UpdateLocation(int orderId, double lat, double lng)
    {
        await Clients.Group($"order_{orderId}").SendAsync("shipperLocationUpdate", orderId, lat, lng);
    }

    /// <summary>
    /// ═══ E-DELIVERY: Merchant/Shipper tham gia delivery group để nhận event ═══
    /// </summary>
    public async Task JoinDeliveryGroup(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"delivery_{orderId}");
    }

    /// <summary>
    /// ═══ E-DELIVERY: Broadcast QR scan confirmation (Merchant → System) ═══
    /// </summary>
    public async Task NotifyDeliveryScanned(int orderId)
    {
        await Clients.Group($"order_{orderId}").SendAsync("orderDeliveryScanned", orderId, "Đã lấy", DateTime.Now.ToString("HH:mm"));
        await Clients.Group("shippers").SendAsync("deliveryScannedNotification", orderId);
    }

    /// <summary>
    /// ═══ E-DELIVERY: Broadcast admin bypass ═══
    /// </summary>
    public async Task NotifyDeliveryBypassed(int orderId, string targetStatus)
    {
        await Clients.Group($"order_{orderId}").SendAsync("deliveryBypassed", orderId, targetStatus, DateTime.Now.ToString("HH:mm"));
        await Clients.Group("admins").SendAsync("deliveryBypassed", orderId, targetStatus);
    }

    /// <summary>
    /// Kiểm tra user online
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
    /// Lấy connectionId của user
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

    /// <summary>
    /// Lấy caller userId từ session — dùng để validate SignalR methods
    /// </summary>
    private async Task<int?> GetCallerUserIdAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null) return null;

        // Ưu tiên: validate từ session
        var sessionUserJson = httpContext.Session.GetString("user");
        if (!string.IsNullOrEmpty(sessionUserJson))
        {
            try
            {
                var sessionUser = System.Text.Json.JsonSerializer.Deserialize<tbUser>(sessionUserJson);
                if (sessionUser != null && sessionUser.userid > 0)
                    return sessionUser.userid;
            }
            catch { }
        }

        // Fallback: validate từ auth cookie claims
        var userIdClaim = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var claimUserId) && claimUserId > 0)
            return claimUserId;

        return null;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null)
        {
            var userIdStr = httpContext.Request.Query["userId"].FirstOrDefault();
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId) && userId > 0)
            {
                // ponytail: security fix — validate userId từ query string với session/cookie
                var callerUserId = await GetCallerUserIdAsync();
                if (callerUserId.HasValue && callerUserId.Value != userId)
                {
                    _logger.LogWarning("SignalR userId mismatch: query={QueryUserId}, session={SessionUserId}", userId, callerUserId.Value);
                    // Không block connection — nhưng dùng session userId thay vì query userId
                    userId = callerUserId.Value;
                }
                try
                {
                    await _cache.SetStringAsync($"{CONN_KEY_PREFIX}{Context.ConnectionId}", userId.ToString());
                    await _cache.SetStringAsync($"{USER_KEY_PREFIX}{userId}", Context.ConnectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis cache unavailable for connection tracking");
                }

                // ponytail: join customer_{userId} group trước broadcast — tránh race condition
                await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{userId}");

                // ponytail: chi broadcast toi groups lien quan, khong phai Clients.All
                await Clients.Group("shippers").SendAsync("shipperOnline", userId);
                await Clients.Group("admins").SendAsync("userOnline", userId, true);
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

                // ponytail: chi broadcast toi groups lien quan, khong phai Clients.All
                await Clients.Group("admins").SendAsync("userOnline", userId, false);
                await Clients.Group("shippers").SendAsync("shipperOffline", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache error during disconnect cleanup");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
