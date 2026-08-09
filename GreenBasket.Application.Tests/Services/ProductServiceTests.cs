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
    public class ProductServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_ShouldFilterByKeywordAndPrice_AndPaginateCorrectly()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, Name = "Trang trại Đà Lạt", Location = "Đà Lạt", ContactInfo = "123" };

            var p1 = new Product
            {
                Id = 1,
                Name = "Cà chua Organic",
                Category = ProductCategory.LeafyGreens,
                Price = 30000,
                IsActive = true,
                StockStatus = StockStatus.InStock,
                Batches = new List<Batch>
                {
                    new Batch { Id = 10, Farm = farm, QuantityRemaining = 50, HarvestDate = DateTime.UtcNow }
                }
            };
            var p2 = new Product
            {
                Id = 2,
                Name = "Táo Nhập Khẩu",
                Category = ProductCategory.TropicalFruit,
                Price = 80000,
                IsActive = true,
                StockStatus = StockStatus.InStock
            };
            var p3 = new Product
            {
                Id = 3,
                Name = "Cà rốt Tươi",
                Category = ProductCategory.RootVeggies,
                Price = 15000,
                IsActive = false // Sản phẩm đã ẩn
            };

            context.Farms.Add(farm);
            context.Products.AddRange(p1, p2, p3);
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act: Tìm từ khóa "Cà" với minPrice = 20000
            var (items, totalCount) = await service.SearchAsync(
                keyword: "Cà",
                minPrice: 20000,
                maxPrice: null,
                category: null,
                organic: null,
                inStock: null,
                sort: "price-asc",
                page: 1,
                pageSize: 10
            );

            // Assert
            totalCount.Should().Be(1); // Chỉ p1 thỏa mãn (p3 bị ẩn)
            items.Should().HaveCount(1);
            items.First().Name.Should().Be("Cà chua Organic");
            items.First().FarmOrigin.Should().Be("Trang trại Đà Lạt");
        }

        [Fact]
        public async Task SearchAsync_ShouldSortByPriceDescending()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.AddRange(
                new Product { Id = 1, Name = "Rau Muống", Price = 10000, IsActive = true },
                new Product { Id = 2, Name = "Nấm Nhu yếu", Price = 50000, IsActive = true }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var (items, totalCount) = await service.SearchAsync(
                keyword: null, minPrice: null, maxPrice: null, category: null,
                organic: null, inStock: null, sort: "price-desc", page: 1, pageSize: 10
            );

            // Assert
            totalCount.Should().Be(2);
            items.First().Price.Should().Be(50000);
            items.Last().Price.Should().Be(10000);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProductDetailWithActiveBatches_WhenExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, Name = "Nông trại Xanh", Location = "Lâm Đồng", ContactInfo = "123" };
            var product = new Product
            {
                Id = 1,
                Name = "Dưa Hấu",
                Price = 40000,
                IsActive = true,
                Batches = new List<Batch>
                {
                    new Batch { Id = 1, Farm = farm, QuantityRemaining = 20, HarvestDate = DateTime.UtcNow },
                    new Batch { Id = 2, Farm = farm, QuantityRemaining = 0, HarvestDate = DateTime.UtcNow.AddDays(-5) } // Hết hàng
                }
            };

            context.Farms.Add(farm);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Dưa Hấu");
            result.Batches.Should().HaveCount(1); // Chỉ lấy lô còn hàng (QuantityRemaining > 0)
            result.Batches.First().Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductIsInactiveOrNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Cần Tây", IsActive = false });
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var resultInactive = await service.GetByIdAsync(1);
            var resultNotFound = await service.GetByIdAsync(999);

            // Assert
            resultInactive.Should().BeNull();
            resultNotFound.Should().BeNull();
        }

        #endregion

        #region CreateAsync & UpdateAsync & DeleteAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldCreateProductWithOutOfStockStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new ProductService(context);
            var request = new CreateProductRequest
            {
                Name = "Bơ Sáp",
                Category = ProductCategory.TropicalFruit,
                Price = 60000,
                Unit = "Kg"
            };

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.StockQty.Should().Be(0);
            result.StockStatus.Should().Be(StockStatus.OutOfStock.ToString());

            var productInDb = await context.Products.FindAsync(result.Id);
            productInDb!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFieldsAndReturnTrue_WhenProductExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Tên Cũ", Price = 10000, IsActive = true });
            await context.SaveChangesAsync();

            var service = new ProductService(context);
            var request = new UpdateProductRequest
            {
                Name = "Tên Mới",
                Price = 20000,
                Category = ProductCategory.LeafyGreens,
                Unit = "Túi"
            };

            // Act
            var result = await service.UpdateAsync(1, request);

            // Assert
            result.Should().BeTrue();
            var updatedProduct = await context.Products.FindAsync(1);
            updatedProduct!.Name.Should().Be("Tên Mới");
            updatedProduct.Price.Should().Be(20000);
        }

        [Fact]
        public async Task DeleteAsync_ShouldPerformSoftDelete_BySettingIsActiveToFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Sản phẩm xóa", IsActive = true });
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
            var productInDb = await context.Products.FindAsync(1);
            productInDb!.IsActive.Should().BeFalse(); // Soft delete check
        }

        #endregion

        #region AddBatchAsync & GetBatchesAsync Tests

        [Fact]
        public async Task AddBatchAsync_ShouldAddBatchAndUpdateStockQuantityAndStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, Name = "Nông trại", Location = "LĐ", ContactInfo = "000" };
            var product = new Product
            {
                Id = 1,
                Name = "Cam Sành",
                StockQty = 0,
                StockStatus = StockStatus.OutOfStock
            };

            context.Farms.Add(farm);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new ProductService(context);
            var request = new CreateBatchRequest
            {
                FarmId = 1,
                Quantity = 5, // < 10 threshold => LowStock
                HarvestDate = DateTime.UtcNow,
                CostPrice = 15000
            };

            // Act
            var result = await service.AddBatchAsync(1, request);

            // Assert
            result.Should().BeTrue();
            var productInDb = await context.Products.FindAsync(1);
            productInDb!.StockQty.Should().Be(5);
            productInDb.StockStatus.Should().Be(StockStatus.LowStock);

            (await context.Batches.CountAsync(b => b.ProductId == 1)).Should().Be(1);
        }

        [Fact]
        public async Task GetBatchesAsync_ShouldReturnAdminBatchListOrderedByHarvestDate()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, Name = "Nông Trại A", Location = "HCM", ContactInfo = "123" };
            var batchOlder = new Batch { Id = 1, ProductId = 10, Farm = farm, HarvestDate = DateTime.UtcNow.AddDays(-2), QuantityReceived = 50, QuantityRemaining = 50 };
            var batchNewer = new Batch { Id = 2, ProductId = 10, Farm = farm, HarvestDate = DateTime.UtcNow, QuantityReceived = 30, QuantityRemaining = 30 };

            context.Farms.Add(farm);
            context.Batches.AddRange(batchOlder, batchNewer);
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var results = await service.GetBatchesAsync(10);

            // Assert
            results.Should().HaveCount(2);
            results.First().Id.Should().Be(2); // Mới nhất lên đầu
            results.First().FarmName.Should().Be("Nông Trại A");
        }

        #endregion

        #region GetLowStockReportAsync Tests

        [Fact]
        public async Task GetLowStockReportAsync_ShouldReturnOnlyActiveAndNonInStockProducts()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Products.AddRange(
                new Product { Id = 1, Name = "Rau Hết Hàng", StockQty = 0, StockStatus = StockStatus.OutOfStock, IsActive = true },
                new Product { Id = 2, Name = "Rau Sắp Hết", StockQty = 3, StockStatus = StockStatus.LowStock, IsActive = true },
                new Product { Id = 3, Name = "Rau Còn Nhiều", StockQty = 50, StockStatus = StockStatus.InStock, IsActive = true },
                new Product { Id = 4, Name = "Rau Ẩn Hết Hàng", StockQty = 0, StockStatus = StockStatus.OutOfStock, IsActive = false }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var results = await service.GetLowStockReportAsync();

            // Assert
            results.Should().HaveCount(2);
            results.Select(r => r.ProductId).Should().Contain(new[] { 1, 2 });
            results.First().StockQty.Should().Be(0); // Sắp xếp tăng dần theo StockQty
        }

        #endregion
    }
}