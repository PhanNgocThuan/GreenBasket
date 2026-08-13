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
                DeliveryAddress = dto.DeliveryAddress ?? "Saved Delivery Location",
                PaymentMethod = dto.PaymentMethod ?? "Credit Card / Online",
                PaymentStatus = "Pending",
                Status = "Processing"
            };

            foreach (var item in cart.CartItems)
            {
                var product = await _context.Products
                    .Include(p => p.Batches)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                    throw new Exception($"Product {item.ProductId} not found");

                if (product.StockQty < item.Quantity)
                    throw new Exception($"Sản phẩm {product.Name} không đủ số lượng tồn kho");

                decimal remainingToDeduct = item.Quantity;

                var availableBatches = product.Batches
                    .Where(b => b.QuantityRemaining > 0)
                    .OrderBy(b => b.ReceivedDate)
                    .ToList();

                foreach (var batch in availableBatches)
                {
                    if (remainingToDeduct <= 0) break;

                    if (batch.QuantityRemaining >= remainingToDeduct)
                    {
                        batch.QuantityRemaining -= remainingToDeduct;
                        remainingToDeduct = 0;
                    }
                    else
                    {
                        remainingToDeduct -= batch.QuantityRemaining;
                        batch.QuantityRemaining = 0;
                    }
                }

                if (remainingToDeduct > 0)
                    throw new Exception($"Sản phẩm {product.Name} không đủ số lượng tồn kho trong các lô hàng");

                product.StockQty -= item.Quantity;
                product.StockStatus = product.StockQty <= 0 ? StockStatus.OutOfStock : (product.StockQty < 10m ? StockStatus.LowStock : StockStatus.InStock);

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            _context.Orders.Add(order);
            _context.Carts.Remove(cart); // Clear cart after order
            
            DeliverySlot? slot = null;
            if (dto.DeliverySlotId.HasValue)
            {
                slot = await _context.DeliverySlots.FindAsync(dto.DeliverySlotId.Value);
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
                DeliveryAddress = order.DeliveryAddress,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                DeliverySlot = slot?.TimeRange,
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
                .Include(o => o.DeliverySlot)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                AppUserId = o.AppUserId,
                TotalCost = o.TotalCost,
                DiscountAmount = o.DiscountAmount,
                Status = o.Status,
                DeliveryAddress = o.DeliveryAddress,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                DeliverySlot = o.DeliverySlot != null ? o.DeliverySlot.TimeRange : null,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            }).ToList();
        }

        public async Task<System.Collections.Generic.List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.DeliverySlot)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                AppUserId = o.AppUserId,
                TotalCost = o.TotalCost,
                DiscountAmount = o.DiscountAmount,
                Status = o.Status,
                DeliveryAddress = o.DeliveryAddress,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                DeliverySlot = o.DeliverySlot != null ? o.DeliverySlot.TimeRange : null,
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
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.AppUserId == userId);
                
            if (order == null || order.Status == "Shipped" || order.Status == "Delivered" || order.Status == "Cancelled" || order.Status == "Refunded")
                return false;

            order.Status = "Cancelled"; // Handles refund flow if needed later

            await RestoreOrderStockAsync(order);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            // If transitioning to Cancelled or Refunded from a different state, restore stock
            if ((status == "Cancelled" || status == "Refunded") && 
                (order.Status != "Cancelled" && order.Status != "Refunded"))
            {
                await RestoreOrderStockAsync(order);
            }

            order.Status = status; // e.g., "Processing", "Shipped", "Delivered"
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task RestoreOrderStockAsync(Order order)
        {
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products
                    .Include(p => p.Batches)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product != null)
                {
                    decimal remainingToAdd = item.Quantity;

                    var recentBatches = product.Batches
                        .OrderByDescending(b => b.ReceivedDate)
                        .ToList();

                    foreach (var batch in recentBatches)
                    {
                        if (remainingToAdd <= 0) break;

                        decimal spaceInBatch = batch.QuantityReceived - batch.QuantityRemaining;
                        if (spaceInBatch > 0)
                        {
                            if (spaceInBatch >= remainingToAdd)
                            {
                                batch.QuantityRemaining += remainingToAdd;
                                remainingToAdd = 0;
                            }
                            else
                            {
                                batch.QuantityRemaining += spaceInBatch;
                                remainingToAdd -= spaceInBatch;
                            }
                        }
                    }

                    if (remainingToAdd > 0)
                    {
                        var latestBatch = recentBatches.FirstOrDefault();
                        if (latestBatch != null)
                        {
                            latestBatch.QuantityRemaining += remainingToAdd;
                            latestBatch.QuantityReceived += remainingToAdd; // update received to prevent inconsistency
                        }
                    }

                    product.StockQty += item.Quantity;
                    product.StockStatus = product.StockQty <= 0 ? StockStatus.OutOfStock : (product.StockQty < 10m ? StockStatus.LowStock : StockStatus.InStock);
                }
            }
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
