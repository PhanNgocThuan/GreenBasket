using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Admin
{
    public class CreateUserDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string RoleName { get; set; } = "Customer";
    }
}
