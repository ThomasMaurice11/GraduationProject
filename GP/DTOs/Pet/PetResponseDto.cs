using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Pet
{
    public class PetResponseDto
    {
        public int PetId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Title { get; set; }  
        public string Breed { get; set; }
        public string Gender { get; set; }
        public string HealthIssues { get; set; } 
        public string Description { get; set; }  
        
        public string UserId { get; set; }
    
        public int Adoption { get; set; } 
       
        public int Marriage { get; set; } 
        public List<string> PhotoUrls { get; set; }
        public OwnerDto Owner { get; set; }
    }


   
}
