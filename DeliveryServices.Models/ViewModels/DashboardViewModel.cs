using System;
using System.Collections.Generic;

namespace DeliveryServices.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int TotalMerchants { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }

        public List<RecentOrderDto> RecentOrders { get; set; } = new();

        public Dictionary<string, int> OrderStatusDistribution { get; set; } = new();
        public Dictionary<string, decimal> DailyRevenue { get; set; } = new();
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
