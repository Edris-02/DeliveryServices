using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Optional: Link to Merchant if user is a merchant
        public int? MerchantId { get; set; }
        public virtual Merchants? Merchant { get; set; }

        // Optional: Link to Driver if user is a driver
        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }
    }
}
