using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Clinic
    {
        [Key]
        public int ClinicId { get; set; }

        [Required]
        public string DoctorId { get; set; } 

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        public string LocationUrl { get; set; } 

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }


        public string Details { get; set; }

        public string Number { get; set; }
        [EmailAddress]
        public string CLinicEmail { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    }
}
