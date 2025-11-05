using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    // Represents a payment made to a merchant
    public class MerchantPayouts
    {
        [Key]
        public int Id { get; set; }

        public int MerchantId { get; set; }
        public virtual Merchants? Merchant { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; } // Cash, Bank Transfer, etc.

        [StringLength(100)]
        public string? ProcessedBy { get; set; } // Who processed this payment
    }
}
