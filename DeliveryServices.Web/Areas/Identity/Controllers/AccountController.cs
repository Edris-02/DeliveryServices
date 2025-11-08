using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using DeliveryServices.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.Areas.Identity.Controllers
{
    [Area("Identity")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(
   UserManager<ApplicationUser> userManager,
     SignInManager<ApplicationUser> signInManager,
     RoleManager<IdentityRole> roleManager,
      IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user!);

                // Redirect based on role
                if (roles.Contains(UserRoles.Admin))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (roles.Contains(UserRoles.Merchant))
                {
                    return RedirectToAction("Index", "Home", new { area = "Merchant" });
                }
                else if (roles.Contains(UserRoles.Driver))
                {
                    return RedirectToAction("Index", "Home", new { area = "Driver" });
                }

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                TempData["error"] = "Account is locked due to multiple failed login attempts. Please try again later.";
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Force merchant registration
            model.IsMerchant = true;

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ensure roles exist
                await EnsureRolesExist();

                // Create merchant profile
                var merchant = new Merchants
                {
                    Name = model.BusinessName ?? model.FullName,
                    PhoneNumber = model.BusinessPhone ?? model.PhoneNumber ?? "",
                    Phone = model.BusinessPhone ?? model.PhoneNumber ?? "",
                    Address = model.BusinessAddress ?? "",
                    UserId = user.Id
                };

                _unitOfWork.Merchant.Add(merchant);
                _unitOfWork.Save();

                // Update user with MerchantId
                user.MerchantId = merchant.Id;
                await _userManager.UpdateAsync(user);

                // Assign Merchant role
                await _userManager.AddToRoleAsync(user, UserRoles.Merchant);

                TempData["success"] = "Merchant account created successfully! Please login.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Access Denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to ensure roles exist
        private async Task EnsureRolesExist()
        {
            string[] roles = { UserRoles.Admin, UserRoles.Merchant, UserRoles.Driver };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Default redirect based on user role
          if (User.IsInRole(UserRoles.Admin))
        {
          return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
     }
         else if (User.IsInRole(UserRoles.Driver))
  {
      return RedirectToAction("Index", "Home", new { area = "Driver" });
            }
   else
            {
                // Default to merchant home for authenticated users
   return RedirectToAction("Index", "Home", new { area = "Merchant" });
}
        }
    }
}
