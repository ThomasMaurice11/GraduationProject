namespace GP.DTOs.Post
{
    public class PostResponseDto
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Breed { get; set; }
        public int? Age { get; set; }
        public DateTime LostDate { get; set; }
        public string LostLocation { get; set; }
        public string Gender { get; set; }
        public DateTime CreationDate { get; set; }
        public List<string> PhotoUrls { get; set; }
        public OwnerDto Owner { get; set; }
    }

}
