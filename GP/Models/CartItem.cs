using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Required]
        public int CartId { get; set; }

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        // Polymorphic relationship: Can reference either Pet or Animal
        public int? PetId { get; set; } // Nullable, in case the item is an Animal
        public int? AnimalId { get; set; } // Nullable, in case the item is a Pet

        [ForeignKey("PetId")]
        public Pet Pet { get; set; }

        [ForeignKey("AnimalId")]
        public Animal Animal { get; set; }

        // Discriminator to differentiate between Pet and Animal
        public string ItemType { get; set; } // "Pet" or "Animal"
    }
}