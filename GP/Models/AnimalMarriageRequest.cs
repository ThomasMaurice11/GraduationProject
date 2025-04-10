using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GP.Models
{
    public class AnimalMarriageRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int SenderPetId { get; set; }

        [ForeignKey("SenderPetId")]
        public Pet SenderPet { get; set; }

        [Required]
        public int ReceiverAnimalId { get; set; }

        [ForeignKey("ReceiverAnimalId")]
        public Animal ReceiverAnimal { get; set; }


        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
