
namespace GP.DTOs.PetMarriageRequest
{
    using GP.DTOs.Pet;
    using GP.Models;

    public class GetPetMarriageRequestDto
    {
        public int RequestId { get; set; }
        public PetResponseDto SenderPet { get; set; }
        public PetResponseDto ReceiverPet { get; set; }
        public string Status { get; set; }

        public DateTime RequestedAt { get; set; }
    }
}
