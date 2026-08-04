using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Address
{
    public class CreateAddressDTO
    {
        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ cụ thể (số nhà, đường) là bắt buộc")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phường/Xã là bắt buộc")]
        public string Ward { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}