namespace GreenBasket.Application.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool Organic { get; set; }
        public int StockQty { get; set; }
        public string StockStatus { get; set; } = string.Empty;

        public string? FarmOrigin { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? HarvestDate { get; set; }
    }
}