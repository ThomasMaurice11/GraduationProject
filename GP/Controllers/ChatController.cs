using DocumentFormat.OpenXml.Spreadsheet;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GP.DTOs.Chat;
using GP.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using GP.Hubs;
using GP.Exceptions;

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
            try
            {
                var senderId = User?.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(senderId))
                    throw new AppException("User is not authenticated.", StatusCodes.Status401Unauthorized, "Unauthorized");

                if (string.IsNullOrEmpty(receiverId))
                    throw new AppException("Receiver ID cannot be null or empty.", StatusCodes.Status400BadRequest, "Bad Request");

                var roomName = GetRoomName(senderId, receiverId);

                return Ok(new { RoomName = roomName });
            }
            catch (AppException)
            {
                throw; // Let middleware handle
            }
            catch (Exception ex)
            {
                throw new AppException("Failed to start chat.", 500, ex.Message);
            }
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(string receiverId)
        {
            try
            {
                var senderId = User?.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(senderId))
                    throw new AppException("User is not authenticated.", StatusCodes.Status401Unauthorized, "Unauthorized");

                if (string.IsNullOrEmpty(receiverId))
                    throw new AppException("Receiver ID cannot be null or empty.", StatusCodes.Status400BadRequest, "Bad Request");

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
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AppException("Failed to retrieve messages.", 500, ex.Message);
            }
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
            try
            {
                var currentUserId = User?.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                    throw new AppException("User is not authenticated.", StatusCodes.Status401Unauthorized, "Unauthorized");

                var rooms = await _context.ChatMessages
                    .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                    .Select(m => m.RoomName)
                    .Distinct()
                    .ToListAsync();

                var previousChats = new List<PreviousChatDto>();

                foreach (var room in rooms)
                {
                    var userIds = room.Split('/');
                    var receiverId = userIds[0] == currentUserId ? userIds[1] : userIds[0];

                    var receiverData = await _jwtTokenService.GetUserData(receiverId);
                    if (receiverData != null)
                    {
                        previousChats.Add(new PreviousChatDto
                        {
                            RoomName = room,
                            ReceiverId = receiverId,
                            ReceiverName = receiverData.UserName
                        });
                    }
                }

                return Ok(previousChats);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AppException("Failed to fetch previous chats.", 500, ex.Message);
            }
        }
    }
}
