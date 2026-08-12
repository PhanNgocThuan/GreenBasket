using System;
using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Admin
{
    public class CreateDiscountDTO
    {
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100)]
        public decimal DiscountPercentage { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
