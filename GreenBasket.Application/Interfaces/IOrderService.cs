using System.Collections.Generic;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs;

namespace GreenBasket.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<List<OrderDto>> GetUserOrdersAsync(string userId);
        Task<decimal> CalculateTotalCostAsync(CalculateCostDto dto);
        Task<bool> CancelOrderAsync(int orderId, string userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> ReportDamagedGoodsAsync(int orderId, string reportDetails);
    }
}
