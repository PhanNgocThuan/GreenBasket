using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBasket.Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser? AppUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        public int? DiscountCodeId { get; set; }
        public virtual DiscountCode? DiscountCode { get; set; }

        public int? DeliverySlotId { get; set; }
        public virtual DeliverySlot? DeliverySlot { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled, Refunded

        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // e.g., "COD", "MoMo", "CreditCard"

        [MaxLength(50)]
        public string? PaymentStatus { get; set; } // e.g., "Pending", "Paid", "Failed"

        [MaxLength(255)]
        public string? DeliveryAddress { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
