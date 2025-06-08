using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GP.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        public string UserId { get; set; } // The user who owns the cart

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}