using DocumentFormat.OpenXml.Spreadsheet;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GP.DTOs.Chat;
using GP.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GP.Hubs;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Identity;

namespace GP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    

    public class ChatController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public ChatController(AuthDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartChat(string receiverId)
        {
            var senderId = User?.FindFirst("id")?.Value;
            var roomName = GetRoomName(senderId, receiverId);

            // Optionally, you can store the room in the database if needed
            return Ok(new { RoomName = roomName });
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(string receiverId)
        {
            var senderId = User?.FindFirst("id")?.Value;
            var roomName = GetRoomName(senderId, receiverId);

            var messages = await _context.ChatMessages
                .Where(m => m.RoomName == roomName)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    Message = m.Message,
                    Timestamp = m.Timestamp,
                    SenderId = m.SenderId,
                    ReceiverId = receiverId,
                    RoomName = roomName,
                    SenderName = m.SenderName
                })
                .ToListAsync();

            return Ok(messages);
        }

        private string GetRoomName(string senderId, string receiverId)
        {
            return string.Compare(senderId, receiverId) < 0
                ? $"{senderId}/{receiverId}"
                : $"{receiverId}/{senderId}";
        }

        [HttpGet("PreviousChats")]
        public async Task<IActionResult> GetPreviousChats()
        {
            // Get the current user's ID
            var currentUserId = User?.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            // Get all unique rooms where the current user has participated
            var rooms = await _context.ChatMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .Select(m => m.RoomName)
                .Distinct()
                .ToListAsync();

            // Get the other user's details for each room
            var previousChats = new List<PreviousChatDto>();

            foreach (var room in rooms)
            {
                // Extract the other user's ID from the room name
                var userIds = room.Split('/');
                var ReceiverId = userIds[0] == currentUserId ? userIds[1] : userIds[0];

                // Get the other user's details (e.g., name)
                //var otherUser = await _context.Users
                //    .Where(u => u.Id == otherUserId)
                //    .Select(u => new { u.Id, u.UserName })
                //    .FirstOrDefaultAsync();



                var ReceiverData = await _jwtTokenService.GetUserData(ReceiverId);
                if (ReceiverData != null)
                {
                    previousChats.Add(new PreviousChatDto
                    {
                        RoomName = room,
                        ReceiverId = ReceiverId,
                        ReceiverName = ReceiverData.UserName
                    });
                }
            }
            return Ok(previousChats);
        }
    }
}