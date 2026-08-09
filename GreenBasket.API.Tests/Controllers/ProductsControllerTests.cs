using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        #region GetProducts Tests (Danh sách & Lọc sản phẩm)

        [Fact]
        public async Task GetProducts_WithValidParameters_ShouldReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/products?keyword=rau&minPrice=10000&maxPrice=50000&organic=true&page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetProducts_WithInvalidCategory_ShouldReturnBadRequestOrOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/products?category=InvalidCategoryName");

            // Assert: Trả về BadRequest nếu có Validate Category hoặc OK với danh sách rỗng
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetProducts_WithOutOfRangePagination_ShouldNormalizeAndReturnOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act: Truyền page = -5 và pageSize = 100
            var response = await client.GetAsync("/api/products?page=-5&pageSize=100");

            // Assert: Controller/Service tự chuẩn hóa tham số về giá trị hợp lệ và trả 200 OK
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region GetProduct Tests (Chi tiết sản phẩm)

        [Fact]
        public async Task GetProduct_WhenProductDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/products/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion
    }
}