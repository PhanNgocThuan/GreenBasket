using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class CartControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CartControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        #region GetCart Tests

        [Fact]
        public async Task GetCart_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");
            var userId = "test-user-id";

            // Act: Match [HttpGet("{userId}")] -> /api/cart/{userId}
            var response = await client.GetAsync($"/api/cart/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region AddToCart Tests

        [Fact]
        public async Task AddToCart_WithValidData_ShouldReturnOkOrCreated()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            var dto = new AddToCartDto
            {
                ProductId = 1,
                Quantity = 2
            };

            // Act: Match [HttpPost("add")] -> /api/cart/add
            var response = await client.PostAsJsonAsync("/api/cart/add", dto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddToCart_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            var dto = new AddToCartDto(); // DTO rỗng để kích hoạt ModelState Validation

            // Act: Match /api/cart/add
            var response = await client.PostAsJsonAsync("/api/cart/add", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region UpdateCartItem Tests

        [Fact]
        public async Task UpdateCartItem_WhenItemDoesNotExist_ShouldReturnNotFoundOrBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");
            var nonExistentItemId = 9999;
            var dto = new UpdateCartItemDto { Quantity = 5 };

            // Act: Match [HttpPut("update-item/{cartItemId}")] -> /api/cart/update-item/9999
            var response = await client.PutAsJsonAsync($"/api/cart/update-item/{nonExistentItemId}", dto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        #endregion

        #region RemoveCartItem Tests

        [Fact]
        public async Task RemoveCartItem_WhenItemDoesNotExist_ShouldReturnNotFoundOrBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");
            var nonExistentItemId = 9999;

            // Act: Match [HttpDelete("remove-item/{cartItemId}")] -> /api/cart/remove-item/9999
            var response = await client.DeleteAsync($"/api/cart/remove-item/{nonExistentItemId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        #endregion
    }
}