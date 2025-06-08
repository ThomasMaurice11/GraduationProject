using GP.DTOs.Cart;
using GP.Exceptions;
using GP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost("Add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO addToCartDTO)
        {
            try
            {
                var userId = User.FindFirst("id")?.Value;
                if (userId == null)
                    throw new AppException("User not authenticated.", 401, "Unauthorized");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    _context.Carts.Add(cart);
                }

                var existingItem = cart.CartItems.FirstOrDefault(ci =>
                    (ci.PetId == addToCartDTO.PetId && ci.ItemType == "Pet") ||
                    (ci.AnimalId == addToCartDTO.AnimalId && ci.ItemType == "Animal"));

                if (existingItem != null)
                    throw new AppException("Item already exists in the cart.", 400, "Bad Request");

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
            catch (AppException)
            {
                throw; // Let global handler catch it
            }
            catch (Exception ex)
            {
                throw new AppException("Failed to add item to cart.", 500, "Internal Server Error") { Source = ex.Source };
            }
        }

        [HttpGet("MyCart")]
        public async Task<ActionResult<CartDTO>> GetCart()
        {
            try
            {
                var userId = User.FindFirst("id")?.Value;
                if (userId == null)
                    throw new AppException("User not authenticated.", 401, "Unauthorized");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Pet)
                            .ThenInclude(p => p.Photos)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Animal)
                            .ThenInclude(a => a.Photos)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    throw new AppException("Cart not found.", 404, "Not Found");

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
                            : ci.Animal?.Photos.Select(p => p.ImageUrl).ToList()
                    }).ToList()
                };

                return Ok(cartDTO);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AppException("Failed to fetch cart.", 500, "Internal Server Error") { Source = ex.Source };
            }
        }

        [HttpDelete("Remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem == null)
                    throw new AppException("Item not found in cart.", 404, "Not Found");

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok("Item removed from cart.");
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AppException("Failed to remove item from cart.", 500, "Internal Server Error") { Source = ex.Source };
            }
        }
    }
}
