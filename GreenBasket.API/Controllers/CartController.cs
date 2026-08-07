using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Interfaces;

namespace GreenBasket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var cart = await _cartService.AddToCartAsync(dto);
                return Ok(cart);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-item/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            var result = await _cartService.UpdateCartItemQuantityAsync(cartItemId, dto);
            if (!result) return NotFound(new { message = "Cart item not found" });
            return Ok(new { message = "Cart item updated successfully" });
        }

        [HttpDelete("remove-item/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            var result = await _cartService.RemoveFromCartAsync(cartItemId);
            if (!result) return NotFound(new { message = "Cart item not found" });
            return Ok(new { message = "Cart item removed successfully" });
        }
    }
}
