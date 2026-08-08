namespace GreenBasket.Domain.Entities
{
    public class Batch
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int FarmId { get; set; }
        public Farm Farm { get; set; } = null!;

        public DateTime HarvestDate { get; set; }
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
        public int QuantityReceived { get; set; }
        public int QuantityRemaining { get; set; }
        public decimal CostPrice { get; set; }
    }
}