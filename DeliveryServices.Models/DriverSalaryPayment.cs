using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryServices.Models
{
    public class DriverSalaryPayment
    {
    [Key]
        public int Id { get; set; }

   [Required]
     public int DriverId { get; set; }
        public virtual Driver Driver { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalaryPortion { get; set; }

    [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionPortion { get; set; }

     public int DeliveriesCount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? ProcessedBy { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
  public string? TransactionReference { get; set; }
    }
}
