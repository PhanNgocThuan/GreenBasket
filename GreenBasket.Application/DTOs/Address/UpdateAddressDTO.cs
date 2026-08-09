using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Address
{
    public class UpdateAddressDTO
    {
        [Required(ErrorMessage = "Recipient name is required.")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required.")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "District is required.")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ward is required.")]
        public string Ward { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}