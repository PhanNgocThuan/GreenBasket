using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        [MaxLength(100)]
        public string ReceiverName { get; set; } = string.Empty;
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [MaxLength(100)]
        public string StreetAddress { get; set; } = string.Empty;
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;
        [MaxLength(50)]
        public string District { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Ward { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}