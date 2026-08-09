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
    public class ReportServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        #region GetRevenueReportAsync Tests

        [Fact]
        public async Task GetRevenueReportAsync_ShouldOnlyIncludeDeliveredOrdersWithinDateRange()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var now = DateTime.UtcNow;

            var product = new Product { Id = 1, Name = "Sữa Tươi", Price = 30000, StockQty = 10 };
            context.Products.Add(product);

            var orderDelivered = new Order { Id = 1, Status = "Delivered", CreatedAt = now };
            var orderPending = new Order { Id = 2, Status = "Pending", CreatedAt = now };
            var orderOld = new Order { Id = 3, Status = "Delivered", CreatedAt = now.AddDays(-10) };

            context.Orders.AddRange(orderDelivered, orderPending, orderOld);

            context.OrderItems.AddRange(
                new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, UnitPrice = 30000 }, // 60k - Hợp lệ
                new OrderItem { Id = 2, OrderId = 2, ProductId = 1, Quantity = 5, UnitPrice = 30000 }, // Pending - Bỏ qua
                new OrderItem { Id = 3, OrderId = 3, ProductId = 1, Quantity = 3, UnitPrice = 30000 }  // Ngoài khoảng - Bỏ qua
            );

            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act
            var result = await service.GetRevenueReportAsync(now.AddDays(-1), now.AddDays(1), "day");

            // Assert
            result.Should().HaveCount(1);
            result.First().Revenue.Should().Be(60000);
            result.First().OrderCount.Should().Be(1);
        }

        [Fact]
        public async Task GetRevenueReportAsync_ShouldGroupByMonth_WhenGroupByIsMonth()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var date1 = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var date2 = new DateTime(2026, 1, 20, 10, 0, 0, DateTimeKind.Utc);
            var date3 = new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc);

            var order1 = new Order { Id = 1, Status = "Delivered", CreatedAt = date1 };
            var order2 = new Order { Id = 2, Status = "Delivered", CreatedAt = date2 };
            var order3 = new Order { Id = 3, Status = "Delivered", CreatedAt = date3 };

            context.Orders.AddRange(order1, order2, order3);
            context.OrderItems.AddRange(
                new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, UnitPrice = 10000 },
                new OrderItem { Id = 2, OrderId = 2, ProductId = 1, Quantity = 2, UnitPrice = 10000 },
                new OrderItem { Id = 3, OrderId = 3, ProductId = 1, Quantity = 4, UnitPrice = 10000 }
            );
            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act
            var result = await service.GetRevenueReportAsync(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), "month");

            // Assert
            result.Should().HaveCount(2);

            result.First().PeriodLabel.Should().Be("2026-01");
            result.First().Revenue.Should().Be(30000);
            result.First().OrderCount.Should().Be(2);

            result.Last().PeriodLabel.Should().Be("2026-02");
            result.Last().Revenue.Should().Be(40000);
            result.Last().OrderCount.Should().Be(1);
        }

        #endregion

        #region GetInventoryTurnoverReportAsync Tests

        [Fact]
        public async Task GetInventoryTurnoverReportAsync_ShouldCalculateTurnoverCorrectly()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var now = DateTime.UtcNow;

            var product1 = new Product { Id = 1, Name = "Rau Cần", StockQty = 10 };
            var product2 = new Product { Id = 2, Name = "Cà Rốt", StockQty = 0 };

            context.Products.AddRange(product1, product2);

            var order = new Order { Id = 1, Status = "Delivered", CreatedAt = now };
            context.Orders.Add(order);

            context.OrderItems.AddRange(
                new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 20, UnitPrice = 5000 },
                new OrderItem { Id = 2, OrderId = 1, ProductId = 2, Quantity = 10, UnitPrice = 8000 }
            );
            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act
            var result = await service.GetInventoryTurnoverReportAsync(now.AddDays(-1), now.AddDays(1));

            // Assert
            result.Should().HaveCount(2);

            var item1 = result.First(r => r.ProductId == 1);
            item1.UnitsSold.Should().Be(20);
            item1.CurrentStock.Should().Be(10);
            item1.TurnoverRatio.Should().Be(2.0); // 20 / 10 = 2

            var item2 = result.First(r => r.ProductId == 2);
            item2.UnitsSold.Should().Be(10);
            item2.CurrentStock.Should().Be(0);
            item2.TurnoverRatio.Should().Be(0); // Tồn kho = 0 => Vòng quay = 0
        }

        #endregion

        #region GetBestSellersAsync Tests

        [Fact]
        public async Task GetBestSellersAsync_ShouldReturnTopNProductsOrderedByUnitsSoldDescending()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var now = DateTime.UtcNow;

            var p1 = new Product { Id = 1, Name = "Táo" };
            var p2 = new Product { Id = 2, Name = "Cam" };
            var p3 = new Product { Id = 3, Name = "Xoài" };

            context.Products.AddRange(p1, p2, p3);

            var order = new Order { Id = 1, Status = "Delivered", CreatedAt = now };
            context.Orders.Add(order);

            context.OrderItems.AddRange(
                new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 10, UnitPrice = 20000 }, // Táo: 10
                new OrderItem { Id = 2, OrderId = 1, ProductId = 2, Quantity = 50, UnitPrice = 15000 }, // Cam: 50
                new OrderItem { Id = 3, OrderId = 1, ProductId = 3, Quantity = 30, UnitPrice = 25000 }  // Xoài: 30
            );
            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act: Lấy top 2 sản phẩm bán chạy nhất
            var result = await service.GetBestSellersAsync(now.AddDays(-1), now.AddDays(1), top: 2);

            // Assert
            result.Should().HaveCount(2);

            result[0].ProductId.Should().Be(2); // Cam (50 sản phẩm)
            result[0].UnitsSold.Should().Be(50);
            result[0].Revenue.Should().Be(750000);

            result[1].ProductId.Should().Be(3); // Xoài (30 sản phẩm)
            result[1].UnitsSold.Should().Be(30);
            result[1].Revenue.Should().Be(750000);
        }

        #endregion
    }
}