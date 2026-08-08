using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GreenBasket.Domain.Entities;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Interfaces;
using GreenBasket.Infrastructure.Data;

namespace GreenBasket.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.AppUserId == userId);

            if (cart == null) return new CartDto { AppUserId = userId };

            return new CartDto
            {
                Id = cart.Id,
                AppUserId = cart.AppUserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "Unknown",
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice
                }).ToList()
            };
        }

        public async Task<CartDto> AddToCartAsync(AddToCartDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AppUserId == dto.AppUserId);

            if (cart == null)
            {
                cart = new Cart { AppUserId = dto.AppUserId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Ensure Cart gets an ID
            }

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) throw new System.Exception("Product not found");

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.Price
                });
            }

            await _context.SaveChangesAsync();
            return await GetCartAsync(dto.AppUserId);
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, UpdateCartItemDto dto)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
