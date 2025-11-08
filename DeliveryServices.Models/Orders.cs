using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DeliveryServices.Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; }

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

        public int? MerchantId { get; set; }
        public virtual Merchants? Merchant { get; set; }

        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DeliveryFee { get; set; } = 0m;

        public virtual ICollection<OrderItems> Items { get; set; } = new List<OrderItems>();

        [NotMapped]
        public decimal SubTotal => ComputeSubTotal();

        [NotMapped]
        public decimal Total => SubTotal + DeliveryFee;

        [NotMapped]
        public decimal MerchantAmount => SubTotal;

        [NotMapped]
        public int DeliveredItemsCount => Items.Count(i => i.Status == OrderItemStatus.Delivered);

        [NotMapped]
        public int CancelledItemsCount => Items.Count(i => i.Status == OrderItemStatus.Cancelled);

        [NotMapped]
        public int PendingItemsCount => Items.Count(i => i.Status == OrderItemStatus.Pending);

        private decimal ComputeSubTotal()
        {
            decimal sum = 0m;
            foreach (var it in Items)
            {
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
        Delivered,
        Cancelled
    }
}