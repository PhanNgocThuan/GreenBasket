using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GreenBasket.Application.DTOs;
using GreenBasket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GreenBasket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        [HttpPost("calculate-cost")]
        public async Task<IActionResult> CalculateCost([FromBody] CalculateCostDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            dto.AppUserId = userId;

            var total = await _orderService.CalculateTotalCostAsync(dto);
            return Ok(new { TotalCost = total });
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(new { isSuccess = true, data = orders });
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(new { isSuccess = true, data = orders });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                dto.AppUserId = userId;

                var order = await _orderService.CreateOrderAsync(dto);
                return Ok(order);
            }
            catch (System.Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { message = msg });
            }
        }

        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _orderService.CancelOrderAsync(orderId, userId);
            if (!result) return BadRequest(new { message = "Order cannot be cancelled or not found." });
            return Ok(new { message = "Order cancelled successfully." });
        }

        [HttpPut("update-status/{orderId}")]
        [Authorize(Roles = "Admin,Staff")]
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
