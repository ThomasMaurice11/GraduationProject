using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;


namespace GP.Models
{
    public class ApplicationUser: IdentityUser
    {
        [JsonIgnore]
        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
        [JsonIgnore]



        public virtual ICollection<ChatMessage> SentMessages { get; set; }
        public virtual ICollection<ChatMessage> ReceivedMessages { get; set; }
       

        // If the user is a doctor, this will store the doctor's profile
        
        
    }
}
