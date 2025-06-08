using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Pet
{
    public class PetCreateDto
    {

   
        public string Name { get; set; }

       
        public int Age { get; set; }
        public string Title { get; set; }

        public string HealthIssues { get; set; }  // Changed from HealthStatus
        public string Description { get; set; }  // New field
        public string? Breed { get; set; }

       
        public string Gender { get; set; }

    
  


        public List<IFormFile> Photos { get; set; }

    }
}
