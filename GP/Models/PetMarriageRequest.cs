using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace GP.Models
{
    public class PetMarriageRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int SenderPetId { get; set; }

        [ForeignKey("SenderPetId")]
        public Pet SenderPet { get; set; }

        [Required]
        public int ReceiverPetId { get; set; }

        [ForeignKey("ReceiverPetId")]
        public Pet ReceiverPet { get; set; }

       
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
