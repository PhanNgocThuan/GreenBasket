using System;
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
    public class CartServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        #region GetCartAsync Tests

        [Fact]
        public async Task GetCartAsync_ShouldReturnEmptyCartDto_WhenCartDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);
            var userId = "user-123";

            // Act
            var result = await service.GetCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.AppUserId.Should().Be(userId);
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCartAsync_ShouldReturnPopulatedCart_WhenCartExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userId = "user-123";

            var product = new Product { Id = 1, Name = "Táo Organic", Price = 50000 };
            var cart = new Cart
            {
                Id = 10,
                AppUserId = userId,
                CartItems = new List<CartItem>
                {
                    new CartItem { Id = 100, ProductId = 1, Quantity = 2, UnitPrice = 50000, Product = product }
                }
            };

            context.Products.Add(product);
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var service = new CartService(context);

            // Act
            var result = await service.GetCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(10);
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductName.Should().Be("Táo Organic");
            result.Items.First().Quantity.Should().Be(2);
        }

        #endregion

        #region AddToCartAsync Tests

        [Fact]
        public async Task AddToCartAsync_ShouldThrowException_WhenProductNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);
            var dto = new AddToCartDto { AppUserId = "user-123", ProductId = 999, Quantity = 1 };

            // Act
            Func<Task> act = async () => await service.AddToCartAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("Product not found");
        }

        [Fact]
        public async Task AddToCartAsync_ShouldCreateNewCartAndAddItem_WhenCartDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var product = new Product { Id = 1, Name = "Rau Cần Tây", Price = 25000 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new CartService(context);
            var dto = new AddToCartDto { AppUserId = "user-123", ProductId = 1, Quantity = 2 };

            // Act
            var result = await service.AddToCartAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductId.Should().Be(1);
            result.Items.First().Quantity.Should().Be(2);
            result.Items.First().UnitPrice.Should().Be(25000);
        }

        [Fact]
        public async Task AddToCartAsync_ShouldAccumulateQuantity_WhenItemAlreadyExistsInCart()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userId = "user-123";
            var product = new Product { Id = 1, Name = "Sữa Tươi", Price = 30000 };
            var cart = new Cart
            {
                Id = 1,
                AppUserId = userId,
                CartItems = new List<CartItem>
                {
                    new CartItem { Id = 10, ProductId = 1, Quantity = 3, UnitPrice = 30000 }
                }
            };

            context.Products.Add(product);
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var service = new CartService(context);
            var dto = new AddToCartDto { AppUserId = userId, ProductId = 1, Quantity = 2 };

            // Act
            var result = await service.AddToCartAsync(dto);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Quantity.Should().Be(5); // 3 + 2 = 5
        }

        #endregion

        #region UpdateCartItemQuantityAsync Tests

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ShouldReturnFalse_WhenItemNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);
            var dto = new UpdateCartItemDto { Quantity = 5 };

            // Act
            var result = await service.UpdateCartItemQuantityAsync(999, dto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ShouldUpdateQuantityAndReturnTrue_WhenItemExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var cartItem = new CartItem { Id = 1, ProductId = 10, Quantity = 1, UnitPrice = 100 };
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();

            var service = new CartService(context);
            var dto = new UpdateCartItemDto { Quantity = 10 };

            // Act
            var result = await service.UpdateCartItemQuantityAsync(1, dto);

            // Assert
            result.Should().BeTrue();
            var updatedItem = await context.CartItems.FindAsync(1);
            updatedItem!.Quantity.Should().Be(10);
        }

        #endregion

        #region RemoveFromCartAsync Tests

        [Fact]
        public async Task RemoveFromCartAsync_ShouldReturnFalse_WhenItemNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);

            // Act
            var result = await service.RemoveFromCartAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveFromCartAsync_ShouldRemoveItemAndReturnTrue_WhenItemExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var cartItem = new CartItem { Id = 1, ProductId = 10, Quantity = 2, UnitPrice = 50 };
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();

            var service = new CartService(context);

            // Act
            var result = await service.RemoveFromCartAsync(1);

            // Assert
            result.Should().BeTrue();
            (await context.CartItems.CountAsync()).Should().Be(0);
        }

        #endregion
    }
}