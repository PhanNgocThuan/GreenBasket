using GreenBasket.Application.DTOs.Admin;
using GreenBasket.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GreenBasket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Bắt buộc phải là Admin mới được phép gọi toàn bộ API trong đây
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // API nâng quyền cho user (Ví dụ: Từ Customer lên Staff)
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Tìm user theo Email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { IsSuccess = false, Message = "User not found with this email." });
            }

            // 2. Kiểm tra xem Role truyền vào có tồn tại trong hệ thống không
            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists)
            {
                return BadRequest(new { IsSuccess = false, Message = $"Role '{model.RoleName}' does not exist in the system." });
            }

            // 3. Kiểm tra xem user đã có quyền này chưa
            var isInRole = await _userManager.IsInRoleAsync(user, model.RoleName);
            if (isInRole)
            {
                return BadRequest(new { IsSuccess = false, Message = "The user already has this role." });
            }

            // 4. Tiến hành gán Role mới cho user
            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (result.Succeeded)
            {
                return Ok(new { IsSuccess = true, Message = $"Successfully assigned role '{model.RoleName}' to user {model.Email}!" });
            }

            return BadRequest(new { IsSuccess = false, Message = "Failed to assign role.", Errors = result.Errors });
        }

        // API lấy danh sách người dùng và quyền của họ
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsersWithRoles()
        {
            var users = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(_userManager.Users);
            var result = new System.Collections.Generic.List<UserWithRolesDTO>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserWithRolesDTO
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles
                });
            }
            
            return Ok(new { IsSuccess = true, Data = result });
        }

        // API gỡ quyền của user
        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { IsSuccess = false, Message = "User not found with this email." });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, model.RoleName);
            if (!isInRole)
            {
                return BadRequest(new { IsSuccess = false, Message = "The user does not have this role." });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
            if (result.Succeeded)
            {
                return Ok(new { IsSuccess = true, Message = $"Successfully removed role '{model.RoleName}' from user {model.Email}!" });
            }

            return BadRequest(new { IsSuccess = false, Message = "Failed to remove role.", Errors = result.Errors });
        }

        // API tạo người dùng mới
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { IsSuccess = false, Message = "Email already exists." });
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { IsSuccess = false, Message = "Failed to create user.", Errors = result.Errors });
            }

            if (!string.IsNullOrEmpty(model.RoleName))
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                }
            }

            return Ok(new { IsSuccess = true, Message = "User created successfully." });
        }

        // API cập nhật thông tin người dùng
        [HttpPut("users/{email}")]
        public async Task<IActionResult> UpdateUser(string email, [FromBody] UpdateUserDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { IsSuccess = false, Message = "User not found." });
            }

            user.FullName = model.FullName;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest(new { IsSuccess = false, Message = "Failed to update user.", Errors = updateResult.Errors });
            }

            if (!string.IsNullOrEmpty(model.RoleName))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                }
            }

            return Ok(new { IsSuccess = true, Message = "User updated successfully." });
        }

        // API xóa người dùng
        [HttpDelete("users/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { IsSuccess = false, Message = "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { IsSuccess = true, Message = "User deleted successfully." });
            }

            return BadRequest(new { IsSuccess = false, Message = "Failed to delete user.", Errors = result.Errors });
        }
    }
}