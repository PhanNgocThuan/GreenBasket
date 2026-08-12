using System.Collections.Generic;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs.Admin;

namespace GreenBasket.Application.Interfaces
{
    public interface IDiscountService
    {
        Task<List<DiscountDTO>> GetAllAsync();
        Task<DiscountDTO?> GetByIdAsync(int id);
        Task<DiscountDTO?> ValidateCodeAsync(string code);
        Task<DiscountDTO> CreateAsync(CreateDiscountDTO request);
        Task<bool> UpdateAsync(int id, UpdateDiscountDTO request);
        Task<bool> DeleteAsync(int id);
    }
}
