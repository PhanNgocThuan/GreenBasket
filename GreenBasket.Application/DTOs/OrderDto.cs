using System;
using System.Collections.Generic;

namespace GreenBasket.Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? DeliverySlot { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateOrderDto
    {
        public string AppUserId { get; set; } = string.Empty;
        public int? DiscountCodeId { get; set; }
        public int? DeliverySlotId { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class CalculateCostDto
    {
        public string AppUserId { get; set; } = string.Empty;
        public string? DiscountCode { get; set; }
    }
}
