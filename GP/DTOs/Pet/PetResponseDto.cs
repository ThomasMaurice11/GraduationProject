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
        public string HealthStatus { get; set; }
        public string UserId { get; set; }
        public List<string> PhotoUrls { get; set; }
        public OwnerDto Owner { get; set; }
    }


   
}
