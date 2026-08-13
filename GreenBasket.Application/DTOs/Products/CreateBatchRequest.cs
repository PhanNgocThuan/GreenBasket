using System.ComponentModel.DataAnnotations;
using GreenBasket.Application.Validations;

namespace GreenBasket.Application.DTOs.Products
{
    public class CreateBatchRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "FarmId is invalid.")]
        public int FarmId { get; set; }

        [PastOrTodayDate(ErrorMessage = "Harvest date must not be in the future or left empty.")]
        public DateTime HarvestDate { get; set; }

        [Range(1, 100000, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Range(0, 100000, ErrorMessage = "Cost price cannot be negative.")]
        public decimal CostPrice { get; set; }
    }
}