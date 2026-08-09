using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class ReportsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ReportsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        #region Authorization Tests (Kiểm tra Phân quyền)

        [Fact]
        public async Task ReportEndpoints_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act: Cập nhật đường dẫn chuẩn có /admin
            var lowStockResponse = await client.GetAsync("/api/admin/reports/low-stock");
            var revenueResponse = await client.GetAsync("/api/admin/reports/revenue?from=2026-08-01&to=2026-08-08");

            // Assert
            lowStockResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            revenueResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region GetLowStock Tests

        [Fact]
        public async Task GetLowStock_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/low-stock");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GetRevenue Tests

        [Fact]
        public async Task GetRevenue_WhenFromIsAfterTo_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/revenue?from=2026-08-10&to=2026-08-01&groupBy=day");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetRevenue_WithValidDates_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/revenue?from=2026-08-01&to=2026-08-08&groupBy=month");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GetInventoryTurnover Tests

        [Fact]
        public async Task GetInventoryTurnover_WhenFromIsAfterTo_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/inventory-turnover?from=2026-08-10&to=2026-08-01");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetInventoryTurnover_WithValidDates_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/inventory-turnover?from=2026-08-01&to=2026-08-08");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GetBestSellers Tests

        [Fact]
        public async Task GetBestSellers_WhenFromIsAfterTo_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/best-sellers?from=2026-08-10&to=2026-08-01&top=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetBestSellers_WithOutOfRangeTop_ShouldNormalizeAndReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/best-sellers?from=2026-08-01&to=2026-08-08&top=150");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBestSellers_WithValidParameters_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            // Act
            var response = await client.GetAsync("/api/admin/reports/best-sellers?from=2026-08-01&to=2026-08-08&top=5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion
    }
}