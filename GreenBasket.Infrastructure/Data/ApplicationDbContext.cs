using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GreenBasket.Domain.Entities;

namespace GreenBasket.Infrastructure.Data
{
    // Tạm thời sử dụng IdentityUser. 
    // Sau khi tạo class AppUser ở tầng Domain, chúng ta sẽ đổi thành IdentityDbContext<AppUser>
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng (DbSet) của hệ thống ở đây (ngoại trừ các bảng của Identity đã có sẵn)
        // Ví dụ:
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DiscountCode> DiscountCodes { get; set; }
        public DbSet<DeliverySlot> DeliverySlots { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // BẮT BUỘC phải gọi base.OnModelCreating(builder) đầu tiên khi dùng IdentityDbContext
            base.OnModelCreating(builder);

            // Tùy chỉnh tên bảng của Identity cho ngắn gọn và đẹp hơn trong SQL Server (Tùy chọn)
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Sau này bạn có thể viết Fluent API để cấu hình quan hệ các bảng (1-N, N-N) ở đây
        }
    }
}