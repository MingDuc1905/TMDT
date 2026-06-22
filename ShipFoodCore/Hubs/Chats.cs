using Microsoft.AspNetCore.SignalR;

namespace ShipFood.Hubs;

public class Chats : Hub
{
    public async Task Message(string message, int id)
    {
        await Clients.All.SendAsync("message", message, id);
    }
}
