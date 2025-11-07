using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
 [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
    {
            _userManager = userManager;
  }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
       var user = await _userManager.GetUserAsync(User);
     if (user == null)
   {
       return NotFound();
    }

   return View(user);
  }

   // POST: Update Email Preferences
   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> UpdateEmailPreferences(bool emailNotifications, bool orderNotifications, bool weeklyReports)
   {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
     {
  return NotFound();
   }

   // For now, just show success message
  // In a real app, you'd save these preferences to database
    TempData["success"] = "Email preferences updated successfully";
      return RedirectToAction(nameof(Index));
      }

        // POST: Update Security Settings
        [HttpPost]
  [ValidateAntiForgeryToken]
   public async Task<IActionResult> UpdateSecuritySettings(bool twoFactorEnabled)
        {
       var user = await _userManager.GetUserAsync(User);
          if (user == null)
     {
      return NotFound();
     }

 var result = await _userManager.SetTwoFactorEnabledAsync(user, twoFactorEnabled);
      if (result.Succeeded)
 {
      TempData["success"] = twoFactorEnabled 
     ? "Two-factor authentication enabled" 
  : "Two-factor authentication disabled";
   }
  else
  {
   TempData["error"] = "Failed to update security settings";
   }

 return RedirectToAction(nameof(Index));
        }
    }
}
