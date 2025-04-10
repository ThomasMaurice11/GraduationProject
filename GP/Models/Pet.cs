    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    namespace GP.Models
    {
        public class Pet
        {
            [Key]
            public int PetId { get; set; }
        [Required]
        public string Title { get; set; }  

        [Required]
            public string Name { get; set; }

            [Required]
            public int Age { get; set; }

            public string Breed { get; set; }

            [Required]
            public string Gender { get; set; }

        public string HealthIssues { get; set; }  // Changed from HealthStatus
        public string Description { get; set; }  // New field

        [Required]
            public string UserId { get; set; } // Owner's ID

            [ForeignKey("UserId")]
          public ApplicationUser Owner { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();

       
        }
    }
