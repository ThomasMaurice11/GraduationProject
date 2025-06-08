using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; } // Add this line

        [Required]
        public string Description { get; set; }
        public string Breed { get; set; }

        [Required]
        public string UserId { get; set; } // Owner's ID

        [ForeignKey("UserId")]
        public ApplicationUser Owner { get; set; }

        [Required]
        public int? Age { get; set; }

        public DateTime LostDate { get; set; }
        public string LostLocation { get; set; }

        [Required]
        public string Gender { get; set; }

        public DateTime CreationDate { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}
