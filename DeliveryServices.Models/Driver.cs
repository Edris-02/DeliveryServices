using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryServices.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Driver name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Vehicle Type")]
        public string? VehicleType { get; set; }

        [StringLength(20)]
        [Display(Name = "Vehicle Plate Number")]
        public string? VehiclePlateNumber { get; set; }

        [Display(Name = "Base Salary")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; } = 0;

        [Display(Name = "Commission Per Delivery")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionPerDelivery { get; set; } = 5.00m; 

        [Display(Name = "Total Deliveries")]
        public int TotalDeliveries { get; set; } = 0;

        [Display(Name = "Current Month Deliveries")]
        public int CurrentMonthDeliveries { get; set; } = 0;

        [Display(Name = "Total Earnings")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEarnings { get; set; } = 0;

        [Display(Name = "Current Balance")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Joined Date")]
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();
        public virtual ICollection<DriverSalaryPayment> SalaryPayments { get; set; } = new List<DriverSalaryPayment>();

        [NotMapped]
        [Display(Name = "This Month Earnings")]
        public decimal CurrentMonthEarnings =>
   (BaseSalary / 12) + (CurrentMonthDeliveries * CommissionPerDelivery);

        [NotMapped]
        [Display(Name = "Average Earnings Per Delivery")]
        public decimal AverageEarningsPerDelivery =>
         TotalDeliveries > 0 ? TotalEarnings / TotalDeliveries : 0;
    }
}
