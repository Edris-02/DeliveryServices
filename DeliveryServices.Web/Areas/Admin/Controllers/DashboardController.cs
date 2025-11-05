using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using DeliveryServices.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel();

            // Get all orders
            var allOrders = _unitOfWork.Order.GetAll(includeProperties: "Items").ToList();

            // Statistics
            viewModel.TotalOrders = allOrders.Count;
            viewModel.PendingOrders = allOrders.Count(o => o.Status == OrderStatus.Pending);
            viewModel.DeliveredOrders = allOrders.Count(o => o.Status == OrderStatus.Delivered);
            viewModel.TotalMerchants = _unitOfWork.Merchant.GetAll().Count();

            // Revenue calculations
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            viewModel.TodayRevenue = allOrders
                .Where(o => o.CreatedAt.Date == today && o.Status == OrderStatus.Delivered)
                .Sum(o => o.Total);

            viewModel.MonthRevenue = allOrders
                .Where(o => o.CreatedAt >= startOfMonth && o.Status == OrderStatus.Delivered)
                .Sum(o => o.Total);

            // Recent orders (last 10)
            viewModel.RecentOrders = allOrders
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    CustomerName = o.CustomerName,
                    CustomerPhone = o.CustomerPhone,
                    Status = o.Status,
                    Total = o.Total,
                    CreatedAt = o.CreatedAt
                })
                .ToList();

            // Order status distribution for chart
            viewModel.OrderStatusDistribution = allOrders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Daily revenue for last 7 days
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Reverse();

            viewModel.DailyRevenue = last7Days
                .ToDictionary(
                    date => date.ToString("MMM dd"),
                    date => allOrders
                        .Where(o => o.CreatedAt.Date == date && o.Status == OrderStatus.Delivered)
                        .Sum(o => o.Total)
                );

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
