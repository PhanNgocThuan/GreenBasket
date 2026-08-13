namespace GreenBasket.Application.DTOs.Products
{
   
    public class BatchDto
    {
        public int Id { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public DateTime HarvestDate { get; set; }
        public decimal QuantityRemaining { get; set; }
    }
}