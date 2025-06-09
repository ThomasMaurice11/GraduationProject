using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GP.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int ClinicId { get; set; }

        [ForeignKey("ClinicId")]
       
        public Clinic Clinic { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public int SlotId { get; set; }

        [ForeignKey("SlotId")]
        //[DeleteBehavior(DeleteBehavior.NoAction)] // Changed from Cascade
        
        public Slot Slot { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime AppointmentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? PatientNotes { get; set; }

        [MaxLength(500)]
        public string? DoctorNotes { get; set; }

        public string PetName { get; set; }
        public string Breed { get; set; }
        public string PetType { get; set; }
        public string ?ReasonForVisit { get; set; }
      


    }
}