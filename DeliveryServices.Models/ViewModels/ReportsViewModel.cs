using System;
using System.Collections.Generic;

namespace DeliveryServices.Models.ViewModels
{
    public class ReportsViewModel
    {
        // Revenue Metrics
  public decimal TodayRevenue { get; set; }
  public decimal MonthRevenue { get; set; }
     public decimal YearRevenue { get; set; }
        public decimal TotalRevenue { get; set; }

  // Order Statistics
   public int TotalOrders { get; set; }
  public int PendingOrders { get; set; }
 public int InTransitOrders { get; set; }
    public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

  // Performance Data
        public List<MerchantPerformanceDto> TopMerchants { get; set; } = new();
        public List<DriverPerformanceDto> TopDrivers { get; set; } = new();

        // Chart Data
    public Dictionary<string, decimal> MonthlyRevenueTrend { get; set; } = new();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    }

    public class MerchantPerformanceDto
    {
        public string MerchantName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class DriverPerformanceDto
    {
        public string DriverName { get; set; }
    public int TotalDeliveries { get; set; }
   public decimal TotalEarnings { get; set; }
        public double AverageDeliveryTime { get; set; }
    }
}
