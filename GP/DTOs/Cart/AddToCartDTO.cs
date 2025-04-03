    namespace GP.DTOs.Cart
{
    public class AddToCartDTO
    {
        public int? PetId { get; set; } // ID of the pet to add (nullable)
        public int? AnimalId { get; set; } // ID of the animal to add (nullable)
        public string ItemType { get; set; } // "Pet" or "Animal"
    }
}
