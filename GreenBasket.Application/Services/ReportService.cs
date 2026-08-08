using System.Globalization;
using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Interfaces;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GreenBasket.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private const string CompletedStatus = "Delivered";

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime from, DateTime to, string groupBy)
        {
            // Chỉ tính doanh thu đơn đã giao thành công — đơn Pending/Cancelled/Refunded không tính
            var raw = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order!.Status == CompletedStatus
                    && oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
                .Select(oi => new { oi.Order!.Id, oi.Order.CreatedAt, Amount = oi.Quantity * oi.UnitPrice })
                .ToListAsync();

            Func<DateTime, string> keySelector = groupBy?.ToLower() switch
            {
                "month" => d => d.ToString("yyyy-MM"),
                "week" => d => $"{ISOWeek.GetYear(d)}-W{ISOWeek.GetWeekOfYear(d):D2}",
                _ => d => d.ToString("yyyy-MM-dd")
            };

            return raw
                .GroupBy(x => keySelector(x.CreatedAt))
                .Select(g => new RevenueReportItem
                {
                    PeriodLabel = g.Key,
                    Revenue = g.Sum(x => x.Amount),
                    OrderCount = g.Select(x => x.Id).Distinct().Count()
                })
                .OrderBy(r => r.PeriodLabel)
                .ToList();
        }

        public async Task<List<InventoryTurnoverItem>> GetInventoryTurnoverReportAsync(DateTime from, DateTime to)
        {
            var sales = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order!.Status == CompletedStatus
                    && oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, UnitsSold = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var productIds = sales.Select(s => s.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            return sales.Select(s =>
            {
                var p = products.First(x => x.Id == s.ProductId);
                // Xấp xỉ: dùng StockQty hiện tại làm avg inventory vì chưa có snapshot tồn kho theo thời gian
                var turnover = p.StockQty > 0 ? (double)(s.UnitsSold / p.StockQty) : 0;

                return new InventoryTurnoverItem
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    UnitsSold = s.UnitsSold,
                    CurrentStock = p.StockQty,
                    TurnoverRatio = Math.Round(turnover, 2)
                };
            })
            .OrderByDescending(x => x.TurnoverRatio)
            .ToList();
        }

        public async Task<List<BestSellerItem>> GetBestSellersAsync(DateTime from, DateTime to, int top)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Order!.Status == CompletedStatus
                    && oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new BestSellerItem
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(top)
                .ToListAsync();
        }
    }
}