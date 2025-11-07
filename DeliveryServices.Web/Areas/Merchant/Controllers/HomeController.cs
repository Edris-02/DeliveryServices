using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryServices.Web.Areas.Merchant.Controllers
{
    [Area("Merchant")]
    [Authorize(Roles = UserRoles.Merchant)]
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

          if (user?.MerchantId == null)
      {
 TempData["error"] = "Merchant profile not found.";
      return RedirectToAction("Login", "Account", new { area = "Identity" });
   }

   var merchant = _unitOfWork.Merchant.Get(
 m => m.Id == user.MerchantId.Value,
 includeProperties: "Orders.Items,Payouts");

   if (merchant == null)
   {
      TempData["error"] = "Merchant profile not found.";
 return RedirectToAction("Login", "Account", new { area = "Identity" });
     }

    return View(merchant);
        }

        // View all orders for this merchant
        public async Task<IActionResult> Orders()
        {
       var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
   var user = await _userManager.FindByIdAsync(userId!);

     if (user?.MerchantId == null)
            {
   return RedirectToAction(nameof(Index));
 }

    var orders = _unitOfWork.Order.GetAll(
      o => o.MerchantId == user.MerchantId.Value,
  includeProperties: "Items")
      .OrderByDescending(o => o.CreatedAt)
      .ToList();

            return View(orders);
        }

 // View order details
        public async Task<IActionResult> OrderDetails(int id)
        {
  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
       var user = await _userManager.FindByIdAsync(userId!);

            var order = _unitOfWork.Order.Get(
    o => o.Id == id && o.MerchantId == user!.MerchantId,
     includeProperties: "Items,Merchant");

   if (order == null)
 {
         TempData["error"] = "Order not found or access denied.";
    return RedirectToAction(nameof(Orders));
        }

   return View(order);
        }

        // View payout history
        public async Task<IActionResult> Payouts()
        {
   var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

  if (user?.MerchantId == null)
   {
    return RedirectToAction(nameof(Index));
       }

 var payouts = _unitOfWork.MerchantPayout.GetAll(
      p => p.MerchantId == user.MerchantId.Value)
      .OrderByDescending(p => p.PaidAt)
         .ToList();

   return View(payouts);
  }

  // View profile
        public async Task<IActionResult> Profile()
  {
     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var user = await _userManager.FindByIdAsync(userId!);

      if (user?.MerchantId == null)
      {
      return RedirectToAction(nameof(Index));
  }

   var merchant = _unitOfWork.Merchant.Get(m => m.Id == user.MerchantId.Value);

       if (merchant == null)
   {
    return RedirectToAction(nameof(Index));
  }

  ViewBag.User = user;
  return View(merchant);
        }
    }
}
