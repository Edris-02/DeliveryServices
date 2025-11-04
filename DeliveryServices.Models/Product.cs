using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }

        public string Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public int QuantityAvailable { get; set; }

        // FK to merchant that supplies this product
        public int MerchantId { get; set; }
        public virtual Merchants Merchant { get; set; }

        // Product can appear in many order items (deliveries)
        public virtual ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>();
    }
}