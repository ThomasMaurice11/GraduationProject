using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GP.Models
{
    public class PetSitterRequest
    {
        [Key]
        public int PetSitterRequestId { get; set; }

        [Required]
        public string UserId { get; set; } // Owner's ID

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)] // Prevent cascade delete
        public ApplicationUser Owner { get; set; }

        public string? Number { get; set; } 
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public DateTime StartDate { get; set; }
        public DateTime EndtDate { get; set; }
        public string HealthIssues { get; set; }
        public string Breed { get; set; }
        public string Age { get; set; }
        public string? Notes { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
