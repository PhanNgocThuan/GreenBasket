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
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10,2)");

            builder.Entity<Batch>()
                .Property(b => b.CostPrice)
                .HasColumnType("decimal(10,2)");

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

            // Fluent API của module Order/Cart/DiscountCode... để người đó tự thêm vào đây nếu cần
        }
    }
}