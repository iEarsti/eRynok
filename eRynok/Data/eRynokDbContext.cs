using eRynok.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Data
{
    public class eRynokDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public eRynokDbContext(
            DbContextOptions<eRynokDbContext> options)
            : base(options)
        {
        }

        public DbSet<SellerProfile> SellerProfiles { get; set; }

        public DbSet<BuyerProfile> BuyerProfiles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderGroup> OrderGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApplicationUser configuration
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(254);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.HasOne(u => u.SellerProfile)
                    .WithOne(p => p.User)
                    .HasForeignKey<SellerProfile>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(u => u.BuyerProfile)
                    .WithOne(p => p.User)
                    .HasForeignKey<BuyerProfile>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Products)
                    .WithOne(p => p.Seller)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.Buyer)
                    .HasForeignKey(o => o.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.OrderGroups)
                    .WithOne(g => g.Buyer)
                    .HasForeignKey(g => g.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SellerProfile configuration
            modelBuilder.Entity<SellerProfile>(entity =>
            {
                entity.HasKey(p => p.UserId);

                entity.Property(p => p.Phone)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(p => p.Address)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            // BuyerProfile configuration
            modelBuilder.Entity<BuyerProfile>(entity =>
            {
                entity.HasKey(p => p.UserId);

                entity.Property(p => p.Phone)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(p => p.Address)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasIndex(c => c.Name)
                    .IsUnique();

                entity.Property(c => c.Description)
                    .IsRequired();

                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(p => p.Description)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(p => p.ImageUrl)
                    .IsRequired(false);

                entity.Property(p => p.CreatedAt)
                    .IsRequired();

                entity.HasMany(p => p.Orders)
                    .WithOne(o => o.Product)
                    .HasForeignKey(o => o.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Quantity)
                    .IsRequired();

                entity.Property(o => o.UnitPrice)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(o => o.Status)
                    .IsRequired();

                entity.Property(o => o.CreatedAt)
                    .IsRequired();

                entity.HasOne(o => o.OrderGroup)
                    .WithMany(g => g.Orders)
                    .HasForeignKey(o => o.OrderGroupId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderGroup configuration
            modelBuilder.Entity<OrderGroup>(entity =>
            {
                entity.HasKey(g => g.Id);

                entity.Property(g => g.CreatedAt)
                    .IsRequired();
            });
        }
    }
}
