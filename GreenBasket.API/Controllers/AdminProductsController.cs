using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenBasket.API.Controllers
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // POST: api/admin/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // GET: api/admin/products/5 (dùng cho CreatedAtAction, và admin xem lại nhanh)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound(new { Message = $"Can't find the product with ID = {id}." });
            return Ok(product);
        }

        // PUT: api/admin/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _productService.UpdateAsync(id, request);
            if (!success) return NotFound(new { Message = $"Can't find the product with ID = {id}." });

            return NoContent();
        }

        // DELETE: api/admin/products/5 (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteAsync(id);
            if (!success) return NotFound(new { Message = $"Can't find the product with ID = {id}." });

            return NoContent();
        }

        // POST: api/admin/products/5/batches — nhập lô hàng mới
        [HttpPost("{id}/batches")]
        public async Task<IActionResult> AddBatch(int id, [FromBody] CreateBatchRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _productService.AddBatchAsync(id, request);
            if (!success) return NotFound(new { Message = $"Can't find the product with ID = {id}." });

            return NoContent();
        }

        // GET: api/admin/products/5/batches — lịch sử batch, có CostPrice
        [HttpGet("{id}/batches")]
        public async Task<IActionResult> GetBatches(int id)
        {
            var batches = await _productService.GetBatchesAsync(id);
            return Ok(batches);
        }

        // GET: api/admin/products/reports/low-stock
        [HttpGet("reports/low-stock")]
        public async Task<IActionResult> GetLowStockReport()
        {
            var report = await _productService.GetLowStockReportAsync();
            return Ok(report);
        }
    }
}