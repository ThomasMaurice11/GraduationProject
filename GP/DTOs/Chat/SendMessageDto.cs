using GP.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP.DTOs.Chat
{
    public class SendMessageDto
    {
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
