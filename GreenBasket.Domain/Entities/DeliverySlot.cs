using System;
using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Domain.Entities
{
    public class DeliverySlot
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string TimeRange { get; set; } = string.Empty; // e.g. "08:00 - 10:00"

        public int MaxOrders { get; set; }
        public int CurrentOrders { get; set; }

        public bool IsAvailable => CurrentOrders < MaxOrders && Date.Date >= DateTime.UtcNow.Date;
    }
}
