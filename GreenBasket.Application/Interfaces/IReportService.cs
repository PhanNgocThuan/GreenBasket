using GreenBasket.Application.DTOs.Products;

namespace GreenBasket.Application.Interfaces
{
    public interface IReportService
    {
        Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime from, DateTime to, string groupBy);
        Task<List<InventoryTurnoverItem>> GetInventoryTurnoverReportAsync(DateTime from, DateTime to);
        Task<List<BestSellerItem>> GetBestSellersAsync(DateTime from, DateTime to, int top);
    }
}