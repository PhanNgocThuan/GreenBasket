using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBasket.Domain.Entities
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        [MaxLength(200)]
        public string StreetAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        // Khóa ngoại (Foreign Key) liên kết với AppUser
        // Lưu ý: Id của IdentityUser mặc định là kiểu string (GUID), nên UserId ở đây cũng phải là string
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}