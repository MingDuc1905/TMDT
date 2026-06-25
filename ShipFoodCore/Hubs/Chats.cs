using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace ShipFood.Hubs;

public class Chats : Hub
{
    /// <summary>
    /// Gửi tin nhắn giữa shipper và khách hàng
    /// </summary>
    public async Task Message(string message, int id)
    {
        await Clients.All.SendAsync("message", message, id);
    }

    /// <summary>
    /// Gửi tin nhắn từ admin đến khách hàng/shipper theo đơn hàng
    /// </summary>
    public async Task AdminSendMessage(string message, int orderId, string connectionId)
    {
        if (!string.IsNullOrEmpty(connectionId))
        {
            await Clients.Client(connectionId).SendAsync("adminMessage", message, orderId);
        }
        await Clients.All.SendAsync("adminMessageBroadcast", message, orderId, Context.ConnectionId);
    }

    /// <summary>
    /// Khách hàng gửi tin nhắn đến admin
    /// </summary>
    public async Task CustomerSendMessage(string message, int orderId, string userName)
    {
        await Clients.All.SendAsync("customerMessage", message, orderId, userName, Context.ConnectionId);
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

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
