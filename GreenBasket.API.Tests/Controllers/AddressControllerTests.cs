using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Address;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class AddressControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AddressControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        #region GetUserAddresses Tests

        [Fact]
        public async Task GetUserAddresses_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/address");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserAddresses_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            // Act
            var response = await client.GetAsync("/api/address");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region CreateAddress Tests

        [Fact]
        public async Task CreateAddress_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var dto = new CreateAddressDTO { StreetAddress = "123 Lê Lợi" };

            // Act
            var response = await client.PostAsJsonAsync("/api/address", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateAddress_WithValidToken_ShouldReturnOkOrCreated()
        {
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            var dto = new CreateAddressDTO
            {
                ReceiverName = "Nguyen Van A",
                PhoneNumber = "0901234567",
                StreetAddress = "123 Đường Nguyễn Huệ",
                Ward = "Phường Bến Nghé",
                District = "Quận 1",
                City = "TP. Hồ Chí Minh",
                IsDefault = true
            };

            var response = await client.PostAsJsonAsync("/api/address", dto);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        #endregion

        #region Update & Delete Tests

        [Fact]
        public async Task UpdateAddress_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var dto = new UpdateAddressDTO { StreetAddress = "456 Nguyễn Huệ" };

            // Act
            var response = await client.PutAsJsonAsync("/api/address/1", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteAddress_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/api/address/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}