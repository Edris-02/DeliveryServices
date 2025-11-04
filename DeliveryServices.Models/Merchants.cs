using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250)]
        public string Address { get; set; }

        // Navigation: a merchant can have many products
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        //public virtual ICollection<Package> Packages { get; set; } = new List<Package>();
    }
}
