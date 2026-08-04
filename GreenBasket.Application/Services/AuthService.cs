using GreenBasket.Application.DTOs.Auth;
using GreenBasket.Application.Interfaces;
using GreenBasket.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GreenBasket.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        // Dependency Injection để gọi UserManager và đọc cấu hình từ appsettings.json
        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                throw new Exception("Email này đã được đăng ký.");

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            // UserManager sẽ tự động băm (hash) mật khẩu trước khi lưu vào Database
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new Exception("Đăng ký thất bại. Vui lòng kiểm tra lại độ khó của mật khẩu.");

            return new AuthResponseDTO
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = new List<string> { "Customer" } // Tạm thời gán mặc định role hiển thị
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new Exception("Tài khoản không tồn tại.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
                throw new Exception("Sai mật khẩu.");

            var userRoles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, userRoles);

            return new AuthResponseDTO
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = userRoles,
                Token = token
            };
        }

        private string GenerateJwtToken(AppUser user, IList<string> roles)
        {
            // Nạp các thông tin cần thiết (Claims) vào Token
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Thêm danh sách Roles vào Claims
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Đọc Secret Key từ cấu hình để ký Token
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}