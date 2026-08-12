using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GreenBasket.Application.DTOs.Admin;
using GreenBasket.Application.Interfaces;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;

namespace GreenBasket.Application.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DiscountDTO>> GetAllAsync()
        {
            return await _context.DiscountCodes
                .Select(d => new DiscountDTO
                {
                    Id = d.Id,
                    Code = d.Code,
                    DiscountPercentage = d.DiscountPercentage,
                    MaxDiscountAmount = d.MaxDiscountAmount,
                    ExpiryDate = d.ExpiryDate,
                    IsActive = d.IsActive
                })
                .ToListAsync();
        }

        public async Task<DiscountDTO?> GetByIdAsync(int id)
        {
            var d = await _context.DiscountCodes.FindAsync(id);
            if (d == null) return null;

            return new DiscountDTO
            {
                Id = d.Id,
                Code = d.Code,
                DiscountPercentage = d.DiscountPercentage,
                MaxDiscountAmount = d.MaxDiscountAmount,
                ExpiryDate = d.ExpiryDate,
                IsActive = d.IsActive
            };
        }

        public async Task<DiscountDTO?> ValidateCodeAsync(string code)
        {
            var d = await _context.DiscountCodes
                .FirstOrDefaultAsync(x => x.Code.ToUpper() == code.ToUpper());

            if (d == null || !d.IsActive || d.ExpiryDate < DateTime.UtcNow)
                return null;

            return new DiscountDTO
            {
                Id = d.Id,
                Code = d.Code,
                DiscountPercentage = d.DiscountPercentage,
                MaxDiscountAmount = d.MaxDiscountAmount,
                ExpiryDate = d.ExpiryDate,
                IsActive = d.IsActive
            };
        }

        public async Task<DiscountDTO> CreateAsync(CreateDiscountDTO request)
        {
            // Ensure unique code
            if (await _context.DiscountCodes.AnyAsync(x => x.Code.ToUpper() == request.Code.ToUpper()))
            {
                throw new Exception("Discount code already exists.");
            }

            var discount = new DiscountCode
            {
                Code = request.Code.ToUpper(),
                DiscountPercentage = request.DiscountPercentage,
                MaxDiscountAmount = request.MaxDiscountAmount,
                ExpiryDate = request.ExpiryDate,
                IsActive = request.IsActive
            };

            _context.DiscountCodes.Add(discount);
            await _context.SaveChangesAsync();

            return new DiscountDTO
            {
                Id = discount.Id,
                Code = discount.Code,
                DiscountPercentage = discount.DiscountPercentage,
                MaxDiscountAmount = discount.MaxDiscountAmount,
                ExpiryDate = discount.ExpiryDate,
                IsActive = discount.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateDiscountDTO request)
        {
            var discount = await _context.DiscountCodes.FindAsync(id);
            if (discount == null) return false;

            discount.DiscountPercentage = request.DiscountPercentage;
            discount.MaxDiscountAmount = request.MaxDiscountAmount;
            discount.ExpiryDate = request.ExpiryDate;
            discount.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var discount = await _context.DiscountCodes.FindAsync(id);
            if (discount == null) return false;

            // Check if there are orders using this discount code
            var hasOrders = await _context.Orders.AnyAsync(o => o.DiscountCodeId == id);
            if (hasOrders)
            {
                throw new Exception("Cannot delete this discount code because it has been used in orders.");
            }

            _context.DiscountCodes.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
