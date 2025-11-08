using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class DriversController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DriversController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var drivers = _unitOfWork.Driver.GetAll(includeProperties: "Orders,SalaryPayments").ToList();
            return View(drivers);
        }

        public IActionResult Details(int id)
        {
            var driver = _unitOfWork.Driver.Get(
           d => d.Id == id,
                   includeProperties: "Orders.Items,SalaryPayments,User");

            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            return View(driver);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Driver driver)
        {
            ModelState.Remove("Orders");
            ModelState.Remove("SalaryPayments");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View(driver);
            }

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(driver.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists");
                    return View(driver);
                }

                var user = new ApplicationUser
                {
                    UserName = driver.Email,
                    Email = driver.Email,
                    FullName = driver.FullName,
                    PhoneNumber = driver.PhoneNumber,
                    EmailConfirmed = true
                };

                var defaultPassword = $"{driver.FullName.Split(' ')[0]}@123";
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    ModelState.AddModelError("", $"Failed to create user account: {errors}");
                    return View(driver);
                }

                await _userManager.AddToRoleAsync(user, UserRoles.Driver);

                driver.UserId = user.Id;
                driver.JoinedDate = DateTime.UtcNow;

                _unitOfWork.Driver.Add(driver);
                _unitOfWork.Save();

                user.DriverId = driver.Id;
                await _userManager.UpdateAsync(user);

                TempData["success"] = $"Driver created successfully! Login credentials - Email: {driver.Email}, Password: {defaultPassword}";
                return RedirectToAction(nameof(Details), new { id = driver.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating driver: {ex.Message}");
                return View(driver);
            }
        }

        public IActionResult Edit(int id)
        {
            var driver = _unitOfWork.Driver.Get(d => d.Id == id);
            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            return View(driver);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Driver driver)
        {
            ModelState.Remove("Orders");
            ModelState.Remove("SalaryPayments");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View(driver);
            }

            _unitOfWork.Driver.Update(driver);
            _unitOfWork.Save();

            TempData["success"] = "Driver updated successfully";
            return RedirectToAction(nameof(Details), new { id = driver.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var driver = _unitOfWork.Driver.Get(d => d.Id == id, tracked: true);
            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            driver.IsActive = !driver.IsActive;
            _unitOfWork.Save();

            TempData["success"] = $"Driver {(driver.IsActive ? "activated" : "deactivated")} successfully";
            return RedirectToAction(nameof(Details), new { id = driver.Id });
        }

        public IActionResult PaySalary(int id)
        {
            var driver = _unitOfWork.Driver.Get(
                 d => d.Id == id,
                         includeProperties: "Orders,SalaryPayments");

            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            if (driver.CurrentBalance <= 0)
            {
                TempData["error"] = "Driver has no pending salary";
                return RedirectToAction(nameof(Details), new { id });
            }

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var payment = new DriverSalaryPayment
            {
                DriverId = driver.Id,
                Amount = driver.CurrentBalance,
                BaseSalaryPortion = driver.BaseSalary > 0 ? driver.BaseSalary / 12 : 0,
                CommissionPortion = driver.CurrentMonthDeliveries * driver.CommissionPerDelivery,
                DeliveriesCount = driver.CurrentMonthDeliveries,
                PaymentDate = now,
                PeriodStart = startOfMonth,
                PeriodEnd = now
            };

            ViewBag.Driver = driver;
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaySalary(DriverSalaryPayment payment)
        {
            ModelState.Remove("Driver");

            var driver = _unitOfWork.Driver.Get(d => d.Id == payment.DriverId, tracked: true);
            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            if (payment.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than zero");
            }

            if (payment.Amount > driver.CurrentBalance)
            {
                ModelState.AddModelError("Amount", $"Amount cannot exceed current balance of {driver.CurrentBalance:N2}");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Driver = driver;
                return View(payment);
            }

            var newPayment = new DriverSalaryPayment
            {
                DriverId = payment.DriverId,
                Amount = payment.Amount,
                BaseSalaryPortion = payment.BaseSalaryPortion,
                CommissionPortion = payment.CommissionPortion,
                DeliveriesCount = payment.DeliveriesCount,
                PaymentDate = payment.PaymentDate,
                PeriodStart = payment.PeriodStart,
                PeriodEnd = payment.PeriodEnd,
                PaymentMethod = payment.PaymentMethod,
                ProcessedBy = User.Identity?.Name ?? "Admin",
                Notes = payment.Notes,
                TransactionReference = payment.TransactionReference
            };

            _unitOfWork.DriverSalaryPayment.Add(newPayment);

            driver.CurrentBalance -= payment.Amount;
            driver.TotalEarnings += payment.Amount;
            driver.CurrentMonthDeliveries = 0;

            _unitOfWork.Save();

            TempData["success"] = $"Salary payment of {payment.Amount:N2} processed successfully";
            return RedirectToAction(nameof(Details), new { id = driver.Id });
        }
    }
}
