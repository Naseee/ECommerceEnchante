
using ECommerceApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Emit;

namespace ECommerceApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<WishListModel> WishList { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<CategoryOffer> CategoryOffers { get; set; }
        public DbSet<ProductOffer> ProductOffers { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Referrals> Referrals { get; set; }
        public DbSet<WalletTransaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
       .HasOne(u => u.Wallet)
       .WithOne(w => w.User)
       .HasForeignKey<Wallet>(w => w.UserId);

            builder.Entity<OrderHeader>()
                .HasOne(o => o.Address)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict); 
            builder.Entity<OrderHeader>()
        .HasOne(o => o.AUser)
        .WithMany()
        .HasForeignKey(o => o.UserId);
            
            builder.Entity<Coupon>().Property(p => p.DiscountPercentage).HasColumnType("decimal(18, 4)");
            //builder.Entity<CategoryOffer>().Property(p => p.DiscountPercentage).HasColumnType("decimal(18, 4)");
            //builder.Entity<Product>().Property(p => p.DiscountPercentage).HasColumnType("decimal(18, 4)");
            builder.Entity<Wallet>().Property(p => p.balance).HasColumnType("decimal(18, 4)");
            builder.Entity<WalletTransaction>().Property(p => p.Amount).HasColumnType("decimal(18, 4)");
           // builder.Entity<Category>().Property(p => p.DiscountPercentage).HasColumnType("decimal(18, 4)");
            builder.Entity<Offer>().Property(p => p.DiscountPercentage).HasColumnType("decimal(18, 2)");


        }
    }
}
