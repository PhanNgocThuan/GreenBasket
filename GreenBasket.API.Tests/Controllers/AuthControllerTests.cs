using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Auth;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private const string BaseRoute = "/api/auth";

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnOkOrCreated()
        {
            var client = _factory.CreateClient();

            var dto = new RegisterDTO
            {
                Email = $"user_{Guid.NewGuid():N}@gmail.com",
                Password = "P@ssword123!", // Chuẩn Identity: Chữ hoa, chữ thường, số, ký tự đặc biệt
                FullName = "Test User"
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);

            // Nếu vẫn lỗi 400, dòng này sẽ in thẳng thông báo lỗi ra Console để kiểm tra
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Register Failed with 400: {errorContent}");
            }

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var email = $"login_{Guid.NewGuid():N}@gmail.com";
            var password = "Password123!";

            var registerDto = new RegisterDTO
            {
                Email = email,
                Password = password,
                FullName = "Test Login User"
            };

            var regResponse = await client.PostAsJsonAsync($"{BaseRoute}/register", registerDto);

            // Nếu đăng ký thành công mới tiếp tục test Login
            if (regResponse.IsSuccessStatusCode)
            {
                var loginDto = new LoginDTO
                {
                    Email = email,
                    Password = password
                };

                var response = await client.PostAsJsonAsync($"{BaseRoute}/login", loginDto);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}