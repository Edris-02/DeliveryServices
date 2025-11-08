using System;
using System.Collections.Generic;

namespace DeliveryServices.Models.ViewModels
{
    public class AnalyticsViewModel
    {
        // Key Metrics
        public int TotalOrders { get; set; }
        public int TotalMerchants { get; set; }
    public int TotalDrivers { get; set; }
  public int ActiveDrivers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal AverageDeliveryFee { get; set; }

   // Growth Metrics
   public double OrderGrowthPercent { get; set; }
        public double RevenueGrowthPercent { get; set; }

        // Distribution Data
        public Dictionary<string, int> HourlyOrderDistribution { get; set; } = new();
        public Dictionary<string, int> DailyOrderTrend { get; set; } = new();
 public Dictionary<string, int> OrdersByDayOfWeek { get; set; } = new();
        public Dictionary<string, decimal> RevenueByMerchant { get; set; } = new();

        // Performance Metrics
        public double OrderCompletionRate { get; set; }
        public double AverageItemsPerOrder { get; set; }
        public double DriverUtilizationRate { get; set; }

 // Additional Insights
        public List<PeakHourDto> PeakHours { get; set; } = new();
        public int ActiveMerchants { get; set; }
        public int NewMerchantsThisMonth { get; set; }
    }

    public class PeakHourDto
    {
 public string Hour { get; set; }
        public int OrderCount { get; set; }
    }
}
