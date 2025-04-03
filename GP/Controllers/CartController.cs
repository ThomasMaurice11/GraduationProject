using DocumentFormat.OpenXml.InkML;
using GP.DTOs.Cart;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public CartController(AuthDbContext context)
        {
            _context = context;
        }

        // Add an item to the cart
        [HttpPost("Add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO addToCartDTO)
        {


            var userId = User.FindFirst("id")?.Value;



            // Find or create the user's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            // Check if the item already exists in the cart
            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                (ci.PetId == addToCartDTO.PetId && ci.ItemType == "Pet") ||
                (ci.AnimalId == addToCartDTO.AnimalId && ci.ItemType == "Animal"));

            if (existingItem != null)
            {
                return BadRequest("Item already exists in the cart.");
            }

            // Create a new cart item
            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                PetId = addToCartDTO.PetId,
                AnimalId = addToCartDTO.AnimalId,
                ItemType = addToCartDTO.ItemType
            };

            cart.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            return Ok("Item added to cart.");
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<CartDTO>> GetCart(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Pet)
                        .ThenInclude(p => p.Photos) // Include Pet photos
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Animal)
                        .ThenInclude(a => a.Photos) // Include Animal photos
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found.");
            }

            var cartDTO = new CartDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                CartItems = cart.CartItems.Select(ci => new CartItemDTO
                {
                    CartItemId = ci.CartItemId,
                    PetId = ci.PetId,
                    AnimalId = ci.AnimalId,
                    ItemType = ci.ItemType,
                    Name = ci.ItemType == "Pet" ? ci.Pet?.Name : ci.Animal?.Description,
                    Age = ci.ItemType == "Pet" ? ci.Pet?.Age ?? 0 : ci.Animal?.Age ?? 0,
                    Gender = ci.ItemType == "Pet" ? ci.Pet?.Gender : ci.Animal?.Gender,
                    PhotoUrls = ci.ItemType == "Pet"
                        ? ci.Pet?.Photos.Select(p => p.ImageUrl).ToList()
                        : ci.Animal?.Photos.Select(p => p.ImageUrl).ToList() // Fetch all photo URLs
                }).ToList()
            };

            return Ok(cartDTO);
        }

        // Remove an item from the cart
        [HttpDelete("Remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound("Item not found in cart.");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok("Item removed from cart.");
        }
    }
}