using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    // Represents a delivery order to a customer (customers are not permanent)
    public class Orders
    {
        [Key]
        public int Id { get; set; }

        // Customers are not permanent, so store customer snapshot on each order
        [Required]
        [StringLength(150)]
        public string CustomerName { get; set; }

        [StringLength(100)]
        public string CustomerPhone { get; set; }

        [StringLength(250)]
        public string CustomerAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredAt { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public virtual ICollection<OrderItems> Items { get; set; } = new List<OrderItems>();

        // Total computed from items (not mapped)
        public decimal Total => ComputeTotal();

        private decimal ComputeTotal()
        {
            decimal sum = 0m;
            foreach (var it in Items)
            {
                sum += it.UnitPrice * it.Quantity;
            }
            return sum;
        }
    }

    public enum OrderStatus
    {
        Pending,
        PickedUp,
        InTransit,
        Delivered,
        Cancelled
    }
}