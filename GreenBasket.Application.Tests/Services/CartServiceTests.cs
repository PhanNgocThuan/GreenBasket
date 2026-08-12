using System;
using System.Linq;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests
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

        [Fact]
        public async Task GetCartAsync_CartDoesNotExist_ReturnsEmptyCartDto()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);

            // Act
            var result = await service.GetCartAsync("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1", result.AppUserId);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetCartAsync_CartExists_ReturnsMappedCartDtoWithItems()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var product = new Product { Id = 10, Name = "Táo Organic", Price = 50000 };
            var cart = new Cart
            {
                Id = 1,
                AppUserId = "user1",
                CartItems = new List<CartItem>
                {
                    new CartItem { Id = 100, ProductId = 10, Product = product, Quantity = 2, UnitPrice = 50000 }
                }
            };
            context.Products.Add(product);
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var service = new CartService(context);

            // Act
            var result = await service.GetCartAsync("user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.Items);
            Assert.Single(result.Items);
            Assert.Equal("Táo Organic", result.Items.First().ProductName);
            Assert.Equal(2, result.Items.First().Quantity);
        }

        [Fact]
        public async Task AddToCartAsync_ProductNotFound_ThrowsException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);
            var dto = new AddToCartDto { AppUserId = "user1", ProductId = 99, Quantity = 1 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => service.AddToCartAsync(dto));
            Assert.Equal("Product not found", exception.Message);
        }

        [Fact]
        public async Task AddToCartAsync_CartDoesNotExist_CreatesCartAndAddsItem()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Cam Sành", Price = 30000 });
            await context.SaveChangesAsync();

            var service = new CartService(context);
            var dto = new AddToCartDto { AppUserId = "user1", ProductId = 1, Quantity = 3 };

            // Act
            var result = await service.AddToCartAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1", result.AppUserId);
            Assert.NotNull(result.Items);
            Assert.Single(result.Items);
            Assert.Equal(3, result.Items.First().Quantity);
            Assert.Equal(30000, result.Items.First().UnitPrice);
        }

        [Fact]
        public async Task AddToCartAsync_ItemAlreadyInCart_IncrementsQuantity()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var product = new Product { Id = 1, Name = "Dưa Hấu", Price = 20000 };
            var cart = new Cart { Id = 1, AppUserId = "user1" };
            cart.CartItems.Add(new CartItem { Id = 10, ProductId = 1, Quantity = 2, UnitPrice = 20000 });

            context.Products.Add(product);
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var service = new CartService(context);
            var dto = new AddToCartDto { AppUserId = "user1", ProductId = 1, Quantity = 3 };

            // Act
            var result = await service.AddToCartAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Single(result.Items);
            Assert.Equal(5, result.Items.First().Quantity); // 2 + 3 = 5
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ItemNotFound_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);

            // Act
            var result = await service.UpdateCartItemQuantityAsync(99, new UpdateCartItemDto { Quantity = 5 });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ItemExists_UpdatesQuantityAndReturnsTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.CartItems.Add(new CartItem { Id = 100, ProductId = 1, Quantity = 2, UnitPrice = 10000 });
            await context.SaveChangesAsync();

            var service = new CartService(context);

            // Act
            var result = await service.UpdateCartItemQuantityAsync(100, new UpdateCartItemDto { Quantity = 10 });

            // Assert
            Assert.True(result);
            var updatedItem = await context.CartItems.FindAsync(100);
            Assert.NotNull(updatedItem);
            Assert.Equal(10, updatedItem.Quantity);
        }

        [Fact]
        public async Task RemoveFromCartAsync_ItemNotFound_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new CartService(context);

            // Act
            var result = await service.RemoveFromCartAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveFromCartAsync_ItemExists_RemovesItemAndReturnsTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.CartItems.Add(new CartItem { Id = 100, ProductId = 1, Quantity = 1, UnitPrice = 10000 });
            await context.SaveChangesAsync();

            var service = new CartService(context);

            // Act
            var result = await service.RemoveFromCartAsync(100);

            // Assert
            Assert.True(result);
            Assert.Equal(0, await context.CartItems.CountAsync());
        }
    }
}