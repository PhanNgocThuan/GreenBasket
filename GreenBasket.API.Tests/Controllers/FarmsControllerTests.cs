using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Products;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class FarmsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private const string BaseRoute = "/api/admin/farms";

        public FarmsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync(BaseRoute);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new CreateFarmRequest { Name = "Nông trại Mẫu" };

            // Act
            var response = await client.PostAsJsonAsync(BaseRoute, request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_AsAdmin_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // DTO rỗng để vi phạm ModelState Validation
            var request = new CreateFarmRequest();

            // Act
            var response = await client.PostAsJsonAsync(BaseRoute, request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_AsAdmin_WithValidData_ShouldReturnCreatedOrOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            var request = new CreateFarmRequest
            {
                Name = "Nông trại Đà Lạt",
                Location = "Đà Lạt, Lâm Đồng"
            };

            // Act
            var response = await client.PostAsJsonAsync(BaseRoute, request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }

        #endregion
    }
}