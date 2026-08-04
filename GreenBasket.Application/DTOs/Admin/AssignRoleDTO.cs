using System.ComponentModel.DataAnnotations;

namespace GreenBasket.Application.DTOs.Admin
{
    public class AssignRoleDTO
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên quyền (Role) không được để trống.")]
        public string RoleName { get; set; } = string.Empty; // Ví dụ: "Staff", "Admin", "Customer"
    }
}