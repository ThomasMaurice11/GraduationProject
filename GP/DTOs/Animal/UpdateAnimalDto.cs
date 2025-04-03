using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Animal
{
    public class UpdateAnimalDto
    {
        
   
      
        public int Age { get; set; }

 
        public string Gender { get; set; }

        public string HealthIssues { get; set; }
        public IFormFile? Photo { get; set; }
        public string Description { get; set; }
    }
}
