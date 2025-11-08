using DeliveryServices.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.ViewComponents
{
    public class CurrentUserViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
                return View(user);
            }

            return View(null);
        }
    }
}
