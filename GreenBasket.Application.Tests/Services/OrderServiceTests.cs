using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests.Services
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

        #region CalculateTotalCostAsync Tests

        [Fact]
        public async Task CalculateTotalCostAsync_ShouldReturnZero_WhenCartIsEmptyOrNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new OrderService(context);
            var dto = new CalculateCostDto { AppUserId = "user-1" };

            // Act
            var result = await service.CalculateTotalCostAsync(dto);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public async Task CalculateTotalCostAsync_ShouldCalculateCorrectTotal_WithoutDiscount()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var cart = new Cart
            {
                Id = 1,
                AppUserId = "user-1",
                CartItems = new List<CartItem>
                {
                    new CartItem { Id = 1, ProductId = 10, Quantity = 2, UnitPrice = 50000 }, // 100k
                    new CartItem { Id = 2, ProductId = 11, Quantity = 1, UnitPrice = 30000 }  // 30k
                }
            };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var dto = new CalculateCostDto { AppUserId = "user-1" };

            // Act
            var result = await service.CalculateTotalCostAsync(dto);

            // Assert
            result.Should().Be(130000m);
        }

        [Fact]
        public async Task CalculateTotalCostAsync_ShouldApplyDiscountAndCapMaxAmount()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var cart = new Cart
            {
                Id = 1,
                AppUserId = "user-1",
                CartItems = new List<CartItem>
                {
                    new CartItem { Id = 1, ProductId = 10, Quantity = 2, UnitPrice = 100000 } // 200k
                }
            };
            var discount = new DiscountCode
            {
                Id = 1,
                Code = "SALE20",
                IsActive = true,
                DiscountPercentage = 20, // 20% of 200k = 40k
                MaxDiscountAmount = 30000, // Capped at 30k
                ExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            context.Carts.Add(cart);
            context.DiscountCodes.Add(discount);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var dto = new CalculateCostDto { AppUserId = "user-1", DiscountCode = "SALE20" };

            // Act
            var result = await service.CalculateTotalCostAsync(dto);

            // Assert
            result.Should().Be(170000m); // 200k - 30k
        }

        #endregion

        #region CreateOrderAsync Tests

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowException_WhenCartIsEmpty()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new OrderService(context);
            var dto = new CreateOrderDto { AppUserId = "user-1" };

            // Act
            Func<Task> act = async () => await service.CreateOrderAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("Cart is empty");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrderRemoveCartAndIncrementSlot_WhenValid()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userId = "user-1";

            var cart = new Cart
            {
                Id = 1,
                AppUserId = userId,
                CartItems = new List<CartItem>
                {
                    new CartItem { Id = 10, ProductId = 1, Quantity = 2, UnitPrice = 50000 }
                }
            };
            var deliverySlot = new DeliverySlot { Id = 5, CurrentOrders = 2 };

            context.Carts.Add(cart);
            context.DeliverySlots.Add(deliverySlot);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var dto = new CreateOrderDto
            {
                AppUserId = userId,
                DeliverySlotId = 5
            };

            // Act
            var result = await service.CreateOrderAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.TotalCost.Should().Be(100000m);
            result.Status.Should().Be("Pending");
            result.Items.Should().HaveCount(1);

            // Kiểm tra giỏ hàng đã bị xoá
            (await context.Carts.AnyAsync(c => c.AppUserId == userId)).Should().BeFalse();

            // Kiểm tra khung giờ giao hàng tăng lượt order
            var slotInDb = await context.DeliverySlots.FindAsync(5);
            slotInDb!.CurrentOrders.Should().Be(3);
        }

        #endregion

        #region CancelOrderAsync Tests

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderNotFoundOrAlreadyShipped()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var order = new Order { Id = 1, AppUserId = "user-1", Status = "Shipped" };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var resultNotFound = await service.CancelOrderAsync(999, "user-1");
            var resultShipped = await service.CancelOrderAsync(1, "user-1");

            // Assert
            resultNotFound.Should().BeFalse();
            resultShipped.Should().BeFalse();
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldUpdateStatusToCancelled_WhenPending()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var order = new Order { Id = 1, AppUserId = "user-1", Status = "Pending" };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.CancelOrderAsync(1, "user-1");

            // Assert
            result.Should().BeTrue();
            var updatedOrder = await context.Orders.FindAsync(1);
            updatedOrder!.Status.Should().Be("Cancelled");
        }

        #endregion

        #region UpdateOrderStatusAsync Tests

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new OrderService(context);

            // Act
            var result = await service.UpdateOrderStatusAsync(999, "Shipped");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldUpdateStatusAndReturnTrue_WhenOrderExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var order = new Order { Id = 1, Status = "Pending" };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.UpdateOrderStatusAsync(1, "Shipped");

            // Assert
            result.Should().BeTrue();
            var updatedOrder = await context.Orders.FindAsync(1);
            updatedOrder!.Status.Should().Be("Shipped");
        }

        #endregion

        #region ReportDamagedGoodsAsync Tests

        [Fact]
        public async Task ReportDamagedGoodsAsync_ShouldReturnFalse_WhenStatusIsNotDelivered()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var order = new Order { Id = 1, Status = "Pending" };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.ReportDamagedGoodsAsync(1, "Hàng bị hỏng");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ReportDamagedGoodsAsync_ShouldUpdateStatusToIssueReported_WhenDelivered()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var order = new Order { Id = 1, Status = "Delivered" };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var service = new OrderService(context);

            // Act
            var result = await service.ReportDamagedGoodsAsync(1, "Sản phẩm bị dập nát");

            // Assert
            result.Should().BeTrue();
            var updatedOrder = await context.Orders.FindAsync(1);
            updatedOrder!.Status.Should().Be("Issue Reported");
        }

        #endregion
    }
}
