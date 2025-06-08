using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.PetSitterRequest
{
    public class PetSitterRequestDto
    {
  
        public string? Number { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndtDate { get; set; }

        [Required]
        [StringLength(500)]
        public string HealthIssues { get; set; }

       
        public string Breed { get; set; }
        public string Age { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

    }

}
