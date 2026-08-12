using System;
using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Admin
{
    public class DiscountDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}
