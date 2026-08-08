using System.ComponentModel.DataAnnotations;
using GreenBasket.Domain.Entities;

namespace GreenBasket.Application.DTOs.Products
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Product name must be 2-150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [EnumDataType(typeof(ProductCategory), ErrorMessage = "Category is invalid.")]
        public ProductCategory Category { get; set; }

        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        [StringLength(20)]
        public string Unit { get; set; } = "kg";

        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public bool Organic { get; set; }
    }

    public class UpdateProductRequest : CreateProductRequest { }
}