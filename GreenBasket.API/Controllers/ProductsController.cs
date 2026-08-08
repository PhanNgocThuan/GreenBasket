using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Interfaces;
using GreenBasket.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GreenBasket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? keyword,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? category,
            [FromQuery] bool? organic,
            [FromQuery] bool? inStock,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
           
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

           
            if (!string.IsNullOrWhiteSpace(category) &&
                !Enum.TryParse<ProductCategory>(category, true, out _))
            {
                return BadRequest(new { Message = $"Category '{category}' is invalid." });
            }

            var (items, totalCount) = await _productService.SearchAsync(
                keyword, minPrice, maxPrice, category, organic, inStock, sort, page, pageSize);

           
            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = items
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { Message = $"Can not find the product with the id = {id}." });
            }

            return Ok(product);
        }
    }
}