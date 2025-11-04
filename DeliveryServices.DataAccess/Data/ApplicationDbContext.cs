using DeliveryServices.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServices.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Merchants> Merchants { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<DeliveryRoutes> DeliveryRoutes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Merchant -> Product (one-to-many)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Merchant)
                .WithMany(m => m.Products)
                .HasForeignKey(p => p.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order -> OrderItems (one-to-many)
            modelBuilder.Entity<OrderItems>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> OrderItems (one-to-many)
            modelBuilder.Entity<OrderItems>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // DeliveryRoutes -> Orders (one-to-many)
            modelBuilder.Entity<Orders>()
                .HasOne<DeliveryRoutes>()
                .WithMany(dr => dr.Orders)
                .HasForeignKey("DeliveryRoutesId")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
