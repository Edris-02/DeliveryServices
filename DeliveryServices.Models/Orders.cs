using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        // Link to Merchant (who is selling the products)
        public int? MerchantId { get; set; }
        public virtual Merchants? Merchant { get; set; }

        // Delivery fee charged for this order
        [Range(0, double.MaxValue)]
        public decimal DeliveryFee { get; set; } = 0m;

        public virtual ICollection<OrderItems> Items { get; set; } = new List<OrderItems>();

        // Total computed from delivered items only (not mapped to database)
        [NotMapped]
        public decimal SubTotal => ComputeSubTotal();

        // Total including delivery fee
        [NotMapped]
        public decimal Total => SubTotal + DeliveryFee;

        // Amount that goes to the merchant (subtotal - we keep delivery fee)
        [NotMapped]
        public decimal MerchantAmount => SubTotal;

        // Count of delivered items
        [NotMapped]
        public int DeliveredItemsCount => Items.Count(i => i.Status == OrderItemStatus.Delivered);

        // Count of cancelled items
        [NotMapped]
        public int CancelledItemsCount => Items.Count(i => i.Status == OrderItemStatus.Cancelled);

        // Count of pending items
        [NotMapped]
        public int PendingItemsCount => Items.Count(i => i.Status == OrderItemStatus.Pending);

        private decimal ComputeSubTotal()
        {
            decimal sum = 0m;
            foreach (var it in Items)
            {
                // Only include delivered items in subtotal
                if (it.Status == OrderItemStatus.Delivered)
                {
                    sum += it.UnitPrice * it.Quantity;
                }
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