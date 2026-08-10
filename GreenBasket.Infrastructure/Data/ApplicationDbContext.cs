using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GreenBasket.Domain.Entities;

namespace GreenBasket.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DiscountCode> DiscountCodes { get; set; }
        public DbSet<DeliverySlot> DeliverySlots { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // --- Product / Farm / Batch (module bạn) ---
            // Product.Price đã có [Column(TypeName = "decimal(18,2)")] trên entity — không cấu hình lại ở đây để tránh 2 nguồn định nghĩa
            builder.Entity<Batch>()
                .Property(b => b.CostPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Batch>()
                .HasOne(b => b.Product)
                .WithMany(p => p.Batches)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Batch>()
                .HasOne(b => b.Farm)
                .WithMany(f => f.Batches)
                .HasForeignKey(b => b.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Cart 1-1 User ---
            builder.Entity<Cart>()
                .HasOne(c => c.AppUser)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}