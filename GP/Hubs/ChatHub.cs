using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GP.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AuthDbContext _context;

        public ChatHub(AuthDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.User?.FindFirst("id")?.Value;
            var senderName = Context.User.FindFirst("userName")?.Value;

            // Create a room name (e.g., "User1/User2")
            var roomName = GetRoomName(senderId, receiverId);

            // Save the message to the database
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                RoomName = roomName,
                Message = message,
                SenderName = senderName,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Broadcast the message to the room
            await Clients.Group(roomName).SendAsync("ReceiveMessage", senderId, senderName, message);
        }

        public async Task JoinRoom(string receiverId)
        {
            var senderId = Context.User?.FindFirst("id")?.Value;
            var roomName = GetRoomName(senderId, receiverId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        private string GetRoomName(string senderId, string receiverId)
        {
            // Ensure the room name is always in the same order
            return string.Compare(senderId, receiverId) < 0
                ? $"{senderId}/{receiverId}"
                : $"{receiverId}/{senderId}";
        }

        // Method to leave a room
        public async Task LeaveRoom(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }
    }
}