using System;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs.Admin;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests
{
    public class DiscountServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDiscounts()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.AddRange(
                new DiscountCode { Id = 1, Code = "SUMMER10", DiscountPercentage = 10, ExpiryDate = DateTime.UtcNow.AddDays(5), IsActive = true },
                new DiscountCode { Id = 2, Code = "WINTER20", DiscountPercentage = 20, ExpiryDate = DateTime.UtcNow.AddDays(10), IsActive = false }
            );
            await context.SaveChangesAsync();

            var service = new DiscountService(context);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ShouldReturnDiscount()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode { Id = 1, Code = "SALE50", DiscountPercentage = 50, ExpiryDate = DateTime.UtcNow.AddDays(1), IsActive = true });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SALE50", result!.Code);
            Assert.Equal(50, result.DiscountPercentage);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new DiscountService(context);

            // Act
            var result = await service.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateCodeAsync_ValidActiveAndNotExpired_ShouldReturnDiscount()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode
            {
                Id = 1,
                Code = "WELCOME10",
                DiscountPercentage = 10,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsActive = true
            });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);

            // Act (Thử nghiệm tìm kiếm không phân biệt hoa thường)
            var result = await service.ValidateCodeAsync("welcome10");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WELCOME10", result!.Code);
        }

        [Fact]
        public async Task ValidateCodeAsync_WhenNotFound_ShouldReturnNull()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new DiscountService(context);

            // Act
            var result = await service.ValidateCodeAsync("INVALIDCODE");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateCodeAsync_WhenInactive_ShouldReturnNull()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode
            {
                Id = 1,
                Code = "OFF50",
                DiscountPercentage = 50,
                ExpiryDate = DateTime.UtcNow.AddDays(5),
                IsActive = false // Ngưng kích hoạt
            });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);

            // Act
            var result = await service.ValidateCodeAsync("OFF50");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateCodeAsync_WhenExpired_ShouldReturnNull()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode
            {
                Id = 1,
                Code = "EXPIRED20",
                DiscountPercentage = 20,
                ExpiryDate = DateTime.UtcNow.AddDays(-1), // Đã hết hạn
                IsActive = true
            });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);

            // Act
            var result = await service.ValidateCodeAsync("EXPIRED20");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_ShouldCreateAndReturnDTO()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new DiscountService(context);
            var dto = new CreateDiscountDTO
            {
                Code = "newcode",
                DiscountPercentage = 15,
                MaxDiscountAmount = 50000,
                ExpiryDate = DateTime.UtcNow.AddDays(10),
                IsActive = true
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NEWCODE", result.Code); // Đã chuyển thành chữ hoa
            Assert.Equal(1, await context.DiscountCodes.CountAsync());
        }

        [Fact]
        public async Task CreateAsync_DuplicateCode_ShouldThrowException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode { Id = 1, Code = "EXISTING", ExpiryDate = DateTime.UtcNow.AddDays(5), IsActive = true });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);
            var dto = new CreateDiscountDTO { Code = "existing", DiscountPercentage = 10 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(dto));
            Assert.Equal("Discount code already exists.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode { Id = 1, Code = "OLDCODE", DiscountPercentage = 10, ExpiryDate = DateTime.UtcNow.AddDays(1), IsActive = true });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);
            var updateDto = new UpdateDiscountDTO
            {
                DiscountPercentage = 25,
                MaxDiscountAmount = 100000,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsActive = false
            };

            // Act
            var result = await service.UpdateAsync(1, updateDto);

            // Assert
            Assert.True(result);
            var updated = await context.DiscountCodes.FindAsync(1);
            Assert.NotNull(updated);
            Assert.Equal(25, updated!.DiscountPercentage);
            Assert.False(updated.IsActive);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotExists_ShouldReturnFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new DiscountService(context);
            var updateDto = new UpdateDiscountDTO { DiscountPercentage = 20 };

            // Act
            var result = await service.UpdateAsync(99, updateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenExistsAndNotInOrders_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode { Id = 1, Code = "DELETE_ME", ExpiryDate = DateTime.UtcNow.AddDays(1) });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal(0, await context.DiscountCodes.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_WhenNotExists_ShouldReturnFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new DiscountService(context);

            // Act
            var result = await service.DeleteAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenUsedInOrders_ShouldThrowException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.DiscountCodes.Add(new DiscountCode { Id = 1, Code = "USED_CODE", ExpiryDate = DateTime.UtcNow.AddDays(1) });
            context.Orders.Add(new Order { Id = 100, DiscountCodeId = 1, AppUserId = "user1" });
            await context.SaveChangesAsync();

            var service = new DiscountService(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(1));
            Assert.Equal("Cannot delete this discount code because it has been used in orders.", exception.Message);
        }
    }
}