using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? MerchantId { get; set; }
        public virtual Merchants? Merchant { get; set; }

        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }
    }
}
