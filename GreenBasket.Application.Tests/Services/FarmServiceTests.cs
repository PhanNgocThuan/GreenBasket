using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests
{
    public class FarmServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnFarmsOrderedByName()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Farms.AddRange(
                new Farm { Id = 1, Name = "Zebra Farm", Location = "Dalat" },
                new Farm { Id = 2, Name = "Alpha Farm", Location = "Lao Cai" }
            );
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Alpha Farm", result[0].Name); // Kiểm tra xem có sắp xếp theo Tên không
            Assert.Equal("Zebra Farm", result[1].Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ShouldReturnFarmDto()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Farms.Add(new Farm { Id = 1, Name = "Green Farm", Location = "Lam Dong", ContactInfo = "0123456789" });
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal("Green Farm", result.Name);
            Assert.Equal("Lam Dong", result.Location);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);

            // Act
            var result = await service.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_ShouldCreateAndReturnFarmDto()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);
            var request = new CreateFarmRequest
            {
                Name = "Organic Farm",
                Location = "Ben Tre",
                ContactInfo = "contact@organic.com"
            };

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Organic Farm", result.Name);
            Assert.Equal("Ben Tre", result.Location);
            Assert.Equal(1, await context.Farms.CountAsync());
        }

        [Fact]
        public async Task UpdateAsync_WhenExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Farms.Add(new Farm { Id = 1, Name = "Old Farm", Location = "Old Location", ContactInfo = "000" });
            await context.SaveChangesAsync();

            var service = new FarmService(context);
            var updateRequest = new UpdateFarmRequest
            {
                Name = "New Farm",
                Location = "New Location",
                ContactInfo = "111"
            };

            // Act
            var result = await service.UpdateAsync(1, updateRequest);

            // Assert
            Assert.True(result);
            var updated = await context.Farms.FindAsync(1);
            Assert.NotNull(updated);
            Assert.Equal("New Farm", updated!.Name);
            Assert.Equal("New Location", updated.Location);
            Assert.Equal("111", updated.ContactInfo);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotExists_ShouldReturnFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);
            var updateRequest = new UpdateFarmRequest { Name = "New Farm" };

            // Act
            var result = await service.UpdateAsync(99, updateRequest);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenExistsAndHasNoBatches_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Farms.Add(new Farm { Id = 1, Name = "Farm To Delete" });
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal(0, await context.Farms.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_WhenNotExists_ShouldReturnFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);

            // Act
            var result = await service.DeleteAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenHasBatches_ShouldReturnFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm
            {
                Id = 1,
                Name = "Farm With Batches",
                Batches = new List<Batch>
                {
                    new Batch { Id = 10 }
                }
            };
            context.Farms.Add(farm);
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.False(result); // Phải trả về false do Nông trại đã có Batch
            Assert.Equal(1, await context.Farms.CountAsync()); // Dữ liệu không bị xóa
        }
    }
}