namespace GP.DTOs.Cart
{
    public class CartItemDTO
    {
        public int CartItemId { get; set; }
        public int? PetId { get; set; }
        public int? AnimalId { get; set; }
        public string ItemType { get; set; } // "Pet" or "Animal"
        public string Name { get; set; } // Name of the pet or animal
        public int Age { get; set; } // Age of the pet or animal
        public string Gender { get; set; } // Gender of the pet or animal
        //public string PhotoUrl { get; set; } // URL of the photo
        public List<string> PhotoUrls { get; set; } // List of photo URLs
    }
}
