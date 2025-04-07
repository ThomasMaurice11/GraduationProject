using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
