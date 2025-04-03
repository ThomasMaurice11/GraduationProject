namespace GP.DTOs.Cart
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public string UserId { get; set; } // ID of the user who owns the cart
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
    }
}
