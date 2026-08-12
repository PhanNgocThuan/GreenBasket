using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Admin
{
    public class UpdateUserDTO
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;
    }
}
