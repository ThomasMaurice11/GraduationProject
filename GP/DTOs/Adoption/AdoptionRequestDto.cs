using System.Text.Json.Serialization;

namespace GP.DTOs.Adoption
{
    public class AdoptionRequestDto
    {

        //[JsonIgnore]
        //public int AdoptionRequestId { get; set; }
        //[JsonIgnore]
        //public UserDto Requester { get; set; }
        [JsonIgnore]
        public string? UserId { get; set; }
        public int? PetId { get; set; }
        public int? AnimalId { get; set; }

        //[JsonIgnore]
        //public string Status { get; set; } = "Pending";

        //[JsonIgnore]
        //public DateTime RequestedAt { get; set; }

        public string AnotherPet { get; set; }
        public string OwnedAnimalBefore { get; set; }
        public string HoursAnimalAlone { get; set; }

    
    }
}
