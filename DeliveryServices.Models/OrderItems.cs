using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    public class OrderItems
    {
        [Key]
        public int Id { get; set; }

        // Product snapshot since products are not permanent
        [Required]
        [StringLength(150)]
        public string ProductName { get; set; }

        [StringLength(50)]
        public string ProductSKU { get; set; }

        public string? ProductDescription { get; set; }

        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        // Status of this specific item
        public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;

        // Link to Order (a delivery request)
        public int OrderId { get; set; }
        public virtual Orders? Order { get; set; }
    }

    public enum OrderItemStatus
    {
        Pending,
        Delivered,
        Cancelled
    }
}