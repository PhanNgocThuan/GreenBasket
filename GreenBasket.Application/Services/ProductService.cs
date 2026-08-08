using GreenBasket.Application.DTOs.Products;
using GreenBasket.Application.Interfaces;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GreenBasket.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private const int LowStockThreshold = 10;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ProductDto> Items, int TotalCount)> SearchAsync(
            string? keyword, decimal? minPrice, decimal? maxPrice,
            string? category, bool? organic, bool? inStock,
            string? sort, int page, int pageSize)
        {
            var query = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Batches)
                    .ThenInclude(b => b.Farm)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(kw) ||
                    (p.Description != null && p.Description.ToLower().Contains(kw)) ||
                    p.Batches.Any(b => b.Farm.Name.ToLower().Contains(kw)));
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (organic.HasValue) query = query.Where(p => p.Organic == organic.Value);

            if (!string.IsNullOrWhiteSpace(category) &&
                Enum.TryParse<ProductCategory>(category, true, out var cat))
            {
                query = query.Where(p => p.Category == cat);
            }

            if (inStock.HasValue)
            {
                query = inStock.Value
                    ? query.Where(p => p.StockStatus != StockStatus.OutOfStock)
                    : query.Where(p => p.StockStatus == StockStatus.OutOfStock);
            }

            query = sort switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();

            // Materialize trước rồi map trong C# — logic "batch mới nhất còn hàng"
            // dùng navigation property, không nên ép EF dịch sang SQL.
            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapToDto).ToList();

            return (items, totalCount);
        }

        public async Task<ProductDetailDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Batches)
                    .ThenInclude(b => b.Farm)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return null;

            var dto = MapToDto(product);

            return new ProductDetailDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Category = dto.Category,
                Price = dto.Price,
                Unit = dto.Unit,
                Organic = dto.Organic,
                StockQty = dto.StockQty,
                StockStatus = dto.StockStatus,
                FarmOrigin = dto.FarmOrigin,
                HarvestDate = dto.HarvestDate,
                ImageUrl = dto.ImageUrl,
                Description = product.Description,
                // Public detail — chỉ dùng BatchDto, KHÔNG dùng AdminBatchDto
                Batches = product.Batches
                    .Where(b => b.QuantityRemaining > 0)
                    .OrderByDescending(b => b.HarvestDate)
                    .Select(b => new BatchDto
                    {
                        Id = b.Id,
                        FarmName = b.Farm.Name,
                        HarvestDate = b.HarvestDate,
                        QuantityRemaining = b.QuantityRemaining
                    })
                    .ToList()
            };
        }

        public async Task<ProductDto> CreateAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                Unit = request.Unit,
                Price = request.Price,
                Organic = request.Organic,
                ImageUrl = request.ImageUrl,
                IsActive = true,
                StockQty = 0,
                StockStatus = StockStatus.OutOfStock
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Name = request.Name;
            product.Category = request.Category;
            product.Description = request.Description;
            product.Unit = request.Unit;
            product.Price = request.Price;
            product.Organic = request.Organic;
            product.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = false; // soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBatchAsync(int productId, CreateBatchRequest request)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var batch = new Batch
            {
                ProductId = productId,
                FarmId = request.FarmId,
                HarvestDate = request.HarvestDate,
                QuantityReceived = request.Quantity,
                QuantityRemaining = request.Quantity,
                CostPrice = request.CostPrice,
                ReceivedDate = DateTime.UtcNow
            };

            _context.Batches.Add(batch);

            product.StockQty += request.Quantity;
            product.StockStatus = ComputeStockStatus(product.StockQty);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminBatchDto>> GetBatchesAsync(int productId)
        {
            return await _context.Batches
                .Where(b => b.ProductId == productId)
                .Include(b => b.Farm)
                .OrderByDescending(b => b.HarvestDate)
                .Select(b => new AdminBatchDto
                {
                    Id = b.Id,
                    FarmName = b.Farm.Name,
                    HarvestDate = b.HarvestDate,
                    QuantityRemaining = b.QuantityRemaining,
                    CostPrice = b.CostPrice,
                    QuantityReceived = b.QuantityReceived,
                    ReceivedDate = b.ReceivedDate
                })
                .ToListAsync();
        }

        private static StockStatus ComputeStockStatus(int qty) =>
            qty <= 0 ? StockStatus.OutOfStock :
            qty < LowStockThreshold ? StockStatus.LowStock :
            StockStatus.InStock;

        private static ProductDto MapToDto(Product p)
        {
            var latestBatch = p.Batches
                .Where(b => b.QuantityRemaining > 0)
                .OrderByDescending(b => b.HarvestDate)
                .FirstOrDefault();

            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category.ToString(),
                Price = p.Price,
                Unit = p.Unit,
                Organic = p.Organic,
                StockQty = p.StockQty,
                StockStatus = p.StockStatus.ToString(),
                FarmOrigin = latestBatch?.Farm.Name,
                ImageUrl = p.ImageUrl,
                HarvestDate = latestBatch?.HarvestDate
            };
        }

        public async Task<List<LowStockReportItem>> GetLowStockReportAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive && p.StockStatus != StockStatus.InStock)
                .OrderBy(p => p.StockQty)
                .Select(p => new LowStockReportItem
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    StockQty = p.StockQty,
                    StockStatus = p.StockStatus.ToString()
                })
                .ToListAsync();
        }
    }
}