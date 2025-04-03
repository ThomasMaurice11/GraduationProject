using GP.DTOs.Pet;

namespace GP.DTOs.Adoption
{
    public class AdoptionRequestWithDetailsDto
    {
        public int AdoptionRequestId { get; set; }
        public UserDto UserInfo { get; set; }
        public PetResponseDto PetInfo { get; set; }
        public AnimalResponseDto AnimalInfo { get; set; }
        public string Status { get; set; }
        public RequestDetailsDto RequestDetails { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
