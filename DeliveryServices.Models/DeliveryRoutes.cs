using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models
{
    // Represents a delivery route or shipment that groups multiple orders
    public class DeliveryRoutes
    {
        [Key]
        public int Id { get; set; }

        public string RouteName { get; set; }

        // Driver or courier assigned
        public string CourierName { get; set; }

        public DateTime ScheduledAt { get; set; }

        public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();
    }
}