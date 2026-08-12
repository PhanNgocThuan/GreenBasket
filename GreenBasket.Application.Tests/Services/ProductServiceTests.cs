using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests
{
    public class ProductServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        #region SearchAsync

        [Fact]
        public async Task SearchAsync_FiltersByKeywordAndExcludesInactiveProducts()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, Name = "Dalat Farm" };

            context.Products.AddRange(
                new Product { Id = 1, Name = "Táo Red", Description = "Ngon", IsActive = true, Price = 10 },
                new Product { Id = 2, Name = "Cam Sành", Description = "Mọng nước", IsActive = true, Price = 20 },
                new Product { Id = 3, Name = "Táo Bị Ẩn", Description = "Ngon", IsActive = false, Price = 10 }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var (items, totalCount) = await service.SearchAsync("Táo", null, null, null, null, null, null, 1, 10);

            // Assert
            Assert.Equal(1, totalCount);
            Assert.Single(items);
            Assert.Equal("Táo Red", items[0].Name);
        }

        [Fact]
        public async Task SearchAsync_FiltersByPriceRangeAndSorting()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.AddRange(
                new Product { Id = 1, Name = "SP A", Price = 50000m, IsActive = true },
                new Product { Id = 2, Name = "SP B", Price = 20000m, IsActive = true },
                new Product { Id = 3, Name = "SP C", Price = 80000m, IsActive = true }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act (Lọc từ 20k-60k, sắp xếp giảm dần)
            var (items, totalCount) = await service.SearchAsync(null, 20000m, 60000m, null, null, null, "price-desc", 1, 10);

            // Assert
            Assert.Equal(2, totalCount);
            Assert.Equal("SP A", items[0].Name); // 50,000 xếp trước 20,000
            Assert.Equal("SP B", items[1].Name);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WhenActive_ReturnsProductDetailWithOnlyAvailableBatches()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, Name = "Nông trại Xanh" };
            var product = new Product
            {
                Id = 1,
                Name = "Cà chua",
                IsActive = true,
                Batches = new List<Batch>
                {
                    new Batch { Id = 10, Farm = farm, QuantityRemaining = 5, HarvestDate = DateTime.UtcNow },
                    new Batch { Id = 11, Farm = farm, QuantityRemaining = 0, HarvestDate = DateTime.UtcNow.AddDays(-1) } // Hết hàng
                }
            };
            context.Farms.Add(farm);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cà chua", result!.Name);
            Assert.Single(result.Batches); // Chỉ lấy batch còn hàng (QuantityRemaining > 0)
            Assert.Equal(10, result.Batches[0].Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenInactiveOrNotFound_ReturnsNull()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "SP Ẩn", IsActive = false });
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var resultInactive = await service.GetByIdAsync(1);
            var resultNotFound = await service.GetByIdAsync(99);

            // Assert
            Assert.Null(resultInactive);
            Assert.Null(resultNotFound);
        }

        #endregion

        #region CreateAsync & UpdateAsync & DeleteAsync

        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesProductWithDefaultStockStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new ProductService(context);
            var request = new CreateProductRequest
            {
                Name = "Dưa Lưới",
                Price = 120000m,
                Unit = "Kg",
                Organic = true
            };

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Dưa Lưới", result.Name);
            Assert.Equal(0, result.StockQty);
            Assert.Equal(StockStatus.OutOfStock.ToString(), result.StockStatus);

            var inDb = await context.Products.FindAsync(result.Id);
            Assert.NotNull(inDb);
            Assert.True(inDb!.IsActive);
        }

        [Fact]
        public async Task UpdateAsync_WhenExists_UpdatesProductDetails()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Táo Cũ", Price = 10000m });
            await context.SaveChangesAsync();

            var service = new ProductService(context);
            var updateRequest = new UpdateProductRequest { Name = "Táo Mới", Price = 15000m };

            // Act
            var result = await service.UpdateAsync(1, updateRequest);

            // Assert
            Assert.True(result);
            var updated = await context.Products.FindAsync(1);
            Assert.Equal("Táo Mới", updated!.Name);
            Assert.Equal(15000m, updated.Price);
        }

        [Fact]
        public async Task DeleteAsync_WhenExists_PerformsSoftDelete()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Sản phẩm A", IsActive = true });
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.True(result);
            var deleted = await context.Products.FindAsync(1);
            Assert.NotNull(deleted);
            Assert.False(deleted!.IsActive); // Soft delete: IsActive chuyển thành false
        }

        #endregion

        #region AddBatchAsync & LowStock Thresholds

        [Fact]
        public async Task AddBatchAsync_UpdatesStockQtyAndComputesLowStockStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Rau Cần",
                StockQty = 0,
                StockStatus = StockStatus.OutOfStock
            });
            await context.SaveChangesAsync();

            var service = new ProductService(context);
            var batchRequest = new CreateBatchRequest
            {
                FarmId = 1,
                Quantity = 5, // Nhập 5 (< 10 -> LowStock)
                CostPrice = 5000m,
                HarvestDate = DateTime.UtcNow
            };

            // Act
            var result = await service.AddBatchAsync(1, batchRequest);

            // Assert
            Assert.True(result);
            var product = await context.Products.FindAsync(1);
            Assert.Equal(5, product!.StockQty);
            Assert.Equal(StockStatus.LowStock, product.StockStatus);
        }

        [Fact]
        public async Task AddBatchAsync_UpdatesStockQtyAndComputesInStockStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product
            {
                Id = 1,
                Name = "Rau Cần",
                StockQty = 0,
                StockStatus = StockStatus.OutOfStock
            });
            await context.SaveChangesAsync();

            var service = new ProductService(context);
            var batchRequest = new CreateBatchRequest
            {
                FarmId = 1,
                Quantity = 15, // Nhập 15 (>= 10 -> InStock)
                CostPrice = 5000m,
                HarvestDate = DateTime.UtcNow
            };

            // Act
            await service.AddBatchAsync(1, batchRequest);

            // Assert
            var product = await context.Products.FindAsync(1);
            Assert.Equal(15, product!.StockQty);
            Assert.Equal(StockStatus.InStock, product.StockStatus);
        }

        #endregion

        #region GetBatchesAsync & GetLowStockReportAsync

        [Fact]
        public async Task GetBatchesAsync_ReturnsMappedAdminBatchDtos()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, Name = "Farm A" };
            context.Farms.Add(farm);
            context.Batches.Add(new Batch
            {
                Id = 10,
                ProductId = 1,
                Farm = farm,
                HarvestDate = DateTime.UtcNow,
                QuantityRemaining = 20,
                QuantityReceived = 20,
                CostPrice = 10000m
            });
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var result = await service.GetBatchesAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("Farm A", result[0].FarmName);
            Assert.Equal(20, result[0].QuantityRemaining);
        }

        [Fact]
        public async Task GetLowStockReportAsync_ReturnsOnlyActiveAndNotInStockProducts()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.AddRange(
                new Product { Id = 1, Name = "Hết hàng", IsActive = true, StockQty = 0, StockStatus = StockStatus.OutOfStock },
                new Product { Id = 2, Name = "Sắp hết", IsActive = true, StockQty = 3, StockStatus = StockStatus.LowStock },
                new Product { Id = 3, Name = "Còn nhiều", IsActive = true, StockQty = 50, StockStatus = StockStatus.InStock },
                new Product { Id = 4, Name = "Hết hàng ẩn", IsActive = false, StockQty = 0, StockStatus = StockStatus.OutOfStock }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var result = await service.GetLowStockReportAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Hết hàng", result[0].Name); // Đã OrderBy theo StockQty
            Assert.Equal("Sắp hết", result[1].Name);
        }

        #endregion
    }
}