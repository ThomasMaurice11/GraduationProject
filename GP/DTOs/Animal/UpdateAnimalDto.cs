using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Animal
{
    public class UpdateAnimalDto
    {



        public string Description { get; set; }
        public string Title { get; set; } // Add this line

        public int Age { get; set; }
        public string FoundDate { get; set; }

        public string Gender { get; set; }

        public string? HealthIssues { get; set; }
      
        public int Adoption { get; set; } = 0;
      
        public int Marriage { get; set; } = 0;


        public List<IFormFile> Photos { get; set; }
    }
}
