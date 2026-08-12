using FluentAssertions;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        [Fact]
        public async Task GetCart_WithValidToken_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            var userId = "test-user-id";
            client.AddBearerToken(role: "Customer", userId: userId);

            // Seed trước 1 Cart trống cho user trong In-Memory Database
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Kiểm tra nếu giỏ hàng chưa tồn tại thì thêm mới
                var existingCart = await context.Carts.FirstOrDefaultAsync(c => c.AppUserId == userId);
                if (existingCart == null)
                {
                    context.Carts.Add(new Cart
                    {
                        AppUserId = userId // Đổi CustomerId thành thuộc tính tương ứng trong Cart.cs
                    });
                    await context.SaveChangesAsync();
                }
            }

            var response = await client.GetAsync("/api/cart");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}