using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DeliveryServices.Models
{
    // Represents a delivery route or shipment that groups multiple orders
    public class DeliveryRoutes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RouteName { get; set; }

        [Required]
        [StringLength(100)]
        public string CourierName { get; set; }

        public DateTime ScheduledAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public RouteStatus Status { get; set; } = RouteStatus.Planned;

        [StringLength(500)]
        public string? Notes { get; set; }

        public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();

        // Computed properties
        public int TotalOrders => Orders?.Count ?? 0;
        
        public decimal TotalRevenue => Orders?.Sum(o => o.Total) ?? 0m;
        
        public int DeliveredOrders => Orders?.Count(o => o.Status == OrderStatus.Delivered) ?? 0;
        
        public int PendingOrders => Orders?.Count(o => o.Status == OrderStatus.Pending) ?? 0;
    }

    public enum RouteStatus
    {
        Planned,      // Route is scheduled but not started
        InProgress,   // Courier is actively delivering
        Completed,    // All deliveries finished
        Cancelled     // Route was cancelled
    }
}