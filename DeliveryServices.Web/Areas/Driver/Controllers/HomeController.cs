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

        // GET: Driver Dashboard
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

     // GET: View all delivery orders for this driver
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

// GET: View delivery order details
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

      // POST: Update delivery status
        [HttpPost]
        [ValidateAntiForgeryToken]
 public async Task<IActionResult> UpdateDeliveryStatus(int orderId, OrderStatus newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            var order = _unitOfWork.Order.Get(
           o => o.Id == orderId && o.DriverId == user!.DriverId,
                tracked: true);

            if (order == null)
            {
     TempData["error"] = "Order not found or access denied.";
          return RedirectToAction(nameof(Deliveries));
     }

            // Validate status transition
            if (!IsValidStatusTransition(order.Status, newStatus))
            {
          TempData["error"] = "Invalid status transition.";
      return RedirectToAction(nameof(DeliveryDetails), new { id = orderId });
 }

  var oldStatus = order.Status;
            order.Status = newStatus;

 // Update delivery timestamp if delivered
    if (newStatus == OrderStatus.Delivered)
            {
   order.DeliveredAt = DateTime.UtcNow;
     }

            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();

     TempData["success"] = $"Order status updated from {oldStatus} to {newStatus}";
     return RedirectToAction(nameof(DeliveryDetails), new { id = orderId });
    }

        // GET: View payment history
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

  // GET: View profile
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

        // Helper method to validate status transitions
        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
  // Drivers can only transition from certain statuses
  return (currentStatus, newStatus) switch
      {
        (OrderStatus.Pending, OrderStatus.PickedUp) => true,
(OrderStatus.PickedUp, OrderStatus.InTransit) => true,
   (OrderStatus.InTransit, OrderStatus.Delivered) => true,
  (OrderStatus.InTransit, OrderStatus.Cancelled) => true,
      _ => false
   };
  }
    }
}
