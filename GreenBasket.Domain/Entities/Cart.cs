using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Domain.Entities
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AppUserId { get; set; } = string.Empty;

        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
