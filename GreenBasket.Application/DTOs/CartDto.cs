using System.Collections.Generic;

namespace GreenBasket.Application.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class AddToCartDto
    {
        public string AppUserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public decimal Quantity { get; set; }
    }
}
