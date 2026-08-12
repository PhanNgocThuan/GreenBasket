using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GreenBasket.Infrastructure.Data;

namespace GreenBasket.API.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // 1. Gỡ bỏ DbContext SQL Server mặc định
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. Thêm InMemory Database với tên DB cố định cho Instance
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // 3. Cấu hình Test Scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                    options.DefaultScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }

        // Thay CreateClient bằng ConfigureClient (phương thức virtual hợp lệ)
        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            SeedRoles();
        }

        private void SeedRoles()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
            var roles = new[] { "Customer", "Admin", "Staff" };

            if (roleManager != null)
            {
                foreach (var roleName in roles)
                {
                    if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new IdentityRole(roleName)
                        {
                            NormalizedName = roleName.ToUpper()
                        }).GetAwaiter().GetResult();
                    }
                }
            }
            else
            {
                foreach (var roleName in roles)
                {
                    if (!context.Roles.Any(r => r.Name == roleName))
                    {
                        context.Roles.Add(new IdentityRole
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = roleName,
                            NormalizedName = roleName.ToUpper()
                        });
                    }
                }
                context.SaveChanges();
            }
        }
    }
}