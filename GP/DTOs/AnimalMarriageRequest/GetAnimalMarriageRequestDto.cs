using GP.DTOs.Pet;

namespace GP.DTOs.AnimalMarriageRequest
{
    public class GetAnimalMarriageRequestDto
    {
        public int RequestId { get; set; }
        public PetResponseDto SenderPet { get; set; }
        public AnimalResponseDto ReceiverAnimal { get; set; }
        public string Status { get; set; }

        public DateTime RequestedAt { get; set; }
    }
}
