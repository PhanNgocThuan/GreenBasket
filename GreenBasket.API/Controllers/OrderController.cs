using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Interfaces;

namespace GreenBasket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("calculate-cost")]
        public async Task<IActionResult> CalculateCost([FromBody] CalculateCostDto dto)
        {
            var total = await _orderService.CalculateTotalCostAsync(dto);
            return Ok(new { TotalCost = total });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(dto);
                return Ok(order);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId, [FromQuery] string userId)
        {
            var result = await _orderService.CancelOrderAsync(orderId, userId);
            if (!result) return BadRequest(new { message = "Order cannot be cancelled or not found." });
            return Ok(new { message = "Order cancelled successfully." });
        }

        [HttpPut("update-status/{orderId}")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromQuery] string status)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId, status);
            if (!result) return NotFound(new { message = "Order not found." });
            return Ok(new { message = $"Order status updated to {status}." });
        }

        [HttpPost("report-issue/{orderId}")]
        public async Task<IActionResult> ReportIssue(int orderId, [FromBody] string reportDetails)
        {
            var result = await _orderService.ReportDamagedGoodsAsync(orderId, reportDetails);
            if (!result) return BadRequest(new { message = "Order not found or not yet delivered." });
            return Ok(new { message = "Issue reported successfully." });
        }
    }
}
