using System.Threading.Tasks;
using GreenBasket.Application.DTOs;

namespace GreenBasket.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task<CartDto> AddToCartAsync(AddToCartDto dto);
        Task<bool> UpdateCartItemQuantityAsync(int cartItemId, UpdateCartItemDto dto);
        Task<bool> RemoveFromCartAsync(int cartItemId);
    }
}
