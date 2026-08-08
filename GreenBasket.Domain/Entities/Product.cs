using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBasket.Domain.Entities
{
    public enum ProductCategory
    {
        LeafyGreens,
        RootVeggies,
        TropicalFruit,
        SeasonalFruit
    }

    public enum StockStatus
    {
        InStock,
        LowStock,
        OutOfStock
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public ProductCategory Category { get; set; }

        public string? Description { get; set; }

        public string Unit { get; set; } = "kg";       // kg / 500g / pack

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool Organic { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Cache field, tính lại mỗi khi Batch thay đổi (không phải nguồn sự thật)
        public int StockQty { get; set; }
        public StockStatus StockStatus { get; set; } = StockStatus.OutOfStock;

        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}