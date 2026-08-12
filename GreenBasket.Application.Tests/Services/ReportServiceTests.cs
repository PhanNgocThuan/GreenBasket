using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GreenBasket.Application.Tests
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

        #region GetRevenueReportAsync

        [Fact]
        public async Task GetRevenueReportAsync_FiltersOnlyDeliveredOrdersInDateRange()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);

            var deliveredOrder = new Order { Id = 1, Status = "Delivered", CreatedAt = new DateTime(2026, 1, 15) };
            var pendingOrder = new Order { Id = 2, Status = "Pending", CreatedAt = new DateTime(2026, 1, 15) };
            var outOfRangeOrder = new Order { Id = 3, Status = "Delivered", CreatedAt = new DateTime(2026, 2, 1) };

            context.Orders.AddRange(deliveredOrder, pendingOrder, outOfRangeOrder);
            context.OrderItems.AddRange(
                new OrderItem { Id = 1, Order = deliveredOrder, Quantity = 2, UnitPrice = 50000m }, // 100,000
                new OrderItem { Id = 2, Order = pendingOrder, Quantity = 5, UnitPrice = 50000m },   // Bị bỏ qua
                new OrderItem { Id = 3, Order = outOfRangeOrder, Quantity = 1, UnitPrice = 50000m } // Bị bỏ qua
            );
            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act
            var result = await service.GetRevenueReportAsync(from, to, "day");

            // Assert
            Assert.Single(result);
            Assert.Equal("2026-01-15", result[0].PeriodLabel);
            Assert.Equal(100000m, result[0].Revenue);
            Assert.Equal(1, result[0].OrderCount);
        }

        [Fact]
        public async Task GetRevenueReportAsync_GroupByMonthAndWeek_FormatsPeriodLabelCorrectly()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);

            var order = new Order { Id = 1, Status = "Delivered", CreatedAt = new DateTime(2026, 1, 15) };
            context.Orders.Add(order);
            context.OrderItems.Add(new OrderItem { Id = 1, Order = order, Quantity = 1, UnitPrice = 20000m });
            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act
            var monthlyResult = await service.GetRevenueReportAsync(from, to, "month");
            var weeklyResult = await service.GetRevenueReportAsync(from, to, "week");

            // Assert
            Assert.Equal("2026-01", monthlyResult[0].PeriodLabel);
            Assert.Contains("2026-W", weeklyResult[0].PeriodLabel);
        }

        #endregion

        #region GetInventoryTurnoverReportAsync

        [Fact]
        public async Task GetInventoryTurnoverReportAsync_CalculatesTurnoverAndOrdersByRatioDesc()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);

            var p1 = new Product { Id = 1, Name = "Sản phẩm A", StockQty = 10 };
            var p2 = new Product { Id = 2, Name = "Sản phẩm B", StockQty = 0 };

            var order = new Order { Id = 1, Status = "Delivered", CreatedAt = new DateTime(2026, 1, 10) };

            context.Products.AddRange(p1, p2);
            context.Orders.Add(order);
            context.OrderItems.AddRange(
                new OrderItem { Id = 1, Order = order, ProductId = 1, Quantity = 20 },
                new OrderItem { Id = 2, Order = order, ProductId = 2, Quantity = 5 }
            );
            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act
            var result = await service.GetInventoryTurnoverReportAsync(from, to);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].ProductId);
            Assert.Equal(2.0, result[0].TurnoverRatio); // 20 sold / 10 stock
            Assert.Equal(20, result[0].UnitsSold);

            Assert.Equal(2, result[1].ProductId);
            Assert.Equal(0, result[1].TurnoverRatio); // StockQty = 0 -> TurnoverRatio = 0
        }

        #endregion

        #region GetBestSellersAsync

        [Fact]
        public async Task GetBestSellersAsync_ReturnsTopNBestSellersOrderedByUnitsSold()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);

            var p1 = new Product { Id = 1, Name = "Táo" };
            var p2 = new Product { Id = 2, Name = "Cam" };
            var p3 = new Product { Id = 3, Name = "Xoài" };

            var order = new Order { Id = 1, Status = "Delivered", CreatedAt = new DateTime(2026, 1, 10) };

            context.Products.AddRange(p1, p2, p3);
            context.Orders.Add(order);
            context.OrderItems.AddRange(
                new OrderItem { Id = 1, Order = order, Product = p1, ProductId = 1, Quantity = 10, UnitPrice = 1000m },
                new OrderItem { Id = 2, Order = order, Product = p2, ProductId = 2, Quantity = 50, UnitPrice = 2000m },
                new OrderItem { Id = 3, Order = order, Product = p3, ProductId = 3, Quantity = 30, UnitPrice = 1500m }
            );
            await context.SaveChangesAsync();

            var service = new ReportService(context);

            // Act (lấy top 2 sản phẩm bán chạy)
            var result = await service.GetBestSellersAsync(from, to, 2);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Cam", result[0].Name);  // 50 units
            Assert.Equal(50, result[0].UnitsSold);
            Assert.Equal(100000m, result[0].Revenue);

            Assert.Equal("Xoài", result[1].Name); // 30 units
            Assert.Equal(30, result[1].UnitsSold);
        }

        #endregion
    }
}