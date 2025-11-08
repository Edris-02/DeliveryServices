using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DeliveryServices.Models
{
    public class Merchants
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Merchant name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250)]
        public string Address { get; set; }

        public decimal CurrentBalance { get; set; } = 0m;

        public decimal TotalPaidOut { get; set; } = 0m;

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();
        public virtual ICollection<MerchantPayouts> Payouts { get; set; } = new List<MerchantPayouts>();

        [NotMapped]
        public decimal TotalRevenue => Orders?.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.SubTotal) ?? 0m;

        [NotMapped]
        public int TotalOrders => Orders?.Count ?? 0;

        [NotMapped]
        public int DeliveredOrders => Orders?.Count(o => o.Status == OrderStatus.Delivered) ?? 0;
    }
}
