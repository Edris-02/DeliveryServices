using DeliveryServices.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeliveryServices.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Merchants> Merchants { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<MerchantPayouts> MerchantPayouts { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<DriverSalaryPayment> DriverSalaryPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Merchant-User relationship
            modelBuilder.Entity<Merchants>()
                .HasOne(m => m.User)
                .WithOne(u => u.Merchant)
                .HasForeignKey<Merchants>(m => m.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Driver-User relationship
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.User)
                .WithOne(u => u.Driver)
                .HasForeignKey<Driver>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Order-Driver relationship
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.Driver)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DriverId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
