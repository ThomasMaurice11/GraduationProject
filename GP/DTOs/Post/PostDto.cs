using System.ComponentModel.DataAnnotations;

namespace GP.DTOs.Post
{
    public class PostDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }


        [Required]
        [StringLength(50)]
        public string Breed { get; set; }

        [Required]
        [Range(0, 30)]
        public int? Age { get; set; }

        [Required]
        public DateTime LostDate { get; set; }

        [Required]
        [StringLength(200)]
        public string LostLocation { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        public List<IFormFile> Photos { get; set; }
    }
}
