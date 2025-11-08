using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class MerchantsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MerchantsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var merchants = _unitOfWork.Merchant.GetAll().ToList();
            return View(merchants);
        }

        public IActionResult Details(int id)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == id, includeProperties: "Orders.Items,Payouts,User");
            if (merchant == null)
            {
                return NotFound();
            }
            return View(merchant);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Merchants merchant)
        {
            ModelState.Remove("Orders");
            ModelState.Remove("Payouts");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View(merchant);
            }

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(merchant.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists");
                    return View(merchant);
                }

                var user = new ApplicationUser
                {
                    UserName = merchant.Email,
                    Email = merchant.Email,
                    FullName = merchant.Name,
                    PhoneNumber = merchant.PhoneNumber,
                    EmailConfirmed = true
                };

                var defaultPassword = $"{merchant.Name.Split(' ')[0]}@123";
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    ModelState.AddModelError("", $"Failed to create user account: {errors}");
                    return View(merchant);
                }

                await _userManager.AddToRoleAsync(user, UserRoles.Merchant);

                merchant.UserId = user.Id;
                merchant.CurrentBalance = 0m;
                merchant.TotalPaidOut = 0m;

                _unitOfWork.Merchant.Add(merchant);
                _unitOfWork.Save();

                user.MerchantId = merchant.Id;
                await _userManager.UpdateAsync(user);

                TempData["success"] = $"Merchant created successfully! Login credentials - Email: {merchant.Email}, Password: {defaultPassword}";
                return RedirectToAction(nameof(Details), new { id = merchant.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating merchant: {ex.Message}");
                return View(merchant);
            }
        }

        public IActionResult Edit(int id)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == id);
            if (merchant == null)
            {
                return NotFound();
            }
            return View(merchant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Merchants merchant)
        {
            ModelState.Remove("Orders");
            ModelState.Remove("Payouts");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _unitOfWork.Merchant.Update(merchant);
                _unitOfWork.Save();
                TempData["success"] = "Merchant updated successfully";
                return RedirectToAction(nameof(Details), new { id = merchant.Id });
            }
            return View(merchant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == id, includeProperties: "Orders");
            if (merchant == null)
            {
                return NotFound();
            }

            if (merchant.Orders.Any())
            {
                TempData["error"] = "Cannot delete merchant with existing orders";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Merchant.Remove(merchant);
            _unitOfWork.Save();
            TempData["success"] = "Merchant deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
