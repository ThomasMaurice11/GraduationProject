using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Microsoft.EntityFrameworkCore;

namespace GP.Models
{
    public class AdoptionRequest
    {
        [Key]
        public int AdoptionRequestId { get; set; }

        [Required]
        public string UserId { get; set; } // Owner's ID

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)] // Prevent cascade delete
        public ApplicationUser Owner { get; set; }

        public int? PetId { get; set; } // Nullable foreign key for Pet

        [ForeignKey("PetId")]
        [DeleteBehavior(DeleteBehavior.NoAction)] // Prevent cascade delete
        public Pet Pet { get; set; } // Navigation property for Pet

        public int? AnimalId { get; set; } // Nullable foreign key for Animal

        [ForeignKey("AnimalId")]
        [DeleteBehavior(DeleteBehavior.NoAction)] // Prevent cascade delete
        public Animal Animal { get; set; } // Navigation property for Animal

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public string AnotherPet { get; set; }
        public string OwnedAnimalBefore { get; set; }
        public string HoursAnimalAlone { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}   