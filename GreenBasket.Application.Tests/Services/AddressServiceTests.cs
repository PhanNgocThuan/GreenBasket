using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Address;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests.Services
{
    public class AddressServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetUserAddressesAsync_ShouldReturnOnlyUserAddresses()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Addresses.AddRange(
                new Address { Id = 1, UserId = "user1", ReceiverName = "User 1 Addr 1" },
                new Address { Id = 2, UserId = "user1", ReceiverName = "User 1 Addr 2" },
                new Address { Id = 3, UserId = "user2", ReceiverName = "User 2 Addr" }
            );
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.GetUserAddressesAsync("user1");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, item => Assert.True(item.Id == 1 || item.Id == 2));
        }

        [Fact]
        public async Task GetAddressByIdAsync_ShouldReturnAddress_WhenAddressExistsAndBelongsToUser()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Addresses.Add(new Address { Id = 1, UserId = "user1", ReceiverName = "Alice", City = "HCM" });
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.GetAddressByIdAsync(1, "user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Alice", result.ReceiverName);
        }

        [Fact]
        public async Task GetAddressByIdAsync_ShouldReturnNull_WhenUserIdDoesNotMatchOrNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Addresses.Add(new Address { Id = 1, UserId = "user1" });
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.GetAddressByIdAsync(1, "other_user");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAddressAsync_ShouldAddNewAddress_AndResetPreviousDefault_WhenIsDefaultIsTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var existingDefault = new Address { Id = 1, UserId = "user1", IsDefault = true, ReceiverName = "Old Default" };
            context.Addresses.Add(existingDefault);
            await context.SaveChangesAsync();

            var service = new AddressService(context);
            var createDto = new CreateAddressDTO
            {
                ReceiverName = "New Default",
                PhoneNumber = "0901234567",
                StreetAddress = "123 Street",
                City = "HCM",
                District = "D1",
                Ward = "W1",
                IsDefault = true
            };

            // Act
            var result = await service.CreateAddressAsync("user1", createDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDefault);

            // Kiểm tra địa chỉ cũ đã bị bỏ IsDefault
            var updatedOldAddress = await context.Addresses.FindAsync(1);
            Assert.False(updatedOldAddress!.IsDefault);

            // Kiểm tra tổng số địa chỉ
            Assert.Equal(2, await context.Addresses.CountAsync());
        }

        [Fact]
        public async Task UpdateAddressAsync_ShouldUpdateAddress_AndUnsetDefaultOfOtherAddresses_WhenIsDefaultSetToTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Addresses.AddRange(
                new Address { Id = 1, UserId = "user1", IsDefault = true, ReceiverName = "Old Default" },
                new Address { Id = 2, UserId = "user1", IsDefault = false, ReceiverName = "Address To Update" }
            );
            await context.SaveChangesAsync();

            var service = new AddressService(context);
            var updateDto = new UpdateAddressDTO
            {
                ReceiverName = "Updated Name",
                PhoneNumber = "0987654321",
                StreetAddress = "456 Street",
                City = "HN",
                District = "D2",
                Ward = "W2",
                IsDefault = true
            };

            // Act
            var result = await service.UpdateAddressAsync(2, "user1", updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.ReceiverName);
            Assert.True(result.IsDefault);

            var address1 = await context.Addresses.FindAsync(1);
            Assert.False(address1!.IsDefault);
        }

        [Fact]
        public async Task UpdateAddressAsync_ShouldReturnNull_WhenAddressNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new AddressService(context);
            var updateDto = new UpdateAddressDTO { ReceiverName = "Test" };

            // Act
            var result = await service.UpdateAddressAsync(99, "user1", updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAddressAsync_ShouldRemoveAddress_WhenAddressExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Addresses.Add(new Address { Id = 1, UserId = "user1" });
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.DeleteAddressAsync(1, "user1");

            // Assert
            Assert.True(result);
            Assert.Equal(0, await context.Addresses.CountAsync());
        }

        [Fact]
        public async Task DeleteAddressAsync_ShouldReturnFalse_WhenAddressNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new AddressService(context);

            // Act
            var result = await service.DeleteAddressAsync(99, "user1");

            // Assert
            Assert.False(result);
        }
    }
}