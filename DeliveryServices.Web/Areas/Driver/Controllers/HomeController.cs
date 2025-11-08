using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryServices.Web.Areas.Driver.Controllers
{
    [Area("Driver")]
    [Authorize(Roles = UserRoles.Driver)]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.DriverId == null)
            {
                TempData["error"] = "Driver profile not found.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var driver = _unitOfWork.Driver.Get(
            d => d.Id == user.DriverId.Value,
                    includeProperties: "Orders,SalaryPayments");

            if (driver == null)
            {
                TempData["error"] = "Driver profile not found.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return View(driver);
        }

        public async Task<IActionResult> Deliveries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.DriverId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var orders = _unitOfWork.Order.GetAll(
     o => o.DriverId == user.DriverId.Value,
   includeProperties: "Items,Merchant")
   .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        public async Task<IActionResult> DeliveryDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            var order = _unitOfWork.Order.Get(
                   o => o.Id == id && o.DriverId == user!.DriverId,
                            includeProperties: "Items,Merchant,Driver");

            if (order == null)
            {
                TempData["error"] = "Order not found or access denied.";
                return RedirectToAction(nameof(Deliveries));
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDeliveryStatus(int orderId, OrderStatus newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            var order = _unitOfWork.Order.Get(
                o => o.Id == orderId && o.DriverId == user!.DriverId,
            includeProperties: "Items,Merchant,Driver",
            tracked: true);

            if (order == null)
            {
                TempData["error"] = "Order not found or access denied.";
                return RedirectToAction(nameof(Deliveries));
            }

            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                TempData["error"] = "Invalid status transition.";
                return RedirectToAction(nameof(DeliveryDetails), new { id = orderId });
            }

            var oldStatus = order.Status;
            order.Status = newStatus;

            if (newStatus == OrderStatus.Delivered && oldStatus != OrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;

                if (order.Driver != null)
                {
                    order.Driver.TotalDeliveries++;
                    order.Driver.CurrentMonthDeliveries++;

                    order.Driver.CurrentBalance += order.Driver.CommissionPerDelivery;

                    if (order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance += order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    _unitOfWork.Driver.Update(order.Driver);
                }
            }

            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();

            TempData["success"] = $"Order status updated from {oldStatus} to {newStatus}";
            return RedirectToAction(nameof(DeliveryDetails), new { id = orderId });
        }

        public async Task<IActionResult> Payments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.DriverId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var payments = _unitOfWork.DriverSalaryPayment.GetAll(
      p => p.DriverId == user.DriverId.Value)
   .OrderByDescending(p => p.PaymentDate)
        .ToList();

            return View(payments);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.DriverId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var driver = _unitOfWork.Driver.Get(d => d.Id == user.DriverId.Value);

            if (driver == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.User = user;
            return View(driver);
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.PickedUp) => true,
                (OrderStatus.PickedUp, OrderStatus.Delivered) => true,
                (OrderStatus.PickedUp, OrderStatus.Cancelled) => true,
                _ => false
            };
        }
    }
}
