using GreenBasket.Application.Interfaces;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using GreenBasket.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));


builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };
});

// 4. Configure CORS for Frontend (allow local dev servers and GitHub Pages)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://127.0.0.1:5500",
            "http://localhost:5500",
            "http://127.0.0.1:8085",
            "http://localhost:8085",
            "http://127.0.0.1:3000",
            "http://localhost:3000",
            "https://phanngocthuan.github.io",
            "null" // Allow file:// requests from browser
        )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 5. Đăng ký Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 6. Cấu hình Swagger kèm nút Authorize (JWT Bearer)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GreenBasket API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsIn...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// 7. Seeding Data: Khởi tạo Roles và tài khoản Admin mặc định khi ứng dụng chạy
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        string[] roleNames = { "Admin", "Staff", "Customer" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Tự động tạo một tài khoản Admin mặc định để test hệ thống
        string adminEmail = "admin@greenbasket.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var newAdmin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator"
            };
            var createAdmin = await userManager.CreateAsync(newAdmin, "Admin@12345");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }

        // Seed Farms/Products/Batches cho module Product & Admin
        await DbInitializer.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during data seeding: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// UseCors must come before UseAuthentication/UseAuthorization
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

// Serve static files (product images uploaded via UploadsController) from wwwroot/uploads/
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

