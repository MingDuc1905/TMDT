using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ShipFood.Hubs;

public class Chats : Hub
{
    /// <summary>
    /// Thread-safe dictionary lưu trữ connectionId → userId mapping
    /// Dùng để gửi tin nhắn real-time đến đúng user mà không cần polling
    /// </summary>
    private static readonly ConcurrentDictionary<string, int> _connections = new();
    private static readonly ConcurrentDictionary<int, string> _userConnections = new();

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
            // Đọc userId từ query string (JS truyền lên khi kết nối)
            var userIdStr = httpContext.Request.Query["userId"].FirstOrDefault();
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId) && userId > 0)
            {
                _connections[Context.ConnectionId] = userId;
                _userConnections[userId] = Context.ConnectionId;

                // Broadcast online status đến tất cả
                await Clients.All.SendAsync("userOnline", userId, true);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out int userId))
        {
            _userConnections.TryRemove(userId, out _);

            // Broadcast offline status
            await Clients.All.SendAsync("userOnline", userId, false);
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
    /// Kiểm tra xem user có đang online không (qua connection tracking)
    /// </summary>
    public static bool IsUserOnline(int userId)
    {
        return _userConnections.ContainsKey(userId);
    }

    /// <summary>
    /// Lấy connectionId của user (để gửi tin nhắn real-time trực tiếp)
    /// </summary>
    public static string? GetUserConnectionId(int userId)
    {
        return _userConnections.TryGetValue(userId, out var connId) ? connId : null;
    }
}
