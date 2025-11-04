using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    public class OrderItems
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        // Link to Order (a delivery request)
        public int OrderId { get; set; }
        public virtual Orders Order { get; set; }
    }
}