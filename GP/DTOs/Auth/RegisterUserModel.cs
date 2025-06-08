using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Auth
{
    public class RegisterUserModel
    {
        
        public string Username { get; set; }

    
        public string Email { get; set; }

        
        public string Password { get; set; }

    
        public string PhoneNumber { get; set; } 
    }
}
