using GreenBasket.Application.DTOs.Products;

public interface IProductService
{
    Task<(List<ProductDto> Items, int TotalCount)> SearchAsync(
        string? keyword, decimal? minPrice, decimal? maxPrice,
        string? category, bool? organic, bool? inStock,
        string? sort, int page, int pageSize);

    Task<ProductDetailDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<bool> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id); // soft delete: IsActive = false
    Task<bool> AddBatchAsync(int productId, CreateBatchRequest request);

    Task<List<AdminBatchDto>> GetBatchesAsync(int productId);
    Task<List<LowStockReportItem>> GetLowStockReportAsync();
}