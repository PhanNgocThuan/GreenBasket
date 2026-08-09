using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Products;
using Xunit;

namespace GreenBasket.API.Tests.Controllers
{
    public class AdminProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private const string BaseRoute = "/api/admin/products";

        public AdminProductsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AdminProductEndpoints_AsCustomer_ShouldReturnForbidden()
        {
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Customer");

            // Đã bỏ CategoryId
            var dto = new CreateProductRequest
            {
                Name = "Sản Phẩm Test",
                Price = 50000,
                Unit = "Kg"
            };

            var response = await client.PostAsJsonAsync(BaseRoute, dto);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AddBatch_AsAdmin_WhenProductDoesNotExist_ShouldReturnNotFound()
        {
            var client = _factory.CreateClient();
            client.AddBearerToken(role: "Admin");

            var dto = new CreateBatchRequest
            {
                FarmId = 1,                              // Bắt buộc >= 1 [Range(1, int.MaxValue)]
                HarvestDate = DateTime.UtcNow.AddDays(-1), // Ngày trong quá khứ hoặc hôm nay
                Quantity = 50,                           // Bắt buộc từ 1 - 100000
                CostPrice = 20000                        // Bắt buộc từ 0 - 100000
            };

            var response = await client.PostAsJsonAsync("/api/admin/products/99999/batches", dto);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}