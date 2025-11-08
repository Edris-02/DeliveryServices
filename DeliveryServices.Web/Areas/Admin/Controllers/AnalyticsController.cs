using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using DeliveryServices.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AnalyticsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnalyticsController(IUnitOfWork unitOfWork)
        {
       _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
      var viewModel = new AnalyticsViewModel();
 var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

      // Get all data
var allOrders = _unitOfWork.Order.GetAll(includeProperties: "Items,Driver,Merchant").ToList();
     var allMerchants = _unitOfWork.Merchant.GetAll().ToList();
       var allDrivers = _unitOfWork.Driver.GetAll().ToList();

     // Key Metrics
   viewModel.TotalOrders = allOrders.Count;
 viewModel.TotalMerchants = allMerchants.Count;
 viewModel.TotalDrivers = allDrivers.Count;
   viewModel.ActiveDrivers = allDrivers.Count(d => d.IsActive);
  
 viewModel.AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.Total) : 0;
            viewModel.AverageDeliveryFee = allOrders.Any() ? allOrders.Average(o => o.DeliveryFee) : 0;

            // Growth metrics (compare to previous month)
         var previousMonthStart = startOfMonth.AddMonths(-1);
    var currentMonthOrders = allOrders.Count(o => o.CreatedAt >= startOfMonth);
   var previousMonthOrders = allOrders.Count(o => o.CreatedAt >= previousMonthStart && o.CreatedAt < startOfMonth);
  
            viewModel.OrderGrowthPercent = previousMonthOrders > 0 
      ? ((currentMonthOrders - previousMonthOrders) / (double)previousMonthOrders) * 100 
      : 0;

     var currentMonthRevenue = allOrders
        .Where(o => o.CreatedAt >= startOfMonth && o.Status == OrderStatus.Delivered)
                .Sum(o => o.DeliveryFee);
       var previousMonthRevenue = allOrders
       .Where(o => o.CreatedAt >= previousMonthStart && o.CreatedAt < startOfMonth && o.Status == OrderStatus.Delivered)
           .Sum(o => o.DeliveryFee);
            
          viewModel.RevenueGrowthPercent = previousMonthRevenue > 0 
    ? ((double)(currentMonthRevenue - previousMonthRevenue) / (double)previousMonthRevenue) * 100 
    : 0;

   // Hourly order distribution (for today)
     viewModel.HourlyOrderDistribution = Enumerable.Range(0, 24)
              .ToDictionary(
    hour => $"{hour:D2}:00",
  hour => allOrders.Count(o => o.CreatedAt.Date == today && o.CreatedAt.Hour == hour)
    );

   // Daily orders for last 30 days
            viewModel.DailyOrderTrend = Enumerable.Range(0, 30)
 .Select(i => today.AddDays(-i))
  .Reverse()
                .ToDictionary(
   date => date.ToString("MMM dd"),
  date => allOrders.Count(o => o.CreatedAt.Date == date)
     );

      // Order completion rate
         var completedOrders = allOrders.Count(o => o.Status == OrderStatus.Delivered);
       viewModel.OrderCompletionRate = allOrders.Any() ? (completedOrders / (double)allOrders.Count) * 100 : 0;

      // Average items per order
            viewModel.AverageItemsPerOrder = allOrders.Any() 
    ? allOrders.Average(o => o.Items?.Count ?? 0) 
            : 0;

        // Top performing days of week
            viewModel.OrdersByDayOfWeek = allOrders
    .GroupBy(o => o.CreatedAt.DayOfWeek)
   .OrderBy(g => (int)g.Key)
             .ToDictionary(
    g => g.Key.ToString(),
         g => g.Count()
);

        // Peak hours (top 5 busiest hours)
        viewModel.PeakHours = allOrders
        .GroupBy(o => o.CreatedAt.Hour)
 .OrderByDescending(g => g.Count())
       .Take(5)
       .Select(g => new PeakHourDto
      {
         Hour = $"{g.Key:D2}:00 - {(g.Key + 1):D2}:00",
   OrderCount = g.Count()
   })
          .ToList();

    // Merchant activity
            viewModel.ActiveMerchants = allMerchants.Count(m => m.Orders.Any(o => o.CreatedAt >= startOfMonth));
            viewModel.NewMerchantsThisMonth = 0; // Merchants model doesn't have CreatedAt, set to 0 for now

       // Driver efficiency
  viewModel.DriverUtilizationRate = allDrivers.Any() 
   ? (allDrivers.Count(d => d.Orders.Any(o => o.CreatedAt >= startOfMonth)) / (double)allDrivers.Count) * 100 
    : 0;

 // Revenue by merchant (top 10)
      viewModel.RevenueByMerchant = allOrders
    .Where(o => o.Status == OrderStatus.Delivered && o.Merchant != null)
         .GroupBy(o => o.Merchant.Name)
  .OrderByDescending(g => g.Sum(o => o.DeliveryFee))
      .Take(10)
     .ToDictionary(
 g => g.Key,
          g => g.Sum(o => o.DeliveryFee)
       );

        return View(viewModel);
        }
    }
}
