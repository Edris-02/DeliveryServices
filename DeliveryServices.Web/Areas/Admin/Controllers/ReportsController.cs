using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using DeliveryServices.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class ReportsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var viewModel = new ReportsViewModel();
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            var allOrders = _unitOfWork.Order.GetAll(includeProperties: "Items,Driver,Merchant").ToList();
            var allMerchants = _unitOfWork.Merchant.GetAll(includeProperties: "Orders").ToList();
            var allDrivers = _unitOfWork.Driver.GetAll(includeProperties: "Orders").ToList();

            viewModel.TodayRevenue = allOrders
.Where(o => o.CreatedAt.Date == today && o.Status == OrderStatus.Delivered)
     .Sum(o => o.DeliveryFee);

            viewModel.MonthRevenue = allOrders
      .Where(o => o.CreatedAt >= startOfMonth && o.Status == OrderStatus.Delivered)
     .Sum(o => o.DeliveryFee);

            viewModel.YearRevenue = allOrders
                  .Where(o => o.CreatedAt >= startOfYear && o.Status == OrderStatus.Delivered)
             .Sum(o => o.DeliveryFee);

            viewModel.TotalRevenue = allOrders
         .Where(o => o.Status == OrderStatus.Delivered)
           .Sum(o => o.DeliveryFee);

            viewModel.TotalOrders = allOrders.Count;
            viewModel.PendingOrders = allOrders.Count(o => o.Status == OrderStatus.Pending);
            viewModel.DeliveredOrders = allOrders.Count(o => o.Status == OrderStatus.Delivered);
            viewModel.CancelledOrders = allOrders.Count(o => o.Status == OrderStatus.Cancelled);

            viewModel.TopMerchants = allMerchants
                        .OrderByDescending(m => m.Orders.Count)
             .Take(5)
             .Select(m => new MerchantPerformanceDto
             {
                 MerchantName = m.Name,
                 TotalOrders = m.Orders.Count,
                 TotalRevenue = m.Orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                 AverageOrderValue = m.Orders.Any() ? m.Orders.Average(o => o.Total) : 0
             })
             .ToList();

            viewModel.TopDrivers = allDrivers
                .OrderByDescending(d => d.Orders.Count(o => o.Status == OrderStatus.Delivered))
                 .Take(5)
            .Select(d => new DriverPerformanceDto
            {
                DriverName = d.FullName,
                TotalDeliveries = d.Orders.Count(o => o.Status == OrderStatus.Delivered),
                TotalEarnings = d.TotalEarnings,
                AverageDeliveryTime = CalculateAverageDeliveryTime(d.Orders.Where(o => o.Status == OrderStatus.Delivered).ToList())
            })
             .ToList();

            viewModel.MonthlyRevenueTrend = Enumerable.Range(0, 6)
               .Select(i => startOfMonth.AddMonths(-i))
           .Reverse()
                .ToDictionary(
                  month => month.ToString("MMM yyyy"),
                     month => allOrders
                       .Where(o => o.CreatedAt.Year == month.Year &&
                    o.CreatedAt.Month == month.Month &&
                    o.Status == OrderStatus.Delivered)
             .Sum(o => o.DeliveryFee)
               );

            viewModel.OrdersByStatus = new Dictionary<string, int>
          {
         { "Pending", viewModel.PendingOrders },
           { "Delivered", viewModel.DeliveredOrders },
            { "Cancelled", viewModel.CancelledOrders }
     };

            return View(viewModel);
        }

        private double CalculateAverageDeliveryTime(List<Orders> deliveredOrders)
        {
            if (!deliveredOrders.Any()) return 0;

            var totalMinutes = 0.0;
            var count = 0;

            foreach (var order in deliveredOrders)
            {
                if (order.DeliveredAt.HasValue)
                {
                    totalMinutes += (order.DeliveredAt.Value - order.CreatedAt).TotalMinutes;
                    count++;
                }
            }

            return count > 0 ? totalMinutes / count : 0;
        }

        public IActionResult ExportRevenue()
        {
            TempData["info"] = "Export feature coming soon!";
            return RedirectToAction(nameof(Index));
        }
    }
}
