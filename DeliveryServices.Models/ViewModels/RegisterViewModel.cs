using System.ComponentModel.DataAnnotations;

namespace DeliveryServices.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public bool IsMerchant { get; set; }

        [StringLength(100)]
        [Display(Name = "Business Name")]
        public string? BusinessName { get; set; }

        [StringLength(250)]
        [Display(Name = "Business Address")]
        public string? BusinessAddress { get; set; }

        [Phone]
        [Display(Name = "Business Phone")]
        public string? BusinessPhone { get; set; }
    }
}
