using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Admin;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class AdminControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private const string AssignRoleUrl = "/api/admin/assign-role";

        public AdminControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AssignRole_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var dto = new AssignRoleDTO
            {
                Email = "user@example.com",
                RoleName = "Staff"
            };

            // Act: Gọi /api/admin/assign-role khi chưa đăng nhập
            var response = await client.PostAsJsonAsync(AssignRoleUrl, dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AssignRole_AsCustomer_ShouldReturnForbidden()
        {
            // Arrange: Giả lập Token có Role là "Customer" (không có quyền Admin)
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            var dto = new AssignRoleDTO
            {
                Email = "user@example.com",
                RoleName = "Staff"
            };

            // Act: Gọi chuẩn URL /api/admin/assign-role
            var response = await client.PostAsJsonAsync(AssignRoleUrl, dto);

            // Assert: Hệ thống trả về 403 Forbidden do thiếu Role "Admin"
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AssignRole_AsAdmin_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange: Token Admin nhưng truyền DTO rỗng để kích hoạt ModelState Validation
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            var dto = new AssignRoleDTO();

            // Act
            var response = await client.PostAsJsonAsync(AssignRoleUrl, dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AssignRole_AsAdmin_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange: Token Admin nhưng truyền Email không tồn tại
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            var dto = new AssignRoleDTO
            {
                Email = "nonexistent_user@example.com",
                RoleName = "Staff"
            };

            // Act
            var response = await client.PostAsJsonAsync(AssignRoleUrl, dto);

            // Assert: Trả về 404 NotFound từ Controller
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}