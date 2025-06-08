using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GP.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        // Foreign key for the sender
        public string SenderId { get; set; }

        // Navigation property for the sender
        [ForeignKey("SenderId")]
        [JsonIgnore]
        public ApplicationUser Sender { get; set; }

        // Foreign key for the receiver
        public string ReceiverId { get; set; }

        // Navigation property for the receiver
        [ForeignKey("ReceiverId")]

        [JsonIgnore]
        public ApplicationUser Receiver { get; set; }

        [Required]
        public string RoomName { get; set; } // Room name (e.g., "User1-User2")
        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
