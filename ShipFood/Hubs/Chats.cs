using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace ShipFood.Hubs
{
    [HubName("nhantin")]
    public class Chats : Hub
    {
        public void Message(string message, int id)
        {
            Clients.All.message(message, id);
        }
    }
}