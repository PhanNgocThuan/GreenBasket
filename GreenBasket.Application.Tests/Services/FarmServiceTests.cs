using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests.Services
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

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnFarmsOrderedByName()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Farms.AddRange(
                new Farm { Id = 1, Name = "Nông trại B", Location = "Lâm Đồng", ContactInfo = "0123" },
                new Farm { Id = 2, Name = "Nông trại A", Location = "Đà Lạt", ContactInfo = "0456" }
            );
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Nông trại A");
            result.Last().Name.Should().Be("Nông trại B");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFarmDto_WhenFarmExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Farms.Add(new Farm { Id = 1, Name = "Nông trại X", Location = "Củ Chi", ContactInfo = "0909" });
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Nông trại X");
            result.Location.Should().Be("Củ Chi");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenFarmDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldAddFarmToDatabaseAndReturnDto()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);
            var request = new CreateFarmRequest
            {
                Name = "Nông trại Xanh",
                Location = "Mộc Châu",
                ContactInfo = "contact@farm.com"
            };

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Nông trại Xanh");

            var farmInDb = await context.Farms.FindAsync(result.Id);
            farmInDb.Should().NotBeNull();
            farmInDb!.Name.Should().Be("Nông trại Xanh");
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenFarmDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);
            var request = new UpdateFarmRequest { Name = "New Name", Location = "Loc", ContactInfo = "000" };

            // Act
            var result = await service.UpdateAsync(999, request);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFarmAndReturnTrue_WhenFarmExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Farms.Add(new Farm { Id = 1, Name = "Tên cũ", Location = "Vị trí cũ", ContactInfo = "0111" });
            await context.SaveChangesAsync();

            var service = new FarmService(context);
            var request = new UpdateFarmRequest { Name = "Tên mới", Location = "Vị trí mới", ContactInfo = "0999" };

            // Act
            var result = await service.UpdateAsync(1, request);

            // Assert
            result.Should().BeTrue();
            var farmInDb = await context.Farms.FindAsync(1);
            farmInDb!.Name.Should().Be("Tên mới");
            farmInDb.Location.Should().Be("Vị trí mới");
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenFarmDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new FarmService(context);

            // Act
            var result = await service.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenFarmHasBatches()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm
            {
                Id = 1,
                Name = "Nông trại có lô hàng",
                Location = "Lâm Đồng",
                ContactInfo = "123",
                Batches = new List<Batch>
                {
                    new Batch
                    {
                        Id = 1,
                        ProductId = 1,
                        QuantityReceived = 100,
                        QuantityRemaining = 100,
                        CostPrice = 50000
                    }
                }
            };
            context.Farms.Add(farm);
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            result.Should().BeFalse();
            (await context.Farms.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveFarmAndReturnTrue_WhenFarmHasNoBatches()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm
            {
                Id = 1,
                Name = "Nông trại trống",
                Location = "Lâm Đồng",
                ContactInfo = "123",
                Batches = new List<Batch>()
            };
            context.Farms.Add(farm);
            await context.SaveChangesAsync();

            var service = new FarmService(context);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
            (await context.Farms.CountAsync()).Should().Be(0);
        }

        #endregion
    }
}