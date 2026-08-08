using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Interfaces;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GreenBasket.Application.Services
{
    public class FarmService : IFarmService
    {
        private readonly ApplicationDbContext _context;

        public FarmService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FarmDto>> GetAllAsync()
        {
            return await _context.Farms
                .OrderBy(f => f.Name)
                .Select(f => MapToDto(f))
                .ToListAsync();
        }

        public async Task<FarmDto?> GetByIdAsync(int id)
        {
            var farm = await _context.Farms.FindAsync(id);
            return farm == null ? null : MapToDto(farm);
        }

        public async Task<FarmDto> CreateAsync(CreateFarmRequest request)
        {
            var farm = new Farm
            {
                Name = request.Name,
                Location = request.Location,
                ContactInfo = request.ContactInfo
            };

            _context.Farms.Add(farm);
            await _context.SaveChangesAsync();

            return MapToDto(farm);
        }

        public async Task<bool> UpdateAsync(int id, UpdateFarmRequest request)
        {
            var farm = await _context.Farms.FindAsync(id);
            if (farm == null) return false;

            farm.Name = request.Name;
            farm.Location = request.Location;
            farm.ContactInfo = request.ContactInfo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var farm = await _context.Farms
                .Include(f => f.Batches)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (farm == null) return false;

            // Farm đã có batch (đã từng nhập hàng) → không cho xóa cứng,
            // vì Batch.FarmId là Restrict, xóa sẽ ném DbUpdateException giữa chừng.
            if (farm.Batches.Any())
            {
                return false;
            }

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();
            return true;
        }

        private static FarmDto MapToDto(Farm f) => new()
        {
            Id = f.Id,
            Name = f.Name,
            Location = f.Location,
            ContactInfo = f.ContactInfo
        };
    }
}