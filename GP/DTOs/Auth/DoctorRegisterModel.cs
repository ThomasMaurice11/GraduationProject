using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Auth
{
    public class DoctorRegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        public string Number { get; set; }

        public string MedicalId { get; set; }

       
      
    }
}
