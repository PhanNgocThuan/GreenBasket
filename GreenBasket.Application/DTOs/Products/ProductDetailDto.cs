namespace GreenBasket.Application.DTOs.Products
{
    public class ProductDetailDto : ProductDto
    {
        public string? Description { get; set; }

        public List<BatchDto> Batches { get; set; } = new();
    }
}