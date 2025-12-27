using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatty.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string roomCode, string user, string message)
        {
            // Send only to clients in this group
            await Clients.Group(roomCode).SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinRoom(string roomCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        }
    }
}
