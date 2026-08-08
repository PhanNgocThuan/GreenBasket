namespace GreenBasket.Application.DTOs.Products
{
    public class AdminBatchDto : BatchDto
    {
        public decimal CostPrice { get; set; }
        public int QuantityReceived { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}