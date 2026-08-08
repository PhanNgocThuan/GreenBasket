using GreenBasket.Application.DTOs.Products;

namespace GreenBasket.Application.Interfaces
{
    public interface IFarmService
    {
        Task<List<FarmDto>> GetAllAsync();
        Task<FarmDto?> GetByIdAsync(int id);
        Task<FarmDto> CreateAsync(CreateFarmRequest request);
        Task<bool> UpdateAsync(int id, UpdateFarmRequest request);
        Task<bool> DeleteAsync(int id);
    }
}