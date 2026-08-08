namespace GreenBasket.Domain.Entities
{
    public class Farm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }

        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }
}