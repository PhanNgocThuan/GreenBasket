using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests
{
    public class OrderServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        #region CalculateTotalCostAsync

        [Fact]
        public async Task CalculateTotalCostAsync_CartNotFoundOrEmpty_ReturnsZero()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new OrderService(context);

            // Act
            var result = await service.CalculateTotalCostAsync(new CalculateCostDto { AppUserId = "user1" });

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task CalculateTotalCostAsync_WithoutDiscount_ReturnsFullTotal()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var cart = new Cart
            {
                AppUserId = "user1",
                CartItems = new List<CartItem>
                {
                    new CartItem { ProductId = 1, Quantity = 2, UnitPrice = 50000m }
                }
            };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.CalculateTotalCostAsync(new CalculateCostDto { AppUserId = "user1" });

            // Assert
            Assert.Equal(100000m, result);
        }

        [Fact]
        public async Task CalculateTotalCostAsync_WithValidDiscountAndMaxAmountCap_AppliesCap()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var cart = new Cart
            {
                AppUserId = "user1",
                CartItems = new List<CartItem>
                {
                    new CartItem { ProductId = 1, Quantity = 2, UnitPrice = 100000m } // Tổng: 200,000
                }
            };
            var discount = new DiscountCode
            {
                Id = 1,
                Code = "SAVE50",
                DiscountPercentage = 50, // 50% = 100,000
                MaxDiscountAmount = 30000m, // Cố định giảm tối đa 30,000
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };
            context.Carts.Add(cart);
            context.DiscountCodes.Add(discount);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.CalculateTotalCostAsync(new CalculateCostDto { AppUserId = "user1", DiscountCode = "SAVE50" });

            // Assert (200,000 - 30,000 = 170,000)
            Assert.Equal(170000m, result);
        }

        #endregion

        #region CreateOrderAsync

        [Fact]
        public async Task CreateOrderAsync_CartIsEmpty_ThrowsException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new OrderService(context);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.CreateOrderAsync(new CreateOrderDto { AppUserId = "user1" }));

            Assert.Equal("Cart is empty", ex.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequest_CreatesOrderAndClearsCart()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var product = new Product
            {
                Id = 10,
                Name = "Mock Product",
                Price = 50000m,
                StockQty = 5m,
                Batches = new List<Batch>
                {
                    new Batch { Id = 101, QuantityReceived = 10m, QuantityRemaining = 10m, CostPrice = 30000m, ReceivedDate = DateTime.UtcNow, Farm = new Farm { Id = 201, Name = "Test Farm" } }
                }
            };
            var cart = new Cart
            {
                AppUserId = "user1",
                CartItems = new List<CartItem>
                {
                    new CartItem { ProductId = 10, Quantity = 2, UnitPrice = 50000m }
                }
            };
            var slot = new DeliverySlot { Id = 1, TimeRange = "08:00 - 10:00", CurrentOrders = 0 };
            context.Products.Add(product);
            context.Carts.Add(cart);
            context.DeliverySlots.Add(slot);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var dto = new CreateOrderDto
            {
                AppUserId = "user1",
                DeliverySlotId = 1,
                DeliveryAddress = "123 Main St",
                PaymentMethod = "COD"
            };

            // Act
            var result = await service.CreateOrderAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100000m, result.TotalCost);
            Assert.Equal("123 Main St", result.DeliveryAddress);
            Assert.Equal(0, await context.Carts.CountAsync()); // Giỏ hàng bị xóa
            Assert.Equal(1, (await context.DeliverySlots.FindAsync(1))!.CurrentOrders); // Số đơn slot tăng
        }

        #endregion

        #region GetUserOrdersAsync & GetAllOrdersAsync

        [Fact]
        public async Task GetUserOrdersAsync_ReturnsOnlyUserOrders()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Orders.AddRange(
                new Order { Id = 1, AppUserId = "user1", CreatedAt = DateTime.UtcNow },
                new Order { Id = 2, AppUserId = "user2", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.GetUserOrdersAsync("user1");

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsAllOrdersOrderedByCreatedAtDescending()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Orders.AddRange(
                new Order { Id = 1, AppUserId = "user1", CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new Order { Id = 2, AppUserId = "user2", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.GetAllOrdersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id); // Đơn mới hơn xếp trước
        }

        #endregion

        #region CancelOrderAsync

        [Fact]
        public async Task CancelOrderAsync_ShippedOrDelivered_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Orders.Add(new Order { Id = 1, AppUserId = "user1", Status = "Shipped" });
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.CancelOrderAsync(1, "user1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CancelOrderAsync_Processing_UpdatesStatusToCancelledAndReturnsTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Orders.Add(new Order { Id = 1, AppUserId = "user1", Status = "Processing" });
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.CancelOrderAsync(1, "user1");

            // Assert
            Assert.True(result);
            var order = await context.Orders.FindAsync(1);
            Assert.Equal("Cancelled", order!.Status);
        }

        #endregion

        #region UpdateOrderStatusAsync & ReportDamagedGoodsAsync

        [Fact]
        public async Task UpdateOrderStatusAsync_OrderExists_UpdatesStatusAndReturnsTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Orders.Add(new Order { Id = 1, Status = "Processing" });
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.UpdateOrderStatusAsync(1, "Delivered");

            // Assert
            Assert.True(result);
            var order = await context.Orders.FindAsync(1);
            Assert.Equal("Delivered", order!.Status);
        }

        [Fact]
        public async Task ReportDamagedGoodsAsync_DeliveredOrder_UpdatesStatusToIssueReported()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Orders.Add(new Order { Id = 1, Status = "Delivered" });
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.ReportDamagedGoodsAsync(1, "Hàng hỏng do vận chuyển");

            // Assert
            Assert.True(result);
            var order = await context.Orders.FindAsync(1);
            Assert.Equal("Issue Reported", order!.Status);
        }

        [Fact]
        public async Task ReportDamagedGoodsAsync_NotDeliveredOrder_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Orders.Add(new Order { Id = 1, Status = "Processing" });
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.ReportDamagedGoodsAsync(1, "Hàng hỏng do vận chuyển");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}