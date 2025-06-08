namespace GP.DTOs.Adoption
{
    public class AdoptionRequestResponse
    {
      
        public int AdoptionRequestId { get; set; }

        public UserDto Requester { get; set; }

       
        public int? PetId { get; set; }
        public int? AnimalId { get; set; }

        public string Status { get; set; } = "Pending";

     
        public DateTime RequestedAt { get; set; }
        public AnimalResponseDto? AnimalInfo { get; set; }
        public string AnotherPet { get; set; }
        public string OwnedAnimalBefore { get; set; }
        public string HoursAnimalAlone { get; set; }
    }
}
