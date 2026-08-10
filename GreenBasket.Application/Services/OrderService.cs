using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GreenBasket.Domain.Entities;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Interfaces;
using GreenBasket.Infrastructure.Data;

namespace GreenBasket.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateTotalCostAsync(CalculateCostDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.AppUserId == dto.AppUserId);

            if (cart == null || !cart.CartItems.Any()) return 0m;

            decimal total = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);

            if (!string.IsNullOrEmpty(dto.DiscountCode))
            {
                var discount = await _context.DiscountCodes
                    .FirstOrDefaultAsync(d => d.Code == dto.DiscountCode && d.IsActive && d.ExpiryDate > DateTime.UtcNow);

                if (discount != null)
                {
                    decimal discountAmount = total * (discount.DiscountPercentage / 100m);
                    if (discount.MaxDiscountAmount.HasValue && discountAmount > discount.MaxDiscountAmount.Value)
                    {
                        discountAmount = discount.MaxDiscountAmount.Value;
                    }
                    total -= discountAmount;
                }
            }

            return total > 0 ? total : 0;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.AppUserId == dto.AppUserId);

            if (cart == null || !cart.CartItems.Any())
                throw new Exception("Cart is empty");

            decimal totalCost = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);
            decimal discountAmount = 0m;

            DiscountCode? discount = null;
            if (dto.DiscountCodeId.HasValue)
            {
                discount = await _context.DiscountCodes.FindAsync(dto.DiscountCodeId.Value);
                if (discount != null && discount.IsActive && discount.ExpiryDate > DateTime.UtcNow)
                {
                    discountAmount = totalCost * (discount.DiscountPercentage / 100m);
                    if (discount.MaxDiscountAmount.HasValue && discountAmount > discount.MaxDiscountAmount.Value)
                    {
                        discountAmount = discount.MaxDiscountAmount.Value;
                    }
                }
            }

            var order = new Order
            {
                AppUserId = dto.AppUserId,
                CreatedAt = DateTime.UtcNow,
                TotalCost = totalCost - discountAmount,
                DiscountAmount = discountAmount,
                DiscountCodeId = discount?.Id,
                DeliverySlotId = dto.DeliverySlotId,
                Status = "Pending"
            };

            foreach (var item in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            _context.Orders.Add(order);
            _context.Carts.Remove(cart); // Clear cart after order
            
            if (dto.DeliverySlotId.HasValue)
            {
                var slot = await _context.DeliverySlots.FindAsync(dto.DeliverySlotId.Value);
                if (slot != null) slot.CurrentOrders++;
            }

            await _context.SaveChangesAsync();

            return new OrderDto
            {
                Id = order.Id,
                AppUserId = order.AppUserId,
                TotalCost = order.TotalCost,
                DiscountAmount = order.DiscountAmount,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        public async Task<System.Collections.Generic.List<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.AppUserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                AppUserId = o.AppUserId,
                TotalCost = o.TotalCost,
                DiscountAmount = o.DiscountAmount,
                Status = o.Status,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            }).ToList();
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.AppUserId == userId);
            if (order == null || order.Status == "Shipped" || order.Status == "Delivered")
                return false;

            order.Status = "Cancelled"; // Handles refund flow if needed later
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status; // e.g., "Processing", "Shipped", "Delivered"
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReportDamagedGoodsAsync(int orderId, string reportDetails)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status != "Delivered") return false;

            // In a real app, this would save to a Reports table. 
            // For now, we change status to denote issues.
            order.Status = "Issue Reported";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
