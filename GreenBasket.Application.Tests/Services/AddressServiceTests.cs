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

        #region GetUserAddressesAsync Tests

        [Fact]
        public async Task GetUserAddressesAsync_ShouldReturnAddressesOnlyForSpecifiedUser()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var targetUserId = "user-1";
            var otherUserId = "user-2";

            context.Addresses.AddRange(
                new Address { Id = 1, UserId = targetUserId, ReceiverName = "Địa chỉ 1", PhoneNumber = "0123456789", StreetAddress = "Đường 1", City = "HCM", District = "Q1", Ward = "P1" },
                new Address { Id = 2, UserId = targetUserId, ReceiverName = "Địa chỉ 2", PhoneNumber = "0123456789", StreetAddress = "Đường 2", City = "HCM", District = "Q1", Ward = "P1" },
                new Address { Id = 3, UserId = otherUserId, ReceiverName = "Địa chỉ User khác", PhoneNumber = "0987654321", StreetAddress = "Đường 3", City = "HN", District = "HK", Ward = "P2" }
            );
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.GetUserAddressesAsync(targetUserId);

            // Assert
            result.Should().HaveCount(2);
            result.Select(a => a.Id).Should().Contain(new[] { 1, 2 });
        }

        #endregion

        #region GetAddressByIdAsync Tests

        [Fact]
        public async Task GetAddressByIdAsync_ShouldReturnAddress_WhenIdAndUserIdMatch()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userId = "user-123";
            var address = new Address
            {
                Id = 1,
                UserId = userId,
                ReceiverName = "Nguyen Van A",
                PhoneNumber = "0901234567",
                StreetAddress = "123 Street",
                City = "HCM",
                District = "District 1",
                Ward = "Ward 1"
            };
            context.Addresses.Add(address);
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.GetAddressByIdAsync(1, userId);

            // Assert
            result.Should().NotBeNull();
            result.ReceiverName.Should().Be("Nguyen Van A");
        }

        [Fact]
        public async Task GetAddressByIdAsync_ShouldReturnNull_WhenAddressBelongsToAnotherUser()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Addresses.Add(new Address { Id = 1, UserId = "user-owner", ReceiverName = "Owner" });
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.GetAddressByIdAsync(1, "other-user");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateAddressAsync Tests

        [Fact]
        public async Task CreateAddressAsync_ShouldUnsetDefaultAddresses_WhenNewAddressIsDefault()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userId = "user-123";

            // Địa chỉ mặc định cũ
            var existingAddress = new Address
            {
                Id = 1,
                UserId = userId,
                IsDefault = true,
                ReceiverName = "Địa chỉ cũ"
            };
            context.Addresses.Add(existingAddress);
            await context.SaveChangesAsync();

            var newAddressDto = new CreateAddressDTO
            {
                ReceiverName = "Địa chỉ mới",
                PhoneNumber = "0900000000",
                StreetAddress = "Đường mới",
                City = "City",
                District = "District",
                Ward = "Ward",
                IsDefault = true
            };

            var service = new AddressService(context);

            // Act
            var result = await service.CreateAddressAsync(userId, newAddressDto);

            // Assert
            result.Should().NotBeNull();
            result.IsDefault.Should().BeTrue();

            // Kiểm tra địa chỉ cũ đã bị hủy trạng thái IsDefault
            var oldAddressInDb = await context.Addresses.FindAsync(1);
            oldAddressInDb!.IsDefault.Should().BeFalse();
        }

        #endregion

        #region UpdateAddressAsync Tests

        [Fact]
        public async Task UpdateAddressAsync_ShouldUpdateAndUnsetOtherDefaults_WhenSetToDefault()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userId = "user-123";

            var addr1 = new Address { Id = 1, UserId = userId, IsDefault = true, ReceiverName = "Địa chỉ 1" };
            var addr2 = new Address { Id = 2, UserId = userId, IsDefault = false, ReceiverName = "Địa chỉ 2" };
            context.Addresses.AddRange(addr1, addr2);
            await context.SaveChangesAsync();

            var updateDto = new UpdateAddressDTO
            {
                ReceiverName = "Địa chỉ 2 Đã cập nhật",
                PhoneNumber = "0911111111",
                StreetAddress = "Street 2",
                City = "City",
                District = "District",
                Ward = "Ward",
                IsDefault = true // Đặt addr2 làm mặc định
            };

            var service = new AddressService(context);

            // Act
            var result = await service.UpdateAddressAsync(2, userId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsDefault.Should().BeTrue();

            // Kiểm tra addr1 không còn là mặc định
            var addr1InDb = await context.Addresses.FindAsync(1);
            addr1InDb!.IsDefault.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAddressAsync_ShouldReturnNull_WhenAddressNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new AddressService(context);
            var updateDto = new UpdateAddressDTO { ReceiverName = "Test" };

            // Act
            var result = await service.UpdateAddressAsync(999, "user-123", updateDto);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteAddressAsync Tests

        [Fact]
        public async Task DeleteAddressAsync_ShouldRemoveAddressAndReturnTrue_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userId = "user-123";
            context.Addresses.Add(new Address { Id = 1, UserId = userId, ReceiverName = "Cần xóa" });
            await context.SaveChangesAsync();

            var service = new AddressService(context);

            // Act
            var result = await service.DeleteAddressAsync(1, userId);

            // Assert
            result.Should().BeTrue();
            (await context.Addresses.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeleteAddressAsync_ShouldReturnFalse_WhenNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new AddressService(context);

            // Act
            var result = await service.DeleteAddressAsync(999, "user-123");

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}