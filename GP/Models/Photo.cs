namespace GP.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageUrl { get; set; }
        public int? PetId { get; set; } // Nullable foreign key for Pet
        public Pet? Pet { get; set; } // Navigation property for Pet
        public int? PostId { get; set; } // Nullable foreign key for Pet
        public Post? Post { get; set; } // Navigation property for Pet
        public int? AnimalId { get; set; } // Nullable foreign key for Animal
        public Animal? Animal { get; set; } // Navigation property for Animal
    }
}
