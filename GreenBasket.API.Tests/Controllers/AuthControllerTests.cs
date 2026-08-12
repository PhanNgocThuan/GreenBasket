using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using GreenBasket.Application.DTOs.Auth;
using GreenBasket.Domain.Entities; // Đảm bảo đúng namespace của ApplicationUser / IdentityUser
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var email = $"login_{Guid.NewGuid():N}@gmail.com";
            var password = "P@ssword123!";

            // Tạo trực tiếp User hợp lệ vào DB thông qua UserManager
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(); // Đổi ApplicationUser nếu class user tên khác

                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // Bắt buộc true để tránh bị chặn đăng nhập
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Seed user error: {string.Join(", ", createResult.Errors)}");
                }
            }

            // Tiến hành Đăng nhập
            var loginDto = new LoginDTO
            {
                Email = email,
                Password = password
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}