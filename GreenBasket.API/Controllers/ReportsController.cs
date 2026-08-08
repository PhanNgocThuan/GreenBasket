using GreenBasket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenBasket.API.Controllers
{
    [Route("api/admin/reports")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IProductService _productService;

        public ReportsController(IReportService reportService, IProductService productService)
        {
            _reportService = reportService;
            _productService = productService;
        }

        // GET: api/admin/reports/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var report = await _productService.GetLowStockReportAsync();
            return Ok(report);
        }

        // GET: api/admin/reports/revenue?from=2026-01-01&to=2026-08-08&groupBy=month
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string groupBy = "day")
        {
            if (from > to) return BadRequest(new { Message = "'from' must not be after 'to'." });

            var report = await _reportService.GetRevenueReportAsync(from, to, groupBy);
            return Ok(report);
        }

        // GET: api/admin/reports/inventory-turnover?from=&to=
        [HttpGet("inventory-turnover")]
        public async Task<IActionResult> GetInventoryTurnover([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from > to) return BadRequest(new { Message = "'from' must not be after 'to'." });

            var report = await _reportService.GetInventoryTurnoverReportAsync(from, to);
            return Ok(report);
        }

        // GET: api/admin/reports/best-sellers?from=&to=&top=10
        [HttpGet("best-sellers")]
        public async Task<IActionResult> GetBestSellers(
            [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int top = 10)
        {
            if (from > to) return BadRequest(new { Message = "'from' must not be after 'to'." });
            if (top < 1 || top > 100) top = 10;

            var report = await _reportService.GetBestSellersAsync(from, to, top);
            return Ok(report);
        }
    }
}