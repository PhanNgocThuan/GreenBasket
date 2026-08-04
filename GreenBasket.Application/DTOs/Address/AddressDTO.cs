namespace GreenBasket.Application.DTOs.Address

{
    public class AddressDTO
    {
        public int Id { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}