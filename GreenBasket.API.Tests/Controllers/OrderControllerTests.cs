using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class OrderControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private const string BaseRoute = "/api/order";

        public OrderControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CalculateCost_WithToken_ShouldReturnOkOrBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            var dto = new CalculateCostDto(); // Khai báo DTO tính phí

            // Act: Đường dẫn chuẩn /api/order/calculate-cost
            var response = await client.PostAsJsonAsync($"{BaseRoute}/calculate-cost", dto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateOrder_WithToken_ShouldReturnOkOrBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            var dto = new CreateOrderDto(); // Khai báo DTO tạo đơn hàng

            // Act: Đường dẫn chuẩn /api/order/create
            var response = await client.PostAsJsonAsync($"{BaseRoute}/create", dto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderNotFound_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            // Act: Đường dẫn chuẩn /api/order/cancel/{orderId}?userId={userId}
            var response = await client.PostAsync($"{BaseRoute}/cancel/99999?userId=test-user-id", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}